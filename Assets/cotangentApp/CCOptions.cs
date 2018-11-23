using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using f3;

namespace cotangent
{
    public static class CCOptions
    {
        public static string CotangentAppDataPath {
            get {
                string sAppData =
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                return Path.Combine(sAppData, "Cotangent");
            }
        }

        public static string SettingsDBPath {
            get {
                return Path.Combine(CotangentAppDataPath, "settingsdb");
            }
        }
    }
}
