#pragma warning disable 414
using System;
using System.Linq;
using System.Threading;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using f3;
using gs;
using cotangent;

public class EditSettingsDialogSetup : MonoBehaviour
{
	Button saveButton;
    Button cancelButton;
    Text titleText;
    Text errorText;

    TMP_InputField settingsText;

    bool machine_changed = false;
    MachinePreset preset;
    public MachinePreset Preset {
        get { return preset; }
        set { preset = value; machine_changed = true; }
    }


    // Use this for initialization
    public void Start()
	{
        saveButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "Save", on_save_clicked);
        cancelButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "Cancel", on_cancel_clicked);

        UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "CloseButton", destroy_dialog);

        GameObject textfieldGO = this.gameObject.FindChildByName("TextArea", true);
        settingsText = textfieldGO.GetComponent<TMP_InputField>();

        titleText = UnityUIUtil.FindTextAndSet(this.gameObject, "Title", "Edit Preset");
        errorText = UnityUIUtil.FindTextAndSet(this.gameObject, "ErrorMessage", " ");

        CC.OnPresetSelectionModified += CC_OnPresetSelectionModified;
        CC.OnPrinterSelectionModified += CC_OnPrinterSelectionModified;
    }

    // close dialog if we change printer or preset
    private void CC_OnPrinterSelectionModified(Manufacturer newMfg, MachineModel newModel) {
        destroy_dialog();
    }
    private void CC_OnPresetSelectionModified(MachinePreset newPreset) {
        destroy_dialog();
    }


    public void Update()
    {
        // in theory we could re-use dialog but currently this only happens once...
        if (machine_changed && preset != null) {


            var save_culture = Thread.CurrentThread.CurrentCulture;
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            string json = System.IO.File.ReadAllText(Preset.SourcePath);
            Thread.CurrentThread.CurrentCulture = save_culture;

            titleText.text = "Edit Preset - " + preset.Settings.Identifier;
            settingsText.text = json;
            machine_changed = false;
        }
    }



    void on_save_clicked()
    {
        if (preset == CC.PrinterDB.ActiveMachine.DefaultPreset) {
            CotangentUI.ShowModalConfirmDialog("Change Defaults?",
                "Are you sure you want to change the Machine Defaults? <b>You cannot undo this change</b>. Use the <i>Derive New Preset...</i> option in the Presets List to save the current settings as a new Preset",
                "Update Defaults", "Cancel", null,
                (obj) => { do_save(); }, null);
        } else {
            do_save();
        }
    }


    void do_save()
    {
        string curText = settingsText.text;

        SettingsSerializer serializer = new SettingsSerializer();
        if (serializer.ValidateSettingsJson(curText) == false) {
            errorText.text = "Invalid Formatting!";
            return;
        }
        errorText.text = "";

        serializer.UpdateSettingsFromJson(CC.PrinterDB.MachineDB, Preset, curText, false);
        CC.PrinterDB.SelectPreset(Preset, true);
    }


    void on_cancel_clicked()
    {
        destroy_dialog();
    }


    void destroy_dialog()
    {
        CC.OnPresetSelectionModified -= CC_OnPresetSelectionModified;
        CC.OnPrinterSelectionModified -= CC_OnPrinterSelectionModified;

        CC.ActiveContext.RegisterNextFrameAction(() => { GameObject.Destroy(this.gameObject); });
    }


    /// <summary>
    /// Launch dialog prefab and attach instance of this script to it
    /// </summary>
    public static void ShowEditCurrentPresetDialog()
    {
        if (CC.PrinterDB.ActivePreset == null)
            return;

        GameObject dialog = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("EditSettingsDialog"));
        EditSettingsDialogSetup script = dialog.AddComponent<EditSettingsDialogSetup>();
        script.Preset = CC.PrinterDB.ActivePreset;

        CotangentUI.MainUICanvas.AddChild(dialog, false);
    }




}
