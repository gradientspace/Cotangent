using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;
namespace cotangent
{
    public static partial class CCActions
    {


        public static bool WireframeEnabled {
            get { return CCState.WireframeEnabled; }
            set { if (value) EnableWireframe(); else DisableWireframe(); }
        }

        static void EnableWireframe()
        {
            if (CCState.WireframeEnabled)
                return;

            CCMaterials.PrintMeshMaterial.EnableWireframe = true;
            CCMaterials.SelectedMaterial.EnableWireframe = true;
            CCMaterials.PreviewMeshMaterial.EnableWireframe = true;

            foreach (SceneObject so in CC.ActiveScene.SceneObjects) {
                if (so is DMeshSO) {
                    fMaterial mat = (so as DMeshSO).CurrentMaterial;
                    if (mat is fMeshMaterial)
                        (mat as fMeshMaterial).EnableWireframe = true;
                }
            }

            CCState.WireframeEnabled = true;
        }

        static void DisableWireframe()
        {
            if (CCState.WireframeEnabled == false)
                return;

            CCMaterials.PrintMeshMaterial.EnableWireframe = false;
            CCMaterials.SelectedMaterial.EnableWireframe = false;
            CCMaterials.PreviewMeshMaterial.EnableWireframe = false;

            foreach (SceneObject so in CC.ActiveScene.SceneObjects) {
                if (so is DMeshSO) {
                    fMaterial mat = (so as DMeshSO).CurrentMaterial;
                    if (mat is fMeshMaterial)
                        (mat as fMeshMaterial).EnableWireframe = false;
                }
            }

            CCState.WireframeEnabled = false;
        }







        public static bool ClipPlaneEnabled {
            get { return CCState.ClipPlaneEnabled; }
            set { if (value) EnableClipPlane(); else DisableClipPlane(); }
        }
        static void EnableClipPlane()
        {
            if (CCState.ClipPlaneEnabled == false) {
                set_all_material_clip((int)SOMeshMaterial.ClipPlaneModes.ClipAndFill);
                CCState.ClipPlaneEnabled = true;
            }
        }
        static void DisableClipPlane()
        {
            if (CCState.ClipPlaneEnabled) {
                set_all_material_clip((int)SOMeshMaterial.ClipPlaneModes.NoClip);
                CCState.ClipPlaneEnabled = false;
            }
        }
        static void set_all_material_clip(int mode)
        {
            CCMaterials.PrintMeshMaterial.ClipPlaneMode = (SOMeshMaterial.ClipPlaneModes)mode;
            CCMaterials.SelectedMaterial.ClipPlaneMode = (fMeshMaterial.ClipPlaneModes)mode;
            CCMaterials.PreviewMeshMaterial.ClipPlaneMode = (SOMeshMaterial.ClipPlaneModes)mode;

            foreach (SceneObject so in CC.ActiveScene.SceneObjects) {
                if (so is DMeshSO) {
                    fMaterial mat = (so as DMeshSO).CurrentMaterial;
                    if (mat is fMeshMaterial)
                        (mat as fMeshMaterial).ClipPlaneMode = (fMeshMaterial.ClipPlaneModes)mode;
                }
            }
        }



    }
}
