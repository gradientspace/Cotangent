using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class ModelToolsPanelSetup : MonoBehaviour
{
    GameObject Panel;
    GameObject Canvas;
    Button triToolsButton;
    Button modelToolsButton;
    Button objectsButton;
    Button voxToolsButton;

    const int VOXTOOLS_FLYOUT = 1;
    const int TRITOOLS_FLYOUT = 2;
    const int MODELTOOLS_FLYOUT = 3;
    const int OBJECTS_FLYOUT = 4;

    GameObject ActiveFlyout = null;
    int nActive = -1;

    ColorBlock standard_colors;
    ColorBlock active_colors;

    // Use this for initialization
    public void Start()
    {
        Panel = this.gameObject;
        Canvas = Panel.transform.parent.gameObject;

        objectsButton = UnityUIUtil.FindButtonAndAddClickHandler("ObjectsButton", on_objects_clicked);
        triToolsButton = UnityUIUtil.FindButtonAndAddClickHandler("TriToolsButton", on_tritools_clicked);
        voxToolsButton = UnityUIUtil.FindButtonAndAddClickHandler("VoxToolsButton", on_voxtools_clicked);
        modelToolsButton = UnityUIUtil.FindButtonAndAddClickHandler("ModelToolsButton", on_modeltools_clicked);

        standard_colors = voxToolsButton.colors;
        active_colors = standard_colors; active_colors.normalColor = active_colors.pressedColor; active_colors.highlightedColor = active_colors.pressedColor;
    }


    void destroy_active()
    {
        if (ActiveFlyout != null) {
            ActiveFlyout.RemoveFromParent(false);
            ActiveFlyout.Destroy();
            ActiveFlyout = null;
            nActive = -1;
        }

        voxToolsButton.colors = standard_colors;
        triToolsButton.colors = standard_colors;
        modelToolsButton.colors = standard_colors;
        objectsButton.colors = standard_colors;
    }


    void on_voxtools_clicked()
    {
        bool was_active = (nActive == VOXTOOLS_FLYOUT);
        destroy_active();
        if (was_active)
            return;
        ActiveFlyout = build_voxtools_flyout();
        nActive = VOXTOOLS_FLYOUT;
        voxToolsButton.colors = active_colors;
    }
    GameObject build_voxtools_flyout()
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolMenu"));
        popupGO.SetName("VoxelTools");

        CCUIBuilder.AddBasicStartToolButton(popupGO, "Solidify", GenerateClosedMeshTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Shell", MeshShellTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "VoxWrap", MeshWrapTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "VoxBoolean", MeshVoxelBooleanTool.Identifier, multi_selection);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "VoxBlend", MeshVoxelBlendTool.Identifier, multi_selection);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Morphology", MeshMorphologyTool.Identifier);


        Canvas.AddChild(popupGO, false);
        UnityUIUtil.PositionRelative2D(popupGO, BoxPosition.TopLeft, Panel, BoxPosition.TopRight, Vector2f.Zero);

        return popupGO;
    }





    void on_tritools_clicked()
    {
        bool was_active = (nActive == TRITOOLS_FLYOUT);
        destroy_active();
        if (was_active)
            return;
        ActiveFlyout = build_tritools_flyout();
        nActive = TRITOOLS_FLYOUT;
        triToolsButton.colors = active_colors;
    }
    GameObject build_tritools_flyout()
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolMenu"));
        popupGO.SetName("PositionTools");

        CCUIBuilder.AddBasicStartToolButton(popupGO, "Remesh", RemeshTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Simplify", ReduceTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Map to Target", ReprojectTool.Identifier, pair_selection);

        Canvas.AddChild(popupGO, false);
        UnityUIUtil.PositionRelative2D(popupGO, BoxPosition.TopLeft, Panel, BoxPosition.TopRight, Vector2f.Zero);

        return popupGO;
    }





    void on_modeltools_clicked()
    {
        bool was_active = (nActive == MODELTOOLS_FLYOUT);
        destroy_active();
        if (was_active)
            return;
        ActiveFlyout = build_modeltools_flyout();
        nActive = MODELTOOLS_FLYOUT;
        modelToolsButton.colors = active_colors;
    }
    GameObject build_modeltools_flyout()
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolMenu"));
        popupGO.SetName("PrintUtilTools");

        CCUIBuilder.AddBasicStartToolButton(popupGO, "Plane Cut", PlaneCutTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Add Hole", AddHoleTool.Identifier, single_selection_or_one_obj);

        Canvas.AddChild(popupGO, false);
        UnityUIUtil.PositionRelative2D(popupGO, BoxPosition.TopLeft, Panel, BoxPosition.TopRight, Vector2f.Zero);

        return popupGO;
    }




    void on_objects_clicked()
    {
        bool was_active = (nActive == OBJECTS_FLYOUT);
        destroy_active();
        if (was_active)
            return;
        ActiveFlyout = build_objects_popup();
        nActive = OBJECTS_FLYOUT;
        objectsButton.colors = active_colors;
    }
    GameObject build_objects_popup()
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolMenu"));
        popupGO.SetName("ObjectsTools");

        CCUIBuilder.AddBasicStartToolButton(popupGO, "Add Shape", GenerateShapeTool.Identifier);
        CCUIBuilder.AddBasicToolButton(popupGO, "Resize", set_size_clicked);
        CCUIBuilder.AddBasicToolButton(popupGO, "Duplicate", on_duplicate_clicked, any_so_selected);
        CCUIBuilder.AddBasicToolButton(popupGO, "Delete", on_delete_clicked, any_so_selected);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Separate", SeparateSolidsTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Combine", CombineMeshesTool.Identifier, multi_selection_or_multi_obj);

        Canvas.AddChild(popupGO, false);
        UnityUIUtil.PositionRelative2D(popupGO, BoxPosition.TopLeft, Panel, BoxPosition.TopRight, Vector2f.Zero);

        return popupGO;
    }
    void on_delete_clicked()
    {
        CCActions.DeleteSelectedObjects(true);
    }
    void on_duplicate_clicked()
    {
        CCActions.DuplicateSelectedObjects(true);
    }
    void set_size_clicked()
    {
        CCActions.BeginTool(SetDimensionsTool.Identifier);
    }






    static bool any_so_selected()
    {
        return CC.ActiveScene.Selected.Count > 0;
    }
    static bool single_so_selected()
    {
        return CC.ActiveScene.Selected.Count == 1;
    }
    static bool single_selection_or_one_obj()
    {
        return (CC.ActiveScene.Selected.Count == 1) ||
            (CC.Objects.PrintMeshes.Count == 1);
    }
    static bool multi_selection_or_multi_obj()
    {
        return (CC.ActiveScene.Selected.Count > 1) ||
            (CC.ActiveScene.Selected.Count == 0 && CC.Objects.PrintMeshes.Count > 1);
    }
    static bool pair_selection()
    {
        return (CC.ActiveScene.Selected.Count == 2);
    }
    static bool multi_selection()
    {
        return (CC.ActiveScene.Selected.Count >= 2);
    }
}
