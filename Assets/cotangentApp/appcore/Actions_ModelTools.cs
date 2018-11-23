using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;
using gs;

namespace cotangent
{
    public static partial class CCActions
    {


        public static void OnApply_MeshMorphologyTool(MeshMorphologyTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            string sName = tool.OpType.ToString();
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


        public static void OnApply_MeshWrapTool(MeshWrapTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, "Wrap");
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


        public static void OnApply_MeshVoxelBooleanTool(MeshVoxelBooleanTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            string sName = tool.OpType.ToString();
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, sName);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


        public static void OnApply_MeshVoxelBlendTool(MeshVoxelBlendTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, "Blend");
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


    }
}
