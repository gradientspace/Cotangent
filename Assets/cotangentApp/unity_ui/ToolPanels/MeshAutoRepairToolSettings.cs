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


public class MeshAutoRepairToolSettings : BaseToolSettings<MeshAutoRepairTool>
{
    Toggle invertResult;
    Toggle replaceInput;
    InputField erosionRounds;
    InputField minEdgeLength;
    MappedDropDown removeInsideMode;


    protected override void register_parameters()
    {
        removeInsideMode = base.RegisterDropDown("InsideModeDropDown", "remove_inside_mode",
            new List<string>() { "None", "Interior", "Hidden" },
            new List<int>() {
                (int)MeshAutoRepairTool.RemoveInsideModes.None,
                (int)MeshAutoRepairTool.RemoveInsideModes.Interior,
                (int)MeshAutoRepairTool.RemoveInsideModes.Occluded
            });

        minEdgeLength = base.RegisterFloatInput("MinEdgeLengthInput", "min_edge_length", new Interval1d(0.0001, 100));

        erosionRounds = base.RegisterIntInput("ErosionRoundsInput", "erosion_rounds", new Interval1i(0, 10000));

        invertResult = base.RegisterToggle("InvertResultToggle", "invert_result");

        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }



}

