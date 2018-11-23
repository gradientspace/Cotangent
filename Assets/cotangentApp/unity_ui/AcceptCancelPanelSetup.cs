using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;

public class AcceptCancelPanelSetup : MonoBehaviour
{
	Button acceptButton;
    Button cancelButton;

    Colorf cantAcceptColor = CotangentUI.DisabledButtonColor;
    Colorf canAcceptColor = Colorf.LightGreen;
    Colorf cancelColor = Colorf.VideoRed;

    // Use this for initialization
    public void Start()
	{
        acceptButton = UnityUIUtil.FindButtonAndAddClickHandler("AcceptButton", on_accept);
        cancelButton = UnityUIUtil.FindButtonAndAddClickHandler("CancelButton", on_cancel);

        UnityUIUtil.SetColors(acceptButton, canAcceptColor, cantAcceptColor);
        UnityUIUtil.SetColors(cancelButton, cancelColor, cantAcceptColor);
    }


    public void Update()
    {
        ITool tool = CC.ActiveContext.ToolManager.ActiveRightTool;
        if (tool != null && tool.HasApply && tool.CanApply) {
            acceptButton.interactable = true;
        } else {
            acceptButton.interactable = false;
         }
        cancelButton.interactable = (tool != null);
    }


    void on_accept()
    {
        ITool tool = CC.ActiveContext.ToolManager.ActiveRightTool;
        if (tool != null && tool.HasApply && tool.CanApply) {
            tool.Apply();
            CC.ActiveContext.ToolManager.DeactivateTools();
        }
    }

    void on_cancel()
    {
        if (CC.ActiveContext.ToolManager.HasActiveTool(ToolSide.Right)) {
            CCActions.CancelCurrentTool();
        }
    }




}
