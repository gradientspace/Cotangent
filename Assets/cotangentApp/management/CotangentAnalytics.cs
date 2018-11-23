using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine.Analytics;

namespace cotangent
{
    public static class CotangentAnalytics
    {
        public static bool ENABLE_ANALYTICS = true;

        static public void StartTool(string sToolIdentifier)
        {
            Analytics.CustomEvent("start_tool_"+sToolIdentifier);
        }

        static public void ExportMesh(string sFilename)
        {
            string sExt = Path.GetExtension(sFilename);
            Analytics.CustomEvent("export_mesh", new Dictionary<string, object> {
                { "format", sExt }
            });
        }

        static public void ExportGCode()
        {
            Analytics.CustomEvent("export_gcode");
        }




    }
}
