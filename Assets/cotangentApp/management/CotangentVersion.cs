using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;
using gs;

namespace cotangent
{
    public static class CotangentVersion
    {
        static public Index3i CurrentVersion = new Index3i(1,0,6);

        static public string CurrentVersionString {
            get { return string.Format("{0}.{1}.{2}", CurrentVersion.a, CurrentVersion.b, CurrentVersion.c); }
        }

        public class CurrentVersionInfo : VersionInfoSource
        {
            public string VersionFileURL { get { return "https://s3.us-east-2.amazonaws.com/cotangent-version/auto_update.txt"; } }
            public int MajorVersion { get { return CotangentVersion.CurrentVersion.a; } }
            public int MinorVersion { get { return CotangentVersion.CurrentVersion.b; } }
            public int BuildNumber { get { return CotangentVersion.CurrentVersion.c; } }
        }



        public static void DoAutoUpdateCheck()
        {
            NewVersionCheck check = new NewVersionCheck(new CurrentVersionInfo());
            check.OnErrorF = (msg) => {
                // ??
            };

            check.DoNewVersionCheck((url, force) => {

                UpdateAvailableDialog.Show_NoForce(CotangentUI.MainUICanvas, url, check);

            });
        }


        static void QuitBeforeInstall()
        {
            CC.ActiveContext.RegisterNextFrameAction(() => {
                FPlatform.QuitApplication();
            });
        }





        static public int PrivacyPolicyVersion = 2;

        static public int GetConfirmedPrivacyPolicyVersion() {
            return FPlatform.GetPrefsInt("PrivacyVersion", 0);
        }
        static public void SetPrivacyPolicyConfirmed() {
            FPlatform.SetPrefsInt("PrivacyVersion", PrivacyPolicyVersion);
        }


    }
}
