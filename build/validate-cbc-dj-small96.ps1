[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$InputDirectory,

    [string]$Configuration = "Debug",

    [switch]$CleanResolvedDirectory
)

$ErrorActionPreference = "Stop"

$repoRoot = Split-Path -Parent $PSScriptRoot
$solverConsoleProject = Join-Path $repoRoot "LotSizingDataModel.Solver.Console\LotSizingDataModel.Solver.Console.csproj"
$baselineScript = Join-Path $repoRoot "build\verify-dj-small96-baseline.ps1"
$resolvedDirectoryName = "r" + [char]0x00E9 + "solu-cbc"
$resolvedDirectory = Join-Path $InputDirectory $resolvedDirectoryName

function Resolve-CbcExecutable {
    if (-not [string]::IsNullOrWhiteSpace($env:LOTSIZING_CBC_EXECUTABLE) -and
        (Test-Path -LiteralPath $env:LOTSIZING_CBC_EXECUTABLE -PathType Leaf)) {
        return [System.IO.Path]::GetFullPath($env:LOTSIZING_CBC_EXECUTABLE)
    }

    foreach ($homeVariableName in @("CBC_HOME", "COINOR_HOME")) {
        $home = [Environment]::GetEnvironmentVariable($homeVariableName)
        if ([string]::IsNullOrWhiteSpace($home)) {
            continue
        }

        foreach ($relative in @("bin\cbc.exe", "bin\cbc", "cbc.exe", "cbc")) {
            $candidate = Join-Path $home $relative
            if (Test-Path -LiteralPath $candidate -PathType Leaf) {
                return [System.IO.Path]::GetFullPath($candidate)
            }
        }
    }

    foreach ($commandName in @("cbc.exe", "cbc")) {
        $command = Get-Command $commandName -ErrorAction SilentlyContinue
        if ($null -ne $command -and -not [string]::IsNullOrWhiteSpace($command.Source)) {
            return $command.Source
        }
    }

    throw "CBC executable not found. Set LOTSIZING_CBC_EXECUTABLE, CBC_HOME/COINOR_HOME, or put cbc on PATH."
}

function Read-SolverSummary {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Lines
    )

    $summaryStart = -1
    for ($index = 0; $index -lt $Lines.Count; $index++) {
        $line = [string]$Lines[$index]
        if ($line.Contains("BILAN DU TRAITEMENT")) {
            $summaryStart = $index
        }
    }

    if ($summaryStart -lt 0) {
        throw "Solver output does not contain the final processing summary."
    }

    $values = New-Object System.Collections.Generic.List[int]
    for ($index = $summaryStart + 1; $index -lt $Lines.Count; $index++) {
        $line = [string]$Lines[$index]
        if ($line -match ':\s*(\d+)\s*$') {
            $values.Add([int]$Matches[1])
            if ($values.Count -eq 4) {
                break
            }
        }
    }

    if ($values.Count -ne 4) {
        throw "Could not parse the four numeric counters from the final solver summary."
    }

    return [PSCustomObject]@{
        Detected = $values[0]
        Verified = $values[1]
        Rejected = $values[2]
        TechnicalFailures = $values[3]
    }
}

function Read-MaxObjectiveDifference {
    param(
        [Parameter(Mandatory = $true)]
        [object[]]$Lines
    )

    foreach ($lineObject in $Lines) {
        $line = [string]$lineObject
        if ($line.TrimStart().StartsWith("Max objective difference", [System.StringComparison]::OrdinalIgnoreCase)) {
            $parts = $line -split ':', 2
            if ($parts.Count -ne 2) {
                throw "Malformed baseline summary line: $line"
            }

            $rawValue = $parts[1].Trim()
            $styles = [System.Globalization.NumberStyles]::Float
            $parsed = 0.0

            if ([double]::TryParse(
                    $rawValue,
                    $styles,
                    [System.Globalization.CultureInfo]::InvariantCulture,
                    [ref]$parsed)) {
                return $parsed
            }

            if ([double]::TryParse(
                    $rawValue,
                    $styles,
                    [System.Globalization.CultureInfo]::CurrentCulture,
                    [ref]$parsed)) {
                return $parsed
            }

            throw "Could not parse baseline objective difference: $rawValue"
        }
    }

    throw "Baseline output does not contain 'Max objective difference'."
}

if (-not (Test-Path -LiteralPath $InputDirectory -PathType Container)) {
    throw "Input directory not found: $InputDirectory"
}

if (-not (Test-Path -LiteralPath $solverConsoleProject -PathType Leaf)) {
    throw "Solver console project not found: $solverConsoleProject"
}

if (-not (Test-Path -LiteralPath $baselineScript -PathType Leaf)) {
    throw "Baseline verification script not found: $baselineScript"
}

$inputFiles = @(
    Get-ChildItem -LiteralPath $InputDirectory -Filter "*.xml" -File |
        Sort-Object Name
)

if ($inputFiles.Count -ne 96) {
    throw "The DJ Small96 certification requires exactly 96 top-level XML instances. Found: $($inputFiles.Count)."
}

$cbcExecutable = Resolve-CbcExecutable
$env:LOTSIZING_CBC_EXECUTABLE = $cbcExecutable

$cbcBanner = @(& $cbcExecutable -quit 2>&1)
if ($LASTEXITCODE -ne 0) {
    throw "CBC availability probe failed with exit code $LASTEXITCODE."
}

$cbcVersion = "unknown"
foreach ($lineObject in $cbcBanner) {
    $line = [string]$lineObject
    if ($line -match '^\s*Version\s*:\s*(\S+)') {
        $cbcVersion = $Matches[1]
        break
    }
}

if ($CleanResolvedDirectory -and (Test-Path -LiteralPath $resolvedDirectory -PathType Container)) {
    Remove-Item -LiteralPath $resolvedDirectory -Recurse -Force
}

Write-Host "CBC DJ Small96 certification" -ForegroundColor Cyan
Write-Host "================================"
Write-Host "CBC executable : $cbcExecutable"
Write-Host "CBC version    : $cbcVersion"
Write-Host "Input directory: $InputDirectory"
Write-Host "Instances      : $($inputFiles.Count)"
Write-Host ""
Write-Host "Running CBC solver campaign..." -ForegroundColor Cyan

$solverOutput = @(
    & dotnet run --project $solverConsoleProject -c $Configuration -- --solver cbc --input $InputDirectory 2>&1
)
$solverExitCode = $LASTEXITCODE
$solverOutput | ForEach-Object { Write-Host $_ }

if ($solverExitCode -ne 0) {
    throw "CBC solver campaign failed with exit code $solverExitCode."
}

$solverSummary = Read-SolverSummary -Lines $solverOutput
$detected = $solverSummary.Detected
$verified = $solverSummary.Verified
$rejected = $solverSummary.Rejected
$technicalFailures = $solverSummary.TechnicalFailures

if ($detected -ne 96 -or $verified -ne 96 -or $rejected -ne 0 -or $technicalFailures -ne 0) {
    throw "CBC campaign summary is not certifiable: detected=$detected, verified=$verified, rejected=$rejected, technicalFailures=$technicalFailures."
}

$sol005Matches = @()
if (Test-Path -LiteralPath $resolvedDirectory -PathType Container) {
    $reportFiles = @(
        Get-ChildItem -LiteralPath $resolvedDirectory -Filter "*.solution-check.txt" -File
    )

    foreach ($reportFile in $reportFiles) {
        $reportMatches = @(
            Select-String -LiteralPath $reportFile.FullName -Pattern "SOL005" -SimpleMatch
        )

        if ($reportMatches.Count -gt 0) {
            $sol005Matches += $reportMatches
        }
    }
}

if ($sol005Matches.Count -gt 0) {
    throw "Legacy SOL005 completeness warning is still present in $($sol005Matches.Count) checker report line(s)."
}

Write-Host ""
Write-Host "Running independent Small96 baseline verification..." -ForegroundColor Cyan

$baselineOutput = @(
    & $baselineScript -ResolvedDirectory $resolvedDirectory 2>&1
)
$baselineExitCode = $LASTEXITCODE
$baselineOutput | ForEach-Object { Write-Host $_ }

if ($baselineExitCode -ne 0) {
    throw "Small96 baseline verification failed with exit code $baselineExitCode."
}

$maxObjectiveDifference = Read-MaxObjectiveDifference -Lines $baselineOutput

$certificationLines = @(
    "CBC VALIDATION",
    "==============",
    "Solver executable        : $cbcExecutable",
    "Solver version           : $cbcVersion",
    "Instances detected       : $detected",
    "Solved and verified      : $verified",
    "Checker rejected         : $rejected",
    "Technical failures       : $technicalFailures",
    "SOL005 warnings          : $($sol005Matches.Count)",
    "Max objective difference : $($maxObjectiveDifference.ToString('G17', [System.Globalization.CultureInfo]::InvariantCulture))",
    "Result                   : PASS"
)

$certificationPath = Join-Path $resolvedDirectory "cbc-certification.txt"
[System.IO.File]::WriteAllLines(
    $certificationPath,
    $certificationLines,
    [System.Text.UTF8Encoding]::new($false))

Write-Host ""
Write-Host "CBC VALIDATION" -ForegroundColor Green
Write-Host "=============="
Write-Host "Solver version           : $cbcVersion"
Write-Host "Instances                : $detected"
Write-Host "Solved and verified      : $verified"
Write-Host "Checker rejected         : $rejected"
Write-Host "Technical failures       : $technicalFailures"
Write-Host "SOL005 warnings          : $($sol005Matches.Count)"
Write-Host "Max objective difference : $($maxObjectiveDifference.ToString('G17', [System.Globalization.CultureInfo]::InvariantCulture))"
Write-Host "Certification report     : $certificationPath"
Write-Host "RESULT                    : PASS" -ForegroundColor Green

exit 0
