#pragma warning disable 414
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using f3;
using gs;
using cotangent;

public class SelectMfgDialogSetup : MonoBehaviour
{
	Button okButton;
    Button cancelButton;
    Button submitNewButton;

    GameObject MfgListView;

    List<KnownManufacturerInfo> manufacturers;
    Dictionary<string, GameObject> mfgToRow = new Dictionary<string, GameObject>();

    // Use this for initialization
    public void Start()
	{
        okButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "OKButton", on_ok_clicked);
        cancelButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "CancelButton", on_cancel_clicked);
        submitNewButton = UnityUIUtil.FindButtonAndAddClickHandler(this.gameObject, "SubmitButton", on_submit_clicked);

        MfgListView = UnityUtil.FindChildByName(this.gameObject, "ManufacturerListView");
        populate_list();
    }


    public void Update()
    {
        bool any_rows_active = false;
        foreach (var pair in mfgToRow) {
            GameObject row = pair.Value;
            Toggle toggle = row.GetComponent<Toggle>();
            if (toggle.isOn) {
                any_rows_active = true;
                break;
            }
        }
        if ( okButton.interactable != any_rows_active )
            okButton.interactable = any_rows_active;
    }


    void on_ok_clicked()
    {
        update_from_list();
        destroy_dialog();
    }

    void on_cancel_clicked()
    {
        destroy_dialog();
    }

    void on_submit_clicked()
    {
        // [TODO]

        update_from_list();
        destroy_dialog();
    }


    void destroy_dialog()
    {
        CC.ActiveContext.RegisterNextFrameAction(() => { GameObject.Destroy(this.gameObject); });
    }



    void populate_list()
    {
        GameObject rowPrefab = Resources.Load<GameObject>("ManufacturerListItem");

        manufacturers = KnownManufacturerInfo.LoadManufacturers();
        foreach (var mfgInfo in manufacturers) {
            GameObject row = GameObject.Instantiate<GameObject>(rowPrefab);
            UnityUIUtil.FindTextAndSet(row, "Label", mfgInfo.name);
            Manufacturer mfg = CC.PrinterDB.FindManufacturerByUUID(mfgInfo.uuid);
            bool is_active = (mfg != null && CC.PrinterDB.IsDisabledManufacturer(mfg) == false);
            //UnityUIUtil.FindToggleAndSet(row, "ManufacturerListItem", is_active);
            UnityUIUtil.FindToggleAndSet(row, is_active);
            MfgListView.AddChild(row, false);

            mfgToRow[mfgInfo.uuid] = row;
        }
    }



    void update_from_list()
    {
        bool modified = false;
        foreach ( var pair in mfgToRow ) {
            string uuid = pair.Key;
            GameObject row = pair.Value;

            bool is_enabled = (CC.PrinterDB.FindManufacturerByUUID(uuid) != null);

            Toggle toggle = row.GetComponent<Toggle>();
            if ( toggle.isOn && is_enabled == false) {
                KnownManufacturerInfo mi = manufacturers.Find((x) => { return x.uuid == uuid; });
                CC.PrinterDB.EnableManufacturer(mi);
                modified = true;
            } else if ( toggle.isOn == false && is_enabled ) {
                KnownManufacturerInfo mi = manufacturers.Find((x) => { return x.uuid == uuid; });
                CC.PrinterDB.DisableManufacturer(mi);
                modified = true;
            }
        }
        if (modified) {
            Manufacturer activeMfg = CC.PrinterDB.ActiveManufacturer;
            if (CC.PrinterDB.IsDisabledManufacturer(activeMfg)) {
                CC.PrinterDB.SelectManufacturer(CC.PrinterDB.Manufacturers().First());
            } else {
                CC.PrinterDB.SavePreferencesHint();
            }
        }
    }





}
