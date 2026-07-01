using System;
using System.Collections.Generic;
using System.Management;
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
          DisplayArea = null, // Removed DisplayArea usage due to WinRT crash
          HMonitor = hMonitor
        });
        index++;
      }
      return true; // Continue enumeration
    }, IntPtr.Zero);

    return monitors;
  }

  public uint? GetBrightness(IntPtr hMonitor)
  {
    uint count = 0;
    if (Win32.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref count) && count > 0)
    {
      var physicalMonitors = new Win32.PHYSICAL_MONITOR[count];
      if (Win32.GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors))
      {
        uint currentBrightness = 50;
        bool success = Win32.GetMonitorBrightness(physicalMonitors[0].hPhysicalMonitor, out uint min, out currentBrightness, out uint max);
        Win32.DestroyPhysicalMonitors(count, physicalMonitors);
        if (success)
          return currentBrightness;
      }
    }
    return GetWmiBrightness();
  }

  private uint? GetWmiBrightness()
  {
    try
    {
      using var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightness");
      foreach (ManagementObject queryObj in searcher.Get())
      {
        return (byte)queryObj["CurrentBrightness"];
      }
    }
    catch
    {
      // WMI not supported or failed
    }
    return null;
  }

  private void SetWmiBrightness(uint brightness)
  {
    try
    {
      using var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM WmiMonitorBrightnessMethods");
      foreach (ManagementObject queryObj in searcher.Get())
      {
        queryObj.InvokeMethod("WmiSetBrightness", new object[] { (uint)0, (byte)brightness });
        break;
      }
    }
    catch
    {
      // WMI not supported or failed
    }
  }

  private readonly Dictionary<IntPtr, uint> _pendingBrightness = new();
  private readonly Dictionary<IntPtr, bool> _isUpdating = new();

  public void SetBrightness(IntPtr hMonitor, uint brightness)
  {
    lock (_pendingBrightness)
    {
      _pendingBrightness[hMonitor] = brightness;

      if (!_isUpdating.TryGetValue(hMonitor, out bool isRunning) || !isRunning)
      {
        _isUpdating[hMonitor] = true;
        Task.Run(() => ProcessBrightnessQueue(hMonitor));
      }
    }
  }

  private void ProcessBrightnessQueue(IntPtr hMonitor)
  {
    while (true)
    {
      uint targetBrightness;
      lock (_pendingBrightness)
      {
        if (!_pendingBrightness.TryGetValue(hMonitor, out targetBrightness))
        {
          _isUpdating[hMonitor] = false;
          return;
        }
        _pendingBrightness.Remove(hMonitor);
      }

      uint count = 0;
      bool useWmiFallback = true;
      if (Win32.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref count) && count > 0)
      {
        var physicalMonitors = new Win32.PHYSICAL_MONITOR[count];
        if (Win32.GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors))
        {
          bool anySetSuccess = false;
          foreach (var pm in physicalMonitors)
          {
            if (Win32.SetMonitorBrightness(pm.hPhysicalMonitor, targetBrightness))
            {
              anySetSuccess = true;
            }
          }
          Win32.DestroyPhysicalMonitors(count, physicalMonitors);

          if (anySetSuccess)
          {
            useWmiFallback = false;
          }
        }
      }

      if (useWmiFallback)
      {
        SetWmiBrightness(targetBrightness);
      }
    }
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
