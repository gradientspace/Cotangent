using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;

public class StatusPanelSetup : MonoBehaviour
{
    GameObject statusBG;
    Image statusBGImage;
    Text statusText;

    Colorf color1 = Colorf.LightGreen;
    Colorf color2 = Colorf.PivotYellow;
    double frequency = 3.0f;

    // Use this for initialization
    public void Start()
	{
        try {
            statusBG = this.gameObject.FindChildByName("StatusBG", true);
            statusBGImage = statusBG.GetComponent<Image>();
            statusText = UnityUIUtil.FindText(statusBG, "StatusText");

            statusBG.SetVisible(false);

        } catch (Exception e) {
            DebugUtil.Log("StatusPanelSetup Start(): " + e.Message);
        }
    }

    double start_time = 0;

    public void Update()
    {
        if ( CCStatus.InOperation ) {
            if (statusBG.IsVisible() == false) {
                statusBG.SetVisible(true);
                statusText.text = CCStatus.CurrentOperation;
                start_time = FPlatform.RealTime();
            } else {
                double cur_time = FPlatform.RealTime();
                double dt = cur_time - start_time;
                double t = (Math.Cos(frequency * dt) + 1) * 0.5;
                Colorf c = Colorf.Lerp(color1, color2, (float)t);
                statusBGImage.color = c;
            }
        } else {
            statusBG.SetVisible(false);
        }
    }



}
