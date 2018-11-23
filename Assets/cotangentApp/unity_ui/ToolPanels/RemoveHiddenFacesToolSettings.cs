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


public class RemoveHiddenFacesToolSettings : BaseToolSettings<RemoveHiddenFacesTool>
{
    MappedDropDown insideMode;
    Toggle allVertices;
    Toggle showRemoved;
    Toggle replaceInput;


    protected override void register_parameters()
    {
        insideMode = base.RegisterDropDown("InsideModeDropDown", "inside_mode",
            new List<string>() { "Interior", "Occluded" },
            new List<int>() {
                (int)RemoveHiddenFacesTool.CalculationMode.WindingNumber,
                (int)RemoveHiddenFacesTool.CalculationMode.OcclusionTest
            });

        allVertices = base.RegisterToggle("AllVerticesToggle", "all_hidden_vertices");
        showRemoved = base.RegisterToggle("ShowRemovedToggle", "show_removed");
        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }

}

