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


public class BlockSupportsToolSettings : BaseToolSettings<GenerateBlockSupportsTool>
{
    InputField overhangAngle;
    InputField gridCellCount;
    InputField gridCellSize;
    Toggle subtractInput;
    InputField subtractOffset;

    protected override void register_parameters()
    {
        overhangAngle = base.RegisterFloatInput("OverhangAngleInput", "overhang_angle", new Interval1d(0, 90));
        gridCellSize = base.RegisterFloatInput("GridCellSizeInput", "grid_cell_size", new Interval1d(0.001f, 500));
        gridCellCount = base.RegisterIntInput("GridCellCountInput", "grid_cell_count", new Interval1i(4,2048));
        subtractInput = base.RegisterToggle("SubtractInputToggle", "subtract_input");
        subtractOffset = base.RegisterFloatInput("OffsetDistanceInput", "subtract_offset", new Interval1d(-500, 500));
    }


    protected override void after_tool_values_update()
    {
        subtractOffset.interactable = subtractInput.isOn;
    }





}

