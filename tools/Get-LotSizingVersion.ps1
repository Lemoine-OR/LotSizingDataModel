[CmdletBinding()]
param(
    [switch]$SkipRestore
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$CoreProject = Join-Path $RepoRoot "LotSizingDataModel.Core\LotSizingDataModel.Core.csproj"

if (-not (Test-Path -LiteralPath $CoreProject)) {
    throw "Version probe project not found: $CoreProject"
}

& (Join-Path $PSScriptRoot "Test-VersioningPreflight.ps1")

if (-not $SkipRestore) {
    Write-Host "Restoring version probe project..."
    & dotnet restore $CoreProject | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet restore failed for the version probe project."
    }
}

$tempRoot = Join-Path $RepoRoot "Documentation\versioning"
New-Item -ItemType Directory -Force $tempRoot | Out-Null

$tempFile = Join-Path $tempRoot "version-info.txt"
Remove-Item -LiteralPath $tempFile -Force -ErrorAction SilentlyContinue

$msbuildArgs = @(
    "msbuild",
    $CoreProject,
    "/t:WriteLotSizingDataModelVersion",
    "/p:LotSizingVersionOutput=$tempFile",
    "/nologo",
    "/v:q"
)

& dotnet @msbuildArgs | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "Nerdbank.GitVersioning version extraction failed."
}

if (-not (Test-Path -LiteralPath $tempFile)) {
    throw "Version information file was not generated: $tempFile"
}

$values = @{}
Get-Content -LiteralPath $tempFile | ForEach-Object {
    $line = $_
    $separator = $line.IndexOf("=")
    if ($separator -gt 0) {
        $key = $line.Substring(0, $separator)
        $value = $line.Substring($separator + 1)
        $values[$key] = $value
    }
}

$required = @(
    "AssemblyVersion",
    "AssemblyFileVersion",
    "AssemblyInformationalVersion",
    "BuildVersion",
    "BuildVersionSimple",
    "GitCommitId",
    "GitCommitIdShort",
    "PackageVersion"
)

foreach ($key in $required) {
    if (-not $values.ContainsKey($key) -or [string]::IsNullOrWhiteSpace($values[$key])) {
        throw "Missing NBGV property '$key' in generated version information."
    }
}

[pscustomobject]$values
