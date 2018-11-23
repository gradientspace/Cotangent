using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using f3;
using gs;

namespace cotangent
{
    /// <summary>
    /// TODO: keep filename / meshso dictionary, so if we have same object
    /// multiple times we only read once?
    /// </summary>
    public class ExternalFileMonitor
    {
        Thread processThread;
        AutoResetEvent processThreadEvent;

        public ExternalFileMonitor()
        {
            CC.ActiveScene.ChangedEvent += ActiveScene_ChangedEvent;

            processThread = new Thread(ProcessingThread);
            processThread.Start();
            processThreadEvent = new AutoResetEvent(false);
        }


        List<PrintMeshSO> TrackedMeshes = new List<PrintMeshSO>();

        public void AddMesh(PrintMeshSO sourceSO)
        {
            if (!File.Exists(sourceSO.SourceFilePath))
                return;

            lock (TrackedMeshes) {
                if (TrackedMeshes.Contains(sourceSO) == false)
                    TrackedMeshes.Add(sourceSO);
            }
        }


        public void RemoveMesh(PrintMeshSO so)
        {
            lock(TrackedMeshes) {
                TrackedMeshes.Remove(so);
            }
        }


        private void ActiveScene_ChangedEvent(object sender, SceneObject so, SceneChangeType type)
        {
            if (so is PrintMeshSO == false)
                return;
            PrintMeshSO printSO = so as PrintMeshSO;

            if (type == SceneChangeType.Removed ) 
                RemoveMesh(printSO);
            else if ( type == SceneChangeType.Added 
                && printSO.CanAutoUpdateFromSource() 
                && printSO.AutoUpdateOnSourceFileChange ) {
                    // this should only happen on undo of delete, right?
                    AddMesh(printSO);
            }
        }


        public void Shutdown()
        {
            CC.ActiveScene.ChangedEvent -= ActiveScene_ChangedEvent;

            // wake up bg thread so it can kill itself
            processThreadEvent.Set();
            processThread.Abort();
        }




        /*
         * Background compute thread for file monitor
         */

        private void ProcessingThread()
        {
            List<PrintMeshSO> toProcess = new List<PrintMeshSO>();
            while (true) {
                processThreadEvent.WaitOne(CCPreferences.AutoReloadCheckFrequencyS * 1000);

                if (FPlatform.ShutdownBackgroundThreadsOnQuit)
                    break;

                toProcess.Clear();
                lock (TrackedMeshes) {
                    toProcess.AddRange(TrackedMeshes);
                }

                foreach (PrintMeshSO so in toProcess) {
                    if (so.AutoUpdateOnSourceFileChange == false)
                        continue;

                    long cur_timestamp = File.GetLastWriteTime(so.SourceFilePath).Ticks;
                    if ( cur_timestamp != so.LastReadFileTimestamp ) {
                        MeshImporter importer = new MeshImporter();
                        importer.ImportAutoUpdate(so);
                    }
                }
            }
        }

    }
}
