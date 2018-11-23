using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace gs {

    /// <summary>
    /// This class wraps Win32DragDropHook, which injects a file drag-drop handler into the running Unity app
    /// Currently only works on Windows. Properly handles Unicode filenames.
    /// 
    /// Call Initialize() to enable dropping, then register a handler with OnDroppedFilesEvent.
    /// 
    /// Call Shutdown() to disable dropping. You probably should also call this in 
    ///    some Monobehavior's OnApplicationQuit() override, but it's not a disaster if you don't.
    /// 
    /// It's probably not a great idea to enable this in the Unity Editor. 
    /// It does seem to work, but the DLL is never unloaded, and it's not clear
    /// what happens when we repeatedly install the Win32 hooks...
    /// 
    /// </summary>
    public class DragDropHandler
    {
        static bool is_initialized = false;
        static Win32DragDropHook Hook = null;


        /// <summary>
        /// If this is true, we should be receiving Win32 drop events and emitting OnDroppedFilesEvent
        /// </summary>
        public static bool IsInitialized {
            get { return is_initialized; }
        }

        /// <summary>
        /// Call this to turn on receiving drop events. 
        /// If this returns false, something has gone wrong.
        /// </summary>
        public static bool Initialize(uint threadID = 0, string windowClassName = null)
        {
            if (is_initialized)
                return true;

            IntPtr mainWin = (threadID > 0 && windowClassName != null) ?
                WinAPI.GetMainWindow(threadID, windowClassName) :
                WinAPI.GetActiveWindow();
            //IntPtr mainWin = WinAPI.GetActiveWindow();
            ////if (threadId > 0)
            ////    mainWindow = GetMainWindow(threadId, "UnityWndClass");

            if (Hook == null) {
                Hook = new Win32DragDropHook(mainWin);
                Hook.OnDroppedFiles += (filenames, point) => {
                    if (OnDroppedFilesEvent != null)
                        OnDroppedFilesEvent.Invoke(filenames);
                };
            }

            if ( Hook.InstallHook() ) {
                f3.DebugUtil.Log("Hook installed!");
                is_initialized = true;
                return true;
            } else {
                f3.DebugUtil.Log("Hook install failed?");
                return false;
            }
        }

        /// <summary>
        /// Call this to disable drop events.
        /// </summary>
        public static void Shutdown()
        {
            if (is_initialized) {
                Hook.UninstallHook();
            }
        }


        public delegate void OnDroppedFilesHandler(List<string> filenames);

        /// <summary>
        /// Register a handler with this event to hear about dropped files
        /// </summary>
        public static OnDroppedFilesHandler OnDroppedFilesEvent;


    }


}