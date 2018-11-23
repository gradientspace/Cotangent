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


public class PurgeSpiralToolSettings : BaseToolSettings<PurgeSpiralTool>
{
    InputField height;
    InputField length;
    InputField spacing;

    protected override void register_parameters()
    {
        height = base.RegisterFloatInput("HeightInput", "height", new Interval1d(0, 10000));
        length = base.RegisterFloatInput("LengthInput", "length", new Interval1d(1, 10000));
        spacing = base.RegisterFloatInput("SpacingInput", "spacing", new Interval1d(0.0001, 10000));
    }
}

