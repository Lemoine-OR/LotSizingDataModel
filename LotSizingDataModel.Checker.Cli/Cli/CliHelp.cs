namespace LotSizingDataModel.Checker.Cli;

internal static class CliHelp
{
    public const string Version = "1.0.0";

    public static void Write()
    {
        Console.WriteLine(
            "LotSizingDataModel generic solution checker CLI\n" +
            "\n" +
            "Usage:\n" +
            "  checker <directory> [options]\n" +
            "\n" +
            "Core options:\n" +
            "  -o, --output <dir>                 Report directory.\n" +
            "  -j, --parallelism <n>              Maximum concurrent checks.\n" +
            "      --level <level>                structural | feasibility | full.\n" +
            "      --search-pattern <pattern>     XML search pattern (default: *.xml).\n" +
            "      --no-recursive                 Do not scan subdirectories.\n" +
            "      --strict-xml                   Non-instance XML becomes a load failure.\n" +
            "      --no-reports                   Do not write campaign report files.\n" +
            "      --no-progress, --quiet         Disable progress output.\n" +
            "\n" +
            "Known-result filters:\n" +
            "      --result-id <id>               Exact KnownResult identifier.\n" +
            "      --result-name-contains <text>  Case-insensitive name substring.\n" +
            "\n" +
            "Numerical tolerances:\n" +
            "      --feasibility-tolerance <x>\n" +
            "      --zero-tolerance <x>\n" +
            "      --integrality-tolerance <x>\n" +
            "      --objective-absolute-tolerance <x>\n" +
            "      --objective-relative-tolerance <x>\n" +
            "\n" +
            "Checker policy:\n" +
            "      --continue-after-structural-errors\n" +
            "      --include-disabled-constraints\n" +
            "      --no-apply-evaluation\n" +
            "      --no-update-known-result\n" +
            "      --no-promote-known-result\n" +
            "      --evaluator-name <name>\n" +
            "      --evaluator-version <version>\n" +
            "\n" +
            "Report policy:\n" +
            "      --max-issues <n>               0 means unlimited.\n" +
            "      --no-info                      Hide informational issue details.\n" +
            "      --no-warnings                  Hide warning issue details.\n" +
            "\n" +
            "Other:\n" +
            "  -h, --help                         Show this help.\n" +
            "      --version                      Show CLI version.\n" +
            "\n" +
            "Exit codes:\n" +
            "  0  All selected candidates are valid and execution succeeded.\n" +
            "  1  At least one checked candidate is invalid.\n" +
            "  2  File loading or candidate execution failed.\n" +
            "  3  Invalid command-line arguments or campaign configuration.\n" +
            "  4  Cancelled by the user.\n" +
            "  5  Unexpected application failure.\n" +
            "\n" +
            "Examples:\n" +
            "  checker C:\\Benchmarks\\NouveauFormat\n" +
            "  checker ./instances --level feasibility -j 4\n" +
            "  checker ./instances --result-name-contains CPLEX --no-info\n" +
            "  checker ./instances --objective-abs-tol=1e-7 --no-progress");
    }
}
