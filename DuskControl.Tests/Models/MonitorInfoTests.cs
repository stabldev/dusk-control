using DuskControl.Models;
using Xunit;

namespace DuskControl.Tests.Models;

public class MonitorInfoTests
{
  [Fact]
  public void DefaultConstructor_InitializesExpectedDefaults()
  {
    var info = new MonitorInfo();

    Assert.Equal(string.Empty, info.Name);
    Assert.Equal(string.Empty, info.DeviceId);
    Assert.False(info.IsPrimary);
    Assert.Null(info.DisplayArea);
    Assert.Equal(IntPtr.Zero, info.HMonitor);
  }

  [Fact]
  public void DeviceId_CanBeSetAndRetrieved()
  {
    var info = new MonitorInfo { DeviceId = @"\\.\DISPLAY1" };

    Assert.Equal(@"\\.\DISPLAY1", info.DeviceId);
  }

  [Theory]
  [InlineData(@"\\.\DISPLAY1")]
  [InlineData(@"\\.\DISPLAY2")]
  [InlineData("")]
  public void DeviceId_AcceptsVariousValues(string deviceId)
  {
    var info = new MonitorInfo { DeviceId = deviceId };

    Assert.Equal(deviceId, info.DeviceId);
  }

  [Fact]
  public void DeviceId_IsIndependentOfName()
  {
    // DeviceId (Win32 device name, e.g. "\\.\DISPLAY1") and Name (friendly display
    // name, e.g. "Display 1 (Dell U2415)") are distinct properties populated from
    // different sources in MonitorService.GetAvailableMonitors().
    var info = new MonitorInfo
    {
      Name = "Display 1 (Dell U2415)",
      DeviceId = @"\\.\DISPLAY1"
    };

    Assert.Equal("Display 1 (Dell U2415)", info.Name);
    Assert.Equal(@"\\.\DISPLAY1", info.DeviceId);
    Assert.NotEqual(info.Name, info.DeviceId);
  }

  [Fact]
  public void ToString_ReturnsName()
  {
    var info = new MonitorInfo { Name = "Display 1 (Dell U2415)", DeviceId = @"\\.\DISPLAY1" };

    Assert.Equal("Display 1 (Dell U2415)", info.ToString());
  }

  [Fact]
  public void ToString_WhenNameEmpty_ReturnsEmptyString()
  {
    var info = new MonitorInfo();

    Assert.Equal(string.Empty, info.ToString());
  }

  [Fact]
  public void IsPrimary_DefaultsToFalse_AndCanBeSetTrue()
  {
    var info = new MonitorInfo();
    Assert.False(info.IsPrimary);

    info.IsPrimary = true;

    Assert.True(info.IsPrimary);
  }

  [Fact]
  public void HMonitor_CanStoreNonZeroHandle()
  {
    var handle = new IntPtr(12345);
    var info = new MonitorInfo { HMonitor = handle };

    Assert.Equal(handle, info.HMonitor);
  }

  [Fact]
  public void TwoInstances_WithSameDeviceId_AreNotEqualByDefaultEquality()
  {
    // MonitorInfo does not override Equals/GetHashCode, so equality remains
    // reference-based even when the stable DeviceId matches. Code that needs to
    // match monitors across refreshes (see MainPage.RefreshMonitors) must compare
    // DeviceId explicitly rather than relying on object equality.
    var first = new MonitorInfo { DeviceId = @"\\.\DISPLAY1" };
    var second = new MonitorInfo { DeviceId = @"\\.\DISPLAY1" };

    Assert.NotEqual(first, second);
    Assert.Equal(first.DeviceId, second.DeviceId);
  }
}