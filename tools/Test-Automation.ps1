[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

Write-Host "=== PowerShell syntax ===" -ForegroundColor Cyan
& (Join-Path $PSScriptRoot "Test-PowerShellSyntax.ps1") | Out-Null

Write-Host ""
Write-Host "=== MSBuild version target ===" -ForegroundColor Cyan
& (Join-Path $PSScriptRoot "Test-VersionTarget.ps1")

Write-Host ""
Write-Host "=== Versioning preflight ===" -ForegroundColor Cyan
& (Join-Path $PSScriptRoot "Test-VersioningPreflight.ps1")

Write-Host ""
Write-Host "Automation preflight passed." -ForegroundColor Green
