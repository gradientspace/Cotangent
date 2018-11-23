using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using f3;
using g3;

namespace cotangent
{
    public static class CotangentUI
    {
        static FContext context;
        public static void Initialize(FContext context) {
            CotangentUI.context = context;
        }

        public static Colorf NormalSettingColor = Colorf.White;
        public static Colorf ModifiedSettingColor = Colorf.Gold;
        public static Colorf DisabledButtonColor = Colorf.Silver;

        public static Colorf ButtonTextColor = Colorf.VideoBlack;
		public static Colorf ButtonBGColor = Colorf.BlueMetal;


        public static float PixelScale {
            get { return context.ActiveCockpit.GetPixelScale(); }
        }


        public static float StandardPanelCornerRadius() {
            return 5 * PixelScale;
        }
        public static float StandardButtonBorderWidth {
            get { return 1.5f * PixelScale; }
        }


        public static float MenuButtonWidth {
            get { return 125 * PixelScale; }
        }
        public static float MenuButtonHeight {
            get { return 30 * PixelScale; }
        }
        public static float MenuButtonTextHeight {
            get { return 20 * PixelScale; }
        }

        public static HUDShape MakeMenuButtonRect(float width, float height)
        {
//            return new HUDShape(HUDShapeType.RoundRect, width, height, height * 0.25f, 6, false);
			return new HUDShape(HUDShapeType.Rectangle, width, height);
		}





        public static GameObject MainUICanvas {
            get { return UnityUtil.FindGameObjectByName("ViewUICanvas"); }
        }
        public static GameObject PrintUICanvas {
            get { return UnityUtil.FindGameObjectByName("PrintUICanvas"); }
        }


        // hack to keep track of our hud items used in print view, that we need to hide in other views
        // (eg radial progress)
        public static List<HUDStandardItem> PrintViewHUDItems = new List<HUDStandardItem>();





        public static void GetStringFromDialog(string title, string instructions, object target,
            Func<string,bool> ValidatorF, Action<string, object> onAccept, Action onCancel)
        {
            GameObject dialog = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("GetStringDialog"));

            UnityUIUtil.FindTextAndSet(dialog, "Title", title);
            UnityUIUtil.FindTextAndSet(dialog, "InfoText", instructions);

            Button cancelButton = UnityUIUtil.FindButtonAndAddClickHandler(dialog, "Cancel", () => {
                if (onCancel != null)
                    onCancel();
                context.RegisterNextFrameAction(() => {
                    GameObject.Destroy(dialog);
                });
            });

            var input = UnityUIUtil.FindInput(dialog, "TextEntry");

            Button okButton = UnityUIUtil.FindButtonAndAddClickHandler(dialog, "OK", () => {
                if (input.text.Length == 0)
                    return;
                if (ValidatorF != null && ValidatorF(input.text) == false)
                    return;

                onAccept(input.text, target);
                context.RegisterNextFrameAction(() => {
                    GameObject.Destroy(dialog);
                });
            });

            input.Select();

            MainUICanvas.AddChild(dialog, false);
        }







        public static void ShowModalMessageDialog(
            string title, string message,
            string confirmText, 
            object target,
            Action<object> onAccept)
        {
            GameObject dialog = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("ModalMessageDialog"));

            UnityUIUtil.FindTextAndSet(dialog, "Title", title);
            UnityUIUtil.FindTextAndSet(dialog, "InfoText", message);
            UnityUIUtil.FindTextAndSet(dialog, "OKText", confirmText);

            Button okButton = UnityUIUtil.FindButtonAndAddClickHandler(dialog, "OK", () => {
                onAccept?.Invoke(target);
                context.RegisterNextFrameAction(() => {
                    GameObject.Destroy(dialog);
                });
            });

            okButton.Select();
            MainUICanvas.AddChild(dialog, false);
        }





        public static void ShowModalConfirmDialog(
            string title, string instructions, 
            string confirmText, string cancelText, 
            object target,
            Action<object> onAccept, Action<object> onCancel)
        {
            GameObject dialog = GameObject.Instantiate<GameObject>(Resources.Load<GameObject>("ModalConfirmCancelDialog"));

            UnityUIUtil.FindTextAndSet(dialog, "Title", title);
            UnityUIUtil.FindTextAndSet(dialog, "InfoText", instructions);
            UnityUIUtil.FindTextAndSet(dialog, "OKText", confirmText);
            UnityUIUtil.FindTextAndSet(dialog, "CancelText", cancelText);

            Button cancelButton = UnityUIUtil.FindButtonAndAddClickHandler(dialog, "Cancel", () => {
                if (onCancel != null)
                    onCancel(target);
                context.RegisterNextFrameAction(() => {
                    GameObject.Destroy(dialog);
                });
            });

            Button okButton = UnityUIUtil.FindButtonAndAddClickHandler(dialog, "OK", () => {
                onAccept(target);
                context.RegisterNextFrameAction(() => {
                    GameObject.Destroy(dialog);
                });
            });

            okButton.Select();

            MainUICanvas.AddChild(dialog, false);
        }










        public static void HideSliceLabel()
        {
            var go = PrintUICanvas.FindChildByName("SliceLabel", true);
            if (go != null && go.IsVisible()) {
                go.GetComponent<Image>().CrossFadeAlpha(0, 2.0f, true);
                //go.SetVisible(false);
            }
        }




    }
}
