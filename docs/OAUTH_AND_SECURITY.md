# OAuth and Security Notes

## Current state

v0.95 Google Calendar sync is **official OAuth client-first**. The app can read calendars and perform user-initiated create/update/delete operations through Google Calendar APIs when an official app-owned Desktop OAuth JSON is supplied by the release process. If the official file is absent, a local Desktop OAuth JSON remains available as a developer fallback. If neither OAuth client file nor token is available, the app safely falls back to mock calendar data.

No OAuth client JSON, refresh tokens, access tokens, Google Cloud project configuration, or user calendar data should be committed to the repo.

## OAuth mode for release candidates

- Use the official app-owned Google Cloud Desktop OAuth client for public/release-candidate validation.
- Use Google OAuth testing mode and explicit test users until the OAuth consent screen is verified.
- Keep local BYO OAuth JSON only as a developer fallback.
- Request the minimum scopes needed for calendar read and user-initiated event create/update/delete.
- Explain in Settings that access is used to read calendars and create/update/delete events only when the user explicitly saves or confirms one.

## OAuth file locations

Official-client lookup order:

1. `DCO_GOOGLE_OAUTH_CLIENT_JSON` environment variable path.
2. `google-oauth-client.official.json` beside `DesktopCalendarOverlay.exe`.
3. `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.official.json` for manual smoke.

Developer fallback path:

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

- `Mock mode: no official OAuth client configured.` No official or fallback OAuth client is available and the app uses mock calendar data.
- `Ready to connect: official OAuth client available, not connected.` The official app OAuth client is selected and Connect can start the OAuth browser flow.
- `Connected: Google Calendar sync enabled via official OAuth client.` The official app OAuth client and local token store are active.
- `Ready to connect: local OAuth JSON found, not connected.` Developer fallback is selected because official config is absent.
- `Connected: Google Calendar sync enabled via local OAuth JSON.` Developer fallback is connected.

Disconnect deletes `%LOCALAPPDATA%\DesktopCalendarOverlay\google-token-store` and returns the app to fallback behavior after refresh/restart. OAuth client JSON files remain where they are and must be removed from their source path if the user no longer wants the app to offer Connect.

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
- Do not commit or log official OAuth client JSON; inject it only through approved release packaging or local smoke paths.

## Logging policy

Never log:

- Access tokens.
- Refresh tokens.
- OAuth authorization codes.
- Client secrets.
- Full HTTP authorization headers.
- Full raw Google API payloads that may contain private event details.

Logs may include sanitized operational facts such as sync status, request category, and high-level error codes.
