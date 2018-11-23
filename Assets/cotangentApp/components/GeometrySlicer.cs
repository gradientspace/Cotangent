using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using f3;
using g3;
using gs;

namespace cotangent
{

    public struct SlicingProgressStatus
    {
        public int curProgress;
        public int maxProgress;
        public bool bFailed;
        public SlicingProgressStatus(int cur, int max)
        {
            bFailed = false; curProgress = cur; maxProgress = max;
        }
        public SlicingProgressStatus(bool failed)
        {
            bFailed = failed; curProgress = 1; maxProgress = 1;
        }
    }


    // This action will be called every frame w/ (current_count, total_count) as arguments
    public delegate void SlicingProgressHandler(SlicingProgressStatus status);




    /// <summary>
    /// Auto-compute slicing of current CC.PrintMeshes
    /// 
    /// [TODO] 
    ///    - be smarter about polylines?
    ///    - don't necessarily always need to discard SlicerMeshes, for example if only
    ///      slicing params changed, we can avoid copy (maybe irrelevant though)
    /// </summary>
    public class GeometrySlicer
    {
        PrintMeshAssembly SlicerMeshes;
        PlanarSliceStack SliceSet;
        bool SliceStackValid;
        object data_lock = new object();


        bool show_slice_polylines = false;
        public bool ShowSlicePolylines {
            get { return show_slice_polylines; }
            set { set_slice_polylines_visible(value); }
        }


        public delegate void SlicingStateChangeHandler();
        public event SlicingStateChangeHandler SlicingInvalidatedEvent;
        public event SlicingStateChangeHandler SlicingUpdatedEvent;

        public event SlicingProgressHandler SlicingProgressEvent;


        public bool PauseSlicing = false;

        public GeometrySlicer()
        {
            InvalidateSlicing();
        }



        public bool IsResultValid()
        {
            bool valid = false;
            lock (data_lock) {
                valid = SliceStackValid;
            }
            return valid;
        }


        public bool ExtractResultsIfValid(out PrintMeshAssembly printMeshes, out PlanarSliceStack slices)
        {
            bool valid = false;
            lock(data_lock) {
                printMeshes = SlicerMeshes;
                slices = SliceSet;
                valid = SliceStackValid;
            }
            return valid;
        }



        public void Update()
        {
            if (CCActions.CurrentViewMode != AppViewMode.PrintView)
                return;
            if (PauseSlicing)
                return;

            // [TODO] && slicing_requested == false
            if (CCPreferences.ActiveSlicingUpdateMode == CCPreferences.SlicingUpdateModes.SliceOnDemand)
                return;


            if ( SliceStackValid == false  ) {
                if (is_computing() == false)
                    update_slicing();
                else
                    do_progress();
            }

            bool new_slicing = process_completed_compute();
            if ( new_slicing ) {
                if (SlicingUpdatedEvent != null)
                    SlicingUpdatedEvent();
            }
        }



        public void InvalidateSlicing()
        {
            cancel_active_compute();

            lock (data_lock) {
                SlicerMeshes = null;
                SliceSet = null;
                SliceStackValid = false;
            }

            discard_slice_polylines();

            CC.InvalidateToolPaths();

            if (SlicingInvalidatedEvent != null)
                SlicingInvalidatedEvent();
        }



        void synchronize_meshes(BackgroundThreadData d)
        {
            // filter out any ignored/invalid meshes
            List<PrintMeshSO> printMeshes = CC.Objects.PrintMeshes;
            List<PrintMeshSO> useMeshes = new List<PrintMeshSO>();
            foreach ( PrintMeshSO printMesh in printMeshes ) {
                if (printMesh.Settings.ObjectType != PrintMeshSettings.ObjectTypes.Ignored)
                    useMeshes.Add(printMesh);
            }

            // this has to run in main thread
            int N = useMeshes.Count;
            d.meshToScene = new Frame3f[N];
            d.localScale = new Vector3f[N];
            d.meshCopies = new DMesh3[N];
            d.meshSettings = new PrintMeshSettings[N];
            for (int k = 0; k < N; ++k) {
                PrintMeshSO so = useMeshes[k];
                d.meshCopies[k] = new DMesh3(so.Mesh);
                d.meshToScene[k] = SceneTransforms.ObjectToScene(so, Frame3f.Identity);
                d.localScale[k] = so.GetLocalScale();
                d.meshSettings[k] = so.Settings.Clone();
            }
        }


        float last_spawn_time = 0;

        bool time_to_start_new_compute() {
            return FPlatform.RealTime() - last_spawn_time > 0.5;
        }
        void mark_spawn_time() {
            last_spawn_time = FPlatform.RealTime();
        }


        void update_slicing(bool immediate = false)
        {
            if (immediate == false && time_to_start_new_compute() == false)
                return;

            // ui thing
            CotangentUI.HideSliceLabel();

            BackgroundThreadData d = new BackgroundThreadData();
            synchronize_meshes(d);

            mark_spawn_time();
            spawn_new_compute(d);
        }

        void do_progress()
        {
            if (SlicingProgressEvent != null && active_compute != null) {
                if (active_compute.Finished && active_compute.Success == false) {
                    SlicingProgressEvent?.Invoke(new SlicingProgressStatus(false));
                } else {
                    int total = 1, progress = 0;
                    active_compute.SafeProgressQuery(ref total, ref progress);
                    SlicingProgressEvent?.Invoke(new SlicingProgressStatus(progress, total));
                }
            }
        }


        BackgroundThreadData active_compute;
        Thread active_compute_thread;
        object active_compute_lock = new object();

        bool is_computing()
        {
            return active_compute != null;
        }

        void cancel_active_compute()
        {
            lock (active_compute_lock) {
                if (active_compute != null) {
                    DebugUtil.Log("[GeometrySlicer] Cancelling!!");
                    active_compute_thread.Abort();
                    active_compute = null;
                    active_compute_thread = null;

                }
            }

            mark_spawn_time();   // otherwise we will start at regular intervals
        }

        void spawn_new_compute(BackgroundThreadData d)
        {
            cancel_active_compute();

            lock (active_compute_lock) {
                DebugUtil.Log("[GeometrySlicer] Spawning Compute!!");
                active_compute = d;
                active_compute_thread = new Thread(d.Compute);
                active_compute_thread.Start();
            }
        }


        bool process_completed_compute()
        {
            if (active_compute != null) {
                if (active_compute.Finished) {
                    lock (active_compute_lock) {

                        lock (data_lock) {
                            SlicerMeshes = active_compute.assembly;
                            SliceStackValid = true;
                            SliceSet = active_compute.result;
                        }

                        // this recomputes slice polylines, but it could be better...
                        if (ShowSlicePolylines) {
                            discard_slice_polylines();
                            compute_slice_polylines();
                        }

                        active_compute = null;
                        active_compute_thread = null;
                        return true;
                    }
                }
            }
            return false;
        }



        class BackgroundThreadData
        {
            // input data
            public Frame3f[] meshToScene;
            public Vector3f[] localScale;
            public DMesh3[] meshCopies;
            public PrintMeshSettings[] meshSettings;

            // computed data
            public bool Finished;
            public bool Success;
            public PrintMeshAssembly assembly;
            public PlanarSliceStack result;

            // internal
            MeshPlanarSlicerPro slicer;

            public BackgroundThreadData()
            {
                Finished = false;
                Success = false;
            }

            public void SafeProgressQuery(ref int total, ref int progress)
            {
                if ( slicer == null || slicer.TotalCompute == 0) {
                    total = 1;
                    progress = 0;
                    return;
                }
                total = slicer.TotalCompute;
                progress = slicer.Progress;
                return;
            }

            public void Compute()
            {
                int N = meshToScene.Length;

                slicer = new MeshPlanarSlicerPro() {
                    LayerHeightMM = CC.Settings.LayerHeightMM,
                    // [RMS] 1.5 here is a hack. If we don't leave a bit of space then often the filament gets squeezed right at
                    //   inside/outside transitions, which is bad. Need a better way to handle.
                    OpenPathDefaultWidthMM = CC.Settings.NozzleDiameterMM*1.5,
                    SetMinZValue = 0,
                    SliceFactoryF = PlanarSlicePro.FactoryF
                };
                if (CC.Settings.OpenMode == PrintSettings.OpenMeshMode.Clipped)
                    slicer.DefaultOpenPathMode = PrintMeshOptions.OpenPathsModes.Clipped;
                else if (CC.Settings.OpenMode == PrintSettings.OpenMeshMode.Embedded)
                    slicer.DefaultOpenPathMode = PrintMeshOptions.OpenPathsModes.Embedded;
                else if (CC.Settings.OpenMode == PrintSettings.OpenMeshMode.Ignored)
                    slicer.DefaultOpenPathMode = PrintMeshOptions.OpenPathsModes.Ignored;

                if ( CC.Settings.StartLayers > 0 ) {
                    int start_layers = CC.Settings.StartLayers;
                    double std_layer_height = CC.Settings.LayerHeightMM;
                    double start_layer_height = CC.Settings.StartLayerHeightMM;
                    slicer.LayerHeightF = (layer_i) => {
                        return (layer_i < start_layers) ? start_layer_height : std_layer_height;
                    };
                }

                try {

                    assembly = new PrintMeshAssembly();
                    for (int k = 0; k < N; ++k) {
                        DMesh3 mesh = meshCopies[k];
                        Frame3f mapF = meshToScene[k];
                        PrintMeshSettings settings = meshSettings[k];

                        PrintMeshOptions options = new PrintMeshOptions();
                        options.IsSupport = (settings.ObjectType == PrintMeshSettings.ObjectTypes.Support);
                        options.IsCavity = (settings.ObjectType == PrintMeshSettings.ObjectTypes.Cavity);
                        options.IsCropRegion = (settings.ObjectType == PrintMeshSettings.ObjectTypes.CropRegion);
                        options.IsOpen = false;
                        if (settings.OuterShellOnly)
                            options.IsOpen = true;
                        options.OpenPathMode = PrintMeshSettings.Convert(settings.OpenMeshMode);
                        options.Extended = new ExtendedPrintMeshOptions() {
                            ClearanceXY = settings.Clearance,
                            OffsetXY = settings.OffsetXY
                        };

                        Vector3f scale = localScale[k];
                        MeshTransforms.Scale(mesh, scale.x, scale.y, scale.z);
                        MeshTransforms.FromFrame(mesh, mapF);
                        MeshTransforms.FlipLeftRightCoordSystems(mesh);
                        MeshTransforms.ConvertYUpToZUp(mesh);

                        MeshAssembly decomposer = new MeshAssembly(mesh);
                        decomposer.HasNoVoids = settings.NoVoids;
                        decomposer.Decompose();

                        assembly.AddMeshes(decomposer.ClosedSolids, options);

                        PrintMeshOptions openOptions = options.Clone();
                        assembly.AddMeshes(decomposer.OpenMeshes, openOptions);
                    }

                    if ( slicer.Add(assembly) == false )
                        throw new Exception("error adding PrintMeshAssembly to Slicer!!");

                    // set clip box
                    Box2d clip_box = new Box2d(Vector2d.Zero, 
                        new Vector2d(CC.Settings.BedSizeXMM/2, CC.Settings.BedSizeYMM/2));
                    slicer.ValidRegions = new List<GeneralPolygon2d>() {
                        new GeneralPolygon2d(new Polygon2d(clip_box.ComputeVertices()))
                    };

                    result = slicer.Compute();
                    Success = true;

                } catch (Exception e) {
                    DebugUtil.Log("GeometrySlicer.Compute: exception: " + e.Message);
                    Success = false;
                }

                Finished = true;
            }
        }






        protected List<fPolylineGameObject> SlicePolylines = new List<fPolylineGameObject>();


        void set_slice_polylines_visible(bool bSet)
        {
            if (bSet == show_slice_polylines)
                return;

            if ( show_slice_polylines ) {
                discard_slice_polylines();
                show_slice_polylines = false;
                return;
            }

            compute_slice_polylines();
            show_slice_polylines = true;
        }


        void discard_slice_polylines()
        {
            foreach (var go in SlicePolylines) {
                go.Destroy();
            }
            SlicePolylines.Clear();
        }


        void compute_slice_polylines()
        {
            fMaterial mat1 = MaterialUtil.CreateFlatMaterialF(Colorf.Black);
            fMaterial mat2 = MaterialUtil.CreateFlatMaterialF(Colorf.BlueMetal);

            // [TODO] do we need to hold data_lock here? seems like no since main thread is blocked,
            //  then it would never be the case that we are setting SliceSet = null

            // create geometry
            int slice_i = 0;
            SlicePolylines = new List<fPolylineGameObject>();
            foreach (PlanarSlice slice in SliceSet.Slices) {
                //DebugUtil.Log(2, "Slice has {0} solids", slice.Solids.Count);
                Colorf slice_color = (slice_i % 2 == 0) ? Colorf.Black : Colorf.BlueMetal;
                fMaterial slice_mat = (slice_i % 2 == 0) ? mat1 : mat2;
                slice_i++;
                foreach (GeneralPolygon2d poly in slice.Solids) {
                    List<Vector3f> polyLine = new List<Vector3f>();
                    for (int pi = 0; pi <= poly.Outer.VertexCount; ++pi) {
                        int i = pi % poly.Outer.VertexCount;
                        Vector2d v2 = poly.Outer[i];
                        Vector2d n2 = poly.Outer.GetTangent(i).Perp;

                        Vector3d v3 = new Vector3d(v2.x, v2.y, slice.Z);
                        v3 = MeshTransforms.ConvertZUpToYUp(v3);
                        v3 = MeshTransforms.FlipLeftRightCoordSystems(v3);
                        Vector3d n3 = MeshTransforms.ConvertZUpToYUp(new Vector3d(n2.x, n2.y, 0));
                        n3 = MeshTransforms.FlipLeftRightCoordSystems(n3);
                        n3.Normalize();
                        v3 += 0.1f * n3;

                        polyLine.Add((Vector3f)v3);
                    }

                    //DebugUtil.Log(2, "Polyline has {0} vertiecs", polyLine.Count);
                    fPolylineGameObject go = GameObjectFactory.CreatePolylineGO(
                        "slice_outer", polyLine, slice_color, 0.1f, LineWidthType.World);
                    go.SetMaterial(slice_mat, true);
                    CC.ActiveScene.RootGameObject.AddChild(go, false);
                    SlicePolylines.Add(go);
                }
            }
        }




    }
}
