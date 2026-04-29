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
Write-Host "Canonical v0.6.0 checklist: docs/QA_CHECKLIST_v0.6.0.md"
Write-Host "[ ] Borderless window opens with custom chrome only."
Write-Host "[ ] Dragging the title area moves the window; double-click toggles maximize/restore."
Write-Host "[ ] Resize works on all edges and corners."
Write-Host "[ ] Position lock disables move/resize and persists after restart."
Write-Host "[ ] Move/resize, close, and reopen restores placement."
Write-Host "[ ] Month grid and selected-day agenda show mock data without Google credentials."
Write-Host "[ ] Settings owns Google sign-in, layer visibility/color, display, and startup controls."
Write-Host "[ ] Tray menu exposes Show/Hide, Settings, Refresh, and Exit."
Write-Host "[ ] UI is legible at 100%, 125%, and 150% display scaling."
