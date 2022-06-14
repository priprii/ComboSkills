using System;
using System.Runtime.InteropServices;

namespace ComboSkills {
    public class WinApi {
        [DllImport("kernel32.dll")] public static extern IntPtr LoadLibrary(string lpFileName);
        [DllImport("user32.dll")] public static extern IntPtr SetWindowsHookEx(int idHook, KeyHookProc callback, IntPtr hInstance, uint threadId);
        [DllImport("user32.dll")] public static extern bool UnhookWindowsHookEx(IntPtr hInstance);
        [DllImport("user32.dll")] public static extern int CallNextHookEx(IntPtr idHook, int nCode, int wParam, ref KeyHookData lParam);
        [DllImport("user32.dll", SetLastError = true)] public static extern void keybd_event(byte bVk, byte bScan, int dwFlags, int dwExtraInfo);
    }
}
