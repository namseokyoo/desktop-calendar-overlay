# Windows validation helper for Desktop Calendar Overlay.
# Run from the repository root on Windows with the .NET 8 SDK installed.

$ErrorActionPreference = "Stop"

$Project = Join-Path $PSScriptRoot "..\src\DesktopCalendarOverlay\DesktopCalendarOverlay.csproj"

Write-Host "== dotnet info =="
dotnet --info

Write-Host "== restore =="
dotnet restore $Project

Write-Host "== build =="
dotnet build $Project -c Debug --no-restore

Write-Host "== manual visual validation =="
Write-Host "Run the app with: dotnet run --project $Project"
Write-Host "Check borderless resize, drag, always-on-top toggle, placement persistence, and DPI scaling."
