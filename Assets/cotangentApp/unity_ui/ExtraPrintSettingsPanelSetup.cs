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
using cotangent;


public class ExtraPrintSettingsPanelSetup : MonoBehaviour
{
    InputField layerRangeMin;
    InputField layerRangeMax;
    InputField startLayers;
    InputField startLayerHeight; GameObject startLayerHeightRow;

    public void Start()
    {
        // Machine Settings panel
        CC.Settings.OnNewSettings += Settings_OnNewSettings;
        CC.Settings.OnSettingModified += Settings_OnSettingModified;

        layerRangeMin = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "LayerRangeMinInputField",
            () => { return CC.Settings.LayerRangeMin; },
            (intValue) => { CC.Settings.LayerRangeMin = intValue; }, 1, 999999);

        layerRangeMax = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "LayerRangeMaxInputField",
            () => { return CC.Settings.LayerRangeMax; },
            (intValue) => { CC.Settings.LayerRangeMax = intValue; }, 1, 999999999+1);

        startLayers = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "StartLayersInputField",
            () => { return CC.Settings.StartLayers; },
            (intValue) => { CC.Settings.StartLayers = intValue; }, 0, 100);

        startLayerHeight = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "StartLayerHeightInputField",
            () => { return (float)CC.Settings.StartLayerHeightMM; },
            (floatValue) => { CC.Settings.StartLayerHeightMM = Math.Round(floatValue, 5); }, 0.01f, 10.0f);
        startLayerHeightRow = startLayerHeight.transform.parent.gameObject;

        is_expanded = true;
        update_visibility();
    }


    bool is_expanded = true;

    private void update_visibility()
    {
        bool set_expanded = (CC.Settings.StartLayers > 0);
        if (set_expanded != is_expanded) {
            is_expanded = set_expanded;
            startLayerHeightRow.SetVisible(is_expanded);
        }
    }


    private void Settings_OnSettingModified(PrintSettings settings)
    {
        SingleMaterialFFFSettings S = CC.PrinterDB.ActivePreset.Settings as SingleMaterialFFFSettings;

        UnityUIUtil.SetBackgroundColor(layerRangeMin, settings.LayerRangeMin_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(layerRangeMax, settings.LayerRangeMax_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(startLayers, settings.StartLayers_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(startLayerHeight, settings.StartLayerHeightMM_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

        update_visibility();
    }

    private void Settings_OnNewSettings(PrintSettings settings)
    {
        layerRangeMin.text = CC.Settings.LayerRangeMin.ToString();
        UnityUIUtil.SetBackgroundColor(layerRangeMin, CotangentUI.NormalSettingColor);

        layerRangeMax.text = CC.Settings.LayerRangeMax.ToString();
        UnityUIUtil.SetBackgroundColor(layerRangeMax, CotangentUI.NormalSettingColor);

        startLayers.text = CC.Settings.StartLayers.ToString();
        UnityUIUtil.SetBackgroundColor(startLayers, CotangentUI.NormalSettingColor);

        startLayerHeight.text = CC.Settings.StartLayerHeightMM.ToString();
        UnityUIUtil.SetBackgroundColor(startLayerHeight, CotangentUI.NormalSettingColor);

        update_visibility();
    }
}

