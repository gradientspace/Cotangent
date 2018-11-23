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
    public class PrinterListPanelSetup : MonoBehaviour
    {
        Dropdown mfgDropDown;
        Dropdown modelDropDown;
        Dropdown presetDropDown;
        bool panel_requires_update = true;
        bool force_mfg_update = false;

        Button saveButton;
        Button editButton;
        Button resetButton;
        Button deleteButton;

        public void Start()
        {
            CC.OnPrinterSelectionModified += CC_OnPrinterSelectionModified;
            CC.OnPresetSelectionModified += CC_OnPresetSelectionModified;
            CC.Settings.OnSettingModified += Settings_OnSettingModified;
            CC.PrinterDB.OnManufacturerListModified += OnManufacturerListModified;

            mfgDropDown = UnityUIUtil.FindDropDownAndAddHandlers("Manufacturer",
                () => { return find_mfg_index(CC.PrinterDB.ActiveManufacturer); },
                (intValue) => { select_mfg_by_index(intValue); } );

            modelDropDown = UnityUIUtil.FindDropDownAndAddHandlers("Model",
                () => { return find_model_index(CC.PrinterDB.ActiveMachine); },
                (intValue) => { select_model_by_index(intValue); });

            presetDropDown = UnityUIUtil.FindDropDownAndAddHandlers("Preset",
                () => { return find_preset_index(CC.PrinterDB.ActivePreset); },
                (intValue) => { select_preset_by_index(intValue); });

            saveButton = UnityUIUtil.FindButtonAndAddClickHandler("PresetSaveButton", on_save_preset_clicked);
            UnityUIUtil.SetColors(saveButton, CotangentUI.ModifiedSettingColor, CotangentUI.DisabledButtonColor);
            resetButton = UnityUIUtil.FindButtonAndAddClickHandler("PresetResetButton", on_reset_preset_clicked);
            UnityUIUtil.SetColors(resetButton, CotangentUI.ModifiedSettingColor, CotangentUI.DisabledButtonColor);
            deleteButton = UnityUIUtil.FindButtonAndAddClickHandler("PresetDeleteButton", on_delete_preset_clicked);
            UnityUIUtil.SetColors(saveButton, CotangentUI.ModifiedSettingColor, CotangentUI.DisabledButtonColor);
            editButton = UnityUIUtil.FindButtonAndAddClickHandler("PresetEditButton", on_edit_preset_clicked);

            panel_requires_update = true;
        }

        private void Settings_OnSettingModified(PrintSettings settings) {
            //if ( saveButton != null )
            //    saveButton.enabled = true;
        }
        private void CC_OnPrinterSelectionModified(Manufacturer newMfg, MachineModel newModel) {
            panel_requires_update = true;
        }
        private void CC_OnPresetSelectionModified(MachinePreset preset) {
            panel_requires_update = true;
        }

        private void OnManufacturerListModified(object sender, EventArgs ignore) {
            panel_requires_update = true;
            force_mfg_update = true;
        }

        public void Update()
        {
            saveButton.interactable = CC.Settings.HasModifiedValues;
            resetButton.interactable = saveButton.interactable;
            if (panel_requires_update)
                update_panel();
        }



        Manufacturer currentMfg = null;
        List<string> mfgInfo = new List<string>();

        MachineModel currentModel = null;
        List<string> modelInfo = new List<string>();

        MachinePreset currentPreset = null;
        List<string> presetInfo = new List<string>();


        bool validate_mfg_list()
        {
            IReadOnlyList<Manufacturer> allmfg = CC.PrinterDB.Manufacturers();
            Manufacturer selectedMfg = CC.PrinterDB.ActiveManufacturer;

            // -1 here because first item is "create new..."
            if ((mfgDropDown.options.Count-1) == allmfg.Count && mfgDropDown.options.Count == mfgInfo.Count)
                return true;


            mfgDropDown.options.Clear();
            List<string> options = new List<string>();
            mfgInfo = new List<string>();
            int selected = -1;

            mfgInfo.Add("-");
            options.Add("Select Manufacturers...");


            for ( int k = 0; k < allmfg.Count; ++k ) {
                options.Add(allmfg[k].Name);
                mfgInfo.Add(allmfg[k].UUID);
                if (allmfg[k] == selectedMfg)
                    selected = k+1;
            }
            if (selected < 1)
                selected = 1;

            mfgDropDown.ClearOptions();
            mfgDropDown.AddOptions(options);
            mfgDropDown.value = selected;
            return false;
        }

        void select_mfg(Manufacturer mfg)
        {
            for ( int k = 0; k < mfgDropDown.options.Count; ++k ) {
                if ( mfgDropDown.options[k].text == mfg.Name ) {
                    mfgDropDown.value = k;
                }
            }
        }

        int find_mfg_index(Manufacturer mfg)
        {
            if (mfg == null)
                return 0;
            for ( int k = 0; k < mfgInfo.Count; ++k ) {
                if (mfg.UUID.Equals(mfgInfo[k], StringComparison.OrdinalIgnoreCase))
                    return k;
            }
            return 0;
        }

        Manufacturer select_mfg_by_index(int idx)
        {
            if (idx == 0) {
                var go = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("AddManufacturersDialog"));
                CotangentUI.PrintUICanvas.AddChild(go, false);
                return null;
            }

            string uuid = mfgInfo[idx];
            Manufacturer mfg = CC.PrinterDB.FindManufacturerByUUID(uuid);
            if ( mfg != null ) {
                CC.PrinterDB.SelectManufacturer(mfg);
            }
            return mfg;
        }



        bool validate_machine_list()
        {
            IReadOnlyList<MachineModel> models = CC.PrinterDB.ModelsForManufacturer(currentMfg);
            MachineModel selectedModel = CC.PrinterDB.ActiveMachine;

            // -1 here because first item is "create new..."
            if ( (modelDropDown.options.Count-1) == models.Count && modelDropDown.options.Count == modelInfo.Count)
                return true;

            modelDropDown.options.Clear();
            modelInfo = new List<string>();
            List<string> options = new List<string>();
            int selected = -1;

            modelInfo.Add("-");
            options.Add("Derive New Printer...");

            for (int k = 0; k < models.Count; ++k) {
                modelInfo.Add(models[k].UUID);
                options.Add(models[k].Name);
                if (models[k] == selectedModel)
                    selected = k+1;
            }
            if (selected < 1)
                selected = 1;

            modelDropDown.ClearOptions();
            modelDropDown.AddOptions(options);
            modelDropDown.value = selected;
            return false;
        }


        void select_model(MachineModel model)
        {
            for (int k = 0; k < modelDropDown.options.Count; ++k) {
                if (modelDropDown.options[k].text == model.Name) {
                    modelDropDown.value = k;
                }
            }
        }

        int find_model_index(MachineModel model)
        {
            if (model == null)
                return 0;
            for (int k = 0; k < modelInfo.Count; ++k) {
                if (model.UUID.Equals(modelInfo[k], StringComparison.OrdinalIgnoreCase))
                    return k;
            }
            return 0;
        }

        MachineModel select_model_by_index(int idx)
        {
            if (idx == 0) {
                CC.PrinterDB.CreateDerivedMachine();
                return null;
            }

            string uuid = modelInfo[idx];
            MachineModel model = CC.PrinterDB.FindActiveMfgModelByUUID(uuid);
            if (model != null) {
                CC.PrinterDB.SelectMachine(model);
            }
            return model;
        }







        bool validate_preset_list(MachinePreset toSelect)
        {
            IReadOnlyList<MachinePreset> presets = CC.PrinterDB.PresetsForModel(currentModel);
            MachinePreset selectedPreset = CC.PrinterDB.ActivePreset;

            deleteButton.interactable = (CC.PrinterDB.ActivePreset != CC.PrinterDB.ActiveMachine.DefaultPreset);

            // -1 here because first item is "create new..."
            if ( (presetDropDown.options.Count-1) == presets.Count && presetDropDown.options.Count == presetInfo.Count)
                return true;

            presetDropDown.options.Clear();
            presetInfo = new List<string>();
            List<string> options = new List<string>();

            presetInfo.Add("-");
            options.Add("Derive New Preset...");

            int selected = -1;
            for (int k = 0; k < presets.Count; ++k) {
                presetInfo.Add(presets[k].UUID);
                options.Add(presets[k].Settings.Identifier);
                if (presets[k] == toSelect)
                    selected = k+1;
            }
            if (selected < 1)
                selected = 1;

            presetDropDown.ClearOptions();
            presetDropDown.AddOptions(options);
            presetDropDown.value = selected;
            return false;
        }


        void select_preset(MachinePreset preset)
        {
            for (int k = 0; k < presetDropDown.options.Count; ++k) {
                if (presetDropDown.options[k].text == preset.Settings.Identifier) {
                    presetDropDown.value = k;
                }
            }
            deleteButton.interactable = (CC.PrinterDB.ActivePreset != CC.PrinterDB.ActiveMachine.DefaultPreset);
        }


        int find_preset_index(MachinePreset preset)
        {
            if (preset == null)
                return 0;
            for (int k = 0; k < presetInfo.Count; ++k) {
                if (preset.UUID.Equals(presetInfo[k], StringComparison.OrdinalIgnoreCase))
                    return k;
            }
            return 0;
        }

        MachinePreset select_preset_by_index(int idx)
        {
            if ( idx == 0 ) {
                CC.PrinterDB.CreateDerivedPreset();
                return null;
            }

            string uuid = presetInfo[idx];
            MachinePreset preset = CC.PrinterDB.FindActiveMachinePresetByUUID(uuid);
            if (preset != null) {
                CC.PrinterDB.SelectPreset(preset);
            }

            deleteButton.interactable = (CC.PrinterDB.ActivePreset != CC.PrinterDB.ActiveMachine.DefaultPreset);

            return preset;
        }




        private void update_panel()
        {
            Manufacturer newMfg = CC.PrinterDB.ActiveManufacturer;
            MachineModel newModel = CC.PrinterDB.ActiveMachine;
            MachinePreset newPreset = CC.PrinterDB.ActivePreset;

            if (force_mfg_update || newMfg != currentMfg ) { 
                if (validate_mfg_list() )
                    select_mfg(newMfg);
                currentMfg = newMfg;
                modelDropDown.options.Clear();  // force fetch of new models
            }
            if (newModel != currentModel) {
                if (validate_machine_list())
                    select_model(newModel);
                currentModel = newModel;
                presetDropDown.options.Clear();
            }
            if (newPreset != currentPreset) {
                if (validate_preset_list(newPreset))
                    select_preset(newPreset);
                currentPreset = newPreset;
            }

            panel_requires_update = false;
        }





        void on_save_preset_clicked()
        {
            if (CC.PrinterDB.ActivePreset == CC.PrinterDB.ActiveMachine.DefaultPreset) {
                CotangentUI.ShowModalConfirmDialog("Change Defaults?",
                    "Are you sure you want to change the Machine Defaults? <b>You cannot undo this change</b>. Use the <i>Derive New Preset...</i> option in the Presets List to save the current settings as a new Preset",
                    "Update Defaults", "Cancel", null,
                    (obj) => { CC.PrinterDB.SaveActivePreset(); }, null);
            } else {
                CC.PrinterDB.SaveActivePreset();
            }
        }
        void on_edit_preset_clicked()
        {
            EditSettingsDialogSetup.ShowEditCurrentPresetDialog();
        }

        void on_reset_preset_clicked()
        {
            CC.PrinterDB.SelectPreset(CC.PrinterDB.ActivePreset, true);
        }

        void on_delete_preset_clicked()
        {
            if (CC.PrinterDB.ActivePreset == CC.PrinterDB.ActiveMachine.DefaultPreset)
                return;

            CotangentUI.ShowModalConfirmDialog("Delete Preset?",
                "Are you sure you want to delete this Preset? <b>You cannot undo this change</b>",
                "Yes, Delete", "Cancel", null,
                (obj) => { CC.PrinterDB.DeleteActivePreset(); }, null);
        }

    }
}
