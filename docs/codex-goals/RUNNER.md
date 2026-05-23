# Codex Release Train Runner Notes

Use this file as Core's quick command sheet while supervising Codex.

## Start Codex `/goal` TUI

```bash
cd /Users/namseokyoo/projects/desktop-calendar-overlay
HOME=/Users/namseokyoo codex --enable goals --no-alt-screen -C /Users/namseokyoo/projects/desktop-calendar-overlay
```

Hermes tool pattern:

```python
terminal(command="HOME=/Users/namseokyoo codex --enable goals --no-alt-screen -C /Users/namseokyoo/projects/desktop-calendar-overlay", workdir="/Users/namseokyoo/projects/desktop-calendar-overlay", background=True, pty=True)
```

Paste exactly one of:

- `docs/codex-goals/v0.8.0-goal.md`
- `docs/codex-goals/v0.9.0-goal.md`
- `docs/codex-goals/v0.95.0-rc1-goal.md`
- `docs/codex-goals/v1.0.0-goal.md`

If submit does not execute in Hermes PTY, send carriage return after submit.

## Verify after each milestone

```bash
git status --short
git diff --stat
git diff --check
```

On Windows:

```powershell
.\scripts\windows-validate.ps1
```

## Push/tag template

Only after gates pass:

```bash
git push -u origin <branch>
git tag <version>
git push origin <version>
```

If using GitHub release manually or via workflow, verify final release has exactly:

```text
desktop-calendar-overlay-win-x64.zip
```

## Steering message template

```text
Steering update from Core: stay within <version>. Do not implement future milestone scope. Complete the required QA/release/report files and record blocked Windows/OAuth gates honestly.
```
