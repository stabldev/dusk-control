using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace dusk;

/// <summary>
/// The application window. This hosts a Frame that displays pages. Add your
/// UI and logic to MainPage.xaml / MainPage.xaml.cs instead of here so you
/// can use Page features such as navigation events and the Loaded lifecycle.
/// </summary>
public sealed partial class MainWindow : Window
{
  public MainWindow()
  {
    InitializeComponent();

    ExtendsContentIntoTitleBar = true;
    SetTitleBar(AppTitleBar);

    AppWindow.SetIcon("Assets/AppIcon.ico");
    AppWindow.Resize(new Windows.Graphics.SizeInt32(400, 300));

    if (AppWindow.Presenter is OverlappedPresenter overlappedPresenter)
    {
      overlappedPresenter.IsResizable = false;
      overlappedPresenter.IsMaximizable = false;
      overlappedPresenter.IsMinimizable = false;
    }

    // Position window at bottom right corner above the taskbar with some padding
    var displayArea = DisplayArea.Primary;
    if (displayArea != null)
    {
      var workArea = displayArea.WorkArea;
      int padding = 6;
      int x = workArea.X + workArea.Width - AppWindow.Size.Width - padding;
      int y = workArea.Y + workArea.Height - AppWindow.Size.Height - padding;
      AppWindow.Move(new Windows.Graphics.PointInt32(x, y));
    }

    // Navigate the root frame to the main page on startup.
    RootFrame.Navigate(typeof(MainPage));
  }
}
