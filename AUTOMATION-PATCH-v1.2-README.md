# LotSizingDataModel Automation Patch v1.2

Merge this patch at the repository root, replacing the existing files.

## Fixed PowerShell collection bug

The previous scripts used constructs such as:

```powershell
$results = New-Object System.Collections.Generic.List[object]
...
adapters = @($results)
```

PowerShell has a reproducible binder bug where `@($list)` can throw:

`System.ArgumentException: Argument types do not match`

for a `List[object]` created through `New-Object`.

v1.2 removes this risky pattern from every automation script.

Small automation collections now use ordinary PowerShell arrays:

```powershell
$results = @()
$results += [pscustomobject]@{ ... }
```

This is deliberately chosen for Windows PowerShell 5.1 compatibility. These
collections contain only a handful of projects/errors, so the array-append
performance cost is irrelevant.

Affected areas corrected:

- solver adapter discovery/build manifest;
- validated-binary assembly list;
- assembly metadata verification;
- versioning preflight diagnostics;
- dynamic Doxygen project discovery;
- Doxygen broken-link validation.

After merging, rerun:

```powershell
cd "D:\Dev\LotSizingDataModel"
powershell -ExecutionPolicy Bypass -File ".\build\Build-All.ps1"
```
