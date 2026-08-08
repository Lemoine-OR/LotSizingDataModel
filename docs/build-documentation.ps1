[CmdletBinding()]
param()

$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path -Parent $PSScriptRoot
Set-Location $RepoRoot

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found on PATH."
    }
}

function Get-Override([object]$Root, [string]$ProjectName) {
    if ($null -eq $Root -or $null -eq $Root.projects) {
        return $null
    }
    $property = $Root.projects.PSObject.Properties[$ProjectName]
    if ($null -eq $property) {
        return $null
    }
    return $property.Value
}

function Escape-Html([string]$Value) {
    return [System.Net.WebUtility]::HtmlEncode($Value)
}

function Escape-Doxygen([string]$Value) {
    if ($null -eq $Value) { return "" }
    return $Value.Replace("\", "\\").Replace('"', '\"')
}

function Get-RepoRelativePath([string]$FullPath) {
    $relative = $FullPath.Substring($RepoRoot.Length)
    while ($relative.StartsWith("\") -or $relative.StartsWith("/")) {
        $relative = $relative.Substring(1)
    }
    return $relative.Replace("\", "/")
}

Require-Command "doxygen"
Require-Command "dot"
Require-Command "dotnet"

$doxygenVersion = (& doxygen --version).Trim()
Write-Host "Doxygen version: $doxygenVersion"

$dotVersion = cmd /c "dot -V 2>&1"
Write-Host $dotVersion

$VersionInfo = & (Join-Path $RepoRoot "tools\Get-LotSizingVersion.ps1")
$displayVersion = $VersionInfo.BuildVersionSimple
$commitShort = $VersionInfo.GitCommitIdShort

$overrideFile = Join-Path $RepoRoot "docs\project-overrides.json"
$overrides = Get-Content -LiteralPath $overrideFile -Raw | ConvertFrom-Json

$projectRecords = @()

$projectDirectories = Get-ChildItem -LiteralPath $RepoRoot -Directory |
    Where-Object { $_.Name -like "LotSizingDataModel.*" } |
    Sort-Object Name

foreach ($directory in $projectDirectories) {
    $projectFile = Join-Path $directory.FullName "$($directory.Name).csproj"
    if (-not (Test-Path -LiteralPath $projectFile)) {
        $candidate = Get-ChildItem -LiteralPath $directory.FullName -File -Filter "*.csproj" |
            Select-Object -First 1
        if ($null -eq $candidate) {
            continue
        }
        $projectFile = $candidate.FullName
    }

    $projectName = [IO.Path]::GetFileNameWithoutExtension($projectFile)
    $override = Get-Override $overrides $projectName

    $isPublic = $false
    $order = 1000
    $category = "Library"
    $cssClass = "library"
    $brief = "$projectName component of LotSizingDataModel."

    if ($null -ne $override) {
        if ($null -ne $override.public) { $isPublic = [bool]$override.public }
        if ($null -ne $override.order) { $order = [int]$override.order }
        if ($null -ne $override.category) { $category = [string]$override.category }
        if ($null -ne $override.cssClass) { $cssClass = [string]$override.cssClass }
        if ($null -ne $override.brief) { $brief = [string]$override.brief }
    }
    elseif ($projectName -like "LotSizingDataModel.Solver.*" -and
            $projectName -notin @(
                "LotSizingDataModel.Solver.Console",
                "LotSizingDataModel.Solver.Test",
                "LotSizingDataModel.Solver.Tests"
            )) {
        # Zero-configuration rule for future solver adapters.
        $isPublic = $true
        $order = 60
        $category = "Solver adapter"
        $cssClass = "adapter"
        $solverName = $projectName.Substring("LotSizingDataModel.Solver.".Length)
        $brief = "$solverName solver adapter for the LotSizingDataModel solver abstraction."
    }

    if (-not $isPublic) {
        continue
    }

    $slug = $projectName
    if ($slug.StartsWith("LotSizingDataModel.")) {
        $slug = $slug.Substring("LotSizingDataModel.".Length)
    }

    $projectRecords += [pscustomobject]@{
        ProjectName = $projectName
        ProjectFile = $projectFile
        ProjectDirectory = $directory.FullName
        Slug = $slug
        Order = $order
        Category = $category
        CssClass = $cssClass
        Brief = $brief
    }
}

$Projects = @($projectRecords | Sort-Object Order, ProjectName)

if ($Projects.Count -eq 0) {
    throw "No public Doxygen projects were discovered."
}

$Documentation = Join-Path $RepoRoot "Documentation"
$DoxygenOutput = Join-Path $Documentation "doxygen"
$Dist = Join-Path $Documentation "site"
$Temp = Join-Path $RepoRoot ".doxygen-tmp"

Remove-Item -LiteralPath $DoxygenOutput -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $Dist -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -LiteralPath $Temp -Recurse -Force -ErrorAction SilentlyContinue

New-Item -ItemType Directory -Force $DoxygenOutput | Out-Null
New-Item -ItemType Directory -Force $Dist | Out-Null
New-Item -ItemType Directory -Force $Temp | Out-Null

foreach ($project in $Projects) {
    $slug = $project.Slug
    $tempProject = Join-Path $Temp $slug
    New-Item -ItemType Directory -Force $tempProject | Out-Null

    $mainPagePath = Join-Path $tempProject "mainpage.md"
    $configPath = Join-Path $tempProject "Doxyfile"

    $mainPageRelative = Get-RepoRelativePath $mainPagePath
    $projectDirRelative = Get-RepoRelativePath $project.ProjectDirectory
    $outputRelative = "Documentation/doxygen/$slug"

    $mainPage = @"
# $($project.ProjectName)

**$($project.Brief)**

## Role in the solution

This component is part of the [**LotSizingDataModel**](../index.html) solution and is classified as **$($project.Category)**.

Current generated documentation version: **$displayVersion**  
Git commit: **$commitShort**

The API reference on this site is generated directly from the C# source code and XML documentation comments.
Each project site is intentionally self-contained to prevent ambiguous cross-project namespace links.

## Navigation

- [Back to the LotSizingDataModel documentation portal](../index.html)
- Browse the namespace, class and file trees from the navigation panel.
- Use the search box to locate types, members and concepts.

## Source

The source code is maintained in the [`Lemoine-OR/LotSizingDataModel`](https://github.com/Lemoine-OR/LotSizingDataModel) GitHub repository.

## Identity

- **Author:** David Lemoine
- **Organization:** Lemoine-OR
- **Version:** $displayVersion
- **Commit:** $commitShort
"@
    Set-Content -LiteralPath $mainPagePath -Value $mainPage -Encoding UTF8

    $projectNameEscaped = Escape-Doxygen $project.ProjectName
    $projectBriefEscaped = Escape-Doxygen $project.Brief
    $versionEscaped = Escape-Doxygen $displayVersion

    $doxy = @"
@INCLUDE_PATH = docs/doxygen
@INCLUDE = Doxyfile.common

PROJECT_NAME            = "$projectNameEscaped"
PROJECT_NUMBER          = "$versionEscaped"
PROJECT_BRIEF           = "$projectBriefEscaped"
INPUT                   = "$projectDirRelative" "$mainPageRelative"
OUTPUT_DIRECTORY        = "$outputRelative"
USE_MDFILE_AS_MAINPAGE  = "$mainPageRelative"
TAGFILES                =
GENERATE_TAGFILE        =
"@
    Set-Content -LiteralPath $configPath -Value $doxy -Encoding UTF8

    Write-Host ""
    Write-Host "Generating HTML documentation: $slug"

    & doxygen $configPath | Out-Host
    if ($LASTEXITCODE -ne 0) {
        throw "Doxygen generation failed for '$($project.ProjectName)'."
    }

    $htmlSource = Join-Path $DoxygenOutput "$slug\html"
    $index = Join-Path $htmlSource "index.html"
    if (-not (Test-Path -LiteralPath $index)) {
        throw "Expected Doxygen entry point was not generated: $index"
    }

    $siteTarget = Join-Path $Dist $slug
    New-Item -ItemType Directory -Force $siteTarget | Out-Null
    Copy-Item -Path "$htmlSource\*" -Destination $siteTarget -Recurse -Force
}

# Render the portal from the discovered project set.
$portalTemplate = Get-Content -LiteralPath (Join-Path $RepoRoot "docs\portal\index.html") -Raw
$cards = New-Object System.Text.StringBuilder

foreach ($project in $Projects) {
    $projectName = Escape-Html $project.ProjectName
    $brief = Escape-Html $project.Brief
    $category = Escape-Html $project.Category
    $slug = Escape-Html $project.Slug
    $cssClass = Escape-Html $project.CssClass

    [void]$cards.AppendLine("      <a class=`"project-card $cssClass`" href=`"$slug/index.html`">")
    [void]$cards.AppendLine("        <div class=`"card-icon`"><img src=`"assets/dll-icon-64.png`" alt=`"`"></div>")
    [void]$cards.AppendLine("        <div class=`"card-body`">")
    [void]$cards.AppendLine("          <div class=`"card-meta`">$category</div>")
    [void]$cards.AppendLine("          <h3>$projectName</h3>")
    [void]$cards.AppendLine("          <p>$brief</p>")
    [void]$cards.AppendLine("        </div>")
    [void]$cards.AppendLine("        <span class=`"arrow`" aria-hidden=`"true`">&rarr;</span>")
    [void]$cards.AppendLine("      </a>")
}

$portal = $portalTemplate.Replace("{{VERSION}}", (Escape-Html $displayVersion))
$portal = $portal.Replace("{{COMMIT}}", (Escape-Html $commitShort))
$portal = $portal.Replace("{{PROJECT_CARDS}}", $cards.ToString().TrimEnd())

Set-Content -LiteralPath (Join-Path $Dist "index.html") -Value $portal -Encoding UTF8
Copy-Item -LiteralPath (Join-Path $RepoRoot "docs\portal\styles.css") -Destination (Join-Path $Dist "styles.css") -Force

New-Item -ItemType Directory -Force (Join-Path $Dist "assets") | Out-Null
Copy-Item -Path (Join-Path $RepoRoot "docs\assets\*") -Destination (Join-Path $Dist "assets") -Recurse -Force
Copy-Item -LiteralPath (Join-Path $RepoRoot "docs\assets\dll-icon.ico") -Destination (Join-Path $Dist "favicon.ico") -Force
New-Item -ItemType File -Path (Join-Path $Dist ".nojekyll") -Force | Out-Null

# Structural validation.
$missing = @()
if (-not (Test-Path -LiteralPath (Join-Path $Dist "index.html"))) {
    $missing += "index.html"
}

foreach ($project in $Projects) {
    $entry = Join-Path $Dist "$($project.Slug)\index.html"
    if (-not (Test-Path -LiteralPath $entry)) {
        $missing += "$($project.Slug)/index.html"
    }
}

if ($missing.Count -gt 0) {
    throw "Documentation site is incomplete. Missing: $($missing -join ', ')"
}

& (Join-Path $RepoRoot "docs\Test-DocumentationLinks.ps1") -SiteRoot $Dist

Remove-Item -LiteralPath $Temp -Recurse -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "Documentation site successfully built and link-validated:" -ForegroundColor Green
Write-Host $Dist
Write-Host ""
Write-Host "Published projects:"
$Projects | Format-Table ProjectName, Category, Slug -AutoSize | Out-Host
