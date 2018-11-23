using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class RepairToolsPanelSetup : MonoBehaviour
{
    GameObject Panel;
    GameObject Canvas;
    Button healToolsButton;
    Button triToolsButton;
    Button modelToolsButton;
    Button objectsButton;

    const int HEAL_FLYOUT = 1;
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

        healToolsButton = UnityUIUtil.FindButtonAndAddClickHandler("HealButton", on_heal_clicked);
        triToolsButton = UnityUIUtil.FindButtonAndAddClickHandler("TriToolsButton", on_tritools_clicked);
        modelToolsButton = UnityUIUtil.FindButtonAndAddClickHandler("ModelToolsButton", on_modeltools_clicked);
        objectsButton = UnityUIUtil.FindButtonAndAddClickHandler("ObjectsButton", on_objects_clicked);

        standard_colors = healToolsButton.colors;
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

        healToolsButton.colors = standard_colors;
        triToolsButton.colors = standard_colors;
        modelToolsButton.colors = standard_colors;
        objectsButton.colors = standard_colors;
    }


    void on_heal_clicked()
    {
        bool was_active = (nActive == HEAL_FLYOUT);
        destroy_active();
        if (was_active)
            return;
        ActiveFlyout = build_heal_flyout();
        nActive = HEAL_FLYOUT;
        healToolsButton.colors = active_colors;
    }
    GameObject build_heal_flyout()
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolMenu"));
        popupGO.SetName("HealTools");

        CCUIBuilder.AddBasicStartToolButton(popupGO, "AutoRepair", MeshAutoRepairTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Solidify", GenerateClosedMeshTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Weld Edges", WeldEdgesTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Fix Normals", RepairOrientationTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Fill Holes", FillHolesTool.Identifier, single_selection_or_one_obj);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Remove Hidden", RemoveHiddenFacesTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Edit Mesh", MeshEditorTool.Identifier, single_selection_or_one_obj);

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
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Hollow", MeshHollowTool.Identifier);
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






    static bool any_so_selected() {
        return CC.ActiveScene.Selected.Count > 0;
    }
    static bool single_so_selected() {
        return CC.ActiveScene.Selected.Count == 1;
    }
    static bool single_selection_or_one_obj() {
        return (CC.ActiveScene.Selected.Count == 1) ||
            (CC.Objects.PrintMeshes.Count == 1);
    }
    static bool multi_selection_or_multi_obj() {
        return (CC.ActiveScene.Selected.Count > 1) ||
            (CC.ActiveScene.Selected.Count == 0 && CC.Objects.PrintMeshes.Count > 1);
    }
    static bool pair_selection() {
        return (CC.ActiveScene.Selected.Count == 2);
    }
}
