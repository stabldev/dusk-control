using Microsoft.Win32;
using System.Diagnostics;

namespace DuskControl.Services;

public class StartupService
{
  private const string RegistryKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
  private const string AppName = "DuskControl";

  public static void SetStartupEnabled(bool enable)
  {
    try
    {
      using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
      if (key == null) return;

      if (enable)
      {
        var exePath = Process.GetCurrentProcess().MainModule?.FileName;
        if (!string.IsNullOrEmpty(exePath))
        {
          key.SetValue(AppName, $"\"{exePath}\" --hidden");
        }
      }
      else
      {
        key.DeleteValue(AppName, false);
      }
    }
    catch (Exception ex)
    {
      Debug.WriteLine($"Failed to set startup enabled: {ex.Message}");
    }
  }
}
