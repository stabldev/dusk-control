using Microsoft.Win32;

namespace DuskControl.Services;

public class SettingsService
{
  private const string RegistryKeyPath = @"Software\DuskControl";

  public static bool StartWithWindows
  {
    get
    {
      using var key = Registry.CurrentUser.OpenSubKey(RegistryKeyPath);
      // Default to true if the value is missing
      return (int)(key?.GetValue("StartWithWindows") ?? 1) != 0;
    }
    set
    {
      using var key = Registry.CurrentUser.CreateSubKey(RegistryKeyPath);
      key?.SetValue("StartWithWindows", value ? 1 : 0, RegistryValueKind.DWord);
    }
  }
}
