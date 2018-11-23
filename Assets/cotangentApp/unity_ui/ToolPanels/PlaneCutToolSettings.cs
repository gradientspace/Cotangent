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


public class PlaneCutToolSettings : BaseToolSettings<PlaneCutTool>
{
    Toggle keepBoth;
    Toggle otherSide;
    Toggle fillHoles;
    Toggle minimalFill;
    InputField fillEdgeLength;
    Button setPlaneButton;

    protected override void register_parameters()
    {
        fillHoles = base.RegisterToggle("FillHolesToggle", "fill_holes");
        otherSide = base.RegisterToggle("FlipSideToggle", "reverse_normal");
        keepBoth = base.RegisterToggle("KeepBothToggle", "keep_both");
        minimalFill = base.RegisterToggle("MinimalFillToggle", "minimal_fill");
        fillEdgeLength = base.RegisterFloatInput("FillEdgeLengthInput", "fill_edge_length", new Interval1d(0.01, 10000));

        setPlaneButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "SetPlaneFromClickButton", on_set_plane_clicked);
    }


    protected override void after_tool_values_update()
    {
        fillEdgeLength.interactable = ! minimalFill.isOn;
    }

    void on_set_plane_clicked()
    {
        Tool.BeginSetPlaneFromSingleClick();
    }

}

