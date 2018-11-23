using System;
using System.Collections.Generic;
using g3;
using f3;
using gs;

namespace cotangent
{
    public static partial class CCActions
    {
        public static bool InTool {
            get { return CC.ActiveContext.ToolManager.HasActiveTool(ToolSide.Right); }
        }


        public static void BeginTool(string identifier)
        {
            AcceptAndExitCurrentTool();

            CC.ActiveContext.ToolManager.SetActiveToolType(identifier, ToolSide.Right);
            CC.ActiveContext.ToolManager.ActivateTool(ToolSide.Right);

            CotangentAnalytics.StartTool(identifier);
        }


        public static void AcceptAndExitCurrentTool()
        {
            if (InTool) {
                ITool tool = CC.ActiveContext.ToolManager.ActiveRightTool;
                if (tool.HasApply && tool.CanApply) {
                    tool.Apply();
                }
                CancelCurrentTool();

                // run gc
                FPlatform.SuggestGarbageCollection();
            }
        }



        public static void CancelCurrentTool()
        {
            if (InTool == false)
                return;

            if (CCStatus.InOperation)
                CCStatus.EndOperation("working...");
            CC.ActiveContext.ToolManager.DeactivateTool(ToolSide.Right);

            // run gc
            FPlatform.SuggestGarbageCollection();
        }



        public static void UpdateCurrentToolStatus()
        {
            // [RMS] this is a quick-and-dirty way to give some feedback when computing ops.
            // todo: improve in future!
            ITool active = CC.ActiveContext.ToolManager.GetActiveTool(ToolSide.Right);
            if (active != null && active.Parameters.HasParameter("computing")) {
                bool computing = active.Parameters.GetValueBool("computing");
                if (CCStatus.InOperation == false && computing)
                    CCStatus.BeginOperation("working...");
                else if (CCStatus.InOperation == true && computing == false)
                    CCStatus.EndOperation("working...");
            }
        }

    }
}
