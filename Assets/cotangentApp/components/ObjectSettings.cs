using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using f3;
using gs;

namespace cotangent
{
    public class ObjectSettings
    {
        protected PrintMeshSO ActiveSO;
        protected PrintMeshSettings Active;

        protected bool bValueModified = false;
        public bool HasModifiedValues {
            get { return bValueModified; }
        }


        public bool IsActive {
            get { return Active != null;  }
        }

        public void ClearCurrentSettings() {
            Active = null;
            ActiveSO = null;
        }

        public SceneObject SourceSO {
            get { return ActiveSO; }
        }



        PrintMeshSettings.ObjectTypes object_type = PrintMeshSettings.ObjectTypes.Solid;
        public PrintMeshSettings.ObjectTypes ObjectType {
            get { return object_type; }
            set { if (object_type != value) { object_type = value; setting_modified(true, true); } }
        }
        public int ObjectTypeInt {
            get { return (int)object_type; }
            set {
                PrintMeshSettings.ObjectTypes new_type = (PrintMeshSettings.ObjectTypes)value;
                if (object_type != new_type) {
                    object_type = new_type; setting_modified(true, true);
                }
            }
        }



        PrintMeshSettings.OpenMeshModes open_mode = PrintMeshSettings.OpenMeshModes.Default;
        public PrintMeshSettings.OpenMeshModes OpenMode {
            get { return open_mode; }
            set { if (open_mode != value) { open_mode = value; setting_modified(true, true); } }
        }
        public int OpenModeInt {
            get { return (int)open_mode; }
            set { PrintMeshSettings.OpenMeshModes newmode = (PrintMeshSettings.OpenMeshModes)value;
                  if (open_mode != newmode) {
                    open_mode = newmode; setting_modified(true, true);
                  }
            }
        }

       

        public bool no_voids = false;
        public bool NoVoids {
            get { return no_voids; }
            set { if (no_voids != value) { no_voids = value; setting_modified(true, true); } }
        }
        public bool NoVoids_Modified { get { return no_voids != Active.NoVoids; } }


        public bool shell_only = false;
        public bool OuterShellOnly {
            get { return shell_only; }
            set { if (shell_only != value) { shell_only = value; setting_modified(true, true); } }
        }
        public bool OuterShellOnly_Modified { get { return shell_only != Active.OuterShellOnly; } }


        public double clearance = 0;
        public double Clearance {
            get { return clearance; }
            set { if (clearance != value) { clearance = value; setting_modified(true, true); } }
        }
        public bool Clearance_Modified { get { return clearance != Active.Clearance; } }


        public double offset_xy = 0;
        public double OffsetXY {
            get { return offset_xy; }
            set { if (offset_xy != value) { offset_xy = value; setting_modified(true, true); } }
        }
        public bool OffsetXY_Modified { get { return offset_xy != Active.OffsetXY; } }



        public bool CanAutoReloadChanges {
            get { return (ActiveSO != null) ? ActiveSO.CanAutoUpdateFromSource() : false; }
        }

        public bool AutoReloadChanges {
            get { return (ActiveSO != null) ? ActiveSO.AutoUpdateOnSourceFileChange : false; }
            set { if (ActiveSO != null) CCActions.SetFileMonitoringEnabled(ActiveSO, value); }
        }


        public delegate void SettingsModifiedEvent(ObjectSettings settings);
        public event SettingsModifiedEvent OnNewSettings;
        public event SettingsModifiedEvent OnSettingModified;


        void setting_modified(bool invalidate_slicing, bool invalidate_paths)
        {
            WriteToCurrentSettings();   // [RMS] need to do this so that settings are reflected in
                                        //   slicer. To avoid, we need to know to grab from CC.ObjSettings instead...

            if (invalidate_slicing)
                CC.InvalidateSlicing();
            else if (invalidate_paths)
                CC.InvalidateToolPaths();
            bValueModified = true;
            OnSettingModified?.Invoke(this);
        }



        public void UpdateFromSettings(PrintMeshSO printSO)
        {
            if (printSO == null)
                return;

            ActiveSO = printSO;
            Active = printSO.Settings;

            // Note!! have to use internal variables here, if we use accessors then we
            // will spawn toolpath invalidations when objects change
            object_type = Active.ObjectType;
            no_voids = Active.NoVoids;
            shell_only = Active.OuterShellOnly;
            open_mode = Active.OpenMeshMode;
            clearance = Active.Clearance;
            offset_xy = Active.OffsetXY;

            bValueModified = false;
            OnNewSettings?.Invoke(this);
        }


        public void WriteToCurrentSettings() {
            if ( Active != null )
                WriteToSettings(Active);
        }
        public void WriteToSettings(PrintMeshSettings settings)
        {
            settings.ObjectType = object_type;
            settings.NoVoids = NoVoids;
            settings.OuterShellOnly = OuterShellOnly;
            settings.OpenMeshMode = OpenMode;
            settings.Clearance = Clearance;
            settings.OffsetXY = OffsetXY;
        }


        public PrintMeshSettings CloneCurrentPrintSettings()
        {
            return Active.Clone();
        }




    }
}
