# Desktop Calendar Overlay v0.6.0/v0.6.1 Manual QA Checklist

Version under test: `0.6.1-performance-cache`

Use this as the canonical release-readiness checklist for v0.6.0 and the v0.6.1 performance-cache patch. Do not paste OAuth client JSON, tokens, authorization codes, refresh tokens, or private calendar payloads into reports.

Archived note: v0.7.x and later use the current v0.7 release checklist (for example [`QA_CHECKLIST_v0.7.1.md`](QA_CHECKLIST_v0.7.1.md)) and publish only the single self-contained `desktop-calendar-overlay-win-x64.zip`; the separate portable-folder ZIP below is historical v0.6.x policy.

## 1. Build and artifact preflight

- [ ] On Windows with .NET 8 SDK, run `./scripts/windows-validate.ps1` from the repo root.
- [ ] Run `dotnet build .\DesktopCalendarOverlay.sln --configuration Release --no-restore` after restore.
- [ ] Publish both ZIP shapes described in `README.md`: single-file and portable-folder Windows x64.
- [ ] Confirm release artifacts do not include OAuth client JSON, token stores, or local logs.

## 2. Mock/no-credential mode

- [ ] Launch with no OAuth client JSON at `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json`.
- [ ] App opens without crashing and shows mock calendar data.
- [ ] Settings clearly reports the missing OAuth client and disables Connect.
- [ ] Refresh from Settings and tray completes without crashing.

## 3. Window shell and tray

- [ ] Borderless overlay opens, drags, resizes from edges/corners, minimizes, and restores.
- [ ] Close button hides to tray; tray **Show/Hide** restores the overlay.
- [ ] Tray menu has **Show/Hide**, **Settings**, **Refresh**, and **Exit**.
- [ ] Tray **Exit** fully closes the app.
- [ ] Position lock disables move/resize and persists across restart.
- [ ] Move/resize, close/reopen, and restart restore placement.
- [ ] Validate legibility at 100%, 125%, and 150% display scaling.

## 4. Calendar UI polish

- [ ] Today badge/date number is visually distinct.
- [ ] Upcoming events in the next 24 hours are emphasized in preview/detail cards.
- [ ] Previous/next month updates the calendar grid without losing stability and performs a real calendar refresh.
- [ ] Selecting several dates in the already-loaded month updates the detail panel immediately from cache; status/log timing should report cached selection, with no visible Google refresh spinner/network delay.
- [ ] Initial startup/manual refresh timing remains distinguishable from cached date-selection timing in status text or diagnostics.
- [ ] Long event titles truncate/wrap without breaking day cells or detail cards.
- [ ] Long locations wrap/truncate in the detail card and remain readable via tooltip.
- [ ] Detail panel expands/collapses without losing selected-day state.
- [ ] Event order remains all-day first, then chronological.

## 5. Settings persistence

- [ ] Theme selection persists after app restart.
- [ ] Overlay opacity persists after app restart.
- [ ] Event text-size slider persists after app restart.
- [ ] Event display format (`time · event` / `event · time`) persists after app restart.
- [ ] Layer visibility persists after app restart and refreshes the UI immediately.
- [ ] Layer color overrides persist after app restart and refresh visible events immediately.
- [ ] Windows startup option can be enabled/disabled under normal user permissions and matches the Run registry state.

## 6. Google Calendar sync and CRUD

- [ ] With a valid local Desktop OAuth client JSON, Connect opens the browser OAuth flow.
- [ ] After consent, real calendar layers and visible events load.
- [ ] Network/token/list failures show a user-visible status error and write diagnostics instead of crashing.
- [ ] Refresh after reconnect/token change updates calendar layers/events.
- [ ] Month navigation, manual refresh, connect/disconnect, layer visibility changes, and add/edit/delete flows still refresh Google Calendar data as appropriate.
- [ ] Create a timed event and verify it appears in Google Calendar after refresh.
- [ ] Create an all-day event and verify it appears in Google Calendar after refresh.
- [ ] Edit title, time/date, and location; verify Google Calendar reflects the change after refresh.
- [ ] Delete an event only after confirmation; verify Google Calendar removes it after refresh.
- [ ] Disconnect deletes local token state and returns to safe mock/disconnected behavior.

## 7. Logs and known limitations

- [ ] Diagnostics are written to `%LOCALAPPDATA%\DesktopCalendarOverlay\logs\startup.log`.
- [ ] Logs contain no OAuth secrets, tokens, authorization headers, or raw private calendar payloads.
- [ ] Known v0.6.x limitations are accepted: Windows-only WPF validation, local OAuth Desktop client setup required for real sync, no repeat/attendee workflows, and no installer/MSIX yet.
