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
    public struct ToolpathProgressStatus
    {
        public int curProgress;
        public int maxProgress;
        public bool bFailed;
        public ToolpathProgressStatus(int cur, int max) {
            bFailed = false; curProgress = cur; maxProgress = max;
        }
        public static readonly ToolpathProgressStatus Failed = new ToolpathProgressStatus(1, 1) { bFailed = true };
    }


    // This action will be called every frame w/ (current_count, total_count) as arguments
    public delegate void ToolpathsProgressHandler(ToolpathProgressStatus status);


    public class ToolpathGenerator
    {
        public GCodeFile CurrentGCode;
        public ToolpathSet Toolpaths;
        public LayersDetector LayerInfo;
        public SingleMaterialFFFSettings Settings;

        public PlanarSliceStack Slices;

        public bool ToolpathsValid;
        public bool ToolpathsFailed;

        public bool ShowActualGCodePaths = false;

        public bool PauseToolpathing = false;


        public delegate void ToolpathsStateChangeHandler();
        public event ToolpathsStateChangeHandler ToolpathsInvalidatedEvent;
        public event ToolpathsStateChangeHandler ToolpathsUpdatedEvent;


        public event ToolpathsProgressHandler ToolpathsProgressEvent;


        public ToolpathGenerator()
        {
            InvalidateToolpaths();
        }




        public void Update()
        {
            if (CCActions.CurrentViewMode != AppViewMode.PrintView)
                return;
            if (PauseToolpathing)
                return;

            if ( ToolpathsFailed ) 
                return;

            if (ToolpathsValid == false) {
                if (is_computing() == false)
                    update_toolpaths();
                else
                    do_progress();
            }

            bool new_toolpaths = process_completed_compute();
            if (new_toolpaths) {
                DebugUtil.Log("[ToolpathGenerator] Toolpaths Available!");
                ToolpathsUpdatedEvent?.Invoke();
            }
        }



        public void InvalidateToolpaths()
        {
            cancel_active_compute();

            CurrentGCode = null;
            Toolpaths = null;
            LayerInfo = null;
            Settings = null;
            Slices = null;
            ToolpathsValid = false;
            ToolpathsFailed = false;

            // [TODO] do via event?
            CC.Objects.DiscardToolpaths();

            ToolpathsInvalidatedEvent?.Invoke();
        }





        float last_spawn_time = 0;

        bool time_to_start_new_compute()
        {
            return FPlatform.RealTime() - last_spawn_time > 0.5;
        }
        void mark_spawn_time()
        {
            last_spawn_time = FPlatform.RealTime();
        }


        void update_toolpaths(bool immediate = false)
        {
            if (immediate == false && time_to_start_new_compute() == false)
                return;

            // have to wait for valid slice stack
            PrintMeshAssembly meshes;
            PlanarSliceStack slices;
            bool valid = CC.Slicer.ExtractResultsIfValid(out meshes, out slices);
            if (valid == false)
                return;
            if ( slices == null ) {
                ToolpathsProgressEvent?.Invoke(ToolpathProgressStatus.Failed);
                return;
            }

            BackgroundThreadData d = new BackgroundThreadData();
            d.PrintSettings = CC.Settings.CloneCurrentSettings();
            d.Meshes = meshes;
            d.SliceSet = slices;
            d.InterpretGCodePaths = ShowActualGCodePaths;

            mark_spawn_time();
            spawn_new_compute(d);
        }

        void do_progress()
        {
            if (ToolpathsProgressEvent != null && active_compute != null) {
                if (active_compute.Finished && active_compute.Success == false) {
                    ToolpathsProgressEvent?.Invoke(ToolpathProgressStatus.Failed);
                } else {
                    int total = 1, progress = 0;
                    active_compute.SafeProgressQuery(ref total, ref progress);
                    ToolpathsProgressEvent?.Invoke(new ToolpathProgressStatus(progress, total));
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
                    DebugUtil.Log("[ToolpathGenerator] Cancelling!!");
                    active_compute.RequestCancel = true;
                    active_compute_thread.Abort();    // [TODO] ideally thread would cancel itself, instead of aborting...
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
                DebugUtil.Log("[ToolpathGenerator] Spawning Compute!!");
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

                        if ( active_compute.Success ) {
                            CurrentGCode = active_compute.gcode;
                            Toolpaths = active_compute.paths;
                            LayerInfo = active_compute.layerInfo;
                            Settings = active_compute.PrintSettings;
                            Slices = active_compute.SliceSet;
                            ToolpathsValid = true;
                            ToolpathsFailed = false;

                            ToolpathsProgressEvent?.Invoke(new ToolpathProgressStatus(1,1));

                            CC.Objects.SetToolpaths(this);

                        } else {
                            CurrentGCode = null;
                            Toolpaths = null;
                            LayerInfo = null;
                            Settings = null;
                            Slices = null;
                            ToolpathsValid = false;
                            ToolpathsFailed = true;

                            // notify of failure here?
                            ToolpathsProgressEvent?.Invoke(ToolpathProgressStatus.Failed);
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
            public SingleMaterialFFFSettings PrintSettings;
            public PrintMeshAssembly Meshes;
            public PlanarSliceStack SliceSet;
            public bool InterpretGCodePaths;

            // computed data
            public bool Finished;
            public bool Success;
            public bool RequestCancel;
            public GCodeFile gcode;
            public ToolpathSet paths;
            public LayersDetector layerInfo;

            // internal
            SingleMaterialFFFPrintGenerator printer;

            public BackgroundThreadData()
            {
                Finished = false;
                Success = false;
                RequestCancel = false;
            }

            public void SafeProgressQuery(ref int total, ref int progress)
            {
                if (printer == null ) {
                    total = 1;
                    progress = 0;
                    return;
                }
                int pcur, ptotal;
                printer.GetProgress(out pcur, out ptotal);
                total = ptotal;
                progress = pcur;
                return;
            }


            public void Compute()
            {
                RequestCancel = false;

                printer =
                    new SingleMaterialFFFPrintGenerator(Meshes, SliceSet, PrintSettings);

                if (PrintSettings.EnableSupportReleaseOpt) {
                    printer.LayerPostProcessor = new SupportConnectionPostProcessor() {
                        ZOffsetMM = PrintSettings.SupportReleaseGap
                    };
                }

                // if we aren't interpreting GCode, we want generator to return its path set
                printer.AccumulatePathSet = (InterpretGCodePaths == false);

                // set clip region
                Box2d clip_box = new Box2d(Vector2d.Zero,
                    new Vector2d(CC.Settings.BedSizeXMM / 2, CC.Settings.BedSizeYMM / 2));
                printer.PathClipRegions = new List<GeneralPolygon2d>() {
                    new GeneralPolygon2d(new Polygon2d(clip_box.ComputeVertices()))
                };

                printer.ErrorF = (msg, trace) => {
                    if (RequestCancel == false)
                        DebugUtil.Log(2, "Slicer Error! msg: {0} stack {1}", msg, trace);
                };

                DebugUtil.Log(2, "Generating gcode...");

                try {
                    if (printer.Generate() == false)
                        throw new Exception("generate failed");   // this will be caught below

                    gcode = printer.Result;

                    //DebugUtil.Log(2, "Interpreting gcode...");

                    if (InterpretGCodePaths) {
                        GCodeToToolpaths converter = new GCodeToToolpaths();
                        MakerbotInterpreter interpreter = new MakerbotInterpreter();
                        interpreter.AddListener(converter);
                        InterpretArgs interpArgs = new InterpretArgs();
                        interpreter.Interpret(gcode, interpArgs);
                        paths = converter.PathSet;
                    } else
                        paths = printer.AccumulatedPaths;

                    //DebugUtil.Log(2, "Detecting layers...");

                    layerInfo = new LayersDetector(paths);

                    Success = true;

                } catch (Exception e) {
                    DebugUtil.Log("ToolpathGenerator.Compute: exception: " + e.Message);
                    Success = false;
                }

                Finished = true;
            }
        }




    }
}
