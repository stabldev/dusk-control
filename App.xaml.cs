using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;

namespace DuskControl;

public partial class App : Application
{
  private Window? _window;

  public App()
  {
    InitializeComponent();

    // UI thread exceptions
    this.UnhandledException += (s, e) =>
    {
      System.IO.File.WriteAllText("crash.txt", $"UI UnhandledException: {e.Exception}\nMessage: {e.Message}");
      e.Handled = true; // Attempt to prevent crash if possible
    };

    // Background thread exceptions
    AppDomain.CurrentDomain.UnhandledException += (s, e) =>
    {
      System.IO.File.WriteAllText("crash.txt", $"AppDomain UnhandledException: {e.ExceptionObject}");
    };

    TaskScheduler.UnobservedTaskException += (s, e) =>
    {
      System.IO.File.WriteAllText("crash.txt", $"UnobservedTaskException: {e.Exception}");
    };
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    _window = new MainWindow();
    _window.Activate();
  }
}
