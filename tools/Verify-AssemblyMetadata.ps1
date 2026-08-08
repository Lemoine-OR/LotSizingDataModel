[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string[]]$DllPath,

    [string]$ExpectedCompany = "Lemoine-OR",
    [string]$ExpectedProduct = "LotSizingDataModel",
    [string]$ExpectedAuthor = "David Lemoine",

    [switch]$SkipIconCheck
)

$ErrorActionPreference = "Stop"

if (-not $SkipIconCheck -and -not ("LotSizingDataModel.NativeResourceInspector" -as [type])) {
    Add-Type -TypeDefinition @"
using System;
using System.Runtime.InteropServices;

namespace LotSizingDataModel
{
    public static class NativeResourceInspector
    {
        private const uint LOAD_LIBRARY_AS_DATAFILE = 0x00000002;
        private const uint LOAD_LIBRARY_AS_IMAGE_RESOURCE = 0x00000020;
        private static readonly IntPtr RT_GROUP_ICON = new IntPtr(14);

        private delegate bool EnumResNameProc(
            IntPtr hModule,
            IntPtr lpszType,
            IntPtr lpszName,
            IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern IntPtr LoadLibraryEx(
            string lpFileName,
            IntPtr hFile,
            uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool EnumResourceNames(
            IntPtr hModule,
            IntPtr lpszType,
            EnumResNameProc lpEnumFunc,
            IntPtr lParam);

        public static bool HasGroupIcon(string fileName)
        {
            IntPtr module = LoadLibraryEx(
                fileName,
                IntPtr.Zero,
                LOAD_LIBRARY_AS_DATAFILE | LOAD_LIBRARY_AS_IMAGE_RESOURCE);

            if (module == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                bool found = false;
                EnumResNameProc callback = delegate(
                    IntPtr h,
                    IntPtr type,
                    IntPtr name,
                    IntPtr data)
                {
                    found = true;
                    return false;
                };

                EnumResourceNames(module, RT_GROUP_ICON, callback, IntPtr.Zero);
                GC.KeepAlive(callback);
                return found;
            }
            finally
            {
                FreeLibrary(module);
            }
        }
    }
}
"@
}

$errors = New-Object System.Collections.Generic.List[string]
$results = New-Object System.Collections.Generic.List[object]

foreach ($inputPath in $DllPath) {
    $resolved = Resolve-Path -LiteralPath $inputPath -ErrorAction Stop
    $path = $resolved.Path
    $info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($path)

    $iconPresent = $true
    if (-not $SkipIconCheck) {
        $iconPresent = [LotSizingDataModel.NativeResourceInspector]::HasGroupIcon($path)
    }

    if ($info.CompanyName -ne $ExpectedCompany) {
        $errors.Add("$path : CompanyName='$($info.CompanyName)' (expected '$ExpectedCompany').")
    }

    if ($info.ProductName -ne $ExpectedProduct) {
        $errors.Add("$path : ProductName='$($info.ProductName)' (expected '$ExpectedProduct').")
    }

    if ([string]::IsNullOrWhiteSpace($info.LegalCopyright) -or
        $info.LegalCopyright.IndexOf($ExpectedAuthor, [StringComparison]::OrdinalIgnoreCase) -lt 0) {
        $errors.Add("$path : LegalCopyright does not contain '$ExpectedAuthor'.")
    }

    if ([string]::IsNullOrWhiteSpace($info.FileVersion)) {
        $errors.Add("$path : FileVersion is empty.")
    }

    if ([string]::IsNullOrWhiteSpace($info.ProductVersion)) {
        $errors.Add("$path : ProductVersion is empty.")
    }

    if (-not $SkipIconCheck -and -not $iconPresent) {
        $errors.Add("$path : no embedded Win32 group icon was found.")
    }

    $results.Add([pscustomobject]@{
        File = [IO.Path]::GetFileName($path)
        Company = $info.CompanyName
        Product = $info.ProductName
        FileVersion = $info.FileVersion
        ProductVersion = $info.ProductVersion
        Copyright = $info.LegalCopyright
        Icon = $iconPresent
    })
}

$results | Format-Table -AutoSize | Out-Host

if ($errors.Count -gt 0) {
    Write-Host ""
    $errors | ForEach-Object { Write-Host $_ -ForegroundColor Red }
    throw "Assembly metadata verification failed with $($errors.Count) error(s)."
}

Write-Host "Assembly metadata and version verification passed." -ForegroundColor Green
if (-not $SkipIconCheck) {
    Write-Host "Embedded Win32 icon verification passed." -ForegroundColor Green
}
