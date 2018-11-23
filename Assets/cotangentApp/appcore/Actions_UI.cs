using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using g3;
using f3;

namespace cotangent
{
    public enum AppViewMode
    {
        PrintView,
        RepairView,
        ModelView
    }


    public static partial class CCActions
    {
        const string RepairCanvasName = "RepairUICanvas";
        const string PrintCanvasName = "PrintUICanvas";
        const string ModelCanvasName = "ModelUICanvas";


        static AppViewMode _view_mode = AppViewMode.PrintView;
        public static AppViewMode CurrentViewMode {
            get { return _view_mode; }
            set { SwitchToViewMode(value); }
        }


        public static void SwitchToViewMode(AppViewMode mode, bool bForceUpdate = false)
        {
            if (bForceUpdate == false && CurrentViewMode == mode)
                return;

            // cannot change view if tool is active
            if (CC.ActiveContext.ToolManager.HasActiveTool(ToolSide.Right))
                return;

            _view_mode = mode;
            if (mode == AppViewMode.PrintView) {
                SwitchToPrintView();
            } else if ( mode == AppViewMode.RepairView ) {
                SwitchToRepairView();
            } else if (mode == AppViewMode.ModelView ) {
                SwitchToModelView();
            }

        }



        private static void SwitchToPrintView()
        {
            // change canvas visibility
            UnityUtil.FindGameObjectByName(RepairCanvasName).SetVisible(false);
            UnityUtil.FindGameObjectByName(ModelCanvasName).SetVisible(false);
            UnityUtil.FindGameObjectByName(PrintCanvasName).SetVisible(true);

            foreach (var huditem in CotangentUI.PrintViewHUDItems)
                huditem.IsVisible = true;
        }


        private static void SwitchToRepairView()
        {
            // change canvas visibility
            UnityUtil.FindGameObjectByName(PrintCanvasName).SetVisible(false);
            UnityUtil.FindGameObjectByName(ModelCanvasName).SetVisible(false);
            UnityUtil.FindGameObjectByName(RepairCanvasName).SetVisible(true);

            foreach (var huditem in CotangentUI.PrintViewHUDItems)
                huditem.IsVisible = false;
        }


        private static void SwitchToModelView()
        {
            // change canvas visibility
            UnityUtil.FindGameObjectByName(PrintCanvasName).SetVisible(false);
            UnityUtil.FindGameObjectByName(RepairCanvasName).SetVisible(false);
            UnityUtil.FindGameObjectByName(ModelCanvasName).SetVisible(true);

            foreach (var huditem in CotangentUI.PrintViewHUDItems)
                huditem.IsVisible = false;
        }


    }
}
