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


public class CombineMeshesToolSettings : BaseToolSettings<CombineMeshesTool>
{
    Toggle sortNested;
    Toggle replaceInput;

    protected override void register_parameters()
    {
        sortNested = base.RegisterToggle("SortNestedToggle", "orient_nested");
        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }
}


