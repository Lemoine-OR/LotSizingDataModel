[CmdletBinding()]
param(
    [switch]$IncludeExternalSolverAdapters
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

# Parse every automation script before any long-running build step.
& (Join-Path $RepoRoot "tools\Test-PowerShellSyntax.ps1") -Quiet | Out-Null
Write-Host "PowerShell automation syntax: OK" -ForegroundColor Green

$validatedArgs = @{}
if ($IncludeExternalSolverAdapters) {
    $validatedArgs["IncludeExternalSolverAdapters"] = $true
}

& (Join-Path $RepoRoot "build\Build-Validated.ps1") @validatedArgs | Out-Null

Write-Host ""
Write-Host "=== Documentation ===" -ForegroundColor Cyan

$doxygen = Get-Command "doxygen" -ErrorAction SilentlyContinue
$dot = Get-Command "dot" -ErrorAction SilentlyContinue

if ($null -eq $doxygen) {
    throw "Doxygen is required for Build-All. Install Doxygen 1.17.0 or run build/Build-Validated.ps1 when documentation is not needed."
}
if ($null -eq $dot) {
    throw "Graphviz 'dot' is required for Build-All."
}

& (Join-Path $RepoRoot "docs\build-documentation.ps1")

Write-Host ""
Write-Host "Full build, tests, binary validation and documentation succeeded." -ForegroundColor Green
