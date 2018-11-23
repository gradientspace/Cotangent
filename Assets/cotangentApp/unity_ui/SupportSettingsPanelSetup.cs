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

namespace cotangent
{
    public class SupportSettingsPanelSetup : MonoBehaviour
    {
        Toggle generateSupport;
        InputField overhangAngle;
        Toggle supportMinZTips;
        InputField supportSpacing;
        InputField supportGap;
        Toggle supportShell;
        Toggle supportReleaseOpt;

        GameObject overhangAngleRow;
        GameObject supportMinZTipsRow;
        GameObject supportSpacingRow;
        GameObject supportShellRow;
        GameObject supportGapRow;

        public void Start()
        {
            CC.Settings.OnNewSettings += Settings_OnNewSettings;
            CC.Settings.OnSettingModified += Settings_OnSettingModified;

            generateSupport = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "GenerateSupportToggle",
                () => { return CC.Settings.GenerateSupport; },
                (boolValue) => { CC.Settings.GenerateSupport = boolValue; });

            overhangAngle = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "OverhangAngleInputField",
                () => { return (float)CC.Settings.OverhangAngleDeg; },
                (floatValue) => { CC.Settings.OverhangAngleDeg = Math.Round(floatValue, 2); }, 0.01f, 89.99f);
            overhangAngleRow = overhangAngle.transform.parent.gameObject;

            supportMinZTips = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "SupportTipsToggle",
                () => { return CC.Settings.SupportMinZTips; },
                (boolValue) => { CC.Settings.SupportMinZTips = boolValue; });
            supportMinZTipsRow = supportMinZTips.transform.parent.gameObject;

            supportSpacing = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "SupportSpacingInputField",
                () => { return (float)CC.Settings.SupportStepX; },
                (floatValue) => { CC.Settings.SupportStepX = Math.Round(floatValue, 5); }, 1.0f, 9999.0f);
            supportSpacingRow = supportSpacing.transform.parent.gameObject;

            supportShell = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "SupportShellToggle",
                () => { return CC.Settings.EnableSupportShell; },
                (boolValue) => { CC.Settings.EnableSupportShell = boolValue; });
            supportShellRow = supportShell.transform.parent.gameObject;

            supportReleaseOpt = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "SupportReleaseOptToggle",
                () => { return CC.Settings.EnableSupportReleaseOpt; },
                (boolValue) => { CC.Settings.EnableSupportReleaseOpt = boolValue; });

            supportGap = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "SupportGapInputField",
                () => { return (float)CC.Settings.SupportSolidSpace; },
                (floatValue) => { CC.Settings.SupportSolidSpace = Math.Round(floatValue, 5); }, 0.0f, 100.0f);
            supportGapRow = supportGap.transform.parent.gameObject;

            is_expanded = true;
            update_visibility();
        }


        bool is_expanded;

        private void update_visibility()
        {
            if ( CC.Settings.GenerateSupport != is_expanded) {
                is_expanded = CC.Settings.GenerateSupport;
                overhangAngleRow.SetVisible(is_expanded);
                supportMinZTipsRow.SetVisible(is_expanded);
            }
        }


        private void Settings_OnSettingModified(PrintSettings settings)
        {
            SingleMaterialFFFSettings S = CC.PrinterDB.ActivePreset.Settings as SingleMaterialFFFSettings;

            if (generateSupport.isOn != CC.Settings.GenerateSupport)
                generateSupport.isOn = CC.Settings.GenerateSupport;
            UnityUIUtil.SetBackgroundColor(generateSupport, settings.GenerateSupport_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(overhangAngle, settings.OverhangAngleDeg_Modified?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(supportMinZTips, settings.SupportMinZTips_Modified?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(supportSpacing, settings.SupportStepX_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(supportShell, settings.EnableSupportShell_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(supportReleaseOpt, settings.EnableSupportReleaseOpt_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(supportGap, settings.SupportSolidSpace_Modified?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            update_visibility();
        }

        private void Settings_OnNewSettings(PrintSettings settings)
        {
            generateSupport.isOn = CC.Settings.GenerateSupport;
            UnityUIUtil.SetBackgroundColor(generateSupport, CotangentUI.NormalSettingColor);

            overhangAngle.text = CC.Settings.OverhangAngleDeg.ToString();
            UnityUIUtil.SetBackgroundColor(overhangAngle, CotangentUI.NormalSettingColor);

            supportMinZTips.isOn = CC.Settings.SupportMinZTips;
            UnityUIUtil.SetBackgroundColor(supportMinZTips, CotangentUI.NormalSettingColor);

            supportSpacing.text = CC.Settings.SupportStepX.ToString();
            UnityUIUtil.SetBackgroundColor(supportSpacing, CotangentUI.NormalSettingColor);

            supportShell.isOn = CC.Settings.EnableSupportShell;
            UnityUIUtil.SetBackgroundColor(supportShell, CotangentUI.NormalSettingColor);

            supportReleaseOpt.isOn = CC.Settings.EnableSupportReleaseOpt;
            UnityUIUtil.SetBackgroundColor(supportReleaseOpt, CotangentUI.NormalSettingColor);

            supportGap.text = CC.Settings.SupportSolidSpace.ToString();
            UnityUIUtil.SetBackgroundColor(supportGap, CotangentUI.NormalSettingColor);

            update_visibility();
        }
    }
}
