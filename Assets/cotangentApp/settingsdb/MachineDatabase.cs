using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using f3;
using g3;
using gs.info;

namespace gs
{
    public class MachineDatabase
    {
        //Manufacturer unknownMfg;
        List<Manufacturer> manufacturers;
        public IReadOnlyList<Manufacturer> Manufacturers {
            get { return manufacturers; }
        }


        public Manufacturer FindManufacturerByUUID(string uuid) {
            return manufacturers.Find((mfg) => { return mfg.UUID.Equals(uuid, StringComparison.OrdinalIgnoreCase); });
        }


        public MachineModel CreateManufacturer(string mfgName, string mfgUUID, string defaultMachineUUID)
        {
            Manufacturer new_mfg = new Manufacturer() {
                Name = mfgName, UUID = mfgUUID
            };
            add_manufacturer(new_mfg);

            MachinePreset preset = new MachinePreset(new GenericPrinterSettings(mfgName, mfgUUID, defaultMachineUUID));

            MachineModel model = new MachineModel() {
                Name = "Unknown", UUID = defaultMachineUUID,
                Presets = new List<MachinePreset>() { preset }
            };
            add_machine(new_mfg, model);

            SettingsSerializer serializer = new SettingsSerializer();
            string machinePath = serializer.CreateNewSettingsFolder(this, new_mfg, model, cotangent.CCOptions.SettingsDBPath);

            preset.Settings.Identifier = "Defaults";
            preset.Settings.BaseMachine.ManufacturerName = mfgName;
            preset.Settings.BaseMachine.ManufacturerUUID = mfgUUID;
            preset.Settings.BaseMachine.ModelIdentifier = "(Unknown)";
            preset.Settings.BaseMachine.ModelUUID = defaultMachineUUID;

            preset.SourcePath = Path.Combine(machinePath, "Default.txt");
            serializer.StoreSettings(this, preset, true);

            model.Presets.Add(preset);
            model.DefaultPreset = preset;

            return model;
        }


        List<MachineModel> noMachines = new List<MachineModel>();
        Dictionary<Manufacturer, List<MachineModel>> models_by_mfg;
        
        public IReadOnlyList<MachineModel> ModelsForManufacturer(Manufacturer mfg)
        {
            List<MachineModel> list;
            if (models_by_mfg.TryGetValue(mfg, out list))
                return list;
            return noMachines;
        }

        public MachineModel FindModelByUUID(Manufacturer mfg, string uuid)
        {
            List<MachineModel> list;
            if (models_by_mfg.TryGetValue(mfg, out list)) {
                return list.Find((machine) => { return machine.UUID.Equals(uuid, StringComparison.OrdinalIgnoreCase); });
            }
            return null;
        }


        public MachineModel CreateDerivedMachine(Manufacturer mfg, MachineModel derivedFrom, MachinePreset initialPreset, string name)
        {
            string existingMachineDir = Path.GetDirectoryName(initialPreset.SourcePath);
            string useMfgDir = Directory.GetParent(existingMachineDir).FullName;

            string newMachineDir = Path.Combine(useMfgDir, name);
            if ( Directory.Exists(newMachineDir) ) 
                throw new Exception("ALREADY_EXISTS");
            try {
                Directory.CreateDirectory(newMachineDir);
            } catch (Exception) {
                throw new Exception("CANNOT_CREATE");
            }
            if (! Directory.Exists(newMachineDir)) 
                throw new Exception("ALREADY_EXISTS");

            string newPresetPath = Path.Combine(newMachineDir, "Default.txt");

            MachineModel model = new MachineModel() {
                Name = name,
                UUID = System.Guid.NewGuid().ToString()
            };
            add_machine(mfg, model, false);

            MachinePreset newPreset = new MachinePreset(initialPreset.Settings.CloneAs<PlanarAdditiveSettings>(), newPresetPath);
            newPreset.Settings.Identifier = "Defaults";

            newPreset.Settings.BaseMachine.ManufacturerName = mfg.Name;
            newPreset.Settings.BaseMachine.ManufacturerUUID = mfg.UUID;
            newPreset.Settings.BaseMachine.ModelIdentifier = model.Name;
            newPreset.Settings.BaseMachine.ModelUUID = model.UUID;

            SettingsSerializer serializer = new SettingsSerializer();
            serializer.StoreSettings(this, newPreset, true);

            model.Presets.Add(newPreset);
            model.DefaultPreset = newPreset;

            return model;
        }





        public IReadOnlyList<MachinePreset> PresetsForModel(MachineModel model)
        {
            return model.Presets;
        }

        public MachinePreset DefaultPresetForModel(MachineModel model)
        {
            return model.DefaultPreset;
        }

        public MachinePreset FindPresetByUUID(MachineModel model, string uuid)
        {
            foreach (var preset in model.Presets) {
                if (preset.UUID.Equals(uuid, StringComparison.OrdinalIgnoreCase))
                    return preset;
            }
            return null;
        }

        public MachinePreset FindPresetByIdentifier(MachineModel model, string identifier)
        {
            foreach (var preset in model.Presets) {
                if (preset.Settings.Identifier.Equals(identifier, StringComparison.OrdinalIgnoreCase))
                    return preset;
            }
            return null;
        }


        public MachinePreset CreateDerivedPreset(MachineModel model, MachinePreset derivedFrom, string name)
        {
            string machineDir = Path.GetDirectoryName(derivedFrom.SourcePath);
            string newPath = Path.Combine(machineDir, name + ".txt");

            MachinePreset newPreset = new MachinePreset(derivedFrom.Settings.CloneAs<PlanarAdditiveSettings>(), newPath);
            newPreset.Settings.Identifier = name;
            SettingsSerializer serializer = new SettingsSerializer();
            serializer.StoreSettings(this, newPreset, true);

            //model.Presets.Add(newPreset);
            model.Presets.Insert(1, newPreset);
            return newPreset;
        }



        public void StorePreset(MachinePreset preset)
        {
            if (preset.SourcePath.Length == 0) {
                DebugUtil.Log(2, "MachineDatabase.StorePreset: preset with id " + preset.Settings.Identifier + " has no path!");
                return;
            }
            SettingsSerializer serializer = new SettingsSerializer();
            serializer.StoreSettings(this, preset, false);
        }


        public void DeletePreset(MachineModel model, MachinePreset preset)
        {
            if ( preset.SourcePath.Length > 0 ) {
                File.Delete(preset.SourcePath);
            }
            model.Presets.Remove(preset);
        }


        public MachineDatabase()
        {
            reset_db();
            initialize_db();

            // fallback - we must have some printers or everything else is very bad
            if (models_by_mfg.Count == 0)
                populate_defaults();
        }



        void reset_db()
        {
            manufacturers = new List<Manufacturer>();
            models_by_mfg = new Dictionary<Manufacturer, List<MachineModel>>();
        }


        void initialize_db()
        {
            // if settings database does not exist, generate it
            if ( Directory.Exists(cotangent.CCOptions.SettingsDBPath) == false ) {
                populate_defaults();
                SettingsSerializer serializer = new SettingsSerializer();
                serializer.GenerateSettingsFolder(this, cotangent.CCOptions.SettingsDBPath);
                reset_db();
            } 

            try {
                DebugUtil.Log("Reading settings database from {0}", cotangent.CCOptions.SettingsDBPath);
                populate_from_disk(cotangent.CCOptions.SettingsDBPath);
            } catch (Exception e) {
                DebugUtil.Log("MachineDatabase: fatal exception restoring : " + e.Message);
                if (FPlatform.InUnityEditor())
                    throw;
            }

            // [TODO] sync w/ new known settings?
            List<MachinePreset> new_defaults = populate_defaults();
            foreach (var preset in new_defaults) {
                SettingsSerializer serializer = new SettingsSerializer();
                Manufacturer mfg = FindManufacturerByUUID(preset.Settings.BaseMachine.ManufacturerUUID);
                MachineModel model = FindModelByUUID(mfg, preset.Settings.BaseMachine.ModelUUID);
                string machinePath = serializer.CreateNewSettingsFolder(this, mfg, model, cotangent.CCOptions.SettingsDBPath);
                preset.SourcePath = Path.Combine(machinePath, preset.Settings.Identifier + ".txt");
                serializer.StoreSettings(this, preset, true);
            }

        }



        void add_manufacturer(Manufacturer mfg)
        {
            manufacturers.Add(mfg);
            models_by_mfg[mfg] = new List<MachineModel>();
        }
        void add_machine(Manufacturer mfg, MachineModel model, bool bUpdateSort = false)
        {
            List<MachineModel> models = models_by_mfg[mfg];
            models.Add(model);
            if (bUpdateSort) 
                sort_models(models);
        }
        void sort_models(List<MachineModel> models)
        {
            models.Sort((a, b) => {
                if (a.Name.Contains("Unknown"))
                    return 1;
                else if (b.Name.Contains("Unknown"))
                    return -1;
                else
                    return a.Name.CompareTo(b.Name);
            });
        }
        void sort_all_model_lists()
        {
            foreach (var pair in models_by_mfg)
                sort_models(pair.Value);
        }



        void sort_presets(MachineModel model)
        {
            model.Presets.Sort((a, b) => {
                if (a == model.DefaultPreset)
                    return -1;
                else if (b == model.DefaultPreset)
                    return 1;
                else
                    return a.Settings.Identifier.CompareTo(b.Settings.Identifier);
            });
        }
        void sort_all_preset_lists()
        {
            foreach (var pair in models_by_mfg)
                foreach (var machine in pair.Value)
                    sort_presets(machine);
        }


        /// <summary>
        /// restore machine database from directories and text files found under root path
        /// </summary>
        void populate_from_disk(string sPath)
        {
            // check all folders for settings files
            // [TODO] this could be multi-threaded, or happen in background? 
            string[] folders = Directory.GetDirectories(sPath);
            foreach ( string mfgFolder in folders ) {

                // scan through subdirs for settings files
                List<MachinePreset> machinePresets = new List<MachinePreset>();
                if ( read_manufacturer(mfgFolder, ref machinePresets) == false ) {
                    DebugUtil.Log(2, "MachineDatabase: did not find any valid settings files in folder " + mfgFolder);
                    continue;
                } 

                foreach ( var preset in machinePresets ) {
                    //foreach ( var info in list ) {
                        // find or create manufacturer
                        Manufacturer mfg = FindManufacturerByUUID(preset.Settings.BaseMachine.ManufacturerUUID);
                        if (mfg == null) {
                            mfg = new Manufacturer() {
                                Name = preset.Settings.BaseMachine.ManufacturerName,
                                UUID = preset.Settings.BaseMachine.ManufacturerUUID
                            };
                            add_manufacturer(mfg);
                        }

                        // find or create machine
                        MachineModel model = FindModelByUUID(mfg, preset.Settings.BaseMachine.ModelUUID);
                        if ( model == null ) {
                            model = new MachineModel() {
                                Name = preset.Settings.BaseMachine.ModelIdentifier,
                                UUID = preset.Settings.BaseMachine.ModelUUID
                            };
                            add_machine(mfg, model);
                        }

                        // add as preset to machine
                        model.Presets.Add(preset);
                    //}
                }

            }

            sort_all_model_lists();

            foreach ( Manufacturer mfg in Manufacturers ) {
                foreach ( MachineModel model in ModelsForManufacturer(mfg) ) {
                    set_default_preset(model);
                }
            }

            sort_all_preset_lists();
        }


        /// <summary>
        /// choose a default for this MachineModel. 
        /// </summary>
        void set_default_preset(MachineModel model)
        {
            if (model.Presets.Count == 0)
                throw new Exception("MachinDatabase.set_default_settings: no presets to choose from!");

            MachinePreset defaultPreset =
                model.Presets.Find((s) => { return s.Settings.Identifier.Equals("Defaults", StringComparison.OrdinalIgnoreCase); });
            if (defaultPreset != null ) {
                model.DefaultPreset = defaultPreset;
                return;
            }

            model.DefaultPreset = model.Presets[0];
        }


        /// <summary>
        /// Scan through a top-level 'manufacturer' folder for a list of machine folders,
        /// and then restore any settings files from each of those.
        /// </summary>
        bool read_manufacturer(string mfgPath, ref List<MachinePreset> presetsForModels )
        {
            SettingsSerializer serializer = new SettingsSerializer();

            string[] folders = Directory.GetDirectories(mfgPath);
            foreach (string machineFolder in folders) {

                List<PlanarAdditiveSettings> settings;
                List<string> sourceFilePaths;
                if ( serializer.RestoreFromFolder(machineFolder, out settings, out sourceFilePaths) == false ) {
                    DebugUtil.Log(2, "MachineDatabase: did not find any valid settings files in folder " + machineFolder);
                    continue;
                }

                for ( int i = 0; i < settings.Count; ++i ) {
                    MachinePreset preset = new MachinePreset(settings[i], sourceFilePaths[i]);
                    presetsForModels.Add(preset);
                }
            }

            return (presetsForModels.Count > 0);
        }


        

        
        List<MachinePreset> populate_defaults()
        {
            List<MachinePreset> new_defaults = new List<MachinePreset>();

            register_new_mfg_machines("Makerbot", Makerbot.UUID, MakerbotSettings.EnumerateDefaults(), new_defaults);
            register_new_mfg_machines("Monoprice", Monoprice.UUID, MonopriceSettings.EnumerateDefaults(), new_defaults);
            register_new_mfg_machines("Printrbot", Printrbot.UUID, PrintrbotSettings.EnumerateDefaults(), new_defaults);
            register_new_mfg_machines("RepRap", RepRap.UUID, RepRapSettings.EnumerateDefaults(), new_defaults);
            register_new_mfg_machines("Flashforge", Flashforge.UUID, FlashforgeSettings.EnumerateDefaults(), new_defaults);
            register_new_mfg_machines("Prusa", Prusa.UUID, PrusaSettings.EnumerateDefaults(), new_defaults);

            return new_defaults;
        }

        void register_new_mfg_machines(string mfgName, string mfgUUID, IEnumerable<SingleMaterialFFFSettings> presets, List<MachinePreset> new_defaults)
        {
            Manufacturer mfg = FindManufacturerByUUID(mfgUUID);
            if ( mfg == null) {
                mfg = new Manufacturer() { Name = mfgName, UUID = mfgUUID };
                add_manufacturer(mfg);
            }

            foreach (var preset in presets) {
                if (FindModelByUUID(mfg, preset.Machine.ModelUUID) == null) {
                    MachineModel newModel = new MachineModel() {
                        Name = preset.Machine.ModelIdentifier, UUID = preset.Machine.ModelUUID,
                        Presets = new List<MachinePreset>() { new MachinePreset(preset) }
                    };
                    add_machine(mfg, newModel);
                    set_default_preset(newModel);
                    new_defaults.Add(newModel.DefaultPreset);
                }
            }

        }


    }
}
