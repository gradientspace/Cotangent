using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using f3;
using gs;

namespace cotangent
{

	public static class CC
	{
        public static bool SHOW_DEVELOPMENT_FEATURES {
            get { return FPlatform.InUnityEditor(); }
        }


        public static FContext ActiveContext;
        public static FScene ActiveScene {
            get { return ActiveContext.Scene; }
        }
        public static Cockpit ActiveCockpit {
            get { return ActiveContext.ActiveCockpit; }
        }


        // temp things
        public static fGameObject PrinterBed = null;
        public static SceneObject PrintHeadSO;

        public static SettingsDatabase PrinterDB; 

        public static PrintSettings Settings;
        public static ObjectSettings ObjSettings;

        public static PrintScene Objects;
        public static GeometrySlicer Slicer;
        public static ToolpathGenerator Toolpather;

        public static MeshAnalysisManager MeshAnalysis;
        public static GCodeAnalysisManager GCodeAnalysis;

        public static ExternalFileMonitor FileMonitor;

        public static event ToolpathsProgressHandler ToolpathProgressEvent;
        public static event SlicingProgressHandler SlicingProgressEvent;



        public static void Initialize(FContext context)
        {
            ActiveContext = context;
            ActiveContext.Scene.SelectionChangedEvent += Scene_SelectionChangedEvent;
            ActiveContext.Scene.ChangedEvent += Scene_ChangedEvent;

            PrinterDB = new SettingsDatabase();
            Settings = new PrintSettings();

            PrinterDB.OnPrinterSelectionModified += PrinterDB_OnPrinterSelectionModified;
            PrinterDB.OnPresetSelectionModified += PrinterDB_OnPresetSelectionModified;

            PrinterDB.Initialize();
            Settings.UpdateFromSettings(PrinterDB.ActivePreset.Settings);
            update_printer_bed();


            ObjSettings = new ObjectSettings();
            CCActions.UpdateObjectSettings();

            Objects = new PrintScene();

            Toolpather = new ToolpathGenerator();
            Toolpather.ToolpathsProgressEvent += (status) => {
                ToolpathProgressEvent?.Invoke(status);
            };

            Slicer = new GeometrySlicer();
            Slicer.SlicingProgressEvent += (status) => {
                SlicingProgressEvent?.Invoke(status);
            };
            Slicer.SlicingInvalidatedEvent += () => { InvalidateToolPaths(); };

            MeshAnalysis = new MeshAnalysisManager();
            GCodeAnalysis = new GCodeAnalysisManager();
            GCodeAnalysis.EnableUpdates = false;

            FileMonitor = new ExternalFileMonitor();
        }


        public static event SettingsDatabase.PrinterSelectionModifiedEvent OnPrinterSelectionModified;
        public static event SettingsDatabase.PresetSelectionModifiedEvent OnPresetSelectionModified;

        private static void PrinterDB_OnPrinterSelectionModified(Manufacturer newMfg, MachineModel newModel)
        {
            InvalidateToolPaths();
            update_printer_bed();
            OnPrinterSelectionModified?.Invoke(newMfg, newModel);
        }
        private static void PrinterDB_OnPresetSelectionModified(MachinePreset newPreset)
        {
            InvalidateToolPaths();
            update_printer_bed();
            OnPresetSelectionModified?.Invoke(newPreset);
        }




        // called every frame
        public static void Update()
        {
            if (FPlatform.ShutdownBackgroundThreadsOnQuit) 
                return;

            Objects.Update();
            Slicer.Update();
            Toolpather.Update();
            MeshAnalysis.Update();
            GCodeAnalysis.Update();

            CCActions.UpdateCurrentToolStatus();
        }





        static void update_printer_bed()
        {
            if ( PrinterBed != null ) {
                float w = (float)Settings.BedSizeXMM;
                float h = (float)Settings.BedSizeYMM;
                if ( w > 0 && h > 0 )
                    PrinterBed.SetLocalScale(new Vector3f(w, 1, h));
            }
        }
        public static void NotifyBedSizeModified() {
            update_printer_bed();
        }







        private static void Scene_SelectionChangedEvent(object sender, EventArgs e) {
            CCActions.UpdateObjectSettings();
        }
        private static void Scene_ChangedEvent(object sender, SceneObject so, SceneChangeType type) {
            CCActions.UpdateObjectSettings();
            CCActions.SetCurrentSceneModified();
        }






        public static void InvalidateSlicing()
        {
            SlicingProgressEvent?.Invoke(new SlicingProgressStatus(0, 1));
            if ( Slicer != null )
                Slicer.InvalidateSlicing();
        }
        public static void InvalidateToolPaths()
        {
            ToolpathProgressEvent?.Invoke(new ToolpathProgressStatus(0, 1));
            if (Toolpather != null) {
                Toolpather.InvalidateToolpaths();
            }
        }






        // only used by print animation
        public static void SetLayerFromZ(double z)
        {
            throw new NotImplementedException("todo: fix this");
            //if ( LayerInfo != null )
            //    CurrentLayer = LayerInfo.GetLayerIndex(z);
        }



        public static void RunPrintAnimationTest()
        {
            if ( Toolpather.CurrentGCode != null ) {
                PrintAnimation anim = new PrintAnimation();

                SceneUtil.SetVisible(PrintHeadSO, true);
                foreach (var so in Objects.PrintMeshes)
                    SceneUtil.SetVisible(so, false);

                anim.PrintHeadSO = PrintHeadSO;
                anim.SpeedUnitsToMMPerSec *= 5.0f;

                anim.Begin(Toolpather.CurrentGCode);
                CC.ActiveScene.ObjectAnimator.Register(anim);
            }
        }


	}

}
