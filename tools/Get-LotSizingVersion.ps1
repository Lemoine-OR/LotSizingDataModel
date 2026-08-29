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

# Create the output directory here. Directory.Build.targets only writes the file.
$tempRoot = Join-Path $RepoRoot "Documentation\versioning"
New-Item -ItemType Directory -Path $tempRoot -Force | Out-Null

$tempFile = Join-Path $tempRoot "version-info.txt"
Remove-Item -LiteralPath $tempFile -Force -ErrorAction SilentlyContinue

$msbuildArgs = @(
    "msbuild",
    $CoreProject,
    "/t:WriteLotSizingDataModelVersion",
    "/p:LotSizingVersionOutput=$tempFile",
    "/nologo",
    "/v:minimal"
)

Write-Host "Extracting version with Nerdbank.GitVersioning..."

& dotnet @msbuildArgs | Out-Host
if ($LASTEXITCODE -ne 0) {
    throw "Nerdbank.GitVersioning version extraction failed while executing target 'WriteLotSizingDataModelVersion' for '$CoreProject'."
}

if (-not (Test-Path -LiteralPath $tempFile -PathType Leaf)) {
    throw "Version information file was not generated: $tempFile"
}

$values = @{}

Get-Content -LiteralPath $tempFile | ForEach-Object {
    $line = $_
    $separator = $line.IndexOf("=")

    if ($separator -gt 0) {
        $key = $line.Substring(0, $separator).Trim()
        $value = $line.Substring($separator + 1).Trim()
        $values[$key] = $value
    }
}

$requiredNonEmpty = @(
    "AssemblyVersion",
    "AssemblyFileVersion",
    "AssemblyInformationalVersion",
    "BuildVersion",
    "BuildVersionSimple",
    "GitCommitId",
    "GitCommitIdShort",
    "PackageVersion"
)

foreach ($key in $requiredNonEmpty) {
    if (-not $values.ContainsKey($key) -or
        [string]::IsNullOrWhiteSpace([string]$values[$key])) {
        throw "Missing or empty NBGV property '$key' in generated version information file '$tempFile'."
    }
}

# PrereleaseVersion is required as a property but may legitimately be empty
# for stable releases.
if (-not $values.ContainsKey("PrereleaseVersion")) {
    throw "Missing NBGV property 'PrereleaseVersion' in generated version information file '$tempFile'."
}

$displayVersion = [string]$values["BuildVersionSimple"]
$prereleaseVersion = [string]$values["PrereleaseVersion"]

if (-not [string]::IsNullOrWhiteSpace($prereleaseVersion)) {
    if (-not $prereleaseVersion.StartsWith("-", [System.StringComparison]::Ordinal)) {
        throw "NBGV PrereleaseVersion must include its leading hyphen. Received '$prereleaseVersion'."
    }

    $displayVersion += $prereleaseVersion
}

$values["DisplayVersion"] = $displayVersion

[pscustomobject]$values
