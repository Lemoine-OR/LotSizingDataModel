[CmdletBinding()]
param(
    [string]$Configuration = "Release",
    [Parameter(Mandatory = $true)]
    [string]$Destination
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$Destination = [System.IO.Path]::GetFullPath($Destination)
New-Item -ItemType Directory -Path $Destination -Force | Out-Null

$projects = @(
    "LotSizingDataModel.Solver.Gurobi",
    "LotSizingDataModel.Solver.Xpress",
    "LotSizingDataModel.Solver.CoinOrCbc"
)

$manifest = @()
foreach ($project in $projects) {
    $bin = Join-Path $root "$project\bin\$Configuration\net10.0"
    $dll = Join-Path $bin "$project.dll"
    if (-not (Test-Path -LiteralPath $dll -PathType Leaf)) {
        throw "Managed adapter DLL not found: $dll"
    }

    foreach ($ext in @(".dll", ".pdb", ".xml")) {
        $source = Join-Path $bin ($project + $ext)
        if (Test-Path -LiteralPath $source -PathType Leaf) {
            Copy-Item -LiteralPath $source -Destination $Destination -Force
            $copied = Join-Path $Destination (Split-Path -Leaf $source)
            $hash = (Get-FileHash -LiteralPath $copied -Algorithm SHA256).Hash.ToLowerInvariant()
            $manifest += "$hash  $(Split-Path -Leaf $copied)"
        }
    }
}

$forbidden = Get-ChildItem -LiteralPath $Destination -File | Where-Object {
    $_.Name -match '^(ILOG\.|gurobi|xprs|xpress|cbc\.exe|libcbc|coin).*' -or
    $_.Extension -in @('.lic', '.key', '.pfx', '.p12')
}
if ($forbidden) {
    throw "Forbidden third-party runtime/license artifact detected in release staging: $($forbidden.Name -join ', ')"
}

$manifestPath = Join-Path $Destination "multisolver-adapters.sha256.txt"
[System.IO.File]::WriteAllLines($manifestPath, $manifest, [System.Text.UTF8Encoding]::new($false))
Write-Host "Managed multi-solver artifacts staged in: $Destination" -ForegroundColor Green
Write-Host "Manifest: $manifestPath"
