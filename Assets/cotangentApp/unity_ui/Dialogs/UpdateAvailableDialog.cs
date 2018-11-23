using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class UpdateAvailableDialog : MonoBehaviour
{
    Button installNowButton;
    Button installOnExitButton;
    Button installLaterButton;

    GameObject downloadPanel;
    Text downloadText;
    Button cancelButton;


    NewVersionCheck VersionCheck;
    string DownloadURL;


    public static void Show_NoForce(GameObject parentCanvas, string url, NewVersionCheck versionCheck)
    {
        GameObject dialog = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("UpdateAvailableDialog"));

        parentCanvas.AddChild(dialog, false);

        var component = dialog.GetComponent<UpdateAvailableDialog>();
        component.VersionCheck = versionCheck;
        component.DownloadURL = url;
    }


    // Use this for initialization
    public void Start()
    {
        installNowButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "InstallNowButton", install_now);
        installOnExitButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "InstallOnExitButton", install_on_exit);
        installLaterButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "InstallLaterButton", install_later);

        downloadPanel = this.gameObject.FindChildByName("DownloadPanel", true);
        downloadText = UnityUIUtil.FindText(this.gameObject, "DownloadingText");
        cancelButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "CancelButton", cancel_download);
        downloadPanel.SetVisible(false);
    }


    NewVersionDownloadController active_controller;


    public void Update()
    {
        if (active_controller != null) {
            long received, total; int percent;
            active_controller.GetProgress(out received, out total, out percent);
            downloadText.text = string.Format("Downloading... {0}%", percent);
        }
    }


    void install_now()
    {
        installNowButton.interactable = false;
        installOnExitButton.interactable = false;
        installLaterButton.interactable = false;

        downloadPanel.SetVisible(true);
        cancelButton.interactable = true;

        active_controller = VersionCheck.DownloadAndLaunchInstaller(DownloadURL, QuitBeforeInstall);
    }


    static void QuitBeforeInstall()
    {
        CC.ActiveContext.RegisterNextFrameAction(() => {
            FPlatform.QuitApplication();
        });
    }


    void cancel_download()
    {
        active_controller.Cancel();

        cancelButton.interactable = false;

        installNowButton.interactable = true;
        installOnExitButton.interactable = true;
        installLaterButton.interactable = true;
    }


    void install_on_exit()
    {
        VersionCheck.LaunchInstallerF = InstallOnExitLauncher;

        CC.ActiveContext.RegisterNextFrameAction(() => {
            active_controller = VersionCheck.DownloadAndLaunchInstaller(DownloadURL, null );
        });

        GameObject.Destroy(this.gameObject);
    }
    static void InstallOnExitLauncher(string sPath)
    {
        DebugUtil.Log("Background download complete, installing on-exit launcher...");
        GameObject on_exit_go = new GameObject("install_on_exit_helper");
        var c = on_exit_go.AddComponent<InstallOnExitBehavior>();
        c.InstallerPath = sPath;
    }


    void install_later()
    {
        GameObject.Destroy(this.gameObject);
    }

}





public class InstallOnExitBehavior : MonoBehaviour
{
    public string InstallerPath;

    public void OnApplicationQuit()
    {
        NewVersionCheck.LaunchInstaller(InstallerPath);
    }
}