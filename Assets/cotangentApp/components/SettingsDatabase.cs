using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using gs;
using f3;

namespace cotangent
{
    public class SettingsDatabase
    {
        public MachineDatabase MachineDB;

        public Manufacturer ActiveManufacturer;
        public MachineModel ActiveMachine;
        public MachinePreset ActivePreset;


        List<Manufacturer> CachedManufacturers;
        List<Manufacturer> DisabledManufacturers = new List<Manufacturer>();


        public void Initialize()
        {
            MachineDB = new MachineDatabase();
            ActiveManufacturer = MachineDB.Manufacturers.First();
            ActiveMachine = MachineDB.ModelsForManufacturer(ActiveManufacturer).First();
            ActivePreset = MachineDB.DefaultPresetForModel(ActiveMachine);

            RestorePreferences();
            //OnPrinterSelectionModified?.Invoke(ActiveManufacturer, ActiveMachine);
            //OnPresetSelectionModified?.Invoke(ActivePreset);
        }


        public event EventHandler OnManufacturerListModified;

        public delegate void PrinterSelectionModifiedEvent(Manufacturer newMfg, MachineModel newModel);
        public event PrinterSelectionModifiedEvent OnPrinterSelectionModified;

        public delegate void PresetSelectionModifiedEvent(MachinePreset newPreset);
        public event PresetSelectionModifiedEvent OnPresetSelectionModified;




        public void EnableManufacturer(KnownManufacturerInfo info)
        {
            Manufacturer mfg = MachineDB.FindManufacturerByUUID(info.uuid);
            if (mfg == null) {
                MachineDB.CreateManufacturer(info.name, info.uuid, info.default_uuid);
                OnManufacturerListModified(this, EventArgs.Empty);
            }
            CachedManufacturers = null;
        }

        public void DisableManufacturer(KnownManufacturerInfo info)
        {
            Manufacturer mfg = MachineDB.FindManufacturerByUUID(info.uuid);
            if (mfg != null && DisabledManufacturers.Contains(mfg) == false) {
                DisabledManufacturers.Add(mfg);
                CachedManufacturers.Remove(mfg);
                OnManufacturerListModified(this, EventArgs.Empty);
            }
        }

        public bool IsDisabledManufacturer(Manufacturer mfg) {
            return DisabledManufacturers.Contains(mfg);
        }


        public void SelectManufacturer(Manufacturer mfg)
        {
            if (mfg != ActiveManufacturer) {
                ActiveManufacturer = mfg;
                // [TODO] should remember this per-mfg
                var newMachine = MachineDB.ModelsForManufacturer(mfg)[0];
                SelectMachine(newMachine);
            }
        }
        public void SelectMachine(MachineModel machine)
        {
            if (machine != ActiveMachine) {
                ActiveMachine = machine;

                // [TODO] should remember this per-machine
                MachinePreset defaultPreset = MachineDB.DefaultPresetForModel(ActiveMachine);
                SelectPreset(defaultPreset);

                OnPrinterSelectionModified?.Invoke(ActiveManufacturer, ActiveMachine);
            }
        }


        public void CreateDerivedMachine()
        {
            MachineModel activeModel = ActiveMachine;

            MachinePreset defaultPreset = MachineDB.DefaultPresetForModel(ActiveMachine);

            CotangentUI.GetStringFromDialog("Create New Machine",
                "Enter the name of new Machine to be derived from \"" + activeModel.Name + "\"", null,
                (name) => { return string.IsNullOrEmpty(name) == false && name.Contains('\"') == false; },
                (name, obj) => {
                    try {
                        MachineModel newModel = MachineDB.CreateDerivedMachine(ActiveManufacturer, ActiveMachine, defaultPreset, name);
                        OnPrinterSelectionModified?.Invoke(null, null);
                        SelectMachine(newModel);
                    } catch (Exception e) {
                        DebugUtil.Log("CreateDerivedMachine:  " + e.Message);
                    }
                },
                () => {
                    SelectMachine(ActiveMachine);
                }
            );
        }






        public void SelectPreset(MachinePreset preset, bool bForceUpdate = false)
        {
            if (preset != ActivePreset || bForceUpdate) {
                ActivePreset = preset;
                CC.Settings.UpdateFromSettings(ActivePreset.Settings);
                UpdatePreferences();
                OnPresetSelectionModified?.Invoke(preset);
            }
        }

        public void CreateDerivedPreset()
        {
            MachineModel activeModel = ActiveMachine;

            MachinePreset basePreset = ActivePreset.Clone();
            CC.Settings.WriteToSettings(basePreset.Settings);

            CotangentUI.GetStringFromDialog("Create New Preset",
                "Enter the name of new Preset to be derived from \"" + basePreset.Settings.Identifier + "\"", null,
                (name) => { return string.IsNullOrEmpty(name) == false && name.Contains('\"') == false; },
                (name, obj) => {
                    MachinePreset derived = MachineDB.CreateDerivedPreset(activeModel, basePreset, name);
                    SelectPreset(derived);
                    // will cause refresh of presets list
                    OnPrinterSelectionModified?.Invoke(ActiveManufacturer, ActiveMachine);
                },
                () => {
                    SelectPreset(basePreset);
                }
            );
        }


        public void SaveActivePreset()
        {
            if (ActivePreset != null) {
                CC.Settings.WriteToSettings(ActivePreset.Settings);
                MachineDB.StorePreset(ActivePreset);
                SelectPreset(ActivePreset, true);
            }
        }


        public void DeleteActivePreset()
        {
            if (ActivePreset != null && ActivePreset != ActiveMachine.DefaultPreset) {
                var deletePreset = ActivePreset;
                SelectPreset(ActiveMachine.DefaultPreset, true);
                MachineDB.DeletePreset(ActiveMachine, deletePreset);
                // refresh presets list
                OnPrinterSelectionModified?.Invoke(ActiveManufacturer, ActiveMachine);
            }
        }





        protected void UpdatePreferences()
        {
            FPlatform.SetPrefsString("LastManufacturer", ActiveManufacturer.UUID);
            FPlatform.SetPrefsString("LastMachine", ActiveMachine.UUID);
            FPlatform.SetPrefsString("LastPreset", ActivePreset.Settings.Identifier);

            StringBuilder disabledList = new StringBuilder();
            foreach (var mfg in DisabledManufacturers) {
                disabledList.Append(mfg.UUID); disabledList.Append(";");
            };
            FPlatform.SetPrefsString("DisabledManufacturers", disabledList.ToString());
        }

        protected void RestorePreferences()
        {
            // do this first because setting preset will replace this
            DisabledManufacturers = new List<Manufacturer>();
            string disabled_list = FPlatform.GetPrefsString("DisabledManufacturers", "");
            string[] disabled_uuids = disabled_list.Split(';');
            foreach (var uuid in disabled_uuids) {
                Manufacturer mfg = FindManufacturerByUUID(uuid);
                if (mfg != null)
                    DisabledManufacturers.Add(mfg);
            }


            string last_mfg_uuid = FPlatform.GetPrefsString("LastManufacturer", ActiveManufacturer.UUID);
            string last_machine_uuid = FPlatform.GetPrefsString("LastMachine", ActiveMachine.UUID);
            string last_preset_id = FPlatform.GetPrefsString("LastPreset", ActivePreset.Settings.Identifier);

            if (last_preset_id != ActivePreset.UUID) {
                Manufacturer mfg = MachineDB.FindManufacturerByUUID(last_mfg_uuid);
                if (mfg != null) {
                    SelectManufacturer(mfg);
                    MachineModel model = MachineDB.FindModelByUUID(mfg, last_machine_uuid);
                    if( model != null ) {
                        SelectMachine(model);
                        MachinePreset preset = MachineDB.FindPresetByIdentifier(model, last_preset_id);
                        if (preset != null) {
                            CC.ActiveContext.RegisterNthFrameAction(2, () => {
                                SelectPreset(preset, true);
                            });
                        }
                    }
                }
            } else {
                CC.ActiveContext.RegisterNthFrameAction(2, () => {
                    SelectPreset(ActivePreset, true);
                });
            }
        }




        public IReadOnlyList<Manufacturer> Manufacturers()
        {
            if (CachedManufacturers == null) {
                CachedManufacturers = new List<Manufacturer>();
                foreach (var mfg in MachineDB.Manufacturers) {
                    if (DisabledManufacturers.Contains(mfg) == false)
                        CachedManufacturers.Add(mfg);
                }
            }
            return CachedManufacturers;
        }

        public IReadOnlyList<MachineModel> ModelsForManufacturer(Manufacturer mfg)
        {
            return MachineDB.ModelsForManufacturer(mfg);
        }

        public Manufacturer FindManufacturerByUUID(string uuid)
        {
            return MachineDB.FindManufacturerByUUID(uuid);
        }

        public MachineModel FindActiveMfgModelByUUID(string uuid)
        {
            return MachineDB.FindModelByUUID(ActiveManufacturer, uuid);
        }

        public IReadOnlyList<MachinePreset> PresetsForModel(MachineModel model)
        {
            return MachineDB.PresetsForModel(model);
        }

        public MachinePreset FindActiveMachinePresetByUUID(string uuid)
        {
            return MachineDB.FindPresetByUUID(ActiveMachine, uuid);
        }



        public void SavePreferencesHint()
        {
            UpdatePreferences();
        }

    }






    /*
     * Should move elsewhere?
     */


    public struct KnownManufacturerInfo
    {
        public string name;
        public string uuid;
        public string default_uuid;

        public static List<KnownManufacturerInfo> LoadManufacturers()
        {
            List<KnownManufacturerInfo> result = new List<KnownManufacturerInfo>();

            string mfg_file_text = FResources.LoadText("printers/manufacturers");
            string[] rows = mfg_file_text.Split('\n', '\r');
            foreach (string rowdata in rows) {
                string[] values = rowdata.Split(',');
                if (values.Length < 3)
                    continue;
                KnownManufacturerInfo mi = new KnownManufacturerInfo();
                mi.name = values[0];
                mi.uuid = values[1];
                mi.default_uuid = values[2];
                result.Add(mi);
            }

            return result;
        }
    }


}
