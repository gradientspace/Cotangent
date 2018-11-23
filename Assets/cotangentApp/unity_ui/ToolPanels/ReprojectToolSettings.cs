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


public class ReprojectToolSettings : BaseToolSettings<ReprojectTool>
{
    MappedDropDown reprojectMode;

    InputField maxDistance;
    InputField transitionSmooth;
    InputField edgeLength;
    InputField smoothAlpha;
    InputField iterations;
    Toggle replaceInput;

    Button smoothButton;
    Button collapseButton;
    Button splitButton;
    Button flipButton;

    MappedDropDown boundaryMode;

    protected override void register_parameters()
    {
        reprojectMode = base.RegisterDropDown("ReprojectTypeDropDown", "project_mode",
            new List<string>() { "Smooth", "Sharp", "Bounded" },
            new List<int>() {
                (int)ReprojectTool.ReprojectModes.Smooth,
                (int)ReprojectTool.ReprojectModes.Sharp,
                (int)ReprojectTool.ReprojectModes.Bounded
            });


        maxDistance = base.RegisterFloatInput("MaxDistanceInput", "max_distance", new Interval1d(0.001, 1000), "F4");
        transitionSmooth = base.RegisterFloatInput("TransitionSmoothInput", "transition_smoothness", new Interval1d(0,1));
        edgeLength = base.RegisterFloatInput("EdgeLengthInput", "edge_length", new Interval1d(0.01, 100));
        smoothAlpha = base.RegisterFloatInput("SmoothingInput", "smooth_alpha", new Interval1d(0, 1), "F4");

        iterations = base.RegisterIntInput("RoundsInput", "iterations", new Interval1i(1, 9999));

        boundaryMode = base.RegisterDropDown("BoundaryModeDropDown", "boundary_mode",
            new List<string>() { "Free", "Fixed", "Split" },
            new List<int>() {
                (int)RemeshTool.BoundaryModes.FreeBoundaries,
                (int)RemeshTool.BoundaryModes.FixedBoundaries,
                (int)RemeshTool.BoundaryModes.ConstrainedBoundaries
            });


        smoothButton = base.RegisterToggleButton("SmoothButton", "enable_smooth");
        collapseButton = base.RegisterToggleButton("CollapseButton", "enable_collapse");
        splitButton = base.RegisterToggleButton("SplitButton", "enable_split");
        flipButton = base.RegisterToggleButton("FlipButton", "enable_flip");

        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }



    protected override void after_tool_values_update()
    {
        maxDistance.interactable = (Tool.ReprojectMode == ReprojectTool.ReprojectModes.Bounded);
        transitionSmooth.interactable = (Tool.ReprojectMode == ReprojectTool.ReprojectModes.Bounded);

        // this doesn't seem to actually work yet...
        boundaryMode.drop.interactable = (Tool.ReprojectMode != ReprojectTool.ReprojectModes.Bounded);
    }




}

