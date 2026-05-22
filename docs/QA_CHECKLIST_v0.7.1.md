# Desktop Calendar Overlay v0.7.1 Manual QA Checklist

Version under test: `0.7.1-portfolio-readiness`

Use this as the canonical release-readiness checklist for v0.7.1. Do not paste OAuth client JSON, tokens, authorization codes, refresh tokens, or private calendar payloads into reports.

## 1. Build and artifact preflight

- [ ] On Windows with .NET 8 SDK, run `./scripts/windows-validate.ps1` from the repo root.
- [ ] Run `dotnet build .\DesktopCalendarOverlay.sln --configuration Release --no-restore` after restore.
- [ ] Publish only `desktop-calendar-overlay-win-x64.zip`, the self-contained Windows x64 single-file ZIP.
- [ ] Confirm no `desktop-calendar-overlay-win-x64-portable.zip` is produced or attached for v0.7.
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

- [ ] Today, selected date, dates with events, and out-of-month dates are visually distinct in the month grid.
- [ ] Day-cell event chips remain readable for all-day events, timed events, long titles, and `+N more` overflow.
- [ ] Upcoming events in the next 24 hours are emphasized in preview/detail cards without hiding layer color.
- [ ] Previous/next month updates the calendar grid without losing stability and performs a real calendar refresh.
- [ ] Selecting several dates in the already-loaded month updates the detail panel immediately from cache; diagnostics should report cached selection without visible Google refresh delay.
- [ ] Empty selected days show a clear no-events state with an obvious add-event path.
- [ ] Long event titles truncate/wrap without breaking day cells or detail cards.
- [ ] Long locations wrap/truncate in the detail card and remain readable via tooltip.
- [ ] Detail panel expands/collapses without losing selected-day state.
- [ ] Event order remains all-day first, then chronological.

## 5. Settings and theme persistence

- [ ] Settings copy clearly explains Google connection, calendar layers, display, and window behavior.
- [ ] OAuth JSON path remains visible; there is no import/upload flow.
- [ ] Theme selection persists after app restart.
- [ ] Ivory Editorial is legible as a representative light theme.
- [ ] Acrylic Dark and Midnight Blue remain legible regressions.
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
- [ ] Create dialog distinguishes create vs edit copy and action labels.
- [ ] Time hints are clear; all-day state disables timed input without losing date/title/location.
- [ ] Validation messages are understandable for missing layer, title, date, bad time, and end-before-start.
- [ ] Create a timed event and verify it appears in Google Calendar after refresh.
- [ ] Create an all-day event and verify it appears in Google Calendar after refresh.
- [ ] Edit title, time/date, and location; verify Google Calendar reflects the change after refresh.
- [ ] Delete an event only after confirmation; verify Google Calendar removes it after refresh.
- [ ] Disconnect deletes local token state and returns to safe mock/disconnected behavior.

## 7. Logs and known limitations

- [ ] Bottom status text is concise and user-facing; detailed timing remains in diagnostics.
- [ ] Diagnostics are written to `%LOCALAPPDATA%\DesktopCalendarOverlay\logs\startup.log`.
- [ ] Logs contain no OAuth secrets, tokens, authorization headers, or raw private calendar payloads.
- [ ] Known v0.7.1 limitations are accepted: Windows-only WPF validation, local OAuth Desktop client setup required for real sync, no repeat/attendee workflows, no separate portable ZIP, and no installer/MSIX/auto-update.


## 8. Portfolio/release-page readiness

- [ ] README hero image renders from `docs/assets/desktop-calendar-overlay-portfolio.png`.
- [ ] GitHub Actions Windows portfolio screenshot workflow can be run manually from `main`.
- [ ] Release page for the version tag includes exactly `desktop-calendar-overlay-win-x64.zip`.
- [ ] Release notes explain that this is portfolio-readiness packaging, not a new installer/autoupdate distribution.
