
/***
 *  Derived from code under license below
 *  https://github.com/Bunny83/UnityWindowsFileDrag-Drop
 */


/* * * * *
 * This is a collection of Win API helpers. Mainly dealing with window message hooks
 * and file drag&drop support for Windows standalone Unity applications.
 * 
 * The MIT License (MIT)
 * 
 * Copyright (c) 2018 Markus Göbel (Bunny83)
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 * 
 * * * * */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace gs
{
    public class Win32DragDropHook
    {
        public delegate void DroppedFilesEvent(List<string> aPathNames, WinAPI.POINT aDropPoint);
        public event DroppedFilesEvent OnDroppedFiles;

#if G3_USING_UNITY && (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)

        private uint threadId = WinAPI.GetCurrentThreadId();
        private IntPtr mainWindow;
        private WinAPI.HookProc m_Callback;
        private IntPtr m_Hook;

        public Win32DragDropHook(IntPtr mainWin)
        {
            mainWindow = mainWin;
        }



        public bool InstallHook()
        {
            if (mainWindow == IntPtr.Zero)
                return false;
            var hModule = WinAPI.GetModuleHandle(null);
            if (hModule == IntPtr.Zero)
                return false;
            m_Callback = Callback;
            m_Hook = WinAPI.SetWindowsHookEx(WinAPI.HookType.WH_GETMESSAGE, m_Callback, hModule, threadId);
            if (m_Hook == IntPtr.Zero)
                return false;

            // Allow dragging of files onto the main window. generates the WM_DROPFILES message
            WinAPI.DragAcceptFiles(mainWindow, true);
            return true;
        }
        public void UninstallHook()
        {
            if (m_Hook == IntPtr.Zero)
                return;

            WinAPI.DragAcceptFiles(mainWindow, false);

            WinAPI.UnhookWindowsHookEx(m_Hook);
            m_Hook = IntPtr.Zero;
        }


        private IntPtr Callback(int code, IntPtr wParam, ref WinAPI.MSG lParam)
        {
            if (code == 0 && lParam.message == WinAPI.WM.DROPFILES) {
                WinAPI.POINT pos;
                WinAPI.DragQueryPoint(lParam.wParam, out pos);

                // 0xFFFFFFFF as index makes the method return the number of files
                uint n = WinAPI.DragQueryFile(lParam.wParam, 0xFFFFFFFF, null, 0);
                if (n > 0) {
                    var sb = new System.Text.StringBuilder(1024);

                    List<string> result = new List<string>();
                    for (uint i = 0; i < n; i++) {
                        int len = (int)WinAPI.DragQueryFile(lParam.wParam, i, sb, 1024);
                        result.Add(sb.ToString(0, len));
                        sb.Length = 0;
                    }
                    WinAPI.DragFinish(lParam.wParam);
                    if (OnDroppedFiles != null)
                        OnDroppedFiles(result, pos);
                }
            }
            return WinAPI.CallNextHookEx(m_Hook, code, wParam, ref lParam);
        }
#else
        public bool InstallHook()
        {
            return false;
        }
        public void UninstallHook()
        {
        }
#endif

    }

}
