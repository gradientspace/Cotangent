using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using f3;
using gs;

namespace cotangent
{
    public static partial class CCActions
    {

        public static void ShowSplashScreen(int MaxFreqInMinutes = 5)
        {
            if (FPlatform.InUnityEditor() == false) {

                int last_splash_mins = FPlatform.GetPrefsInt("LastSplashTime", 0);
                TimeSpan now_span = DateTime.Now - new DateTime(2018, 1, 1);
                int cur_mins = (int)now_span.TotalMinutes;
                double mins_elapsed = cur_mins - last_splash_mins;
                //DebugUtil.Log("ticks now {0}  saved {1}   spansec {2}", cur_mins, last_splash_mins, mins_elapsed);
                if (mins_elapsed > MaxFreqInMinutes) {
                    GameObject splash_screen = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("SplashScreenPanel"));
                    CotangentUI.MainUICanvas.AddChild(splash_screen, false);
                    FPlatform.SetPrefsInt("LastSplashTime", cur_mins);
                }
            }
        }




        public static void ShowPrivacyDialogIfRequired()
        {
            int privacy_version = CotangentVersion.GetConfirmedPrivacyPolicyVersion();
            if (privacy_version != CotangentVersion.PrivacyPolicyVersion ) {
                PrivacyStartupDialog.ShowDialog();
            }
        }




        public static void ShowPreferencesDialog(bool bShowPrivacyPage = false)
        {
            GameObject prefs = CotangentUI.MainUICanvas.FindChildByName("PreferencesDialog", true);
            if ( prefs != null ) {
                prefs.GetComponent<PreferencesDialog>().TransitionVisibility(true);
                if (bShowPrivacyPage) {
                    CC.ActiveContext.RegisterNthFrameAction(2, () => {
                        prefs.GetComponent<PreferencesDialog>().SetToPrivacyTab();
                    });
                }
            }
        }
        public static void HidePreferencesDialog()
        {
            GameObject prefs = CotangentUI.MainUICanvas.FindChildByName("PreferencesDialog", true);
            if ( prefs != null ) 
                prefs.GetComponent<PreferencesDialog>().TransitionVisibility(false);
        }
        public static bool IsPreferencesVisible() {
            GameObject prefs = CotangentUI.MainUICanvas.FindChildByName("PreferencesDialog", true);
            return (prefs != null && prefs.IsVisible());
        }



        /// <summary>
        /// called on scene & selection changes, to update the global current PrintSettings object
        /// (which currently can only be applied to a single object at a time)
        /// </summary>
        public static void UpdateObjectSettings()
        {
            if (CC.ObjSettings.IsActive)
                CC.ObjSettings.WriteToCurrentSettings();

            List<PrintMeshSO> meshes = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>();
            List<PrintMeshSO> selected = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);

            // invalid scene/selection, clear settings and hide panel
            if (meshes.Count == 0 || (meshes.Count > 1 && selected.Count != 1)) {
                CC.ObjSettings.ClearCurrentSettings();
            } else {
                // upate for current object
                PrintMeshSO so = (meshes.Count == 1) ? meshes[0] : selected[0];
                CC.ObjSettings.UpdateFromSettings(so);
            }

            ObjectsTabbedPanelSetup.UpdateVisibilityFromSelection();
        }






    }
}
