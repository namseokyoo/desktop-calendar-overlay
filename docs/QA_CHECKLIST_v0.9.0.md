# QA Checklist — v0.9.0 Public-Readiness Foundation

Use this checklist on Windows before tagging `v0.9.0`. Automated macOS/CI checks are useful, but they do not replace the Windows UI smoke because this is a WPF desktop overlay.

## Automated gates

- [ ] `dotnet restore DesktopCalendarOverlay.sln`
- [ ] `dotnet build DesktopCalendarOverlay.sln --no-restore`
- [ ] `dotnet test DesktopCalendarOverlay.sln --no-restore`
- [ ] `git diff --check`
- [ ] `scripts/windows-validate.ps1` runs on a Windows host with .NET 8 SDK.

## First launch / fallback

- [ ] Clean install or clear local app data first.
- [ ] First launch without OAuth JSON opens successfully.
- [ ] App shows mock/fallback calendar data instead of crashing.
- [ ] Settings clearly communicates that Google is not connected.
- [ ] No token store is created until the user starts a Google sign-in path.

## Google developer/tester connect path

- [ ] Place a local Desktop OAuth JSON file at the documented app path.
- [ ] Settings changes from missing-client state to ready-to-connect state.
- [ ] Connect opens the Google OAuth browser flow for an allowed test user.
- [ ] Calendar layers and events load after sign-in.
- [ ] Restart preserves connected state through the local token store.

## Calendar CRUD

- [ ] Create event from the overlay UI.
- [ ] Edit event title/time/calendar from the overlay UI.
- [ ] Delete event from the overlay UI.
- [ ] Failed create/update/delete operations show user-readable errors and do not crash the app.
- [ ] When Google is not connected, CRUD routes to mock/local fallback and does not attempt live Google writes.

## Disconnect / restart

- [ ] Disconnect deletes the local token store.
- [ ] Refresh after disconnect returns to mock/fallback mode.
- [ ] Restart after disconnect remains disconnected.
- [ ] Reconnect works again with the same allowed test user.

## Overlay, scaling, and window behavior

- [ ] Borderless window opens with custom chrome only.
- [ ] Dragging the title area moves the window.
- [ ] Double-click toggles maximize/restore.
- [ ] Resize works on all edges and corners.
- [ ] Position lock disables move/resize and persists after restart.
- [ ] Move/resize/close/reopen restores placement.
- [ ] UI is legible at 100%, 125%, and 150% display scaling.
- [ ] Tray menu exposes Show/Hide, Settings, Refresh, and Exit.

## Release artifact audit

- [ ] ZIP contains only the expected Windows app files.
- [ ] ZIP does not contain OAuth JSON files.
- [ ] ZIP does not contain token stores.
- [ ] ZIP does not contain logs.
- [ ] ZIP does not contain private calendar data.
- [ ] Version label shows `v0.9.0-public-readiness`.
- [ ] Release notes and privacy/OAuth prep docs are included or linked from the release page.

## Manual blockers to record honestly

- [ ] Google OAuth verification approval is not claimed unless the approval exists.
- [ ] Windows manual smoke result is recorded with host/version/date.
- [ ] Release tag is not created until automated gates and required manual checks are complete.
