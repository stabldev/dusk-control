using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using DuskControl.Models;
using DuskControl.Services;

#pragma warning disable IDE0060 // Remove unused parameter

namespace DuskControl;

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

    RefreshMonitors();
  }

  public void RefreshMonitors()
  {
    SlideUpStoryboard.Begin();

    var currentSelection = MonitorComboBox.SelectedItem as MonitorInfo;
    string? selectedDeviceId = currentSelection?.DeviceId;

    var monitors = MonitorService.GetAvailableMonitors();
    MonitorComboBox.ItemsSource = monitors;

    var newSelection = monitors?.FirstOrDefault(m => m.DeviceId == selectedDeviceId)
                       ?? monitors?.FirstOrDefault(m => m.IsPrimary);

    if (newSelection != null)
    {
      MonitorComboBox.SelectedItem = newSelection;
    }
  }

  private void MonitorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (MonitorComboBox.SelectedItem is MonitorInfo selectedMonitor)
    {
      _isUpdatingUI = true;
      var brightness = MonitorService.GetBrightness(selectedMonitor.HMonitor);
      if (brightness.HasValue && BrightnessSlider != null)
      {
        BrightnessSlider.Value = brightness.Value;
      }

      var contrast = MonitorService.GetContrast(selectedMonitor.HMonitor);
      if (contrast.HasValue)
      {
        if (ContrastSlider != null) ContrastSlider.Value = contrast.Value;
        if (ContrastPanel != null) ContrastPanel.Visibility = Visibility.Visible;
      }
      else
      {
        if (ContrastPanel != null) ContrastPanel.Visibility = Visibility.Collapsed;
      }

      _isUpdatingUI = false;
    }
  }

  private void BrightnessSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
  {
    if (BrightnessText != null)
    {
      BrightnessText.Text = $"{e.NewValue}%";
    }

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

  private void DecreaseBrightnessButton_Click(object sender, RoutedEventArgs e)
  {
    if (BrightnessSlider != null)
    {
      BrightnessSlider.Value = Math.Max(BrightnessSlider.Minimum, BrightnessSlider.Value - 5);
    }
  }

  private void IncreaseBrightnessButton_Click(object sender, RoutedEventArgs e)
  {
    if (BrightnessSlider != null)
    {
      BrightnessSlider.Value = Math.Min(BrightnessSlider.Maximum, BrightnessSlider.Value + 5);
    }
  }

  private void ContrastSlider_ValueChanged(object sender, Microsoft.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
  {
    if (ContrastText != null)
    {
      ContrastText.Text = $"{e.NewValue}%";
    }

    if (_isUpdatingUI) return;

    if (MonitorComboBox.SelectedItem is MonitorInfo selectedMonitor)
    {
      _monitorService.SetContrast(selectedMonitor.HMonitor, (uint)e.NewValue);
    }
  }

  private void DecreaseContrastButton_Click(object sender, RoutedEventArgs e)
  {
    if (ContrastSlider != null)
    {
      ContrastSlider.Value = Math.Max(ContrastSlider.Minimum, ContrastSlider.Value - 5);
    }
  }

  private void IncreaseContrastButton_Click(object sender, RoutedEventArgs e)
  {
    if (ContrastSlider != null)
    {
      ContrastSlider.Value = Math.Min(ContrastSlider.Maximum, ContrastSlider.Value + 5);
    }
  }

  private void ResetLevelsButton_Click(object sender, RoutedEventArgs e)
  {
    if (BrightnessSlider != null) BrightnessSlider.Value = 0;
    if (ContrastSlider != null && ContrastPanel?.Visibility == Visibility.Visible) ContrastSlider.Value = 50;

    if (MonitorComboBox.SelectedItem is MonitorInfo selectedMonitor)
    {
      _overlayService.UpdateOverlay(selectedMonitor.HMonitor, 0);
    }
  }
}
