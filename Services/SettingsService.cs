using Microsoft.Win32;

namespace DuskControl.Services;

public class SettingsService
{
  private const string RegistryKeyPath = @"Software\DuskControl";

  public static bool StartWithWindows
  {
    get
    {
      try
      {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
        // Default to true if the value is missing
        var value = key?.GetValue("StartWithWindows") ?? 1;
        return Convert.ToInt32(value) != 0;
      }
      catch (Exception)
      {
        return true;
      }
    }
    set
    {
      try
      {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
        key?.SetValue("StartWithWindows", value ? 1 : 0, RegistryValueKind.DWord);
      }
      catch (Exception)
      {
        // Log/telemetry as appropriate
      }
    }
  }
}
