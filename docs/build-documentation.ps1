[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

$Projects = @(
    @{ Slug = "Core"; Config = "docs/doxygen/Core/Doxyfile" }
    @{ Slug = "Solution"; Config = "docs/doxygen/Solution/Doxyfile" }
    @{ Slug = "Instance"; Config = "docs/doxygen/Instance/Doxyfile" }
    @{ Slug = "Solver"; Config = "docs/doxygen/Solver/Doxyfile" }
    @{ Slug = "Solver.Cplex"; Config = "docs/doxygen/Solver.Cplex/Doxyfile" }
    @{ Slug = "Solver.Console"; Config = "docs/doxygen/Solver.Console/Doxyfile" }
    @{ Slug = "Checker"; Config = "docs/doxygen/Checker/Doxyfile" }
    @{ Slug = "Checker.Cli"; Config = "docs/doxygen/Checker.Cli/Doxyfile" }
)

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found on PATH."
    }
}

Require-Command "doxygen"
Require-Command "dot"

$doxygenVersion = (& doxygen --version).Trim()
Write-Host "Doxygen version: $doxygenVersion"

$dotVersion = cmd /c "dot -V 2>&1"
Write-Host $dotVersion

$Documentation = Join-Path $RepoRoot "Documentation"
$Dist = Join-Path $Documentation "site"
$Temp = Join-Path $RepoRoot ".doxygen-tmp"

Remove-Item $Documentation -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $Temp -Recurse -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Force $Dist | Out-Null
New-Item -ItemType Directory -Force $Temp | Out-Null

# Generate each public project independently.
# Cross-project TAGFILES are deliberately disabled: multiple projects share the
# LotSizingDataModel root namespace and automatic tag-file linking can produce
# links to pages that do not exist in the target project.
foreach ($project in $Projects) {
    $slug = $project.Slug
    $sourceConfig = Join-Path $RepoRoot $project.Config
    $tempConfig = Join-Path $Temp "$slug.doxy"

    if (-not (Test-Path -LiteralPath $sourceConfig)) {
        throw "Doxygen configuration not found: $sourceConfig"
    }

    $content = Get-Content -LiteralPath $sourceConfig -Raw

    # Force a self-contained project documentation build.
    $content += @"

TAGFILES =
GENERATE_TAGFILE =
"@

    Set-Content -LiteralPath $tempConfig -Value $content -Encoding UTF8

    Write-Host ""
    Write-Host "Generating HTML documentation: $slug"

    & doxygen $tempConfig
    if ($LASTEXITCODE -ne 0) {
        throw "Doxygen HTML generation failed for $slug."
    }

    $htmlSource = Join-Path $Documentation "$slug/html"
    $index = Join-Path $htmlSource "index.html"

    if (-not (Test-Path -LiteralPath $index)) {
        throw "Expected Doxygen entry point was not generated: $index"
    }

    $siteTarget = Join-Path $Dist $slug
    New-Item -ItemType Directory -Force $siteTarget | Out-Null
    Copy-Item -Path "$htmlSource/*" -Destination $siteTarget -Recurse -Force
}

# Portal and shared assets.
Copy-Item -Path "docs/portal/*" -Destination $Dist -Recurse -Force

New-Item -ItemType Directory -Force (Join-Path $Dist "assets") | Out-Null
Copy-Item -Path "docs/assets/*" -Destination (Join-Path $Dist "assets") -Recurse -Force
Copy-Item -Path "docs/assets/dll-icon.ico" -Destination (Join-Path $Dist "favicon.ico") -Force

New-Item -ItemType File -Path (Join-Path $Dist ".nojekyll") -Force | Out-Null

# Structural validation.
$missing = @()

if (-not (Test-Path -LiteralPath (Join-Path $Dist "index.html"))) {
    $missing += "index.html"
}

foreach ($project in $Projects) {
    $entry = Join-Path $Dist "$($project.Slug)/index.html"
    if (-not (Test-Path -LiteralPath $entry)) {
        $missing += "$($project.Slug)/index.html"
    }
}

if ($missing.Count -gt 0) {
    throw "Documentation site is incomplete. Missing: $($missing -join ', ')"
}

# Exhaustive validation of local links in generated HTML.
$linkValidator = Join-Path $RepoRoot "docs/Test-DocumentationLinks.ps1"

if (-not (Test-Path -LiteralPath $linkValidator)) {
    throw "Documentation link validator is missing: $linkValidator"
}

& $linkValidator -SiteRoot $Dist

Remove-Item $Temp -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Documentation site successfully built and link-validated:"
Write-Host $Dist
