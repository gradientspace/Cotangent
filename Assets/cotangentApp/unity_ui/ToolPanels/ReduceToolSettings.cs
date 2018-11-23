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


public class ReduceToolSettings : BaseToolSettings<ReduceTool>
{
    MappedDropDown reduceMode;

    InputField triangleCount;
    GameObject trianglePanel;
    InputField vertexCount;
    GameObject vertexPanel;
    InputField minEdgeLength;
    GameObject minEdgeLengthPanel;

    Text statusText;

    Toggle reproject;
    Toggle replaceInput;


    protected override void register_parameters()
    {
        reduceMode = base.RegisterDropDown("ReduceTypeDropDown", "target_mode",
            new List<string>() { "Triangles", "Vertices", "Min Length" },
            new List<int>() {
                (int)ReduceOp.TargetModes.TriangleCount,
                (int)ReduceOp.TargetModes.VertexCount,
                (int)ReduceOp.TargetModes.MinEdgeLength
            });

        triangleCount = base.RegisterIntInput("TriangleCountInput", "triangle_count", new Interval1i(0, int.MaxValue));
        trianglePanel = this.gameObject.FindChildByName("TriangleCountParam", true);

        vertexCount = base.RegisterIntInput("VertexCountInput", "vertex_count", new Interval1i(0, int.MaxValue));
        vertexPanel = this.gameObject.FindChildByName("VertexCountParam", true);

        minEdgeLength = base.RegisterFloatInput("MinEdgeLengthInput", "min_edge_length", new Interval1d(0.0001, 100), "F8");
        minEdgeLengthPanel = this.gameObject.FindChildByName("MinEdgeLengthParam", true);

        reproject = base.RegisterToggle("ReprojectToggle", "reproject");
        replaceInput = base.RegisterToggle("ReplaceInputToggle", "replace_input");

        statusText = UnityUIUtil.FindTextAndSet(this.gameObject, "StatusText", "");
    }


    int frame_count = 0;
    private void LateUpdate()
    {
        if (frame_count++ % 30 == 0) {
            int v, t, e;
            Tool.CurrentTriStats(out v, out t, out e);
            statusText.text = string.Format("V {0} T {1} E {2}", v, t, e);
        }
    }



    protected override void after_tool_values_update() { 

        trianglePanel.gameObject.SetVisible(false);
        vertexPanel.gameObject.SetVisible(false);
        minEdgeLengthPanel.gameObject.SetVisible(false);
        switch (Tool.ReduceMode) {
            case ReduceOp.TargetModes.TriangleCount: trianglePanel.gameObject.SetVisible(true); break;
            case ReduceOp.TargetModes.VertexCount: vertexPanel.gameObject.SetVisible(true); break;
            case ReduceOp.TargetModes.MinEdgeLength: minEdgeLengthPanel.gameObject.SetVisible(true); break;
        }
    }








}

