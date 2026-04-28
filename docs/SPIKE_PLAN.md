# WPF Shell Spike Plan

## Purpose

Validate that WPF is a practical MVP foundation before implementing real Google Calendar integration. The spike proves the risky Windows desktop behaviors first.

## Current status

Implementation status: **v0.2 UI update ready for Windows build/visual validation**.

This repo now includes a first-pass WPF shell with mock data only:

- Borderless, resizable WPF window using `WindowChrome`.
- Custom title/drag region with minimize and close controls.
- Acrylic/minimal dark glass visual direction.
- 6x7 dense month calendar mock grid populated through MVVM, with weekday column headers only.
- Previous/next month navigation that regenerates mock events for the visible range.
- Right-side selected-day agenda with an explicit collapsible/foldable panel.
- Separate Settings window documenting future auth/layer ownership; settings are not embedded in the right agenda panel.
- Always-on-top toggle bound to `Topmost` and persisted with window placement.
- JSON-backed local settings store for window position/size/topmost state.
- Mock calendar service behind `ICalendarService`; no Google SDK or OAuth coupling.

## Required validation host

Run this spike on Windows with the .NET 8 SDK. The current execution host is macOS with only .NET SDK 7.0.203, so WPF `net8.0-windows` build/run and visual validation cannot be completed here.

## Spike checklist

### 1. Borderless resizable window

- [x] Use `WindowStyle=None` and WPF `WindowChrome` resize border support.
- [x] Provide custom title, minimize, and close affordances.
- [ ] Confirm on Windows that the window can be moved by dragging the custom title region.
- [ ] Confirm on Windows that the resize border works on all edges and corners.

### 2. Always-on-top toggle

- [x] Expose an in-app toggle bound to the WPF `Topmost` property.
- [x] Persist the selected state for the next launch.
- [ ] Confirm on Windows that the window stays above normal apps when enabled.
- [ ] Confirm on Windows that disabling the toggle returns to normal window behavior.

### 3. Position and size persistence

- [x] Save normal/restored window bounds on close.
- [x] Restore placement on next launch.
- [x] Guard against obviously off-screen persisted bounds with virtual-screen checks.
- [ ] Confirm on Windows primary monitor.
- [ ] Confirm multi-monitor behavior and document any edge cases.

### 4. DPI scaling

- [ ] Test at 100%, 125%, and 150% scaling.
- [ ] Confirm text remains readable and not clipped.
- [ ] Confirm calendar cards, event chips, and right panel spacing scale acceptably.
- [ ] Capture screenshots for later visual comparison.

### 5. Mock calendar shell

- [x] Render dense month grid with top-left date numbers and mock events.
- [x] Render weekday names only as top column headers.
- [x] Render selected-day details in a collapsible agenda panel.
- [x] Open a separate Settings window for Google account and calendar layer placeholders.
- [x] Render mock calendar layers in Settings.
- [x] Keep Google auth/layer selection as Settings-owned placeholder only.
- [x] Avoid real credentials, OAuth flow, Google SDK types, or Google Cloud config.

## Windows validation commands

From the repository root on Windows:

```powershell
.\scripts\windows-validate.ps1
```

Or run manually:

```powershell
dotnet --info
dotnet restore .\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj
dotnet build .\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj -c Debug --no-restore
dotnet run --project .\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj
```

## Exit criteria

The WPF stack remains the MVP default if:

- The shell builds and runs reliably on Windows with .NET 8.
- Borderless resize and drag behavior feel acceptable.
- Always-on-top is reliable enough for MVP.
- Placement persistence works on the primary monitor.
- The acrylic/minimal productivity direction is visually plausible.

Consider WinUI 3 or Tauri only if this spike produces concrete evidence that WPF cannot meet the MVP shell requirements or development speed is unacceptable.
