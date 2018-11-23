using System;
using f3;
using g3;

namespace cotangent
{
    public static class CCState
    {
        /// <summary>
        /// transform applied to "first" imported scene object, based on 
        /// CCPreferences.ImportTransformMode
        /// </summary>
        public static Vector3d SceneImportTransform = Vector3d.Zero;


        /// <summary>
        /// is wireframe currently enabled
        /// </summary>
        public static bool WireframeEnabled = false;



        /// <summary>
        /// is clip plane currently enabled
        /// </summary>
        public static bool ClipPlaneEnabled = false;


        /// <summary>
        /// clip plane frame
        /// </summary>
        public static Frame3f ClipPlaneFrameS = Frame3f.Identity;

    }
}