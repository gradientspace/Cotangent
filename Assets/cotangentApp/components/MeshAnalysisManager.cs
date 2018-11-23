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

    public delegate void AnalysisRequiresUpdateHandler(AnalysisViz analysis);


    /// <summary>
    /// Implementation for background-compute analysis/visualization
    /// </summary>
    public interface AnalysisViz
    {
        // 1) you emit this event when you want to be updated
        event AnalysisRequiresUpdateHandler OnComputeUpdateRequired;

        // 2) this gets called on main thread. You don't have to do anything here,
        //    but it's good form because your background compute might take a while
        void DiscardDirtyGeometry();

        // 2) when this gets called, you do your update. Practice safe scene access!
        void ComputeOnBackgroundThread();

        // 3) you emit this when you have new geometry you want to add to scene
        event AnalysisRequiresUpdateHandler OnGeometryUpdateRequired;

        // 4) emit the geometry when this is called
        IEnumerator UpdateGeometryOnMainThread();

        // for when we are throwing you away
        void Disconnect();
    }




    public class MeshAnalysisManager
    {
        Thread processThread;
        AutoResetEvent processThreadEvent;


        public bool EnableMeshBoundaries {
            get { return enable_mesh_boundaries; }
            set { if (enable_mesh_boundaries != value) {
                    enable_mesh_boundaries = value;
                    update_mesh_boundaries();
                 }
            }
        }
        bool enable_mesh_boundaries = true;

        public bool EnableCavities {
            get { return enable_cavities; }
            set { if (enable_cavities != value) {
                    enable_cavities = value;
                    update_cavities();
                 }
            }
        }
        bool enable_cavities = false;




        public MeshAnalysisManager()
        {
            processThread = new Thread(ProcessingThread);
            processThread.Start();

            processThreadEvent = new AutoResetEvent(false);
        }


        public void AddMesh(DMeshSO sourceSO)
        {
            // make sure we have this SO
            lock (SOToAnalysis) {
                List<AnalysisViz> l;
                if (SOToAnalysis.TryGetValue(sourceSO, out l) == false) {
                    l = new List<AnalysisViz>();
                    SOToAnalysis[sourceSO] = l;
                }
            }

            if (EnableMeshBoundaries) {
                AnalysisViz boundaries = new MeshSOBoundaryViz(sourceSO);
                add_new_so_analysis(sourceSO, boundaries);
            }

            if (EnableCavities) {
                AnalysisViz interiorCavities = new MeshSOComponentsViz(sourceSO);
                add_new_so_analysis(sourceSO, interiorCavities);
            }
        }


        public void RemoveMesh(DMeshSO sourceSO)
        {
            List<AnalysisViz> found;
            lock (SOToAnalysis) {
                if (SOToAnalysis.TryGetValue(sourceSO, out found)) {
                    foreach (var analysis in found)
                        disconnect_analysis(analysis);
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
        void disconnect_analysis(AnalysisViz analysis) {
            analysis.OnComputeUpdateRequired -= OnVisualizationRequestedUpdate;
            analysis.OnGeometryUpdateRequired -= OnVisualizationHasGeometryUpdate;
            analysis.Disconnect();
        }



        void update_cavities()
        {
            if (enable_cavities == false)
                remove_by_type<MeshSOComponentsViz>();
            else
                add_type<MeshSOComponentsViz>((so) => { return new MeshSOComponentsViz(so as DMeshSO); });
        }


        void update_mesh_boundaries()
        {
            if (enable_mesh_boundaries == false)
                remove_by_type<MeshSOBoundaryViz>();
            else
                add_type<MeshSOBoundaryViz>((so) => { return new MeshSOBoundaryViz(so as DMeshSO); });
        }



        void add_type<T>( Func<SceneObject, T> factoryF ) where T : class, AnalysisViz
        {
            List<KeyValuePair<DMeshSO, AnalysisViz>> to_add = new List<KeyValuePair<DMeshSO, AnalysisViz>>();

            foreach (var pairs in SOToAnalysis) {
                if (pairs.Key is DMeshSO) {
                    DMeshSO so = pairs.Key as DMeshSO;
                    List<AnalysisViz> types = pairs.Value;
                    bool found = false;
                    for (int i = 0; i < types.Count; ++i) {
                        if (types[i] is T) {
                            found = true;
                            break;
                        }
                    }
                    if ( found == false ) {
                        AnalysisViz analysis = factoryF(so);
                        to_add.Add(new KeyValuePair<DMeshSO, AnalysisViz>(so, analysis));
                    }
                }
            }
            foreach (var pair in to_add)
                add_new_so_analysis(pair.Key, pair.Value);
        }


        void remove_by_type<T>() where T : class, AnalysisViz
        {
            List<KeyValuePair<DMeshSO, int>> to_remove = new List<KeyValuePair<DMeshSO, int>>();

            foreach (var pairs in SOToAnalysis) {
                if (pairs.Key is DMeshSO) {
                    DMeshSO so = pairs.Key as DMeshSO;
                    List<AnalysisViz> types = pairs.Value;
                    for (int i = 0; i < types.Count; ++i) {
                        if (types[i] is T) {
                            disconnect_analysis(types[i]);
                            to_remove.Add(new KeyValuePair<DMeshSO, int>(so, i));
                            break;
                        }
                    }
                }
            }
            foreach (var pair in to_remove)
                SOToAnalysis[pair.Key].RemoveAt(pair.Value);
        }



        T find_by_type<T>(SceneObject so) where T : class, AnalysisViz
        {
            lock (SOToAnalysis) {
                List<AnalysisViz> l;
                if (SOToAnalysis.TryGetValue(so, out l)) {
                    foreach ( var a in l ) {
                        if (a is T)
                            return a as T;
                    }
                }
                return null;
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




        //AnalysisViz FindFromSO(DMeshSO so)
        //{
        //    AnalysisViz am = null;
        //    lock (SOToAnalysis) {
        //        if (SOToAnalysis.TryGetValue(so, out am))
        //            return am;
        //    }
        //    return null;
        //}



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
