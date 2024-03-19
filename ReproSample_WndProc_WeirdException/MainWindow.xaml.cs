using Microsoft.UI;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Windows.System;
using WinRT.Interop;

namespace ReproSample_WndProc_WeirdException;

public sealed partial class MainWindow : Window
{
    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    [DllImport("kernel32.dll")]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
    public static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    [DllImport("user32.dll", EntryPoint = "SetWindowLong")] //32-bit
    public static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
    public delegate IntPtr WndProcDelegate(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam);
    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    public static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hwnd, uint msg, IntPtr wParam, IntPtr lParam);
    private IntPtr _oldWndProc;
    public static IntPtr hWnd;

    private const int GWLP_WNDPROC = -4;
    private const int HOTKEY_ID = 0;
    private const int WM_HOTKEY = 0x0312;

    public MainWindow()
    {
        this.InitializeComponent();

        hWnd = WindowNative.GetWindowHandle(this);
        WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);

        _oldWndProc = SetWndProc(WindowProcess);

        //Register hotkey:
        RegisterHotKey(hWnd, HOTKEY_ID, (int)(VirtualKeyModifiers.Shift | VirtualKeyModifiers.Control), (int)VirtualKey.Space);
    }
    public IntPtr SetWndProc(WndProcDelegate newProc)
    {
        IntPtr functionPointer = Marshal.GetFunctionPointerForDelegate(newProc);
        if (IntPtr.Size == 8) //64 or 32bit. So the problem is probably here:
            return SetWindowLongPtr(hWnd, GWLP_WNDPROC, functionPointer);
        else
            return SetWindowLong(hWnd, GWLP_WNDPROC, functionPointer);
    }

    private IntPtr WindowProcess(IntPtr hwnd, uint message, IntPtr wParam, IntPtr lParam)
    {
        if (message == WM_HOTKEY && (int)wParam == HOTKEY_ID)
        {
            Debug.WriteLine("Hotkey pressed");
        }

        return CallWindowProc(_oldWndProc, hwnd, message, wParam, lParam);
    }
}
