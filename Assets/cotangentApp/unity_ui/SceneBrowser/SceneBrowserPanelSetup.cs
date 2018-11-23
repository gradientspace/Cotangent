using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;
using TMPro;


/// <summary>
/// Object-properties panel controller
/// </summary>
public class SceneBrowserPanelSetup : MonoBehaviour
{
    bool initialized = false;


    class PanelRow
    {
        public SceneObject SO;
        public GameObject panel;
        public Text label;
        public Button rowButton;
        public Button visibleButton;
    }

    Dictionary<SceneObject, PanelRow> SORows = new Dictionary<SceneObject, PanelRow>();
    HashSet<SceneObject> SelectedRows = new HashSet<SceneObject>();

    Sprite visible_on;
    Sprite visible_off;
    Colorf selectedRowColor = Colorf.SelectionGold;
    Colorf defaultRowColor = Colorf.LightGrey;


    public void Start()
    {
        visible_on = Resources.Load<Sprite>("icons/visible");
        visible_off = Resources.Load<Sprite>("icons/visible_un");

        Update();
    }


    List<SceneObject> pending_updates = new List<SceneObject>();


    public void Update()
    {
        if ( initialized == false && CC.ActiveScene != null ) {
            CC.ActiveScene.SelectionChangedEvent += ActiveScene_SelectionChangedEvent;
            CC.ActiveScene.ChangedEvent += ActiveScene_ChangedEvent;
            initialized = true;

            List<PrintMeshSO> initial = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>();
            foreach ( var so in initial ) {
                PanelRow newRow = add_new_row(so as PrintMeshSO);
                SORows.Add(so, newRow);
            }
            update_selection();
        }

        foreach ( var so in pending_updates ) {
            if ( SORows.ContainsKey(so) )
                update_row(so);
        }
        pending_updates.Clear();
    }


    private void ActiveScene_ChangedEvent(object sender, SceneObject so, SceneChangeType type)
    {
        if (so is PrintMeshSO == false)
            return;

        if ( type == SceneChangeType.Added ) {
            PanelRow newRow = add_new_row(so as PrintMeshSO);
            SORows.Add(so, newRow);
            pending_updates.Add(so);        // in case name changes after this event posts

        } else if ( type == SceneChangeType.Removed ) {
            PanelRow row;
            if ( SORows.TryGetValue(so, out row)) {
                GameObject.Destroy(row.panel);
                SORows.Remove(so);
            }
        }
    }


    private void ActiveScene_SelectionChangedEvent(object sender, EventArgs e)
    {
        update_selection();
    }



    private void update_row(SceneObject so)
    {
        PanelRow row = SORows[so];
        if (row.label.text != so.Name)
            row.label.text = so.Name;
        // how to know visibliity state?
        //bool visible = SceneUtil.IsVisible(so);
    }


    private PanelRow add_new_row(PrintMeshSO so)
    {
        PanelRow row = new PanelRow();
        row.SO = so;

        row.panel = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("SceneBrowserPrintSOPanel"));
        this.gameObject.AddChild(row.panel, false);

        row.label = UnityUIUtil.FindText(row.panel, "SONameLabel");
        row.label.text = so.Name;

        row.rowButton = UnityUIUtil.FindButtonAndAddClickHandler(row.panel, "RowButton", () => { on_row_clicked(row); });
        row.rowButton.image.color = defaultRowColor;

        row.visibleButton = UnityUIUtil.FindButtonAndAddClickHandler(row.panel, "VisibleButton", () => { on_visible_clicked(row); });

        return row;
    }


    bool click_shift_down = false;
    bool click_ctrl_down = false;
    int click_count = 0;
    void on_row_clicked(PanelRow row)
    {
        bool bShiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        bool bCtrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        click_shift_down = bShiftDown;
        click_ctrl_down = bCtrlDown;
        on_row_single_clicked(row);


        //if (click_count == 0) {
        //    click_shift_down = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        //    StartCoroutine(ClickOrDoubleClick(row, on_row_single_clicked, on_row_double_clicked));
        //}
        //click_count++;
    }

    void on_row_single_clicked(PanelRow row)
    {
        bool is_selected = CC.ActiveScene.IsSelected(row.SO);
        if (is_selected && click_ctrl_down) {
            CC.ActiveScene.Deselect(row.SO);
        } else {
            bool bAdd = click_ctrl_down || click_shift_down;
            CC.ActiveScene.Select(row.SO, bAdd == false);
        }
    }
    void on_row_double_clicked(PanelRow row)
    {
        CotangentUI.GetStringFromDialog("Rename", "Enter the new object name", row,
            (s)=> { return !string.IsNullOrEmpty(s); } , 
            update_object_name, null);
    }
    void update_object_name(string s, object target)
    {
        if (string.IsNullOrEmpty(s) == false) {
            PanelRow row = target as PanelRow;
            row.SO.Name = s;
            row.label.text = row.SO.Name;
        }
    }

    float doubleclick_time = 0.3f;
    private IEnumerator ClickOrDoubleClick<T>(T obj, Action<T> single_click_f, Action<T> double_click_f)
    {
        float start_time = Time.realtimeSinceStartup;
        while ( (Time.realtimeSinceStartup - start_time) < doubleclick_time) {
            if ( click_count > 1 ) {
                double_click_f(obj);
                click_count = 0;
                yield break;
            }
            yield return null;  // wait for the next frame
        }
        single_click_f(obj);
        click_count = 0;
    }





    void on_visible_clicked(PanelRow row)
    {
        bool set_visible = !SceneUtil.IsVisible(row.SO);
        SceneUtil.SetVisible(row.SO, set_visible);
        row.visibleButton.image.sprite = (set_visible) ? visible_on : visible_off;
    }

    void update_selection()
    {
        List<PrintMeshSO> selected = CC.ActiveScene.FindSceneObjectsOfType<PrintMeshSO>(true);

        List<SceneObject> remove = new List<SceneObject>();
        foreach ( var so in SelectedRows ) {
            if (selected.Contains(so) == false) {
                deselect(so);
                remove.Add(so);
            }
        }

        foreach ( var so in remove )
            SelectedRows.Remove(so);

        foreach ( var so in selected ) {
            if ( SelectedRows.Contains(so) == false ) {
                select(so);
                SelectedRows.Add(so);
            }
        }
    }



    void select(SceneObject so)
    {
        PanelRow row;
        if (SORows.TryGetValue(so, out row))
            row.rowButton.image.color = selectedRowColor;
    }
    void deselect(SceneObject so)
    {
        PanelRow row;
        if (SORows.TryGetValue(so, out row))
            row.rowButton.image.color = defaultRowColor;
    }

}
