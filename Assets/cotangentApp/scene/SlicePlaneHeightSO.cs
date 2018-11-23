using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;
using gs;

namespace cotangent
{
    public class SlicePlaneHeightSO : PivotSO
    {
        override public SOType Type { get { return CotangentTypes.SlicePlane; } }

        public Frame3f ConstraintFrameS;
        public Vector3f MinPosS, MaxPosS;
        public Vector3f CenterPosS;
        public float BoundsDim;

        SOSceneIndicatorSet indicators;

        override public bool SupportsScaling {
            get { return false; }
        }

        public override bool IsSurface {
            get { return false; }
        }

        override public bool IsSelectable { get { return false; } }

        public bool LineAlwaysVisible { get; set; }


        public SlicePlaneHeightSO()
        {
            MaintainConsistentViewSize = false;
            IsOverlaySO = false;
        }


        double mesh_height = 1.0;
        protected override fGameObject create_pivot_shape()
        {
            mesh_height = CC.Settings.LayerHeightMM;
            TrivialBox3Generator boxgen = new TrivialBox3Generator() { NoSharedVertices = true,
                Box = new Box3d(Vector3d.Zero, new Vector3d(2.5, CC.Settings.LayerHeightMM, 2.5))
            };
            DMesh3 mesh = boxgen.Generate().MakeDMesh();
            fMeshGameObject meshGO = GameObjectFactory.CreateMeshGO("pivotMesh",
                new fMesh(mesh), true, true);
            meshGO.SetMaterial(MaterialUtil.CreateStandardMaterial(Colorf.Orange));
            return meshGO;
        }



        override public void SetLocalFrame(Frame3f newFrame, CoordSpace eSpace)
        {
            if (Parent != GetScene())
                throw new Exception("SlicePlaneHeightSO.SetLocalFrame: unsupported");

            Frame3f newSceneFrame = newFrame;
            if (eSpace == CoordSpace.WorldCoords)
                newSceneFrame = SceneTransforms.WorldToScene(GetScene(), newFrame);

            Line3d axis = new Line3d(ConstraintFrameS.Origin, Vector3d.AxisY);
            Segment3d seg = new Segment3d(axis.ClosestPoint(MinPosS), axis.ClosestPoint(MaxPosS));
            Vector3d cp = seg.NearestPoint(newSceneFrame.Origin);

            newSceneFrame.Origin = (Vector3f)cp;
            base.SetLocalFrame(newSceneFrame, CoordSpace.SceneCoords);
        }



        public override void Connect(bool bRestore)
        {
            if (indicators == null) {
                FScene scene = GetScene();
                indicators = new SOSceneIndicatorSet(this, scene);

                LineIndicator line = new LineIndicator() {
                    LineWidth = fDimension.Scene(0.5),
                    VisibleF = () => { return LineAlwaysVisible || SceneUtil.IsVisible(this); },
                    SceneStartF = () => { return MinPosS; },
                    SceneEndF = () => { return MaxPosS; },
                    LayerF = () => { return FPlatform.GeometryLayer; }
                };
                indicators.AddIndicator(line);

                SectionPlaneIndicator plane = new SectionPlaneIndicator() {
                    Width = fDimension.Scene(() => { return BoundsDim; }),
                    ColorF = () => { return Colorf.VideoYellow.WithAlpha(0.5f); },
                    VisibleF = () => { return GetScene().IsSelected(this) || (active_gizmo != null && active_gizmo.IsCapturing) ; },
                    SceneFrameF = () => {
                        Vector3f curPosS = GetLocalFrame(CoordSpace.SceneCoords).Origin;
                        return new Frame3f(CenterPosS + (curPosS - MinPosS));
                    }
                };
                indicators.AddIndicator(plane);
            }
        }

        public override void Disconnect(bool bDestroying)
        {
            indicators.Disconnect(true);
            indicators = null;

            if (active_gizmo != null) {
                this.GetScene().RemoveUIElement(active_gizmo, true);
                active_gizmo = null;
            }
        }



        public override void PreRender()
        {
            base.PreRender();
            indicators.PreRender();

            bool is_visible = SceneUtil.IsVisible(this);
            if (active_gizmo != null && active_gizmo.RootGameObject.IsVisible() != is_visible)
                active_gizmo.RootGameObject.SetVisible(is_visible);

            double scaled_height = CC.Settings.LayerHeightMM / mesh_height;
            PivotShapeGO.SetLocalScale(new Vector3f(1, scaled_height, 1));
        }


        AxisTransformGizmo active_gizmo = null;

        public void InitializeLiveGizmo(FScene scene)
        {
            SlicePlaneHeightGizmoBuilder builder = new SlicePlaneHeightGizmoBuilder() {
                Factory = new SlicePlaneHeightGizmoBuilder.WidgetFactory(),
                GizmoVisualDegrees = 4.0f,
                GizmoLayer = FPlatform.GeometryLayer,
                MaintainConsistentViewSize = false,
                DynamicScaleFactor = 12f
            };
            active_gizmo = builder.Build(scene, new List<SceneObject>() { this }) as AxisTransformGizmo;
            scene.AddUIElement(active_gizmo);
        }

    }




    public class SlicePlaneHeightGizmoBuilder : AxisTransformGizmoBuilder
    {
        public override bool SupportsMultipleObjects { get { return false; } }

        public override ITransformGizmo Build(FScene scene, List<SceneObject> targets) {
            if (targets.Count != 1 || (targets[0] is SlicePlaneHeightSO) == false)
                return null;
            return base.Build(scene, targets);
        }

        protected override AxisTransformGizmo create_gizmo() {
            return new AxisTransformGizmo(this.Factory) {
                ActiveWidgets = AxisGizmoFlags.AxisTranslateY,
                EmitChanges = false
            };
        }


        public static TransformManager.GizmoTypeFilter MakeTypeFilter()
        {
            return new TransformManager.GizmoTypeFilter() {
                FilterF = (so) => {
                    if (so is SlicePlaneHeightSO)
                        return CotangentTypes.SlicePlaneHeightGizmoType;
                    else
                        return null;
                }
            };
        }



        public class WidgetFactory : DefaultAxisGizmoWidgetFactory
        {
            static fMesh MyAxisTranslateY;

            public override bool Supports(AxisGizmoFlags widget) {
                return (widget == AxisGizmoFlags.AxisTranslateY) ? true : false;
            }

            fMaterial MyYMaterial;

            public override fMaterial MakeMaterial(AxisGizmoFlags widget)
            {
                switch (widget) {
                    case AxisGizmoFlags.AxisTranslateY:
                        if (MyYMaterial == null) {
                            MyYMaterial = MaterialUtil.CreateStandardMaterial(Colorf.VideoGreen);
                        }
                        return MyYMaterial;
                    default:
                        return null;
                }
            }

            public override fMaterial MakeHoverMaterial(AxisGizmoFlags widget)
            {
                return MakeMaterial(widget);            
            }

            public override fMesh MakeGeometry(AxisGizmoFlags widget)
            {
                switch (widget) {
                    case AxisGizmoFlags.AxisTranslateY:
                        if (MyAxisTranslateY == null) {
                            Radial3DArrowGenerator arrowGen = new Radial3DArrowGenerator() {
                                HeadLength = 2.0f, TipRadius = 0.1f, StickLength = 1.5f, Clockwise = true
                            };
                            DMesh3 mesh = arrowGen.Generate().MakeDMesh();
                            MeshNormals.QuickCompute(mesh);
                            MeshTransforms.Translate(mesh, 0.5 * Vector3d.AxisY);
                            DMesh3 flip = new DMesh3(mesh);
                            MeshTransforms.Rotate(flip, Vector3d.Zero, Quaterniond.AxisAngleD(Vector3d.AxisX, 180));
                            MeshEditor.Append(mesh, flip);
                            MyAxisTranslateY = new fMesh(mesh);
                        }
                        return MyAxisTranslateY;
                    default:
                        return null;
                }
            }
        }

    }








}
