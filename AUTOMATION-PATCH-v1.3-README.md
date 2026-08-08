# LotSizingDataModel Automation Patch v1.3

This patch fixes MSBuild error **MSB4092** in `Directory.Build.targets`.

## Root cause

The previous target tried to calculate the parent directory of
`$(LotSizingVersionOutput)` inside an MSBuild `Condition`. That path-property
expression was unnecessary and caused the MSBuild condition parser to fail.

## Fix

The version bridge is now deliberately minimal:

- `WriteLotSizingDataModelVersion` depends on NBGV `GetBuildVersion`;
- it only collects the NBGV version properties;
- it only writes them with `WriteLinesToFile`;
- it contains no path property function;
- it contains no `MakeDir`.

`tools/Get-LotSizingVersion.ps1` creates
`Documentation\versioning` before invoking the target.

## Test sequence

```powershell
cd "D:\Dev\LotSizingDataModel"

powershell -ExecutionPolicy Bypass -File ".\tools\Test-VersionTarget.ps1"

powershell -ExecutionPolicy Bypass -File ".\tools\Get-LotSizingVersion.ps1"
```

Only after both succeed:

```powershell
powershell -ExecutionPolicy Bypass -File ".\build\Build-All.ps1"
```
