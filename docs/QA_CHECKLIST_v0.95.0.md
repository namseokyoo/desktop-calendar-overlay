# QA Checklist — v0.95.0 Official OAuth Client Transition

Use this checklist before tagging or merging the official OAuth client transition.

## Automated gates

- [ ] `dotnet restore DesktopCalendarOverlay.sln`
- [ ] `dotnet build DesktopCalendarOverlay.sln --no-restore`
- [ ] `dotnet test DesktopCalendarOverlay.sln --no-restore`
- [ ] `git diff --check`
- [ ] `scripts/windows-validate.ps1` runs on Windows with .NET 8 SDK.

## Official OAuth priority

- [ ] With `google-oauth-client.official.json` beside the executable, Settings shows official OAuth ready/connected states.
- [ ] With both official JSON and local developer JSON present, official provider wins.
- [ ] With official JSON absent and local developer JSON present, local JSON fallback still works for developer/tester validation.
- [ ] With no OAuth JSON files present, app falls back to mock calendar data without crashing.
- [ ] Environment override `DCO_GOOGLE_OAUTH_CLIENT_JSON` can point to an official client file for CI/manual smoke.

## Connect / disconnect

- [ ] Connect opens the Google OAuth browser flow using the official app OAuth client file.
- [ ] Calendar layers and events load after successful sign-in.
- [ ] Create/edit/delete actions are user-initiated and operate on the selected calendar.
- [ ] Disconnect deletes the local token store.
- [ ] Restart after disconnect remains disconnected until the user reconnects.

## Release artifact audit

- [ ] The official OAuth JSON is included only through the approved release packaging path.
- [ ] The artifact does not contain local developer BYO JSON.
- [ ] The artifact does not contain token stores, logs, screenshots with private calendar data, or exported calendar payloads.
- [ ] The release notes describe official OAuth as the primary path and local JSON as developer fallback.
- [ ] Google OAuth verification status is stated honestly; no approval is claimed unless approval exists.

## Manual blockers to record

- [ ] Google Cloud OAuth client ID / consent screen source is documented outside Git secrets.
- [ ] Google OAuth verification submission/approval status is recorded.
- [ ] Windows smoke host/date/result is recorded.
