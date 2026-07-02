using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using DuskControl.Helpers;

namespace DuskControl.Services;

public partial class TrayService : IDisposable
{
  private readonly Window _window;
  private readonly IntPtr _hWnd;
  private readonly Win32.SUBCLASSPROC _subclassProc;
  private bool _isDisposed;
  private IntPtr _hIcon = IntPtr.Zero;

  public TrayService(Window window)
  {
    _window = window;
    _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(_window);

    _subclassProc = new Win32.SUBCLASSPROC(WndProc);
    Win32.SetWindowSubclass(_hWnd, _subclassProc, 1, IntPtr.Zero);

    InitializeTrayIcon();
  }

  private void InitializeTrayIcon()
  {
    string iconPath = Path.Combine(AppContext.BaseDirectory, "Assets", "AppIcon.ico");
    _hIcon = Win32.LoadImage(IntPtr.Zero, iconPath, Win32.IMAGE_ICON, 0, 0, Win32.LR_LOADFROMFILE | Win32.LR_DEFAULTSIZE);

    var nid = new Win32.NOTIFYICONDATA
    {
      cbSize = Marshal.SizeOf<Win32.NOTIFYICONDATA>(),
      hWnd = _hWnd,
      uID = 1,
      uFlags = (int)(Win32.NIF_MESSAGE | Win32.NIF_ICON | Win32.NIF_TIP),
      uCallbackMessage = (int)Win32.WM_TRAYICON,
      hIcon = _hIcon,
      szTip = "Dusk Control"
    };

    Win32.Shell_NotifyIcon(Win32.NIM_ADD, ref nid);
  }

  private IntPtr WndProc(IntPtr hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, uint uIdSubclass, IntPtr dwRefData)
  {
    if (uMsg == Win32.WM_TRAYICON)
    {
      var mouseMsg = (uint)lParam.ToInt32();
      if (mouseMsg == Win32.WM_LBUTTONUP)
      {
        if (_window.AppWindow.IsVisible)
        {
          _window.AppWindow.Hide();
        }
        else
        {
          _window.AppWindow.Show();
          Win32.SetForegroundWindow(_hWnd);
        }
      }
      else if (mouseMsg == Win32.WM_RBUTTONUP)
      {
        ShowContextMenu();
      }
    }

    return Win32.DefSubclassProc(hWnd, uMsg, wParam, lParam);
  }

  private void ShowContextMenu()
  {
    IntPtr hMenu = Win32.CreatePopupMenu();
    if (hMenu == IntPtr.Zero) return;

    Win32.AppendMenu(hMenu, Win32.MF_STRING, 1001, "Open Dusk Control");
    Win32.AppendMenu(hMenu, Win32.MF_STRING, 1003, "Reset Position");
    Win32.AppendMenu(hMenu, Win32.MF_STRING, 1002, "Exit");

    Win32.GetCursorPos(out Win32.POINT pt);
    Win32.SetForegroundWindow(_hWnd);

    int selectedId = Win32.TrackPopupMenu(hMenu, Win32.TPM_LEFTALIGN | Win32.TPM_RIGHTBUTTON | Win32.TPM_RETURNCMD, pt.x, pt.y, 0, _hWnd, IntPtr.Zero);
    Win32.DestroyMenu(hMenu);

    if (selectedId == 1001)
    {
      _window.AppWindow.Show();
      Win32.SetForegroundWindow(_hWnd);
    }
    else if (selectedId == 1003)
    {
      if (_window is MainWindow mainWindow)
      {
        mainWindow.ResetPosition();
        _window.AppWindow.Show();
        Win32.SetForegroundWindow(_hWnd);
      }
    }
    else if (selectedId == 1002)
    {
      if (_window is MainWindow mainWindow)
      {
        mainWindow.ExitApplication();
      }
    }
  }

  public void Dispose()
  {
    if (_isDisposed) return;
    GC.SuppressFinalize(this);

    var nid = new Win32.NOTIFYICONDATA
    {
      cbSize = Marshal.SizeOf<Win32.NOTIFYICONDATA>(),
      hWnd = _hWnd,
      uID = 1
    };

    Win32.Shell_NotifyIcon(Win32.NIM_DELETE, ref nid);
    Win32.RemoveWindowSubclass(_hWnd, _subclassProc, 1);

    if (_hIcon != IntPtr.Zero)
    {
      Win32.DestroyIcon(_hIcon);
      _hIcon = IntPtr.Zero;
    }

    _isDisposed = true;
  }
}
