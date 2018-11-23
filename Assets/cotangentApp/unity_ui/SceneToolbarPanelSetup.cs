#pragma warning disable 414
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using f3;
using gs;
using cotangent;

public class SceneToolbarPanelSetup : MonoBehaviour
{
	Button undoButton;
    Button redoButton;
    Button fitToViewButton;
    Button prefsButton;

    // Use this for initialization
    public void Start()
	{
        try {

            undoButton = UnityUIUtil.FindButtonAndAddClickHandler("UndoButton", on_undo_clicked);
            redoButton = UnityUIUtil.FindButtonAndAddClickHandler("RedoButton", on_redo_clicked);
            fitToViewButton = UnityUIUtil.FindButtonAndAddClickHandler("FitToViewButton", on_fit_view_clicked);
            prefsButton = UnityUIUtil.FindButtonAndAddClickHandler("PreferencesButton", ()=> { CCActions.ShowPreferencesDialog(); });

        } catch(Exception e) {
            DebugUtil.Log("ScenePanelSetup Start(): " + e.Message);
        }
    }



    void on_undo_clicked()
    {
        CC.ActiveScene.History.InteractiveStepBack();
    }
    void on_redo_clicked()
    {
        CC.ActiveScene.History.InteractiveStepForward();
    }
    void on_fit_view_clicked()
    {
        CCActions.FitCurrentSelectionToView();
    }

}
