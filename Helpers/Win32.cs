#pragma warning disable SYSLIB1054
using System.Runtime.InteropServices;

namespace DuskControl.Helpers;

internal static class Win32
{
  public const uint WM_USER = 0x0400;
  public const uint WM_TRAYICON = WM_USER + 101;
  public const uint WM_LBUTTONUP = 0x0202;
  public const uint WM_RBUTTONUP = 0x0205;

  public const uint NIM_ADD = 0x00000000;
  public const uint NIM_MODIFY = 0x00000001;
  public const uint NIM_DELETE = 0x00000002;

  public const uint NIF_MESSAGE = 0x00000001;
  public const uint NIF_ICON = 0x00000002;
  public const uint NIF_TIP = 0x00000004;

  public const uint MF_STRING = 0x00000000;
  public const uint TPM_RETURNCMD = 0x0100;
  public const uint TPM_LEFTALIGN = 0x0000;
  public const uint TPM_RIGHTBUTTON = 0x0002;

  public const uint IMAGE_ICON = 1;
  public const uint LR_LOADFROMFILE = 0x00000010;
  public const uint LR_DEFAULTSIZE = 0x00000040;

  public const uint WS_EX_LAYERED = 0x00080000;
  public const uint WS_EX_TRANSPARENT = 0x00000020;
  public const uint WS_EX_TOOLWINDOW = 0x00000080;
  public const uint WS_EX_TOPMOST = 0x00000008;
  public const uint WS_POPUP = 0x80000000;
  public const uint LWA_ALPHA = 0x00000002;
  public const int SWP_NOZORDER = 0x0004;
  public const int SWP_NOACTIVATE = 0x0010;

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct NOTIFYICONDATA
  {
    public int cbSize;
    public IntPtr hWnd;
    public int uID;
    public int uFlags;
    public int uCallbackMessage;
    public IntPtr hIcon;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string szTip;
    public int dwState;
    public int dwStateMask;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
    public string szInfo;
    public int uTimeoutOrVersion;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 64)]
    public string szInfoTitle;
    public int dwInfoFlags;
    public Guid guidItem;
    public IntPtr hBalloonIcon;
  }

  [StructLayout(LayoutKind.Sequential)]
  public struct POINT
  {
    public int x;
    public int y;
  }

  public delegate IntPtr SUBCLASSPROC(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData);

  [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
  internal static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

  [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  internal static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cx, int cy, uint fuLoad);

  [DllImport("user32.dll")]
  internal static extern bool DestroyIcon(IntPtr hIcon);

  [DllImport("user32.dll", SetLastError = true)]
  internal static extern IntPtr CreatePopupMenu();

  [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  internal static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

  [DllImport("user32.dll")]
  internal static extern bool DestroyMenu(IntPtr hMenu);

  [DllImport("user32.dll")]
  internal static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

  [DllImport("user32.dll")]
  internal static extern bool GetCursorPos(out POINT lpPoint);

  [DllImport("user32.dll")]
  internal static extern bool SetForegroundWindow(IntPtr hWnd);

  [DllImport("comctl32.dll")]
  internal static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

  [DllImport("comctl32.dll")]
  internal static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

  [DllImport("comctl32.dll")]
  internal static extern bool RemoveWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass);

  // Monitor APIs
  public const int MONITORINFOF_PRIMARY = 1;

  [StructLayout(LayoutKind.Sequential)]
  internal struct RECT
  {
    public int left;
    public int top;
    public int right;
    public int bottom;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  internal struct MONITORINFOEX
  {
    public int cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public int dwFlags;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string szDevice;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  internal struct DISPLAY_DEVICE
  {
    public int cb;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
    public string DeviceName;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceString;
    public uint StateFlags;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceID;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string DeviceKey;
  }

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  internal struct PHYSICAL_MONITOR
  {
    public IntPtr hPhysicalMonitor;
    [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
    public string szPhysicalMonitorDescription;
  }

  internal delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

  [DllImport("user32.dll", CharSet = CharSet.Unicode)]
  internal static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

  [DllImport("user32.dll", CharSet = CharSet.Unicode)]
  internal static extern bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

  [DllImport("dxva2.dll", SetLastError = true)]
  internal static extern bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

  [DllImport("dxva2.dll", SetLastError = true, CharSet = CharSet.Unicode)]
  internal static extern bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

  [DllImport("dxva2.dll", SetLastError = true)]
  internal static extern bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

  [DllImport("dxva2.dll", SetLastError = true)]
  internal static extern bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

  [DllImport("dxva2.dll", SetLastError = true)]
  internal static extern bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

  [DllImport("dxva2.dll", SetLastError = true)]
  internal static extern bool GetMonitorContrast(IntPtr hMonitor, out uint pdwMinimumContrast, out uint pdwCurrentContrast, out uint pdwMaximumContrast);

  [DllImport("dxva2.dll", SetLastError = true)]
  internal static extern bool SetMonitorContrast(IntPtr hMonitor, uint dwNewContrast);

  [DllImport("user32.dll")]
  internal static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

  // Overlay API
  public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

  [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
  public struct WNDCLASSEX
  {
    public uint cbSize;
    public uint style;
    public WndProcDelegate lpfnWndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    public string lpszMenuName;
    public string lpszClassName;
    public IntPtr hIconSm;
  }

  [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  internal static extern ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

  [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
  internal static extern IntPtr CreateWindowEx(
      uint dwExStyle,
      string lpClassName,
      string lpWindowName,
      uint dwStyle,
      int x,
      int y,
      int nWidth,
      int nHeight,
      IntPtr hWndParent,
      IntPtr hMenu,
      IntPtr hInstance,
      IntPtr lpParam);

  [DllImport("user32.dll", SetLastError = true)]
  internal static extern bool DestroyWindow(IntPtr hWnd);

  [DllImport("user32.dll", SetLastError = true)]
  internal static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

  [DllImport("user32.dll")]
  internal static extern IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

  [DllImport("user32.dll", SetLastError = true)]
  internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

  [DllImport("kernel32.dll")]
  internal static extern IntPtr GetModuleHandle(string? lpModuleName);

  [DllImport("gdi32.dll")]
  internal static extern IntPtr CreateSolidBrush(uint crColor);
}
