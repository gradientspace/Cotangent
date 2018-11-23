using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using f3;

namespace cotangent
{
    public static class CCStatus
    {
        static string current_operation = "";
        public static string CurrentOperation {
            get { return current_operation; }
        }

        static bool in_operation = false;
        public static bool InOperation {
            get { return in_operation; }
        }

        public static void BeginOperation(string name)
        {
            current_operation = name;
            in_operation = true;
        }

        public static void EndOperation(string name)
        {
            in_operation = false;
        }

    }
}
