[CmdletBinding()]
param(
    [string]$RepositoryPath = (Split-Path -Parent $PSScriptRoot)
)

Set-StrictMode -Version Latest
$ErrorActionPreference='Stop'

$contractPath=
    Join-Path $RepositoryPath 'governance\PUBLIC-API-CONTRACT.json'

if(-not (Test-Path -LiteralPath $contractPath -PathType Leaf)) {
    throw ('Public API contract not found: '+$contractPath)
}

$raw=
    [System.IO.File]::ReadAllText(
        $contractPath)

try {
    $contract=
        $raw |
        ConvertFrom-Json `
            -ErrorAction Stop
}
catch {
    throw (
        'Invalid PUBLIC-API-CONTRACT.json: '+
        $_.Exception.Message)
}

if([string]::IsNullOrWhiteSpace(
        [string]$contract.contractVersion)) {
    throw 'Public API contract version is missing.'
}

$criticalTypes=
    @($contract.criticalPublicTypes)

if($criticalTypes.Count -eq 0) {
    throw 'Public API contract contains no critical public types.'
}

foreach($entry in $criticalTypes) {
    $sourcePath=
        Join-Path `
            $RepositoryPath `
            ([string]$entry.sourcePath).Replace('/','\')

    if(-not (Test-Path -LiteralPath $sourcePath -PathType Leaf)) {
        throw (
            'Critical public API source is missing: '+
            $entry.sourcePath)
    }

    $source=
        [System.IO.File]::ReadAllText(
            $sourcePath)

    $fullType=
        [string]$entry.type

    $typeName=
        $fullType.Substring(
            $fullType.LastIndexOf('.')+1)

    $pattern=
        '(?m)^\s*public\s+(?:(?:sealed|abstract|static|partial)\s+)*(?:class|record|interface|enum)\s+'+
        [System.Text.RegularExpressions.Regex]::Escape(
            $typeName)+
        '\b'

    if(-not [System.Text.RegularExpressions.Regex]::IsMatch(
            $source,
            $pattern)) {
        throw (
            'Critical public type declaration not found in expected source: '+
            $fullType)
    }
}

$projectFiles=
    @(
        Get-ChildItem `
            -LiteralPath $RepositoryPath `
            -Recurse `
            -File `
            -Filter '*.csproj'
    )

$forbidden=
    @($contract.forbiddenFoundationDependencies)

foreach($project in $projectFiles) {
    $projectRaw=
        [System.IO.File]::ReadAllText(
            $project.FullName)

    foreach($token in $forbidden) {
        $name=
            [string]$token

        if($projectRaw.IndexOf(
                $name,
                [System.StringComparison]::OrdinalIgnoreCase) -ge 0) {
            throw (
                'Forbidden foundational dependency token '+
                $name+
                ' found in '+
                $project.FullName)
        }
    }
}

Write-Host 'PUBLIC-API-CONTRACT JSON            : GREEN'
Write-Host 'Critical public source identities   : GREEN'
Write-Host 'No UI dependency in project files   : GREEN'
Write-Host 'No downstream MLLP dependency       : GREEN'
