using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;

namespace gs
{
    public class Manufacturer
    {
        public string UUID;
        public string Name;
    }

    public class MachineModel
    {
        public string UUID;
        public string Name;

        public List<MachinePreset> Presets = new List<MachinePreset>();
        public MachinePreset DefaultPreset;
    }


    public class MachinePreset
    {
        public string UUID;
        public PlanarAdditiveSettings Settings;
        public string SourcePath;

        public MachinePreset(PlanarAdditiveSettings settings, string sourcePath = "")
        {
            UUID = System.Guid.NewGuid().ToString();   // [RMS] we don't have uuids for settings?
            Settings = settings;
            SourcePath = sourcePath;
        }

        public MachinePreset Clone()
        {
            return new MachinePreset(
                Settings.CloneAs<PlanarAdditiveSettings>(), SourcePath);
        }

    }


}
