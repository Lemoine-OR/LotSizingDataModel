$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $repoRoot "LotSizingDataModel.Checker.Tests\LotSizingDataModel.Checker.Tests.csproj"
$resultDirectory = Join-Path $repoRoot "TestResults\Checker"

New-Item -ItemType Directory -Force -Path $resultDirectory | Out-Null

dotnet test $testProject `
    -c Release `
    --logger "trx;LogFileName=checker-tests.trx" `
    --results-directory $resultDirectory

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host "Checker regression suite completed successfully."
Write-Host "TRX: $resultDirectory\checker-tests.trx"
