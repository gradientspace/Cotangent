using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using g3;
using f3;

namespace cotangent
{
    public static class CCMaterials
    {


        public static void InitializeMaterials()
        {
            PreviewTransparentMaterial = SOMaterial.CreateTransparent("preview_trans", new Colorf(Colorf.SelectionGold, 0.2f));

            PathMaterial_Extrude = MaterialUtil.CreateFlatMaterial(Colorf.VideoBlack);
            PathMaterial_Support = MaterialUtil.CreateFlatMaterial(Colorf.VideoMagenta);
            PathMaterial_Travel = MaterialUtil.CreateFlatMaterial(Colorf.VideoGreen, 0.5f);
            PathMaterial_PlaneChange = MaterialUtil.CreateFlatMaterial(Colorf.VideoBlue, 0.5f);
            PathMaterial_Default = MaterialUtil.CreateFlatMaterial(Colorf.VideoRed, 0.5f);
        }


        public static void SetupSceneMaterials(FContext context)
        {
            context.Scene.PerTypeSelectionMaterialMap.Add(
                CotangentTypes.PrintMesh,
                (so) => {
                    var type = (so as PrintMeshSO).Settings.ObjectType;
                    if (type == PrintMeshSettings.ObjectTypes.CropRegion || type == PrintMeshSettings.ObjectTypes.Cavity)
                        return SelectedMaterialTransparent;
                    return null;
                }
            );
        }




        static SOMeshMaterial printMeshMaterial = null;
        public static SOMeshMaterial PrintMeshMaterial {
            get {
                if ( printMeshMaterial == null ) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_print_mesh"));
                    printMeshMaterial = new UnitySOMeshMaterial(m);
                }
                return printMeshMaterial;
            }
        }


        static SOMeshMaterial printMeshTransparentMaterial = null;
        public static SOMeshMaterial PrintMeshTransparentMaterial {
            get {
                if (printMeshTransparentMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_print_mesh_trans"));
                    printMeshTransparentMaterial = new UnitySOMeshMaterial(m);
                }
                return printMeshTransparentMaterial;
            }
        }



        static SOMeshMaterial openPrintMeshMaterial = null;
        public static SOMeshMaterial OpenPrintMeshMaterial {
            get {
                if (openPrintMeshMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_open_print_mesh"));
                    openPrintMeshMaterial = new UnitySOMeshMaterial(m);
                }
                return openPrintMeshMaterial;
            }
        }


        static SOMeshMaterial supportMeshMaterial = null;
        public static SOMeshMaterial SupportMeshMaterial {
            get {
                if (supportMeshMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_support_mesh"));
                    supportMeshMaterial = new UnitySOMeshMaterial(m) { Name = "SupportMeshMat" };
                }
                return supportMeshMaterial;
            }
        }


        static SOMeshMaterial cavityMeshMaterial = null;
        public static SOMeshMaterial CavityMeshMaterial {
            get {
                if (cavityMeshMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_cavity_mesh"));
                    cavityMeshMaterial = new UnitySOMeshMaterial(m) { Name = "CavityMeshMat" };
                }
                return cavityMeshMaterial;
            }
        }


        static SOMeshMaterial cropMeshMaterial = null;
        public static SOMeshMaterial CropRegionMeshMaterial {
            get {
                if (cropMeshMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_crop_mesh"));
                    cropMeshMaterial = new UnitySOMeshMaterial(m) { Name = "CropRegionMat" };
                    //cropMeshMaterial.Hints = SOMaterial.HintFlags.UseTransparentPass;
                }
                return cropMeshMaterial;
            }
        }


        static SOMeshMaterial ignoreMeshMaterial = null;
        public static SOMeshMaterial IgnoreMeshMaterial {
            get {
                if (ignoreMeshMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_ignore_mesh"));
                    ignoreMeshMaterial = new UnitySOMeshMaterial(m) { Name = "IgnoreMeshMat" };
                }
                return ignoreMeshMaterial;
            }
        }



        static SOMeshMaterial previewMeshMaterial = null;
        public static SOMeshMaterial PreviewMeshMaterial {
            get {
                if (previewMeshMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_preview_mesh"));
                    previewMeshMaterial = new UnitySOMeshMaterial(m);
                }
                return previewMeshMaterial;
            }
        }


        static SOMeshMaterial previewMeshTransparentMaterial = null;
        public static SOMeshMaterial PreviewMeshTransparentMaterial {
            get {
                if (previewMeshTransparentMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_preview_mesh_trans"));
                    previewMeshTransparentMaterial = new UnitySOMeshMaterial(m);
                }
                return previewMeshTransparentMaterial;
            }
        }




        static SOMeshMaterial deleteMeshMaterial = null;
        public static SOMeshMaterial DeleteMeshMaterial {
            get {
                if (deleteMeshMaterial == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_delete_mesh"));
                    deleteMeshMaterial = new UnitySOMeshMaterial(m);
                }
                return deleteMeshMaterial;
            }
        }


        static SOMeshMaterial deleteMeshMaterialTransparent = null;
        public static SOMeshMaterial DeleteMeshTransparentMaterial {
            get {
                if (deleteMeshMaterialTransparent == null) {
                    fMeshMaterial m = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_delete_mesh_trans"));
                    deleteMeshMaterialTransparent = new UnitySOMeshMaterial(m);
                }
                return deleteMeshMaterialTransparent;
            }
        }



        static fMeshMaterial selectedMaterial = null;
        public static fMeshMaterial SelectedMaterial {
            get {
                if (selectedMaterial == null) {
                    selectedMaterial = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/selected_print_mesh"));
                }
                return selectedMaterial;
            }
        }


        static fMeshMaterial selectedMaterialTrans = null;
        public static fMeshMaterial SelectedMaterialTransparent {
            get {
                if (selectedMaterialTrans == null) {
                    selectedMaterialTrans = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/selected_print_mesh_trans"));
                }
                return selectedMaterialTrans;
            }
        }



        static fMeshMaterial nestedComponentMaterial = null;
        public static fMeshMaterial NestedComponentMaterial {
            get {
                if (nestedComponentMaterial == null) {
                    nestedComponentMaterial = new fMeshMaterial(MaterialUtil.SafeLoadMaterial("Materials/default_nested_component_mesh"));
                }
                return nestedComponentMaterial;
            }
        }




        public static SOMaterial PreviewTransparentMaterial;





        public static fMaterial PathMaterial_Extrude;
        public static fMaterial PathMaterial_Support;
        public static fMaterial PathMaterial_Travel;
        public static fMaterial PathMaterial_Default;
        public static fMaterial PathMaterial_PlaneChange;






    }
}
