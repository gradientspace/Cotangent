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


public class RemeshToolSettings : BaseToolSettings<RemeshTool>
{
    InputField edgeLength;
    InputField smoothAlpha;
    InputField iterations;
    Toggle reproject;
    Toggle replaceInput;
    Toggle preserveCreases;
    InputField creaseAngle;

    Button smoothButton;
    Button collapseButton;
    Button splitButton;
    Button flipButton;

    MappedDropDown boundaryMode;

    protected override void register_parameters()
    {
        edgeLength = base.RegisterFloatInput("EdgeLengthInput", "edge_length", new Interval1d(0.01, 100), "F4");
        smoothAlpha = base.RegisterFloatInput("SmoothingInput", "smooth_alpha", new Interval1d(0, 1), "F4");

        iterations = base.RegisterIntInput("RoundsInput", "iterations", new Interval1i(1, 9999));

        boundaryMode = base.RegisterDropDown("BoundaryModeDropDown", "boundary_mode",
            new List<string>() { "Free", "Fixed", "Split" },
            new List<int>() {
                (int)RemeshTool.BoundaryModes.FreeBoundaries,
                (int)RemeshTool.BoundaryModes.FixedBoundaries,
                (int)RemeshTool.BoundaryModes.ConstrainedBoundaries
            });

        preserveCreases = base.RegisterToggle("PreserveCreasesToggle", "preserve_creases");
        creaseAngle = base.RegisterFloatInput("CreaseAngleInput", "crease_angle", new Interval1d(10,90), "F2");

        reproject = base.RegisterToggle("ReprojectToggle", "reproject");

        smoothButton = base.RegisterToggleButton("SmoothButton", "enable_smooth");
        collapseButton = base.RegisterToggleButton("CollapseButton", "enable_collapse");
        splitButton = base.RegisterToggleButton("SplitButton", "enable_split");
        flipButton = base.RegisterToggleButton("FlipButton", "enable_flip");

        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }





}

