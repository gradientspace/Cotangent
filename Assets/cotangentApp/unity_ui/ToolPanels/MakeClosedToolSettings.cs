#pragma warning disable 414
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


public class MakeClosedToolSettings : BaseToolSettings<GenerateClosedMeshTool>
{
    MappedDropDown closingMode;
    InputField simpleCellSize; GameObject simpleCellSizeRow;
    InputField gridCellCount;  GameObject gridCellCountRow;
    InputField gridCellSize; GameObject gridCellSizeRow;
    InputField meshCellCount; GameObject meshCellCountRow;
    InputField meshCellSize; GameObject meshCellSizeRow;
    InputField minComponentSize; GameObject minComponentSizeRow;
    InputField offsetDistance;
    InputField windingIso;
    Toggle replaceInput;

    Button basicButton;
    bool basic_mode = true;

    protected override void register_parameters()
    {
        closingMode = base.RegisterDropDown("ClosingTypeDropDown", "closing_type",
            new List<string>() { "Robust", "Precise", "Fast" },
            new List<int>() {
                (int)GenerateClosedMeshTool.ClosingTypes.WindingNumberGrid,
                (int)GenerateClosedMeshTool.ClosingTypes.WindingNumberAnalytic,
                (int)GenerateClosedMeshTool.ClosingTypes.LevelSet
            });

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

        offsetDistance = base.RegisterFloatInput("OffsetDistanceInput", "offset_distance", new Interval1d(-500,500));
        windingIso = base.RegisterFloatInput("WindingIsoInput", "winding_inflate", new Interval1d(0.01, 0.99));

        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");

        basicButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "BasicToggleButton", on_basic_expert_toggle);
        TabOrder.Add(basicButton.gameObject);
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
        offsetDistance.interactable = (Tool.ClosingType == GenerateClosedMeshTool.ClosingTypes.LevelSet);
        windingIso.interactable = (Tool.ClosingType != GenerateClosedMeshTool.ClosingTypes.LevelSet);
    }








}

