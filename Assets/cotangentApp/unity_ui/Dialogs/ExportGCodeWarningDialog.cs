#pragma warning disable 414
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class ExportGCodeWarningDialog : MonoBehaviour
{
    Button cancelButton;
    Button exportButton;
    Button exportAlwaysButton;

    // Use this for initialization
    public void Start()
    {
        cancelButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "CancelButton", on_cancel);
        exportButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "ExportButton", on_export);
        exportAlwaysButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "ExportAlwaysButton", on_export_always);
    }

    void on_cancel() {
        this.gameObject.Destroy();
    }

    void on_export()
    {
        CC.ActiveContext.RegisterNextFrameAction(() => {
            CCActions.DoGCodeExportInteractive();
        });
        this.gameObject.Destroy();
    }

    void on_export_always()
    {
        FPlatform.SetPrefsInt("WarnAboutGCodeExport", 1);

        CC.ActiveContext.RegisterNextFrameAction(() => {
            CCActions.DoGCodeExportInteractive();
        });
        this.gameObject.Destroy();
    }


    public static void ShowDialog()
    {
        var panel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("ExportGCodeWarningDialog"));
        CotangentUI.MainUICanvas.AddChild(panel, false);
    }

}

