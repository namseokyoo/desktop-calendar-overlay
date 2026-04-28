# WPF Shell Spike Plan

## Purpose

Validate that WPF is a practical MVP foundation before implementing real Google Calendar integration. The spike proves the risky Windows desktop behaviors first.

## Required validation host

Run this spike on Windows with the .NET 8 SDK. The current bootstrap host is macOS with .NET 7, so WPF build and visual validation are intentionally not run here.

## Spike checklist

### 1. Borderless resizable window

- Use `WindowStyle=None` and WPF `WindowChrome` resize border support.
- Confirm the window can be moved by dragging the custom title region.
- Confirm the resize border works on all edges and corners.
- Confirm the UI still has obvious close/minimize affordances.

### 2. Always-on-top toggle

- Expose an in-app toggle bound to the WPF `Topmost` property.
- Confirm the window stays above normal apps when enabled.
- Confirm disabling the toggle returns to normal window behavior.
- Persist the selected state for the next launch.

### 3. Position and size persistence

- Save normal window bounds on close.
- Restore placement on next launch.
- Avoid restoring invalid/off-screen bounds in the final implementation; for the spike, document any observed multi-monitor issues.

### 4. DPI scaling

- Test at 100%, 125%, and 150% scaling.
- Confirm text remains readable and not clipped.
- Confirm calendar cards, event chips, and right panel spacing scale acceptably.
- Capture screenshots for later visual comparison.

## Exit criteria

The WPF stack remains the MVP default if:

- The shell builds and runs reliably on Windows with .NET 8.
- Borderless resize and drag behavior feel acceptable.
- Always-on-top is reliable enough for MVP.
- Placement persistence works on the primary monitor.
- The acrylic/minimal productivity direction is visually plausible.

Consider WinUI 3 or Tauri only if this spike produces concrete evidence that WPF cannot meet the MVP shell requirements or development speed is unacceptable.
