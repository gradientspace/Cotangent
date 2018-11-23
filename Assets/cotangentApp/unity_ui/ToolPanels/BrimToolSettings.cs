#pragma warning disable 414
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#pragma warning disable 414
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;


public class BrimToolSettings : BaseToolSettings<BrimTool>
{
    InputField layers;
    InputField offsetDistance;
    Toggle subtractSolids;

    protected override void register_parameters()
    {
        layers = base.RegisterIntInput("LayersInput", "layers", new Interval1i(1, 1000));
        offsetDistance = base.RegisterFloatInput("OffsetDistanceInput", "offset_distance", new Interval1d(0, 10000));
        subtractSolids = base.RegisterToggle("SubtractSolidsToggle", "subtract_solids");
    }

}

