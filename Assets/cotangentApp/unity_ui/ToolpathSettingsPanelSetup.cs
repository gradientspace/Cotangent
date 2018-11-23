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
    public class ToolpathSettingsPanelSetup : MonoBehaviour
    {
        InputField layerHeight;
        InputField shells;
        InputField roofLayers;
        InputField floorLayers;
        InputField infill;
        InputField interiorSolidRegionShells;
        Toggle clipOverlaps;
        Dropdown openMeshMode;

        public void Start()
        {
            CC.Settings.OnNewSettings += Settings_OnNewSettings;
            CC.Settings.OnSettingModified += Settings_OnSettingModified;

            layerHeight = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "LayerHeightInputField",
                () => { return (float)CC.Settings.LayerHeightMM; },
                (floatValue) => { CC.Settings.LayerHeightMM = Math.Round(floatValue, 5); }, 0.01f, 10.0f);

            infill = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "InfillSpacingInputField",
                () => { return (float)CC.Settings.InfillStepX; },
                (floatValue) => { CC.Settings.InfillStepX = Math.Round(floatValue, 5); }, 1.0f, 9999.0f);

            shells = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "ShellsInputField",
                () => { return CC.Settings.OuterShells; },
                (intValue) => { CC.Settings.OuterShells = intValue; }, 1, 1000);

            roofLayers = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "RoofLayersInputField",
                () => { return CC.Settings.RoofLayers; },
                (intValue) => { CC.Settings.RoofLayers = intValue; }, 0, 999999);

            floorLayers = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "FloorLayersInputField",
                () => { return CC.Settings.FloorLayers; },
                (intValue) => { CC.Settings.FloorLayers = intValue; }, 0, 999999);

            clipOverlaps = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "ClipSelfOverlapsToggle",
                () => { return CC.Settings.ClipSelfOverlaps; },
                (boolValue) => { CC.Settings.ClipSelfOverlaps = boolValue; });

            interiorSolidRegionShells = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "InteriorSolidRegionShellsInputField",
                () => { return CC.Settings.InteriorSolidRegionShells; },
                (intValue) => { CC.Settings.InteriorSolidRegionShells = intValue; }, 0, 999999);

            openMeshMode = UnityUIUtil.FindDropDownAndAddHandlers(this.gameObject, "OpenMeshesDropDown",
                () => { return CC.Settings.OpenModeInt; },
                (intValue) => { CC.Settings.OpenModeInt = intValue; }, (int)PrintSettings.OpenMeshMode.Clipped, (int)PrintSettings.OpenMeshMode.Ignored);

        }

        private void Settings_OnSettingModified(PrintSettings settings)
        {
            SingleMaterialFFFSettings S = CC.PrinterDB.ActivePreset.Settings as SingleMaterialFFFSettings;

            UnityUIUtil.SetBackgroundColor(layerHeight, settings.LayerHeightMM_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
            UnityUIUtil.SetBackgroundColor(infill, settings.InfillStepX_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(shells, settings.OuterShells_Modified?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
            UnityUIUtil.SetBackgroundColor(roofLayers, settings.RoofLayers_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
            UnityUIUtil.SetBackgroundColor(floorLayers, settings.FloorLayers_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
            UnityUIUtil.SetBackgroundColor(interiorSolidRegionShells, settings.InteriorSolidRegionShells_Modified?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

            UnityUIUtil.SetBackgroundColor(clipOverlaps, settings.ClipSelfOverlaps_Modified ?
                CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);

        }

        private void Settings_OnNewSettings(PrintSettings settings)
        {
            layerHeight.text = CC.Settings.LayerHeightMM.ToString();
            UnityUIUtil.SetBackgroundColor(layerHeight, CotangentUI.NormalSettingColor);

            infill.text = CC.Settings.InfillStepX.ToString();
            UnityUIUtil.SetBackgroundColor(infill, CotangentUI.NormalSettingColor);

            shells.text = CC.Settings.OuterShells.ToString();
            UnityUIUtil.SetBackgroundColor(shells, CotangentUI.NormalSettingColor);

            roofLayers.text = CC.Settings.RoofLayers.ToString();
            UnityUIUtil.SetBackgroundColor(roofLayers, CotangentUI.NormalSettingColor);

            floorLayers.text = CC.Settings.FloorLayers.ToString();
            UnityUIUtil.SetBackgroundColor(floorLayers, CotangentUI.NormalSettingColor);

            interiorSolidRegionShells.text = CC.Settings.InteriorSolidRegionShells.ToString();
            UnityUIUtil.SetBackgroundColor(interiorSolidRegionShells, CotangentUI.NormalSettingColor);

            clipOverlaps.isOn = CC.Settings.ClipSelfOverlaps;
            UnityUIUtil.SetBackgroundColor(clipOverlaps, CotangentUI.NormalSettingColor);

            openMeshMode.value = CC.Settings.OpenModeInt;
        }
    }
}
