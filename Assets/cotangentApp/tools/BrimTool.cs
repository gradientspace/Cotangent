using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using g3;
using f3;
using cotangent;

namespace gs
{
    public class BrimToolBuilder : IToolBuilder
    {
        public Action<BrimTool, DMeshSO> OnApplyF = null;
        public SOMaterial PreviewMaterial = null;

        public Action<BrimTool> BuildCustomizeF = null;

        public virtual bool IsSupported(ToolTargetType type, List<SceneObject> targets)
        {
            foreach ( var so in CC.Objects.PrintMeshes ) {
                if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Solid)
                    return true;
            }
            return false;
        }

        public virtual ITool Build(FScene scene, List<SceneObject> targets)
        {
            BrimTool tool = build_tool(scene);
            tool.OnApplyF = this.OnApplyF;
            tool.PreviewMaterial = this.PreviewMaterial;
            if (BuildCustomizeF != null)
                BuildCustomizeF(tool);
            return tool;
        }

        public virtual BrimTool build_tool(FScene scene)
        {
            return new BrimTool(scene);
        }
    }





    public class BrimTool : BaseToolCore, ITool
    {
        static readonly public string Identifier = "brim";

        virtual public string Name { get { return "Brim"; } }
        virtual public string TypeIdentifier { get { return Identifier; } }

        /// <summary>
        /// Called on Apply(). By default, does nothing, as the DMeshSO is already
        /// in scene as preview. Override to implement your own behavior
        /// </summary>
        public Action<BrimTool, DMeshSO> OnApplyF;

        /// <summary>
        /// This is the material set on the DMeshSO during the preview
        /// </summary>
        public SOMaterial PreviewMaterial;


        InputBehaviorSet behaviors;
        virtual public InputBehaviorSet InputBehaviors {
            get { return behaviors; }
            set { behaviors = value; }
        }

        ParameterSet parameters;
        public ParameterSet Parameters {
            get { return parameters; }
        }


        double path_width = 0.4;
        public double PathWidth {
            get { return path_width; }
            set { path_width = value; }
        }

        double layer_height = 0.2;
        public double LayerHeight {
            get { return layer_height; }
            set { layer_height = value; }
        }


        public BrimTool(FScene scene)
        {
            this.Scene = scene;

            // no behaviors..
            behaviors = new InputBehaviorSet();

            // disable transformations
            Scene.Context.TransformManager.PushOverrideGizmoType(TransformManager.NoGizmoType);

            // set up parameters
            parameters = new ParameterSet();
        }


        protected DMeshSO PreviewSO;
        protected List<SceneObject> inputSelection;

        List<GeneralPolygon2d> combined_solid = new List<GeneralPolygon2d>();
        List<GeneralPolygon2d> combined_support = new List<GeneralPolygon2d>();
        List<GeneralPolygon2d> combined_all = new List<GeneralPolygon2d>();
        List<GeneralPolygon2d> subtract = new List<GeneralPolygon2d>();

        public virtual void Setup()
        {
            // push history stream, so that we can do undo/redo internal to tool,
            // that will not end up in external history
            push_history_stream();

            if (OnApplyF == null)
                OnApplyF = this.add_so_to_scene;

            if (PreviewMaterial == null)
                PreviewMaterial = SOMaterial.CreateFlatShaded("tool_generated", Colorf.DimGrey);

            // clear selection here
            inputSelection = new List<SceneObject>(Scene.Selected);
            set_allow_selection_changes(true);
            Scene.ClearSelection();
            set_allow_selection_changes(false);


            cache_brim_polys();

            PreviewSO = new DMeshSO();
            PreviewSO.Create(new DMesh3(), PreviewMaterial);
            Scene.AddSceneObject(PreviewSO);
            PreviewSO.Name = "Generated Brim";

            initialize_parameters();
        }





        virtual protected void cache_brim_polys()
        {
            combined_solid = new List<GeneralPolygon2d>();
            combined_support = new List<GeneralPolygon2d>();
            subtract = new List<GeneralPolygon2d>();

            Frame3f cutPlane = new Frame3f((float)layer_height * 0.5f * Vector3f.AxisY, Vector3f.AxisY);

            foreach (var so in CC.Objects.PrintMeshes) {
                if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Ignored)
                    continue;
                SOSectionPlane section = new SOSectionPlane(so);
                section.UpdateSection(cutPlane, CoordSpace.SceneCoords);
                List<GeneralPolygon2d> solids = section.GetSolids();

                // [TODO] should subtract holes explicitly here, like we do in slicer?

                if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Cavity)
                    subtract = ClipperUtil.Union(solids, subtract);
                else if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Support)
                    combined_support = ClipperUtil.Union(solids, combined_support);
                else if (so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Solid)
                    combined_solid = ClipperUtil.Union(solids, combined_solid);
            }
            if (subtract.Count > 0)
                combined_solid = ClipperUtil.Difference(combined_solid, subtract);

            combined_all = ClipperUtil.Union(combined_solid, combined_support);
            combined_all = CurveUtils2.FilterDegenerate(combined_all, 0.001);

            foreach (var poly in combined_all) 
                poly.Simplify(path_width * 0.02);

        }





        virtual public void PreRender()
        {
            if (in_shutdown())
                return;

            if ( parameters_dirty ) {

                // offset
                List<GeneralPolygon2d> offset = ClipperUtil.RoundOffset(combined_all, offset_distance);
                // aggressively simplify after round offset...
                foreach (var poly in offset)
                    poly.Simplify(path_width);

                // subtract initial and add tiny gap so these don't get merged by slicer
                if (SubtractSolids) {
                    offset = ClipperUtil.Difference(offset, combined_solid);
                    offset = ClipperUtil.MiterOffset(offset, -path_width * 0.1);
                }

                offset = CurveUtils2.FilterDegenerate(offset, 0.001);
                foreach (var poly in offset)
                    poly.Simplify(path_width * 0.02);

                DMesh3 mesh = new DMesh3();
                MeshEditor editor = new MeshEditor(mesh);
                foreach (var poly in offset) {
                    TriangulatedPolygonGenerator polygen = new TriangulatedPolygonGenerator() { Polygon = poly };
                    editor.AppendMesh(polygen.Generate().MakeDMesh());
                }
                MeshTransforms.ConvertZUpToYUp(mesh);

                if (mesh.TriangleCount > 0) {
                    MeshExtrudeMesh extrude = new MeshExtrudeMesh(mesh);
                    extrude.ExtrudedPositionF = (v, n, vid) => {
                        return v + Layers*layer_height * Vector3d.AxisY;
                    };
                    extrude.Extrude();
                    MeshTransforms.Translate(mesh, -mesh.CachedBounds.Min.y * Vector3d.AxisY);
                }

                PreviewSO.ReplaceMesh(mesh, true);

                //Vector3d translate = scene_bounds.Point(1, -1, 1);
                //translate.x += spiral.Bounds.Width + PathWidth;
                //Frame3f sceneF = Frame3f.Identity.Translated((Vector3f)translate);
                //PreviewSO.SetLocalFrame(sceneF, CoordSpace.SceneCoords);

                parameters_dirty = false;
            }

        }







        virtual public void Shutdown()
        {
            begin_shutdown();

            pop_history_stream();

            if (PreviewSO != null) {
                Scene.RemoveSceneObject(PreviewSO, true);
                PreviewSO = null;
            }

            Scene.Context.TransformManager.PopOverrideGizmoType();
        }



        virtual public bool HasApply { get { return true; } }
        virtual public bool CanApply { get { return true; } }
        virtual public void Apply()
        {
            if (OnApplyF != null) {

                set_allow_selection_changes(true);

                // pop the history stream we pushed
                pop_history_stream();

                // restore input selection
                Scene.ClearSelection();
                foreach (var so in inputSelection)
                    Scene.Select(so, false);

                // apply
                OnApplyF(this, PreviewSO);

                set_allow_selection_changes(false);

                PreviewSO = null;
            }
        }





        int layers = 1;
        double offset_distance = 5;
        double spacing = 0.2;
        bool subtract_solids = true;
        bool parameters_dirty = true;


        public int Layers {
            get { return get_layers(); }
            set { set_layers(value); }
        }
        int get_layers() { return layers; }
        void set_layers(int value) { layers = Math.Max(1, value); parameters_dirty = true;  }

        public double OffsetDistance {
            get { return get_distance(); }
            set { set_distance(value); }
        }
        double get_distance() { return offset_distance; }
        void set_distance(double value) { offset_distance = Math.Max(path_width, value); parameters_dirty = true; }


        public bool SubtractSolids {
            get { return get_subtract_solids(); }
            set { set_subtract_solids(value); }
        }
        bool get_subtract_solids() { return subtract_solids; }
        void set_subtract_solids(bool value) { subtract_solids = value; parameters_dirty = true; }

        public double Spacing {
            get { return get_spacing(); }
            set { set_spacing(value); }
        }
        double get_spacing() { return spacing; }
        void set_spacing(double value) { spacing = value; parameters_dirty = true; }


        protected virtual void initialize_parameters()
        {
            parameters.Register("layers", get_layers, set_layers, 1, false);
            parameters.Register("offset_distance", get_distance, set_distance, 50.0, false);
            parameters.Register("subtract_solids", get_subtract_solids, set_subtract_solids, true, false);
            parameters.Register("spacing", get_spacing, set_spacing, 0.2, false);
            //parameters.Register("computing", get_is_computing, set_is_computing, false, false);
        }



        void add_so_to_scene(BrimTool tool, DMeshSO result)
        {
            // already added
        }

    }
}
