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

Require-Command "git"
Require-Command "dotnet"

$versionFile = Join-Path $RepoRoot "version.json"
if (-not (Test-Path -LiteralPath $versionFile)) {
    throw "Missing version.json at repository root."
}

# The version file must exist in HEAD and must not contain uncommitted edits.
& git cat-file -e "HEAD:version.json" 2>$null
if ($LASTEXITCODE -ne 0) {
    throw "version.json is not committed in HEAD. Commit the automation pack before the first versioned build."
}

& git diff --quiet HEAD -- version.json
if ($LASTEXITCODE -ne 0) {
    throw "version.json has uncommitted changes. Commit it before building so the computed DLL version is reproducible."
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
$legacyHits = New-Object System.Collections.Generic.List[string]

Get-ChildItem -LiteralPath $RepoRoot -Recurse -File -Filter "*.cs" |
    Where-Object {
        $_.FullName -notmatch '[\\/](bin|obj|Documentation|\.git)[\\/]'
    } |
    ForEach-Object {
        $match = Select-String -LiteralPath $_.FullName -Pattern $assemblyAttributePattern -AllMatches
        if ($null -ne $match) {
            $legacyHits.Add($_.FullName)
        }
    }

if ($legacyHits.Count -gt 0) {
    $paths = ($legacyHits | Sort-Object -Unique) -join [Environment]::NewLine
    throw "Legacy explicit assembly version attribute(s) detected. They would conflict with automatic Git versioning:`n$paths"
}

Write-Host "Versioning preflight passed." -ForegroundColor Green
