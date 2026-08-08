[CmdletBinding()]
param(
    [string]$Version = "1.17.0",
    [string]$ExpectedSha256 = "94594407c4cbca3049d76aacbb05d4a6f7d0f4e93c0de410b825d25ca5621c83",
    [string]$DestinationRoot
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrWhiteSpace($DestinationRoot)) {
    if (-not [string]::IsNullOrWhiteSpace($env:RUNNER_TEMP)) {
        $DestinationRoot = Join-Path $env:RUNNER_TEMP "LotSizingDataModel-Doxygen"
    }
    else {
        $DestinationRoot =
            Join-Path ([System.IO.Path]::GetTempPath()) "LotSizingDataModel-Doxygen"
    }
}

$releaseTag = "Release_" + $Version.Replace(".", "_")
$expectedAssetName = "doxygen-$Version.windows.x64.bin.zip"
$expected = $ExpectedSha256.ToLowerInvariant()

$downloadRoot = Join-Path $DestinationRoot "download"
$installRoot = Join-Path $DestinationRoot "doxygen-$Version"

Remove-Item -LiteralPath $downloadRoot -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $installRoot -Recurse -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Path $downloadRoot -Force | Out-Null
New-Item -ItemType Directory -Path $installRoot -Force | Out-Null

$zipPath = Join-Path $downloadRoot $expectedAssetName

$headers = @{
    "Accept" = "application/vnd.github+json"
    "User-Agent" = "LotSizingDataModel-CI"
}

$githubApi =
    "https://api.github.com/repos/doxygen/doxygen/releases/tags/$releaseTag"

Write-Host "Resolving official Doxygen release from GitHub..."
Write-Host "  Release: $releaseTag"
Write-Host "  Expected SHA-256: $expected"

$downloaded = $false
$downloadSource = $null
$githubFailure = $null

try {
    $release =
        Invoke-RestMethod `
            -Uri $githubApi `
            -Headers $headers `
            -Method Get

    $asset =
        @(
            $release.assets |
            Where-Object {
                [string]$_.name -eq $expectedAssetName
            }
        ) |
        Select-Object -First 1

    if ($null -eq $asset) {
        $candidateAssets =
            @(
                $release.assets |
                Where-Object {
                    [string]$_.name -match
                        ('^doxygen-' +
                         [regex]::Escape($Version) +
                         '.*windows.*x64.*\.zip$')
                }
            )

        if ($candidateAssets.Count -eq 1) {
            $asset = $candidateAssets[0]
        }
    }

    if ($null -eq $asset) {
        $available =
            @($release.assets | ForEach-Object { [string]$_.name }) -join ", "

        throw (
            "Official GitHub release does not expose the expected Windows x64 ZIP. " +
            "Available assets: $available"
        )
    }

    if (
        $null -ne $asset.PSObject.Properties["digest"] -and
        -not [string]::IsNullOrWhiteSpace([string]$asset.digest)
    ) {
        $githubDigest = ([string]$asset.digest).ToLowerInvariant()
        $expectedDigest = "sha256:$expected"

        if ($githubDigest -ne $expectedDigest) {
            throw (
                "GitHub release metadata digest mismatch for '$($asset.name)'. " +
                "Expected '$expectedDigest', GitHub reports '$githubDigest'."
            )
        }

        Write-Host "  GitHub asset digest matches the pinned SHA-256."
    }

    Write-Host "Downloading official GitHub release asset:"
    Write-Host "  $($asset.browser_download_url)"

    Invoke-WebRequest `
        -Uri ([string]$asset.browser_download_url) `
        -Headers @{ "User-Agent" = "LotSizingDataModel-CI" } `
        -OutFile $zipPath

    $downloaded = $true
    $downloadSource = "GitHub official doxygen/doxygen release"
}
catch {
    $githubFailure = $_.Exception.Message
    Write-Warning (
        "GitHub release download failed: {0}" -f $githubFailure
    )
}

# Fallback to the official Doxygen website only if the official GitHub
# release could not be retrieved. The same pinned checksum is mandatory.
if (-not $downloaded) {
    $fallbackUrl =
        "https://www.doxygen.nl/files/$expectedAssetName"

    Write-Host "Trying official doxygen.nl fallback:"
    Write-Host "  $fallbackUrl"

    Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue

    Invoke-WebRequest `
        -Uri $fallbackUrl `
        -OutFile $zipPath

    $downloaded = $true
    $downloadSource = "doxygen.nl official download"
}

if (
    -not $downloaded -or
    -not (Test-Path -LiteralPath $zipPath -PathType Leaf)
) {
    throw "Doxygen archive was not downloaded."
}

$file = Get-Item -LiteralPath $zipPath

# A valid 1.17.0 Windows archive is tens of MB. This catches HTML/error bodies
# before even considering their checksum.
if ($file.Length -lt 1000000) {
    throw (
        "Downloaded Doxygen file is unexpectedly small ({0} bytes). " +
        "It is probably an HTML/error response rather than the release archive." -f
        $file.Length
    )
}

# ZIP local-file header signature: PK 03 04.
$stream = [System.IO.File]::OpenRead($zipPath)

try {
    $signature = New-Object byte[] 4
    $read = $stream.Read($signature, 0, 4)
}
finally {
    $stream.Dispose()
}

if (
    $read -ne 4 -or
    $signature[0] -ne 0x50 -or
    $signature[1] -ne 0x4B -or
    $signature[2] -ne 0x03 -or
    $signature[3] -ne 0x04
) {
    throw (
        "Downloaded Doxygen file is not a ZIP archive. " +
        "Source: $downloadSource"
    )
}

$actual =
    (Get-FileHash -LiteralPath $zipPath -Algorithm SHA256).
    Hash.
    ToLowerInvariant()

Write-Host "  Download source : $downloadSource"
Write-Host "  Download size   : $($file.Length) bytes"
Write-Host "  Actual SHA-256  : $actual"

if ($actual -ne $expected) {
    Remove-Item -LiteralPath $zipPath -Force -ErrorAction SilentlyContinue

    throw (
        "Doxygen SHA-256 mismatch. Expected the checksum published by Doxygen " +
        "'$expected', got '$actual'. The downloaded file is rejected. " +
        "Do NOT update the pinned checksum to the observed value unless the " +
        "official Doxygen checksum publication itself changes."
    )
}

Write-Host "Doxygen archive checksum verified." -ForegroundColor Green

Expand-Archive `
    -Path $zipPath `
    -DestinationPath $installRoot `
    -Force

$exe =
    Get-ChildItem `
        -LiteralPath $installRoot `
        -Recurse `
        -File `
        -Filter "doxygen.exe" |
    Select-Object -First 1

if ($null -eq $exe) {
    throw "doxygen.exe was not found in the verified official archive."
}

$reportedVersion = (& $exe.FullName --version).Trim()

# Official Doxygen binaries may append a build/commit identifier, for example:
#   1.17.0 (65a43c0aba45cc23b3ca11b6b5334d4eea931726)
#
# Validate the semantic version at the beginning of the output, but do not
# require the complete output string to equal the bare version number.
$escapedVersion = [regex]::Escape($Version)
$versionPattern = "^$escapedVersion(?:$|\s|\()"

if ($reportedVersion -notmatch $versionPattern) {
    throw (
        "Verified archive contains Doxygen version output '$reportedVersion', " +
        "which does not identify expected version '$Version'."
    )
}

if (-not [string]::IsNullOrWhiteSpace($env:GITHUB_PATH)) {
    Add-Content `
        -Path $env:GITHUB_PATH `
        -Value $exe.Directory.FullName
}

Write-Host (
    "Verified Doxygen {0} installed from {1}." -f
    $reportedVersion,
    $downloadSource
) -ForegroundColor Green

[pscustomobject]@{
    Version = $reportedVersion
    Executable = $exe.FullName
    Directory = $exe.Directory.FullName
    Source = $downloadSource
    Sha256 = $actual
    ArchiveSize = $file.Length
}
