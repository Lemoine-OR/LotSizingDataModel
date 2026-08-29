[CmdletBinding()]
param()

Set-StrictMode -Version 2.0
$ErrorActionPreference = 'Stop'

$RepoRoot = Split-Path -Parent $PSScriptRoot
$GovernanceRoot = Join-Path $RepoRoot 'governance'

function Read-LsdmGovernanceJson {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        throw "Required governance file not found: $Path"
    }

    try {
        return Get-Content -LiteralPath $Path -Raw |
            ConvertFrom-Json
    }
    catch {
        throw "Invalid JSON in governance file '$Path': $($_.Exception.Message)"
    }
}

function Assert-LsdmCondition {
    param(
        [Parameter(Mandatory = $true)]
        [bool]$Condition,

        [Parameter(Mandatory = $true)]
        [string]$Message
    )

    if (-not $Condition) {
        throw $Message
    }
}

function Assert-LsdmUniqueValues {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Values,

        [Parameter(Mandatory = $true)]
        [string]$Description
    )

    $normalized =
        @(
            $Values |
            ForEach-Object { [string]$_ }
        )

    $duplicates =
        @(
            $normalized |
            Group-Object |
            Where-Object { $_.Count -gt 1 } |
            ForEach-Object { $_.Name }
        )

    Assert-LsdmCondition `
        -Condition ($duplicates.Count -eq 0) `
        -Message (
            "Duplicate $Description values: " +
            ($duplicates -join ', ')
        )
}

$requiredFiles = @(
    'README.md',
    'CAPITALIZATION.json',
    'SCIENTIFIC-COVERAGE.json',
    'VALIDATION-RULES.json',
    'EXTENSION-CONTRACT.schema.json',
    'EXTENSION-CONTRACT.template.json',
    'ARTIFACT-LIFECYCLE.json'
)

foreach ($fileName in $requiredFiles) {
    $filePath = Join-Path $GovernanceRoot $fileName

    Assert-LsdmCondition `
        -Condition (Test-Path -LiteralPath $filePath -PathType Leaf) `
        -Message "Missing governance file: $filePath"
}

$capitalization =
    Read-LsdmGovernanceJson `
        -Path (Join-Path $GovernanceRoot 'CAPITALIZATION.json')

Assert-LsdmCondition `
    -Condition ($capitalization.schemaVersion -eq 1) `
    -Message 'Unsupported CAPITALIZATION.json schemaVersion.'

$capitalizationEntries = @($capitalization.entries)

Assert-LsdmCondition `
    -Condition ($capitalizationEntries.Count -gt 0) `
    -Message 'CAPITALIZATION.json must contain at least one active lesson.'

Assert-LsdmUniqueValues `
    -Values @($capitalizationEntries | ForEach-Object { $_.id }) `
    -Description 'capitalization rule ID'

foreach ($entry in $capitalizationEntries) {
    Assert-LsdmCondition `
        -Condition (-not [string]::IsNullOrWhiteSpace([string]$entry.id)) `
        -Message 'Every capitalization entry requires a non-empty id.'

    Assert-LsdmCondition `
        -Condition ([string]$entry.status -eq 'active') `
        -Message "Capitalization rule '$($entry.id)' is not active."
}

$coverage =
    Read-LsdmGovernanceJson `
        -Path (Join-Path $GovernanceRoot 'SCIENTIFIC-COVERAGE.json')

Assert-LsdmCondition `
    -Condition ($coverage.schemaVersion -eq 1) `
    -Message 'Unsupported SCIENTIFIC-COVERAGE.json schemaVersion.'

$concepts = @($coverage.concepts)

Assert-LsdmCondition `
    -Condition ($concepts.Count -gt 0) `
    -Message 'SCIENTIFIC-COVERAGE.json must contain concepts.'

Assert-LsdmUniqueValues `
    -Values @($concepts | ForEach-Object { $_.id }) `
    -Description 'scientific concept ID'

$allowedImplementationStatus = @(
    'implemented',
    'partial',
    'unsupported',
    'deferred',
    'experimental'
)

foreach ($concept in $concepts) {
    Assert-LsdmCondition `
        -Condition (
            $allowedImplementationStatus -contains
            [string]$concept.implementationStatus
        ) `
        -Message (
            "Scientific concept '$($concept.id)' has unsupported " +
            "implementationStatus '$($concept.implementationStatus)'."
        )
}

$validation =
    Read-LsdmGovernanceJson `
        -Path (Join-Path $GovernanceRoot 'VALIDATION-RULES.json')

Assert-LsdmCondition `
    -Condition ($validation.schemaVersion -eq 1) `
    -Message 'Unsupported VALIDATION-RULES.json schemaVersion.'

Assert-LsdmUniqueValues `
    -Values @($validation.services | ForEach-Object { $_.id }) `
    -Description 'validation service ID'

Assert-LsdmUniqueValues `
    -Values @($validation.codeNamespaces | ForEach-Object { $_.prefix }) `
    -Description 'validation code prefix'

$requiredSeverities = @(
    'Information',
    'Warning',
    'Error',
    'Fatal'
)

foreach ($severity in $requiredSeverities) {
    Assert-LsdmCondition `
        -Condition (@($validation.severities) -contains $severity) `
        -Message "Validation severity '$severity' is missing."
}

$extensionSchema =
    Read-LsdmGovernanceJson `
        -Path (Join-Path $GovernanceRoot 'EXTENSION-CONTRACT.schema.json')

Assert-LsdmCondition `
    -Condition ($extensionSchema.title -eq 'LotSizingDataModel scientific extension contract') `
    -Message 'Unexpected extension-contract schema identity.'

$extensionTemplate =
    Read-LsdmGovernanceJson `
        -Path (Join-Path $GovernanceRoot 'EXTENSION-CONTRACT.template.json')

Assert-LsdmCondition `
    -Condition ($extensionTemplate.schemaVersion -eq 1) `
    -Message 'Unsupported extension-contract template schemaVersion.'

$artifactLifecycle =
    Read-LsdmGovernanceJson `
        -Path (Join-Path $GovernanceRoot 'ARTIFACT-LIFECYCLE.json')

Assert-LsdmCondition `
    -Condition ($artifactLifecycle.schemaVersion -eq 1) `
    -Message 'Unsupported ARTIFACT-LIFECYCLE.json schemaVersion.'

Assert-LsdmUniqueValues `
    -Values @($artifactLifecycle.managedRoots | ForEach-Object { $_.path }) `
    -Description 'managed artifact root'

Write-Host 'Scientific governance validation passed.' -ForegroundColor Green
