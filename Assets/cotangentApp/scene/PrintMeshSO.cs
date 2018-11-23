using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using f3;

namespace cotangent
{
    public class PrintMeshSettings
    {
        // WARNING: this class is serialized! Do not change enum constants!
        public int Version = 1;

        public enum ObjectTypes
        {
            Solid = 0, Support = 1, Cavity = 2, CropRegion = 3, Ignored = 4
        }

        public enum OpenMeshModes
        {
            Default = 0, Clipped = 1, Embedded = 2, Ignored = 3, 
        }

        public ObjectTypes ObjectType = ObjectTypes.Solid;
        public bool NoVoids = false;
        public bool OuterShellOnly = false;
        public OpenMeshModes OpenMeshMode = OpenMeshModes.Default;

        public double Clearance = 0;
        public double OffsetXY = 0;


        public PrintMeshSettings Clone() {
            return new PrintMeshSettings() {
                ObjectType = this.ObjectType,
                NoVoids = this.NoVoids,
                OuterShellOnly = this.OuterShellOnly,
                OpenMeshMode = this.OpenMeshMode,
                Clearance = this.Clearance,
                OffsetXY = this.OffsetXY
            };
        }


        public static gs.PrintMeshOptions.OpenPathsModes Convert(OpenMeshModes mode)
        {
            switch (mode) {
                case OpenMeshModes.Clipped: return gs.PrintMeshOptions.OpenPathsModes.Clipped;
                case OpenMeshModes.Embedded: return gs.PrintMeshOptions.OpenPathsModes.Embedded;
                case OpenMeshModes.Ignored: return gs.PrintMeshOptions.OpenPathsModes.Ignored;
                default:
                case OpenMeshModes.Default: return gs.PrintMeshOptions.OpenPathsModes.Default;
            }
        }


    }





    public class PrintMeshSO : DMeshSO
    {
        override public SOType Type { get { return CotangentTypes.PrintMesh; } }

        public PrintMeshSettings Settings = new PrintMeshSettings();

        public UpDirection UpDirection = UpDirection.ZUp;


        // file we got this SO from. Used for auto-refresh. 
        public string SourceFilePath = "";
        public bool AutoUpdateOnSourceFileChange = false;
        public long LastReadFileTimestamp = 0;


        public bool CanAutoUpdateFromSource()
        {
            return SourceFilePath != "" && LastReadFileTimestamp != 0;
        }


        protected override void duplicate_to(DMeshSO copy)
        {
            base.duplicate_to(copy);
            PrintMeshSO pcopy = copy as PrintMeshSO;
            if (pcopy != null) {
                pcopy.Settings = this.Settings.Clone();
                pcopy.UpDirection = this.UpDirection;

                pcopy.SourceFilePath = this.SourceFilePath;
                pcopy.LastReadFileTimestamp = this.LastReadFileTimestamp;
                pcopy.AutoUpdateOnSourceFileChange = this.AutoUpdateOnSourceFileChange;
            }
        }





    }



    static class PrintMeshSO_Serialization
    {

        const string AttrSourceFilePath = "SourceFilePath";

        public static bool Emit(SceneSerializer s, IOutputStream o, SceneObject gso)
        {
            PrintMeshSO so = gso as PrintMeshSO;
            o.AddAttribute(IOStrings.ASOType, so.Type.identifier);
            o.AddAttribute(AttrSourceFilePath, so.SourceFilePath);

            EmitPrintMeshSettings(s, o, so.Settings);

            SceneSerializerEmitTypesExt.EmitDMeshSO(s, o, so as DMeshSO);
            return true;
        }
        public static SceneObject Build(SOFactory factory, FScene scene, TypedAttribSet attributes)
        {
            PrintMeshSO so = new PrintMeshSO();
            factory.RestoreDMeshSO(scene, attributes, so);

            if (attributes.ContainsKey(AttrSourceFilePath))
                so.SourceFilePath = attributes[AttrSourceFilePath] as string;

            PrintMeshSettings settings = null;
            try {
                settings = RestorePrintMeshSettings(factory, attributes);
            } catch { }
            if (settings != null)
                so.Settings = settings;

            return so;
        }



        const string PrintSettingsStruct = "PrintSettings";

        const string AttrPrintSettingsVersion = "iVersion";

        const string AttrPrintSettingsObjectType = "iPrintObjectType";
        const string AttrPrintSettingsOpenMode = "iOpenMeshMode";
        const string AttrPrintSettingsNoVoids = "bNoVoids";
        const string AttrPrintSettingsShellOnly = "bShellOnly";
        const string AttrPrintSettingsClearanceXY = "fClearanceXY";
        const string AttrPrintSettingsOffsetXY = "fOffsetXY";

        public static void EmitPrintMeshSettings(this SceneSerializer s, IOutputStream o, PrintMeshSettings settings)
        {
            o.BeginStruct(PrintSettingsStruct);
            o.AddAttribute(AttrPrintSettingsVersion, settings.Version);

            o.AddAttribute(AttrPrintSettingsObjectType, (int)settings.ObjectType);
            o.AddAttribute(AttrPrintSettingsOpenMode, (int)settings.OpenMeshMode);

            o.AddAttribute(AttrPrintSettingsNoVoids, settings.NoVoids);
            o.AddAttribute(AttrPrintSettingsShellOnly, settings.OuterShellOnly);

            o.AddAttribute(AttrPrintSettingsClearanceXY, (float)settings.Clearance);
            o.AddAttribute(AttrPrintSettingsOffsetXY, (float)settings.OffsetXY);

            o.EndStruct();
        }

        public static PrintMeshSettings RestorePrintMeshSettings(SOFactory factory, TypedAttribSet attributes)
        {
            PrintMeshSettings settings = new PrintMeshSettings();

            TypedAttribSet attribs = factory.find_struct(attributes, PrintSettingsStruct);
            if (attribs == null)
                throw new Exception("PrintMeshSO.RestorePrintMeshSettings: PrintMeshSettings struct not found!");

            if (factory.check_key_or_debug_print(attribs, AttrPrintSettingsVersion))
                settings.Version = (int)attribs[AttrPrintSettingsVersion];

            if (factory.check_key_or_debug_print(attribs, AttrPrintSettingsObjectType))
                settings.ObjectType = (PrintMeshSettings.ObjectTypes)(int)attribs[AttrPrintSettingsObjectType];
            if (factory.check_key_or_debug_print(attribs, AttrPrintSettingsOpenMode))
                settings.OpenMeshMode = (PrintMeshSettings.OpenMeshModes)(int)attribs[AttrPrintSettingsOpenMode];

            if (factory.check_key_or_debug_print(attribs, AttrPrintSettingsNoVoids))
                settings.NoVoids = (bool)attribs[AttrPrintSettingsNoVoids];
            if (factory.check_key_or_debug_print(attribs, AttrPrintSettingsShellOnly))
                settings.OuterShellOnly = (bool)attribs[AttrPrintSettingsShellOnly];

            if (factory.check_key_or_debug_print(attribs, AttrPrintSettingsClearanceXY))
                settings.Clearance = (float)attribs[AttrPrintSettingsClearanceXY];
            if (factory.check_key_or_debug_print(attribs, AttrPrintSettingsOffsetXY))
                settings.OffsetXY = (float)attribs[AttrPrintSettingsOffsetXY];

            return settings;
        }
    }

}
