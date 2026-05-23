# Desktop Calendar Overlay v0.8.0 Release Notes

Tag target: `v0.8.0`

## Summary

v0.8.0 is a developer/tester Google Calendar release. It keeps the local-first WPF overlay and BYO Google Desktop OAuth JSON model, while making the OAuth state, disconnect behavior, CRUD failure handling, and release limitations explicit.

Google Calendar sync is in `developer/tester mode`. Real sync requires a local Google Cloud **Desktop app** OAuth JSON at:

```text
%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json
```

This is not a normal public OAuth onboarding release.

## Changes

- Updated README and OAuth/security docs to describe v0.8 as `developer/tester mode`.
- Added exact Settings auth states for mock mode, ready to connect, and connected sync.
- Kept mock calendar fallback when no local OAuth JSON/token is available.
- Hardened disconnect so it deletes the local Google token store and returns to mock fallback after refresh/restart.
- Kept create/edit/delete failures caught at the view-model boundary with user-readable status errors.
- Reduced diagnostic exception detail so logs do not dump tokens, OAuth codes, client secrets, auth headers, or full raw API payloads.
- Bumped app metadata to `0.8.0-developer-test`.

## Release artifact policy

The release should attach exactly one downloadable asset:

- `desktop-calendar-overlay-win-x64.zip`

No OAuth client JSON, refresh tokens, access tokens, private calendar exports, local logs, installer, MSIX package, or separate portable-folder ZIP should be attached.

## Out of scope

- Official public OAuth client mode.
- Backend OAuth broker.
- Service account flow.
- Repeat events.
- Attendee/invitation workflow.
- Installer/MSIX/auto-update.

## Validation notes

- Windows CI/build validation must pass before tagging.
- Manual Windows OAuth checks require a valid local Desktop OAuth JSON and an allowed Google test user.
- If Windows/OAuth validation cannot be run from the current environment, record it as blocked in the execution report instead of treating the release as fully verified.
