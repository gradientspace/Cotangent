using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;
using f3;
using cotangent;

namespace gs
{

    public static class OrientationChanges
    {


        public static void SwapUpDirection(FScene scene, List<PrintMeshSO> objects)
        {
            AxisAlignedBox3f sceneBounds = AxisAlignedBox3f.Empty;
            Vector3f sharedOrigin = Vector3f.Zero;
            foreach (var meshSO in objects) {
                sharedOrigin += meshSO.GetLocalFrame(CoordSpace.SceneCoords).Origin;
                sceneBounds.Contain(meshSO.GetBoundingBox(CoordSpace.SceneCoords).ToAABB());
            }
            sharedOrigin /= objects.Count;

            foreach (var so in objects) {
                Frame3f curF = so.GetLocalFrame(CoordSpace.SceneCoords);
                UpDirection from = so.UpDirection;
                UpDirection to = (from == UpDirection.YUp) ? UpDirection.ZUp : UpDirection.YUp;

                Quaternionf rotate = Quaternionf.AxisAngleD(Vector3f.AxisX, (to == UpDirection.YUp) ? -90 : 90);
                Frame3f newF = curF;
                newF.RotateAround(sharedOrigin, rotate);
                TransformSOChange upChange = new TransformSOChange(so, newF, CoordSpace.SceneCoords) {
                    OnApplyF = (x) => { so.UpDirection = to; },
                    OnRevertF = (x) => { so.UpDirection = from; }
                };
                scene.History.PushChange(upChange, false);
            }

            AxisAlignedBox3f newSceneBounds = AxisAlignedBox3f.Empty;
            foreach (var meshSO in objects) {
                newSceneBounds.Contain(meshSO.GetBoundingBox(CoordSpace.SceneCoords).ToAABB());
            }

            Vector3f startBase = sceneBounds.Center; startBase.y = 0;
            Vector3f newBase = newSceneBounds.Center; newBase.y = newSceneBounds.Min.y;

            Vector3f df = startBase - newBase;
            foreach (var so in objects) {
                Frame3f curF = so.GetLocalFrame(CoordSpace.SceneCoords);
                Frame3f newF = curF.Translated(df);
                TransformSOChange centerChange = new TransformSOChange(so, newF, CoordSpace.SceneCoords);
                scene.History.PushChange(centerChange, false);
            }


            // reposition pivots at base of
            List<Frame3f> setF = new List<Frame3f>();
            foreach (var so in objects) {
                Frame3f objF = so.GetLocalFrame(CoordSpace.ObjectCoords);
                Vector3f center = so.GetBoundingBox(CoordSpace.SceneCoords).Center;
                center.y = 0;
                Frame3f sceneF = new Frame3f(center);
                setF.Add(sceneF);
            }
            for ( int k = 0; k < objects.Count; ++k ) {
                RepositionPivotChangeOp change = new RepositionPivotChangeOp(setF[k], objects[k], CoordSpace.SceneCoords);
                scene.History.PushChange(change, false);
            }

        }




        public static void Mirror(FScene scene, DMeshSO so, bool bInteractive)
        {
            MirrorChangeOp change = new MirrorChangeOp(new List<DMeshSO>() { so });
            scene.History.PushChange(change, false);
            if (bInteractive)
                scene.History.PushInteractionCheckpoint();
        }



        public static void RecenterPivot(FScene scene, DMeshSO so, PivotLocation location, bool bInteractive)
        {
            if (so.Parent != scene) {
                DebugUtil.Log("OrientationChanges.RecenterPivot: tried to recenter pivot on non-scene-child");
                return;
            }

            Frame3f sceneL = SceneTransforms.SceneToObject(so, Frame3f.Identity);
            AxisAlignedBox3d sceneBoundsL = AxisAlignedBox3d.Empty;
            so.SafeMeshRead((mesh) => {
                sceneBoundsL = MeshMeasurements.BoundsInFrame(mesh, sceneL);
                return null;
            });

            Vector3d originL = sceneBoundsL.Center;
            if ( location == PivotLocation.BaseCenter )
                originL -= sceneBoundsL.Extents.y * Vector3d.AxisY;
            Frame3f newFrameL = new Frame3f(originL);

            RepositionPivotChangeOp change = new RepositionPivotChangeOp(newFrameL, so);
            scene.History.PushChange(change, false);
            if (bInteractive)
                scene.History.PushInteractionCheckpoint();
        }



        public static void MoveToPrintBed(FScene scene, DMeshSO so, bool bInteractive)
        {
            TransformSequence seq = SceneTransforms.ObjectToSceneXForm(so);
            AxisAlignedBox3d bounds = BoundsUtil.Bounds(so.Mesh.Vertices(), seq);

            Frame3f curFrameS = so.GetLocalFrame(CoordSpace.SceneCoords);
            float dy = (float)(bounds.Center.y - bounds.Extents.y);

            if (Math.Abs(dy) > MathUtil.ZeroTolerancef) {
                Frame3f newFrameS = curFrameS;
                newFrameS.Origin = curFrameS.Origin - dy*Vector3f.AxisY;
                TransformSOChange change = new TransformSOChange(so, curFrameS, newFrameS, CoordSpace.SceneCoords);
                change.Tags.Add("MoveToPrintBed");
                scene.History.PushChange(change, false);
                if (bInteractive)
                    scene.History.PushInteractionCheckpoint();
            }
        }



        public static void Center(FScene scene, IEnumerable<DMeshSO> objects, bool bInteractive)
        {
            AxisAlignedBox3d all_bounds = AxisAlignedBox3d.Empty;
            foreach ( var so in objects ) {
                TransformSequence seq = SceneTransforms.ObjectToSceneXForm(so);
                AxisAlignedBox3d bounds = BoundsUtil.Bounds(so.Mesh.Vertices(), seq);
                all_bounds.Contain(bounds);
            }
            Vector3f c = (Vector3f)all_bounds.Center;
            c.y = 0;

            foreach ( var so in objects ) {
                Frame3f curFrameS = so.GetLocalFrame(CoordSpace.SceneCoords);
                Frame3f newFrameS = curFrameS;
                newFrameS.Origin = curFrameS.Origin - c;
                TransformSOChange change = new TransformSOChange(so, curFrameS, newFrameS, CoordSpace.SceneCoords);
                scene.History.PushChange(change, false);
            }
            if (bInteractive)
                scene.History.PushInteractionCheckpoint();
        }



        public static void SetBasePoint(FScene scene, DMeshSO so, Frame3f baseFrameS, bool bInteractive)
        {
            Frame3f curFrameS = so.GetLocalFrame(CoordSpace.SceneCoords);
            Frame3f relFrameS = baseFrameS.ToFrame(curFrameS);

            baseFrameS.AlignAxis(2, -Vector3f.AxisY);
            baseFrameS.Translate(-baseFrameS.Origin);

            Frame3f newFrameS = baseFrameS.FromFrame(relFrameS);
            TransformSOChange change = new TransformSOChange(so, curFrameS, newFrameS, CoordSpace.SceneCoords);
            change.Tags.Add("SetBasePoint");
            scene.History.PushChange(change, false);

            Frame3f pivotS = Frame3f.Identity;
            // WHAT why is it the scene pivot ?!
            //Frame3f pivotL = SceneTransforms.SceneToObject(so, pivotS);
            RepositionPivotChangeOp pivotChange = new RepositionPivotChangeOp(pivotS, so);
            scene.History.PushChange(pivotChange, false);

            if (bInteractive)
                scene.History.PushInteractionCheckpoint();
        }


        public static void RecenterAboveOrigin(FScene scene, SceneObject so, bool bInteractive)
        {
            Frame3f curFrameO = so.GetLocalFrame(CoordSpace.ObjectCoords);
            AxisAlignedBox3f bounds = so.GetLocalBoundingBox();
            Box3f box = new Box3f(bounds);
            box = curFrameO.FromFrame(ref box);
            AxisAlignedBox3f boundsS = box.ToAABB();
           
            Vector3f c = boundsS.Center - 0.5f * boundsS.Height * Vector3f.AxisY;
            Vector3f dt = -c;
            if (dt.MaxAbs > MathUtil.ZeroTolerancef) {
                Frame3f newFrameO = curFrameO.Translated(dt);
                TransformSOChange change = new TransformSOChange(so, curFrameO, newFrameO, CoordSpace.ObjectCoords);
                scene.History.PushChange(change, false);
                if (bInteractive)
                    scene.History.PushInteractionCheckpoint();
            }
        }

    }






    
    

    public class MirrorChangeOp : BaseChangeOp
    {
        public List<DMeshSO> Targets;

        public MirrorChangeOp(IEnumerable<DMeshSO> targets)
        {
            Targets = new List<DMeshSO>(targets);
        }

        public override string Identifier() { return "MirrorChangeOp"; }
        public override OpStatus Apply()
        {
            foreach (var so in Targets)
                apply(so);
            return OpStatus.Success;
        }
        public override OpStatus Revert()
        {
            foreach (var so in Targets)
                apply(so);
            return OpStatus.Success;
        }

        public void apply(DMeshSO meshSO)
        {
            meshSO.EditAndUpdateMesh((mesh) => {
                MeshTransforms.FlipLeftRightCoordSystems(mesh);
            }, GeometryEditTypes.ArbitraryEdit);    // reverses orientation, too!
        }

        public override OpStatus Cull()
        {
            Targets = null;
            return OpStatus.Success;
        }
    }





}
