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

                EnumResourceNames(
                    module,
                    RT_GROUP_ICON,
                    callback,
                    IntPtr.Zero);

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

$errors = @()
$results = @()

foreach ($inputPath in $DllPath) {
    $resolved = Resolve-Path -LiteralPath $inputPath -ErrorAction Stop
    $path = $resolved.Path
    $info = [System.Diagnostics.FileVersionInfo]::GetVersionInfo($path)

    $iconPresent = $true

    if (-not $SkipIconCheck) {
        $iconPresent =
            [LotSizingDataModel.NativeResourceInspector]::HasGroupIcon($path)
    }

    if ($info.CompanyName -ne $ExpectedCompany) {
        $errors += (
            "{0} : CompanyName='{1}' (expected '{2}')." -f
            $path,
            $info.CompanyName,
            $ExpectedCompany
        )
    }

    if ($info.ProductName -ne $ExpectedProduct) {
        $errors += (
            "{0} : ProductName='{1}' (expected '{2}')." -f
            $path,
            $info.ProductName,
            $ExpectedProduct
        )
    }

    if (
        [string]::IsNullOrWhiteSpace($info.LegalCopyright) -or
        $info.LegalCopyright.IndexOf(
            $ExpectedAuthor,
            [StringComparison]::OrdinalIgnoreCase
        ) -lt 0
    ) {
        $errors += (
            "{0} : LegalCopyright does not contain '{1}'." -f
            $path,
            $ExpectedAuthor
        )
    }

    if ([string]::IsNullOrWhiteSpace($info.FileVersion)) {
        $errors += ("{0} : FileVersion is empty." -f $path)
    }

    if ([string]::IsNullOrWhiteSpace($info.ProductVersion)) {
        $errors += ("{0} : ProductVersion is empty." -f $path)
    }

    if (-not $SkipIconCheck -and -not $iconPresent) {
        $errors += (
            "{0} : no embedded Win32 group icon was found." -f $path
        )
    }

    $results += [pscustomobject]@{
        File = [IO.Path]::GetFileName($path)
        Company = $info.CompanyName
        Product = $info.ProductName
        FileVersion = $info.FileVersion
        ProductVersion = $info.ProductVersion
        Copyright = $info.LegalCopyright
        Icon = $iconPresent
    }
}

$results |
    Format-Table -AutoSize |
    Out-Host

if ($errors.Count -gt 0) {
    Write-Host ""

    foreach ($errorMessage in $errors) {
        Write-Host $errorMessage -ForegroundColor Red
    }

    throw (
        "Assembly metadata verification failed with {0} error(s)." -f
        $errors.Count
    )
}

Write-Host `
    "Assembly metadata and version verification passed." `
    -ForegroundColor Green

if (-not $SkipIconCheck) {
    Write-Host `
        "Embedded Win32 icon verification passed." `
        -ForegroundColor Green
}
