using System.Runtime.InteropServices;
using System.Text;
using System;

namespace StayFocused
{
    public static class WinApi
    {
        // Windows API functions for retrieving the active window's title
        [DllImport("user32.dll")]
        public static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

        [DllImport("psapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool GetModuleFileNameEx(IntPtr hProcess, IntPtr hModule, [Out] StringBuilder lpBaseName, int nSize);

        public static string GetWindowTitle(IntPtr handle)
        {
            const int nChars = 256;
            StringBuilder sb = new StringBuilder(nChars);
            WinApi.GetWindowText(handle, sb, nChars);
            return sb.ToString();
        }
    }
}
