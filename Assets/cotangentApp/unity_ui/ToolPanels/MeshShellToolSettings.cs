using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;


public class MeshShellToolSettings : BaseToolSettings<MeshShellTool>
{
    MappedDropDown shellType;
    MappedDropDown directionType;
    InputField simpleCellSize; GameObject simpleCellSizeRow;
    InputField gridCellCount; GameObject gridCellCountRow;
    InputField gridCellSize; GameObject gridCellSizeRow;
    InputField meshCellCount; GameObject meshCellCountRow;
    InputField meshCellSize; GameObject meshCellSizeRow;
    InputField minComponentSize; GameObject minComponentSizeRow;
    //InputField offsetDistance;
    InputField shellThickness;
    Toggle showInputObjects;
    Toggle replaceInput;

    Button basicButton;
    bool basic_mode = true;


    protected override void register_parameters()
    {
        shellType = base.RegisterDropDown("ShellTypeDropDown", "shell_type",
            new List<string>() { "Voxel", "Extrusion" },
            new List<int>() {
                (int)MeshShellTool.ShellTypes.DistanceField,
                (int)MeshShellTool.ShellTypes.Extrusion
            });

        directionType = base.RegisterDropDown("ShellDirectionDropDown", "shell_direction",
            new List<string>() { "Inset", "Outset", "Symmetric" },
            new List<int>() {
                (int)MeshShellTool.ShellDirections.Inner,
                (int)MeshShellTool.ShellDirections.Outer,
                (int)MeshShellTool.ShellDirections.Symmetric,
            });

        shellThickness = base.RegisterFloatInput("ShellThicknessInput", "shell_thickness", new Interval1d(0.0001f, 9999f));

        simpleCellSize = base.RegisterFloatInput("SimpleCellSizeInput", "all_cell_size", new Interval1d(0.25f, 500.0f));
        simpleCellSizeRow = this.gameObject.FindChildByName("SimpleCellSizeParam", true);

        gridCellCount = base.RegisterIntInput("GridCellCountInput", "grid_cell_count", new Interval1i(4, 2048));
        gridCellCountRow = this.gameObject.FindChildByName("GridCellCountParam", true);
        gridCellSize = base.RegisterFloatInput("GridCellSizeInput", "grid_cell_size", new Interval1d(0.001f, 500.0f));
        gridCellSizeRow = this.gameObject.FindChildByName("GridCellSizeParam", true);

        meshCellCount = base.RegisterIntInput("MeshCellCountInput", "mesh_cell_count", new Interval1i(4, 2048));
        meshCellCountRow = this.gameObject.FindChildByName("MeshCellCountParam", true);
        meshCellSize = base.RegisterFloatInput("MeshCellSizeInput", "mesh_cell_size", new Interval1d(0.001f, 500.0f));
        meshCellSizeRow = this.gameObject.FindChildByName("MeshCellSizeParam", true);

        minComponentSize = base.RegisterFloatInput("MinComponentSizeInput", "min_component_size", new Interval1d(0.0f, 500.0f));
        minComponentSizeRow = this.gameObject.FindChildByName("MinComponentSizeParam", true);

        showInputObjects = base.RegisterToggle("ShowOriginalToggle", "show_original");
        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");

        basicButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "BasicToggleButton", on_basic_expert_toggle);
        TabOrder.Add(basicButton.gameObject);

        update_basic_expert_visibility();
    }


    void update_basic_expert_visibility()
    {
        if (basic_mode == true) {
            gridCellCountRow.SetVisible(false); gridCellSizeRow.SetVisible(false);
            meshCellCountRow.SetVisible(false); meshCellSizeRow.SetVisible(false);
            minComponentSizeRow.SetVisible(false);
            simpleCellSizeRow.SetVisible(true);
            UnityUIUtil.SetColors(basicButton, Colorf.White, Colorf.DimGrey);
        } else {
            simpleCellSizeRow.SetVisible(false);
            gridCellCountRow.SetVisible(true); gridCellSizeRow.SetVisible(true);
            meshCellCountRow.SetVisible(true); meshCellSizeRow.SetVisible(true);
            minComponentSizeRow.SetVisible(true);
            UnityUIUtil.SetColors(basicButton, Colorf.LightGreen, Colorf.DimGrey);
        }
    }


    void on_basic_expert_toggle()
    {
        basic_mode = !basic_mode;

        if (basic_mode == true) {
            double grid_val = ActiveParameterSet.GetValue<double>("grid_cell_size");
            ActiveParameterSet.SetValue<double>("all_cell_size", grid_val);
        }

        update_basic_expert_visibility();
        base.update_values_from_tool();
    }

    protected override void after_tool_values_update()
    {
        bool is_df = (Tool.ShellType == MeshShellTool.ShellTypes.DistanceField); ;
        simpleCellSize.interactable = is_df;
        gridCellCount.interactable = gridCellSize.interactable = is_df;
        meshCellCount.interactable = meshCellSize.interactable = is_df;
    }

}

