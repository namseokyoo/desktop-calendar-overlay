# Windows validation helper for Desktop Calendar Overlay.
# Run from the repository root on Windows with the .NET 8 SDK installed.

$ErrorActionPreference = "Stop"

$Project = Join-Path $PSScriptRoot "..\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj"

Write-Host "== host =="
Write-Host "OS: $([System.Environment]::OSVersion.VersionString)"

Write-Host "== dotnet info =="
dotnet --info

Write-Host "== project target sanity =="
Select-String -Path $Project -Pattern "<TargetFramework>net8.0-windows</TargetFramework>", "<UseWPF>true</UseWPF>" | ForEach-Object {
    Write-Host $_.Line.Trim()
}

Write-Host "== restore =="
dotnet restore $Project

Write-Host "== build =="
dotnet build $Project -c Debug --no-restore

Write-Host "== run command =="
Write-Host "dotnet run --project $Project"

Write-Host "== manual visual validation checklist =="
Write-Host "Canonical v0.8.0 checklist: docs/QA_CHECKLIST_v0.8.0.md"
Write-Host "[ ] Borderless window opens with custom chrome only."
Write-Host "[ ] Dragging the title area moves the window; double-click toggles maximize/restore."
Write-Host "[ ] Resize works on all edges and corners."
Write-Host "[ ] Position lock disables move/resize and persists after restart."
Write-Host "[ ] Move/resize, close, and reopen restores placement."
Write-Host "[ ] First launch without OAuth JSON shows mock mode and mock calendar data."
Write-Host "[ ] Settings shows mock, ready-to-connect, and connected Google auth states as applicable."
Write-Host "[ ] Connect works only with local Desktop OAuth JSON and an allowed test user."
Write-Host "[ ] Create/edit/delete failures show user-readable errors and do not crash."
Write-Host "[ ] Disconnect deletes the token store and returns to mock fallback after refresh/restart."
Write-Host "[ ] Settings owns Google sign-in, layer visibility/color, display, and startup controls."
Write-Host "[ ] Tray menu exposes Show/Hide, Settings, Refresh, and Exit."
Write-Host "[ ] UI is legible at 100%, 125%, and 150% display scaling."
Write-Host "[ ] Release ZIP contains no OAuth JSON, token store, logs, or private calendar data."
