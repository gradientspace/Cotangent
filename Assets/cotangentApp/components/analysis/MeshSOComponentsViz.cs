using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using g3;
using f3;
using gs;

namespace cotangent
{

    /// <summary>
    /// Computes mesh connected components on background thread, visualizes interior cavities
    /// as transparent shells
    /// 
    /// [TODO] better visualization
    /// [TODO] cache MeshConnectedComponents? This would be useful to keep around...
    /// 
    /// </summary>
    public class MeshSOComponentsViz : AnalysisViz
    {
        public Colorf ComponentColor = new Colorf(1.0f, 1.0f, 1.0f, 0.01f);
        public fMaterial CavityMaterial;

        DMeshSO SO;

        class ComponentData
        {
            public List<DMesh3> InteriorMeshes = new List<DMesh3>();
            public int timestamp;
        }
        ComponentData InteriorComponents;

        List<fMeshGameObject> MeshGOs = new List<fMeshGameObject>();

        public MeshSOComponentsViz(DMeshSO so)
        {
            SO = so;
            SO.OnMeshModified += OnInputMeshModified;

            CavityMaterial = CCMaterials.NestedComponentMaterial;
            //CavityMaterial = MaterialUtil.CreateTransparentMaterialF(ComponentColor);
        }
        ~MeshSOComponentsViz()
        {
        }

        public event AnalysisRequiresUpdateHandler OnComputeUpdateRequired;
        public event AnalysisRequiresUpdateHandler OnGeometryUpdateRequired;


        private void OnInputMeshModified(DMeshSO so)
        {
            OnComputeUpdateRequired?.Invoke(this);
        }


        public void DiscardDirtyGeometry()
        {
            foreach (var pgo in MeshGOs)
                pgo.Destroy();
            MeshGOs.Clear();
        }


        public void Disconnect()
        {
            is_disconnected = true;
            DiscardDirtyGeometry();
        }
        bool is_disconnected = false;



        public void ComputeOnBackgroundThread()
        {
            InteriorComponents = null;

            object result = SO.SafeMeshRead(LockedComputeMeshBoundaryData);
            if (result is ComponentData) {
                InteriorComponents = result as ComponentData;
                OnGeometryUpdateRequired?.Invoke(this);
            } else if (result is Exception) {
                DebugUtil.Log("MeshAnalysisViz.UpdateMesh - Exception: " + (result as Exception).Message);
            }
        }



        public IEnumerator UpdateGeometryOnMainThread()
        {
            if (InteriorComponents == null)
                yield break;
            if (is_disconnected)
                yield break;
            ComponentData interior = InteriorComponents;

            // if mesh was modified before we got here, push it back onto dirty list
            if (interior.timestamp != SO.Mesh.ShapeTimestamp) {
                OnComputeUpdateRequired?.Invoke(this);
                yield break;
            }

            foreach ( DMesh3 mesh in interior.InteriorMeshes) {
                if (is_disconnected || interior.timestamp != SO.Mesh.ShapeTimestamp)
                    yield break;
                fMeshGameObject go = GameObjectFactory.CreateMeshGO("interior_comp", new fMesh(mesh), false, true);
                SO.RootGameObject.AddChild(go, false);
                go.SetMaterial(CavityMaterial, true);
                go.SetLayer(FPlatform.WidgetOverlayLayer);
                MeshGOs.Add(go);
                yield return null;
            }
        }




        /// <summary>
        /// this runs inside SO.SafeMeshRead
        /// </summary>
        private object LockedComputeMeshBoundaryData(DMesh3 mesh)
        {
            ComponentData data = new ComponentData();
            data.timestamp = mesh.ShapeTimestamp;

            MeshConnectedComponents comp = new MeshConnectedComponents(mesh);
            comp.FindConnectedT();
            if (comp.Count == 1)
                return data;

            // [TODO] 
            //   - a very common case is a huge mesh with a few floaters. We
            //     should have a way to handle this w/o having to create all
            //     these spatial data structures for the huge mesh! We can
            //     do a quick sort based on bounding boxes, and for very
            //     small submeshes we could use non-fast WN, etc, etc...


            DSubmesh3Set subMeshes = new DSubmesh3Set(mesh, comp);

            MeshSpatialSort sort = new MeshSpatialSort();
            foreach (var submesh in subMeshes)
                sort.AddMesh(submesh.SubMesh, submesh);
            sort.Sort();

            foreach ( var solid in sort.Solids ) {
                foreach (var cavity in solid.Cavities)
                    data.InteriorMeshes.Add(cavity.Mesh);
                if (solid.Outer.InsideOf.Count > 0)
                    data.InteriorMeshes.Add(solid.Outer.Mesh);
            }

            // reverse orientation so that shading and culling works
            gParallel.ForEach(data.InteriorMeshes, (m) => {
                m.ReverseOrientation(true);
            });

            return data;
        }

    }
}
