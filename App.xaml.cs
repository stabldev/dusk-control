using Microsoft.UI.Xaml;

namespace DuskControl;

public partial class App : Application
{
  private Window? _window;

  public App()
  {
    InitializeComponent();
    this.UnhandledException += (s, e) => {
        System.IO.File.WriteAllText("crash.txt", e.Exception.ToString() + "\nMessage: " + e.Message);
    };
  }

  protected override void OnLaunched(LaunchActivatedEventArgs args)
  {
    _window = new MainWindow();
    _window.Activate();
  }
}
