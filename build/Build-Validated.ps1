[CmdletBinding()]
param(
    [switch]$IncludeExternalSolverAdapters
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

# Parse every automation script before any long-running build step.
& (Join-Path $RepoRoot "tools\Test-PowerShellSyntax.ps1") -Quiet | Out-Null
Write-Host "PowerShell automation syntax: OK" -ForegroundColor Green

& (Join-Path $RepoRoot "tools\Test-VersioningPreflight.ps1")
& (Join-Path $RepoRoot "tools\Test-VersionIdentity.ps1")

# Scientific governance is a first-class build gate from v1.2.0-alpha.1 onward.
& (Join-Path $RepoRoot "tools\Test-ScientificGovernance.ps1")

$TestResults = Join-Path $RepoRoot "Documentation\test-results"
$Artifacts = Join-Path $RepoRoot "Documentation\artifacts"
$SolverManifest = Join-Path $Artifacts "solver-adapters.json"

Remove-Item -LiteralPath $TestResults -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $Artifacts -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force $TestResults | Out-Null
New-Item -ItemType Directory -Force $Artifacts | Out-Null

$batchConverter = Join-Path $RepoRoot "LotSizingDataModel.BatchConverter\LotSizingDataModel.BatchConverter.csproj"
$checkerTests = Join-Path $RepoRoot "LotSizingDataModel.Checker.Tests\LotSizingDataModel.Checker.Tests.csproj"

foreach ($requiredProject in @($batchConverter, $checkerTests)) {
    if (-not (Test-Path -LiteralPath $requiredProject)) {
        throw "Required project not found: $requiredProject"
    }
}

Write-Host ""
Write-Host "=== Restore/build core dependency chain ===" -ForegroundColor Cyan
& dotnet restore $batchConverter | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Restore failed for BatchConverter." }

& dotnet build $batchConverter -c Release --no-restore | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Release build failed for BatchConverter." }

Write-Host ""
Write-Host "=== Restore/build checker test chain ===" -ForegroundColor Cyan
& dotnet restore $checkerTests | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Restore failed for Checker.Tests." }

& dotnet build $checkerTests -c Release --no-restore | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Release build failed for Checker.Tests." }

Write-Host ""
Write-Host "=== Tests ===" -ForegroundColor Cyan
& dotnet test $checkerTests `
    -c Release `
    --no-build `
    --logger "trx;LogFileName=LotSizingDataModel.Checker.Tests.trx" `
    --results-directory $TestResults | Out-Host
if ($LASTEXITCODE -ne 0) { throw "Checker test suite failed." }

Write-Host ""
Write-Host "=== Solver adapter discovery/build ===" -ForegroundColor Cyan
$solverArgs = @{
    Configuration = "Release"
    ManifestPath = $SolverManifest
}
if ($IncludeExternalSolverAdapters) {
    $solverArgs["IncludeExternal"] = $true
}
& (Join-Path $RepoRoot "build\Build-SolverAdapters.ps1") @solverArgs | Out-Null

Write-Host ""
Write-Host "=== Package + metadata/icon verification ===" -ForegroundColor Cyan
$package = & (Join-Path $RepoRoot "build\Package-ValidatedBinaries.ps1") `
    -Configuration "Release" `
    -SolverManifestPath $SolverManifest

$version = & (Join-Path $RepoRoot "tools\Get-LotSizingVersion.ps1") -SkipRestore

Write-Host ""
Write-Host "============================================================"
Write-Host "LotSizingDataModel validated build succeeded" -ForegroundColor Green
Write-Host "Version : $($version.DisplayVersion)"
Write-Host "File    : $($version.AssemblyFileVersion)"
Write-Host "Commit  : $($version.GitCommitIdShort)"
Write-Host "Package : $($package.ZipPath)"
Write-Host "============================================================"

return $package
