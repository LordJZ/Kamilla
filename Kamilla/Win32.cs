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

        public const int WM_SETREDRAW = 11;
        public const int GWL_STYLE = -16;
        public const int WS_SYSMENU = 0x80000;
        public const int WS_MINIMIZE = 131072;
        public const int WS_MAXIMIZE = 65536;

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
