using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;


/// <summary>
/// print settings bar controller
/// </summary>
public class PrintSettingsPanelSetup : MonoBehaviour
{
    public void Awake()
    {
    }

    // Use this for initialization
    public void Start()
    {
        CC.ActiveContext.ToolManager.OnToolActivationChanged += ToolManager_OnToolActivationChanged;
    }

    private void ToolManager_OnToolActivationChanged(ITool tool, ToolSide eSide, bool bActivated)
    {
        if (bActivated)
            this.gameObject.SetVisible(false);
        else
            this.gameObject.SetVisible(true);
    }

    public void Update()
    {

    }

}
