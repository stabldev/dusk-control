<p align="center">
  <img src="Assets/AppIcon.ico" alt="Dusk Control" width="80" />
</p>

<h1 align="center">Dusk Control</h1>

<p align="center">
A Windows system tray utility for advanced brightness control, <br/> featuring DDC/CI support and sub-zero software dimming.
</p>

<p align="center">
  <a href="https://github.com/stabldev/dusk-control/releases/latest"><img src="https://img.shields.io/github/v/release/stabldev/dusk-control?style=flat-square&color=blue" alt="Latest Release" /></a>
  <a href="https://github.com/stabldev/dusk-control/releases"><img src="https://img.shields.io/github/downloads/stabldev/dusk-control/total?style=flat-square&color=green" alt="Downloads" /></a>
  <a href="https://github.com/stabldev/dusk-control/actions/workflows/release.yml"><img src="https://img.shields.io/github/actions/workflow/status/stabldev/dusk-control/release.yml?style=flat-square&label=build" alt="Build" /></a>
  <img src="https://img.shields.io/badge/platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows" alt="Platform" />
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square&logo=dotnet" alt=".NET 10" />
</p>

<p align="center">
  <a href="https://apps.microsoft.com/detail/9PC2NH8VN5NT">
    <img src="https://get.microsoft.com/images/en-us%20dark.svg" alt="Get it from Microsoft Store" width="160" />
  </a>
</p>

## Features

- **Unified brightness slider** — single slider from −100 to +100
- **Hardware brightness (DDC/CI)** — controls external monitors natively
- **Software dimming** — transparent overlay for values below zero
- **Contrast control** — per-monitor contrast adjustment via DDC/CI
- **Multi-monitor support** — switch between connected displays
- **System tray** — runs quietly in the background
- **Launch at startup** — optional auto-start with Windows
- **Fluent Design** — native Windows 11 look with light & dark mode
- **Native AOT** — fast startup, low memory footprint

## Download

| Channel | Link |
|---------|------|
| **Microsoft Store** | [Get Dusk Control](https://apps.microsoft.com/detail/9PC2NH8VN5NT) |
| **Installer (.exe)** | [Latest Release](https://github.com/stabldev/dusk-control/releases/latest) |
| **Portable (.zip)** | [Latest Release](https://github.com/stabldev/dusk-control/releases/latest) |

## Requirements

- Windows 10 (1809) or later
- External monitors must have **DDC/CI enabled** in their OSD settings

## How It Works

Dusk Control combines two brightness approaches behind a single slider:

```
-100 ◄━━━━━━━━━━ 0 ━━━━━━━━━━► +100
   Software dim    ↑    Hardware brightness
  (overlay)     Normal       (DDC/CI)
```

- **Above 0** → Sets hardware brightness via DDC/CI
- **Below 0** → Dims the screen using a transparent overlay
- **At 0** → Normal brightness, no overlay

## Building from Source

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [Windows App SDK](https://learn.microsoft.com/windows/apps/windows-app-sdk/)
- Visual Studio 2022+ with the **Windows application development** workload (optional)

```powershell
# Clone
git clone https://github.com/stabldev/dusk-control.git
cd dusk-control
# Build
dotnet build
# Run
dotnet run
```

## License

This project is source-available. See the repository for details.
