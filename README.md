# Desktop Calendar Overlay

![Desktop Calendar Overlay portfolio screenshot](docs/assets/desktop-calendar-overlay-portfolio.png)

**Desktop Calendar Overlay** is a Windows desktop calendar overlay for selected developers/testers who want a large, always-visible, low-distraction Google Calendar view while working. It is built as a .NET 8 WPF app with a Windows 11-style acrylic/minimal productivity direction: borderless overlay shell, compact month grid, collapsible day agenda, tray controls, persisted placement, and a separate settings window for account, display, theme, and layer controls.

This repository is the standalone implementation repo for the Windows app. Planning history and future product ideas remain outside this MVP repo so the codebase stays release-focused.

## Portfolio status

Current release line: **v0.95.0 official OAuth transition**

| Area | Status |
| --- | --- |
| Windows app MVP | Developer/tester release for overlay + Google Calendar CRUD MVP scope |
| Release artifact | `desktop-calendar-overlay-win-x64.zip` only |
| Runtime target | Windows x64, self-contained single-file publish |
| Visual proof | GitHub Actions Windows runner portfolio screenshot committed under `docs/assets/` |
| Credential policy | No OAuth client JSON, tokens, calendar exports, or user data in repo/release assets |
| Google sync mode | Official app OAuth client is preferred; local Desktop OAuth JSON remains developer fallback |

## What it demonstrates

- **Desktop productivity UX:** a persistent, resizable, lockable calendar layer that can sit beside normal work.
- **Windows/WPF execution:** .NET 8 WPF, MVVM-style view models, app/tray lifecycle, persisted window state, app icon metadata, and Windows startup option.
- **Google Calendar integration:** official OAuth client-first read/create/update/delete path through service interfaces, with local developer JSON fallback and safe mock fallback when credentials are absent.
- **Release discipline:** Windows CI build/release workflow, single ZIP artifact policy, QA checklist, startup diagnostics, and portfolio screenshot workflow.

## MVP scope

Included in the current MVP:

- .NET 8 + WPF desktop app.
- Borderless/resizable overlay window with persisted position/size and a position-lock toggle.
- Google Calendar read support behind service interfaces.
- Google Calendar create/update/delete path for user-initiated single-event CRUD.
- Separate Settings window for Google authentication, connect/disconnect, and calendar layer selection.
- Mock calendar fallback when no official/local Desktop OAuth client JSON or token is available.
- Settings shows Google auth state explicitly: mock mode, ready to connect, or connected.
- Disconnect deletes local Google token/cache state and returns to mock fallback after refresh/restart.
- Today date-number badge, display-format setting, opacity slider, event-list text-size slider, layer color palette, and theme selector in Settings.
- Tray menu with Show/Hide, Settings, Refresh, and Exit.
- Windows startup registration option.
- App/window/taskbar icon.

Intentionally out of MVP:

- Repeat events.
- Attendee/invitation workflows.
- Installer/MSIX/auto-update.
- Committed Google OAuth credentials, token stores, or user calendar data.
- Claiming Google OAuth verification approval before the external Google review is complete.

## Prerequisites

Build and visual validation must run on Windows:

- Windows 10/11, with Windows 11 preferred for acrylic/glass visual validation.
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).
- Visual Studio 2022 with .NET desktop development workload, or the .NET 8 SDK CLI.

> Development note: WPF targets `net8.0-windows`. macOS/Linux can inspect and version the repo, but release validation should be treated as complete only after Windows GitHub Actions or a Windows machine passes the build/runtime checks.

## Run, logs, and release package

Developer run on Windows:

```powershell
dotnet run --project .\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj
```

Diagnostics are written to:

```text
%LOCALAPPDATA%\DesktopCalendarOverlay\logs\startup.log
```

Release packaging uses one ZIP artifact, not an installer/MSIX:

- `desktop-calendar-overlay-win-x64.zip` — self-contained Windows x64 single-file publish output.

Extract the ZIP on Windows and run `DesktopCalendarOverlay.exe`. The app now prefers an official app-owned Desktop OAuth client JSON injected by the release process; local Desktop OAuth JSON remains available only as a developer fallback.

## Google Calendar OAuth mode

Google Calendar sync now uses an official-client-first provider chain. Public/release builds should package `google-oauth-client.official.json` beside the executable through an approved release-secret process. Developers can still use a local Google Cloud **Desktop app** OAuth client JSON fallback when the official file is absent.

Official-client lookup order:

1. `DCO_GOOGLE_OAUTH_CLIENT_JSON` environment override.
2. `google-oauth-client.official.json` beside `DesktopCalendarOverlay.exe`.
3. `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.official.json`.

Developer fallback path:

```text
%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json
```

Auth states shown in Settings:

- `Mock mode: no official OAuth client configured.` The app uses mock calendar data and Connect is disabled unless an official or developer fallback client file is available.
- `Ready to connect: official OAuth client available, not connected.` Connect opens the Google OAuth browser flow using the official app client.
- `Ready to connect: local OAuth JSON found, not connected.` Developer fallback is available when official config is absent.
- `Connected: Google Calendar sync enabled via official OAuth client.` Real calendar layers/events are active through the official client.
- `Connected: Google Calendar sync enabled via local OAuth JSON.` Developer fallback is connected.

Disconnect deletes the local token/cache state under `%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store` and the app returns to fallback behavior after refresh or restart. This release line still does not include a backend, service account, installer, MSIX package, or auto-update.

## Windows validation

From a Windows terminal at the repository root:

```powershell
.\scripts\windows-validate.ps1
dotnet run --project .\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj
```

Manual release checks are tracked in [`docs/QA_CHECKLIST_v0.95.0.md`](docs/QA_CHECKLIST_v0.95.0.md). Summary checks:

1. Window opens without normal OS chrome and remains resizable.
2. Position lock prevents title-drag movement/resizing after placing the overlay.
3. Moving/resizing the window, closing it, and reopening it restores placement.
4. UI remains legible at 100%, 125%, and 150% Windows display scaling.
5. Previous/next month buttons update the displayed calendar month and perform a real refresh.
6. Mock events render inside day cells as small time-ordered list items.
7. Selecting dates and collapsing/expanding the right agenda panel preserves selected-day state; cached date selection avoids unnecessary Google refreshes.
8. Settings opens as a separate dialog containing Google account connect/disconnect controls and calendar layer toggles.
9. Without official/local OAuth client JSON or token, mock calendar layers and events render safely and Settings shows `Mock mode: no official OAuth client configured.`
10. With a valid official OAuth client JSON, Settings shows `Ready to connect: official OAuth client available, not connected.`; with only local developer JSON, it shows the local fallback ready state.
11. Connect opens the Google OAuth browser flow and then loads real calendar layers/events with the official or fallback connected state.
12. Add/Edit/Delete single events and verify Google Calendar reflects the changes after refresh; failures show user-readable status errors and do not crash.
13. Disconnect deletes local token/cache state, refreshes to mock fallback, and remains disconnected after restart.
14. Theme, opacity, text size, display format, layer visibility, and layer colors persist after restart.
15. Tray menu exposes Show/Hide, Settings, Refresh, and Exit; Windows startup can be enabled/disabled from Settings.
16. The app/window/taskbar icon uses the calendar `.ico` asset.
17. The release page attaches exactly `desktop-calendar-overlay-win-x64.zip` for the version tag.

See [`scripts/windows-validate.ps1`](scripts/windows-validate.ps1) for a documented validation helper and [`docs/SPIKE_PLAN.md`](docs/SPIKE_PLAN.md) for the original spike plan.

## Repository layout

```text
src/DesktopCalendarOverlay/   WPF app project and MVVM implementation
docs/                         Architecture, QA, OAuth/security, and portfolio assets
scripts/                      Windows-side validation helper
.github/workflows/            Windows release and portfolio screenshot workflows
```

## Security notes

Do not commit OAuth client secrets, refresh tokens, exported credential files, or Google Cloud project configuration. Official OAuth client JSON must be injected through the approved release path (`DCO_GOOGLE_OAUTH_CLIENT_JSON` or packaged `google-oauth-client.official.json`), while developer fallback JSON remains at `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json`. The app stores tokens under `%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store`. See [`docs/OAUTH_AND_SECURITY.md`](docs/OAUTH_AND_SECURITY.md).

## Known limitations

- WPF visual/runtime validation is Windows-only.
- Real Google sync requires official OAuth client packaging and Google consent/verification setup; local JSON is developer fallback only.
- Repeat events and attendee/invitation workflows are intentionally out of MVP scope.
- Packaging is a single self-contained win-x64 ZIP; no separate portable-folder ZIP, installer, MSIX, or auto-update is included yet.
