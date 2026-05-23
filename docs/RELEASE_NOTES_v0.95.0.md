# Release Notes — v0.95.0 Official OAuth Client Transition

`v0.95.0` changes Google Calendar sync from a BYO OAuth JSON-first developer/tester flow to an official app OAuth client-first architecture.

## Highlights

- Added `OfficialGoogleOAuthClientProvider`.
- Added `CompositeOAuthClientProvider` that prefers official app OAuth config and falls back to local developer JSON only when official config is absent.
- Replaced the placeholder `FutureOfficial` availability state with `Official`.
- Updated Settings copy so official OAuth is the expected public path and local JSON is a developer fallback.
- Added tests proving official provider priority and fallback behavior.

## Official OAuth config loading

The app now looks for the official OAuth desktop client JSON in this priority order:

1. Environment override: `DCO_GOOGLE_OAUTH_CLIENT_JSON`
2. App install directory: `google-oauth-client.official.json` next to the executable
3. Local app data override: `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.official.json`

The developer fallback remains:

- `%LOCALAPPDATA%\DesktopCalendarOverlay\google-oauth-client.json`

## Security notes

- Real OAuth JSON files are still not committed to Git.
- `.gitignore` now blocks common OAuth client JSON names.
- Release packaging must inject the official OAuth client file from a secure release secret or manually controlled release process.
- Token/cache state remains local and Disconnect clears the local token store.

## Validation

- `dotnet restore DesktopCalendarOverlay.sln`
- `dotnet build DesktopCalendarOverlay.sln --no-restore`
- `dotnet test DesktopCalendarOverlay.sln --no-restore`
- `git diff --check`

## Still blocked before public claim

- Google OAuth consent screen setup/submission/approval is still external to this repo.
- Windows OAuth smoke with the real official client file is still required.
- Release ZIP audit must confirm no unreviewed secrets, tokens, logs, or private calendar data are included.
