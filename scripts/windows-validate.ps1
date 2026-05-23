# Windows validation helper for Desktop Calendar Overlay.
# Run from the repository root on Windows with the .NET 8 SDK installed.

$ErrorActionPreference = "Stop"

$Solution = Join-Path $PSScriptRoot "..\DesktopCalendarOverlay.sln"
$Project = Join-Path $PSScriptRoot "..\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj"
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")

Write-Host "== host =="
Write-Host "OS: $([System.Environment]::OSVersion.VersionString)"
Write-Host "Repo: $RepoRoot"

Write-Host "== dotnet info =="
dotnet --info

Write-Host "== project target sanity =="
Select-String -Path $Project -Pattern "<TargetFramework>net8.0-windows</TargetFramework>", "<UseWPF>true</UseWPF>" | ForEach-Object {
    Write-Host $_.Line.Trim()
}

Write-Host "== restore solution =="
dotnet restore $Solution

Write-Host "== build solution =="
dotnet build $Solution --no-restore

Write-Host "== test solution =="
dotnet test $Solution --no-restore

Write-Host "== run command =="
Write-Host "dotnet run --project $Project"

Write-Host "== release artifact hygiene checklist =="
Write-Host "Before publishing a ZIP, inspect the artifact and confirm:"
Write-Host "[ ] ZIP contains the expected Desktop Calendar Overlay app files only."
Write-Host "[ ] ZIP contains the official OAuth JSON only when it was injected through the approved release path."
Write-Host "[ ] ZIP does not contain local developer OAuth JSON, token stores, logs, or private calendar data."
Write-Host "[ ] ZIP does not contain google-token-store or other token/cache folders."
Write-Host "[ ] ZIP does not contain logs, local settings, screenshots with private calendar data, or private calendar exports."
Write-Host "[ ] Version label and release notes match the intended release."

Write-Host "== manual visual validation checklist =="
Write-Host "Canonical v0.9.0 checklist: docs/QA_CHECKLIST_v0.9.0.md"
Write-Host "[ ] Borderless window opens with custom chrome only."
Write-Host "[ ] Dragging the title area moves the window; double-click toggles maximize/restore."
Write-Host "[ ] Resize works on all edges and corners."
Write-Host "[ ] Position lock disables move/resize and persists after restart."
Write-Host "[ ] Move/resize, close, and reopen restores placement."
Write-Host "[ ] First launch without official or local OAuth JSON shows mock mode and mock calendar data."
Write-Host "[ ] Settings shows missing client, official client, local fallback, and connected Google auth states as applicable."
Write-Host "[ ] Connect uses official OAuth JSON when packaged, and local Desktop OAuth JSON only as developer fallback."
Write-Host "[ ] Create/edit/delete failures show user-readable errors and do not crash."
Write-Host "[ ] Disconnect deletes the token store and returns to mock fallback after refresh/restart."
Write-Host "[ ] Settings owns Google sign-in, layer visibility/color, display, and startup controls."
Write-Host "[ ] Tray menu exposes Show/Hide, Settings, Refresh, and Exit."
Write-Host "[ ] UI is legible at 100%, 125%, and 150% display scaling."
Write-Host "[ ] Release ZIP contains no unapproved OAuth JSON, token store, logs, or private calendar data."
