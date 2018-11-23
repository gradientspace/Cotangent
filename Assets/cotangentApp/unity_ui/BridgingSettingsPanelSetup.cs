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
    public class BridgingSettingsPanelSetup : MonoBehaviour
    {
        Toggle enableBridging;
        InputField maxDistance;

        GameObject maxDistanceRow;

        public void Start()
        {
            CC.Settings.OnNewSettings += Settings_OnNewSettings;
            CC.Settings.OnSettingModified += Settings_OnSettingModified;

            enableBridging = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "EnableBridgingToggle",
                () => { return CC.Settings.EnableBridging; },
                (boolValue) => { CC.Settings.EnableBridging = boolValue; });

            maxDistance = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "MaxBridgeDistInputField",
                () => { return (float)CC.Settings.MaxBridgeDistanceMM; },
                (floatValue) => { CC.Settings.MaxBridgeDistanceMM = Math.Round(floatValue, 2); }, 0.01f, 500.00f);
            maxDistanceRow = maxDistance.transform.parent.gameObject;

            is_expanded = true;
            update_visibility();
        }


        bool is_expanded;

        private void update_visibility()
        {
            if ( CC.Settings.EnableBridging != is_expanded) {
                is_expanded = CC.Settings.EnableBridging;
                maxDistanceRow.SetVisible(is_expanded);
            }
        }


        private void Settings_OnSettingModified(PrintSettings settings)
        {
            SingleMaterialFFFSettings S = CC.PrinterDB.ActivePreset.Settings as SingleMaterialFFFSettings;

            UnityUIUtil.SetBackgroundColor(enableBridging, settings.EnableBridging_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(maxDistance, settings.MaxBridgeDistanceMM_Modified?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            update_visibility();
        }

        private void Settings_OnNewSettings(PrintSettings settings)
        {
            enableBridging.isOn = CC.Settings.EnableBridging;
            UnityUIUtil.SetBackgroundColor(enableBridging, CotangentUI.NormalSettingColor);

            maxDistance.text = CC.Settings.MaxBridgeDistanceMM.ToString();
            UnityUIUtil.SetBackgroundColor(maxDistance, CotangentUI.NormalSettingColor);

            update_visibility();
        }
    }
}
