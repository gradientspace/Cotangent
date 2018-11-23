using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using g3;
using f3;
using gs;

namespace cotangent
{
    public static class CCUIBuilder
    {

        static public Button AddBasicToolButton(GameObject popupGO, string text, UnityAction onClickedF, Func<bool> isEnabledF = null)
        {
            GameObject buttonGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolButton"));
            popupGO.AddChild(buttonGO, false);
            buttonGO.SetName(text);
            Button button = UnityUIUtil.GetButtonAndAddClickHandler(buttonGO, onClickedF);
            UnityUIUtil.SetButtonText(button, text);

            if (isEnabledF != null) {
                var enabler = buttonGO.AddComponent<UIWidgetEnabler>();
                enabler.widget = button;
                enabler.IsEnabledF = isEnabledF;
                enabler.Update();
            }

            return button;
        }



        static public Button AddBasicStartToolButton(GameObject popupGO, string text, string toolIdentifier, Func<bool> isEnabledF = null)
        {

            GameObject buttonGO = GameObject.Instantiate(Resources.Load<GameObject>("BasicToolButton"));
            popupGO.AddChild(buttonGO, false);
            buttonGO.SetName(text);
            Button button = UnityUIUtil.GetButtonAndAddClickHandler(buttonGO, () => {
                CCActions.BeginTool(toolIdentifier);
            });
            UnityUIUtil.SetButtonText(button, text);

            if (isEnabledF != null) {
                var enabler = buttonGO.AddComponent<UIWidgetEnabler>();
                enabler.widget = button;
                enabler.IsEnabledF = isEnabledF;
                enabler.Update();
            }

            return button;
        }




        class UIWidgetEnabler : MonoBehaviour
        {
            public Selectable widget;
            public Func<bool> IsEnabledF = () => { return true; };

            public void Update() {
                widget.interactable = IsEnabledF();
            }
        }


    }
}
