using System.Runtime.InteropServices;

namespace dusk.Helpers;

public static class Win32
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
  public static extern bool Shell_NotifyIcon(uint dwMessage, ref NOTIFYICONDATA lpData);

  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  public static extern IntPtr LoadImage(IntPtr hInst, string lpszName, uint uType, int cx, int cy, uint fuLoad);

  [DllImport("user32.dll")]
  public static extern bool DestroyIcon(IntPtr hIcon);

  [DllImport("user32.dll", SetLastError = true)]
  public static extern IntPtr CreatePopupMenu();

  [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
  public static extern bool AppendMenu(IntPtr hMenu, uint uFlags, uint uIDNewItem, string lpNewItem);

  [DllImport("user32.dll")]
  public static extern bool DestroyMenu(IntPtr hMenu);

  [DllImport("user32.dll")]
  public static extern int TrackPopupMenu(IntPtr hMenu, uint uFlags, int x, int y, int nReserved, IntPtr hWnd, IntPtr prcRect);

  [DllImport("user32.dll")]
  public static extern bool GetCursorPos(out POINT lpPoint);

  [DllImport("user32.dll")]
  public static extern bool SetForegroundWindow(IntPtr hWnd);

  [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
  public static extern bool SetWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass, IntPtr dwRefData);

  [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
  public static extern IntPtr DefSubclassProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam);

  [DllImport("comctl32.dll", CharSet = CharSet.Auto)]
  public static extern bool RemoveWindowSubclass(IntPtr hWnd, SUBCLASSPROC pfnSubclass, uint uIdSubclass);
}
