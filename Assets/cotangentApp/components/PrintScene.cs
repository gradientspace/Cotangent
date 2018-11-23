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
    public class PrintScene
    {
        public List<PrintMeshSO> PrintMeshes = new List<PrintMeshSO>();

        public ToolpathSO CurrentToolpaths = null;
        public LayersDetector CurrentLayersInfo = null;

        public SlicePlaneHeightSO SliceHeightGizmo = null;
        protected bool slice_gizmo_valid = false;


        public PrintScene()
        {
            CC.ObjSettings.OnSettingModified += ObjSettings_OnSettingModified;
        }



        public void Update()
        {
            if ( CurrentToolpaths != null ) {
                bool show_toolpaths = ShowToolpaths && (CCActions.CurrentViewMode == AppViewMode.PrintView);
                if (show_toolpaths != SceneUtil.IsVisible(CurrentToolpaths))
                    SceneUtil.SetVisible(CurrentToolpaths, show_toolpaths);
            }

            update_slice_height_gizmo();
        }




        public void AddPrintMesh(PrintMeshSO mesh)
        {
            PrintMeshes.Add(mesh);
            mesh.OnMeshModified += Mesh_OnMeshModified;
            mesh.OnTransformModified += Mesh_OnTransformModified;

            CC.MeshAnalysis.AddMesh(mesh);

            update_mesh_material(mesh);

            CC.Slicer.InvalidateSlicing();
        }


        public void RemovePrintMesh(PrintMeshSO mesh)
        {
            mesh.OnMeshModified -= Mesh_OnMeshModified;
            mesh.OnTransformModified -= Mesh_OnTransformModified;
            PrintMeshes.Remove(mesh);

            CC.MeshAnalysis.RemoveMesh(mesh);

            CC.Slicer.InvalidateSlicing();
        }



        public void ClearScene()
        {
            SliceHeightGizmo = null;
            slice_gizmo_valid = false;
        }



        int current_layer = 0;
        public int CurrentLayer {
            get { return current_layer; }
            set {
                int nLayer = value;
                if (CurrentLayersInfo != null)
                    nLayer = MathUtil.Clamp(nLayer, 0, CurrentLayersInfo.Layers - 1);
                if (current_layer == nLayer)
                    return;
                current_layer = nLayer;

                if (CurrentToolpaths != null) {
                    CurrentToolpaths.ZInterval = CurrentLayersInfo.GetLayerZInterval(current_layer);

                    float h = (float)CurrentToolpaths.ZInterval.Center;
                    CCState.ClipPlaneFrameS = new Frame3f(h * Vector3f.AxisY, Vector3f.AxisY);

                    set_slice_height_from_layer();
                }
            }
        }
        public int NumLayers {
            get { return (CurrentLayersInfo != null && CurrentToolpaths != null) ? CurrentLayersInfo.Layers : 0; }
        }


        bool show_toolpath_polylines = true;
        public bool ShowToolpaths {
            get { return show_toolpath_polylines; }
            set { show_toolpath_polylines = value;   }
        }



        public void DiscardToolpaths()
        {
            if (CurrentToolpaths != null) {
                CC.GCodeAnalysis.RemoveToolpaths(CurrentToolpaths);
                CC.ActiveScene.RemoveSceneObject(CurrentToolpaths, true);
                CurrentToolpaths = null;
            }

            slice_gizmo_valid = false;
        }


        public void SetToolpaths(ToolpathGenerator generator)
        {
            DiscardToolpaths();

            ToolpathSO so = new ToolpathSO();
            so.Create(generator.Toolpaths, generator.Settings.CloneAs<SingleMaterialFFFSettings>(), CCMaterials.PreviewMeshMaterial);
            so.SetSlices(generator.Slices);

            CurrentLayersInfo = generator.LayerInfo;

            // make sure current layer value is valid
            current_layer = MathUtil.Clamp(current_layer, 0, CurrentLayersInfo.Count - 1);

            // update toolpaths
            CurrentToolpaths = so;
            CC.ActiveScene.AddSceneObject(CurrentToolpaths, false);
            CurrentToolpaths.ZInterval = CurrentLayersInfo.GetLayerZInterval(current_layer);

            CC.GCodeAnalysis.AddToolpaths(CurrentToolpaths);
        }





        public AxisAlignedBox3f GetPrintMeshesBounds(bool bPrecise)
        {
            AxisAlignedBox3f b = AxisAlignedBox3f.Empty;
            foreach (PrintMeshSO so in PrintMeshes) {
                if ( SceneUtil.IsVisible(so) == false)
                    continue;

                if (bPrecise) {
                    var xform = SceneTransforms.ObjectToSceneXForm(so);
                    foreach (Vector3d v in so.Mesh.Vertices())
                        b.Contain((Vector3f)xform.TransformP(v));

                } else {
                    Box3f sobox = so.GetBoundingBox(CoordSpace.SceneCoords);
                    if (sobox.Volume > 0) {
                        foreach (Vector3d v in sobox.VerticesItr())
                            b.Contain(v);
                    }
                }
            }
            return b;
        }





        protected void update_mesh_material(PrintMeshSO so)
        {
            bool enable_shadow = true;

            if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Support) {
                so.AssignSOMaterial(CCMaterials.SupportMeshMaterial);
            } else if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Cavity) {
                so.AssignSOMaterial(CCMaterials.CavityMeshMaterial);
            } else if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.CropRegion) {
                so.AssignSOMaterial(CCMaterials.CropRegionMeshMaterial);
                enable_shadow = false;
            } else if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Ignored) {
                so.AssignSOMaterial(CCMaterials.IgnoreMeshMaterial);
                enable_shadow = false;
            } else if (so.Mesh.CachedIsClosed == false) {
                so.AssignSOMaterial(CCMaterials.OpenPrintMeshMaterial);
            } else {
                so.AssignSOMaterial(CCMaterials.PrintMeshMaterial);
            }

            if ( enable_shadow != so.ShadowsEnabled ) {
                so.SetShadowsEnabled(enable_shadow);
            }
        }


        private void ObjSettings_OnSettingModified(ObjectSettings settings) {
            PrintMeshSO so = settings.SourceSO as PrintMeshSO;
            update_mesh_material(so);

            // set dirty flag on scene
            CCActions.SetCurrentSceneModified();
        }

        private void Mesh_OnTransformModified(SceneObject so)
        {
            CC.InvalidateSlicing();

            slice_gizmo_valid = false;

            // set dirty flag on scene
            CCActions.SetCurrentSceneModified();
        }

        private void Mesh_OnMeshModified(DMeshSO so)
        {
            CC.InvalidateSlicing();

            if ( so is PrintMeshSO ) 
                update_mesh_material(so as PrintMeshSO);

            slice_gizmo_valid = false;

            // set dirty flag on scene
            CCActions.SetCurrentSceneModified();
        }



        private void update_slice_height_gizmo()
        {
            if (CurrentToolpaths == null) {
                if (SliceHeightGizmo != null && SceneUtil.IsVisible(SliceHeightGizmo) ) 
                    SceneUtil.SetVisible(SliceHeightGizmo, false);
                slice_gizmo_valid = false;
                return;
            }

            if (slice_gizmo_valid)
                return;

            AxisAlignedBox3f bounds = GetPrintMeshesBounds(true);
            if (bounds == AxisAlignedBox3f.Empty)
                return;   // will be rectified next frame?

            int nSlices = CurrentToolpaths.GetSlices().Count;
            PlanarSliceStack slices = CurrentToolpaths.GetSlices();
            // line is floating
            //bounds.Min.y = (float)slices[0].Z;
            //bounds.Max.y = (float)slices[slices.Count - 1].Z;
            // line at ground
            bounds.Min.y = 0;
            bounds.Max.y = (float)(slices[slices.Count - 1].Z - slices[0].Z);

            Vector3f basePos = new Vector3f(bounds.Max.x, bounds.Min.y, bounds.Max.z);
            basePos.x += 10; basePos.z += 10;
            Vector3f topPos = basePos + bounds.Height * Vector3f.AxisY;


            if ( SliceHeightGizmo == null ) {
                SliceHeightGizmo = new SlicePlaneHeightSO() {
                    LineAlwaysVisible = false
                };
                SliceHeightGizmo.Create(CC.ActiveScene.PivotSOMaterial, null);
                SliceHeightGizmo.DisableShadows();

                CC.ActiveScene.AddSceneObject(SliceHeightGizmo);
                SliceHeightGizmo.Name = "Slice_Height";

                SliceHeightGizmo.ConstraintFrameS = new Frame3f(basePos);
                SliceHeightGizmo.SetLocalFrame(SliceHeightGizmo.ConstraintFrameS, CoordSpace.SceneCoords);

                SliceHeightGizmo.MinPosS = basePos;
                SliceHeightGizmo.MaxPosS = topPos;
                SliceHeightGizmo.CenterPosS = bounds.Point(0, -1, 0);
                SliceHeightGizmo.BoundsDim = (float)Math.Max(bounds.Width, bounds.Depth) + 10.0f;

                SliceHeightGizmo.OnTransformModified += SliceHeightGizmo_OnTransformModified;

                SliceHeightGizmo.InitializeLiveGizmo(CC.ActiveScene);
            }

            if (SceneUtil.IsVisible(SliceHeightGizmo) == false)
                SceneUtil.SetVisible(SliceHeightGizmo, true);
                

            SliceHeightGizmo.ConstraintFrameS = new Frame3f(basePos);
            ignore_xform_event = true;
            SliceHeightGizmo.SetLocalFrame(SliceHeightGizmo.ConstraintFrameS, CoordSpace.SceneCoords);
            ignore_xform_event = false;
            SliceHeightGizmo.MinPosS = basePos;
            SliceHeightGizmo.MaxPosS = topPos;
            SliceHeightGizmo.CenterPosS = bounds.Point(0, -1, 0); 
            SliceHeightGizmo.BoundsDim = (float)Math.Max(bounds.Width, bounds.Depth) + 10.0f;

            set_slice_height_from_layer();

            slice_gizmo_valid = true;
        }

        bool ignore_xform_event = false;
        private void SliceHeightGizmo_OnTransformModified(SceneObject so)
        {
            if (ignore_xform_event)
                return;
            Frame3f curFrameS = SliceHeightGizmo.GetLocalFrame(CoordSpace.SceneCoords);
            double t = (curFrameS.Origin.y - SliceHeightGizmo.MinPosS.y) /
                            (SliceHeightGizmo.MaxPosS.y - SliceHeightGizmo.MinPosS.y);
            int nLayer = (int)(t * CurrentLayersInfo.Count);
            CurrentLayer = nLayer;
        }

        private void set_slice_height_from_layer()
        {
            double z = (CurrentLayersInfo == null) ? 0 : CurrentLayersInfo.GetLayerZ(CurrentLayer);
            Frame3f newFrameS = SliceHeightGizmo.GetLocalFrame(CoordSpace.SceneCoords);
            Vector3f o = newFrameS.Origin;
            o.y = (float)z;
            newFrameS.Origin = o;

            ignore_xform_event = true;
            SliceHeightGizmo.SetLocalFrame(newFrameS, CoordSpace.SceneCoords);
            ignore_xform_event = false;
        }


    }
}
