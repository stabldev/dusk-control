using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using DuskControl.Services;

namespace DuskControl;

public partial class App : Application
{
  private Window? _window;

  public App()
  {
    InitializeComponent();

    LoggerService.LogInfo("Application starting...");

    // UI thread exceptions
    this.UnhandledException += (s, e) =>
    {
      LoggerService.LogFatal($"UI UnhandledException: {e.Exception}\nMessage: {e.Message}");
      e.Handled = true; // Attempt to prevent crash if possible
    };

    // Background thread exceptions
    AppDomain.CurrentDomain.UnhandledException += (s, e) =>
    {
      LoggerService.LogFatal($"AppDomain UnhandledException: {e.ExceptionObject}");
    };

    TaskScheduler.UnobservedTaskException += (s, e) =>
    {
      LoggerService.LogFatal($"UnobservedTaskException: {e.Exception}");
    };
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    _window = new MainWindow();
    _window.Activate();
  }
}
