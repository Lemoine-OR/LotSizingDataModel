[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$ReleaseDirectory
)

$ErrorActionPreference = "Stop"

$resolvedReleaseDirectory =
    (Resolve-Path -LiteralPath $ReleaseDirectory).Path

$manifest =
    Get-ChildItem -LiteralPath $resolvedReleaseDirectory `
        -File `
        -Filter "LotSizingDataModel-*-release-manifest.json"

if ($manifest.Count -ne 1) {
    throw (
        "Expected exactly one release manifest, found {0}." -f
        $manifest.Count
    )
}

$data =
    Get-Content -LiteralPath $manifest[0].FullName -Raw |
    ConvertFrom-Json

if ([string]$data.product -ne "LotSizingDataModel") {
    throw "Release manifest product is invalid."
}

if ([string]$data.author -ne "David Lemoine") {
    throw "Release manifest author is invalid."
}

if ([string]$data.organization -ne "Lemoine-OR") {
    throw "Release manifest organization is invalid."
}

if (-not [bool]$data.publicRelease) {
    throw "Release manifest is not marked as a public release."
}

$expectedTag = "v$([string]$data.releaseVersion)"

if ([string]$data.tag -ne $expectedTag) {
    throw (
        "Release tag '{0}' does not match expected '{1}'." -f
        [string]$data.tag,
        $expectedTag
    )
}

$checksumPath =
    Join-Path $resolvedReleaseDirectory "SHA256SUMS.txt"

if (-not (Test-Path -LiteralPath $checksumPath -PathType Leaf)) {
    throw "SHA256SUMS.txt is missing."
}

$checksumErrors = @()
$checksumEntries = @{}

foreach ($line in Get-Content -LiteralPath $checksumPath) {
    if ([string]::IsNullOrWhiteSpace($line)) {
        continue
    }

    if ($line -notmatch '^([0-9a-fA-F]{64})  (.+)$') {
        $checksumErrors += "Invalid checksum line: $line"
        continue
    }

    $checksumEntries[$Matches[2]] = $Matches[1].ToLowerInvariant()
}

$filesToVerify =
    Get-ChildItem -LiteralPath $resolvedReleaseDirectory -File |
    Where-Object { $_.Name -ne "SHA256SUMS.txt" } |
    Sort-Object Name

foreach ($file in $filesToVerify) {
    if (-not $checksumEntries.ContainsKey($file.Name)) {
        $checksumErrors += (
            "Checksum missing for '{0}'." -f $file.Name
        )
        continue
    }

    $actual =
        (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).
        Hash.
        ToLowerInvariant()

    if ($actual -ne $checksumEntries[$file.Name]) {
        $checksumErrors += (
            "Checksum mismatch for '{0}'." -f $file.Name
        )
    }
}

foreach ($name in $checksumEntries.Keys) {
    $path = Join-Path $resolvedReleaseDirectory $name

    if (-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        $checksumErrors += (
            "Checksum references missing file '{0}'." -f $name
        )
    }
}

if ($checksumErrors.Count -gt 0) {
    $checksumErrors |
        ForEach-Object { Write-Host $_ -ForegroundColor Red }

    throw (
        "Release checksum validation failed with {0} error(s)." -f
        $checksumErrors.Count
    )
}

Add-Type -AssemblyName System.IO.Compression.FileSystem

function Test-ZipContainsEntry(
    [string]$ZipPath,
    [string]$ExpectedEntry
) {
    $archive =
        [System.IO.Compression.ZipFile]::OpenRead($ZipPath)

    try {
        $entry =
            $archive.Entries |
            Where-Object {
                $_.FullName.Replace("\", "/") -eq $ExpectedEntry
            } |
            Select-Object -First 1

        return $null -ne $entry
    }
    finally {
        $archive.Dispose()
    }
}

function Test-ZipReadable([string]$ZipPath) {
    $archive =
        [System.IO.Compression.ZipFile]::OpenRead($ZipPath)

    try {
        $buffer = New-Object byte[] 81920

        foreach ($entry in $archive.Entries) {
            if ([string]::IsNullOrEmpty($entry.Name)) {
                continue
            }

            $stream = $entry.Open()

            try {
                while ($stream.Read($buffer, 0, $buffer.Length) -gt 0) {
                }
            }
            finally {
                $stream.Dispose()
            }
        }
    }
    finally {
        $archive.Dispose()
    }

    return $true
}

$releaseVersion = [string]$data.releaseVersion

$validatedZip =
    Join-Path $resolvedReleaseDirectory `
        "LotSizingDataModel-$releaseVersion-validated.zip"

$documentationZip =
    Join-Path $resolvedReleaseDirectory `
        "LotSizingDataModel-$releaseVersion-documentation.zip"

foreach ($zip in @($validatedZip, $documentationZip)) {
    if (-not (Test-Path -LiteralPath $zip -PathType Leaf)) {
        throw "Required ZIP is missing: $zip"
    }

    [void](Test-ZipReadable $zip)
}

if (-not (Test-ZipContainsEntry $validatedZip "build-info.json")) {
    throw "Validated ZIP does not contain build-info.json."
}

if (-not (
    Test-ZipContainsEntry `
        $validatedZip `
        "bin/LotSizingDataModel.Core.dll"
)) {
    throw "Validated ZIP does not contain LotSizingDataModel.Core.dll."
}

if (-not (Test-ZipContainsEntry $documentationZip "index.html")) {
    throw "Documentation ZIP does not contain index.html."
}

Write-Host (
    "Release artifact validation passed: {0} file(s), checksums and ZIP contents are valid." -f
    $filesToVerify.Count
) -ForegroundColor Green

[pscustomobject]@{
    ReleaseVersion = $releaseVersion
    Tag = [string]$data.tag
    FilesValidated = $filesToVerify.Count
    ChecksumErrors = 0
}
