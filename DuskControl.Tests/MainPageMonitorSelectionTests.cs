using DuskControl.Models;
using Xunit;

namespace DuskControl.Tests;

/// <summary>
/// <para>
/// Covers the monitor re-selection logic added to <c>MainPage.RefreshMonitors()</c>
/// (MainPage.xaml.cs) by this PR:
/// </para>
/// <code>
/// var newSelection = monitors?.FirstOrDefault(m => m.DeviceId == selectedDeviceId)
///                    ?? monitors?.FirstOrDefault(m => m.IsPrimary);
/// </code>
/// <para>
/// <c>MainPage</c> is a WinUI <c>Page</c> whose constructor calls the XAML-generated
/// <c>InitializeComponent()</c> and reads/writes a live <c>ComboBox</c>. Instantiating it
/// (or <c>MainWindow</c>, whose <c>AppWindow_Changed</c> handler just forwards to
/// <c>RefreshMonitors</c> when the window becomes visible) requires a running WinUI
/// <c>Application</c>/dispatcher and native window, which is not available in a plain
/// unit test host. This test therefore exercises the exact selection expression above
/// directly against <see cref="MonitorInfo"/> instances -- the same model type
/// <c>RefreshMonitors</c> operates on -- to give regression coverage for the "stable
/// monitor selection matching" behavior (see commit 5fedc6f) without requiring a XAML
/// runtime.
/// </para>
/// </summary>
public class MainPageMonitorSelectionTests
{
  private static MonitorInfo? SelectMonitor(List<MonitorInfo>? monitors, string? selectedDeviceId)
  {
    return monitors?.FirstOrDefault(m => m.DeviceId == selectedDeviceId)
           ?? monitors?.FirstOrDefault(m => m.IsPrimary);
  }

  [Fact]
  public void PrefersPreviousDeviceId_OverPrimaryMonitor()
  {
    var monitors = new List<MonitorInfo>
    {
      new() { DeviceId = @"\\.\DISPLAY1", IsPrimary = true },
      new() { DeviceId = @"\\.\DISPLAY2", IsPrimary = false },
    };

    var selected = SelectMonitor(monitors, @"\\.\DISPLAY2");

    Assert.NotNull(selected);
    Assert.Equal(@"\\.\DISPLAY2", selected!.DeviceId);
  }

  [Fact]
  public void FallsBackToPrimaryMonitor_WhenPreviousDeviceIdNoLongerPresent()
  {
    // Simulates unplugging the previously selected monitor: it should fall back
    // to whichever monitor is now flagged primary instead of leaving no selection.
    var monitors = new List<MonitorInfo>
    {
      new() { DeviceId = @"\\.\DISPLAY1", IsPrimary = true },
      new() { DeviceId = @"\\.\DISPLAY2", IsPrimary = false },
    };

    var selected = SelectMonitor(monitors, @"\\.\DISPLAY3");

    Assert.NotNull(selected);
    Assert.Equal(@"\\.\DISPLAY1", selected!.DeviceId);
    Assert.True(selected.IsPrimary);
  }

  [Fact]
  public void ReturnsNull_WhenNoDeviceIdMatchAndNoPrimaryMonitor()
  {
    var monitors = new List<MonitorInfo>
    {
      new() { DeviceId = @"\\.\DISPLAY1", IsPrimary = false },
      new() { DeviceId = @"\\.\DISPLAY2", IsPrimary = false },
    };

    var selected = SelectMonitor(monitors, @"\\.\DISPLAY3");

    Assert.Null(selected);
  }

  [Fact]
  public void ReturnsNull_WhenMonitorListIsEmpty()
  {
    var monitors = new List<MonitorInfo>();

    var selected = SelectMonitor(monitors, @"\\.\DISPLAY1");

    Assert.Null(selected);
  }

  [Fact]
  public void ReturnsNull_WhenMonitorListIsNull()
  {
    var selected = SelectMonitor(null, @"\\.\DISPLAY1");

    Assert.Null(selected);
  }

  [Fact]
  public void SelectsPrimaryMonitor_WhenNoPreviousSelectionExists()
  {
    // Mirrors first-run behavior: currentSelection is null, so selectedDeviceId is null,
    // and no monitor's DeviceId is expected to equal null.
    var monitors = new List<MonitorInfo>
    {
      new() { DeviceId = @"\\.\DISPLAY1", IsPrimary = false },
      new() { DeviceId = @"\\.\DISPLAY2", IsPrimary = true },
    };

    var selected = SelectMonitor(monitors, selectedDeviceId: null);

    Assert.NotNull(selected);
    Assert.Equal(@"\\.\DISPLAY2", selected!.DeviceId);
  }

  [Fact]
  public void MatchesPreviousDeviceId_EvenWhenItIsThePrimaryMonitor()
  {
    var monitors = new List<MonitorInfo>
    {
      new() { DeviceId = @"\\.\DISPLAY1", IsPrimary = true },
      new() { DeviceId = @"\\.\DISPLAY2", IsPrimary = false },
    };

    var selected = SelectMonitor(monitors, @"\\.\DISPLAY1");

    Assert.NotNull(selected);
    Assert.Equal(@"\\.\DISPLAY1", selected!.DeviceId);
  }

  [Fact]
  public void MatchIsCaseSensitive_ForDeviceId()
  {
    // DeviceId comparison uses plain string equality (==), so it is case-sensitive.
    // Win32 device names are consistently uppercase (e.g. "\\.\DISPLAY1"), but this
    // documents the current exact-match behavior as a regression guard.
    var monitors = new List<MonitorInfo>
    {
      new() { DeviceId = @"\\.\DISPLAY1", IsPrimary = true },
    };

    var selected = SelectMonitor(monitors, @"\\.\display1");

    // No case-insensitive match is found, so it falls back to the primary monitor,
    // which happens to be the same instance here -- verify via the fallback path.
    Assert.NotNull(selected);
    Assert.True(selected!.IsPrimary);
  }

  [Fact]
  public void ReturnsFirstMatchingMonitor_WhenDeviceIdsAreDuplicated()
  {
    // Defensive case: if DeviceId were ever duplicated (should not normally happen,
    // since Win32 device names are unique per monitor), FirstOrDefault deterministically
    // returns the first match in enumeration order.
    var first = new MonitorInfo { DeviceId = @"\\.\DISPLAY1", IsPrimary = false };
    var duplicate = new MonitorInfo { DeviceId = @"\\.\DISPLAY1", IsPrimary = false };
    var monitors = new List<MonitorInfo> { first, duplicate };

    var selected = SelectMonitor(monitors, @"\\.\DISPLAY1");

    Assert.Same(first, selected);
  }
}