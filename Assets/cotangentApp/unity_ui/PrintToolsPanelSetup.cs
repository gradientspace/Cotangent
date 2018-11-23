using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class PrintToolsPanelSetup : MonoBehaviour
{
    GameObject Panel;
    GameObject Canvas;
    Button objectsButton;
    Button positionButton;
    Button printUtilButton;

    const int POSITION_FLYOUT = 1;
    const int OBJECTS_FLYOUT = 2;
    const int PRINT_UTIL_FLYOUT = 3;

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
        positionButton = UnityUIUtil.FindButtonAndAddClickHandler("PositionButton", on_position_clicked);
        printUtilButton = UnityUIUtil.FindButtonAndAddClickHandler("PrintUtilButton", on_print_util_clicked);

        standard_colors = objectsButton.colors;
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

        objectsButton.colors = standard_colors;
        positionButton.colors = standard_colors;
        printUtilButton.colors = standard_colors;
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






    void on_position_clicked()
	{
        bool was_active = (nActive == POSITION_FLYOUT);
        destroy_active();
        if (was_active)
            return;
        ActiveFlyout = build_position_popup();
        nActive = POSITION_FLYOUT;
        positionButton.colors = active_colors;
    }
    GameObject build_position_popup()
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolMenu"));
        popupGO.SetName("PositionTools");

        CCUIBuilder.AddBasicToolButton(popupGO, "On Bed", on_bed_clicked);
        CCUIBuilder.AddBasicToolButton(popupGO, "Center", center_clicked);
        CCUIBuilder.AddBasicToolButton(popupGO, "Swap Y/Z Up", swap_up_clicked);
        CCUIBuilder.AddBasicToolButton(popupGO, "Mirror", mirror_clicked);
        CCUIBuilder.AddBasicToolButton(popupGO, "Reset Pivot", reset_pivot_clicked);
        CCUIBuilder.AddBasicToolButton(popupGO, "Set Base", set_base_clicked, single_so_selected);

        Canvas.AddChild(popupGO, false);
        UnityUIUtil.PositionRelative2D(popupGO, BoxPosition.TopLeft, Panel, BoxPosition.TopRight, Vector2f.Zero);

        return popupGO;
    }
    void on_bed_clicked()
    {
        CCActions.MoveCurrentToPrintBed(true);
    }
    void center_clicked()
    {
        CCActions.CenterCurrent(true);
    }
    void swap_up_clicked()
    {
        CCActions.SwapCurrentUpDirections(true);
    }
    void mirror_clicked()
    {
        CCActions.Mirror(true);
    }
    void reset_pivot_clicked()
    {
        CCActions.ResetCurrentPivots(true);
    }
    void set_base_clicked()
    {
        CCActions.BeginTool(SurfacePointTool.Identifier);
    }








    void on_print_util_clicked()
    {
        bool was_active = (nActive == PRINT_UTIL_FLYOUT);
        destroy_active();
        if (was_active)
            return;
        ActiveFlyout = build_print_util_popup();
        nActive = PRINT_UTIL_FLYOUT;
        printUtilButton.colors = active_colors;
    }
    GameObject build_print_util_popup()
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolMenu"));
        popupGO.SetName("PrintUtilTools");

        CCUIBuilder.AddBasicStartToolButton(popupGO, "Brim", BrimTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Purge Spiral", PurgeSpiralTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Block Support", GenerateBlockSupportsTool.Identifier);
        CCUIBuilder.AddBasicStartToolButton(popupGO, "Tree Support", GenerateGraphSupportsTool.Identifier);

        Canvas.AddChild(popupGO, false);
        UnityUIUtil.PositionRelative2D(popupGO, BoxPosition.TopLeft, Panel, BoxPosition.TopRight, Vector2f.Zero);

        return popupGO;
    }





    static bool any_so_selected() {
        return CC.ActiveScene.Selected.Count > 0;
    }
    static bool single_so_selected() {
        return CC.ActiveScene.Selected.Count == 1;
    }
    static bool multi_selection_or_multi_obj() {
        return (CC.ActiveScene.Selected.Count > 1) ||
            (CC.ActiveScene.Selected.Count == 0 && CC.Objects.PrintMeshes.Count > 1);
    }
}
