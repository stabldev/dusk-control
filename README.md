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

- Single unified slider ranging from −100 to +100
- Controls external monitor brightness natively via DDC/CI
- Sub-zero software dimming using a transparent overlay
- Per-monitor contrast adjustment
- Multi-monitor support with quick switching
- Runs quietly in the system tray
- Optional launch at Windows startup
- Native Windows 11 Fluent Design with light & dark mode
- Compiled with Native AOT for fast startup and low memory usage

## Download

> 🏪 **Microsoft Store** — [Get Dusk Control](https://apps.microsoft.com/detail/9PC2NH8VN5NT)
>
> 📦 **Installer** (.exe) — [Latest Release](https://github.com/stabldev/dusk-control/releases/latest)
>
> 📁 **Portable** (.zip) — [Latest Release](https://github.com/stabldev/dusk-control/releases/latest)

## Requirements

- Windows 10 (1809) or later
- External monitors must have **DDC/CI enabled** in their OSD settings

## How It Works

Dusk Control combines two brightness approaches behind a single slider:

```
-100 ◄━━━━━━━━━━━━━━━━━ 0 ━━━━━━━━━━━━━━━━━► +100
Software dim            ↑     Hardware brightness
(overlay)             Normal             (DDC/CI)
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
