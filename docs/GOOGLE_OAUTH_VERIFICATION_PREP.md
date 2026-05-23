# Google OAuth Verification Prep — v0.9.0

This document captures the work needed before Desktop Calendar Overlay can move from developer/tester BYO JSON mode to a public Google OAuth client.

## Current state

- App type: Windows desktop app.
- Backend: none. Calendar data and OAuth tokens stay local to the user's machine.
- Current connection path: official app-owned Desktop OAuth JSON is now the preferred provider path when packaged or supplied through the release environment; user-provided local Desktop OAuth JSON remains as a developer fallback.
- v0.95 code state: OAuth client loading is abstracted behind `IOAuthClientProvider`, token persistence is abstracted behind `ITokenStore`, and `CompositeOAuthClientProvider` selects `OfficialGoogleOAuthClientProvider` before `LocalJsonOAuthClientProvider`.

## Verification claims that must remain true

- The app uses Google Calendar data only to display and manage the user's calendar overlay.
- Tokens are stored locally and are removable through Disconnect.
- Calendar create/update/delete actions are initiated by the user.
- No calendar data is uploaded to an app-owned server.
- Calendar data is not sold, used for ads, or shared for marketing.

## Required pre-submission assets

1. Final public privacy policy URL based on `docs/PRIVACY_POLICY_DRAFT.md`.
2. Public support/contact route for users.
3. App homepage or landing page that describes the Google Calendar feature clearly.
4. OAuth consent screen content:
   - App name.
   - App logo if available.
   - User support email.
   - Developer contact email.
   - Privacy policy URL.
   - Authorized domains if a public site is used.
5. Scope justification for the minimum Google Calendar scopes actually requested by the app.
6. Demo video showing:
   - First launch without Google connected.
   - User-initiated connect/sign-in.
   - Calendar display.
   - User-initiated create/edit/delete.
   - Disconnect and local token removal behavior.
7. Release artifact audit showing no OAuth secrets, token stores, logs, or calendar data are packaged.

## Scope notes

The app currently references Google Calendar APIs for read/write calendar operations. Before submission, re-check the exact scopes requested in `GoogleCalendarService` and reduce them if the product can satisfy its public promise with narrower scopes.

## Official-client implementation path

1. Keep `LocalJsonOAuthClientProvider` as developer/tester fallback until public verification is complete.
2. Inject `google-oauth-client.official.json` only from the approved release packaging path or `DCO_GOOGLE_OAUTH_CLIENT_JSON`; never commit the real file to Git.
3. Ensure the official provider never commits client secrets or platform-private credentials to Git.
4. Preserve `IOAuthClientProvider.Availability` states so UI can distinguish missing, local JSON, and official-client readiness.
5. Re-run the full v0.95 validation checklist and Windows smoke before any public tag.

## Blockers before public OAuth claim

- Google OAuth verification has not been submitted or approved.
- Public privacy/support URLs are not finalized in this repository.
- A manual Windows demo video and release artifact audit still need to be produced for the real release candidate.
