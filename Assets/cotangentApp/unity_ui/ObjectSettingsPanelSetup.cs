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


/// <summary>
/// Object-properties panel controller
/// </summary>
public class ObjectSettingsPanelSetup : MonoBehaviour
{
    Dropdown objectType;
    Toggle noVoids;
    Toggle shellOnly;
    Dropdown openMeshMode;
    Toggle autoReload;

    InputField clearance;
    GameObject clearanceRow;
    InputField offsetxy;
    GameObject offsetxyRow;


    public void Start()
    {
        // Machine Settings panel
        CC.ObjSettings.OnNewSettings += Settings_OnNewSettings;
        CC.ObjSettings.OnSettingModified += Settings_OnSettingModified;

        objectType = UnityUIUtil.FindDropDownAndAddHandlers(Panel, "ObjectTypeDropDown",
            () => { return CC.ObjSettings.ObjectTypeInt; },
            (intValue) => { CC.ObjSettings.ObjectTypeInt = intValue; },
            (int)PrintMeshSettings.ObjectTypes.Solid, (int)PrintMeshSettings.ObjectTypes.Ignored);

        noVoids = UnityUIUtil.FindToggleAndConnectToSource(Panel, "NoVoidsToggle",
            () => { return CC.ObjSettings.NoVoids; },
            (boolValue) => { CC.ObjSettings.NoVoids = boolValue; });

        shellOnly = UnityUIUtil.FindToggleAndConnectToSource(Panel, "ShellOnlyToggle",
            () => { return CC.ObjSettings.OuterShellOnly; },
            (boolValue) => { CC.ObjSettings.OuterShellOnly = boolValue; });

        openMeshMode = UnityUIUtil.FindDropDownAndAddHandlers(Panel, "OpenMeshesDropDown",
            () => { return CC.ObjSettings.OpenModeInt; },
            (intValue) => { CC.ObjSettings.OpenModeInt = intValue; }, 
            (int)PrintMeshSettings.OpenMeshModes.Default, (int)PrintMeshSettings.OpenMeshModes.Ignored);

        clearance = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "ClearanceInputField",
            () => { return (float)CC.ObjSettings.Clearance; },
            (floatValue) => { CC.ObjSettings.Clearance = Math.Round(floatValue, 3); }, -100.0f, 100.0f);
        clearanceRow = clearance.transform.parent.gameObject;

        offsetxy = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "XYOffsetInputField",
            () => { return (float)CC.ObjSettings.OffsetXY; },
            (floatValue) => { CC.ObjSettings.OffsetXY = Math.Round(floatValue, 3); }, -100.0f, 100.0f);
        offsetxyRow = offsetxy.transform.parent.gameObject;

        autoReload = UnityUIUtil.FindToggleAndConnectToSource(Panel, "AutoUpdateToggle",
            () => { return CC.ObjSettings.AutoReloadChanges; },
            (boolValue) => { CC.ObjSettings.AutoReloadChanges = boolValue; });
        autoReload.interactable = false;
    }


 

    private void Settings_OnSettingModified(ObjectSettings settings) {
        //UnityUIUtil.SetBackgroundColor(noVoids, settings.NoVoids_Modified ?
        //    CotangentUI.ModifiedSettingColor : CotangentUI.NormalSettingColor);
    }

    private void Settings_OnNewSettings(ObjectSettings settings) {
        objectType.value = settings.ObjectTypeInt;
        noVoids.isOn = settings.NoVoids;
        shellOnly.isOn = settings.OuterShellOnly;
        //UnityUIUtil.SetBackgroundColor(noVoids, CotangentUI.NormalSettingColor);

        clearance.text = CC.ObjSettings.Clearance.ToString();
        offsetxy.text = CC.ObjSettings.OffsetXY.ToString();

        openMeshMode.value = settings.OpenModeInt;

        if ( settings.CanAutoReloadChanges ) {
            autoReload.interactable = true;
            autoReload.isOn = settings.AutoReloadChanges;
        } else {
            autoReload.interactable = false;
        }

    }




    static GameObject panelGO;
    public static GameObject Panel {
        get {
            if (panelGO == null)
                panelGO = UnityUtil.FindGameObjectByName("ObjectSettingsPanel");
            return panelGO;
        }
    }


}
