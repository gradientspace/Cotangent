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


public class MeshEditorToolSettings : MonoBehaviour
{
    Button deleteTri;
    Button deleteEdge;
    Button deleteVtx;
    Button deleteComponent;
    Button deleteRing;

    Button collapseEdge;
    Button splitEdge;
    Button flipEdge;

    Button pokeFace;
    Button bridgeEdges;

    Toggle allowBackface;

    public MeshEditorTool Tool;
    public ParameterSet ActiveParameterSet;

    public void Start()
    {
        ITool curTool = CC.ActiveContext.ToolManager.ActiveRightTool;
        if (curTool == null)
            return;
        Tool = curTool as MeshEditorTool;
        if (Tool == null)
            return;
        ActiveParameterSet = Tool.Parameters;

        deleteTri = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "DeleteTriangleButton", () => {
            clear_selection(); set_selection(deleteTri, MeshEditorTool.EditOperations.DeleteTriangle);
        });
        deleteEdge = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "DeleteEdgeButton", () => {
            clear_selection(); set_selection(deleteEdge, MeshEditorTool.EditOperations.DeleteEdge);
        });
        deleteVtx = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "DeleteVertexButton", () => {
            clear_selection(); set_selection(deleteVtx, MeshEditorTool.EditOperations.DeleteVertex);
        });
        deleteComponent = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "DeleteComponentButton", () => {
            clear_selection(); set_selection(deleteComponent, MeshEditorTool.EditOperations.DeleteComponent);
        });
        deleteRing = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "DeleteRingButton", () => {
            clear_selection(); set_selection(deleteRing, MeshEditorTool.EditOperations.DeleteBorderRing);
        });

        collapseEdge = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "CollapseButton", () => {
            clear_selection(); set_selection(collapseEdge, MeshEditorTool.EditOperations.CollapseEdge);
        });
        flipEdge = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "FlipButton", () => {
            clear_selection(); set_selection(flipEdge, MeshEditorTool.EditOperations.FlipEdge);
        });
        splitEdge = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "SplitButton", () => {
            clear_selection(); set_selection(splitEdge, MeshEditorTool.EditOperations.SplitEdge);
        });


        pokeFace = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "PokeTriangleButton", () => {
            clear_selection(); set_selection(pokeFace, MeshEditorTool.EditOperations.PokeTriangle);
        });
        bridgeEdges = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "BridgeButton", () => {
            clear_selection(); set_selection(bridgeEdges, MeshEditorTool.EditOperations.BridgeEdges);
        });


        allowBackface = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "BackfaceToggle",
            () => { return ActiveParameterSet.GetValueBool("allow_backface_hits"); },
            (boolValue) => { ActiveParameterSet.SetValue("allow_backface_hits", boolValue); });

        set_selection(deleteTri, MeshEditorTool.EditOperations.DeleteTriangle);
    }


    void update_from_tool()
    {
        allowBackface.isOn = ActiveParameterSet.GetValueBool("allow_backface_hits");
    }



    void clear_selection()
    {
        switch (Tool.ActiveOperation) {
            case MeshEditorTool.EditOperations.DeleteTriangle:
                clear_selection(deleteTri); break;
            case MeshEditorTool.EditOperations.PokeTriangle:
                clear_selection(pokeFace); break;

            case MeshEditorTool.EditOperations.DeleteEdge:
                clear_selection(deleteEdge); break;
            case MeshEditorTool.EditOperations.FlipEdge:
                clear_selection(flipEdge); break;
            case MeshEditorTool.EditOperations.SplitEdge:
                clear_selection(splitEdge); break;
            case MeshEditorTool.EditOperations.CollapseEdge:
                clear_selection(collapseEdge); break;

            case MeshEditorTool.EditOperations.DeleteVertex:
                clear_selection(deleteVtx); break;

            case MeshEditorTool.EditOperations.BridgeEdges:
                clear_selection(bridgeEdges); break;

            case MeshEditorTool.EditOperations.DeleteComponent:
                clear_selection(deleteComponent); break;
            case MeshEditorTool.EditOperations.DeleteBorderRing:
                clear_selection(deleteRing); break;
        }
    }

    void clear_selection(Button button)
    {
        UnityUIUtil.SetColors(button, Colorf.White, Colorf.DimGrey);
    }

    void set_selection(Button button, MeshEditorTool.EditOperations opType)
    {
        UnityUIUtil.SetColors(button, Colorf.LightGreen, Colorf.DimGrey);
        Tool.ActiveOperation = opType;
    }








}

