# Desktop Calendar Overlay

Desktop Calendar Overlay is a Windows desktop calendar MVP for keeping a large, low-distraction Google Calendar view visible while working. The MVP direction is a Windows 11-style **acrylic glass + minimal productivity** overlay: a borderless, resizable WPF shell with compact month navigation, a collapsible day-detail panel, and a separate settings window for Google account/layer controls.

This is the standalone implementation repository for the Windows Desktop Calendar Overlay app. Planning history and post-MVP ideas remain in the SidequestLab planning project and are intentionally not copied into this MVP implementation scope.

## MVP scope

Included in MVP planning:

- .NET 8 + WPF + MVVM desktop app.
- WPF shell spike before full feature implementation.
- Borderless/resizable overlay window with persisted position/size and a position-lock toggle.
- Position and size persistence.
- DPI scaling validation on Windows.
- Google Calendar read support behind service interfaces.
- Google Calendar create/update/delete path for user-initiated single-event CRUD.
- Separate Settings window for Google authentication, connect/disconnect, and calendar layer selection.
- v0.6 Google Calendar OAuth/read/create/update/delete path with mock fallback when no local client JSON/token is available.
- Today date-number badge, display-format setting, opacity slider, event-list text-size slider, layer color palette, and theme selector in Settings.
- No repeat/attendee workflows in MVP.

This build is `0.6.0-polish-release-ready`: a WPF shell plus Google Calendar read/create/update/delete integration, stable error reporting, persisted display/layer settings, tray controls, Windows startup option, and release-readiness docs. It does **not** contain Google OAuth credentials, Google Cloud configuration, tokens, or user calendar exports.

## Prerequisites

Build and visual validation must run on Windows:

- Windows 10/11, with Windows 11 preferred for acrylic/glass visual validation.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- Visual Studio 2022 with .NET desktop development workload, or the .NET 8 SDK CLI.

> Current host limitation: this repository was bootstrapped from macOS where only .NET 7 is installed. WPF targets `net8.0-windows` and cannot be built or visually validated on this host. Run the validation commands below on Windows with the .NET 8 SDK.

## Run, logs, and release package

Developer run on Windows:

```powershell
dotnet run --project .\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj
```

Diagnostics are written to:

```text
%LOCALAPPDATA%\DesktopCalendarOverlay\logs\startup.log
```

Release packaging currently uses ZIP artifacts, not an installer/MSIX. The Windows release workflow publishes:

- `desktop-calendar-overlay-win-x64.zip` — self-contained Windows x64 single-file publish output.
- `desktop-calendar-overlay-win-x64-portable.zip` — self-contained Windows x64 portable folder publish output.

Extract a ZIP on Windows and run `DesktopCalendarOverlay.exe`. OAuth credentials/tokens are not bundled; real Google Calendar sync still requires the local Desktop OAuth client JSON described below.

## Known v0.6.0 limitations

- WPF visual/runtime validation is Windows-only.
- Real Google sync requires a local Google OAuth Desktop app JSON and permitted test user/consent setup.
- Repeat events and attendee/invitation workflows are intentionally out of MVP scope.
- Packaging is ZIP-based for now; no installer, auto-update, or MSIX package is included.

## Windows validation

From a Windows terminal at the repository root:

```powershell
.\scripts\windows-validate.ps1
dotnet run --project .\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj
```

Manual release checks are tracked in [`docs/QA_CHECKLIST_v0.6.0.md`](docs/QA_CHECKLIST_v0.6.0.md). Summary checks:

1. Window opens without normal OS chrome and remains resizable.
2. Position lock prevents title-drag movement/resizing after placing the overlay.
3. Moving/resizing the window, closing it, and reopening it restores placement.
4. UI remains legible at 100%, 125%, and 150% Windows display scaling.
5. Previous/next month buttons update the displayed mock calendar month.
6. Weekday names appear only as top column headers; date numbers sit top-left in compact day cells.
7. Mock events render inside day cells as small time-ordered list items.
8. The right agenda panel collapses/expands while preserving selected-day state and giving the calendar more room.
9. Settings opens as a separate dialog containing Google account connect/disconnect controls and calendar layer toggles.
10. Without a local OAuth client JSON/token, mock calendar layers and events render safely.
11. With a valid local OAuth client JSON, Connect opens the Google OAuth browser flow and then loads real calendar layers/events.
12. Click `+ Add event` from the selected-day panel, create a single event, and verify it appears in Google Calendar after refresh.
13. Use `Edit` and `Delete` from the selected-day detail panel and verify the Google Calendar event updates/deletes after refresh.
14. Settings can switch event display between `time · event` and `event · time` while preserving time sorting.
15. Settings can adjust overlay opacity, event-list text size, calendar layer visibility/colors via the current-color circle/native palette, and switch between built-in themes; changes persist after restart.
16. Tray menu exposes Show/Hide, Settings, Refresh, and Exit; Windows startup can be enabled/disabled from Settings.
17. The app/window/taskbar icon uses the calendar `.ico` asset.

See [`scripts/windows-validate.ps1`](scripts/windows-validate.ps1) for a documented validation helper and [`docs/SPIKE_PLAN.md`](docs/SPIKE_PLAN.md) for the spike plan.

## Repository layout

```text
src/DesktopCalendarOverlay/   WPF app project and MVVM skeleton
docs/                         Architecture, spike, OAuth/security notes
scripts/                      Windows-side validation helper
```

## Security notes

Do not commit OAuth client secrets, refresh tokens, exported credential files, or Google Cloud project configuration. For local v0.5 testing, place the Desktop OAuth client JSON at `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json`; the app stores tokens under `%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store`. See [`docs/OAUTH_AND_SECURITY.md`](docs/OAUTH_AND_SECURITY.md).
