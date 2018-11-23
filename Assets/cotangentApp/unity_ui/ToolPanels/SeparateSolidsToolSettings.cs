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



public class SeparateSolidsToolSettings : BaseToolSettings<SeparateSolidsTool>
{
    Toggle groupNested;
    Toggle orientNested;
    Toggle replaceInput;

    protected override void register_parameters()
    {
        groupNested = base.RegisterToggle("GroupNestedToggle", "group_nested");
        orientNested = base.RegisterToggle("OrientNestedToggle", "orient_nested");
        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }

    protected override void after_tool_values_update()
    {
        orientNested.interactable = groupNested.isOn;
    }
}



