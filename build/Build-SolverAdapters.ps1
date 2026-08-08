[CmdletBinding()]
param(
    [switch]$IncludeExternal,
    [string]$Configuration = "Release",
    [string]$ManifestPath
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

if ([string]::IsNullOrWhiteSpace($ManifestPath)) {
    $ManifestPath = Join-Path $RepoRoot "Documentation\artifacts\solver-adapters.json"
}

$profileFile = Join-Path $PSScriptRoot "solver-build-profiles.json"
$profileRoot = $null
if (Test-Path -LiteralPath $profileFile) {
    $profileRoot = Get-Content -LiteralPath $profileFile -Raw | ConvertFrom-Json
}

function Get-ProjectProfile([string]$ProjectName) {
    if ($null -eq $profileRoot -or $null -eq $profileRoot.projects) {
        return $null
    }

    $property = $profileRoot.projects.PSObject.Properties[$ProjectName]
    if ($null -eq $property) {
        return $null
    }

    return $property.Value
}

function Find-ProjectAssembly([string]$ProjectDirectory, [string]$ProjectName) {
    $releaseRoot = Join-Path $ProjectDirectory "bin\$Configuration"
    if (-not (Test-Path -LiteralPath $releaseRoot)) {
        return $null
    }

    $assemblyName = $ProjectName
    $projectFile = Join-Path $ProjectDirectory "$ProjectName.csproj"

    if (Test-Path -LiteralPath $projectFile) {
        try {
            [xml]$xml = Get-Content -LiteralPath $projectFile -Raw
            $candidateNames = @()
            foreach ($group in $xml.Project.PropertyGroup) {
                if ($null -ne $group.AssemblyName -and
                    -not [string]::IsNullOrWhiteSpace([string]$group.AssemblyName)) {
                    $candidateNames += [string]$group.AssemblyName
                }
            }
            if ($candidateNames.Count -gt 0) {
                $assemblyName = $candidateNames[0]
            }
        }
        catch {
            # Conventional project-name assembly remains the fallback.
        }
    }

    $matches = Get-ChildItem -LiteralPath $releaseRoot -Recurse -File -Filter "$assemblyName.dll" |
        Sort-Object @{Expression = { $_.FullName.Length }; Ascending = $true},
                    @{Expression = { $_.LastWriteTimeUtc }; Ascending = $false}

    if ($matches.Count -eq 0 -and $assemblyName -ne $ProjectName) {
        $matches = Get-ChildItem -LiteralPath $releaseRoot -Recurse -File -Filter "$ProjectName.dll" |
            Sort-Object @{Expression = { $_.FullName.Length }; Ascending = $true},
                        @{Expression = { $_.LastWriteTimeUtc }; Ascending = $false}
    }

    if ($matches.Count -eq 0) {
        return $null
    }

    return $matches[0].FullName
}

$adapterDirs = Get-ChildItem -LiteralPath $RepoRoot -Directory |
    Where-Object {
        $_.Name -like "LotSizingDataModel.Solver.*" -and
        $_.Name -notin @(
            "LotSizingDataModel.Solver.Console",
            "LotSizingDataModel.Solver.Test",
            "LotSizingDataModel.Solver.Tests"
        )
    } |
    Sort-Object Name

$results = @()

foreach ($directory in $adapterDirs) {
    $projectName = $directory.Name
    $projectFile = Join-Path $directory.FullName "$projectName.csproj"

    if (-not (Test-Path -LiteralPath $projectFile)) {
        $candidate = Get-ChildItem -LiteralPath $directory.FullName -File -Filter "*.csproj" |
            Select-Object -First 1
        if ($null -eq $candidate) {
            Write-Warning "No .csproj found in $($directory.FullName); skipping."
            continue
        }
        $projectFile = $candidate.FullName
        $projectName = [IO.Path]::GetFileNameWithoutExtension($candidate.Name)
    }

    $profile = Get-ProjectProfile $projectName
    $external = $false
    $ciBuild = $true
    $reason = ""

    if ($null -ne $profile) {
        if ($null -ne $profile.external) {
            $external = [bool]$profile.external
        }
        if ($null -ne $profile.ciBuild) {
            $ciBuild = [bool]$profile.ciBuild
        }
        if ($null -ne $profile.reason) {
            $reason = [string]$profile.reason
        }
    }

    $shouldBuild = $ciBuild
    if ($IncludeExternal -and $external) {
        $shouldBuild = $true
    }

    if (-not $shouldBuild) {
        Write-Host "Skipping external solver adapter: $projectName"
        if (-not [string]::IsNullOrWhiteSpace($reason)) {
            Write-Host "  $reason"
        }

        $results += [pscustomobject]@{
            project = $projectName
            status = "skipped"
            external = $external
            reason = $reason
            assembly = $null
        }
        continue
    }

    Write-Host ""
    Write-Host "Restoring solver adapter: $projectName"
    & dotnet restore $projectFile | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Restore failed for solver adapter '$projectName'. If it requires a proprietary SDK, add a profile entry in build/solver-build-profiles.json."
    }

    Write-Host "Building solver adapter: $projectName"
    & dotnet build $projectFile -c $Configuration --no-restore | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Build failed for solver adapter '$projectName'. If it requires a proprietary SDK, add a profile entry in build/solver-build-profiles.json."
    }

    $assembly = Find-ProjectAssembly $directory.FullName $projectName
    if ([string]::IsNullOrWhiteSpace($assembly)) {
        throw "Could not locate the built primary assembly for solver adapter '$projectName'."
    }

    $results += [pscustomobject]@{
        project = $projectName
        status = "built"
        external = $external
        reason = ""
        assembly = $assembly
    }
}

$manifestDirectory = Split-Path -Parent $ManifestPath
New-Item -ItemType Directory -Force $manifestDirectory | Out-Null

$manifest = [pscustomobject]@{
    generatedAtUtc = [DateTime]::UtcNow.ToString("o")
    configuration = $Configuration
    includeExternal = [bool]$IncludeExternal
    adapters = $results
}

$manifest | ConvertTo-Json -Depth 6 |
    Set-Content -LiteralPath $ManifestPath -Encoding UTF8

Write-Host ""
Write-Host "Solver adapter manifest: $ManifestPath"
$results | Format-Table project, status, external, assembly -AutoSize | Out-Host

return $results
