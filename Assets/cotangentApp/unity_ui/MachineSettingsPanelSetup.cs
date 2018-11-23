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


public class MachineSettingsPanelSetup : MonoBehaviour
{
    InputField nozzleTemp;
    InputField bedTemp;
    InputField printSpeed;
    InputField travelSpeed;
    InputField fanSpeed;
    InputField nozzleWidth;
    InputField filamentDiam;
    InputField bedSizeX, bedSizeY, bedSizeZ;

    GameObject bedTempGO;

    public void Start()
    {
        // Machine Settings panel
        CC.Settings.OnNewSettings += Settings_OnNewSettings;
        CC.Settings.OnSettingModified += Settings_OnSettingModified;

        nozzleWidth = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "NozzleWidthInputField",
            () => { return (float)CC.Settings.NozzleDiameterMM; },
            (floatValue) => { CC.Settings.NozzleDiameterMM = Math.Round(floatValue,5); }, 0.05f, 50.0f);

        filamentDiam = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "FilamentDiamInputField",
            () => { return (float)CC.Settings.FilamentDiameterMM; },
            (floatValue) => { CC.Settings.FilamentDiameterMM = Math.Round(floatValue, 5); }, 0.05f, 10.0f);

        nozzleTemp = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "ExtruderTempInputField",
            () => { return CC.Settings.ExtruderTempC; },
            (intValue) => { CC.Settings.ExtruderTempC = intValue; }, 10, 500);

        bedTemp = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "BedTempInputField",
            () => { return CC.Settings.BedTempC; },
            (intValue) => { CC.Settings.BedTempC = intValue; }, 0, 100);
        bedTempGO = bedTemp.gameObject.transform.parent.gameObject;
        bedTempGO.SetVisible(CC.Settings.HasHeatedBed);

        printSpeed = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "PrintSpeedInputField",
            () => { return CC.Settings.PrintSpeedMMS; },
            (intValue) => { CC.Settings.PrintSpeedMMS = intValue; }, 10, 1000);

        travelSpeed = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "TravelSpeedInputField",
            () => { return CC.Settings.TravelSpeedMMS; },
            (intValue) => { CC.Settings.TravelSpeedMMS = intValue; }, 10, 1000);

        fanSpeed = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "FanSpeedInput",
            () => { return CC.Settings.FanSpeedX; },
            (intValue) => { CC.Settings.FanSpeedX = intValue; }, 0, 100);

        bedSizeX = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "BedSizeXInputField",
            () => { return CC.Settings.BedSizeXMM; },
            (intValue) => { CC.Settings.BedSizeXMM = intValue; }, 10, 10000);
        bedSizeY = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "BedSizeYInputField",
            () => { return CC.Settings.BedSizeYMM; },
            (intValue) => { CC.Settings.BedSizeYMM = intValue; }, 10, 10000);
        bedSizeZ = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "BedSizeZInputField",
            () => { return CC.Settings.BedSizeZMM; },
            (intValue) => { CC.Settings.BedSizeZMM = intValue; }, 10, 10000);

    }

    private void Settings_OnSettingModified(PrintSettings settings)
    {
        SingleMaterialFFFSettings S = CC.PrinterDB.ActivePreset.Settings as SingleMaterialFFFSettings;

        UnityUIUtil.SetBackgroundColor(nozzleWidth, settings.NozzleDiameterMM_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(filamentDiam, settings.FilamentDiameterMM_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

        UnityUIUtil.SetBackgroundColor(nozzleTemp, settings.ExtruderTempC_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(bedTemp, settings.BedTempC_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

        UnityUIUtil.SetBackgroundColor(printSpeed, settings.PrintSpeedMMS_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(travelSpeed, settings.TravelSpeedMMS_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(fanSpeed, settings.FanSpeedX_Modified?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

        UnityUIUtil.SetBackgroundColor(bedSizeX, settings.BedSizeXMM_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(bedSizeY, settings.BedSizeYMM_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
        UnityUIUtil.SetBackgroundColor(bedSizeZ, settings.BedSizeZMM_Modified ?
            CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
    }

    private void Settings_OnNewSettings(PrintSettings settings)
    {
        nozzleWidth.text = CC.Settings.NozzleDiameterMM.ToString();
        UnityUIUtil.SetBackgroundColor(nozzleWidth, CotangentUI.NormalSettingColor);

        filamentDiam.text = CC.Settings.FilamentDiameterMM.ToString();
        UnityUIUtil.SetBackgroundColor(filamentDiam, CotangentUI.NormalSettingColor);

        nozzleTemp.text = CC.Settings.ExtruderTempC.ToString();
        UnityUIUtil.SetBackgroundColor(nozzleTemp, CotangentUI.NormalSettingColor);

        bedTemp.text = CC.Settings.BedTempC.ToString();
        UnityUIUtil.SetBackgroundColor(bedTemp, CotangentUI.NormalSettingColor);
        bedTempGO.SetVisible(CC.Settings.HasHeatedBed);

        printSpeed.text = CC.Settings.PrintSpeedMMS.ToString();
        UnityUIUtil.SetBackgroundColor(printSpeed, CotangentUI.NormalSettingColor);

        fanSpeed.text = CC.Settings.FanSpeedX.ToString();
        UnityUIUtil.SetBackgroundColor(fanSpeed, CotangentUI.NormalSettingColor);

        travelSpeed.text = CC.Settings.TravelSpeedMMS.ToString();
        UnityUIUtil.SetBackgroundColor(travelSpeed, CotangentUI.NormalSettingColor);

        bedSizeX.text = CC.Settings.BedSizeXMM.ToString();
        UnityUIUtil.SetBackgroundColor(bedSizeX, CotangentUI.NormalSettingColor);

        bedSizeY.text = CC.Settings.BedSizeYMM.ToString();
        UnityUIUtil.SetBackgroundColor(bedSizeY, CotangentUI.NormalSettingColor);

        bedSizeZ.text = CC.Settings.BedSizeZMM.ToString();
        UnityUIUtil.SetBackgroundColor(bedSizeZ, CotangentUI.NormalSettingColor);

    }
}

