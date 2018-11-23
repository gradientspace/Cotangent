using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using cotangent;

public class ViewModePanelSetup : MonoBehaviour
{
    Button printModeButton;
    Button repairModeButton;
    Button modelModeButton;
    AppViewMode highlighted;

    Colorf normalColor = Colorf.VideoWhite;
    Colorf highlightColor = Colorf.LightGreen;
    Colorf disabledColor = Colorf.DimGrey;

    // Use this for initialization
    public void Start()
    {
        printModeButton = UnityUIUtil.FindButtonAndAddClickHandler("PrintButton", on_print_mode);
        repairModeButton = UnityUIUtil.FindButtonAndAddClickHandler("RepairButton", on_repair_mode);
        modelModeButton = UnityUIUtil.FindButtonAndAddClickHandler("ModelButton", on_model_mode);
        set_highlight(AppViewMode.PrintView);
        highlighted = AppViewMode.PrintView;
    }


    public void Update()
    {
        if (highlighted != CCActions.CurrentViewMode) {
            highlighted = CCActions.CurrentViewMode;
            set_highlight(highlighted);
        }
    }


    void on_print_mode()
    {
        CCActions.SwitchToViewMode(AppViewMode.PrintView);
    }

    void on_repair_mode()
    {
        CCActions.SwitchToViewMode(AppViewMode.RepairView);
    }

    void on_model_mode()
    {
        CCActions.SwitchToViewMode(AppViewMode.ModelView);
    }


    void set_highlight(AppViewMode mode)
    {
        if (mode == AppViewMode.PrintView) {
            UnityUIUtil.SetColors(printModeButton, highlightColor, disabledColor);
            UnityUIUtil.SetColors(repairModeButton, normalColor, disabledColor);
            UnityUIUtil.SetColors(modelModeButton, normalColor, disabledColor);
        } else if (mode == AppViewMode.RepairView) {
            UnityUIUtil.SetColors(printModeButton, normalColor, disabledColor);
            UnityUIUtil.SetColors(repairModeButton, highlightColor, disabledColor);
            UnityUIUtil.SetColors(modelModeButton, normalColor, disabledColor);
        } else if (mode == AppViewMode.ModelView) {
            UnityUIUtil.SetColors(printModeButton, normalColor, disabledColor);
            UnityUIUtil.SetColors(repairModeButton, normalColor, disabledColor);
            UnityUIUtil.SetColors(modelModeButton, highlightColor, disabledColor);
        }
    }



}