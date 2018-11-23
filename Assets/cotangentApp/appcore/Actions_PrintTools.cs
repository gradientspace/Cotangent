using System;
using System.Collections.Generic;
using System.Linq;
using g3;
using f3;
using gs;

namespace cotangent
{
    public static partial class CCActions
    {


        public static void OnApply_SetBasePoint(SurfacePointTool tool, SceneObject targetSO)
        {
            Frame3f frame = tool.SurfacePointSceneFrame;
            DMeshSO so = targetSO as DMeshSO;
            if (so == null)
                return;

            OrientationChanges.SetBasePoint(CC.ActiveScene, so, frame, false);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }




        public static void OnApply_SetDimensionsTool(SetDimensionsTool tool, List<DMeshSO> targets)
        {
            tool.PushFinalChanges();
            foreach ( var so in targets ) {
                BakeScalingChangeOp change = new BakeScalingChangeOp(so);
                CC.ActiveScene.History.PushChange(change, false);
            }

            // should this be in history?
            CCActions.UpdateViewClippingBounds();

            CC.ActiveScene.History.PushInteractionCheckpoint();
        }




        public static void OnApply_GenerateBlockSupportsTool(GenerateBlockSupportsTool tool, DMeshSO previewSO)
        {
            if (previewSO.Mesh.TriangleCount == 0)
                return;

            if (tool.Targets.Count() == CC.Objects.PrintMeshes.Count) {
                CC.Settings.GenerateSupport = false;
            }

            PrintMeshSO printSO = convert_to_print_mesh(previewSO);
            printSO.Name = UniqueNames.GetNext("Block Support");
            printSO.Settings.ObjectType = PrintMeshSettings.ObjectTypes.Support;
            printSO.AssignSOMaterial(CCMaterials.SupportMeshMaterial);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }


        public static void OnApply_GenerateGraphSupportsTool(GenerateGraphSupportsTool tool, DMeshSO previewSO)
        {
            if (previewSO.Mesh.TriangleCount == 0)
                return;

            if ( tool.Targets.Count() == CC.Objects.PrintMeshes.Count ) {
                CC.Settings.GenerateSupport = false;
            }

            PrintMeshSO printSO = convert_to_print_mesh(previewSO);
            printSO.Name = UniqueNames.GetNext("Tree Support");
            printSO.Settings.ObjectType = PrintMeshSettings.ObjectTypes.Support;
            printSO.AssignSOMaterial(CCMaterials.SupportMeshMaterial);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }









        public static void OnApply_GenerateShapeTool(GenerateShapeTool tool, DMeshSO previewSO)
        {
            if (previewSO.Mesh.TriangleCount == 0)
                return;
            PrintMeshSO printSO = convert_to_print_mesh(previewSO);
            printSO.Name = UniqueNames.GetNext(previewSO.Name);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }




        public static void OnApply_PurgeSpiralTool(PurgeSpiralTool tool, DMeshSO previewSO)
        {
            if (previewSO.Mesh.TriangleCount == 0)
                return;
            PrintMeshSO printSO = convert_to_print_mesh(previewSO);
            printSO.Name = UniqueNames.GetNext(previewSO.Name);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }

        public static void OnApply_BrimTool(BrimTool tool, DMeshSO previewSO)
        {
            if (previewSO.Mesh.TriangleCount == 0)
                return;
            PrintMeshSO printSO = convert_to_print_mesh(previewSO);
            printSO.Name = UniqueNames.GetNext(previewSO.Name);
            CC.ActiveScene.History.PushInteractionCheckpoint();
        }



    }
}