using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.Display;
using Microsoft.UI.Windowing;
using dusk.Models;
using dusk.Helpers;

namespace dusk.Services;

public class MonitorService
{
  public List<MonitorInfo> GetAvailableMonitors()
  {
    var monitors = new List<MonitorInfo>();
    int index = 1;

    Win32.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Win32.RECT lprcMonitor, IntPtr dwData)
    {
      var mi = new Win32.MONITORINFOEX();
      mi.cbSize = Marshal.SizeOf(mi);
      if (Win32.GetMonitorInfo(hMonitor, ref mi))
      {
        bool isPrimary = (mi.dwFlags & Win32.MONITORINFOF_PRIMARY) != 0;
        string friendlyName = GetMonitorName(hMonitor);
        string displayName = $"Display {index} ({friendlyName})";

        monitors.Add(new MonitorInfo
        {
          Name = displayName,
          IsPrimary = isPrimary,
          DisplayArea = null // Removed DisplayArea usage due to WinRT crash
        });
        index++;
      }
      return true; // Continue enumeration
    }, IntPtr.Zero);

    return monitors;
  }

  private string GetMonitorName(IntPtr hMonitor)
  {
    var mi = new Win32.MONITORINFOEX();
    mi.cbSize = Marshal.SizeOf(mi);

    if (!Win32.GetMonitorInfo(hMonitor, ref mi))
      return "Unknown Monitor";

    var dd = new Win32.DISPLAY_DEVICE();
    dd.cb = Marshal.SizeOf(dd);

    // Pass 1 (EDD_GET_DEVICE_INTERFACE_NAME) to get the device interface path in DeviceID
    if (!Win32.EnumDisplayDevices(mi.szDevice, 0, ref dd, 1))
      return mi.szDevice;

    if (!string.IsNullOrEmpty(dd.DeviceID))
    {
      try
      {
        var task = DisplayMonitor.FromInterfaceIdAsync(dd.DeviceID).AsTask();
        task.Wait(200); // 200ms timeout to prevent UI hang

        if (task.IsCompleted && task.Result != null && !string.IsNullOrEmpty(task.Result.DisplayName))
        {
          return task.Result.DisplayName;
        }
      }
      catch
      {
        // Fallback below
      }
    }

    if (!string.IsNullOrEmpty(dd.DeviceString))
      return dd.DeviceString;

    return mi.szDevice;
  }
}
