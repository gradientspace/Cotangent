using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using f3;

namespace cotangent
{
    public static class CCPreferences
    {


        public static void RestorePreferences()
        {
            camera_mode =
                (CameraModes)FPlatform.GetPrefsInt("CameraMode", (int)CameraModes.Perspective);
            startup_workspace =
                (AppViewMode)FPlatform.GetPrefsInt("StartupWorkspace", (int)AppViewMode.PrintView);
            graphics_quality =
                (GraphicsQualityLevels)FPlatform.GetPrefsInt("GraphicsQuality", (int)GraphicsQualityLevels.Max);


            import_xform_mode =
                (ImportTransformModes)FPlatform.GetPrefsInt("ImportTransformMode", (int)ImportTransformModes.AutoCenterFirst);
            import_assistant_mode =
                (ImportAssistantModes)FPlatform.GetPrefsInt("ImportAssistantMode", (int)ImportAssistantModes.All);
            large_mesh_import_threshold =
                FPlatform.GetPrefsInt("LargeMeshImportThreshold", 250000);

            default_slicing_mode =
                (SlicingUpdateModes)FPlatform.GetPrefsInt("DefaultSlicingUpdateMode", (int)SlicingUpdateModes.SliceOnDemand);
            active_slicing_mode = default_slicing_mode;
        }





        public static int file_auto_reload_check_freq = 3;  // seconds
        public static int AutoReloadCheckFrequencyS {
            get { return file_auto_reload_check_freq; }
            set { file_auto_reload_check_freq = value; }
        }


        public enum ImportTransformModes
        {
            AutoCenterAll = 0,
            AutoCenterFirst = 1,
            NoAutoCenter = 2
        }
        static ImportTransformModes import_xform_mode = ImportTransformModes.AutoCenterFirst;
        public static ImportTransformModes ImportTransformMode {
            get { return import_xform_mode; }
            set {
                import_xform_mode = value;
                FPlatform.SetPrefsInt("ImportTransformMode", (int)import_xform_mode);
            }
        }



        public enum ImportAssistantModes
        {
            All = 0,
            MeshSizeOnly = 1,
            PhysicalSizeOnly = 2,
            Disabled = 3
        }
        static ImportAssistantModes import_assistant_mode = ImportAssistantModes.All;
        public static ImportAssistantModes ImportAssistantMode {
            get { return import_assistant_mode; }
            set {
                import_assistant_mode = value;
                FPlatform.SetPrefsInt("ImportAssistantMode", (int)import_assistant_mode);
            }
        }



        static int large_mesh_import_threshold = 250000;
        public static int LargeMeshImportThreshold {
            get { return large_mesh_import_threshold; }
            set {
                large_mesh_import_threshold = Math.Max(10000, value);
                FPlatform.SetPrefsInt("LargeMeshImportThreshold", (int)large_mesh_import_threshold);
            }
        }




        static AppViewMode startup_workspace = AppViewMode.PrintView;
        public static AppViewMode StartupWorkspace {
            get { return startup_workspace; }
            set {
                startup_workspace = value;
                FPlatform.SetPrefsInt("StartupWorkspace", (int)startup_workspace);
            }
        }





        public enum CameraModes
        {
            Perspective = 0,
            Orthographic = 1
        }
        static CameraModes camera_mode = CameraModes.Perspective;
        public static CameraModes CameraMode {
            get { return camera_mode; }
            set {
                camera_mode = value;
                CCActions.UpdateCameraMode(camera_mode);
                FPlatform.SetPrefsInt("CameraMode", (int)camera_mode);
            }
        }




        public enum SlicingUpdateModes
        {
            ImmediateSlicing = 0,
            SliceOnDemand = 1
        }
        static SlicingUpdateModes active_slicing_mode = SlicingUpdateModes.SliceOnDemand;
        //static SlicingUpdateModes slicing_mode = SlicingUpdateModes.ImmediateSlicing;
        public static SlicingUpdateModes ActiveSlicingUpdateMode {
            get { return active_slicing_mode; }
            set { active_slicing_mode = value; }
        }
        static SlicingUpdateModes default_slicing_mode = SlicingUpdateModes.SliceOnDemand;
        public static SlicingUpdateModes DefaultSlicingUpdateMode {
            get { return default_slicing_mode; }
            set {
                default_slicing_mode = value;
                FPlatform.SetPrefsInt("DefaultSlicingUpdateMode", (int)default_slicing_mode);
            }
        }






        public enum GraphicsQualityLevels
        {
            Max = 0,
            High = 1,
            Medium = 2,
            Low = 3,
            Fastest = 4
        }
        static GraphicsQualityLevels graphics_quality = GraphicsQualityLevels.Max;
        public static GraphicsQualityLevels GraphicsQuality {
            get { return graphics_quality; }
            set {
                if (graphics_quality != value) {
                    graphics_quality = value;
                    CCActions.UpdateGraphicsQuality(graphics_quality);
                    FPlatform.SetPrefsInt("GraphicsQuality", (int)graphics_quality);
                }
            }
        }





        public static bool VerboseToolOutput {
            get { return false && FPlatform.InUnityEditor(); }
        }

    }
}
