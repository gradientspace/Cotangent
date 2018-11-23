#pragma warning disable 414
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class PreferencesDialog : UnityUIDialogBase
{
    Button dismissButton;

    // tabs
    Button prefsTabButton;      GameObject prefsTab;
    Button privacyTabButton;    GameObject privacyTab;
    Button aboutTabButton;      GameObject aboutTab;

    // options
    // --- camera settings
    MappedDropDown startupWorkspace;
    MappedDropDown cameraMode;
    MappedDropDown graphicsQuality;
    // --- import settings
    MappedDropDown importAssistantMode;
    MappedDropDown importXFormMode;
    InputField largeMeshSizeInput;


    // privacy
    Button unityPolicyButton;


    // Use this for initialization
    public void Start()
    {
        dismissButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "DismissButton", on_dismiss);
        UnityUIUtil.FindTextAndSet(this.gameObject, "VersionText", string.Format("v{0}.{1}.{2}",
            CotangentVersion.CurrentVersion.a, CotangentVersion.CurrentVersion.b, CotangentVersion.CurrentVersion.c));

        prefsTabButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "PreferencesButton", on_prefs_tab);
        prefsTab = this.gameObject.FindChildByName("PreferencesPanel", true);
        privacyTabButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "PrivacyButton", on_privacy_tab);
        privacyTab = this.gameObject.FindChildByName("PrivacyPanel", true);
        aboutTabButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "AboutButton", on_about_tab);
        aboutTab = this.gameObject.FindChildByName("AboutPanel", true);



        startupWorkspace = new MappedDropDown(
            this.gameObject.FindChildByName("StartupWorkspaceDropDown", true).GetComponent<Dropdown>(),
            () => { return (int)CCPreferences.StartupWorkspace; },
            (intValue) => { CCPreferences.StartupWorkspace = (AppViewMode)intValue; });
        startupWorkspace.SetOptions(
            new List<string>() { "Print", "Repair", "Model" },
            new List<int>() {
                (int)AppViewMode.PrintView,
                (int)AppViewMode.RepairView,
                (int)AppViewMode.ModelView });
        startupWorkspace.SetFromId((int)CCPreferences.StartupWorkspace);


        graphicsQuality = new MappedDropDown(
            this.gameObject.FindChildByName("GraphicsQualityDropDown", true).GetComponent<Dropdown>(),
            () => { return (int)CCPreferences.GraphicsQuality; },
            (intValue) => { CCPreferences.GraphicsQuality = (CCPreferences.GraphicsQualityLevels)intValue; });
        graphicsQuality.SetOptions(
            new List<string>() { "Maximum", "High", "Medium", "Low", "Fastest" },
            new List<int>() {
                (int)CCPreferences.GraphicsQualityLevels.Max,
                (int)CCPreferences.GraphicsQualityLevels.High,
                (int)CCPreferences.GraphicsQualityLevels.Medium,
                (int)CCPreferences.GraphicsQualityLevels.Low,
                (int)CCPreferences.GraphicsQualityLevels.Fastest });
        graphicsQuality.SetFromId((int)CCPreferences.GraphicsQuality);


        cameraMode = new MappedDropDown(
            this.gameObject.FindChildByName("CameraModeDropDown", true).GetComponent<Dropdown>(),
            () => { return (int)CCPreferences.CameraMode; },
            (intValue) => { CCPreferences.CameraMode = (CCPreferences.CameraModes)intValue; });
        cameraMode.SetOptions(
            new List<string>() { "Perspective", "Orthographic" },
            new List<int>() {
                (int)CCPreferences.CameraModes.Perspective,
                (int)CCPreferences.CameraModes.Orthographic });
        cameraMode.SetFromId((int)CCPreferences.CameraMode);



        importXFormMode = new MappedDropDown(
            this.gameObject.FindChildByName("ImportXFormDropdown", true).GetComponent<Dropdown>(),
            () => { return (int)CCPreferences.ImportTransformMode; },
            (intValue) => { CCPreferences.ImportTransformMode = (CCPreferences.ImportTransformModes)intValue; });
        importXFormMode.SetOptions(
            new List<string>() { "Auto-Center First", "Auto-Center All", "No Transformations" },
            new List<int>() {
                (int)CCPreferences.ImportTransformModes.AutoCenterFirst,
                (int)CCPreferences.ImportTransformModes.AutoCenterAll,
                (int)CCPreferences.ImportTransformModes.NoAutoCenter });
        importXFormMode.SetFromId((int)CCPreferences.ImportTransformMode);


        importAssistantMode = new MappedDropDown(
            this.gameObject.FindChildByName("ImportAssistantModeDropdown", true).GetComponent<Dropdown>(),
            () => { return (int)CCPreferences.ImportAssistantMode; },
            (intValue) => { CCPreferences.ImportAssistantMode = (CCPreferences.ImportAssistantModes)intValue; });
        importAssistantMode.SetOptions(
            new List<string>() { "All Assistants", "Mesh Size Only", "Dimensions Only", "Disabled" },
            new List<int>() {
                (int)CCPreferences.ImportAssistantModes.All,
                (int)CCPreferences.ImportAssistantModes.MeshSizeOnly,
                (int)CCPreferences.ImportAssistantModes.PhysicalSizeOnly,
                (int)CCPreferences.ImportAssistantModes.Disabled });
        importAssistantMode.SetFromId((int)CCPreferences.ImportAssistantMode);


        largeMeshSizeInput = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, "LargeMeshSizeInput",
                () => { return CCPreferences.LargeMeshImportThreshold; },
                (intValue) => { CCPreferences.LargeMeshImportThreshold = intValue; },
                10000, 999999999);
        largeMeshSizeInput.text = CCPreferences.LargeMeshImportThreshold.ToString();


        unityPolicyButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "PrivacyPolicyButton", on_unity_privacy_policy);


        // about dialog
        UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "GradientspaceWebButton", on_open_gradientspace_site);
        UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "CotangentWebButton", on_open_cotangent_site);


        on_prefs_tab();
    }


    public void Update()
    {
    }


    public void SetToPrivacyTab()
    {
        set_to_tab(privacyTab, privacyTabButton);
    }


    void set_to_tab(GameObject panel, Button button)
    {
        prefsTab.SetVisible(false);
        UnityUIUtil.SetColors(prefsTabButton, Colorf.White, Colorf.White);
        privacyTab.SetVisible(false);
        UnityUIUtil.SetColors(privacyTabButton, Colorf.White, Colorf.White);
        aboutTab.SetVisible(false);
        UnityUIUtil.SetColors(aboutTabButton, Colorf.White, Colorf.White);
        panel.SetVisible(true);
        UnityUIUtil.SetColors(button, Colorf.LightGreen, Colorf.LightGreen);
    }

    void on_prefs_tab() {
        set_to_tab(prefsTab, prefsTabButton);
    }
    void on_privacy_tab() {
        set_to_tab(privacyTab, privacyTabButton);
    }
    void on_about_tab() {
        set_to_tab(aboutTab, aboutTabButton);
    }




    void on_unity_privacy_policy()
    {
        Application.OpenURL("https://unity3d.com/legal/privacy-policy");
    }


    void on_open_cotangent_site()
    {
        Application.OpenURL("https://www.cotangent.io");
    }

    void on_open_gradientspace_site()
    {
        Application.OpenURL("https://www.gradientspace.com");
    }


    void on_dismiss()
    {
        base.TransitionVisibility(false);
    }

}



