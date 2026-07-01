using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using dusk.Helpers;

namespace dusk.Services;

public class OverlayService
{
  private readonly Dictionary<IntPtr, IntPtr> _overlays = new();
  private readonly Win32.WndProcDelegate _wndProc;
  private readonly string _className = "DuskOverlayClass";
  private bool _classRegistered = false;

  public OverlayService()
  {
    _wndProc = WndProc;
  }

  private void EnsureClassRegistered()
  {
    if (_classRegistered) return;
    _classRegistered = true;

    var wndClass = new Win32.WNDCLASSEX
    {
      cbSize = (uint)Marshal.SizeOf<Win32.WNDCLASSEX>(),
      lpfnWndProc = _wndProc,
      hInstance = Win32.GetModuleHandle(null),
      lpszClassName = _className,
      hbrBackground = Win32.CreateSolidBrush(0x000000) // Black
    };

    Win32.RegisterClassEx(ref wndClass);
  }

  public void UpdateOverlay(IntPtr hMonitor, double opacity)
  {
    if (opacity <= 0)
    {
      if (_overlays.TryGetValue(hMonitor, out var hwnd))
      {
        Win32.DestroyWindow(hwnd);
        _overlays.Remove(hMonitor);
      }
      return;
    }

    EnsureClassRegistered();

    if (!_overlays.TryGetValue(hMonitor, out var overlayHwnd))
    {
      var mi = new Win32.MONITORINFOEX();
      mi.cbSize = Marshal.SizeOf<Win32.MONITORINFOEX>();
      if (Win32.GetMonitorInfo(hMonitor, ref mi))
      {
        overlayHwnd = Win32.CreateWindowEx(
          Win32.WS_EX_LAYERED | Win32.WS_EX_TRANSPARENT | Win32.WS_EX_TOOLWINDOW | Win32.WS_EX_TOPMOST,
          _className,
          "DuskOverlay",
          Win32.WS_POPUP,
          mi.rcMonitor.left,
          mi.rcMonitor.top,
          mi.rcMonitor.right - mi.rcMonitor.left,
          mi.rcMonitor.bottom - mi.rcMonitor.top,
          IntPtr.Zero,
          IntPtr.Zero,
          Win32.GetModuleHandle(null),
          IntPtr.Zero
        );

        if (overlayHwnd != IntPtr.Zero)
        {
          _overlays[hMonitor] = overlayHwnd;
          // Show without activating
          Win32.SetWindowPos(overlayHwnd, (IntPtr)(-1), 0, 0, 0, 0, Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER | 0x0001 | 0x0002);
          Win32.SetWindowPos(overlayHwnd, IntPtr.Zero, 0, 0, 0, 0, 0x0040 | 0x0001 | 0x0002 | Win32.SWP_NOACTIVATE); // SWP_SHOWWINDOW
        }
      }
    }

    if (overlayHwnd != IntPtr.Zero)
    {
      byte alpha = (byte)Math.Clamp(opacity * 255.0, 0, 255);
      Win32.SetLayeredWindowAttributes(overlayHwnd, 0, alpha, Win32.LWA_ALPHA);
    }
  }

  private IntPtr WndProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
  {
    return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
  }
}
