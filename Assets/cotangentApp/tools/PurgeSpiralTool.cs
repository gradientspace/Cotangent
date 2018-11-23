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
    public class PurgeSpiralToolBuilder : IToolBuilder
    {
        public Action<PurgeSpiralTool, DMeshSO> OnApplyF = null;
        public SOMaterial PreviewMaterial = null;

        public Action<PurgeSpiralTool> BuildCustomizeF = null;

        public virtual bool IsSupported(ToolTargetType type, List<SceneObject> targets)
        {
            return true;
        }

        public virtual ITool Build(FScene scene, List<SceneObject> targets)
        {
            PurgeSpiralTool tool = build_tool(scene);
            tool.OnApplyF = this.OnApplyF;
            tool.PreviewMaterial = this.PreviewMaterial;
            if (BuildCustomizeF != null)
                BuildCustomizeF(tool);
            return tool;
        }

        public virtual PurgeSpiralTool build_tool(FScene scene)
        {
            return new PurgeSpiralTool(scene);
        }
    }





    public class PurgeSpiralTool : BaseToolCore, ITool
    {
        static readonly public string Identifier = "purge_spiral";

        virtual public string Name { get { return "PurgeSpiral"; } }
        virtual public string TypeIdentifier { get { return Identifier; } }

        /// <summary>
        /// Called on Apply(). By default, does nothing, as the DMeshSO is already
        /// in scene as preview. Override to implement your own behavior
        /// </summary>
        public Action<PurgeSpiralTool, DMeshSO> OnApplyF;

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



        public PurgeSpiralTool(FScene scene)
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

        protected AxisAlignedBox3d scene_bounds;

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

            scene_bounds = AxisAlignedBox3d.Empty;
            foreach ( var so in CC.Objects.PrintMeshes ) {
                if ( so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Solid ||
                     so.Settings.ObjectType == PrintMeshSettings.ObjectTypes.Support ) {
                    var seq = SceneTransforms.ObjectToSceneXForm(so);
                    foreach (Vector3d p in so.Mesh.Vertices()) {
                        Vector3d ps = seq.TransformP(p);
                        if (ps.y >= 0)
                            scene_bounds.Contain(ps);
                    }
                }
            }
            scene_bounds.Min.y = 0;
            if (scene_bounds.Width <= 0)
                scene_bounds.Min.x = scene_bounds.Max.x = 0;
            if (scene_bounds.Depth <= 0)
                scene_bounds.Min.z = scene_bounds.Max.z = 0;
            Height = scene_bounds.Height;

            PreviewSO = new DMeshSO();
            PreviewSO.Create(new DMesh3(), PreviewMaterial);
            Scene.AddSceneObject(PreviewSO);
            PreviewSO.Name = "Purge Spiral";

            initialize_parameters();
        }



        virtual public void PreRender()
        {
            if (in_shutdown())
                return;

            if ( parameters_dirty ) {

                PolyLine2d spiral = PolyLine2d.MakeBoxSpiral(Vector2d.Zero, length, PathWidth + spacing);

                DMesh3 mesh = new DMesh3();

                List<int> bottom_verts = new List<int>();
                List<int> top_verts = new List<int>();
                for (int i = 0; i < spiral.VertexCount; ++i) {
                    Vector2d x = spiral[i];
                    Vector3d vb = new Vector3d(x.x, 0, x.y);
                    bottom_verts.Add(mesh.AppendVertex(vb));
                    top_verts.Add(mesh.AppendVertex(vb + Height * Vector3d.AxisY));
                }

                MeshEditor editor = new MeshEditor(mesh);
                editor.StitchSpan(bottom_verts, top_verts);

                PreviewSO.ReplaceMesh(mesh, true);

                Vector3d translate = scene_bounds.Point(1, -1, 1);
                translate.x += spiral.Bounds.Width + PathWidth;
                Frame3f sceneF = Frame3f.Identity.Translated((Vector3f)translate);
                PreviewSO.SetLocalFrame(sceneF, CoordSpace.SceneCoords);

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





        double length = 50;
        double height = 10;
        double spacing = 0.2;
        bool parameters_dirty = true;


        public double Height {
            get { return get_height(); }
            set { set_height(value); }
        }
        double get_height() { return height; }
        void set_height(double value) { height = value; parameters_dirty = true;  }

        public double Length {
            get { return get_length(); }
            set { set_length(value); }
        }
        double get_length() { return length; }
        void set_length(double value) { length = value; parameters_dirty = true; }

        public double Spacing {
            get { return get_spacing(); }
            set { set_spacing(value); }
        }
        double get_spacing() { return spacing; }
        void set_spacing(double value) { spacing = value; parameters_dirty = true; }


        protected virtual void initialize_parameters()
        {
            parameters.Register("height", get_height, set_height, 10.0, false);
            parameters.Register("length", get_length, set_length, 50.0, false);
            parameters.Register("spacing", get_spacing, set_spacing, 0.2, false);
            //parameters.Register("computing", get_is_computing, set_is_computing, false, false);
        }



        void add_so_to_scene(PurgeSpiralTool tool, DMeshSO result)
        {
            // already added
        }

    }
}
