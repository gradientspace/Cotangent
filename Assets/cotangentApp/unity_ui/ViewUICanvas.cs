using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using f3;
using gs;
using cotangent;

public class ViewUICanvas : MonoBehaviour
{

	// Use this for initialization
	void Start () {
        CC.ActiveContext.ToolManager.OnToolActivationChanged += ToolManager_OnToolActivationChanged;
    }

    private void ToolManager_OnToolActivationChanged(ITool tool, ToolSide eSide, bool bActivated)
    {
        ITool curTool = CC.ActiveContext.ToolManager.ActiveRightTool;
        if (curTool == null)
            ActiveToolPanel.SetVisible(false);
        else
            ActiveToolPanel.SetVisible(true);
    }

    // Update is called once per frame
    void Update () {
		
	}
}
