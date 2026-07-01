using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using dusk.Models;
using dusk.Services;

namespace dusk;

public sealed partial class MainPage : Page
{
  private readonly MonitorService _monitorService;

  public MainPage()
  {
    InitializeComponent();
    _monitorService = new MonitorService();

    var monitors = _monitorService.GetAvailableMonitors();
    MonitorComboBox.ItemsSource = monitors;

    var primaryMonitor = monitors?.FirstOrDefault(m => m.IsPrimary);
    if (primaryMonitor != null)
    {
      MonitorComboBox.SelectedItem = primaryMonitor;
    }
  }

  private void MonitorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (MonitorComboBox.SelectedItem is MonitorInfo selectedMonitor)
    {
      // Prepare for brightness adjustment logic here
    }
  }

  private void ResetLevelsButton_Click(object sender, RoutedEventArgs e)
  {
    if (BrightnessSlider != null) BrightnessSlider.Value = 0;
    if (ContrastSlider != null) ContrastSlider.Value = 50;
  }
}
