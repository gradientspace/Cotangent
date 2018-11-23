using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using f3;
using g3;
using gs;

namespace cotangent
{
    public class ToolpathSO : BaseSO
    {
        fGameObject parentGO;


        fGameObjectPool<fPolylineGameObject> PolylinePool;

        List<fPolylineGameObject> Polylines = new List<fPolylineGameObject>();
        bool polylines_valid;


        ToolpathSet Toolpaths;
        SingleMaterialFFFSettings Settings;

        PlanarSliceStack Slices;


        public ToolpathSO()
        {
        }

        static readonly Vector3f[] EmptyToolpathCurve = new Vector3f[2] { Vector3f.Zero, Vector3f.Zero };

        public virtual ToolpathSO Create(ToolpathSet toolpaths, SingleMaterialFFFSettings settings, SOMaterial setMaterial)
        {
            AssignSOMaterial(setMaterial);       // need to do this to setup BaseSO material stack
            parentGO = GameObjectFactory.CreateParentGO(UniqueNames.GetNext("Toolpath"));

            Toolpaths = toolpaths;
            Settings = settings;

            polylines_valid = false;

            PolylinePool = new fGameObjectPool<fPolylineGameObject>(allocate_polyline_go);
            PolylinePool.FreeF = (go) => {
                //go.SetVertices(EmptyToolpathCurve, false, true);
            };

            return this;
        }


        public override void Disconnect(bool bDestroying)
        {
            invalidate_polylines();
            PolylinePool.Destroy();
        }



        public void SetSlices(PlanarSliceStack slices) {
            Slices = slices;
        }

        public PlanarSliceStack GetSlices() {
            return Slices;
        }
        public ToolpathSet GetToolpaths() {
            return Toolpaths;
        }
        public SingleMaterialFFFSettings GetSettings() {
            return Settings;
        }


        public override fGameObject RootGameObject {
            get { return parentGO; }
        }
        override public string Name {
            get { return parentGO.GetName(); }
            set { parentGO.SetName(value); }
        }
        public override SOType Type {
            get { return CotangentTypes.Toolpath; }
        }

        public override SceneObject Duplicate()
        {
            throw new NotImplementedException();
        }



        // override some parameters
        override public bool IsTemporary { get { return true; } }
        override public bool IsSurface { get { return false; } }
        override public bool IsSelectable { get { return false; } }

        override public bool FindRayIntersection(Ray3f ray, out SORayHit hit)
        {
            hit = null;
            return false;
        }



        public Interval1d ZInterval {
            get { return z_interval; }
            set { z_interval = value; invalidate_polylines(); }
        }
        Interval1d z_interval = Interval1d.Infinite;


        /// <summary>
        /// TODO: this should be in settings, not something we have to compute...
        /// </summary>
        public Vector3d BedOrigin {
            get {
                double bed_width = Settings.Machine.BedSizeXMM;
                double bed_height = Settings.Machine.BedSizeYMM;
                // [RMS] if we are loading GCodeFile we need to hack the origin,
                // but if we are using export pathset, we just use zero
                Vector3d origin = Vector3d.Zero;
                //Vector3d origin = new Vector3d(-bed_width / 2, -bed_height / 2, 0);
                //if (Settings is gs.info.MakerbotSettings)
                //    origin = Vector3d.Zero;
                return origin;
            }
        }


        public string GetToolpathStats()
        {
            if (Toolpaths == null)
                return "unknown";
            else
                return "stats here";
        }


        public override void PreRender()
        {
            // nothing
            if (polylines_valid == false) {
                compute_toolpath_polylines();
                polylines_valid = true;
            }
        }


        void invalidate_polylines()
        {
            foreach (var go in Polylines) {
                base.RemoveGO(go);
            }
            Polylines.Clear();
            PolylinePool.FreeAll();

            polylines_valid = false;
        }



        void compute_toolpath_polylines()
        {
            Func<Vector3d, byte> LayerFilterF = (v) => { return 255; };

            Vector3d origin = BedOrigin;
            Interval1d zrange = ZInterval;

            double width = Settings.Machine.NozzleDiamMM;
            float extrude_width = (float)width / 2;
            float travel_width = (float)width / 3;

            Polylines.Clear();      // should already be empty, no?

            Action<LinearToolpath3<PrintVertex>> drawPath3F = (polyPath) => {
                Vector3d v0 = polyPath.Start.Position;

                // LAYER FILTER
                if (zrange.Contains(v0.z) == false)
                    return;

                byte layer_alpha = LayerFilterF(v0);
                if (layer_alpha == 0)
                    return;
                bool is_below = (layer_alpha < 255);

                fMaterial mat = CCMaterials.PathMaterial_Default;
                float w = 0.1f;
                if (polyPath.Type == ToolpathTypes.Deposition) {
                    if ((polyPath.TypeModifiers & FillTypeFlags.SupportMaterial) != 0) {
                        mat = CCMaterials.PathMaterial_Support;
                    } else {
                        mat = CCMaterials.PathMaterial_Extrude;
                    }
                    w = extrude_width;
                } else if (polyPath.Type == ToolpathTypes.Travel) {
                    mat = CCMaterials.PathMaterial_Travel;
                    w = travel_width;
                } else if (polyPath.Type == ToolpathTypes.PlaneChange) {
                    mat = CCMaterials.PathMaterial_PlaneChange;
                    w = travel_width;
                } else {
                    //
                }
                // todo layer alpha...
                //paint.Color = SkiaUtil.Color(paint.Color, layer_alpha);
                if (is_below)
                    w *= 6;
                fPolylineGameObject path = make_path(polyPath, mat, w, origin);

                Polylines.Add(path);
            };

            process_linear_paths(Toolpaths, drawPath3F);

            foreach (fPolylineGameObject go in Polylines) {
                AppendNewGO(go, parentGO, false);
                go.SetLayer(FPlatform.WidgetOverlayLayer);
            }
        }


        List<Vector3f> tempPolyLine = new List<Vector3f>();

        fPolylineGameObject make_path<T>(LinearToolpath3<T> path, fMaterial material, float width, Vector3d origin) where T : IToolpathVertex
        {
            Vector3d prev = Vector3d.Zero;
            tempPolyLine.Clear();
            foreach (T vtx in path) {
                Vector3d v = origin + vtx.Position;
                v = MeshTransforms.ConvertZUpToYUp(v);
                v = MeshTransforms.FlipLeftRightCoordSystems(v);

                // [RMS] because of the sharp turns we make, unity polyline will get twisted up unless we put
                // in some duplicate vertices =\
                if (tempPolyLine.Count > 0) {
                    tempPolyLine.Add((Vector3f)Vector3d.Lerp(prev, v, 0.001));
                    tempPolyLine.Add((Vector3f)Vector3d.Lerp(prev, v, 0.998));
                    tempPolyLine.Add((Vector3f)Vector3d.Lerp(prev, v, 0.999));
                }
                tempPolyLine.Add((Vector3f)v);
                prev = v;
            }

            fPolylineGameObject go = PolylinePool.Allocate();
            go.SetMaterial(material, true);
            go.SetLineWidth(width);
            go.SetVertices(tempPolyLine.ToArray(), false, true);
            return go;
        }


        void process_linear_paths(ToolpathSet ToolpathSetIn, Action<LinearToolpath3<PrintVertex>> processF)
        {
            Action<IToolpath> drawPath = (path) => {
                if (path is LinearToolpath3<PrintVertex>)
                    processF(path as LinearToolpath3<PrintVertex>);
                // else we might have other path type...
            };
            Action<IToolpathSet> drawPaths = null;
            drawPaths = (pathList) => {
                foreach (IToolpath path in pathList) {
                    if (path is IToolpathSet)
                        drawPaths(path as IToolpathSet);
                    else
                        drawPath(path);
                }
            };

            drawPaths(ToolpathSetIn);
        }



        fPolylineGameObject allocate_polyline_go()
        {
            fPolylineGameObject go = GameObjectFactory.CreatePolylineGO(
                "gcode_path", new List<Vector3f>(), CCMaterials.PathMaterial_Default, true, 1.0f, LineWidthType.World);
            go.SetCornerQuality(fCurveGameObject.CornerQuality.Minimal);
            MaterialUtil.DisableShadows(go);
            return go;
        }


    }




}
