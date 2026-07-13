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
  private List<MonitorInfo>? _cachedMonitors;
  private readonly Dictionary<IntPtr, (uint? Brightness, uint? Contrast)> _cachedHardwareValues = [];

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

    // 1. Show Stale Data Instantly
    if (_cachedMonitors != null && _cachedMonitors.Count > 0)
    {
      MonitorComboBox.ItemsSource = _cachedMonitors;
      MonitorComboBox.SelectedItem ??= _cachedMonitors.FirstOrDefault(m => m.IsPrimary);
    }

    // 2. Revalidate Async
    _ = RevalidateMonitorsAsync();
  }

  private async Task RevalidateMonitorsAsync()
  {
    var currentSelection = MonitorComboBox.SelectedItem as MonitorInfo;
    string? selectedDeviceId = currentSelection?.DeviceId;

    var monitors = await Task.Run(() => MonitorService.GetAvailableMonitors());

    DispatcherQueue.TryEnqueue(() =>
    {
      _cachedMonitors = monitors;
      MonitorComboBox.ItemsSource = monitors;

      var newSelection = monitors?.FirstOrDefault(m => m.DeviceId == selectedDeviceId)
                         ?? monitors?.FirstOrDefault(m => m.IsPrimary);

      if (newSelection != null)
      {
        MonitorComboBox.SelectedItem = newSelection;
      }
    });
  }

  private void MonitorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {
    if (MonitorComboBox.SelectedItem is MonitorInfo selectedMonitor)
    {
      var hMonitor = selectedMonitor.HMonitor;

      // 1. Show Stale Data Instantly
      if (_cachedHardwareValues.TryGetValue(hMonitor, out var cachedVals))
      {
        UpdateSliders(cachedVals.Brightness, cachedVals.Contrast);
      }

      // 2. Revalidate Async
      _ = RevalidateHardwareValuesAsync(hMonitor);
    }
  }

  private async Task RevalidateHardwareValuesAsync(IntPtr hMonitor)
  {
    var (brightness, contrast) = await Task.Run(() =>
    {
      return (MonitorService.GetBrightness(hMonitor), MonitorService.GetContrast(hMonitor));
    });

    DispatcherQueue.TryEnqueue(() =>
    {
      if (MonitorComboBox.SelectedItem is MonitorInfo currentMonitor && currentMonitor.HMonitor == hMonitor)
      {
        _cachedHardwareValues[hMonitor] = (brightness, contrast);
        UpdateSliders(brightness, contrast);
      }
    });
  }

  private void UpdateSliders(uint? brightness, uint? contrast)
  {
    _isUpdatingUI = true;

    if (brightness.HasValue && BrightnessSlider != null)
    {
      BrightnessSlider.Value = brightness.Value;
    }

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
