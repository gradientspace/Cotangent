#pragma warning disable 414
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using UnityEngine.UI;
using f3;
using g3;
using gs;
using cotangent;

public class ImportMeshDialog : MonoBehaviour
{
    GameObject dimensionPanel;
    Text dimensionText;
    InputField dimensionInput;
    Button inchButton, meterButton, TenCMButton, FiveCMButton;

    GameObject polycountPanel;
    Text polycountText;
    InputField polycountInput;
    Button tenKButton, hundredKButton, twofiftyKButton, millionButton;

    Button skipButton;
    Button applyButton;

    bool SetPolycount = false;
    int InitialPolycount = -1;

    bool SetDimension = false;
    double InitialDimension = 0;
    int message_mode = 0;

    Action<double, int>  OnApplyF;
    Action OnSkipF;

    public static void Show(GameObject parentCanvas, 
        bool bDimensionSmall, bool bDimensionTall, bool bDimensionWide,
        double initialDimension,
        bool bPolycount, int initialPolycount,
        Action<double, int> applyF, Action skipF)
    {
        GameObject dialog = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("ImportMeshDialog"));

        parentCanvas.AddChild(dialog, false);

        var component = dialog.GetComponent<ImportMeshDialog>();
        component.SetDimension = (bDimensionSmall || bDimensionTall || bDimensionWide);
        if (bDimensionTall)
            component.message_mode = 1;
        else if (bDimensionWide)
            component.message_mode = 2;
        else
            component.message_mode = 0;
        component.InitialDimension = initialDimension;
        component.SetPolycount = bPolycount;
        component.InitialPolycount = initialPolycount;
        component.OnApplyF = applyF;
        component.OnSkipF = skipF;
    }


    // Use this for initialization
    public void Start()
    {
        dimensionPanel = this.gameObject.FindChildByName("DimensionPanel", true);
        dimensionText = UnityUIUtil.FindText(this.gameObject, "DimensionText");
        dimensionInput = UnityUIUtil.FindInput(this.gameObject, "DimensionInput");
        dimensionInput.text = InitialDimension.ToString("F4");
        if ( message_mode == 0 )
            dimensionText.text = "The imported mesh is very small. Maybe the units are not mm? You can select other units or enter a new height in the field below.";
        else if (message_mode == 1)
            dimensionText.text = "The imported mesh is taller than the print volume. You can enter a new height in the field below, or select the original units or a new height.";
        else if (message_mode == 2)
            dimensionText.text = "The imported mesh is much larger than the print bed. You can enter a new height in the field below, or select the original units or a new height.";

        inchButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "InchButton", on_dimension_inch);
        meterButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "MeterButton", on_dimension_meter);
        FiveCMButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "FiveButton", on_dimension_fivecm);
        TenCMButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "TenButton", on_dimension_tencm);

        polycountPanel = this.gameObject.FindChildByName("PolycountPanel", true);
        polycountText = UnityUIUtil.FindText(this.gameObject, "PolycountText");
        polycountInput = UnityUIUtil.FindInput(this.gameObject, "PolycountInput");
        polycountInput.text = InitialPolycount.ToString();

        tenKButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "TenKButton", () => { polycountInput.text = "25000"; });
        if (InitialPolycount < 10000)
            tenKButton.interactable = false;
        hundredKButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "HundredKButton", () => { polycountInput.text = "100000"; });
        if (InitialPolycount < 100000)
            hundredKButton.interactable = false;
        twofiftyKButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "TwoFiftyKButton", () => { polycountInput.text = "250000"; });
        if (InitialPolycount < 250000)
            twofiftyKButton.interactable = false;
        millionButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "MillionButton", () => { polycountInput.text = "1000000"; });
        if (InitialPolycount < 1000000)
            millionButton.interactable = false;

        skipButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "SkipButton", on_skip);
        applyButton= UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "ApplyButton", on_apply);


        if ( SetDimension == false ) {
            dimensionPanel.SetVisible(false);
            dimensionText.gameObject.SetVisible(false);
        }
        if (SetPolycount == false) {
            polycountPanel.SetVisible(false);
            polycountText.gameObject.SetVisible(false);
        }
    }


    void on_dimension_inch() {
        double dimension = InitialDimension * Units.Convert(Units.Linear.Inches, Units.Linear.Millimeters);
        dimensionInput.text = dimension.ToString("F4");
    }
    void on_dimension_meter() {
        double dimension = InitialDimension * Units.Convert(Units.Linear.Meters, Units.Linear.Millimeters);
        dimensionInput.text = dimension.ToString("F4");
    }
    void on_dimension_fivecm() {
        dimensionInput.text = "50.0";
    }
    void on_dimension_tencm() {
        dimensionInput.text = "100.0";
    }

    void on_apply()
    {
        double dimension = 0.0;
        if ( SetDimension ) {
            if (double.TryParse(dimensionInput.text, out dimension) == false)
                dimension = 0;
        }
        int polycount = -1;
        if ( SetPolycount ) {
            if (int.TryParse(polycountInput.text, out polycount) == false)
                polycount = -1;
            polycount = MathUtil.Clamp(polycount, 1, 4000000);
        }

        CC.ActiveContext.RegisterNextFrameAction(() => {
            OnApplyF(dimension, polycount);
        });
        GameObject.Destroy(this.gameObject);
    }


    void on_skip()
    {
        CC.ActiveContext.RegisterNextFrameAction(() => {
            OnSkipF();
        });
        GameObject.Destroy(this.gameObject);
    }




}



