using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Kamilla
{
    public static class Win32
    {
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct PrivilegeToken
        {
            public int Count;
            public long Luid;
            public int Attr;
        }

        public enum ProcessAccessFlags
        {
            All = 0x1f0fff,
            CreateThread = 2,
            DupHandle = 0x40,
            QueryInformation = 0x400,
            SetInformation = 0x200,
            Synchronize = 0x100000,
            Terminate = 1,
            VMOperation = 8,
            VMRead = 0x10,
            VMWrite = 0x20
        }

        [Flags]
        public enum WindowStyles : uint
        {
            WS_OVERLAPPED      = 0x00000000,
            WS_POPUP           = 0x80000000,
            WS_CHILD           = 0x40000000,
            WS_MINIMIZE        = 0x20000000,
            WS_VISIBLE         = 0x10000000,
            WS_DISABLED        = 0x08000000,
            WS_CLIPSIBLINGS    = 0x04000000,
            WS_CLIPCHILDREN    = 0x02000000,
            WS_MAXIMIZE        = 0x01000000,
            WS_BORDER          = 0x00800000,
            WS_DLGFRAME        = 0x00400000,
            WS_VSCROLL         = 0x00200000,
            WS_HSCROLL         = 0x00100000,
            WS_SYSMENU         = 0x00080000,
            WS_THICKFRAME      = 0x00040000,
            WS_GROUP           = 0x00020000,
            WS_TABSTOP         = 0x00010000,

            WS_MINIMIZEBOX     = 0x00020000,
            WS_MAXIMIZEBOX     = 0x00010000,

            WS_TITLEBARBUTTONS = WS_MINIMIZEBOX | WindowStyles.WS_MAXIMIZEBOX | WindowStyles.WS_SYSMENU,

            WS_CAPTION         = WS_BORDER | WS_DLGFRAME,
            WS_TILED           = WS_OVERLAPPED,
            WS_ICONIC          = WS_MINIMIZE,
            WS_SIZEBOX         = WS_THICKFRAME,
            WS_TILEDWINDOW     = WS_OVERLAPPEDWINDOW,

            WS_OVERLAPPEDWINDOW    = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX,
            WS_POPUPWINDOW     = WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_CHILDWINDOW     = WS_CHILD,

            //Extended Window Styles

            WS_EX_DLGMODALFRAME    = 0x00000001,
            WS_EX_NOPARENTNOTIFY   = 0x00000004,
            WS_EX_TOPMOST      = 0x00000008,
            WS_EX_ACCEPTFILES      = 0x00000010,
            WS_EX_TRANSPARENT      = 0x00000020,

        //#if(WINVER >= 0x0400)

            WS_EX_MDICHILD     = 0x00000040,
            WS_EX_TOOLWINDOW       = 0x00000080,
            WS_EX_WINDOWEDGE       = 0x00000100,
            WS_EX_CLIENTEDGE       = 0x00000200,
            WS_EX_CONTEXTHELP      = 0x00000400,

            WS_EX_RIGHT        = 0x00001000,
            WS_EX_LEFT         = 0x00000000,
            WS_EX_RTLREADING       = 0x00002000,
            WS_EX_LTRREADING       = 0x00000000,
            WS_EX_LEFTSCROLLBAR    = 0x00004000,
            WS_EX_RIGHTSCROLLBAR   = 0x00000000,

            WS_EX_CONTROLPARENT    = 0x00010000,
            WS_EX_STATICEDGE       = 0x00020000,
            WS_EX_APPWINDOW    = 0x00040000,

            WS_EX_OVERLAPPEDWINDOW = (WS_EX_WINDOWEDGE | WS_EX_CLIENTEDGE),
            WS_EX_PALETTEWINDOW    = (WS_EX_WINDOWEDGE | WS_EX_TOOLWINDOW | WS_EX_TOPMOST),

        //#endif /* WINVER >= 0x0400 */

        //#if(WIN32WINNT >= 0x0500)

            WS_EX_LAYERED      = 0x00080000,

        //#endif /* WIN32WINNT >= 0x0500 */

        //#if(WINVER >= 0x0500)

            WS_EX_NOINHERITLAYOUT  = 0x00100000, // Disable inheritence of mirroring by children
            WS_EX_LAYOUTRTL    = 0x00400000, // Right to left mirroring

        //#endif /* WINVER >= 0x0500 */

        //#if(WIN32WINNT >= 0x0500)

            WS_EX_COMPOSITED       = 0x02000000,
            WS_EX_NOACTIVATE       = 0x08000000

        //#endif /* WIN32WINNT >= 0x0500 */

        }

        public const int WM_SETREDRAW = 11;
        public const int GWL_STYLE = -16;
        public const int WS_SYSMENU = 0x80000;

        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool LookupPrivilegeValue(string host, string name, ref long pluid);
        [DllImport("advapi32.dll", SetLastError = true)]
        public static extern bool AdjustTokenPrivileges(IntPtr TokenHandle, bool DisableAllPrivileges,
            ref PrivilegeToken NewState, UInt32 BufferLength, IntPtr PreviousState, IntPtr ReturnLength);
        [DllImport("advapi32.dll", ExactSpelling = true, SetLastError = true)]
        public static extern bool OpenProcessToken(IntPtr h, int acc, ref IntPtr phtok);
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, bool bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int CloseHandle(IntPtr hObject);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        public static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, [Out] byte[] lpBuffer, UIntPtr nSize, IntPtr lpNumberOfBytesRead);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll", SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, Int32 wMsg, bool wParam, Int32 lParam);
        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);
        /// <summary>
        /// Deletes a GDI object.
        /// </summary>
        /// <param name="hObject">
        /// The GDI object to delete.
        /// </param>
        /// <returns>
        /// true if the deletion succeded; otherwise, false.
        /// </returns>
        [DllImport("gdi32.dll")]
        public static extern bool DeleteObject(IntPtr hObject);

        public static void SuspendDrawing(IntPtr hWnd)
        {
            SendMessage(hWnd, WM_SETREDRAW, false, 0);
        }

        public static void ResumeDrawing(IntPtr hWnd)
        {
            SendMessage(hWnd, WM_SETREDRAW, true, 0);
            // control.Refresh(); // !!
        }

        public static bool GrantDebugPrivilege(Process process)
        {
            var token = IntPtr.Zero;
            OpenProcessToken(process.Handle, 0x28, ref token);
            PrivilegeToken new_priv;

            new_priv.Count = 1;
            new_priv.Luid = 0;
            new_priv.Attr = 0x2;

            LookupPrivilegeValue(null, "SeDebugPrivilege", ref new_priv.Luid);
            if (AdjustTokenPrivileges(token, false, ref new_priv, 0x10, IntPtr.Zero, IntPtr.Zero))
            {
                Console.WriteLine("Granted debug privilege to process {0}.", process.Id);
                return true;
            }

            Console.WriteLine("Error: Failed to grant debug privilege to process {0}", process.Id);
            return false;
        }
    }
}
