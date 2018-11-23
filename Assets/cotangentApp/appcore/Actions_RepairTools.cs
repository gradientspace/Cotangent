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
        public static void AppendReplaceInputParam(ParameterSet Parameters, bool bDefault = true) {
            BoolParameterData replaceParam = new BoolParameterData(bDefault);
            Parameters.Register("replace_input", replaceParam.getValue, replaceParam.setValue, false);
        }






        public static void OnApply_GenerateClosedMeshTool(GenerateClosedMeshTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, "Solid");
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }



        public static void OnApply_RemeshTool(RemeshTool tool, Dictionary<DMeshSO, DMeshSO> result)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_multi_so_tool_handler(result, replace_input);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }

        public static void OnApply_ReduceTool(ReduceTool tool, Dictionary<DMeshSO, DMeshSO> result)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_multi_so_tool_handler(result, replace_input);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }

        public static void OnApply_RemoveHiddenFacesTool(RemoveHiddenFacesTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }

        public static void OnApply_RepairOrientationTool(RepairOrientationTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }

        public static void OnApply_MeshAutoRepairTool(MeshAutoRepairTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, "Repaired");
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }

        public static void OnApply_WeldEdgesTool(WeldEdgesTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, "Welded");
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }

        public static void OnApply_ReprojectTool(ReprojectTool tool, Dictionary<DMeshSO, DMeshSO> result)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_multi_so_tool_handler(result, replace_input);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }




        public static void OnApply_CombineMeshesTool(CombineMeshesTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, "Combined");
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


        public static void OnApply_SeparateSolidsTool(SeparateSolidsTool tool, List<DMeshSO> previews)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            if (replace_input) {
                foreach (DMeshSO so in tool.Targets)
                    CCActions.RemovePrintMesh(so as PrintMeshSO);
            }

            foreach (var so in previews) {
                so.Name = UniqueNames.GetNext("SeparatedPart");
                convert_to_print_mesh(so);
            }
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }



        public static void OnApply_MeshEditorTool(MeshEditorTool tool, DMeshSO previewSO)
        {
            replace_single(tool.Target, previewSO);
            CC.ActiveScene.RemoveSceneObject(previewSO, true);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }

        public static void OnApply_FillHolesTool(FillHolesTool tool, DMeshSO previewSO)
        {
            replace_single(tool.Target, previewSO);
            CC.ActiveScene.RemoveSceneObject(previewSO, true);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


        public static void OnApply_PlaneCutTool(PlaneCutTool tool, Dictionary<DMeshSO,DMeshSO> result)
        {
            standard_multi_so_tool_handler(result, true);
            if ( tool.KeepBothSides ) {
                foreach (var pair in tool.OtherSideMeshes) {
                    PrintMeshSO newSO = emit_new_print_mesh(pair.Value, pair.Key);
                    newSO.Name = UniqueNames.GetNext(pair.Key.Name);
                }
            }
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }



        public static void OnApply_MeshShellTool(MeshShellTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, "Shell");
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


        public static void OnApply_MeshHollowTool(MeshHollowTool tool, DMeshSO previewSO)
        {
            bool replace_input = tool.Parameters.GetValueBool("replace_input");
            standard_mesh_tool_handler(tool.Targets.ToList(), previewSO, replace_input, "Hollowed");
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


        public static void OnApply_AddHoleTool(AddHoleTool tool)
        {
            if (tool.HoleType == AddHoleTool.HoleTypes.CavityObject) {
                PrintMeshSO printSO = convert_to_print_mesh(tool.GetOutputSO());
                printSO.Name = UniqueNames.GetNext("Hole");
                printSO.Settings.ObjectType = PrintMeshSettings.ObjectTypes.Cavity;
                printSO.AssignSOMaterial(CCMaterials.CavityMeshMaterial);
            } else {
                bool replace_input = tool.Parameters.GetValueBool("replace_input");
                DMeshSO previewSO = tool.GetOutputSO();
                previewSO.EnableSpatial = true;   // required because preview has no spatial DS
                standard_mesh_tool_handler(new List<DMeshSO>() { tool.TargetSO as DMeshSO }, previewSO, replace_input);
            }
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }



        /// <summary>
        /// - if not replace input, convert preview to print
        /// - if replace, if one target and one preview, keep target and replace w/ mesh from preview
        ///   - otherwise remove targets and convert preview to print
        /// </summary>
        static void standard_mesh_tool_handler(List<DMeshSO> targets, DMeshSO previewSO, bool replace_input, string newNameStr = null)
        {
            if (replace_input) {
                if (targets.Count == 1) {
                    replace_single(targets[0], previewSO);
                    CC.ActiveScene.RemoveSceneObject(previewSO, true);
                } else {
                    foreach (DMeshSO so in targets)
                        CCActions.RemovePrintMesh(so as PrintMeshSO);
                    if ( newNameStr != null )
                        previewSO.Name = UniqueNames.GetNext(newNameStr);
                    convert_to_print_mesh(previewSO);
                }
            } else {
                if (newNameStr != null) 
                    previewSO.Name = UniqueNames.GetNext(newNameStr);

                convert_to_print_mesh(previewSO);
            }
        }


        static void standard_multi_so_tool_handler(Dictionary<DMeshSO, DMeshSO> result, bool replace_input)
        {
            if (replace_input) {
                foreach (var pair in result) {
                    DMeshSO target = pair.Key;
                    DMeshSO preview = pair.Value;
                    replace_single(target, preview);
                    CC.ActiveScene.RemoveSceneObject(preview, true);
                }
            } else {
                foreach (var pair in result) {
                    convert_to_print_mesh(pair.Value);
                }
            }
        }


        static PrintMeshSO emit_new_print_mesh(DMesh3 mesh, DMeshSO fromParent)
        {
            PrintMeshSO newSO = new PrintMeshSO();
            newSO.Create(mesh, CCMaterials.PrintMeshMaterial);
            CCActions.AddNewPrintMesh(newSO);
            if (fromParent != null) {
                newSO.SetLocalScale(fromParent.GetLocalScale());
                newSO.SetLocalFrame(fromParent.GetLocalFrame(CoordSpace.SceneCoords), CoordSpace.SceneCoords);
            } else {
                throw new NotImplementedException("have not implemented this path yet...");
                // estimate frame??
            }

            return newSO;
        }



        /// <summary>
        /// convert preview SO to print mesh, discard preview
        /// TODO: be able to swap/transfer preview into print
        /// </summary>
        static PrintMeshSO convert_to_print_mesh(DMeshSO previewSO)
        {
            previewSO.EnableSpatial = true;
            PrintMeshSO printMeshSO = previewSO.DuplicateSubtype<PrintMeshSO>();
            printMeshSO.Name = previewSO.Name;
            printMeshSO.AssignSOMaterial(CCMaterials.PrintMeshMaterial);

            CC.ActiveScene.RemoveSceneObject(previewSO, true);

            CCActions.AddNewPrintMesh(printMeshSO);

            return printMeshSO;
        }


        /// <summary>
        /// transfer source mesh to target
        /// TODO: more efficiencies
        /// </summary>
        static void replace_single(DMeshSO target, DMeshSO source)
        {
            ReplaceEntireMeshChange replaceChange = new ReplaceEntireMeshChange(target,
                new DMesh3(target.Mesh), source.Mesh);
            CC.ActiveScene.History.PushChange(replaceChange, false);
        }

    }
}
