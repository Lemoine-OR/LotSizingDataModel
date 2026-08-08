[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$targetFile = Join-Path $RepoRoot "Directory.Build.targets"

if (-not (Test-Path -LiteralPath $targetFile)) {
    throw "Missing Directory.Build.targets."
}

[xml]$xml = Get-Content -LiteralPath $targetFile -Raw

$target = $xml.Project.Target |
    Where-Object { $_.Name -eq "WriteLotSizingDataModelVersion" } |
    Select-Object -First 1

if ($null -eq $target) {
    throw "Target WriteLotSizingDataModelVersion was not found."
}

if ([string]$target.DependsOnTargets -ne "GetBuildVersion") {
    throw "Version bridge must depend on NBGV GetBuildVersion."
}

$raw = Get-Content -LiteralPath $targetFile -Raw

if ($raw -match '\$\(\[System\.IO\.Path\]') {
    throw "Directory.Build.targets unexpectedly contains a path property function."
}

if ($raw -match '<MakeDir\b') {
    throw "Directory.Build.targets unexpectedly contains MakeDir."
}

if ($raw -notmatch '<WriteLinesToFile\b') {
    throw "Directory.Build.targets does not contain WriteLinesToFile."
}

Write-Host "Version target audit passed." -ForegroundColor Green
