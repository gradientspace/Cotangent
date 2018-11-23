using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using g3;
using f3;
using gs;

namespace cotangent
{
    public static partial class CCActions
    {
        // this bit is to let us get rid of the startup object automatically
        public static bool HaveOpenedOrImportedFile = false;
        public static string StartupObjectUUID = "";
        public static bool CurrentSceneIsStartupScene {
            get { return HaveOpenedOrImportedFile == false && CC.Objects.PrintMeshes.Count == 1 && CC.Objects.PrintMeshes[0].UUID == StartupObjectUUID; }
        }


        public static void DoFileDialogOpen()
        {
            Action<string> onCompletedF = (filename) => {
                FPlatform.SetPrefsString("LastImportPath", filename);
                CC.ActiveScene.History.PushInteractionCheckpoint();
                CCActions.UpdateViewClippingBounds();
            };

            FPlatform.GetOpenFileName_Async("Select File", "", new string[] { "*.stl", "*.obj", "*.cota" }, "Supported Files (.stl, .obj, .cota)",
                (sFilename) => { DoFileOpen(sFilename, true, onCompletedF); });
        }
        public static async void DoFileOpen(string sFilename, bool bInteractive, Action<string> onCompletedF = null)
        {
            if (string.IsNullOrEmpty(sFilename))
                return;
            if ( File.Exists(sFilename) == false ) {
                CotangentUI.ShowModalMessageDialog("File Does Not Exist",
                    "File " + sFilename + " does not exist",
                    "Ok", null, null);
                return;
            }

            HaveOpenedOrImportedFile = true;

            if ( sFilename.EndsWith(".cota", StringComparison.OrdinalIgnoreCase) ) {
                // [TODO] make this multi threaded?
                OpenSceneFile(sFilename);
                FPlatform.SetPrefsString("LastImportPath", sFilename);

            }  else {
                ClearScene();
                MeshImporter importer = new MeshImporter();
                CCStatus.BeginOperation("reading");
                await importer.ImportInteractive(sFilename, onCompletedF);
                CCStatus.EndOperation("reading");
            }
        }



        public static void DoFileDialogImport()
        {
            Action<string> onCompletedF = (filename) => {
                FPlatform.SetPrefsString("LastImportPath", filename);
                CC.ActiveScene.History.PushInteractionCheckpoint();
                CCActions.UpdateViewClippingBounds();
            };

            FPlatform.GetOpenFileName_Async("Select File", "", new string[] { "*.stl", "*.obj" }, "Supported Files (.stl, .obj)",
                (sFilename) => { DoFileImport(sFilename, true, onCompletedF); });
        }
        public static async void DoFileImport(string sFilename, bool bInteractive, Action<string> onCompletedF = null)
        {
            if (string.IsNullOrEmpty(sFilename))
                return;
            if ( File.Exists(sFilename) == false ) {
                CotangentUI.ShowModalMessageDialog("File Does Not Exist",
                    "File " + sFilename + " does not exist",
                    "Ok", null, null);
                return;
            }

            if (CurrentSceneIsStartupScene)
                ClearScene();
            HaveOpenedOrImportedFile = true;

            if ( sFilename.EndsWith(".cota", StringComparison.OrdinalIgnoreCase) ) {
                // [RMS] not supported for now
                // [TODO] make this multi threaded?
                //OpenSceneFile(sFilename);
                //FPlatform.SetPrefsString("LastImportPath", sFilename);

            }  else { 
                CC.Slicer.InvalidateSlicing();
                MeshImporter importer = new MeshImporter();
                CCStatus.BeginOperation("reading");
                await importer.ImportInteractive(sFilename, onCompletedF);
                CCStatus.EndOperation("reading");
            }
        }



        public static async void DoDragDropImport(List<string> filenames)
        {
            List<string> meshFiles, sceneFiles;
            get_valid_filenames(filenames, out meshFiles, out sceneFiles);

            if ( sceneFiles.Count > 0 ) {
                HaveOpenedOrImportedFile = true;
                DoDragDropOpen(sceneFiles);
            }

            int count = 0;
            Action<string> onCompletedF = (filename) => {
                count++;
            };

            if (CurrentSceneIsStartupScene)
                ClearScene();
            HaveOpenedOrImportedFile = true;

            CC.Slicer.InvalidateSlicing();

            CCStatus.BeginOperation("reading");

            foreach ( string filename in filenames ) {
                if (filename.Length > 0 && File.Exists(filename)) {
                    MeshImporter importer = new MeshImporter();
                    await importer.ImportInteractive(filename, onCompletedF);
                }
            }

            CCStatus.EndOperation("reading");

            if ( count > 0 ) {
                CC.ActiveScene.History.PushInteractionCheckpoint();
                CCActions.UpdateViewClippingBounds();
            } else {
                // show an error message or something...
            }
        }
        public static void DoDragDropOpen(List<string> filenames)
        {
            if ( filenames.Count > 1 ) {
                CotangentUI.ShowModalMessageDialog("Cannot Open Multiple Scenes",
                    "Sorry, currently Cotangent can only open a single dropped scene.",
                    "Ok", null, null);
            }

            OpenSceneFile(filenames[0]);
        }




        public static void DoArgumentsImport(string[] filenames)
        {
            List<string> meshFiles, sceneFiles;
            get_valid_filenames(filenames, out meshFiles, out sceneFiles);

            // dismiss splash screen if we have these args
            if ( sceneFiles.Count > 0 || meshFiles.Count > 0 ) {
                var go = UnityUtil.FindGameObjectByName("SplashScreenPanel");
                if (go != null)
                    go.GetComponent<SplashScreenDialog>().TransitionVisibility(false);
            }


            bool clear_scene = true;
            if ( sceneFiles.Count > 0 ) {
                OpenSceneFile(sceneFiles[0]);
                clear_scene = false;
            }

            if ( meshFiles.Count > 0 ) {
                if (clear_scene)
                    CCActions.ClearScene();
                run_arguments_imports(meshFiles);
            }

        }
        public static async void run_arguments_imports(List<string> filenames)
        {
            int count = 0;
            Action<string> onCompletedF = (filename) => {
                count++;
            };

            CCStatus.BeginOperation("reading");

            foreach (string filename in filenames) {
                if (filename.Length > 0 && File.Exists(filename)) {
                    MeshImporter importer = new MeshImporter();
                    await importer.ImportInteractive(filename, onCompletedF);
                }
            }

            CCStatus.EndOperation("reading");

            if (count > 0) {
                // discard history
                CC.ActiveScene.History.Clear();
            } else {
                // show an error message or something...
            }
        }



        static void get_valid_filenames(IEnumerable<string> filenames, out List<string> meshFiles, out List<string> sceneFiles)
        {
            meshFiles = new List<string>();
            sceneFiles = new List<string>();
            foreach (var filename in filenames) {
                if (filename.EndsWith(".obj", StringComparison.OrdinalIgnoreCase) ||
                     filename.EndsWith(".stl", StringComparison.OrdinalIgnoreCase) ||
                     filename.EndsWith(".off", StringComparison.OrdinalIgnoreCase) ||
                     filename.EndsWith(".g3mesh", StringComparison.OrdinalIgnoreCase)) {
                    if (File.Exists(filename)) {
                        meshFiles.Add(filename);
                    }
                }
                if (filename.EndsWith(".cota", StringComparison.OrdinalIgnoreCase)) {
                    if (File.Exists(filename))
                        sceneFiles.Add(filename);
                }
            }
        }





        public static void DoGCodeExport()
        {
            if (CC.Toolpather.CurrentGCode == null) {
                HUDUtil.ShowCenteredPopupMessage("Sorry", "Cannot Export GCode until Toolpaths are generated!", CC.ActiveCockpit);
                return;
            }

            int skipWarning = FPlatform.GetPrefsInt("WarnAboutGCodeExport", 0);
            if (skipWarning == 0) {
                ExportGCodeWarningDialog.ShowDialog();
            } else {
                DoGCodeExportInteractive();
            }
        }
        public static void DoGCodeExportInteractive()
        {
            FPlatform.GetSaveFileName_Async("Enter Filename", "", new string[] { "*.gcode" }, "GCode Files",
                (sFilename) => { DoGCodeExport(sFilename); });
        }
        public static void DoGCodeExport(string sFilename)
        {
            if (sFilename != null && sFilename.Length > 0) {
                if (string.IsNullOrEmpty(Path.GetExtension(sFilename)))
                    sFilename = sFilename + ".gcode";

                StandardGCodeWriter writer = new StandardGCodeWriter();
                using (StreamWriter w = new StreamWriter(sFilename)) {
                    writer.WriteFile(CC.Toolpather.CurrentGCode, w);
                }

                CotangentAnalytics.ExportGCode();

                if ( CC.PrinterDB.ActivePreset.Settings is gs.info.ISailfishSettings) {
                    gs.info.ISailfishSettings sailfish = CC.PrinterDB.ActivePreset.Settings as gs.info.ISailfishSettings;
                    Task.Run(() => {
                        string GPX_PATH = Path.Combine(FPlatform.GameExecutablePath(), "utilities/gpx.exe");
                        string args = sailfish.GPXModelFlag + " -p " + sFilename;
                        DebugUtil.Log("Running " + GPX_PATH + " " + args);
                        System.Diagnostics.Process.Start(GPX_PATH, args);
                    });
                }
            }
        }




        public static void DoMeshExport()
        {
            FPlatform.GetSaveFileName_Async("Enter Filename", "", new string[] { "*.stl", "*.obj" }, "Mesh files (stl, obj)",
                (sFilename) => { DoMeshExport(sFilename); });
        }
        public static void DoMeshExport(string sFilename)
        {
            if (sFilename != null && sFilename.Length > 0) {
                if (string.IsNullOrEmpty(Path.GetExtension(sFilename)))
                    sFilename = sFilename + ".obj";

                List<DMeshSO> exportSOs = new List<DMeshSO>();
                foreach ( var so in CC.ActiveScene.Selected ) {
                    if (so is PrintMeshSO && CC.ActiveScene.IsVisible(so) )
                        exportSOs.Add(so as PrintMeshSO);
                }
                if (exportSOs.Count == 0)
                    exportSOs.AddRange( CC.Objects.PrintMeshes.Where(CC.ActiveScene.IsVisible).Cast<DMeshSO>() );

                MeshExporter exporter = new MeshExporter(exportSOs, sFilename);
                exporter.OnCompletedF = (result) => {
                    if (result.code == IOCode.Ok) {
                        DebugUtil.Log("Export OK");
                        ThreadMailbox.PostToMainThread(() => { CotangentAnalytics.ExportMesh(sFilename); });
                    } else {
                        DebugUtil.Log("Export ERROR: " + result.message);
                    }
                };
                FPlatform.CoroutineExec.StartAnonymousCoroutine(exporter.RunMainThreadProcessing());
            }
        }



        public static void DeleteSelectedObjects(bool bInteractive)
        {
            List<SceneObject> delete = new List<SceneObject>(CC.ActiveScene.Selected);
            foreach (var removeSO in delete) {
                if (removeSO is PrintMeshSO == false)
                    throw new NotSupportedException("CCActions.DeleteSelectedObjects: currently can only delete print meshes?");
                RemovePrintMesh(removeSO as PrintMeshSO);
            }
            if (bInteractive)
                CC.ActiveScene.History.PushInteractionCheckpoint();
        }




        public static void DuplicateSelectedObjects(bool bInteractive)
        {
            List<SceneObject> duplicate = new List<SceneObject>(CC.ActiveScene.Selected);
            foreach (var existingSO in duplicate) {
                if (existingSO is PrintMeshSO == false)
                    throw new NotSupportedException("CCActions.DuplicateSelectedObjects: currently can only delete print meshes?");

                PrintMeshSO dupeSO = (existingSO as PrintMeshSO).DuplicateSubtype<PrintMeshSO>();
                dupeSO.Name = UniqueNames.GetNext(existingSO.Name);
                AddNewPrintMesh(dupeSO);

                // If we have multi-select, then we duplicated relative to a transient group that will
                // go away. So, update position using scene coords
                if (existingSO.Parent is FScene == false) {
                    var sceneF = existingSO.GetLocalFrame(CoordSpace.SceneCoords);
                    dupeSO.SetLocalFrame(sceneF, CoordSpace.SceneCoords);
                    Vector3f scaleL = existingSO.GetLocalScale();
                    Vector3f scaleS = SceneTransforms.ObjectToSceneV(existingSO, scaleL);
                    float scale = scaleS.Length / scaleL.Length;
                    dupeSO.SetLocalScale(scale * Vector3f.One);
                }

                if ( dupeSO.CanAutoUpdateFromSource() && dupeSO.AutoUpdateOnSourceFileChange == true )
                    CC.FileMonitor.AddMesh(dupeSO);
            }
            if (bInteractive)
                CC.ActiveScene.History.PushInteractionCheckpoint();
        }




        public static void ClearScene()
        {
            List<PrintMeshSO> printMeshes = new List<PrintMeshSO>(CC.Objects.PrintMeshes);
            foreach (var so in printMeshes)
                RemovePrintMesh(so);
            CC.Objects.ClearScene();
            CC.ActiveContext.NewScene(false, false);
        }



        public static void RemovePrintMesh(PrintMeshSO removeSO)
        {
            DeleteSOChange change = new DeleteSOChange() {
                scene = CC.ActiveScene, so = removeSO,
                OnAddedF = (so) => { CC.Objects.AddPrintMesh(so as PrintMeshSO); },
                OnRemovedF = (so) => { CC.Objects.RemovePrintMesh(so as PrintMeshSO); }
            };
            CC.ActiveScene.History.PushChange(change, false);
        }




        public static void AddNewPrintMesh(PrintMeshSO printMeshSO)
        {
            AddSOChange addChange = new AddSOChange() {
                scene = CC.ActiveScene,
                so = printMeshSO,
                OnAddedF = (so) => {
                    CC.Objects.AddPrintMesh(so as PrintMeshSO);
                },
                OnRemovedF = (so) => {
                    CC.Objects.RemovePrintMesh(so as PrintMeshSO);
                }
            };
            CC.ActiveScene.History.PushChange(addChange, false);
        }




        public static void SetFileMonitoringEnabled(PrintMeshSO so, bool bEnabled)
        {
            if (so.CanAutoUpdateFromSource() == false || so.AutoUpdateOnSourceFileChange == bEnabled)
                return;

            if ( bEnabled ) {
                so.AutoUpdateOnSourceFileChange = true;
                CC.FileMonitor.AddMesh(so);
            } else {
                so.AutoUpdateOnSourceFileChange = false;
                CC.FileMonitor.RemoveMesh(so);
            }
        }




        public static void DoInteractiveClearScene()
        {
            ClearScene();
            ClearCurrentSceneModified();
            SetCurrentSaveFilePath("");
        }





        public static void SaveCurrentSceneAs()
        {
            FPlatform.GetSaveFileName_Async("Enter Filename", "", new string[] { "*.cota" }, "Cotangent files (.cota)",
                (sFilename) => { DoSaveCurrentScene(sFilename); });
        }
        public static void SaveCurrentSceneOrSaveAs()
        {
            if (string.IsNullOrEmpty(CurrentSceneFilename))
                SaveCurrentSceneAs();
            else
                DoSaveCurrentScene(CurrentSceneFilename);
        }
        public static void DoSaveCurrentScene(string sFilename)
        {
            if (sFilename != null && sFilename.Length > 0) {
                if (sFilename.EndsWith(".cota", StringComparison.OrdinalIgnoreCase) == false)
                    sFilename += ".cota";

                CotangentSerializer serializer = new CotangentSerializer();
                serializer.SerializeOptions.MinimalMeshStorage = true;
                serializer.SerializeOptions.FastCompression = false;
                serializer.SerializeOptions.StoreMeshVertexNormals = false;
                serializer.SerializeOptions.StoreMeshVertexUVs = false;
                serializer.SerializeOptions.StoreMeshVertexColors = false;
                try {
                    serializer.StoreCurrent(sFilename);
                    SetCurrentSaveFilePath(sFilename);
                    ClearCurrentSceneModified();
                } catch (Exception e) {
                    DebugUtil.Log("DoSaveCurrentScene: Exception: " + e.Message);
                    CotangentUI.ShowModalMessageDialog("Save Failed",
                        "Error writing to file " + sFilename,
                        "Ok", null, null);
                }
            }
        }




        public static void OpenSceneFile(string sFilename)
        {
            CotangentSerializer serializer = new CotangentSerializer();
            try {
                serializer.RestoreToCurrent(sFilename);
                SetCurrentSaveFilePath(sFilename);
                ClearCurrentSceneModified();
            } catch (Exception e) {
                DebugUtil.Log("OpenSceneFile: exception restoring cota file: " + e.Message);
                CotangentUI.ShowModalMessageDialog("Error Opening Scene",
                    "Sorry, errors ocurred while trying to read this scene file.",
                    "Ok", null, null);
            }
        }


        public static void BeginRestoreExistingScene()
        {
            ClearScene();
        }

        public static void CompleteRestoreExistingScene()
        {
            List<PrintMeshSO> printMeshes = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>();
            foreach (var so in printMeshes)
                CC.Objects.AddPrintMesh(so);

            CC.ActiveScene.ClearHistory();
        }



        public static bool HaveActiveSaveFile {
            get { return ! string.IsNullOrEmpty(CurrentSceneFilename); }
        }


        static bool current_scene_modified = false;
        public static bool CurrentSceneModified {
            get { return current_scene_modified; }
        }
        public static void SetCurrentSceneModified()
        {
            current_scene_modified = true;
        }
        public static void ClearCurrentSceneModified()
        {
            current_scene_modified = false;
        }


        static string current_scene_filename = "";
        public static string CurrentSceneFilename {
            get { return current_scene_filename; }
        }

        public static void SetCurrentSaveFilePath(string sPath)
        {
            current_scene_filename = sPath;
            if (HaveActiveSaveFile) {
                string filename = Path.GetFileName(sPath);
                FPlatform.SetWindowTitle(filename);
            } else {
                FPlatform.SetWindowTitle( string.Format("cotangent {0}", CotangentVersion.CurrentVersionString) );
            }
        }


    }
}
