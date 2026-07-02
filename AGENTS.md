# AGENTS.md

## Project

**Dusk Control** is a lightweight native Windows brightness utility built with **C#**, **.NET**, and **WinUI 3**.

It combines two approaches to display brightness:

- **Hardware brightness (DDC/CI)** for values above `0`.
- **Software dimming (transparent overlay)** for values below `0`.

The application should feel like a native Windows 11 utility with minimal resource usage and fast startup.

---

# Goals

- Native Windows experience
- Low memory usage
- Fast startup
- Clean, maintainable architecture
- No unnecessary abstractions
- Simple code over clever code

---

# Tech Stack

- C#
- .NET
- WinUI 3
- Windows App SDK

Use built-in .NET libraries whenever possible.

Avoid unnecessary third-party dependencies.

---

# Coding Guidelines

- Prefer readable code over compact code.
- Keep methods small and focused.
- Use meaningful names.
- Avoid deeply nested code.
- Use async/await where appropriate.
- Handle exceptions gracefully.
- Do not prematurely optimize.

---

# Project Structure

```
Views/
Controls/
Models/
Services/
Helpers/
Assets/

App.xaml
```

Responsibilities:

- **Views** → UI
- **Controls** → Reusable UI components
- **Models** → Data models
- **Services** → Business logic and Windows APIs
- **Helpers** → Utility classes

---

# Service Responsibilities

MonitorService

- Enumerate monitors
- Read brightness
- Set DDC/CI brightness

OverlayService

- Create transparent overlay windows
- Update opacity
- Handle multi-monitor overlays

TrayService

- System tray icon
- Context menu
- Window visibility

SettingsService

- Persist application settings

HotkeyService

- Register global hotkeys
- Handle shortcuts

StartupService

- Register/unregister app startup

---

# UI Guidelines

The UI should follow Windows 11 Fluent Design.

Preferred characteristics:

- Clean spacing
- Rounded corners
- Simple animations
- Native controls
- Light and Dark mode support

Avoid custom-drawn controls unless necessary.

---

# Brightness Logic

Brightness range:

```
-100 ... 0 ... +100
```

Rules:

- Value > 0 → Use DDC/CI brightness.
- Value < 0 → Use overlay dimming.
- Value == 0 → Disable overlay and restore normal hardware brightness.

Users should interact with a single slider. The implementation details should remain invisible.

---

# AI Guidelines

When modifying code:

- Preserve the existing architecture.
- Reuse existing services before creating new ones.
- Avoid duplicate logic.
- Do not introduce new frameworks.
- Do not rewrite working code unnecessarily.
- Prefer extending existing components over replacing them.

---

# Performance

This application runs continuously in the background.

Prioritize:

- Low RAM usage
- Low CPU usage
- Minimal allocations
- Fast startup

Avoid polling when event-driven APIs are available.

---

# Dependencies

Before adding a NuGet package:

1. Check whether .NET or WinUI already provides the functionality.
2. Prefer native Windows APIs.
3. Keep dependencies to a minimum.

---

# Build Philosophy

Dusk Control should feel like a small Windows utility rather than a large application.

Every feature should be:

- Lightweight
- Native
- Maintainable
- Easy to understand

When in doubt, choose the simpler implementation.
