using System;
using System.Runtime.InteropServices;

namespace LFNet.Common.Win32
{
    public class WindowUtils
    {
        public static void Restore(IntPtr hWnd)
        {
            ShowWindow(hWnd, ShowWindowCommands.Restore);
        }

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, ShowWindowCommands nCmdShow);

        private enum ShowWindowCommands
        {
            /// <summary>
            /// Activates and displays the window. If the window is minimized or 
            /// maximized, the system restores it to its original size and position. 
            /// An application should specify this flag when restoring a minimized window.
            /// </summary>
            Restore = 9
        }
    }
}

