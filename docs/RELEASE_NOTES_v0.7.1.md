# Desktop Calendar Overlay v0.7.1 Release Notes

Tag target: `v0.7.1-portfolio-readiness`

## Summary

v0.7.1 is a portfolio-readiness release. It keeps the v0.7 app behavior intact and packages the project as a cleaner public portfolio artifact before the next planning phase for a fuller deployment/distribution model.

## Changes

- Refined README into a portfolio-facing project page with status, scope, validation, release artifact policy, and security notes.
- Added `docs/QA_CHECKLIST_v0.7.1.md` as the current manual QA checklist.
- Bumped app/package metadata to `0.7.1-portfolio-readiness`.
- Preserved the single release artifact policy: `desktop-calendar-overlay-win-x64.zip` only.
- Kept the Windows portfolio screenshot workflow available for manual visual proof from GitHub Actions.

## Release artifact policy

The release should attach exactly one downloadable asset:

- `desktop-calendar-overlay-win-x64.zip`

No OAuth client JSON, refresh tokens, access tokens, private calendar exports, local logs, installer, MSIX package, or separate portable-folder ZIP should be attached.

## Validation notes

- Windows CI build/release must pass on the tag.
- The release ZIP must contain `DesktopCalendarOverlay.exe`.
- Manual Windows usability/OAuth QA remains recommended before treating the app as end-user deployment-ready.
