# Architecture

## Goal

The MVP uses a small .NET 8 WPF + MVVM architecture that keeps Windows shell behavior, calendar data, settings, tray/startup behavior, and Google integration behind interfaces. The first milestone is a WPF shell spike, not a complete Google Calendar app.

## Layers

```text
Views (WPF XAML)
  MainWindow.xaml
      |
ViewModels
  MainViewModel
      |
Services behind interfaces
  ICalendarService
  ISettingsStore
  IWindowPlacementService
      |
Models
  CalendarLayer
  CalendarEvent
  WindowPlacementState
```

## View layer

- `App.xaml` defines app-level resources and merges theme resources.
- `MainWindow.xaml` owns WPF shell composition: borderless chrome, acrylic-like translucent styling, resize behavior, and high-level controls.
- Code-behind should stay limited to WPF-specific window lifecycle and shell integration that is awkward to express in pure XAML.

## ViewModel layer

- `MainViewModel` exposes UI state: current month label, selected date, calendar layers, events, selected date/detail state, Google connection status, persisted display settings, and startup setting.
- It uses `INotifyPropertyChanged` directly to avoid introducing dependencies during the spike.
- Later commands can be added for date navigation, settings, and event creation.

## Service interfaces

- `ICalendarService` abstracts calendar reads and narrow user-initiated event create/update/delete writes.
- `ISettingsStore` abstracts local app settings persistence.
- `IWindowPlacementService` abstracts saving/restoring WPF window placement.

The Google Calendar implementation stays behind `ICalendarService`/`IGoogleCalendarIntegration`. The WPF views and view models should not directly depend on Google SDK types.

## Current implementations

- `MockCalendarService` returns deterministic in-memory layers and events for design-time/spike validation.
- `JsonSettingsStore` persists lightweight local settings under `%LOCALAPPDATA%\DesktopCalendarOverlay`.
- `WindowPlacementService` persists normal window bounds and topmost state through `ISettingsStore`.

## MVP boundaries

Included:

- Google Calendar read support.
- User-initiated single-event create/update/delete.
- Settings-owned authentication and calendar layer selection.

Excluded until post-MVP:

- Repeat event authoring.
- Attendee invitation workflows.
- Outlook/Apple/CalDAV integrations.
- WorkerW/desktop-icon-behind embedding as a required behavior.
