using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using g3;
using f3;
using gs;

namespace cotangent
{
    /// <summary>
    /// Generic base-class that connects Tool parameters to standard UI widgets
    /// </summary>
    public abstract class BaseToolSettings<T> : MonoBehaviour where T : class, ITool
    {
        // functions that subclasses need to implement
        protected abstract void register_parameters();

        // optional
        protected virtual void per_frame_update() { }
        protected virtual void after_tool_values_update() { }

        // variables

        public T Tool;
        public ParameterSet ActiveParameterSet;

        struct FloatInputParam
        {
            public InputField widget;
            public string paramName;
            public string formatString;
        }
        List<FloatInputParam> float_params = new List<FloatInputParam>();

        struct IntInputParam
        {
            public InputField widget;
            public string paramName;
        }
        List<IntInputParam> int_params = new List<IntInputParam>();

        struct ToggleParam
        {
            public Toggle widget;
            public string paramName;
        }
        List<ToggleParam> toggle_params = new List<ToggleParam>();

        struct ToggleButtonParam
        {
            public Button widget;
            public string paramName;
        }
        List<ToggleButtonParam> toggle_button_params = new List<ToggleButtonParam>();

        struct DropDownParam
        {
            public MappedDropDown widget;
            public string paramName;
        }
        List<DropDownParam> dropdown_params = new List<DropDownParam>();


        protected UnityUIUtil.DialogTabber TabOrder = new UnityUIUtil.DialogTabber();


        // internal api

        public InputField RegisterFloatInput(string inputName, string toolParamName, Interval1d validRange, string formatString = null)
        {
            InputField input = UnityUIUtil.FindInputAndAddFloatHandlers(this.gameObject, inputName,
                () => { return (float)ActiveParameterSet.GetValueDouble(toolParamName); },
                (floatValue) => { ActiveParameterSet.SetValue<double>(toolParamName, floatValue); update_values_from_tool(); },
                (float)validRange.a, (float)validRange.b);
            TabOrder.Add(input);

            float_params.Add(new FloatInputParam() {
                widget = input, paramName = toolParamName, formatString = formatString
            });

            return input;
        }


        public InputField RegisterIntInput(string inputName, string toolParamName, Interval1i validRange)
        {
            InputField input = UnityUIUtil.FindInputAndAddIntHandlers(this.gameObject, inputName,
                () => { return ActiveParameterSet.GetValueInt(toolParamName); },
                (intValue) => { ActiveParameterSet.SetValue<int>(toolParamName, intValue); update_values_from_tool(); },
                validRange.a, validRange.b);
            TabOrder.Add(input);

            int_params.Add(new IntInputParam() {
                widget = input, paramName = toolParamName
            });

            return input;
        }

        public Toggle RegisterToggle(string toggleName, string toolParamName)
        {
            Toggle toggle = UnityUIUtil.FindToggleAndConnectToSource(this.gameObject, toggleName,
                () => { return ActiveParameterSet.GetValueBool(toolParamName); },
                (boolValue) => { ActiveParameterSet.SetValue(toolParamName, boolValue); update_values_from_tool(); });
            TabOrder.Add(toggle);

            toggle_params.Add(new ToggleParam() {
                widget = toggle, paramName = toolParamName
            });

            return toggle;
        }


        public Button RegisterToggleButton(string toggleName, string toolParamName)
        {
            Button button = UnityUIUtil.FindButtonAndAddToggleBehavior(this.gameObject, toggleName,
                () => { return ActiveParameterSet.GetValueBool(toolParamName); },
                (boolValue) => { ActiveParameterSet.SetValue(toolParamName, boolValue); },
                standard_update_toggle_button );
            TabOrder.Add(button);

            toggle_button_params.Add(new ToggleButtonParam() {
                widget = button, paramName = toolParamName
            });

            return button;
        }
        void standard_update_toggle_button(bool bValue, Button button)
        {
            Colorf onColor = Colorf.LightGreen;
            Colorf offColor = Colorf.Silver;
            Colorf disabledColor = Colorf.DimGrey;
            UnityUIUtil.SetColors(button, (bValue) ? onColor : offColor, disabledColor);
        }


        public MappedDropDown RegisterDropDown(string dropdownName, string toolParamName, 
            List<string> options, List<int> values )
        {
            MappedDropDown dropdown = new MappedDropDown(
                this.gameObject.FindChildByName(dropdownName, true).GetComponent<Dropdown>(),
                () => { return ActiveParameterSet.GetValueInt(toolParamName); },
                (intValue) => { ActiveParameterSet.SetValue<int>(toolParamName, intValue); update_values_from_tool(); });
            dropdown.SetOptions(options, values);
            TabOrder.Add(dropdown.drop);

            dropdown_params.Add(new DropDownParam() {
                widget = dropdown, paramName = toolParamName
            });

            return dropdown;
        }



        // internals

        protected bool initialization_complete = false;

        public virtual void Start()
        {
            ITool curTool = CC.ActiveContext.ToolManager.ActiveRightTool;
            if (curTool == null)
                return;
            Tool = curTool as T;
            if (Tool == null)
                return;
            ActiveParameterSet = Tool.Parameters;

            register_parameters();

            initialization_complete = true;
            update_values_from_tool();
        }


        public virtual void Update()
        {
            if (TabOrder.HasFocus() && Input.GetKeyUp(KeyCode.Tab)) {
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
                    TabOrder.Previous();
                else
                    TabOrder.Next();
            }

            per_frame_update();
        }


        // subclasses can override this to extend
        protected virtual void update_values_from_tool()
        {
            if (initialization_complete == false)
                return;

            foreach (var param in float_params) {
                if (string.IsNullOrEmpty(param.formatString))
                    param.widget.text = ActiveParameterSet.GetValueDouble(param.paramName).ToString();
                else
                    param.widget.text = ActiveParameterSet.GetValueDouble(param.paramName).ToString(param.formatString);
            }

            foreach (var param in int_params) {
                param.widget.text = ActiveParameterSet.GetValueInt(param.paramName).ToString();
            }

            foreach (var param in toggle_params) {
                param.widget.isOn = ActiveParameterSet.GetValueBool(param.paramName);
            }

            foreach (var param in toggle_button_params) {
                standard_update_toggle_button(ActiveParameterSet.GetValueBool(param.paramName), param.widget);
            }

            foreach (var param in dropdown_params) {
                param.widget.SetFromId(ActiveParameterSet.GetValueInt(param.paramName));
            }

            after_tool_values_update();
        }





    }

}