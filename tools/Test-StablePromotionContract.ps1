[CmdletBinding()]
param(
    [string]$RepositoryPath = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'

function Read-Json {
    param(
        [Parameter(Mandatory=$true)][string]$Path,
        [Parameter(Mandatory=$true)][string]$Label
    )

    if(-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw ($Label+' file is missing: '+$Path)
    }

    try {
        return (
            [System.IO.File]::ReadAllText($Path) |
            ConvertFrom-Json `
                -ErrorAction Stop
        )
    }
    catch {
        throw (
            'Invalid '+
            $Label+
            ' JSON: '+
            $_.Exception.Message)
    }
}

$version=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'version.json') `
        -Label 'version'

if([string]$version.version -ne '1.2.0') {
    throw ('Stable promotion requires version 1.2.0; found '+[string]$version.version)
}

$api=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\PUBLIC-API-CONTRACT.json') `
        -Label 'PUBLIC-API-CONTRACT'

if([string]$api.contractVersion -ne '1.0' -or
   [string]$api.stability -ne 'stable') {
    throw 'Public API contract is not stable 1.0.'
}

$xml=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\XML-COMPATIBILITY-CONTRACT.json') `
        -Label 'XML-COMPATIBILITY-CONTRACT'

if([string]$xml.contractVersion -ne '1.0') {
    throw 'XML compatibility contract is not stable 1.0.'
}

if([string]$xml.instance.rootElement -ne 'lotSizingInstance' -or
   [string]$xml.solution.rootElement -ne 'lotSizingSolution') {
    throw 'Protected XML roots changed during stable promotion.'
}

$release=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\STABLE-RELEASE-CONTRACT.json') `
        -Label 'STABLE-RELEASE-CONTRACT'

if([string]$release.stableVersion -ne '1.2.0') {
    throw 'Stable release contract version mismatch.'
}

if([bool]$release.releaseMayBePublishedByThisPack) {
    throw 'Stable promotion pack must not publish the release.'
}

$gaps=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\STABLE-OPEN-GAPS.json') `
        -Label 'STABLE-OPEN-GAPS'

if(@($gaps.openItems).Count -eq 0) {
    throw 'Stable open-gap register is unexpectedly empty.'
}

$citation=
    [System.IO.File]::ReadAllText(
        (Join-Path $RepositoryPath 'CITATION.cff'))

if($citation.IndexOf(
        'version: "1.2.0"',
        [System.StringComparison]::Ordinal) -lt 0) {
    throw 'CITATION.cff stable version token is missing.'
}

Write-Host 'Stable version identity             : GREEN'
Write-Host 'Public API contract                 : STABLE 1.0'
Write-Host 'XML compatibility contract          : STABLE 1.0'
Write-Host 'Stable release publication          : FORBIDDEN IN PACK'
Write-Host 'Stable open-gap register            : PRESENT'
Write-Host 'CITATION stable version             : GREEN'
