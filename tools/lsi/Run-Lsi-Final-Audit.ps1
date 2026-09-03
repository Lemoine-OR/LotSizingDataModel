[CmdletBinding()]
param(
    [Parameter(Mandatory = $false)]
    [string]$Repository = "D:\Dev\LotSizingDataModel"
)

$ErrorActionPreference = "Stop"
Set-StrictMode -Version 2.0

$testProject = Join-Path `
    $Repository `
    "LotSizingDataModel.Checker.Tests\LotSizingDataModel.Checker.Tests.csproj"

if (-not (Test-Path -LiteralPath $testProject -PathType Leaf))
{
    throw ("Checker tests project not found: " + $testProject)
}

Write-Host "LSI_FINAL_AUDIT"
Write-Host ("REPOSITORY|" + $Repository)

& dotnet test `
    $testProject `
    -c Release `
    --no-restore `
    --nologo `
    --filter "FullyQualifiedName~LsiNotationIntegrationTests|FullyQualifiedName~LsiPack03LocalCompatibilityTests|FullyQualifiedName~LsiPack04FinalizationTests"

if ($LASTEXITCODE -ne 0)
{
    throw ("LSI final audit failed with exit code " + $LASTEXITCODE)
}

Write-Host "LSI_FINAL_AUDIT_VALID"
exit 0
