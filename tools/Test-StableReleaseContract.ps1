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

    $raw=
        [System.IO.File]::ReadAllText(
            $Path)

    try {
        return (
            $raw |
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

$versionPath=
    Join-Path $RepositoryPath 'version.json'

$version=
    Read-Json `
        -Path $versionPath `
        -Label 'version'

if([string]$version.version -ne '1.2.0-alpha.44') {
    throw (
        'Stable-release hardening requires version 1.2.0-alpha.44; found '+
        [string]$version.version)
}

$publicApi=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\PUBLIC-API-CONTRACT.json') `
        -Label 'PUBLIC-API-CONTRACT'

if([string]$publicApi.contractVersion -ne '1.0-alpha.44') {
    throw 'Unexpected public API contract version.'
}

if([string]$publicApi.stability -ne 'release-candidate') {
    throw 'Public API contract is not marked release-candidate.'
}

$xmlContract=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\XML-COMPATIBILITY-CONTRACT.json') `
        -Label 'XML-COMPATIBILITY-CONTRACT'

if([string]$xmlContract.instance.rootElement -ne 'lotSizingInstance') {
    throw 'Protected instance XML root changed.'
}

if([string]$xmlContract.solution.rootElement -ne 'lotSizingSolution') {
    throw 'Protected solution XML root changed.'
}

$releaseContract=
    Read-Json `
        -Path (Join-Path $RepositoryPath 'governance\STABLE-RELEASE-CONTRACT.json') `
        -Label 'STABLE-RELEASE-CONTRACT'

if([string]$releaseContract.targetStableVersion -ne '1.2.0') {
    throw 'Unexpected target stable version.'
}

if([bool]$releaseContract.releaseMayBePublishedByThisPack) {
    throw 'Alpha.44 hardening pack must not authorize release publication.'
}

foreach($requiredFile in @(
    'API-STABILITY.md',
    'RELEASE-CHECKLIST.md',
    'CITATION.cff',
    'docs\integration\stable-release-hardening-alpha44.md',
    'tools\Test-PublicApiContract.ps1'
)) {
    $path=
        Join-Path $RepositoryPath $requiredFile

    if(-not (Test-Path -LiteralPath $path -PathType Leaf)) {
        throw ('Stable-release contract file missing: '+$requiredFile)
    }
}

$citation=
    [System.IO.File]::ReadAllText(
        (Join-Path $RepositoryPath 'CITATION.cff'))

foreach($token in @(
    'cff-version: 1.2.0',
    'title: "LotSizingDataModel"',
    'version: "1.2.0-alpha.44"',
    'repository-code: "https://github.com/Lemoine-OR/LotSizingDataModel"'
)) {
    if($citation.IndexOf(
            $token,
            [System.StringComparison]::Ordinal) -lt 0) {
        throw ('CITATION.cff required token missing: '+$token)
    }
}

Write-Host 'Stable hardening version            : GREEN'
Write-Host 'Public API release-candidate contract: GREEN'
Write-Host 'XML root compatibility contract     : GREEN'
Write-Host 'Stable promotion contract           : GREEN'
Write-Host 'API stability documentation         : GREEN'
Write-Host 'Citation metadata                   : GREEN'
Write-Host 'Release checklist                   : GREEN'
