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




public class RepairOrientationToolSettings : BaseToolSettings<RepairOrientationTool>
{
    Toggle invertResult;
    Toggle replaceInput;


    protected override void register_parameters()
    {
        invertResult = base.RegisterToggle("InvertResultToggle", "invert_result");
        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");
    }

}

