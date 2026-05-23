# Desktop Calendar Overlay — Privacy Policy Draft

_Last updated: 2026-05-23_

This draft is for the public-readiness milestone and must be reviewed before any public listing, OAuth verification submission, or production release.

## Summary

Desktop Calendar Overlay is a local Windows desktop app that displays calendar information in a desktop overlay. The current Google Calendar integration is designed as a local desktop OAuth flow. The app does not operate a backend service and does not upload calendar data to a Desktop Calendar Overlay server.

## Data processed

When Google Calendar integration is enabled by the user, the app may process:

- Calendar list metadata needed to show available calendars and colors.
- Event titles, start/end times, calendar IDs, all-day state, descriptions, locations, and event identifiers needed to display, create, update, or delete calendar events.
- OAuth access/refresh tokens issued by Google for the local desktop app flow.
- Local UI settings such as window placement, opacity, theme, layer visibility, display mode, startup option, and local Google connection state.

## Local-only storage

- Calendar overlay settings are stored locally under the user's Windows local application data folder.
- Google OAuth tokens are stored locally in the app token store folder.
- The app does not send OAuth tokens, calendar events, settings, or logs to a Desktop Calendar Overlay server.
- The app does not include official production Google client secrets in the repository or release artifact at this milestone.

## Google Calendar access and user actions

The app requests Google Calendar access only so the user can view and manage their own calendar from the desktop overlay. Calendar create, update, and delete operations are user-initiated from the app UI. The app does not perform hidden background calendar edits unrelated to user actions.

## No selling, ads, or sharing

Desktop Calendar Overlay does not sell calendar data, use calendar data for advertising, or share calendar data with third parties for marketing. Calendar data is used only to provide the overlay, settings, and calendar-management features requested by the user.

## Disconnect and local deletion

The app provides a disconnect flow that deletes the local Google token store and returns the app to mock/local fallback behavior after refresh or restart. Users may also remove the app's local application data folder manually to clear local settings and token/cache state.

## Logs and diagnostics

Diagnostics are intended for local troubleshooting. Release artifacts must not include private OAuth JSON files, token stores, logs, or private calendar data.

## Current milestone limitations

- Google OAuth verification is not approved yet.
- The current Google integration remains a developer/tester-mode BYO OAuth JSON path unless and until an official verified OAuth client path is completed.
- This draft is not legal advice and needs final review before public distribution.
