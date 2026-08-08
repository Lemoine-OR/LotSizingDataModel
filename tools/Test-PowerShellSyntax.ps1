[CmdletBinding()]
param(
    [switch]$Quiet
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot

$scriptFiles =
    Get-ChildItem -LiteralPath $RepoRoot -Recurse -File -Filter "*.ps1" |
    Where-Object {
        $_.FullName -notmatch '[\\/](bin|obj|Documentation|\.git)[\\/]'
    } |
    Sort-Object FullName

if ($scriptFiles.Count -eq 0) {
    throw "No PowerShell automation scripts were found."
}

$allErrors = @()

foreach ($scriptFile in $scriptFiles) {
    $tokens = $null
    $parseErrors = $null

    [void][System.Management.Automation.Language.Parser]::ParseFile(
        $scriptFile.FullName,
        [ref]$tokens,
        [ref]$parseErrors
    )

    if ($parseErrors.Count -gt 0) {
        foreach ($parseError in $parseErrors) {
            $allErrors += [pscustomobject]@{
                File = $scriptFile.FullName
                Line = $parseError.Extent.StartLineNumber
                Column = $parseError.Extent.StartColumnNumber
                Error = $parseError.Message
                Text = $parseError.Extent.Text
            }
        }
    }
}

if ($allErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "PowerShell syntax errors detected:" -ForegroundColor Red

    $allErrors |
        Format-Table File, Line, Column, Error, Text -AutoSize |
        Out-String -Width 300 |
        Write-Host

    throw (
        "PowerShell syntax validation failed with {0} parser error(s)." -f
        $allErrors.Count
    )
}

if (-not $Quiet) {
    Write-Host (
        "PowerShell syntax validation passed: {0} script(s) parsed successfully." -f
        $scriptFiles.Count
    ) -ForegroundColor Green
}

[pscustomobject]@{
    ScriptsParsed = $scriptFiles.Count
    Errors = 0
}
