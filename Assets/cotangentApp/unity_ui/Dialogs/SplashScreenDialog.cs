#pragma warning disable 414
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class SplashScreenDialog : UnityUIDialogBase
{
    Button dismissButton;


    public override bool HideOnAwake {
        get { return false; }
    }

    float start_time;

    // Use this for initialization
    public void Start()
    {
        dismissButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "DimissButton", dismiss_on_click);
        start_time = FPlatform.RealTime();

        if (FPlatform.InUnityEditor())
            dismiss_on_click();
    }


    public void Update()
    {
        if (FPlatform.RealTime() - start_time > 2.0)
            base.TransitionVisibility(false);
    }



    void dismiss_on_click()
    {
        base.TransitionVisibility(false);
    }

    protected override void on_hide_transition_complete()
    {
        this.gameObject.Destroy();
    }

}



