# OAuth and Security Notes

## Current state

This bootstrap contains no real Google integration. It must not include OAuth credentials, Google Cloud config files, refresh tokens, access tokens, or user calendar exports.

## OAuth mode for MVP development

- Use Google OAuth testing mode during development.
- Add only explicit test users while the OAuth consent screen is unverified.
- Request the minimum scopes needed for calendar read and user-initiated event creation.
- Explain in Settings that access is used to read calendars and create new events only when the user explicitly saves one.

## Create-only write policy

The MVP write policy is intentionally narrow:

- Allow creating a single event initiated by the user from the app UI.
- Do not edit existing events in MVP.
- Do not delete events in MVP.
- Do not create repeat rules in MVP.
- Do not manage attendees/invitations in MVP.

Represent this in code by keeping creation as an explicit service method, separate from read/list methods. Avoid broad generic mutation methods until post-MVP requirements are approved.

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
