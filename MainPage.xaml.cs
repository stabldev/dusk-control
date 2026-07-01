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
  private readonly OverlayService _overlayService;
  private bool _isUpdatingUI = false;

  public MainPage()
  {
    InitializeComponent();
    _monitorService = new MonitorService();
    _overlayService = new OverlayService();

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
      if (e.NewValue >= 0)
      {
        _monitorService.SetBrightness(selectedMonitor.HMonitor, (uint)e.NewValue);
        _overlayService.UpdateOverlay(selectedMonitor.HMonitor, 0);
      }
      else
      {
        _monitorService.SetBrightness(selectedMonitor.HMonitor, 0);
        double opacity = Math.Abs(e.NewValue) / 100.0 * 0.85; // Max 85% opacity at -100
        _overlayService.UpdateOverlay(selectedMonitor.HMonitor, opacity);
      }
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

    if (MonitorComboBox.SelectedItem is MonitorInfo selectedMonitor)
    {
      _overlayService.UpdateOverlay(selectedMonitor.HMonitor, 0);
    }
  }
}
