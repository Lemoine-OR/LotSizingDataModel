[CmdletBinding()]
param(
    [string]$OutputDirectory
)

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

& (Join-Path $RepoRoot "tools\Test-PowerShellSyntax.ps1") -Quiet | Out-Null

$VersionInfo =
    & (Join-Path $RepoRoot "tools\Get-LotSizingVersion.ps1") -SkipRestore

$publicReleaseText = [string]$VersionInfo.PublicRelease
$publicRelease = $false

if (-not [bool]::TryParse($publicReleaseText, [ref]$publicRelease)) {
    throw (
        "NBGV returned an invalid PublicRelease value: '{0}'." -f
        $publicReleaseText
    )
}

if (-not $publicRelease) {
    throw (
        "Release assets must be built with PublicRelease=true. " +
        "The GitHub release workflow sets this automatically."
    )
}

$buildVersion = [string]$VersionInfo.BuildVersionSimple
$releaseVersion = [string]$VersionInfo.PackageVersion
$commitId = [string]$VersionInfo.GitCommitId
$commitShort = [string]$VersionInfo.GitCommitIdShort
$fileVersion = [string]$VersionInfo.AssemblyFileVersion

if ([string]::IsNullOrWhiteSpace($buildVersion)) {
    throw "NBGV BuildVersionSimple is empty."
}

if ([string]::IsNullOrWhiteSpace($releaseVersion)) {
    throw "NBGV PackageVersion is empty."
}

if ($releaseVersion -match '-g[0-9a-fA-F]{6,}$') {
    throw (
        "The computed public package version still contains a Git commit suffix: " +
        $releaseVersion
    )
}

$tag = "v$releaseVersion"
$isPrerelease = $releaseVersion.Contains("-")

$artifactRoot = Join-Path $RepoRoot "Documentation\artifacts"
$siteRoot = Join-Path $RepoRoot "Documentation\site"
$validatedRoot = Join-Path $artifactRoot "validated"

$sourceValidatedZip =
    Join-Path $artifactRoot "LotSizingDataModel-$buildVersion-validated.zip"

$sourceBuildInfo = Join-Path $validatedRoot "build-info.json"
$sourceSolverManifest = Join-Path $artifactRoot "solver-adapters.json"

foreach ($requiredPath in @(
    $sourceValidatedZip,
    $sourceBuildInfo,
    $sourceSolverManifest,
    (Join-Path $siteRoot "index.html")
)) {
    if (-not (Test-Path -LiteralPath $requiredPath -PathType Leaf)) {
        throw "Required release input is missing: $requiredPath"
    }
}

$buildInfo =
    Get-Content -LiteralPath $sourceBuildInfo -Raw |
    ConvertFrom-Json

if ([string]$buildInfo.buildVersionSimple -ne $buildVersion) {
    throw (
        "Validated package build version '{0}' does not match NBGV '{1}'." -f
        [string]$buildInfo.buildVersionSimple,
        $buildVersion
    )
}

if ([string]$buildInfo.packageVersion -ne $releaseVersion) {
    throw (
        "Validated package version '{0}' does not match release version '{1}'." -f
        [string]$buildInfo.packageVersion,
        $releaseVersion
    )
}

if ([string]$buildInfo.gitCommitId -ne $commitId) {
    throw (
        "Validated package commit '{0}' does not match NBGV commit '{1}'." -f
        [string]$buildInfo.gitCommitId,
        $commitId
    )
}

$buildInfoPublicRelease = $false
if (-not [bool]::TryParse(
    [string]$buildInfo.publicRelease,
    [ref]$buildInfoPublicRelease
)) {
    throw "Validated build-info.json contains an invalid publicRelease value."
}

if (-not $buildInfoPublicRelease) {
    throw "Validated package was not produced as a public release build."
}

# Every packaged first-party DLL must carry exactly the NBGV file version.
$packagedDlls =
    Get-ChildItem -LiteralPath (Join-Path $validatedRoot "bin") `
        -File `
        -Filter "LotSizingDataModel*.dll" |
    Sort-Object Name

if ($packagedDlls.Count -eq 0) {
    throw "No packaged LotSizingDataModel DLL was found."
}

$versionErrors = @()

foreach ($dll in $packagedDlls) {
    $fileInfo =
        [System.Diagnostics.FileVersionInfo]::GetVersionInfo($dll.FullName)

    if ([string]$fileInfo.FileVersion -ne $fileVersion) {
        $versionErrors += (
            "{0}: FileVersion '{1}' != expected '{2}'." -f
            $dll.Name,
            [string]$fileInfo.FileVersion,
            $fileVersion
        )
    }
}

if ($versionErrors.Count -gt 0) {
    $versionErrors | ForEach-Object {
        Write-Host $_ -ForegroundColor Red
    }

    throw (
        "Release DLL version consistency failed with {0} error(s)." -f
        $versionErrors.Count
    )
}

# The generated portal must visibly identify this build version and commit.
$portal =
    Get-Content -LiteralPath (Join-Path $siteRoot "index.html") -Raw

if ($portal.IndexOf(
    $buildVersion,
    [StringComparison]::OrdinalIgnoreCase
) -lt 0) {
    throw (
        "Documentation portal does not contain build version '{0}'." -f
        $buildVersion
    )
}

if ($portal.IndexOf(
    $commitShort,
    [StringComparison]::OrdinalIgnoreCase
) -lt 0) {
    throw (
        "Documentation portal does not contain commit '{0}'." -f
        $commitShort
    )
}

if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $RepoRoot "Documentation\release"
}

Remove-Item -LiteralPath $OutputDirectory `
    -Recurse `
    -Force `
    -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Path $OutputDirectory -Force |
    Out-Null

$validatedAssetName =
    "LotSizingDataModel-$releaseVersion-validated.zip"

$validatedAsset =
    Join-Path $OutputDirectory $validatedAssetName

Copy-Item -LiteralPath $sourceValidatedZip `
    -Destination $validatedAsset `
    -Force

$documentationAssetName =
    "LotSizingDataModel-$releaseVersion-documentation.zip"

$documentationAsset =
    Join-Path $OutputDirectory $documentationAssetName

Add-Type -AssemblyName System.IO.Compression.FileSystem

if (Test-Path -LiteralPath $documentationAsset) {
    Remove-Item -LiteralPath $documentationAsset -Force
}

[System.IO.Compression.ZipFile]::CreateFromDirectory(
    $siteRoot,
    $documentationAsset,
    [System.IO.Compression.CompressionLevel]::Optimal,
    $false
)

$buildInfoAssetName =
    "LotSizingDataModel-$releaseVersion-build-info.json"

$buildInfoAsset =
    Join-Path $OutputDirectory $buildInfoAssetName

Copy-Item -LiteralPath $sourceBuildInfo `
    -Destination $buildInfoAsset `
    -Force

$solverAssetName =
    "LotSizingDataModel-$releaseVersion-solver-adapters.json"

$solverAsset =
    Join-Path $OutputDirectory $solverAssetName

Copy-Item -LiteralPath $sourceSolverManifest `
    -Destination $solverAsset `
    -Force

$primaryAssets = @(
    $validatedAsset,
    $documentationAsset,
    $buildInfoAsset,
    $solverAsset
)

$assetRecords = @()

foreach ($asset in $primaryAssets) {
    $file = Get-Item -LiteralPath $asset
    $hash =
        (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).
        Hash.
        ToLowerInvariant()

    $assetRecords += [ordered]@{
        name = $file.Name
        size = $file.Length
        sha256 = $hash
    }
}

$solverManifest =
    Get-Content -LiteralPath $sourceSolverManifest -Raw |
    ConvertFrom-Json

$releaseManifest = [ordered]@{
    schemaVersion = 1
    product = "LotSizingDataModel"
    author = "David Lemoine"
    organization = "Lemoine-OR"
    repository = "https://github.com/Lemoine-OR/LotSizingDataModel"
    releaseVersion = $releaseVersion
    buildVersion = $buildVersion
    tag = $tag
    prerelease = $isPrerelease
    publicRelease = $publicRelease
    assemblyVersion = [string]$VersionInfo.AssemblyVersion
    assemblyFileVersion = $fileVersion
    informationalVersion = [string]$VersionInfo.AssemblyInformationalVersion
    gitCommitId = $commitId
    gitCommitIdShort = $commitShort
    gitVersionHeight = [string]$VersionInfo.GitVersionHeight
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
    packagedDlls = @($packagedDlls | ForEach-Object { $_.Name })
    solverAdapters = @($solverManifest.adapters)
    assets = $assetRecords
}

$manifestName =
    "LotSizingDataModel-$releaseVersion-release-manifest.json"

$manifestPath =
    Join-Path $OutputDirectory $manifestName

$releaseManifest |
    ConvertTo-Json -Depth 10 |
    Set-Content -LiteralPath $manifestPath -Encoding UTF8

$checksumFiles =
    Get-ChildItem -LiteralPath $OutputDirectory -File |
    Sort-Object Name

$checksumLines = @()

foreach ($file in $checksumFiles) {
    $hash =
        (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).
        Hash.
        ToLowerInvariant()

    $checksumLines += ("{0}  {1}" -f $hash, $file.Name)
}

$checksumPath =
    Join-Path $OutputDirectory "SHA256SUMS.txt"

Set-Content -LiteralPath $checksumPath `
    -Value $checksumLines `
    -Encoding ASCII

& (Join-Path $RepoRoot "tools\Test-ReleaseArtifacts.ps1") `
    -ReleaseDirectory $OutputDirectory |
    Out-Null

Write-Host ""
Write-Host "Release assets prepared and validated." -ForegroundColor Green
Write-Host "Release version : $releaseVersion"
Write-Host "Build version   : $buildVersion"
Write-Host "Tag             : $tag"
Write-Host "Commit          : $commitShort"
Write-Host "Prerelease      : $isPrerelease"
Write-Host "Directory       : $OutputDirectory"

[pscustomobject]@{
    ReleaseVersion = $releaseVersion
    BuildVersion = $buildVersion
    Tag = $tag
    CommitId = $commitId
    CommitIdShort = $commitShort
    Prerelease = $isPrerelease
    ReleaseDirectory = $OutputDirectory
}
