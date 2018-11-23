#pragma warning disable 414
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using g3;
using f3;
using gs;
using cotangent;

public class PrintInfoPanelSetup : MonoBehaviour
{
    GameObject statusBG;
    Text layerText;
    Button layerUpButton;
    Button layerDownButton;

    // Use this for initialization
    public void Start()
	{
        try {
            statusBG = this.gameObject.FindChildByName("InfoBG", true);
            layerText = UnityUIUtil.FindText(statusBG, "LayerText");
            layerUpButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "LayerUpButton", on_up_button);
            layerDownButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "LayerDownButton", on_down_button);

            ButtonLongPress layerUpHold = layerUpButton.gameObject.AddComponent<ButtonLongPress>();
            layerUpHold.HoldTime = 0.5f; layerUpHold.RepeatTime = 0.1f;
            layerUpHold.onLongPress.AddListener(on_up_button_longpress);

            ButtonLongPress layerDownHold = layerDownButton.gameObject.AddComponent<ButtonLongPress>();
            layerDownHold.HoldTime = 0.5f; layerDownHold.RepeatTime = 0.1f;
            layerDownHold.onLongPress.AddListener(on_down_button_longpress);

            statusBG.SetVisible(false);
            visible = false;
            cur_layer = -1;

        } catch (Exception e) {
            DebugUtil.Log("StatusPanelSetup Start(): " + e.Message);
        }
    }

    bool visible = false;
    int cur_layer = -1;

    public void Update()
    {
        bool force_update = false;
        int num_layers = CC.Objects.NumLayers;
        if ( num_layers == 0 && visible ) {
            visible = false;
            statusBG.SetVisible(false);
        } else if ( num_layers > 0 && visible == false ) {
            visible = true;
            statusBG.SetVisible(true);
            force_update = true;
        }

        if (force_update || cur_layer != CC.Objects.CurrentLayer) {
            cur_layer = CC.Objects.CurrentLayer;
            layerText.text = string.Format("Layer {0}/{1}", cur_layer + 1, num_layers);
        }
    }


    void on_up_button() {
        int step = Input.GetKey(KeyCode.LeftShift) ? 10 : 1;
        CC.Objects.CurrentLayer = CC.Objects.CurrentLayer + step;
    }
    void on_down_button() {
        int step = Input.GetKey(KeyCode.LeftShift) ? 10 : 1;
        CC.Objects.CurrentLayer = CC.Objects.CurrentLayer - step;
    }


    void on_up_button_longpress() {
        on_up_button();
    }
    void on_down_button_longpress() {
        on_down_button();
    }

}







