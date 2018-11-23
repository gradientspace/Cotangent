using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using f3;
using g3;

namespace cotangent
{
    public class MeshExporter
    {
        List<DMeshSO> ExportMeshSOs;
        string WritePath;

        public bool ExportYUp = false;

        // This is called when work on main thread completes
        public Action MainThreadWorkCompletedF = null;


        // This is called when write finishes
        public Action<IOWriteResult> OnCompletedF = null;

        // This will be called by StandardMeshWriter. 
        // **THIS RUNS IN A BACKGROUND THREAD** so be safe!
        public Action<int, int> BackgroundProgressFunc = null;

        public MeshExporter(List<DMeshSO> meshes, string path)
        {
            ExportMeshSOs = meshes;
            WritePath = path;
        }


        DMesh3[] ExportMeshes;
        Frame3f[] MeshFrames;

        public IEnumerator RunMainThreadProcessing(bool bSpawnBackgroundWrite = true)
        {
            int N = ExportMeshSOs.Count;
            ExportMeshes = new DMesh3[N];
            MeshFrames = new Frame3f[N];

            for ( int k = 0; k < N; ++k ) {
                DMeshSO so = ExportMeshSOs[k];
                DMesh3 copy = new DMesh3(so.Mesh, false, MeshComponents.VertexColors|MeshComponents.VertexNormals);

                // if we have scaling or parent hierarchy, need to transform to scene in main
                // thread, because we have no way of storing transform stack currently
                if ( so.GetLocalScale() != Vector3f.One || so.Parent != so.GetScene()) {
                    foreach ( int vid in copy.VertexIndices() ) {
                        Vector3d v = copy.GetVertex(vid);
                        Vector3f n = copy.GetVertexNormal(vid);
                        Frame3f f = new Frame3f(v, n);
                        f = SceneTransforms.ObjectToScene(so, f);
                        copy.SetVertex(vid, f.Origin);
                        copy.SetVertexNormal(vid, f.Z);
                    }
                    MeshFrames[k] = Frame3f.Identity;
                } else {
                    MeshFrames[k] = ExportMeshSOs[k].GetLocalFrame(CoordSpace.ObjectCoords);
                }

                ExportMeshes[k] = copy;

                yield return null;
            }

            if (MainThreadWorkCompletedF != null)
                MainThreadWorkCompletedF();
            DebugUtil.Log("MeshExporter: completed main thread processing");

            if (bSpawnBackgroundWrite) {
                ThreadPool.QueueUserWorkItem((state) => {
                    var result = RunBackgroundWrite();
                    if (OnCompletedF != null)
                        OnCompletedF(result);
                });
            }

        }



        public IOWriteResult RunBackgroundWrite()
        {
            // transform meshes
            gParallel.ForEach(Interval1i.Range(ExportMeshes.Length), (i) => {
                if (MeshFrames[i].Origin != Vector3f.Zero || MeshFrames[i].Rotation != Quaternionf.Identity)
                    MeshTransforms.FromFrame(ExportMeshes[i], MeshFrames[i]);

                MeshTransforms.FlipLeftRightCoordSystems(ExportMeshes[i]);

                if (ExportYUp == false)
                    MeshTransforms.ConvertYUpToZUp(ExportMeshes[i]);
            });


            List<WriteMesh> writeMeshes = new List<WriteMesh>();
            for (int i = 0; i < ExportMeshes.Length; ++i)
                writeMeshes.Add(new WriteMesh(ExportMeshes[i]));


            WriteOptions options = WriteOptions.Defaults;
            options.bWriteBinary = true;
            options.ProgressFunc = BackgroundProgressFunc;

            StandardMeshWriter writer = new StandardMeshWriter();
            IOWriteResult result = writer.Write(WritePath, writeMeshes, options);
            return result;
        }



    }
}
