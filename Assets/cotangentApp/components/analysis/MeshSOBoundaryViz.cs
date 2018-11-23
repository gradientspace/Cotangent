using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using g3;
using f3;

namespace cotangent
{

    /// <summary>
    /// Computes mesh boundaries in background thread, visualizes as curves
    /// 
    /// [TODO] cache MeshBoundaryLoops? this could be useful to keep around.
    /// 
    /// </summary>
    public class MeshSOBoundaryViz : AnalysisViz
    {
        public Colorf BoundaryLoopColor = Colorf.VideoBlue;
        public Colorf BoundarySpanColor = Colorf.VideoRed;
        public float BoundaryCurveWidth = 2.0f;

        DMeshSO SO;

        class MeshBoundaryData
        {
            public List<DCurve3> Loops = new List<DCurve3>();
            public List<DCurve3> Spans = new List<DCurve3>();
            public int timestamp;
        }
        MeshBoundaryData BoundaryInfo;

        List<fPolylineGameObject> BoundaryCurveGOs = new List<fPolylineGameObject>();


        public MeshSOBoundaryViz(DMeshSO so)
        {
            SO = so;
            SO.OnMeshModified += OnInputMeshModified;
        }


        public event AnalysisRequiresUpdateHandler OnComputeUpdateRequired;
        public event AnalysisRequiresUpdateHandler OnGeometryUpdateRequired;


        private void OnInputMeshModified(DMeshSO so)
        {
            OnComputeUpdateRequired?.Invoke(this);
        }



        public void DiscardDirtyGeometry()
        {
            foreach (var pgo in BoundaryCurveGOs)
                pgo.Destroy();
            BoundaryCurveGOs.Clear();
        }


        public void Disconnect()
        {
            is_disconnected = true;
            DiscardDirtyGeometry();
        }
        bool is_disconnected = false;



        public void ComputeOnBackgroundThread()
        {
            BoundaryInfo = null;

            object result = SO.SafeMeshRead(LockedComputeMeshBoundaryData);
            if (result is MeshBoundaryData) {
                BoundaryInfo = result as MeshBoundaryData;
                OnGeometryUpdateRequired?.Invoke(this);
            } else if (result is Exception) {
                DebugUtil.Log("MeshAnalysisViz.UpdateMesh - Exception: " + (result as Exception).Message);
            }
        }



        public IEnumerator UpdateGeometryOnMainThread()
        {
            if (BoundaryInfo == null)
                yield break;
            if (is_disconnected)
                yield break;
            MeshBoundaryData boundary_info = BoundaryInfo;

            // if mesh was modified before we got here, push it back onto dirty list
            if (boundary_info.timestamp != SO.Mesh.ShapeTimestamp) {
                OnComputeUpdateRequired?.Invoke(this);
                yield break;
            }

            int add_per_frame = 100;
            int counter = 0;
            foreach (DCurve3 loop in boundary_info.Loops) {
                if (is_disconnected || boundary_info.timestamp != SO.Mesh.ShapeTimestamp)
                    yield break;
                List<Vector3f> vertices = new List<Vector3f>(loop.Vertices.Select(x => (Vector3f)x).ToList());
                vertices.Add(vertices[0]);
                fPolylineGameObject go = GameObjectFactory.CreatePolylineGO("boundary_curve", vertices, BoundaryLoopColor, BoundaryCurveWidth, LineWidthType.Pixel);
                SO.RootGameObject.AddChild(go, false);
                BoundaryCurveGOs.Add(go);
                if (counter++ == add_per_frame) {
                    counter = 0;
                    yield return null;
                }
            }
            foreach (DCurve3 span in boundary_info.Spans) {
                if (is_disconnected || boundary_info.timestamp != SO.Mesh.ShapeTimestamp)
                    yield break;
                List<Vector3f> vertices = new List<Vector3f>(span.Vertices.Select(x => (Vector3f)x).ToList());
                fPolylineGameObject go = GameObjectFactory.CreatePolylineGO("boundary_span", vertices, BoundaryLoopColor, BoundaryCurveWidth, LineWidthType.Pixel);
                SO.RootGameObject.AddChild(go, false);
                BoundaryCurveGOs.Add(go);
                if (counter++ == add_per_frame) {
                    counter = 0;
                    yield return null;
                }
            }

        }




        /// <summary>
        /// this runs inside SO.SafeMeshRead
        /// </summary>
        private object LockedComputeMeshBoundaryData(DMesh3 mesh)
        {
            MeshBoundaryLoops loops = new MeshBoundaryLoops(mesh, true);

            MeshBoundaryData bd = new MeshBoundaryData();
            foreach (EdgeLoop loop in loops.Loops)
                bd.Loops.Add(loop.ToCurve());
            if (loops.SawOpenSpans) {
                foreach (EdgeSpan span in loops.Spans)
                    bd.Spans.Add(span.ToCurve());
            }

            bd.timestamp = mesh.ShapeTimestamp;

            return bd;
        }

    }
}
