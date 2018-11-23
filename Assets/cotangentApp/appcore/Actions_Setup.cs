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

        public static void InitializeCotangentScene(FContext context)
        {
            CotangentTypes.RegisterCotangentTypes(context.Scene.TypeRegistry);
        }




        public static void InitializePrintTools(FContext context)
        {
            context.ToolManager.RegisterToolType(GenerateShapeTool.Identifier, new GenerateShapeToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_GenerateShapeTool
            });

            context.ToolManager.RegisterToolType(SurfacePointTool.Identifier, new SurfacePointToolBuilder() {
                OnApplyF = CCActions.OnApply_SetBasePoint
            });

            context.ToolManager.RegisterToolType(SetDimensionsTool.Identifier, new SetDimensionsToolBuilder() {
                OnApplyF = CCActions.OnApply_SetDimensionsTool
            });

            context.ToolManager.RegisterToolType(PurgeSpiralTool.Identifier, new PurgeSpiralToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_PurgeSpiralTool,
                BuildCustomizeF = (tool) => { tool.PathWidth = CC.Settings.NozzleDiameterMM; }
            });

            context.ToolManager.RegisterToolType(BrimTool.Identifier, new BrimToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_BrimTool,
                BuildCustomizeF = (tool) => { tool.PathWidth = CC.Settings.NozzleDiameterMM; tool.LayerHeight = CC.Settings.LayerHeightMM; }
            });


            context.ToolManager.RegisterToolType(GenerateBlockSupportsTool.Identifier, new GenerateBlockSupportsToolBuilder() {
                PreviewMaterial = CCMaterials.SupportMeshMaterial,
                OnApplyF = CCActions.OnApply_GenerateBlockSupportsTool
            });
            context.ToolManager.RegisterToolType(GenerateGraphSupportsTool.Identifier, new GenerateGraphSupportsToolBuilder() {
                PreviewMaterial = CCMaterials.SupportMeshMaterial,
                OnApplyF = CCActions.OnApply_GenerateGraphSupportsTool
            });
        }



        public static void InitializeRepairTools(FContext context)
        {
            context.ToolManager.RegisterToolType(GenerateClosedMeshTool.Identifier, new GenerateClosedMeshToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_GenerateClosedMeshTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(WeldEdgesTool.Identifier, new WeldEdgesToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_WeldEdgesTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(RemoveHiddenFacesTool.Identifier, new RemoveHiddenFacesToolBuilder() {
                HiddenPreviewMaterial = CCMaterials.DeleteMeshTransparentMaterial,
                PreviewMaterial = CCMaterials.PrintMeshTransparentMaterial,
                OnApplyF = CCActions.OnApply_RemoveHiddenFacesTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(SeparateSolidsTool.Identifier, new SeparateSolidsToolBuilder() {
                HiddenPreviewMaterial = CCMaterials.PreviewTransparentMaterial,
                KeepPreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_SeparateSolidsTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(CombineMeshesTool.Identifier, new CombineMeshesToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_CombineMeshesTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });


            context.ToolManager.RegisterToolType(RepairOrientationTool.Identifier, new RepairOrientationToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_RepairOrientationTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(MeshAutoRepairTool.Identifier, new MeshAutoRepairToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_MeshAutoRepairTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(RemeshTool.Identifier, new RemeshToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_RemeshTool,
                BuildCustomizeF = (remeshTool) => { AppendReplaceInputParam(remeshTool.Parameters); }
            });

            context.ToolManager.RegisterToolType(ReduceTool.Identifier, new ReduceToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_ReduceTool,
                BuildCustomizeF = (reduceTool) => { AppendReplaceInputParam(reduceTool.Parameters); }
            });

            context.ToolManager.RegisterToolType(ReprojectTool.Identifier, new ReprojectToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_ReprojectTool,
                BuildCustomizeF = (reprojTool) => { AppendReplaceInputParam(reprojTool.Parameters); }
            });

            context.ToolManager.RegisterToolType(MeshEditorTool.Identifier, new MeshEditorToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                TypeFilterF = (so) => { return so is PrintMeshSO; },
                OnApplyF = CCActions.OnApply_MeshEditorTool 
            });

            context.ToolManager.RegisterToolType(FillHolesTool.Identifier, new FillHolesToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                TypeFilterF = (so) => { return so is PrintMeshSO; },
                OnApplyF = CCActions.OnApply_FillHolesTool,
                BuildCustomizeF = (tool) => { tool.VerboseOutput = CCPreferences.VerboseToolOutput; }
            });

            context.ToolManager.RegisterToolType(PlaneCutTool.Identifier, new PlaneCutToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_PlaneCutTool,
                BuildCustomizeF = (tool) => { tool.VerboseOutput = CCPreferences.VerboseToolOutput; }
            });

            context.ToolManager.RegisterToolType(MeshShellTool.Identifier, new MeshShellToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OuterPreviewMaterial = CCMaterials.PrintMeshTransparentMaterial,
                OnApplyF = CCActions.OnApply_MeshShellTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(MeshHollowTool.Identifier, new MeshHollowToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshTransparentMaterial,
                OuterPreviewMaterial = CCMaterials.PrintMeshTransparentMaterial,
                OnApplyF = CCActions.OnApply_MeshHollowTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });



            context.ToolManager.RegisterToolType(AddHoleTool.Identifier, new AddHoleToolBuilder() {
                HolePreviewMaterial = CCMaterials.PreviewMeshTransparentMaterial,
                CutPreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_AddHoleTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); tool.VerboseOutput = CCPreferences.VerboseToolOutput; }
            });

        }





        public static void InitializeModelTools(FContext context)
        {

            context.ToolManager.RegisterToolType(MeshMorphologyTool.Identifier, new MeshMorphologyToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_MeshMorphologyTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(MeshWrapTool.Identifier, new MeshWrapToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_MeshWrapTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });


            context.ToolManager.RegisterToolType(MeshVoxelBooleanTool.Identifier, new MeshVoxelBooleanToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_MeshVoxelBooleanTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

            context.ToolManager.RegisterToolType(MeshVoxelBlendTool.Identifier, new MeshVoxelBlendToolBuilder() {
                PreviewMaterial = CCMaterials.PreviewMeshMaterial,
                OnApplyF = CCActions.OnApply_MeshVoxelBlendTool,
                BuildCustomizeF = (tool) => { AppendReplaceInputParam(tool.Parameters); }
            });

        }




        public static void ShutdownApp()
        {
            CCActions.CancelCurrentTool();

            CC.FileMonitor.Shutdown();          // will kill itself
            CC.MeshAnalysis.Shutdown();
            CC.GCodeAnalysis.Shutdown();

            CC.Slicer.PauseSlicing = true;
            CC.Slicer.InvalidateSlicing();      // should terminate background thread

            CC.Toolpather.PauseToolpathing = true;
            CC.Toolpather.InvalidateToolpaths();      // should terminate background thread
        }






    }
}
