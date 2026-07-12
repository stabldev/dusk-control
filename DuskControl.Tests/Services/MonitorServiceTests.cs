using DuskControl.Services;
using Xunit;

namespace DuskControl.Tests.Services;

/// <summary>
/// These tests exercise <see cref="MonitorService.GetAvailableMonitors"/> against the real
/// Win32 display APIs (EnumDisplayMonitors / GetMonitorInfo). They are hardware/OS dependent:
/// on a machine or CI agent with no attached display, the returned list may be empty, in which
/// case the per-monitor assertions are skipped rather than failed, since there is nothing to
/// validate. Where at least one display is detected, the tests validate the DeviceId mapping
/// introduced in this PR (DeviceId = mi.DeviceName) alongside the pre-existing Name/IsPrimary
/// population that lives in the same method.
/// </summary>
public class MonitorServiceTests
{
  [Fact]
  public void GetAvailableMonitors_ReturnsNonNullList()
  {
    var monitors = MonitorService.GetAvailableMonitors();

    Assert.NotNull(monitors);
  }

  [Fact]
  public void GetAvailableMonitors_EachMonitor_HasNonEmptyDeviceId()
  {
    var monitors = MonitorService.GetAvailableMonitors();
    if (monitors.Count == 0) return; // No displays detected in this environment.

    Assert.All(monitors, m => Assert.False(string.IsNullOrEmpty(m.DeviceId)));
  }

  [Fact]
  public void GetAvailableMonitors_DeviceIds_FollowWin32DeviceNamingConvention()
  {
    var monitors = MonitorService.GetAvailableMonitors();
    if (monitors.Count == 0) return; // No displays detected in this environment.

    // Win32 MONITORINFOEX.szDevice values look like "\\.\DISPLAY1".
    Assert.All(monitors, m => Assert.StartsWith(@"\\.\DISPLAY", m.DeviceId));
  }

  [Fact]
  public void GetAvailableMonitors_DeviceIds_AreUniquePerMonitor()
  {
    var monitors = MonitorService.GetAvailableMonitors();
    if (monitors.Count == 0) return; // No displays detected in this environment.

    var deviceIds = monitors.Select(m => m.DeviceId).ToList();

    Assert.Equal(deviceIds.Count, deviceIds.Distinct().Count());
  }

  [Fact]
  public void GetAvailableMonitors_HasAtMostOnePrimaryMonitor()
  {
    var monitors = MonitorService.GetAvailableMonitors();
    if (monitors.Count == 0) return; // No displays detected in this environment.

    Assert.True(monitors.Count(m => m.IsPrimary) <= 1);
  }

  [Fact]
  public void GetAvailableMonitors_EachMonitor_HasValidHMonitorHandle()
  {
    var monitors = MonitorService.GetAvailableMonitors();
    if (monitors.Count == 0) return; // No displays detected in this environment.

    Assert.All(monitors, m => Assert.NotEqual(IntPtr.Zero, m.HMonitor));
  }

  [Fact]
  public void GetAvailableMonitors_EachMonitor_HasNonEmptyName()
  {
    var monitors = MonitorService.GetAvailableMonitors();
    if (monitors.Count == 0) return; // No displays detected in this environment.

    Assert.All(monitors, m => Assert.False(string.IsNullOrWhiteSpace(m.Name)));
  }

  [Fact]
  public void GetAvailableMonitors_EachMonitor_HasNullDisplayArea()
  {
    // DisplayArea is intentionally left null in GetAvailableMonitors to avoid a
    // WinRT crash (see inline comment in MonitorService). Regression guard for that.
    var monitors = MonitorService.GetAvailableMonitors();
    if (monitors.Count == 0) return; // No displays detected in this environment.

    Assert.All(monitors, m => Assert.Null(m.DisplayArea));
  }

  [Fact]
  public void GetAvailableMonitors_CalledTwice_ReturnsConsistentDeviceIds()
  {
    // DeviceId is what MainPage.RefreshMonitors relies on to preserve the user's
    // selection across refreshes, so it must be stable across repeated calls when
    // the physical monitor configuration hasn't changed.
    var first = MonitorService.GetAvailableMonitors();
    var second = MonitorService.GetAvailableMonitors();
    if (first.Count == 0 || second.Count == 0) return; // No displays detected.

    var firstIds = first.Select(m => m.DeviceId).OrderBy(id => id).ToList();
    var secondIds = second.Select(m => m.DeviceId).OrderBy(id => id).ToList();

    Assert.Equal(firstIds, secondIds);
  }
}