[CmdletBinding()]
param()

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
$VersionJsonPath = Join-Path $RepoRoot 'version.json'

if (-not (Test-Path -LiteralPath $VersionJsonPath -PathType Leaf)) {
    throw "version.json not found: $VersionJsonPath"
}

$declaredObject =
    Get-Content -LiteralPath $VersionJsonPath -Raw |
    ConvertFrom-Json

$declared = [string]$declaredObject.version

if ([string]::IsNullOrWhiteSpace($declared)) {
    throw 'version.json does not contain a non-empty version.'
}

# This validation gate runs before the main build restore.
# A fresh worktree has no restored NBGV/MSBuild assets yet, so this call
# MUST perform its own restore. Do not add -SkipRestore here.
$versionInfo =
    & (Join-Path $PSScriptRoot 'Get-LotSizingVersion.ps1')

$display = [string]$versionInfo.DisplayVersion
$numeric = [string]$versionInfo.BuildVersionSimple
$prerelease = [string]$versionInfo.PrereleaseVersion
$package = [string]$versionInfo.PackageVersion

$declaredParts = @($declared -split '-', 2)
$declaredNumeric = [string]$declaredParts[0]

$declaredPrerelease = ''
if (@($declaredParts).Count -eq 2) {
    $declaredPrerelease = '-' + [string]$declaredParts[1]
}

if ($numeric -ne $declaredNumeric) {
    throw (
        "Numeric version mismatch. version.json requires '$declaredNumeric' " +
        "but NBGV BuildVersionSimple is '$numeric'."
    )
}

if ($prerelease -ne $declaredPrerelease) {
    throw (
        "Prerelease version mismatch. version.json requires " +
        "'$declaredPrerelease' but NBGV PrereleaseVersion is '$prerelease'."
    )
}

if ($display -ne $declared) {
    throw (
        "Display version mismatch. version.json declares '$declared' " +
        "but NBGV-derived DisplayVersion is '$display'."
    )
}

if ([string]::IsNullOrWhiteSpace($package)) {
    throw 'NBGV PackageVersion is empty.'
}

Write-Host (
    "Version identity validation passed: " +
    "declared=$declared; numeric=$numeric; prerelease=$prerelease; " +
    "package=$package"
) -ForegroundColor Green
