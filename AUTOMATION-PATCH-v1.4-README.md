# LotSizingDataModel Automation Patch v1.4

This is a cumulative corrective patch for the automation layer.

It repairs two source-code corruptions introduced by the v1.2 automated regex
transformation and adds an actual PowerShell parser gate.

## Fixed

### `tools/Verify-AssemblyMetadata.ps1`

The malformed interpolation around `CompanyName` and `ProductName` has been
removed. Messages now use PowerShell's `-f` format operator, avoiding fragile
nested quote/subexpression combinations.

### `docs/build-documentation.ps1`

The malformed expression:

```powershell
$missing += "$($project.Slug/index.html")
```

is corrected to:

```powershell
$missing += "$($project.Slug)/index.html"
```

### Repository-wide syntax validation

`tools/Test-PowerShellSyntax.ps1` uses the PowerShell engine's own parser:

`System.Management.Automation.Language.Parser::ParseFile`

to parse every repository `.ps1`.

Both `Build-Validated.ps1` and `Build-All.ps1` run this syntax check before
long-running build/test/package/documentation work.

## Test after merge

```powershell
cd "D:\Dev\LotSizingDataModel"

powershell -ExecutionPolicy Bypass -File ".\tools\Test-Automation.ps1"

powershell -ExecutionPolicy Bypass -File ".\tools\Get-LotSizingVersion.ps1"

powershell -ExecutionPolicy Bypass -File ".\build\Build-All.ps1"
```
