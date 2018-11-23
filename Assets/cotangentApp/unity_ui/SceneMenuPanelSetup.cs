#pragma warning disable 414
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;

public class SceneMenuPanelSetup : MonoBehaviour
{
    GameObject Panel;
    GameObject Canvas;

    Button openButton;
    Button importButton;
    Button saveButton;
    Button saveAsButton;
    Button exportGCodeButton;
    Button exportMeshButton;
    Button moreButton;

    GameObject ActiveFlyout = null;

    // Use this for initialization
    public void Start()
	{
        Panel = this.gameObject;
        Canvas = CotangentUI.MainUICanvas;

        try {

            openButton = UnityUIUtil.FindButtonAndAddClickHandler("OpenButton", on_open_clicked);
            importButton = UnityUIUtil.FindButtonAndAddClickHandler("ImportButton", on_import_clicked);
            saveButton = UnityUIUtil.FindButtonAndAddClickHandler("SaveButton", on_save_clicked);
            saveAsButton = UnityUIUtil.FindButtonAndAddClickHandler("SaveAsButton", on_save_as_clicked);
            exportGCodeButton = UnityUIUtil.FindButtonAndAddClickHandler("ExportButton", on_export_clicked);
            exportMeshButton = UnityUIUtil.FindButtonAndAddClickHandler("ExportMeshButton", on_export_mesh_clicked);
            moreButton = UnityUIUtil.FindButtonAndAddClickHandler("MoreButton", on_more_clicked);

            UnityUIUtil.SetColors(saveButton, CotangentUI.ModifiedSettingColor, CotangentUI.DisabledButtonColor);

        }catch(Exception e) {
            DebugUtil.Log("ScenePanelSetup Start(): " + e.Message);
        }
    }


    public void Update()
    {
        if (exportGCodeButton != null)
            exportGCodeButton.gameObject.SetVisible(CCActions.CurrentViewMode == AppViewMode.PrintView);

        if (exportGCodeButton != null && exportGCodeButton.gameObject.IsVisible())
            exportGCodeButton.interactable = CC.Toolpather.ToolpathsValid;
        if (exportMeshButton != null)
            exportMeshButton.interactable = CC.Objects.PrintMeshes.Count > 0;

        if (CCActions.HaveActiveSaveFile && CCActions.CurrentSceneModified) {
            UnityUIUtil.SetColors(saveButton, CotangentUI.ModifiedSettingColor, CotangentUI.DisabledButtonColor);
        } else {
            UnityUIUtil.SetColors(saveButton, CotangentUI.NormalSettingColor, CotangentUI.DisabledButtonColor);
        }
    }



    void on_open_clicked()
    {
        CC.ActiveContext.RegisterNextFrameAction(CCActions.DoFileDialogOpen);
        hide_flyout();
    }

    void on_import_clicked()
    {
        CC.ActiveContext.RegisterNextFrameAction( CCActions.DoFileDialogImport );
        hide_flyout();
    }

    void on_export_clicked()
    {
        CC.ActiveContext.RegisterNextFrameAction( CCActions.DoGCodeExport );
        hide_flyout();
    }

    void on_export_mesh_clicked()
    {
        CC.ActiveContext.RegisterNextFrameAction( CCActions.DoMeshExport );
        hide_flyout();
    }

    void on_save_clicked()
    {
        CC.ActiveContext.RegisterNextFrameAction(CCActions.SaveCurrentSceneOrSaveAs);
        hide_flyout();
    }

    void on_save_as_clicked()
    {
        CC.ActiveContext.RegisterNextFrameAction(CCActions.SaveCurrentSceneAs);
        hide_flyout();
    }




    void hide_flyout()
    {
        if (ActiveFlyout != null) {
            if (ActiveFlyout != null) {
                ActiveFlyout.RemoveFromParent(false);
                ActiveFlyout.Destroy();
                ActiveFlyout = null;
            }
        }
    }



    void on_more_clicked()
    {
        if (ActiveFlyout != null) {
            hide_flyout();
            return;
        } else {
            ActiveFlyout = build_more_flyout();
        }
    }
    GameObject build_more_flyout()
    {
        GameObject popupGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolMenu"));
        popupGO.SetName("MoreSceneMenu");

        CCUIBuilder.AddBasicToolButton(popupGO, "New Scene", on_clear_scene_clicked);

        Canvas.AddChild(popupGO, false);

        // [RMS] I have no idea what is going on here...but this does work

        RectTransform targetRect = moreButton.gameObject.GetComponent<RectTransform>();
        AxisAlignedBox2f targetBox = targetRect.rect;
        Vector2f targetPoint = BoxModel.GetBoxPosition(ref targetBox, BoxPosition.TopRight);
        Vector3f targetW = moreButton.gameObject.transform.TransformPoint(targetPoint.x, targetPoint.y, 0);

        //RectTransform popupRect = popupGO.GetComponent<RectTransform>();
        //AxisAlignedBox2f popupBox = popupRect.rect;
        //Vector2f popupCorner = BoxModel.GetBoxPosition(ref popupBox, BoxPosition.TopLeft);
        //Vector3f popupW = popupGO.transform.TransformPoint(popupCorner.x, popupCorner.y, 0);
        //Vector2f popupCenter = BoxModel.GetBoxPosition(ref popupBox, BoxPosition.Center);
        //Vector3f popupC = popupGO.transform.TransformPoint(popupCenter.x, popupCenter.y, 0);
        //Vector3f dl = popupW - popupC;

        //Vector3f dx = popupW - targetW;

        Vector3f shift = new Vector3f(5, 5, 0);
        popupGO.transform.position = targetW + shift;


        return popupGO;
    }

    void on_clear_scene_clicked()
    {
        CCActions.DoInteractiveClearScene();
        hide_flyout();
    }


}
