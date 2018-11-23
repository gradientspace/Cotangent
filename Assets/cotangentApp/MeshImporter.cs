using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using g3;
using f3;

namespace cotangent
{
    /// <summary>
    /// Imports print meshes.
    /// TODO:
    ///    - be able to track all xforms to a mesh, so we can replay
    /// </summary>
    public class MeshImporter
    {
        // thresholds for breaking out to interactive import assistant flow
        public int TriCountThreshold = CCPreferences.LargeMeshImportThreshold;
        public double HeightMinThreshold = 5.0;

        // if things go wrong
        public string ErrorMessage = null;


        // internal bits

        int TriCount = 0;
        AxisAlignedBox3d Bounds = AxisAlignedBox3d.Zero;
        string SourceFilePath;



        public bool ImportAutoUpdate(PrintMeshSO so)
        {
            SourceFilePath = so.SourceFilePath;
            if ( ! File.Exists(SourceFilePath) ) {
                ErrorMessage = "MeshImporter.ImportAutoUpdate: file does not exist";
                return false;
            }

            DMesh3Builder builder = new DMesh3Builder();
            StandardMeshReader reader = new StandardMeshReader() { MeshBuilder = builder };

            long timestamp = File.GetLastWriteTime(SourceFilePath).Ticks;

            IOReadResult result = reader.Read(SourceFilePath, ReadOptions.Defaults);
            if (result.code != IOCode.Ok) {
                ErrorMessage = "MeshImporter.ImportAutoUpdate: failed with message " + result.message;
                return false;
            }
            if (builder.Meshes.Count == 0) {
                ErrorMessage = "MeshImporter.ImportAutoUpdate: no meshes in file!";
                return false;
            }
            if (builder.Meshes.Count != 1) {
                ErrorMessage = "MeshImporter.ImportAutoUpdate: can only auto-update from file with single mesh!";
                return false;
            }
            DMesh3 mesh = builder.Meshes[0];

            // unity xforms
            MeshTransforms.ConvertZUpToYUp(mesh);
            MeshTransforms.FlipLeftRightCoordSystems(mesh);

            // wait for any active tools to finish
            // [TODO] do we need to do this?
            while ( CC.ActiveContext.ToolManager.HasActiveTool() ) {
                Thread.Sleep(1000);
            }
            
            if ( CC.ActiveScene.SceneObjects.Contains(so) == false ) {
                ErrorMessage = "MeshImporter.ImportAutoUpdate: SO no longer exists";
                return false;
            }

            // change event??

            so.LastReadFileTimestamp = timestamp;
            ThreadMailbox.PostToMainThread(() => {
                so.ReplaceMesh(mesh, true);
            });
            return true;
        }





        public async Task ImportInteractive(string sFilename, Action<string> onCompletedF)
        {
            SourceFilePath = sFilename;

            DMesh3Builder builder = new DMesh3Builder();
            StandardMeshReader reader = new StandardMeshReader() { MeshBuilder = builder };

            await Task.Run(() => {
                IOReadResult result = reader.Read(SourceFilePath, ReadOptions.Defaults);
                if (result.code != IOCode.Ok) {
                    ErrorMessage = "MeshImporter.Import: failed with message " + result.message;
                    return;
                }
            });

            if (builder.Meshes.Count == 0) {
                ErrorMessage = "MeshImporter.Import: no meshes in file!";
                return;
            }

            await Task.Run(() => {
                // apply unity xforms
                foreach (DMesh3 mesh in builder.Meshes) {
                    MeshTransforms.ConvertZUpToYUp(mesh);
                    MeshTransforms.FlipLeftRightCoordSystems(mesh);
                }

                TriCount = 0;
                Bounds = AxisAlignedBox3d.Empty;
                foreach (var m in builder.Meshes) {
                    TriCount += m.TriangleCount;
                    Bounds.Contain(m.CachedBounds);
                }
            });

            bool bSmall = (Bounds.MaxDim < HeightMinThreshold);
            bool bTall = (Bounds.Height > CC.Settings.BedSizeYMM);
            double maxXZ = Math.Max(Bounds.Width, Bounds.Depth);
            double bedMin = Math.Min(CC.Settings.BedSizeXMM, CC.Settings.BedSizeZMM);
            bool bLarge = (Bounds.Width > 2*CC.Settings.BedSizeXMM) || (maxXZ > 2*bedMin);
            bool bTriCount = (TriCount > TriCountThreshold);

            switch (CCPreferences.ImportAssistantMode) {
                case CCPreferences.ImportAssistantModes.PhysicalSizeOnly:
                    bTriCount = false; break;
                case CCPreferences.ImportAssistantModes.MeshSizeOnly:
                    bLarge = bSmall = bTall = false; break;
                case CCPreferences.ImportAssistantModes.Disabled:
                    bLarge = bSmall = bTall = bTriCount = false; break;
            }

            if (bTriCount || bSmall || bLarge) {
                ImportMeshDialog.Show(CotangentUI.MainUICanvas,
                    bSmall, bTall, bLarge, Bounds.Height,
                    bTriCount, TriCount, 
                    async (scale, tricount) => { await process_and_complete_import(SourceFilePath, builder, scale, tricount, onCompletedF); },
                    async () => { await complete_import(SourceFilePath, builder, onCompletedF); }
                    );
            } else {
                await complete_import(SourceFilePath, builder, onCompletedF);
            }
        }




        async Task process_and_complete_import(string sFilename, DMesh3Builder builder, 
            double targetHeight, int reduceToCount, 
            Action<string> onCompletedF)
        {
            CCStatus.BeginOperation("processing");

            await Task.Run( () => { 
                if ( targetHeight > 0.001 && Math.Abs(targetHeight-Bounds.Height) > 0.001 ) {
                    double scaleH = targetHeight / Bounds.Height;
                    Vector3d o = Bounds.Center - Bounds.Extents.y * Vector3d.AxisY;
                    foreach ( var mesh in builder.Meshes ) {
                        MeshTransforms.Scale(mesh, scaleH * Vector3d.One, o);
                    }
                }
            });

            await Task.Run( () => { 
                if ( reduceToCount != -1 && reduceToCount < TriCount) {
                    foreach ( var mesh in builder.Meshes) {
                        if (mesh.TriangleCount < 10)
                            continue;
                        double tri_fraction = (double)mesh.TriangleCount / (double)TriCount;
                        int NT = (int)(tri_fraction * reduceToCount);
                        if (NT < 10)
                            NT = 10;
                        Reducer r = new Reducer(mesh);
                        r.ReduceToTriangleCount(NT);
                    }
                }
            });

            CCStatus.EndOperation("processing");

            await complete_import(sFilename, builder, onCompletedF);
        }




        async Task complete_import(string sFilename, DMesh3Builder builder, Action<string> onCompletedF )
        {
            AxisAlignedBox3d bounds = AxisAlignedBox3d.Empty;
            foreach (DMesh3 mesh in builder.Meshes) {
                bounds.Contain(mesh.CachedBounds);
            }
            Vector3d centerPt = bounds.Center;
            Vector3d basePt = centerPt - bounds.Height * 0.5f * Vector3d.AxisY;

            Vector3d vTranslate = basePt;
            await Task.Run(() => {
                foreach (DMesh3 mesh in builder.Meshes)
                    MeshTransforms.Translate(mesh, -vTranslate);
            });

            bool bFirst = (CC.Objects.PrintMeshes.Count == 0);
            Vector3d postTranslate = Vector3d.Zero;
            switch (CCPreferences.ImportTransformMode) {
                case CCPreferences.ImportTransformModes.AutoCenterAll:
                    break;
                case CCPreferences.ImportTransformModes.AutoCenterFirst:
                    if (bFirst)
                        CCState.SceneImportTransform = vTranslate;
                    postTranslate = vTranslate - CCState.SceneImportTransform;
                    break;
                case CCPreferences.ImportTransformModes.NoAutoCenter:
                    postTranslate = vTranslate;
                    break;
            }

            // compact input meshes
            await Task.Run(() => {
                gParallel.ForEach(builder.Meshes, (mesh) => {
                    MeshEditor.RemoveUnusedVertices(mesh);
                });
                gParallel.ForEach(Interval1i.Range(builder.Meshes.Count), (k) => {
                    if (builder.Meshes[k].IsCompact == false)
                        builder.Meshes[k] = new DMesh3(builder.Meshes[k], true);
                });
            });

            string sBaseName = Path.GetFileNameWithoutExtension(sFilename);

            foreach (DMesh3 mesh in builder.Meshes) {
                PrintMeshSO meshSO = new PrintMeshSO();
                meshSO.Create(mesh, CCMaterials.PrintMeshMaterial);
                meshSO.UpDirection = UpDirection.ZUp;

                Frame3f f = meshSO.GetLocalFrame(CoordSpace.ObjectCoords);
                f.Origin = f.Origin + (Vector3f)postTranslate;
                meshSO.SetLocalFrame(f, CoordSpace.ObjectCoords);

                // if only one mesh, we can keep a reference
                if ( builder.Meshes.Count == 1 ) {
                    meshSO.SourceFilePath = sFilename;
                    meshSO.LastReadFileTimestamp = File.GetLastWriteTime(SourceFilePath).Ticks;
                }

                meshSO.Name = UniqueNames.GetNext(sBaseName);

                CCActions.AddNewPrintMesh(meshSO);
            }

            if (onCompletedF != null)
                onCompletedF(sFilename);
        }


    }
}
