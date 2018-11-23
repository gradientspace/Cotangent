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



public class WeldEdgesToolSettings : BaseToolSettings<WeldEdgesTool>
{
    InputField mergeTolerance;
    Toggle onlyUnique;
    Toggle replaceInput;

    protected override void register_parameters()
    {
        mergeTolerance = base.RegisterFloatInput("MergeToleranceInput", "merge_tolerance", new Interval1d(0, 10.0), "F8");
        onlyUnique = base.RegisterToggle("OnlyUniqueToggle", "only_unique_pairs");
        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }

}




