using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using g3;
using f3;

namespace cotangent
{

    public class GCodeAnalysisManager
    {
        Thread processThread;
        AutoResetEvent processThreadEvent;

        public bool EnableUpdates = true;

        public GCodeAnalysisManager()
        {
            processThread = new Thread(ProcessingThread);
            processThread.Start();

            processThreadEvent = new AutoResetEvent(false);
        }


        public void AddToolpaths(ToolpathSO sourceSO)
        {
            AnalysisViz boundaries = new ToolpathDeviationViz(sourceSO);
            add_new_so_analysis(sourceSO, boundaries);
        }


        public void RemoveToolpaths(ToolpathSO sourceSO)
        {
            List<AnalysisViz> found;
            lock (SOToAnalysis) {
                if (SOToAnalysis.TryGetValue(sourceSO, out found)) {
                    foreach (var analysis in found) {
                        analysis.OnComputeUpdateRequired -= OnVisualizationRequestedUpdate;
                        analysis.OnGeometryUpdateRequired -= OnVisualizationHasGeometryUpdate;
                        analysis.Disconnect();
                    }
                    SOToAnalysis.Remove(sourceSO);
                }
            }
            lock (DirtyList) {
                if (found != null) {
                    foreach (var analysis in found)
                        DirtyList.Remove(analysis);
                }
            }
        }



        void add_new_so_analysis(SceneObject so, AnalysisViz analysis)
        {
            lock (SOToAnalysis) {
                List<AnalysisViz> l;
                if (SOToAnalysis.TryGetValue(so, out l) == false) {
                    l = new List<AnalysisViz>();
                    SOToAnalysis[so] = l;
                }
                l.Add(analysis);
            }

            analysis.OnComputeUpdateRequired += OnVisualizationRequestedUpdate;
            analysis.OnGeometryUpdateRequired += OnVisualizationHasGeometryUpdate;

            lock (DirtyList) {
                DirtyList.Add(analysis);
                processThreadEvent.Set();
            }
        }



        Dictionary<SceneObject, List<AnalysisViz>> SOToAnalysis = new Dictionary<SceneObject, List<AnalysisViz>>();

        List<AnalysisViz> DirtyList = new List<AnalysisViz>();
        List<AnalysisViz> ClearPending = new List<AnalysisViz>();
        List<AnalysisViz> UpdatePending = new List<AnalysisViz>();
        


        /// <summary>
        /// Update is called on UI thread, where we do UI-thread things
        /// </summary>
        public void Update()
        {
            if (EnableUpdates == false)
                return;

            if (UpdatePending.Count == 0)
                return;

            List<AnalysisViz> ClearThisFrame = new List<AnalysisViz>();
            lock (ClearPending) {
                ClearThisFrame.AddRange(ClearPending);
                ClearPending.Clear();
            }
            foreach (var analysis in ClearThisFrame)
                analysis.DiscardDirtyGeometry();

            // extract updates we will process this loop
            List<AnalysisViz> UpdateThisFrame = new List<AnalysisViz>();
            lock (UpdatePending) {
                UpdateThisFrame.AddRange(UpdatePending);
                UpdatePending.Clear();
            }


            FPlatform.CoroutineExec.StartAnonymousCoroutine(update_geometry(UpdateThisFrame));
        }


        public void Shutdown()
        {
            // wake up bg thread so it can kill itself
            processThreadEvent.Set();
            processThread.Abort();
        }



        IEnumerator update_geometry(List<AnalysisViz> updates)
        {
            foreach (AnalysisViz am in updates) {
                IEnumerator e = am.UpdateGeometryOnMainThread();
                while (e.MoveNext())
                    yield return e.Current;
                //yield return StartCoroutine(am.UpdateGeometryOnMainThread());
            }
        }



        /// <summary>
        /// **This may be called from background threads**
        /// </summary>
        private void OnVisualizationRequestedUpdate(AnalysisViz analysis)
        {
            // [TODO] check that this analysis is still active

            lock (ClearPending) {
                if (ClearPending.Contains(analysis) == false)
                    ClearPending.Add(analysis);
            }
            lock (DirtyList) {
                if (DirtyList.Contains(analysis) == false) {
                    DirtyList.Add(analysis);
                    processThreadEvent.Set();
                }
            }
        }




        private void OnVisualizationHasGeometryUpdate(AnalysisViz analysis)
        {
            // [TODO] check that this analysis is still active
            lock (UpdatePending) {
                UpdatePending.Add(analysis);
            }
        }




        /*
         * Background compute thread for mesh boundary loops
         */

        private void ProcessingThread()
        {
            List<AnalysisViz> toProcess = new List<AnalysisViz>();
            while (true) {
                processThreadEvent.WaitOne(1000);

                if (FPlatform.ShutdownBackgroundThreadsOnQuit)
                    break;

                toProcess.Clear();
                lock (DirtyList) {
                    toProcess.AddRange(DirtyList);
                }

                foreach (AnalysisViz a in toProcess ) {
                    bool still_dirty = true;
                    lock (DirtyList) {
                        still_dirty = DirtyList.Remove(a);
                    }
                    if (still_dirty) {
                        a.ComputeOnBackgroundThread();
                    }
                }
            }
        }

       

    }
}
