#pragma warning disable 414
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class PrivacyStartupDialog : MonoBehaviour
{
    Button closeButton;
    Button privacySettingsButton;

    // Use this for initialization
    public void Start()
    {
        closeButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "CloseButton", on_close);
        privacySettingsButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "PrivacySettingsButton", on_privacy_settings);
    }

    void on_close() {
        CotangentVersion.SetPrivacyPolicyConfirmed();
        this.gameObject.Destroy();
    }

    void on_privacy_settings()
    {
        CotangentVersion.SetPrivacyPolicyConfirmed();
        CC.ActiveContext.RegisterNextFrameAction(() => {
            CCActions.ShowPreferencesDialog(true);
        });
        this.gameObject.Destroy();
    }


    public static void ShowDialog()
    {
        var panel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("PrivacyStartupDialog"));
        CotangentUI.MainUICanvas.AddChild(panel, false);
    }

}

