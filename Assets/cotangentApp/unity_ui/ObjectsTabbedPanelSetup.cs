#pragma warning disable 414
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;


/// <summary>
/// SceneBrowser / ObjectProperties tab view controller
/// </summary>
public class ObjectsTabbedPanelSetup : MonoBehaviour
{
    static ObjectsTabbedPanelSetup panelSetup;

    /// <summary>
    /// Auto-select tab based on current selection
    /// </summary>
    public static void UpdateVisibilityFromSelection()
    {
        if (panelSetup == null)
            return;

        List<PrintMeshSO> meshes = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>();
        List<PrintMeshSO> selected = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);

        // invalid scene/selection, clear settings and hide panel
        if (panelSetup.can_show_object_panel() == false ) {
            panelSetup.visiblePanel = VisiblePanel.ScenePanel;
        } else {
            //panelSetup.visiblePanel = VisiblePanel.ObjectPanel;
        }
        panelSetup.update_visibility();
    }




    public enum VisiblePanel
    {
        ScenePanel,
        ObjectPanel
    }


    Button sceneButton;
    Button objectButton;
    VisiblePanel visiblePanel;

    GameObject scenePanel;
    GameObject objectPanel;

    Colorf normalColor = Colorf.VideoWhite;
    Colorf highlightColor = Colorf.LightGreen;
    Colorf disabledColor = Colorf.DimGrey;

    public void Awake()
    {
    }

    // Use this for initialization
    public void Start()
    {
        sceneButton = UnityUIUtil.FindButtonAndAddClickHandler("SceneTab", on_scene);
        objectButton = UnityUIUtil.FindButtonAndAddClickHandler("ObjectTab", on_object);

        scenePanel = this.gameObject.FindChildByName("SceneScrollView", true);
        objectPanel = this.gameObject.FindChildByName("ObjectScrollView", true);

        visiblePanel = VisiblePanel.ObjectPanel;
        update_visibility();

        panelSetup = this;
    }


    int currentViewMode = -1;

    public void Update()
    {
        if ( currentViewMode != (int)CCActions.CurrentViewMode ) {
            currentViewMode = (int)CCActions.CurrentViewMode;
            update_tabs();
        }
    }



    void update_tabs()
    {
        if ( CCActions.CurrentViewMode == AppViewMode.PrintView ) {
            objectButton.gameObject.SetVisible(true);
        } else {
            objectButton.gameObject.SetVisible(false);
            if ( visiblePanel == VisiblePanel.ObjectPanel ) {
                visiblePanel = VisiblePanel.ScenePanel;
                update_visibility();
            }
        }
    }


    void on_scene()
    {
        visiblePanel = VisiblePanel.ScenePanel;
        update_visibility();
    }

    void on_object()
    {
        if (can_show_object_panel() == false)
            return;

        visiblePanel = VisiblePanel.ObjectPanel;
        update_visibility();
    }


    bool can_show_object_panel()
    {
        List<PrintMeshSO> selected = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);
        List<PrintMeshSO> meshes = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>();
        return (selected.Count == 1 || meshes.Count == 1);
    }


    void update_visibility()
    {
        if (visiblePanel == VisiblePanel.ScenePanel) {
            UnityUIUtil.SetColors(sceneButton, highlightColor, disabledColor);
            UnityUIUtil.SetColors(objectButton, normalColor, disabledColor);
            objectPanel.SetVisible(false);
            scenePanel.SetVisible(true);
        } else {
            UnityUIUtil.SetColors(sceneButton, normalColor, disabledColor);
            UnityUIUtil.SetColors(objectButton, highlightColor, disabledColor);
            scenePanel.SetVisible(false);
            objectPanel.SetVisible(true);
        }
    }
}
