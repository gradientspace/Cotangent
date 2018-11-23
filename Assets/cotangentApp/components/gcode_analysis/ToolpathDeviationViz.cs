using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using g3;
using f3;
using gs;

namespace cotangent
{

    /// <summary>
    /// (todo)
    /// 
    /// </summary>
    public class ToolpathDeviationViz : AnalysisViz
    {
        fMaterial HighDeviationMaterial;
        fMaterial MedDeviationMaterial;
        fMaterial LowDeviationMaterial;

        double DeviationToleranceMM = 0.25f;

        ToolpathSO SO;


        GOWrapperSO internalSO;

        struct DeviationPt
        {
            public Vector3d pos;
            public double dist;
        }

        class DeviationData
        {
            public List<DeviationPt> DeviationPoints = new List<DeviationPt>();
        }
        DeviationData Deviation;

        List<fGameObject> PointGOs = new List<fGameObject>();

        public ToolpathDeviationViz(ToolpathSO so)
        {
            SO = so;
            //SO.OnMeshModified += OnInputModified;

            HighDeviationMaterial = MaterialUtil.CreateTransparentMaterialF(Colorf.VideoRed);
            MedDeviationMaterial = MaterialUtil.CreateTransparentMaterialF(Colorf.Orange);
            LowDeviationMaterial = MaterialUtil.CreateTransparentMaterialF(Colorf.Yellow);
        }
        ~ToolpathDeviationViz()
        {
        }

        public event AnalysisRequiresUpdateHandler OnComputeUpdateRequired;
        public event AnalysisRequiresUpdateHandler OnGeometryUpdateRequired;


        private void OnInputModified(ToolpathSO so)
        {
            OnComputeUpdateRequired?.Invoke(this);
        }


        public void DiscardDirtyGeometry()
        {
            foreach (var pgo in PointGOs)
                pgo.Destroy();
            PointGOs.Clear();
        }

        public void Disconnect()
        {
            is_disconnected = true;
            if (internalSO != null) {
                CC.ActiveScene.LinkManager.RemoveAllLinksToSO(internalSO, true, true);
                CC.ActiveScene.RemoveSceneObject(internalSO, true);
            }
        }
        bool is_disconnected = false;


        public void ComputeOnBackgroundThread()
        {
            Deviation = null;

            DebugUtil.Log(SO.GetToolpathStats());

            ToolpathSet paths = SO.GetToolpaths();
            PlanarSliceStack slices = SO.GetSlices();
            var settings = SO.GetSettings();

            // AAHHH
            double bed_width = settings.Machine.BedSizeXMM;
            double bed_height = settings.Machine.BedSizeYMM;
            Vector3d origin = new Vector3d(-bed_width / 2, -bed_height / 2, 0);
            if (settings is gs.info.MakerbotSettings)
                origin = Vector3d.Zero;

            List<DeviationPt> points = new List<DeviationPt>();
            SpinLock pointsLock = new SpinLock();
            Action<DeviationPt> appendPointF = (pt) => {
                bool entered = false;
                pointsLock.Enter(ref entered);
                points.Add(pt);
                pointsLock.Exit();
            };

            double tolerance = settings.Machine.NozzleDiamMM * 0.5 + DeviationToleranceMM;

            gParallel.ForEach(Interval1i.Range(slices.Count), (slicei) => {
                PlanarSlice slice = slices[slicei];

                //Interval1d zrange = (slicei < slices.Count - 1) ?
                //    new Interval1d(slice.Z, slices[slicei + 1].Z - 0.5*settings.LayerHeightMM) :
                //    new Interval1d(slice.Z, slice.Z + 0.5*settings.LayerHeightMM);
                double dz = 0.5 * settings.LayerHeightMM;
                Interval1d zrange = new Interval1d(slice.Z - dz, slice.Z + dz);

                double cellSize = 2.0f;

                ToolpathsLayerGrid grid = new ToolpathsLayerGrid();
                grid.Build(paths, zrange, cellSize);

                foreach (GeneralPolygon2d poly in slice.Solids) {
                    measure_poly(poly.Outer, slice.Z, grid, tolerance, appendPointF);
                    foreach (var hole in poly.Holes)
                        measure_poly(poly.Outer, slice.Z, grid, tolerance, appendPointF);
                }
            });

            int N = points.Count;
            for ( int k = 0; k < N; ++k ) {
                DeviationPt pt = points[k];
                Vector3d v = origin + pt.pos;
                v = MeshTransforms.ConvertZUpToYUp(v);
                pt.pos = MeshTransforms.FlipLeftRightCoordSystems(v);
                points[k] = pt;
            }

            Deviation = new DeviationData();
            Deviation.DeviationPoints = points;
            OnGeometryUpdateRequired?.Invoke(this);
        }
        void measure_poly(Polygon2d poly, double layerZ, ToolpathsLayerGrid grid, double tolerance, Action<DeviationPt> appendPointF)
        {
            int NV = poly.VertexCount;
            for ( int k = 0; k < NV; ++k ) {
                Vector2d pt = poly[k];
                double dist = grid.Distance(pt, 3 * tolerance);
                if (dist > tolerance) {
                    appendPointF(new DeviationPt() {
                        pos = new Vector3d(pt.x, pt.y, layerZ),
                        dist = dist
                    });
                }
            }
        }



        public IEnumerator UpdateGeometryOnMainThread()
        {
            if (Deviation == null)
                yield break;
            if (Deviation.DeviationPoints.Count == 0)
                yield break;
            if (is_disconnected)
                yield break;

            if ( internalSO == null ) {
                internalSO = new GOWrapperSO();
                internalSO.Create(GameObjectFactory.CreateParentGO("toolpath_deviation_so"));
                CC.ActiveScene.AddSceneObject(internalSO);
                internalSO.SetLocalFrame(SO.GetLocalFrame(CoordSpace.SceneCoords), CoordSpace.SceneCoords);
                SOFrameLink link = new SOFrameLink(internalSO, SO);
                CC.ActiveScene.LinkManager.AddLink(link);
            }

            var settings = SO.GetSettings();
            double nozzleDiam = settings.Machine.NozzleDiamMM;
            double tolerance = nozzleDiam*0.5 + DeviationToleranceMM;

            float layerHeight = (float)settings.LayerHeightMM;
            float ptWidth = (float)nozzleDiam * 0.5f;

            // probably should just make a mesh for all these...
            int counter = 0;
            int N = Deviation.DeviationPoints.Count;
            for ( int i = 0; i < N; ++i ) {
                if (is_disconnected)
                    yield break;
                DeviationPt pt = Deviation.DeviationPoints[i];
                fMaterial useMaterial = HighDeviationMaterial;
                if (pt.dist < tolerance * 1.5)
                    useMaterial = LowDeviationMaterial;
                //else if (pt.dist < tolerance * 1.2)
                //    useMaterial = LowDeviationMaterial;

                fGameObject go = GameObjectFactory.CreateBoxGO("deviation", ptWidth, layerHeight, ptWidth, useMaterial, true, false);
                go.SetLocalPosition((Vector3f)pt.pos);
                internalSO.AppendNewGO(go, internalSO.RootGameObject, false);
                go.SetLayer(FPlatform.WidgetOverlayLayer);
                PointGOs.Add(go);
                if ( counter++ == 10) {
                    counter = 0;
                    yield return null;
                }
            }
        }



        class ToolpathsLayerGrid
        {
            DVector<Segment2d> segments;
            SegmentHashGrid2d<int> grid;

            public void Build(ToolpathSet toolpaths, Interval1d zrange, double cellSize = 2.0)
            {
                segments = new DVector<Segment2d>();
                grid = new SegmentHashGrid2d<int>(cellSize, -1);

                Action<LinearToolpath3<PrintVertex>> processPathF = (polyPath) => {
                    if (polyPath.Type != ToolpathTypes.Deposition || polyPath.IsPlanar == false)
                        return;
                    if ((polyPath.TypeModifiers & FillTypeFlags.OutermostShell) == 0)
                        return;
                    Vector3d v0 = polyPath.Start.Position;
                    Vector3d v1 = polyPath.End.Position;
                    if (zrange.Contains(v0.z) == false || zrange.Contains(v1.z) == false)
                        return;
                    append_path(polyPath, cellSize);
                };
                process_linear_paths(toolpaths, processPathF);
            }



            public double Distance(Vector2d v, double max_query_dist)
            {
                var found = grid.FindNearestInSquaredRadius(v, max_query_dist * max_query_dist,
                    (idx) => {
                        return segments[idx].DistanceSquared(v);
                    });
                if (found.Key != -1)
                    return Math.Sqrt(found.Value);
                else
                    return max_query_dist;
            }


            void append_path(LinearToolpath3<PrintVertex> path, double cellSize)
            {
                double threshSqr = 4 * cellSize * cellSize;

                int NV = path.VertexCount - 1;
                for ( int i = 0; i < NV; ++i) {
                    Vector2d a = path[i].Position.xy;
                    Vector2d b = path[i + 1].Position.xy;
                    double dist_sqr = a.DistanceSquared(b);
                    if ( dist_sqr < threshSqr) {
                        Segment2d seg = new Segment2d(a, b);
                        append_segment(ref seg);
                    } else {
                        int n = (int)(dist_sqr / threshSqr) + 1;
                        Vector2d prev = a;
                        for ( int k = 1; k <= n; ++k ) {
                            double t = ((double)k / (double)n);
                            Vector2d next = Vector2d.Lerp(a, b, t);
                            Segment2d seg = new Segment2d(prev, next);
                            append_segment(ref seg);
                            prev = next;
                        }
                    }
                }
            }

            void append_segment(ref Segment2d seg) {
                int idx = segments.size;
                segments.push_back(seg);
                grid.InsertSegment(idx, seg.Center, seg.Extent);
            }



            void process_linear_paths(ToolpathSet ToolpathSetIn, Action<LinearToolpath3<PrintVertex>> processF)
            {
                Action<IToolpathSet> pathsF = null;
                pathsF = (pathList) => {
                    foreach (IToolpath path in pathList) {
                        if (path is IToolpathSet)
                            pathsF(path as IToolpathSet);
                        else if (path is LinearToolpath3<PrintVertex>)
                            processF(path as LinearToolpath3<PrintVertex>);
                    }
                };
                pathsF(ToolpathSetIn);
            }

        }




        



        ///// <summary>
        ///// this runs inside SO.SafeMeshRead
        ///// </summary>
        //private object LockedComputeMeshBoundaryData(DMesh3 mesh)
        //{
        //    MeshConnectedComponents comp = new MeshConnectedComponents(mesh);
        //    comp.FindConnectedT();
        //    DSubmesh3Set subMeshes = new DSubmesh3Set(mesh, comp);

        //    MeshSpatialSort sort = new MeshSpatialSort();
        //    foreach (var submesh in subMeshes)
        //        sort.AddMesh(submesh.SubMesh, submesh);
        //    sort.Sort();

        //    ComponentData data = new ComponentData();
        //    foreach ( var solid in sort.Solids ) {
        //        foreach (var cavity in solid.Cavities)
        //            data.InteriorMeshes.Add(cavity.Mesh);
        //        if (solid.Outer.InsideOf.Count > 0)
        //            data.InteriorMeshes.Add(solid.Outer.Mesh);
        //    }

        //    data.timestamp = mesh.ShapeTimestamp;
        //    return data;
        //}

    }
}
