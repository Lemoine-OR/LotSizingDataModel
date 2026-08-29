[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [string]$SolverManifestPath
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

if ([string]::IsNullOrWhiteSpace($SolverManifestPath)) {
    $SolverManifestPath = Join-Path $RepoRoot "Documentation\artifacts\solver-adapters.json"
}

$VersionInfo = & (Join-Path $RepoRoot "tools\Get-LotSizingVersion.ps1") -SkipRestore

$ArtifactRoot = Join-Path $RepoRoot "Documentation\artifacts"
$PackageRoot = Join-Path $ArtifactRoot "validated"
$PackageBin = Join-Path $PackageRoot "bin"

Remove-Item -LiteralPath $PackageRoot -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $PackageBin | Out-Null

function Get-ConfiguredAssemblyName([string]$ProjectFile, [string]$Fallback) {
    if (-not (Test-Path -LiteralPath $ProjectFile)) {
        return $Fallback
    }

    try {
        [xml]$xml = Get-Content -LiteralPath $ProjectFile -Raw
        foreach ($group in $xml.Project.PropertyGroup) {
            if ($null -ne $group.AssemblyName -and
                -not [string]::IsNullOrWhiteSpace([string]$group.AssemblyName)) {
                return [string]$group.AssemblyName
            }
        }
    }
    catch {
    }

    return $Fallback
}

function Find-PrimaryAssembly([string]$ProjectName) {
    $projectDirectory = Join-Path $RepoRoot $ProjectName
    $projectFile = Join-Path $projectDirectory "$ProjectName.csproj"
    $assemblyName = Get-ConfiguredAssemblyName $projectFile $ProjectName
    $releaseRoot = Join-Path $projectDirectory "bin\$Configuration"

    if (-not (Test-Path -LiteralPath $releaseRoot)) {
        throw "Release output directory not found for '$ProjectName': $releaseRoot"
    }

    $matches = Get-ChildItem -LiteralPath $releaseRoot -Recurse -File -Filter "$assemblyName.dll" |
        Sort-Object @{Expression = { $_.FullName.Length }; Ascending = $true},
                    @{Expression = { $_.LastWriteTimeUtc }; Ascending = $false}

    if ($matches.Count -eq 0 -and $assemblyName -ne $ProjectName) {
        $matches = Get-ChildItem -LiteralPath $releaseRoot -Recurse -File -Filter "$ProjectName.dll" |
            Sort-Object @{Expression = { $_.FullName.Length }; Ascending = $true},
                        @{Expression = { $_.LastWriteTimeUtc }; Ascending = $false}
    }

    if ($matches.Count -eq 0) {
        throw "Primary DLL not found for '$ProjectName'."
    }

    return $matches[0].FullName
}

function Copy-AssemblySet([string]$AssemblyPath) {
    $sourceDirectory = Split-Path -Parent $AssemblyPath
    $baseName = [IO.Path]::GetFileNameWithoutExtension($AssemblyPath)

    foreach ($extension in @(".dll", ".xml", ".pdb")) {
        $source = Join-Path $sourceDirectory "$baseName$extension"
        if (Test-Path -LiteralPath $source) {
            Copy-Item -LiteralPath $source -Destination $PackageBin -Force
        }
    }
}

$CoreLibraries = @(
    "LotSizingDataModel.Core",
    "LotSizingDataModel.Solution",
    "LotSizingDataModel.Instance",
    "LotSizingDataModel.Import",
    "LotSizingDataModel.Solver",
    "LotSizingDataModel.Checker"
)

$firstPartyDlls = @()

foreach ($project in $CoreLibraries) {
    $assembly = Find-PrimaryAssembly $project
    Copy-AssemblySet $assembly
    $firstPartyDlls += (Join-Path $PackageBin ([IO.Path]::GetFileName($assembly)))
}

if (Test-Path -LiteralPath $SolverManifestPath) {
    $manifest = Get-Content -LiteralPath $SolverManifestPath -Raw | ConvertFrom-Json
    foreach ($adapter in $manifest.adapters) {
        if ($adapter.status -eq "built" -and
            -not [string]::IsNullOrWhiteSpace([string]$adapter.assembly) -and
            (Test-Path -LiteralPath ([string]$adapter.assembly))) {
            Copy-AssemblySet ([string]$adapter.assembly)
            $firstPartyDlls += (Join-Path $PackageBin ([IO.Path]::GetFileName([string]$adapter.assembly)))
        }
    }
}

# Branded icon shipped with the binary package.
Copy-Item -LiteralPath (Join-Path $RepoRoot "docs\assets\dll-icon.ico") `
          -Destination (Join-Path $PackageRoot "LotSizingDataModel.ico") -Force

$buildInfo = [ordered]@{
    product = "LotSizingDataModel"
    author = "David Lemoine"
    organization = "Lemoine-OR"
    repository = "https://github.com/Lemoine-OR/LotSizingDataModel"
    buildVersion = $VersionInfo.BuildVersion
    buildVersionSimple = $VersionInfo.BuildVersionSimple
    displayVersion = $VersionInfo.DisplayVersion
    assemblyVersion = $VersionInfo.AssemblyVersion
    assemblyFileVersion = $VersionInfo.AssemblyFileVersion
    informationalVersion = $VersionInfo.AssemblyInformationalVersion
    packageVersion = $VersionInfo.PackageVersion
    gitCommitId = $VersionInfo.GitCommitId
    gitCommitIdShort = $VersionInfo.GitCommitIdShort
    gitVersionHeight = $VersionInfo.GitVersionHeight
    publicRelease = $VersionInfo.PublicRelease
}

$buildInfo | ConvertTo-Json -Depth 5 |
    Set-Content -LiteralPath (Join-Path $PackageRoot "build-info.json") -Encoding UTF8

$readme = @"
LotSizingDataModel $($VersionInfo.DisplayVersion)

Author: David Lemoine
Organization: Lemoine-OR
Repository: https://github.com/Lemoine-OR/LotSizingDataModel
Commit: $($VersionInfo.GitCommitId)

This package contains the validated first-party LotSizingDataModel libraries
and any solver adapters that were successfully built by the automated adapter pipeline.
Proprietary solver runtimes/SDK binaries are never bundled automatically.
"@
Set-Content -LiteralPath (Join-Path $PackageRoot "README.txt") -Value $readme -Encoding UTF8

# Validate every packaged first-party DLL before producing the ZIP.
& (Join-Path $RepoRoot "tools\Verify-AssemblyMetadata.ps1") -DllPath $firstPartyDlls

$zipName = "LotSizingDataModel-$($VersionInfo.DisplayVersion)-validated.zip"
$zipPath = Join-Path $ArtifactRoot $zipName
Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue

Compress-Archive -Path (Join-Path $PackageRoot "*") -DestinationPath $zipPath -CompressionLevel Optimal

$hash = (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).Hash.ToLowerInvariant()
$checksumPath = "$zipPath.sha256"
Set-Content -LiteralPath $checksumPath -Value "$hash  $zipName" -Encoding ASCII

Write-Host ""
Write-Host "Validated package:" -ForegroundColor Green
Write-Host $zipPath
Write-Host "SHA-256: $hash"

[pscustomobject]@{
    PackageRoot = $PackageRoot
    ZipPath = $zipPath
    ChecksumPath = $checksumPath
    Version = $VersionInfo.DisplayVersion
}
