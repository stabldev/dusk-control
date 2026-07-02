using Microsoft.UI.Windowing;

namespace DuskControl.Models;

public class MonitorInfo
{
  public string Name { get; set; } = string.Empty;
  public bool IsPrimary { get; set; }

  // WinUI's DisplayArea for reference
  public DisplayArea? DisplayArea { get; set; }

  // The Win32 HMONITOR handle
  public IntPtr HMonitor { get; set; }

  public override string ToString() => Name;
}
