using Microsoft.UI.Windowing;

namespace dusk.Models;

public class MonitorInfo
{
  public string Name { get; set; } = string.Empty;
  public bool IsPrimary { get; set; }

  // WinUI's DisplayArea for reference
  public DisplayArea? DisplayArea { get; set; }

  public override string ToString() => Name;
}
