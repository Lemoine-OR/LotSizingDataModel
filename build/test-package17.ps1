param(
    [string]$Configuration = "Debug"
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$testProject = Join-Path $root "LotSizingDataModel.Checker.Tests\LotSizingDataModel.Checker.Tests.csproj"

Write-Host "Package 17 - multi-solver regression tests"
Write-Host "Project: $testProject"
Write-Host

dotnet test $testProject -c $Configuration

if ($LASTEXITCODE -ne 0) {
    exit $LASTEXITCODE
}

Write-Host
Write-Host "Expected test count after Package 17: 30"
