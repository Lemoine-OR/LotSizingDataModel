param(
    [Parameter(Mandatory = $true)]
    [string]$ResolvedDirectory
)

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$project = Join-Path $repoRoot "LotSizingDataModel.Checker.Cli\LotSizingDataModel.Checker.Cli.csproj"

if (-not (Test-Path -LiteralPath $ResolvedDirectory -PathType Container)) {
    throw "Resolved directory not found: $ResolvedDirectory"
}

$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()
$output = & dotnet run --project $project -- $ResolvedDirectory --level full --quiet --no-reports 2>&1
$exitCode = $LASTEXITCODE
$stopwatch.Stop()

$output | ForEach-Object { Write-Host $_ }

if ($exitCode -ne 0) {
    throw "Checker campaign failed with exit code $exitCode."
}

$candidateLine = $output | Where-Object { $_ -match '^Candidates\s*:' } | Select-Object -First 1
$candidates = 0
if ($candidateLine -match ':\s*(\d+)') {
    $candidates = [int]$Matches[1]
}

$seconds = $stopwatch.Elapsed.TotalSeconds
$rate = if ($seconds -gt 0 -and $candidates -gt 0) { $candidates / $seconds } else { 0 }

Write-Host ""
Write-Host "Checker performance measurement"
Write-Host "  Candidates : $candidates"
Write-Host ("  Elapsed    : {0:N3} s" -f $seconds)
Write-Host ("  Throughput : {0:N2} candidates/s" -f $rate)
Write-Host "  Note       : measurement only; no brittle timing threshold is enforced."
