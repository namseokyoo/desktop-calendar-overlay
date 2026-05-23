# Desktop Calendar Overlay Codex `/goal` Release Train Plan

> **Primary operator:** Core supervises. Codex executes one milestone at a time through `/goal` or `codex exec`.
>
> **Important rule:** Do not fake release progression. Each version must leave measurable evidence: commits, local validation logs, GitHub push/tag evidence when available, and a milestone report. If a gate cannot run on macOS because WPF is Windows-only, record it as `BLOCKED: Windows runner required`, not as passed.

## Goal

Take `desktop-calendar-overlay` from current `v0.7.1` portfolio readiness through `v0.8.0`, `v0.9.0`, `v0.95.0-rc1`, and `v1.0.0` with a visible push/verify/release cadence.

## Operating model

1. Start a Codex background PTY in the repo:

```bash
HOME=/Users/namseokyoo codex --enable goals --no-alt-screen -C /Users/namseokyoo/projects/desktop-calendar-overlay
```

2. Feed one milestone prompt at a time from `docs/codex-goals/`.
3. Codex must work in a branch for each milestone.
4. Codex must commit at logical checkpoints.
5. Core verifies filesystem/git evidence before allowing the next milestone.
6. Only after a milestone gate passes: push branch/tag or prepare the exact push commands if credentials/CI are unavailable in the current environment.

## Branch/tag cadence

| Milestone | Branch | Tag | Release meaning |
| --- | --- | --- | --- |
| v0.8.0 | `release/v0.8.0-developer-test` | `v0.8.0` | Honest developer/tester Google OAuth flow |
| v0.9.0 | `release/v0.9.0-public-readiness` | `v0.9.0` | Public-readiness foundations, tests, privacy docs |
| v0.95.0-rc1 | `release/v0.95.0-rc1` | `v0.95.0-rc1` | OAuth verification/release candidate package |
| v1.0.0 | `release/v1.0.0` | `v1.0.0` | Public desktop release, only if real user onboarding is acceptable |

## Global non-negotiable gates

These apply to every milestone:

- `git status --short` reviewed before starting and after finishing.
- No OAuth JSON, token store, logs, exported calendar data, or Google Cloud secrets in repo or ZIP.
- README and docs must not overclaim user readiness.
- WPF build/runtime gates must be run on Windows/GitHub Actions; macOS inspection alone is not a pass.
- Each milestone creates or updates:
  - `docs/QA_CHECKLIST_<version>.md`
  - `docs/RELEASE_NOTES_<version>.md`
  - `docs/reports/<version>-codex-execution-report.md`
- Each milestone report must contain:
  - goals completed
  - files changed summary
  - commands run and results
  - blocked checks
  - release/push/tag evidence or exact pending commands

## Version gates summary

| Milestone | Goal | Measurable gate |
| --- | --- | --- |
| v0.8.0 | Current app becomes safe developer/test release | developer/tester labeling, settings auth states, disconnect fallback, CRUD error handling, artifact hygiene checklist |
| v0.9.0 | App foundations become public-release-ready | token/client abstractions, test project, privacy draft, stronger validation script, tests pass |
| v0.95.0-rc1 | OAuth verification and RC package ready | scopes documented, verification evidence docs, official-client path/fallback design, RC QA checklist |
| v1.0.0 | Public desktop release candidate finalized | v1.0 docs, version metadata, all gates green, GitHub release artifact policy verified |

## Core supervision checklist per milestone

1. Read the milestone prompt.
2. Start Codex with the exact goal text.
3. Monitor for actual file changes, not only narration.
4. If Codex stalls or drifts, send:

```text
Steering update from Core: stay within <milestone>; do not implement later-version scope. Produce the required docs/report/gates before finishing.
```

5. After Codex says done, verify:

```bash
git diff --stat
git status --short
```

6. Run available local checks:

```bash
git diff --check
```

7. On Windows/GitHub Actions, run:

```powershell
.\scripts\windows-validate.ps1
```

8. If gate passes, push/tag according to the milestone instructions. If not possible, record the exact blocker in the report.

## Stop conditions

Stop and ask Core/형 before continuing if:

- Google OAuth verification is required but not approved.
- The implementation would require shipping secrets or embedding unsafe credentials.
- A milestone cannot pass its own safety gates.
- Codex proposes backend/SaaS architecture before v1.0 without explicit approval.
- v1.0 would still require normal users to create Google Cloud credentials, unless explicitly renamed beta/limited.
