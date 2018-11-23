using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using g3;
using f3;

namespace gs
{
    public class GenerateShapeToolBuilder : IToolBuilder
    {
        public Action<GenerateShapeTool, DMeshSO> OnApplyF = null;
        public SOMaterial PreviewMaterial = null;
        public SOMaterial ErrorMaterial = null;

        public Action<GenerateShapeTool> BuildCustomizeF = null;

        public virtual bool IsSupported(ToolTargetType type, List<SceneObject> targets)
        {
            return true;
        }

        public virtual ITool Build(FScene scene, List<SceneObject> targets)
        {
            GenerateShapeTool tool = build_tool(scene);
            tool.OnApplyF = this.OnApplyF;
            tool.PreviewMaterial = this.PreviewMaterial;
            tool.ErrorMaterial = this.ErrorMaterial;
            if (BuildCustomizeF != null)
                BuildCustomizeF(tool);
            return tool;
        }

        public virtual GenerateShapeTool build_tool(FScene scene)
        {
            return new GenerateShapeTool(scene);
        }
    }





    public class GenerateShapeTool : BaseToolCore, ITool
    {
        static readonly public string Identifier = "generate_shape";

        virtual public string Name { get { return "GenerateShape"; } }
        virtual public string TypeIdentifier { get { return Identifier; } }

        /// <summary>
        /// Called on Apply(). By default, does nothing, as the DMeshSO is already
        /// in scene as preview. Override to implement your own behavior
        /// </summary>
        public Action<GenerateShapeTool, DMeshSO> OnApplyF;

        /// <summary>
        /// This is the material set on the DMeshSO during the preview
        /// </summary>
        public SOMaterial PreviewMaterial;

        /// <summary>
        /// This is the material set on the DMeshSO during the preview if there is an error
        /// </summary>
        public SOMaterial ErrorMaterial;


        InputBehaviorSet behaviors;
        virtual public InputBehaviorSet InputBehaviors {
            get { return behaviors; }
            set { behaviors = value; }
        }

        ParameterSet parameters;
        public ParameterSet Parameters {
            get { return parameters; }
        }


        public GenerateShapeTool(FScene scene)
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

        public virtual void Setup()
        {
            // push history stream, so that we can do undo/redo internal to tool,
            // that will not end up in external history
            push_history_stream();

            if (OnApplyF == null)
                OnApplyF = this.add_so_to_scene;

            if (PreviewMaterial == null)
                PreviewMaterial = SOMaterial.CreateFlatShaded("tool_generated", Colorf.DimGrey);
            if (ErrorMaterial == null)
                ErrorMaterial = SOMaterial.CreateFlatShaded("tool_generated_error", Colorf.VideoRed);

            // clear selection here
            inputSelection = new List<SceneObject>(Scene.Selected);
            set_allow_selection_changes(true);
            Scene.ClearSelection();
            set_allow_selection_changes(false);

            PreviewSO = new DMeshSO();
            PreviewSO.Create(new DMesh3(), PreviewMaterial);
            Scene.AddSceneObject(PreviewSO);

            initialize_parameters();
        }



        virtual public void PreRender()
        {
            if (in_shutdown())
                return;

            if ( parameters_dirty ) {
                DMesh3 new_shape_mesh = null;
                switch (shape_type) {
                    case ShapeTypes.Box:
                        new_shape_mesh = make_shape_box(); break;
                    default:
                    case ShapeTypes.Sphere:
                        new_shape_mesh = make_shape_sphere(); break;
                    case ShapeTypes.Cylinder:
                        new_shape_mesh = make_shape_cylinder(); break;
                    case ShapeTypes.Arrow:
                        new_shape_mesh = make_shape_arrow(); break;
                    case ShapeTypes.Bunny:
                        new_shape_mesh = make_bunny(); break;
                }

                if (new_shape_mesh != null)
                    PreviewSO.ReplaceMesh(new_shape_mesh, true);

                PreviewSO.Name = shape_type.ToString();

                parameters_dirty = false;
            }

        }










        virtual protected DMesh3 make_shape_box()
        {
            Vector3d origin = new Vector3d(0, ShapeHeight / 2, 0);

            GridBox3Generator boxgen = new GridBox3Generator() {
                Box = new Box3d(origin, new Vector3d(shape_width/2, shape_height/2, shape_depth/2)),
                EdgeVertices = Subdivisions
            };
            return boxgen.Generate().MakeDMesh();
        }


        virtual protected DMesh3 make_shape_sphere()
        {
            Vector3d origin = new Vector3d(0, ShapeHeight / 2, 0);
            Sphere3Generator_NormalizedCube spheregen = new Sphere3Generator_NormalizedCube() {
                Box = new Box3d(origin, Vector3d.One),
                Radius = ShapeHeight/2,
                EdgeVertices = Subdivisions
            };
            return spheregen.Generate().MakeDMesh();
        }


        virtual protected DMesh3 make_shape_cylinder()
        {
            Vector3d origin = new Vector3d(0, ShapeHeight / 2, 0);
            CappedCylinderGenerator cylgen = new CappedCylinderGenerator() {
                BaseRadius = (float)ShapeWidth/2, TopRadius = (float)ShapeWidth /2,
                Height = (float)ShapeHeight,
                Slices = Math.Max(2,Subdivisions),
                Clockwise = true
            };
            return cylgen.Generate().MakeDMesh();
        }


        virtual protected DMesh3 make_shape_arrow()
        {
            float radius = (float)ShapeWidth / 2;
            float total_height = (float)ShapeHeight;
            float head_height = total_height * (0.4f);
            float stick_height = total_height * (0.6f);
            float tip_radius = radius * 0.01f;
            float stick_radius = radius * 0.5f;

            Vector3d origin = new Vector3d(0, ShapeHeight / 2, 0);
            Radial3DArrowGenerator arrowgen = new Radial3DArrowGenerator() {
                HeadBaseRadius = radius, TipRadius = tip_radius, HeadLength = head_height,
                StickRadius = stick_radius, StickLength = stick_height,
                Slices = Math.Max(2, Subdivisions),
                NoSharedVertices = false,
                Clockwise = true
            };
            DMesh3 mesh = arrowgen.Generate().MakeDMesh(); ;
            // fuuuuck
            MeshAutoRepair repair = new MeshAutoRepair(mesh);
            repair.Apply();
            return mesh;
        }


        DMesh3 cached_bunny = null;
        virtual protected DMesh3 make_bunny()
        {
            if (cached_bunny == null) {
                // [RMS] yiiiiiikes
                MemoryStream stream = FResources.LoadBinary("meshes/unit_height_bunny");
                if (stream != null) {
                    cached_bunny = StandardMeshReader.ReadMesh(stream, "obj");
                    MeshTransforms.ConvertZUpToYUp(cached_bunny);
                    MeshTransforms.FlipLeftRightCoordSystems(cached_bunny);
                } else {
                    cached_bunny = make_shape_sphere();
                    MeshTransforms.Scale(cached_bunny, 1 / ShapeHeight);
                }
            }
            DMesh3 mesh = new DMesh3(cached_bunny);
            MeshTransforms.Scale(mesh, ShapeHeight);
            return mesh;
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





        double shape_width = 50;
        double shape_height = 50;
        double shape_depth = 50;
        int subdivisions = 16;
        ShapeTypes shape_type = ShapeTypes.Bunny;
        bool parameters_dirty = true;


        public enum ShapeTypes
        {
            Box = 0,
            Sphere = 1,
            Cylinder = 2,
            Arrow = 3,
            Bunny = 4
        }
        public ShapeTypes ShapeType {
            get { return get_shape_type(); }
            set { set_shape_type(value); }
        }
        ShapeTypes get_shape_type() { return shape_type; }
        void set_shape_type(ShapeTypes value) { shape_type = value; parameters_dirty = true; }
        int get_shape_type_int() { return (int)get_shape_type(); }
        void set_shape_type_int(int value) { set_shape_type((ShapeTypes)value); }



        public double ShapeHeight {
            get { return get_shape_height(); }
            set { set_shape_height(value); }
        }
        double get_shape_height() { return shape_height; }
        void set_shape_height(double value) { shape_height = value; parameters_dirty = true;  }

        public double ShapeWidth {
            get { return get_shape_width(); }
            set { set_shape_width(value); }
        }
        double get_shape_width() { return shape_width; }
        void set_shape_width(double value) { shape_width = value; parameters_dirty = true; }

        public double ShapeDepth {
            get { return get_shape_depth(); }
            set { set_shape_depth(value); }
        }
        double get_shape_depth() { return shape_depth; }
        void set_shape_depth(double value) { shape_depth = value; parameters_dirty = true; }

        public int Subdivisions {
            get { return get_subdivisions(); }
            set { set_subdivisions(value); }
        }
        int get_subdivisions() { return subdivisions; }
        void set_subdivisions(int value) { subdivisions = MathUtil.Clamp(value,1,1000); parameters_dirty = true; }



        protected virtual void initialize_parameters()
        {
            Parameters.Register("shape_type", get_shape_type_int, set_shape_type_int, (int)ShapeTypes.Box, false)
                .SetValidRange(0, (int)ShapeTypes.Bunny);

            parameters.Register("shape_height", get_shape_height, set_shape_height, 10.0, false);
            parameters.Register("shape_width", get_shape_width, set_shape_width, 10.0, false);
            parameters.Register("shape_depth", get_shape_depth, set_shape_depth, 10.0, false);
            parameters.Register("subdivisions", get_subdivisions, set_subdivisions, 16, false);
            //parameters.Register("computing", get_is_computing, set_is_computing, false, false);
        }



        void add_so_to_scene(GenerateShapeTool tool, DMeshSO result)
        {
            // already added
        }

    }
}
