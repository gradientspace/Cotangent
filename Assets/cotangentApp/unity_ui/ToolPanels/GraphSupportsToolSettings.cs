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


public class GraphSupportsToolSettings : BaseToolSettings<GenerateGraphSupportsTool>
{
    InputField overhangAngle;
    InputField surfaceOffset;
    InputField supportMinAngle;
    InputField optimizeRounds;
    InputField gridCellCount;
    InputField gridCellSize;
    InputField postWidth;
    Toggle bottomUp;
    Toggle subtractInput;
    InputField subtractOffset;



    protected override void register_parameters()
    {
        overhangAngle = base.RegisterFloatInput("OverhangAngleInput", "overhang_angle", new Interval1d(0, 90));
        gridCellSize = base.RegisterFloatInput("GridCellSizeInput", "grid_cell_size", new Interval1d(0.001f, 500));
        gridCellCount = base.RegisterIntInput("GridCellCountInput", "grid_cell_count", new Interval1i(4, 2048));
        bottomUp = base.RegisterToggle("BottomUpToggle", "bottom_up");
        optimizeRounds = base.RegisterIntInput("OptimizeRoundsInput", "optimize_rounds", new Interval1i(0, 1000));
        surfaceOffset = base.RegisterFloatInput("SurfaceOffsetInput", "surface_offset", new Interval1d(-100,100));
        supportMinAngle = base.RegisterFloatInput("SupportMinAngleInput", "support_min_angle", new Interval1d(0,90));

        postWidth = base.RegisterFloatInput("PostWidthInput", "post_diameter", new Interval1d(0.1, 100));
        subtractInput = base.RegisterToggle("SubtractInputToggle", "subtract_input");
        subtractOffset = base.RegisterFloatInput("OffsetDistanceInput", "subtract_offset", new Interval1d(-500, 500));
    }



    protected override void after_tool_values_update()
    {
        supportMinAngle.interactable = ActiveParameterSet.GetValueInt("optimize_rounds") > 0;
        surfaceOffset.interactable = ActiveParameterSet.GetValueInt("optimize_rounds") > 0;

        subtractInput.interactable = false;
        subtractOffset.interactable = false;
    }





}

