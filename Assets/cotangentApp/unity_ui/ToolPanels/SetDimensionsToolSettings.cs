using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;
using cotangent;



public class SetDimensionsToolSettings : MonoBehaviour
{
    // !!! Does not current use BaseToolSettings because parameter setting is non-standard!!
    // (because this tool tracks parameter changes)

    InputField dimension_x;
    InputField dimension_y;
    InputField dimension_z;
    InputField scale_x;
    InputField scale_y;
    InputField scale_z;
    Toggle uniform;
    Toggle objectFrame;    GameObject objectFrameRow;

    public SetDimensionsTool Tool;
    public ParameterSet ActiveParameterSet;

    UnityUIUtil.DialogTabber tabber = new UnityUIUtil.DialogTabber();

    public void Start()
    {
        ITool curTool = CC.ActiveContext.ToolManager.ActiveRightTool;
        if (curTool == null)
            return;
        Tool = curTool as SetDimensionsTool;
        if (Tool == null)
            return;
        ActiveParameterSet = Tool.Parameters;

        dimension_x = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "WidthInput",
            () => { return (float)ActiveParameterSet.GetValueDouble("dimension_x"); },
            (floatValue) => { set_value("dimension_x", floatValue); update_from_tool(); }, 
            0, 9999999.0f);
        dimension_y = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "HeightInput",
            () => { return (float)ActiveParameterSet.GetValueDouble("dimension_y"); },
            (floatValue) => { set_value("dimension_y", floatValue); update_from_tool(); },
            0, 9999999.0f);
        dimension_z = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "DepthInput",
            () => { return (float)ActiveParameterSet.GetValueDouble("dimension_z"); },
            (floatValue) => { set_value("dimension_z", floatValue); update_from_tool(); },
            0, 9999999.0f);
        tabber.Add(dimension_x); tabber.Add(dimension_z); tabber.Add(dimension_y);

        scale_x = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "WidthXInput",
            () => { return (float)ActiveParameterSet.GetValueDouble("scale_x"); },
            (floatValue) => { set_value("scale_x", floatValue); update_from_tool(); },
            0, 9999999.0f);
        scale_y = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "HeightXInput",
            () => { return (float)ActiveParameterSet.GetValueDouble("scale_y"); },
            (floatValue) => { set_value("scale_y", floatValue); update_from_tool(); },
            0, 9999999.0f);
        scale_z = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, "DepthXInput",
            () => { return (float)ActiveParameterSet.GetValueDouble("scale_z"); },
            (floatValue) => { set_value("scale_z", floatValue); update_from_tool(); },
            0, 9999999.0f);
        tabber.Add(scale_x); tabber.Add(scale_z); tabber.Add(scale_y);


        uniform = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "UniformToggle",
            () => { return ActiveParameterSet.GetValueBool("uniform"); },
            (boolValue) => { set_value("uniform", boolValue); });
        tabber.Add(uniform);

        objectFrame = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, "ObjectFrameToggle",
            () => { return ActiveParameterSet.GetValueBool("use_object_frame"); },
            (boolValue) => { set_value("use_object_frame", boolValue); });
        tabber.Add(objectFrame);
        objectFrameRow = objectFrame.transform.parent.gameObject;

        // this doesn't work yet because we need to also change the visible dimensions...
        //objectFrameRow.SetVisible(Tool.Targets.Count() == 1);
        objectFrameRow.SetVisible(false);

        update_from_tool();

        curTool.Parameters.OnParameterModified += on_parameter_modified;
    }


    public void Update()
    {
        if (tabber.HasFocus() && Input.GetKeyUp(KeyCode.Tab)) {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                tabber.Previous();
            else
                tabber.Next();
        }
    }




    void on_parameter_modified(ParameterSet pset, string sParamName)
    {
        update_from_tool();
    }

    bool track_changes = false;

    void set_value(string name, double newValue)
    {
        if (track_changes) {
            Tool.Scene.History.PushChange(ActiveParameterSet.MakeChange(name, newValue));
            Tool.Scene.History.PushInteractionCheckpoint();
        } else
            ActiveParameterSet.SetValue<double>(name, newValue);
    }
    void set_value(string name, bool newValue)
    {
        if (track_changes) {
            Tool.Scene.History.PushChange(ActiveParameterSet.MakeChange(name, newValue));
            Tool.Scene.History.PushInteractionCheckpoint();
        } else
            ActiveParameterSet.SetValue<bool>(name, newValue);
    }



    void update_from_tool()
    {
        track_changes = false;

        dimension_x.text = ActiveParameterSet.GetValueDouble("dimension_x").ToString();
        dimension_y.text = ActiveParameterSet.GetValueDouble("dimension_y").ToString();
        dimension_z.text = ActiveParameterSet.GetValueDouble("dimension_z").ToString();

        scale_x.text = ActiveParameterSet.GetValueDouble("scale_x").ToString();
        scale_y.text = ActiveParameterSet.GetValueDouble("scale_y").ToString();
        scale_z.text = ActiveParameterSet.GetValueDouble("scale_z").ToString();

        uniform.isOn = ActiveParameterSet.GetValueBool("uniform");

        track_changes = true;
    }








}

