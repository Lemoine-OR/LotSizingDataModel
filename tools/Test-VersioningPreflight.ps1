[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found on PATH."
    }
}

Require-Command "dotnet"

$versionFile = Join-Path $RepoRoot "version.json"
if (-not (Test-Path -LiteralPath $versionFile)) {
    throw "Missing version.json at repository root."
}

# Nerdbank.GitVersioning requires a real Git checkout (the .git metadata),
# but it does NOT require git.exe to be available from this PowerShell session.
$gitMetadata = Join-Path $RepoRoot ".git"
if (-not (Test-Path -LiteralPath $gitMetadata)) {
    throw "The repository Git metadata (.git) was not found. Nerdbank.GitVersioning requires a real Git checkout, not a source ZIP."
}

# Detect nested NBGV version files, which would create multiple version domains.
$nestedVersions = Get-ChildItem -LiteralPath $RepoRoot -Recurse -File -Filter "version.json" |
    Where-Object {
        $_.FullName -ne $versionFile -and
        $_.FullName -notmatch '[\\/](bin|obj|Documentation|\.git)[\\/]'
    }

if ($nestedVersions.Count -gt 0) {
    $paths = ($nestedVersions | ForEach-Object { $_.FullName }) -join [Environment]::NewLine
    throw "Nested version.json file(s) detected. LotSizingDataModel uses one repository-wide version domain:`n$paths"
}

# SDK-style projects should not carry hand-authored version assembly attributes.
$assemblyAttributePattern = '\[assembly\s*:\s*(?:System\.Reflection\.)?(?:AssemblyVersion|AssemblyFileVersion|AssemblyInformationalVersion)\s*\('
$legacyHits = @()

Get-ChildItem -LiteralPath $RepoRoot -Recurse -File -Filter "*.cs" |
    Where-Object {
        $_.FullName -notmatch '[\\/](bin|obj|Documentation|\.git)[\\/]'
    } |
    ForEach-Object {
        $match = Select-String -LiteralPath $_.FullName -Pattern $assemblyAttributePattern -AllMatches
        if ($null -ne $match) {
            $legacyHits += $_.FullName
        }
    }

if ($legacyHits.Count -gt 0) {
    $paths = ($legacyHits | Sort-Object -Unique) -join [Environment]::NewLine
    throw "Legacy explicit assembly version attribute(s) detected. They would conflict with automatic Git versioning:`n$paths"
}

Write-Host "Versioning preflight passed." -ForegroundColor Green
Write-Host "Git metadata detected; no git.exe PATH dependency is required." -ForegroundColor DarkGray
