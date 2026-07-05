using System.Runtime.InteropServices;
using WmiLight;
using Windows.Devices.Display;
using DuskControl.Models;
using DuskControl.Helpers;

namespace DuskControl.Services;

public class MonitorService
{
  public static List<MonitorInfo> GetAvailableMonitors()
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

  public static uint? GetBrightness(IntPtr hMonitor)
  {
    uint count = 0;
    if (Win32.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref count) && count > 0)
    {
      var physicalMonitors = new Win32.PHYSICAL_MONITOR[count];
      if (Win32.GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors))
      {
        bool success = Win32.GetMonitorBrightness(physicalMonitors[0].hPhysicalMonitor, out _, out uint currentBrightness, out _);
        Win32.DestroyPhysicalMonitors(count, physicalMonitors);
        if (success)
          return currentBrightness;
      }
    }
    return GetWmiBrightness();
  }

  private static uint? GetWmiBrightness()
  {
    try
    {
      using WmiConnection con = new(@"root\wmi");
      foreach (var instance in con.CreateQuery("SELECT * FROM WmiMonitorBrightness"))
      {
        return Convert.ToUInt32(instance["CurrentBrightness"]);
      }
    }
    catch
    {
      // WMI not supported or failed
    }
    return null;
  }

  private static void SetWmiBrightness(uint brightness)
  {
    try
    {
      using WmiConnection con = new(@"root\wmi");
      foreach (var instance in con.CreateQuery("SELECT * FROM WmiMonitorBrightnessMethods"))
      {
        var method = instance.GetMethod("WmiSetBrightness");
        var methodParameters = method.CreateInParameters();

        // WmiLight often requires uint32 WMI properties to be passed as int due to COM variant mapping
        methodParameters.SetPropertyValue("Timeout", 0);
        methodParameters.SetPropertyValue("Brightness", (byte)brightness);

        instance.ExecuteMethod(method, methodParameters, out var _);
        break;
      }
    }
    catch
    {
      // WMI not supported or failed
    }
  }

  private readonly Dictionary<IntPtr, uint> _pendingBrightness = [];
  private readonly Dictionary<IntPtr, bool> _isUpdating = [];

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

  public static uint? GetContrast(IntPtr hMonitor)
  {
    uint count = 0;
    if (Win32.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref count) && count > 0)
    {
      var physicalMonitors = new Win32.PHYSICAL_MONITOR[count];
      if (Win32.GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors))
      {
        bool success = Win32.GetMonitorContrast(physicalMonitors[0].hPhysicalMonitor, out _, out uint currentContrast, out _);
        Win32.DestroyPhysicalMonitors(count, physicalMonitors);
        if (success)
          return currentContrast;
      }
    }
    return null;
  }

  private readonly Dictionary<IntPtr, uint> _pendingContrast = [];
  private readonly Dictionary<IntPtr, bool> _isUpdatingContrast = [];

  public void SetContrast(IntPtr hMonitor, uint contrast)
  {
    lock (_pendingContrast)
    {
      _pendingContrast[hMonitor] = contrast;

      if (!_isUpdatingContrast.TryGetValue(hMonitor, out bool isRunning) || !isRunning)
      {
        _isUpdatingContrast[hMonitor] = true;
        Task.Run(() => ProcessContrastQueue(hMonitor));
      }
    }
  }

  private void ProcessContrastQueue(IntPtr hMonitor)
  {
    while (true)
    {
      uint targetContrast;
      lock (_pendingContrast)
      {
        if (!_pendingContrast.TryGetValue(hMonitor, out targetContrast))
        {
          _isUpdatingContrast[hMonitor] = false;
          return;
        }
        _pendingContrast.Remove(hMonitor);
      }

      uint count = 0;
      if (Win32.GetNumberOfPhysicalMonitorsFromHMONITOR(hMonitor, ref count) && count > 0)
      {
        var physicalMonitors = new Win32.PHYSICAL_MONITOR[count];
        if (Win32.GetPhysicalMonitorsFromHMONITOR(hMonitor, count, physicalMonitors))
        {
          foreach (var pm in physicalMonitors)
          {
            Win32.SetMonitorContrast(pm.hPhysicalMonitor, targetContrast);
          }
          Win32.DestroyPhysicalMonitors(count, physicalMonitors);
        }
      }
    }
  }

  private static string GetMonitorName(IntPtr hMonitor)
  {
    var mi = new Win32.MONITORINFOEX();
    mi.cbSize = Marshal.SizeOf(mi);

    if (!Win32.GetMonitorInfo(hMonitor, ref mi))
      return "Unknown Monitor";

    var dd = new Win32.DISPLAY_DEVICE();
    dd.cb = Marshal.SizeOf(dd);

    // Pass 1 (EDD_GET_DEVICE_INTERFACE_NAME) to get the device interface path in DeviceID
    if (!Win32.EnumDisplayDevices(mi.DeviceName, 0, ref dd, 1))
      return mi.DeviceName;

    if (!string.IsNullOrEmpty(dd.DeviceIDStr))
    {
      try
      {
        var task = DisplayMonitor.FromInterfaceIdAsync(dd.DeviceIDStr).AsTask();
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

    if (!string.IsNullOrEmpty(dd.DeviceStringStr))
      return dd.DeviceStringStr;

    return mi.DeviceName;
  }
}
