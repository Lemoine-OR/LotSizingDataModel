using System.Globalization;
using LotSizingDataModel.Checker.Common;

namespace LotSizingDataModel.Checker.Cli;

internal static class CliOptionsParser
{
    private static readonly CultureInfo ParsingCulture =
        CultureInfo.InvariantCulture;

    public static CliParseResult Parse(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var options = new CliOptions();
        string? inputDirectory = null;
        bool endOfOptions = false;

        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index];

            if (endOfOptions)
            {
                if (!TrySetInputDirectory(argument, ref inputDirectory, out string? error))
                {
                    return Failure(error!);
                }

                continue;
            }

            if (argument == "--")
            {
                endOfOptions = true;
                continue;
            }

            if (argument is "-h" or "--help")
            {
                return new CliParseResult { ShowHelp = true };
            }

            if (argument == "--version")
            {
                return new CliParseResult { ShowVersion = true };
            }

            if (!argument.StartsWith("-", StringComparison.Ordinal) ||
                argument == "-")
            {
                if (!TrySetInputDirectory(argument, ref inputDirectory, out string? error))
                {
                    return Failure(error!);
                }

                continue;
            }

            SplitOption(argument, out string optionName, out string? inlineValue);

            switch (optionName)
            {
                case "-o":
                case "--output":
                    if (!TryReadValue(args, ref index, inlineValue, optionName, out string? output, out string? outputError))
                    {
                        return Failure(outputError!);
                    }

                    options.OutputDirectory = output;
                    break;

                case "-j":
                case "--parallelism":
                    if (!TryReadPositiveInt(args, ref index, inlineValue, optionName, out int parallelism, out string? parallelismError))
                    {
                        return Failure(parallelismError!);
                    }

                    options.MaxDegreeOfParallelism = parallelism;
                    break;

                case "--level":
                    if (!TryReadValue(args, ref index, inlineValue, optionName, out string? levelValue, out string? levelError))
                    {
                        return Failure(levelError!);
                    }

                    if (!TryParseLevel(levelValue!, out SolutionCheckLevel level))
                    {
                        return Failure(
                            "Invalid --level value. Expected structural, feasibility, or full.");
                    }

                    options.CheckOptions.Level = level;
                    break;

                case "--search-pattern":
                    if (!TryReadValue(args, ref index, inlineValue, optionName, out string? searchPattern, out string? patternError))
                    {
                        return Failure(patternError!);
                    }

                    options.SearchPattern = searchPattern!;
                    break;

                case "--result-id":
                    if (!TryReadValue(args, ref index, inlineValue, optionName, out string? resultId, out string? resultIdError))
                    {
                        return Failure(resultIdError!);
                    }

                    options.KnownResultId = resultId;
                    break;

                case "--result-name-contains":
                    if (!TryReadValue(args, ref index, inlineValue, optionName, out string? resultName, out string? resultNameError))
                    {
                        return Failure(resultNameError!);
                    }

                    options.KnownResultNameContains = resultName;
                    break;

                case "--feasibility-tolerance":
                case "--feasibility-tol":
                    if (!TryReadTolerance(args, ref index, inlineValue, optionName, out double feasibilityTolerance, out string? feasibilityError))
                    {
                        return Failure(feasibilityError!);
                    }

                    options.CheckOptions.FeasibilityTolerance = feasibilityTolerance;
                    break;

                case "--zero-tolerance":
                case "--zero-tol":
                    if (!TryReadTolerance(args, ref index, inlineValue, optionName, out double zeroTolerance, out string? zeroError))
                    {
                        return Failure(zeroError!);
                    }

                    options.CheckOptions.ZeroTolerance = zeroTolerance;
                    break;

                case "--integrality-tolerance":
                case "--integrality-tol":
                    if (!TryReadTolerance(args, ref index, inlineValue, optionName, out double integralityTolerance, out string? integralityError))
                    {
                        return Failure(integralityError!);
                    }

                    options.CheckOptions.IntegralityTolerance = integralityTolerance;
                    break;

                case "--objective-absolute-tolerance":
                case "--objective-abs-tol":
                    if (!TryReadTolerance(args, ref index, inlineValue, optionName, out double objectiveAbsoluteTolerance, out string? objectiveAbsoluteError))
                    {
                        return Failure(objectiveAbsoluteError!);
                    }

                    options.CheckOptions.ObjectiveAbsoluteTolerance = objectiveAbsoluteTolerance;
                    break;

                case "--objective-relative-tolerance":
                case "--objective-rel-tol":
                    if (!TryReadTolerance(args, ref index, inlineValue, optionName, out double objectiveRelativeTolerance, out string? objectiveRelativeError))
                    {
                        return Failure(objectiveRelativeError!);
                    }

                    options.CheckOptions.ObjectiveRelativeTolerance = objectiveRelativeTolerance;
                    break;

                case "--max-issues":
                    if (!TryReadNonNegativeInt(args, ref index, inlineValue, optionName, out int maximumIssues, out string? maximumIssuesError))
                    {
                        return Failure(maximumIssuesError!);
                    }

                    options.ReportOptions.MaximumDetailedIssues = maximumIssues;
                    break;

                case "--evaluator-name":
                    if (!TryReadValue(args, ref index, inlineValue, optionName, out string? evaluatorName, out string? evaluatorNameError))
                    {
                        return Failure(evaluatorNameError!);
                    }

                    options.VerificationOptions.EvaluatorName = evaluatorName!;
                    break;

                case "--evaluator-version":
                    if (!TryReadValue(args, ref index, inlineValue, optionName, out string? evaluatorVersion, out string? evaluatorVersionError))
                    {
                        return Failure(evaluatorVersionError!);
                    }

                    options.VerificationOptions.EvaluatorVersion = evaluatorVersion!;
                    break;

                case "--no-recursive":
                    if (!RejectInlineValue(inlineValue, optionName, out string? noRecursiveError))
                    {
                        return Failure(noRecursiveError!);
                    }

                    options.SearchSubdirectories = false;
                    break;

                case "--strict-xml":
                    if (!RejectInlineValue(inlineValue, optionName, out string? strictXmlError))
                    {
                        return Failure(strictXmlError!);
                    }

                    options.IgnoreNonLotSizingInstanceXml = false;
                    break;

                case "--no-reports":
                    if (!RejectInlineValue(inlineValue, optionName, out string? noReportsError))
                    {
                        return Failure(noReportsError!);
                    }

                    options.WriteReports = false;
                    break;

                case "--no-progress":
                case "--quiet":
                    if (!RejectInlineValue(inlineValue, optionName, out string? noProgressError))
                    {
                        return Failure(noProgressError!);
                    }

                    options.ShowProgress = false;
                    break;

                case "--continue-after-structural-errors":
                    if (!RejectInlineValue(inlineValue, optionName, out string? continueError))
                    {
                        return Failure(continueError!);
                    }

                    options.CheckOptions.ContinueAfterStructuralErrors = true;
                    break;

                case "--include-disabled-constraints":
                    if (!RejectInlineValue(inlineValue, optionName, out string? disabledError))
                    {
                        return Failure(disabledError!);
                    }

                    options.CheckOptions.IgnoreDisabledConstraints = false;
                    break;

                case "--no-apply-evaluation":
                    if (!RejectInlineValue(inlineValue, optionName, out string? applyError))
                    {
                        return Failure(applyError!);
                    }

                    options.VerificationOptions.ApplyToSolutionEvaluation = false;
                    break;

                case "--no-update-known-result":
                    if (!RejectInlineValue(inlineValue, optionName, out string? updateError))
                    {
                        return Failure(updateError!);
                    }

                    options.VerificationOptions.UpdateKnownResultFeasibility = false;
                    break;

                case "--no-promote-known-result":
                    if (!RejectInlineValue(inlineValue, optionName, out string? promoteError))
                    {
                        return Failure(promoteError!);
                    }

                    options.VerificationOptions.PromoteFullyVerifiedKnownResult = false;
                    break;

                case "--no-info":
                    if (!RejectInlineValue(inlineValue, optionName, out string? infoError))
                    {
                        return Failure(infoError!);
                    }

                    options.ReportOptions.IncludeInformationIssues = false;
                    break;

                case "--no-warnings":
                    if (!RejectInlineValue(inlineValue, optionName, out string? warningsError))
                    {
                        return Failure(warningsError!);
                    }

                    options.ReportOptions.IncludeWarningIssues = false;
                    break;

                default:
                    return Failure($"Unknown option '{optionName}'. Use --help for usage.");
            }
        }

        if (string.IsNullOrWhiteSpace(inputDirectory))
        {
            return Failure("An input directory is required. Use --help for usage.");
        }

        options.InputDirectory = inputDirectory;

        try
        {
            options.BuildCampaignOptions();
        }
        catch (Exception exception) when (
            exception is ArgumentException or InvalidOperationException)
        {
            return Failure(exception.Message);
        }

        return new CliParseResult { Options = options };
    }

    private static void SplitOption(
        string argument,
        out string optionName,
        out string? inlineValue)
    {
        int equalsIndex = argument.IndexOf('=');

        if (equalsIndex < 0)
        {
            optionName = argument;
            inlineValue = null;
            return;
        }

        optionName = argument[..equalsIndex];
        inlineValue = argument[(equalsIndex + 1)..];
    }

    private static bool TrySetInputDirectory(
        string value,
        ref string? inputDirectory,
        out string? error)
    {
        if (inputDirectory is not null)
        {
            error =
                $"Unexpected positional argument '{value}'. Only one input directory is allowed.";
            return false;
        }

        inputDirectory = value;
        error = null;
        return true;
    }

    private static bool TryReadValue(
        string[] args,
        ref int index,
        string? inlineValue,
        string optionName,
        out string? value,
        out string? error)
    {
        if (inlineValue is not null)
        {
            if (inlineValue.Length == 0)
            {
                value = null;
                error = $"Option '{optionName}' requires a non-empty value.";
                return false;
            }

            value = inlineValue;
            error = null;
            return true;
        }

        if (index + 1 >= args.Length)
        {
            value = null;
            error = $"Option '{optionName}' requires a value.";
            return false;
        }

        index++;
        value = args[index];

        if (string.IsNullOrWhiteSpace(value))
        {
            error = $"Option '{optionName}' requires a non-empty value.";
            return false;
        }

        error = null;
        return true;
    }

    private static bool TryReadPositiveInt(
        string[] args,
        ref int index,
        string? inlineValue,
        string optionName,
        out int value,
        out string? error)
    {
        if (!TryReadValue(args, ref index, inlineValue, optionName, out string? text, out error))
        {
            value = 0;
            return false;
        }

        if (!int.TryParse(text, NumberStyles.Integer, ParsingCulture, out value) || value < 1)
        {
            error = $"Option '{optionName}' requires an integer greater than or equal to 1.";
            return false;
        }

        return true;
    }

    private static bool TryReadNonNegativeInt(
        string[] args,
        ref int index,
        string? inlineValue,
        string optionName,
        out int value,
        out string? error)
    {
        if (!TryReadValue(args, ref index, inlineValue, optionName, out string? text, out error))
        {
            value = 0;
            return false;
        }

        if (!int.TryParse(text, NumberStyles.Integer, ParsingCulture, out value) || value < 0)
        {
            error = $"Option '{optionName}' requires a non-negative integer.";
            return false;
        }

        return true;
    }

    private static bool TryReadTolerance(
        string[] args,
        ref int index,
        string? inlineValue,
        string optionName,
        out double value,
        out string? error)
    {
        if (!TryReadValue(args, ref index, inlineValue, optionName, out string? text, out error))
        {
            value = 0.0;
            return false;
        }

        if (!double.TryParse(text, NumberStyles.Float, ParsingCulture, out value) ||
            !double.IsFinite(value) ||
            value < 0.0)
        {
            error =
                $"Option '{optionName}' requires a finite non-negative number using invariant notation.";
            return false;
        }

        return true;
    }

    private static bool TryParseLevel(
        string value,
        out SolutionCheckLevel level)
    {
        switch (value.Trim().ToLowerInvariant())
        {
            case "structural":
            case "structure":
                level = SolutionCheckLevel.Structural;
                return true;

            case "feasibility":
            case "feasible":
                level = SolutionCheckLevel.Feasibility;
                return true;

            case "full":
                level = SolutionCheckLevel.Full;
                return true;

            default:
                level = default;
                return false;
        }
    }

    private static bool RejectInlineValue(
        string? inlineValue,
        string optionName,
        out string? error)
    {
        if (inlineValue is null)
        {
            error = null;
            return true;
        }

        error = $"Flag '{optionName}' does not accept a value.";
        return false;
    }

    private static CliParseResult Failure(string message)
    {
        return new CliParseResult { ErrorMessage = message };
    }
}
