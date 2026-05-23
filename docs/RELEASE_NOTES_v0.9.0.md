# Release Notes — v0.9.0 Public-Readiness Foundation

`v0.9.0` prepares Desktop Calendar Overlay for a future public Google Calendar release while preserving the current developer/tester BYO OAuth JSON flow.

## Highlights

- Added explicit Google OAuth client provider abstraction.
- Added local Google token store abstraction.
- Preserved BYO Desktop OAuth JSON support for developer/tester mode.
- Added service/model test project runnable with `dotnet test`.
- Added privacy policy draft and Google OAuth verification prep notes.
- Strengthened Windows validation expectations for restore/build/test and release artifact hygiene.

## User-visible changes

- Settings can distinguish missing OAuth client, local JSON developer/tester mode, and future official OAuth client readiness.
- Version label now reports `v0.9.0-public-readiness`.
- Disconnect continues to clear local Google token state and return the app to fallback behavior after refresh/restart.

## Developer changes

- `ITokenStore` isolates token persistence from Google calendar service logic.
- `IOAuthClientProvider` isolates OAuth client-secret loading and availability states.
- `CalendarServiceRouter` continues to use Google only when client and stored token state indicate Google is available; otherwise it falls back to mock calendar behavior.
- New xUnit tests cover router fallback/availability behavior and JSON settings persistence.

## Validation

Validated locally on macOS with the .NET 8 SDK path used for this repo:

- `dotnet restore DesktopCalendarOverlay.sln`
- `dotnet build DesktopCalendarOverlay.sln --no-restore`
- `dotnet test DesktopCalendarOverlay.sln --no-restore` — 6 tests passed
- `git diff --check`

## Known blockers before public release

- Google OAuth verification is not approved yet.
- Official public OAuth client distribution is not implemented in this milestone.
- Windows manual smoke and release ZIP audit still need to be completed on a Windows host before tagging a public release.
- Do not package OAuth JSON files, token stores, logs, or private calendar data in release artifacts.
