# OAuth and Security Notes

## Current state

v0.8.0 Google Calendar sync is `developer/tester mode`. This build contains Google Calendar read/create/update/delete integration for allowed test users who provide a local Google Cloud **Desktop app** OAuth client JSON. Without that local JSON and token, it safely falls back to mock calendar data. No OAuth client secrets, refresh tokens, access tokens, or Google Cloud configuration are committed to the repo.

This is not normal consumer OAuth onboarding. There is no official public OAuth client mode, backend OAuth broker, service account flow, installer, MSIX package, or auto-update in v0.8.0.

## OAuth mode for developer/tester release

- Use Google OAuth testing mode for v0.8 developer/tester validation.
- Add only explicit test users while the OAuth consent screen is unverified.
- Request the minimum scopes needed for calendar read and user-initiated event create/update/delete.
- Explain in Settings that access is used to read calendars and create/update/delete events only when the user explicitly saves or confirms one.

## Local OAuth file locations

For local testing, place the downloaded Google Cloud **Desktop app** OAuth JSON here on Windows:

```text
%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json
```

The app stores Google OAuth tokens under:

```text
%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store
```

The token store is local-only and must not be committed, copied into release artifacts, pasted into chat, or logged.

## Auth states shown in Settings

Settings must present one of these user-readable states:

- `Mock mode: no OAuth JSON found.` The local Desktop OAuth JSON is missing and the app uses mock calendar data.
- `Ready to connect: OAuth JSON found, not connected.` The local Desktop OAuth JSON exists, no local token is active, and Connect can start the OAuth browser flow for an allowed test user.
- `Connected: Google Calendar sync enabled.` The local Desktop OAuth JSON and token store are present, and real Google Calendar sync is active.

Disconnect deletes `%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store` and returns the app to mock fallback after refresh/restart. The local Desktop OAuth JSON remains local and must be removed manually if the tester no longer wants the app to offer Connect.

## Narrow CRUD write policy

The MVP write policy is intentionally narrow:

- Allow creating, editing, and deleting a single event only after a user action from the app UI.
- Event mutations send only title, calendar layer, start/end or all-day date, and optional location to Google Calendar.
- Show a clear status error and keep the app running when network, token, or Google CRUD requests fail.
- Do not create repeat rules in MVP.
- Do not manage attendees/invitations in MVP.

Represent this in code by keeping explicit create/update/delete service methods separate from read/list methods. Avoid broad generic mutation methods until post-MVP requirements are approved.

## Token and credential storage

Preferred Windows storage options:

1. Windows Credential Manager through a small adapter service.
2. DPAPI-protected local data if Credential Manager integration is not selected.

Guidance:

- Keep credential storage behind an interface.
- Store refresh tokens only in OS-protected storage.
- Store non-sensitive settings, such as selected calendar layer IDs and window placement, separately from secrets.
- Provide a Settings action to disconnect Google and delete local tokens/cache.

## Logging policy

Never log:

- Access tokens.
- Refresh tokens.
- OAuth authorization codes.
- Client secrets.
- Full HTTP authorization headers.
- Full raw Google API payloads that may contain private event details.

Logs may include sanitized operational facts such as sync status, request category, and high-level error codes.
