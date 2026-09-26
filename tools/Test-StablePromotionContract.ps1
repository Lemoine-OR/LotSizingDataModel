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

if([string]$version.version -notmatch '^\d+\.\d+\.\d+$') {
    throw ('Stable promotion requires a stable semantic version; found '+[string]$version.version)
}

$api=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\PUBLIC-API-CONTRACT.json') `
        -Label 'PUBLIC-API-CONTRACT'

if([string]$api.contractVersion -ne '2.0' -or
   [string]$api.stability -ne 'stable') {
    throw 'Public API contract is not stable 2.0.'
}

$xml=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\XML-COMPATIBILITY-CONTRACT.json') `
        -Label 'XML-COMPATIBILITY-CONTRACT'

if([string]$xml.contractVersion -ne '2.0') {
    throw 'XML compatibility contract is not stable 2.0.'
}

if([string]$xml.instance.rootElement -ne 'lotSizingInstance' -or
   [string]$xml.solution.rootElement -ne 'lotSizingSolution') {
    throw 'Protected XML roots changed during stable promotion.'
}

$release=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\STABLE-RELEASE-CONTRACT.json') `
        -Label 'STABLE-RELEASE-CONTRACT'

if([string]$release.stableVersion -ne [string]$version.version) {
    throw 'Stable release contract version mismatch.'
}

if(-not [bool]$release.publicationRequiresExplicitAuthorization) {
    throw 'Stable publication must retain the explicit authorization requirement.'
}

$gaps=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\STABLE-OPEN-GAPS.json') `
        -Label 'STABLE-OPEN-GAPS'

if(@($gaps.openItems).Count -eq 0) {
    throw 'Stable open-gap register is unexpectedly empty.'
}

if([string]$gaps.version -ne [string]$version.version) {
    throw 'Stable open-gap register version mismatch.'
}

$stableLine = ([string]$version.version -replace '\.\d+$', '.x')
if([string]$api.stableLine -ne $stableLine -or
   [string]$xml.stableLine -ne $stableLine) {
    throw 'API/XML stable line does not match the release version.'
}

$citation=
    [System.IO.File]::ReadAllText(
        (Join-Path $RepositoryPath 'CITATION.cff'))

if($citation.IndexOf(
        ('version: "'+[string]$version.version+'"'),
        [System.StringComparison]::Ordinal) -lt 0) {
    throw 'CITATION.cff stable version token is missing.'
}

Write-Host 'Stable version identity             : GREEN'
Write-Host 'Public API contract                 : STABLE 2.0'
Write-Host 'XML compatibility contract          : STABLE 2.0'
Write-Host ('Stable publication eligibility      : '+[bool]$release.releaseMayBePublishedByThisPack)
Write-Host 'Stable open-gap register            : PRESENT'
Write-Host 'CITATION stable version             : GREEN'
