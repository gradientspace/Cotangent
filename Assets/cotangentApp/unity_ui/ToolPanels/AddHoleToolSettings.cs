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


public class AddHoleToolSettings : BaseToolSettings<AddHoleTool>
{
    MappedDropDown holeType;
    MappedDropDown directionType;
    InputField holeSize;
    InputField holeDepth;
    InputField subdivisions;
    Toggle throughHole;
    Toggle showPreview;
    Toggle replaceInput;


    protected override void register_parameters()
    {
        holeType = base.RegisterDropDown("HoleTypeDropDown", "hole_type",
            new List<string>() { "Cut Mesh", "Add Cavity" },
            new List<int>() {
                (int)AddHoleTool.HoleTypes.CutHole,
                (int)AddHoleTool.HoleTypes.CavityObject
            });

        directionType = base.RegisterDropDown("HoleDirectionDropDown", "hole_direction",
            new List<string>() { "Vertical", "Right/Left", "Back/Fwd", "Normal" },
            new List<int>() {
                (int)AddHoleTool.HoleDirections.AxisY,
                (int)AddHoleTool.HoleDirections.AxisX,
                (int)AddHoleTool.HoleDirections.AxisZ,
                (int)AddHoleTool.HoleDirections.Normal
            });

        holeSize = base.RegisterFloatInput("HoleSizeInput", "hole_size", new Interval1d(0.001f, 500.0f));
        throughHole = base.RegisterToggle("ThroughHoleToggle", "through_hole");
        holeDepth = base.RegisterFloatInput("HoleDepthInput", "hole_depth", new Interval1d(0.001f, 500.0f));
        subdivisions = base.RegisterIntInput("SubdivisionsInput", "subdivisions", new Interval1i(3, 1000));
        showPreview = base.RegisterToggle("ShowPreviewToggle", "show_preview");

        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }


    protected override void after_tool_values_update()
    {
        holeDepth.interactable = !throughHole.isOn;
        replaceInput.interactable = (Tool.HoleType == AddHoleTool.HoleTypes.CutHole);
    }

}

