param(
    [Parameter(Mandatory = $true)]
    [string]$ResolvedDirectory
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "LotSizingDataModel.Checker.Cli\LotSizingDataModel.Checker.Cli.csproj"
$baselinePath = Join-Path $repoRoot "LotSizingDataModel.Checker.Tests\Baselines\DellaertJeunet.Small96.baseline.json"

if (-not (Test-Path -LiteralPath $ResolvedDirectory -PathType Container)) {
    throw "Resolved directory not found: $ResolvedDirectory"
}

if (-not (Test-Path -LiteralPath $project -PathType Leaf)) {
    throw "Checker CLI project not found: $project"
}

if (-not (Test-Path -LiteralPath $baselinePath -PathType Leaf)) {
    throw "Baseline definition not found: $baselinePath"
}

Write-Host "Running independent checker campaign..."
& dotnet run --project $project -- $ResolvedDirectory --level full --quiet

if ($LASTEXITCODE -ne 0) {
    throw "Checker campaign failed with exit code $LASTEXITCODE."
}

$reportPath = Join-Path $ResolvedDirectory "_checker-reports\campaign-validation.txt"

if (-not (Test-Path -LiteralPath $reportPath -PathType Leaf)) {
    throw "Campaign validation report was not produced: $reportPath"
}

$baseline = Get-Content -LiteralPath $baselinePath -Raw | ConvertFrom-Json
$lines = Get-Content -LiteralPath $reportPath

function Read-SummaryValue([string]$label) {
    $line = $lines | Where-Object { $_.TrimStart().StartsWith($label, [System.StringComparison]::OrdinalIgnoreCase) } | Select-Object -First 1
    if ($null -eq $line) {
        throw "Missing summary line: $label"
    }
    $parts = $line -split ':', 2
    if ($parts.Count -ne 2) {
        throw "Malformed summary line: $line"
    }
    return $parts[1].Trim()
}

$overall = Read-SummaryValue "Overall status"
$candidateCount = [int](Read-SummaryValue "Candidates")
$validCount = [int](Read-SummaryValue "Valid")
$invalidCount = [int](Read-SummaryValue "Invalid")
$executionFailureCount = [int](Read-SummaryValue "Execution failures")
$fileLoadFailureCount = [int](Read-SummaryValue "File load failures")

$rows = @()
foreach ($line in $lines) {
    $trimmed = $line.Trim()
    if ($trimmed -notmatch '^\d+\s*\|') {
        continue
    }

    $columns = @($trimmed -split '\|' | ForEach-Object { $_.Trim() })
    if ($columns.Count -ne 11) {
        continue
    }

    $rows += [PSCustomObject]@{
        Instance = $columns[1]
        Exec = $columns[3]
        Valid = $columns[4]
        Structure = $columns[5]
        Domain = $columns[6]
        Feasibility = $columns[7]
        Objective = $columns[8]
        Violations = [int]$columns[9]
        ObjectiveDifference = [double]::Parse($columns[10], [System.Globalization.CultureInfo]::InvariantCulture)
    }
}

$errors = New-Object System.Collections.Generic.List[string]

if ($overall -ne $baseline.expectedOverallStatus) { $errors.Add("Overall status: expected $($baseline.expectedOverallStatus), got $overall") }
if ($candidateCount -ne $baseline.expectedCandidateCount) { $errors.Add("Candidates: expected $($baseline.expectedCandidateCount), got $candidateCount") }
if ($validCount -ne $baseline.expectedValidCandidateCount) { $errors.Add("Valid: expected $($baseline.expectedValidCandidateCount), got $validCount") }
if ($invalidCount -ne $baseline.expectedInvalidCandidateCount) { $errors.Add("Invalid: expected $($baseline.expectedInvalidCandidateCount), got $invalidCount") }
if ($executionFailureCount -ne $baseline.expectedExecutionFailureCount) { $errors.Add("Execution failures: expected $($baseline.expectedExecutionFailureCount), got $executionFailureCount") }
if ($fileLoadFailureCount -ne $baseline.expectedFileLoadFailureCount) { $errors.Add("File load failures: expected $($baseline.expectedFileLoadFailureCount), got $fileLoadFailureCount") }
if ($rows.Count -ne $baseline.expectedCandidateCount) { $errors.Add("Candidate rows: expected $($baseline.expectedCandidateCount), got $($rows.Count)") }

$expectedInstances = @($baseline.expectedInstances | Sort-Object)
$actualInstances = @($rows.Instance | Sort-Object)
if ((Compare-Object -ReferenceObject $expectedInstances -DifferenceObject $actualInstances).Count -ne 0) {
    $errors.Add("Instance set differs from the Small96 baseline.")
}

foreach ($row in $rows) {
    if ($row.Exec -ne "OK") { $errors.Add("$($row.Instance): execution status is $($row.Exec)") }
    if ($row.Valid -ne "YES") { $errors.Add("$($row.Instance): validity is $($row.Valid)") }
    if ($row.Structure -ne "Passed") { $errors.Add("$($row.Instance): structural check is $($row.Structure)") }
    if ($row.Domain -ne "Passed") { $errors.Add("$($row.Instance): domain check is $($row.Domain)") }
    if ($row.Feasibility -ne "Passed") { $errors.Add("$($row.Instance): feasibility check is $($row.Feasibility)") }
    if ($row.Objective -ne "Passed") { $errors.Add("$($row.Instance): objective check is $($row.Objective)") }
    if ($row.Violations -gt $baseline.maximumViolatedConstraintCount) { $errors.Add("$($row.Instance): $($row.Violations) violated constraints") }
    if ([Math]::Abs($row.ObjectiveDifference) -gt $baseline.maximumObjectiveAbsoluteDifference) { $errors.Add("$($row.Instance): objective difference $($row.ObjectiveDifference) exceeds $($baseline.maximumObjectiveAbsoluteDifference)") }
}

if ($errors.Count -gt 0) {
    Write-Host ""
    Write-Host "REGRESSION DETECTED" -ForegroundColor Red
    foreach ($errorMessage in $errors) {
        Write-Host "  - $errorMessage" -ForegroundColor Red
    }
    exit 1
}

$maxObjectiveDifference = ($rows | Measure-Object -Property ObjectiveDifference -Maximum).Maximum

Write-Host ""
Write-Host "BASELINE OK" -ForegroundColor Green
Write-Host "  Candidates              : $candidateCount / $($baseline.expectedCandidateCount)"
Write-Host "  Valid                   : $validCount"
Write-Host "  Invalid                 : $invalidCount"
Write-Host "  Execution failures      : $executionFailureCount"
Write-Host "  File load failures      : $fileLoadFailureCount"
Write-Host "  Max objective difference: $maxObjectiveDifference"
Write-Host "  Allowed maximum         : $($baseline.maximumObjectiveAbsoluteDifference)"
exit 0
