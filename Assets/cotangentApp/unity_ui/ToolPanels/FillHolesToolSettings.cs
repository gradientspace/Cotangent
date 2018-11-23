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

public class FillHolesToolSettings : BaseToolSettings<FillHolesTool>
{
    MappedDropDown closingMode;
    Toggle showHidden;

    GameObject minimalPanel;
    GameObject smoothPanel;
    Button fillAllButton;

    Toggle minOptimizeTris;
    InputField minOptTrisTolerance;

    Toggle smoothAutoTargetEdgeLen;
    InputField smoothTargetEdgeLen;
    InputField smoothOptRounds;

    Text statusText;

    protected override void register_parameters()
    {
        closingMode = base.RegisterDropDown("FillTypeDropDown", "fill_type",
            new List<string>() { "Trivial", "Minimal", "Smooth" },
            new List<int>() {
                (int)FillHolesTool.FillTypes.Trivial,
                (int)FillHolesTool.FillTypes.Minimal,
                (int)FillHolesTool.FillTypes.Smooth
            });

        showHidden = base.RegisterToggle("ShowHiddenToggle", "show_hidden");

        minimalPanel = this.gameObject.FindChildByName("MinimalPanel", true);
        smoothPanel = this.gameObject.FindChildByName("SmoothPanel", true);
        fillAllButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "FillAllButton", on_fill_all_clicked);

        minOptimizeTris = base.RegisterToggle("MinimalOptimizeTrisToggle", "optimize_tris");
        minOptTrisTolerance = base.RegisterFloatInput("MinimalOptTrisTolInput", "optimize_tris_deviation_thresh", new Interval1d(0, 1));

        smoothAutoTargetEdgeLen = base.RegisterToggle("AutoEdgeLengthToggle", "auto_edge_length");
        smoothTargetEdgeLen = base.RegisterFloatInput("TargetEdgeLenInput", "edge_length", new Interval1d(0.01, 100));
        smoothOptRounds = base.RegisterIntInput("OptimizationRoundsInput", "smooth_opt_rounds", new Interval1i(1, 999));

        statusText = UnityUIUtil.FindTextAndSet(this.gameObject, "StatusText", "");
    }



    int frame_count = 0;
    private void LateUpdate()
    {
        if (frame_count++ % 30 == 0) {
            int num, filled;
            Tool.HoleStats(out num, out filled);
            if (num == 0)
                statusText.text = "no holes!";
            else
                statusText.text = string.Format("filled: {0} remaining: {1}", filled, num-filled);
        }
    }


    protected override void after_tool_values_update()
    {
        minimalPanel.SetVisible(false);
        smoothPanel.SetVisible(false);
        switch (Tool.FillType) {
            case FillHolesTool.FillTypes.Minimal: minimalPanel.SetVisible(true); break;
            case FillHolesTool.FillTypes.Smooth: smoothPanel.SetVisible(true); break;
        }

        minOptTrisTolerance.interactable = minOptimizeTris.isOn;

        smoothTargetEdgeLen.interactable = ! smoothAutoTargetEdgeLen.isOn;
    }


    void on_fill_all_clicked()
    {
        Tool.FillAllHoles();
    }





}

