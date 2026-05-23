# Desktop Calendar Overlay v0.8.0 Manual QA Checklist

Version under test: `0.8.0-developer-test`

Use this as the canonical release-readiness checklist for v0.8.0. Google Calendar sync is `developer/tester mode`; do not paste OAuth client JSON, tokens, authorization codes, refresh tokens, auth headers, or private calendar payloads into reports.

## 1. Build and release ZIP audit

- [ ] On Windows with .NET 8 SDK, run `./scripts/windows-validate.ps1` from the repo root.
- [ ] Run `dotnet build .\DesktopCalendarOverlay.sln --configuration Release --no-restore` after restore.
- [ ] Publish only `desktop-calendar-overlay-win-x64.zip`, the self-contained Windows x64 single-file ZIP.
- [ ] Confirm the ZIP contains `DesktopCalendarOverlay.exe`.
- [ ] Confirm the ZIP does not contain `%LOCALAPPDATA%` content, `google-oauth-client.json`, `google-token-store`, logs, private calendar exports, installer/MSIX files, or a separate portable ZIP.

## 2. First launch and mock mode

- [ ] Remove or move `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json`.
- [ ] Remove `%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store`.
- [ ] Launch the app from the release ZIP on Windows.
- [ ] App opens without crashing and shows mock calendar layers/events.
- [ ] Settings shows `Mock mode: no OAuth JSON found.`
- [ ] Connect is disabled while the OAuth JSON is missing.
- [ ] Refresh from Settings and tray completes without crashing.

## 3. JSON present, not connected

- [ ] Place a valid local Google Cloud Desktop OAuth JSON at `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json`.
- [ ] Keep `%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store` absent.
- [ ] Start or refresh the app.
- [ ] Settings shows `Ready to connect: OAuth JSON found, not connected.`
- [ ] Connect is enabled and Disconnect remains disabled.
- [ ] Mock calendar fallback remains active until Connect completes.

## 4. Connect and Google Calendar sync

- [ ] Click Connect.
- [ ] Browser OAuth flow opens for an allowed Google OAuth test user.
- [ ] Complete consent without copying secrets or authorization codes into the report.
- [ ] App returns to Settings/main overlay without crashing.
- [ ] Settings shows `Connected: Google Calendar sync enabled.`
- [ ] Real Google calendar layers and visible events load.
- [ ] Manual refresh and month navigation refresh Google Calendar data as appropriate.

## 5. CRUD and failure behavior

- [ ] Create a timed event and verify it appears in Google Calendar after refresh.
- [ ] Create an all-day event and verify it appears in Google Calendar after refresh.
- [ ] Edit title, time/date, and location; verify Google Calendar reflects the change after refresh.
- [ ] Delete an event only after confirmation; verify Google Calendar removes it after refresh.
- [ ] Simulate a network/token/Google request failure during create, edit, or delete.
- [ ] Failure shows a user-readable status error and the app keeps running.
- [ ] Diagnostics record only sanitized operational error detail, not tokens, OAuth codes, client secrets, auth headers, or full raw event payloads.

## 6. Disconnect and restart

- [ ] Click Disconnect from Settings.
- [ ] Confirm `%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store` is deleted.
- [ ] App refreshes to mock calendar fallback without crashing.
- [ ] Settings returns to `Ready to connect: OAuth JSON found, not connected.` if the local OAuth JSON remains.
- [ ] Close and restart the app.
- [ ] App remains disconnected, uses mock fallback until reconnect, and does not silently recreate token state.
- [ ] Remove the OAuth JSON and restart again.
- [ ] Settings shows `Mock mode: no OAuth JSON found.`

## 7. Window shell, settings, and persistence

- [ ] Borderless overlay opens, drags, resizes from edges/corners, minimizes, and restores.
- [ ] Close button hides to tray; tray **Show/Hide** restores the overlay.
- [ ] Tray menu has **Show/Hide**, **Settings**, **Refresh**, and **Exit**.
- [ ] Tray **Exit** fully closes the app.
- [ ] Position lock disables move/resize and persists across restart.
- [ ] Move/resize, close/reopen, and restart restore placement.
- [ ] Validate legibility at 100%, 125%, and 150% display scaling.
- [ ] Theme, opacity, event text size, event display format, layer visibility, and layer colors persist after restart.
- [ ] Windows startup option can be enabled/disabled under normal user permissions and matches the Run registry state.

## 8. Known limitations acceptance

- [ ] Tester understands v0.8.0 Google sync is `developer/tester mode`, not a normal public OAuth flow.
- [ ] Tester understands real sync requires `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json` and an allowed Google test user.
- [ ] No official OAuth client mode, backend, service account, repeat events, attendee/invitation workflow, installer/MSIX, or auto-update is expected in this release.
- [ ] Release notes and README do not imply normal users can connect without setup.
