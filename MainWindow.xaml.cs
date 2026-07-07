using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using DuskControl.Services;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace DuskControl;

/// <summary>
/// The application window. This hosts a Frame that displays pages. Add your
/// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
/// can use Page features such as navigation events and the Loaded lifecycle.
/// </summary>
public sealed partial class MainWindow : Window
{
  private readonly TrayService? _trayService;
  private bool _isExiting = false;

  public MainWindow()
  {
    InitializeComponent();

    ExtendsContentIntoTitleBar = true;
    SetTitleBar(AppTitleBar);

    AppWindow.SetIcon("Assets/AppIcon.ico");

    IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
    uint dpi = Helpers.Win32.GetDpiForWindow(hwnd);
    if (dpi == 0) dpi = 96;
    double scaleFactor = dpi / 96.0;

    int width = (int)(350 * scaleFactor);
    int height = (int)(320 * scaleFactor);

    AppWindow.Resize(new Windows.Graphics.SizeInt32(width, height));

    if (AppWindow.Presenter is OverlappedPresenter overlappedPresenter)
    {
      overlappedPresenter.IsResizable = false;
      overlappedPresenter.IsMaximizable = false;
      overlappedPresenter.IsMinimizable = false;
    }

    // Remove Minimize and Maximize buttons completely
    int style = Helpers.Win32.GetWindowLong(hwnd, Helpers.Win32.GWL_STYLE);
    style &= ~(int)Helpers.Win32.WS_MINIMIZEBOX;
    style &= ~(int)Helpers.Win32.WS_MAXIMIZEBOX;
    _ = Helpers.Win32.SetWindowLong(hwnd, Helpers.Win32.GWL_STYLE, style);

    // Position window at bottom right corner above the taskbar with some padding
    ResetPosition();

    // Navigate the root frame to the main page on startup.
    RootFrame.Navigate(typeof(MainPage));

    // Initialize Tray Icon and intercept close
    _trayService = new TrayService(this);
    Closed += MainWindow_Closed;
  }

  private void MainWindow_Closed(object sender, WindowEventArgs args)
  {
    if (!_isExiting)
    {
      args.Handled = true;
      AppWindow.Hide();
    }
    else
    {
      _trayService?.Dispose();
    }
  }

  public void ResetPosition()
  {
    var displayArea = DisplayArea.Primary;
    if (displayArea != null)
    {
      var workArea = displayArea.WorkArea;
      IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
      uint dpi = Helpers.Win32.GetDpiForWindow(hwnd);
      if (dpi == 0) dpi = 96;
      double scaleFactor = dpi / 96.0;

      int padding = (int)(6 * scaleFactor);
      int x = workArea.X + workArea.Width - AppWindow.Size.Width - padding;
      int y = workArea.Y + workArea.Height - AppWindow.Size.Height - padding;
      AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
    }
  }

  public void ExitApplication()
  {
    _isExiting = true;
    Close();
  }
}
