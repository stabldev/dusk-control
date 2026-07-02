using System.Runtime.InteropServices;

namespace DuskControl.Helpers;

internal static partial class Win32
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

  public static unsafe void SetFixedString(char* dest, int destLength, string value)
  {
    if (value == null) { dest[0] = '\0'; return; }
    int len = Math.Min(value.Length, destLength - 1);
    value.AsSpan(0, len).CopyTo(new Span<char>(dest, len));
    dest[len] = '\0';
  }

  [StructLayout(LayoutKind.Sequential)]
  public unsafe struct NOTIFYICONDATA
  {
    public int cbSize;
    public IntPtr hWnd;
    public int uID;
    public int uFlags;
    public int uCallbackMessage;
    public IntPtr hIcon;
    public fixed char szTip[128];
    public int dwState;
    public int dwStateMask;
    public fixed char szInfo[256];
    public int uTimeoutOrVersion;
    public fixed char szInfoTitle[64];
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

  [LibraryImport("shell32.dll", EntryPoint = "Shell_NotifyIconW")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

  [LibraryImport("user32.dll", EntryPoint = "LoadImageW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  internal static partial IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cx, int cy, uint fuLoad);

  [LibraryImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool DestroyIcon(IntPtr hIcon);

  [LibraryImport("user32.dll", SetLastError = true)]
  internal static partial IntPtr CreatePopupMenu();

  [LibraryImport("user32.dll", EntryPoint = "AppendMenuW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

  [LibraryImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool DestroyMenu(IntPtr hMenu);

  [LibraryImport("user32.dll")]
  internal static partial int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

  [LibraryImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool GetCursorPos(out POINT lpPoint);

  [LibraryImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool SetForegroundWindow(IntPtr hWnd);

  [LibraryImport("comctl32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

  [LibraryImport("comctl32.dll")]
  internal static partial IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

  [LibraryImport("comctl32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool RemoveWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass);

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

  [StructLayout(LayoutKind.Sequential)]
  internal unsafe struct MONITORINFOEX
  {
    public int cbSize;
    public RECT rcMonitor;
    public RECT rcWork;
    public int dwFlags;
    public fixed char szDevice[32];

    public string DeviceName
    {
      get
      {
        fixed (char* p = szDevice)
          return new string(p);
      }
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  internal unsafe struct DISPLAY_DEVICE
  {
    public int cb;
    public fixed char DeviceName[32];
    public fixed char DeviceString[128];
    public uint StateFlags;
    public fixed char DeviceID[128];
    public fixed char DeviceKey[128];

    public string DeviceIDStr { get { fixed (char* p = DeviceID) return new string(p); } }
    public string DeviceStringStr { get { fixed (char* p = DeviceString) return new string(p); } }
  }

  [StructLayout(LayoutKind.Sequential)]
  internal unsafe struct PHYSICAL_MONITOR
  {
    public IntPtr hPhysicalMonitor;
    public fixed char szPhysicalMonitorDescription[128];
  }

  internal delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

  [LibraryImport("user32.dll", EntryPoint = "GetMonitorInfoW")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

  [LibraryImport("user32.dll", EntryPoint = "EnumDisplayDevicesW", StringMarshalling = StringMarshalling.Utf16)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool EnumDisplayDevices(string? lpDevice, uint iDevNum, ref DISPLAY_DEVICE lpDisplayDevice, uint dwFlags);

  [LibraryImport("dxva2.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool GetNumberOfPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, ref uint pdwNumberOfPhysicalMonitors);

  [LibraryImport("dxva2.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool GetPhysicalMonitorsFromHMONITOR(IntPtr hMonitor, uint dwPhysicalMonitorArraySize, [Out] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

  [LibraryImport("dxva2.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool DestroyPhysicalMonitors(uint dwPhysicalMonitorArraySize, [In] PHYSICAL_MONITOR[] pPhysicalMonitorArray);

  [LibraryImport("dxva2.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool GetMonitorBrightness(IntPtr hMonitor, out uint pdwMinimumBrightness, out uint pdwCurrentBrightness, out uint pdwMaximumBrightness);

  [LibraryImport("dxva2.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool SetMonitorBrightness(IntPtr hMonitor, uint dwNewBrightness);

  [LibraryImport("dxva2.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool GetMonitorContrast(IntPtr hMonitor, out uint pdwMinimumContrast, out uint pdwCurrentContrast, out uint pdwMaximumContrast);

  [LibraryImport("dxva2.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool SetMonitorContrast(IntPtr hMonitor, uint dwNewContrast);

  [LibraryImport("user32.dll")]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

  // Overlay API
  public delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

  [StructLayout(LayoutKind.Sequential)]
  public struct WNDCLASSEX
  {
    public uint cbSize;
    public uint style;
    public IntPtr lpfnWndProc;
    public int cbClsExtra;
    public int cbWndExtra;
    public IntPtr hInstance;
    public IntPtr hIcon;
    public IntPtr hCursor;
    public IntPtr hbrBackground;
    public IntPtr lpszMenuName;
    public IntPtr lpszClassName;
    public IntPtr hIconSm;
  }

  [LibraryImport("user32.dll", EntryPoint = "RegisterClassExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  internal static partial ushort RegisterClassEx(ref WNDCLASSEX lpwcx);

  [LibraryImport("user32.dll", EntryPoint = "CreateWindowExW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
  internal static partial IntPtr CreateWindowEx(
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

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool DestroyWindow(IntPtr hWnd);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

  [LibraryImport("user32.dll", EntryPoint = "DefWindowProcW")]
  internal static partial IntPtr DefWindowProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

  [LibraryImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  internal static partial bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

  [LibraryImport("kernel32.dll", EntryPoint = "GetModuleHandleW", StringMarshalling = StringMarshalling.Utf16)]
  internal static partial IntPtr GetModuleHandle(string? lpModuleName);

  [LibraryImport("gdi32.dll")]
  internal static partial IntPtr CreateSolidBrush(uint crColor);
}
