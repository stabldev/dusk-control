using System;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using dusk.Models;
using dusk.Services;

namespace dusk;

public sealed partial class MainPage : Page
{
  private readonly MonitorService _monitorService;
  private bool _isUpdatingUI = false;

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
      _isUpdatingUI = true;
      var brightness = _monitorService.GetBrightness(selectedMonitor.HMonitor);
      if (brightness.HasValue && BrightnessSlider != null)
      {
        BrightnessSlider.Value = brightness.Value;
      }

      var contrast = _monitorService.GetContrast(selectedMonitor.HMonitor);
      if (contrast.HasValue && ContrastSlider != null)
      {
        ContrastSlider.Value = contrast.Value;
      }

      _isUpdatingUI = false;
    }
  }

  private void BrightnessSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
  {
    if (_isUpdatingUI) return;

    if (MonitorComboBox.SelectedItem is MonitorInfo selectedMonitor)
    {
      uint hwBrightness = (uint)Math.Max(0, e.NewValue);
      _monitorService.SetBrightness(selectedMonitor.HMonitor, hwBrightness);
    }
  }

  private void ContrastSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
  {
    if (_isUpdatingUI) return;

    if (MonitorComboBox.SelectedItem is MonitorInfo selectedMonitor)
    {
      _monitorService.SetContrast(selectedMonitor.HMonitor, (uint)e.NewValue);
    }
  }

  private void ResetLevelsButton_Click(object sender, RoutedEventArgs e)
  {
    if (BrightnessSlider != null) BrightnessSlider.Value = 0;
    if (ContrastSlider != null) ContrastSlider.Value = 50;
  }
}
