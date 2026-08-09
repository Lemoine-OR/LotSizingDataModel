using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using LotSizingDataModel.Solver.Adapters;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Configuration;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.External;
using LotSizingDataModel.Solver.Modeling;
using LotSizingDataModel.Solver.Monitoring;

namespace LotSizingDataModel.Solver.CoinOrCbc;

/// <summary>
/// Solves generic linear and mixed-integer linear models with the COIN-OR CBC
/// stand-alone executable.
/// </summary>
/// <remarks>
/// The adapter intentionally uses the official stand-alone CBC executable and
/// therefore has no managed package or native-DLL compile-time dependency.
/// </remarks>
public sealed partial class CoinOrCbcSolverAdapter :
    MathematicalModelSolverPluginBase
{
    private const string ExplicitExecutableEnvironmentVariable =
        "LOTSIZING_CBC_EXECUTABLE";

    private readonly ExternalSolverProcessRunner _processRunner =
        new();

    private string _cachedExecutablePath =
        string.Empty;

    private string _cachedVersion =
        string.Empty;

    /// <summary>
    /// Initializes the COIN-OR CBC adapter.
    /// </summary>
    public CoinOrCbcSolverAdapter()
        : base(
        [
            SolverCapability.LinearProgramming,
            SolverCapability.MixedIntegerLinearProgramming,
            SolverCapability.Interruption,
            SolverCapability.LpExport,
            SolverCapability.OptimalityGapReporting,
            SolverCapability.SearchStatistics
        ])
    {
    }

    /// <inheritdoc />
    public override SolverKind SolverKind =>
        SolverKind.CoinOrCbc;

    /// <inheritdoc />
    public override string SolverName =>
        "COIN-OR CBC";

    /// <inheritdoc />
    public override string SolverVersion =>
        _cachedVersion;

    /// <inheritdoc />
    public override string AdapterId =>
        "LotSizingDataModel.Solver.CoinOrCbc";

    /// <inheritdoc />
    public override string AdapterName =>
        "LotSizingDataModel COIN-OR CBC Adapter";

    /// <inheritdoc />
    public override string AdapterVersion =>
        "1.0.0";

    /// <inheritdoc />
    public override string MinimumSupportedSolverVersion =>
        "2.10";

    /// <inheritdoc />
    public override async ValueTask<SolverAvailabilityInfo>
        CheckAvailabilityAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string executablePath =
            ResolveExecutablePath();

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            var notInstalled =
                new SolverAvailabilityInfo(
                    SolverKind.CoinOrCbc,
                    SolverAvailabilityStatus.NotInstalled)
                {
                    SolverName = SolverName
                };

            notInstalled.AddDiagnostic(
                "cbc executable was not found. Put cbc on PATH, define " +
                "CBC_HOME or COINOR_HOME, or set " +
                $"{ExplicitExecutableEnvironmentVariable}.");

            return notInstalled;
        }

        try
        {
            string workingDirectory =
                Path.GetDirectoryName(executablePath) ??
                Environment.CurrentDirectory;

            ExternalSolverProcessResult processResult =
                await _processRunner.RunAsync(
                    executablePath,
                    ["-quit"],
                    workingDirectory,
                    standardInput: null,
                    cancellationToken);

            if (processResult.WasCancelled)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            string output =
                JoinOutput(processResult);

            if (processResult.ExitCode != 0)
            {
                var failed =
                    new SolverAvailabilityInfo(
                        SolverKind.CoinOrCbc,
                        SolverAvailabilityStatus.LoadFailure)
                    {
                        SolverName = SolverName
                    };

                failed.AddDiagnostic(
                    FirstMeaningfulLine(output) ??
                    $"cbc exited with code {processResult.ExitCode}.");

                return failed;
            }

            _cachedExecutablePath =
                executablePath;

            _cachedVersion =
                ParseVersion(output);

            var available =
                new SolverAvailabilityInfo(
                    SolverKind.CoinOrCbc,
                    SolverAvailabilityStatus.Available)
                {
                    SolverName = SolverName,
                    SolverVersion = _cachedVersion
                };

            available.AddDiagnostic(
                $"CBC executable detected at '{executablePath}'.");

            if (!string.IsNullOrWhiteSpace(output))
            {
                available.AddDiagnostic(
                    FirstMeaningfulLine(output) ?? output.Trim());
            }

            return available;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            var failed =
                new SolverAvailabilityInfo(
                    SolverKind.CoinOrCbc,
                    SolverAvailabilityStatus.LoadFailure)
                {
                    SolverName = SolverName
                };

            failed.AddDiagnostic(exception.Message);
            return failed;
        }
    }

    /// <inheritdoc />
    protected override async ValueTask<MathematicalModelSolveResult>
        SolveCoreAsync(
            MathematicalModelSolveRequest request,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        request.Parameters.EnsureValid();

        string executablePath =
            !string.IsNullOrWhiteSpace(_cachedExecutablePath) &&
            File.Exists(_cachedExecutablePath)
                ? _cachedExecutablePath
                : ResolveExecutablePath();

        if (string.IsNullOrWhiteSpace(executablePath))
        {
            throw new InvalidOperationException(
                "COIN-OR CBC is not available. Call solver discovery/" +
                "availability before attempting to solve.");
        }

        string temporaryDirectory =
            ExternalSolverResultUtilities.CreateTemporaryDirectory(
                "cbc");

        string modelPath =
            Path.Combine(
                temporaryDirectory,
                "model.lp");

        string solutionPath =
            Path.Combine(
                temporaryDirectory,
                "solution.sol");

        var stopwatch =
            Stopwatch.StartNew();

        try
        {
            await PublishProgressAsync(
                request,
                new SolverProgressSnapshot
                {
                    Stage = SolverProgressStage.BuildingModel,
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                    Message = "Writing the portable LP model for COIN-OR CBC."
                },
                cancellationToken);

            new PortableLpModelWriter().Write(
                request.Model,
                modelPath);

            if (request.Parameters.ExportModel)
            {
                ExternalSolverResultUtilities.ExportPortableModel(
                    modelPath,
                    request.Parameters.ExportModelPath);
            }

            IReadOnlyList<string> parameterDiagnostics;
            IReadOnlyList<string> arguments =
                BuildArguments(
                    request.Parameters,
                    modelPath,
                    solutionPath,
                    out parameterDiagnostics);

            await PublishProgressAsync(
                request,
                new SolverProgressSnapshot
                {
                    Stage = ResolveOptimizationProgressStage(),
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                    Message = "COIN-OR CBC optimization started."
                },
                cancellationToken);

            ExternalSolverProcessResult processResult =
                await _processRunner.RunAsync(
                    executablePath,
                    arguments,
                    temporaryDirectory,
                    standardInput: null,
                    cancellationToken);

            stopwatch.Stop();

            string output =
                JoinOutput(processResult);

            var values =
                new Dictionary<int, double>(
                    NamedSolutionValueParser.ParseFile(solutionPath));

            bool solutionFileExists =
                File.Exists(solutionPath);

            bool hasSolution =
                solutionFileExists &&
                !SolutionFileSaysNoSolution(solutionPath);

            if (hasSolution)
            {
                // CBC solution output can be sparse depending on printing
                // options. An omitted column represents a zero activity.
                foreach (MathematicalVariable variable in request.Model.Variables)
                {
                    values.TryAdd(
                        variable.Id,
                        0.0);
                }
            }

            var result =
                new MathematicalModelSolveResult
                {
                    SolverKind = SolverKind.CoinOrCbc,
                    SolverName = SolverName,
                    SolverVersion =
                        !string.IsNullOrWhiteSpace(_cachedVersion)
                            ? _cachedVersion
                            : ParseVersion(output),
                    SolveDuration = stopwatch.Elapsed,
                    HasFeasibleSolution = hasSolution,
                    TerminationReason =
                        MapTerminationReason(
                            output,
                            solutionPath,
                            hasSolution,
                            processResult.WasCancelled),
                    IsOptimal =
                        IsOptimal(output, solutionPath)
                };

            ExternalSolverResultUtilities.PopulateVariableValues(
                result,
                request.Model,
                values);

            PopulateStatistics(
                result,
                request.Model,
                values,
                output,
                solutionPath);

            foreach (string diagnostic in parameterDiagnostics)
            {
                result.AddDiagnostic(diagnostic);
            }

            result.AddDiagnostic(
                $"cbc exit code: {processResult.ExitCode}.");

            string? finalLine =
                LastMeaningfulLine(output);

            if (!string.IsNullOrWhiteSpace(finalLine))
            {
                result.AddDiagnostic(
                    $"CBC final log: {finalLine}");
            }

            await PublishProgressAsync(
                request,
                new SolverProgressSnapshot
                {
                    Stage = SolverProgressStage.Completed,
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                    IncumbentObjective = result.ObjectiveValue,
                    BestBound = result.BestBound,
                    AbsoluteGap = result.AbsoluteGap,
                    RelativeGap = result.RelativeGap,
                    ExploredNodeCount = result.ExploredNodeCount,
                    IterationCount = result.IterationCount,
                    Message =
                        $"COIN-OR CBC terminated with reason " +
                        $"'{result.TerminationReason}'."
                },
                CancellationToken.None);

            return result;
        }
        finally
        {
            ExternalSolverResultUtilities.TryDeleteDirectory(
                temporaryDirectory);
        }
    }

    /// <inheritdoc />
    protected override void RequestNativeStop()
    {
        _processRunner.Stop();
    }

    private static IReadOnlyList<string> BuildArguments(
        SolverParameters parameters,
        string modelPath,
        string solutionPath,
        out IReadOnlyList<string> diagnostics)
    {
        var arguments =
            new List<string>
            {
                modelPath
            };

        var messages =
            new List<string>();

        AddParameter(
            arguments,
            "seconds",
            parameters.TimeLimitSeconds);
        AddParameter(
            arguments,
            "threads",
            parameters.ThreadCount);
        AddParameter(
            arguments,
            "ratioGap",
            parameters.RelativeMipGap);
        AddParameter(
            arguments,
            "allowableGap",
            parameters.AbsoluteMipGap);
        AddParameter(
            arguments,
            "maxNodes",
            parameters.NodeLimit);
        AddParameter(
            arguments,
            "maxSolutions",
            parameters.SolutionLimit);
        AddParameter(
            arguments,
            "randomSeed",
            parameters.RandomSeed);

        if (parameters.EnablePresolve.HasValue)
        {
            arguments.Add("-presolve");
            arguments.Add(
                parameters.EnablePresolve.Value
                    ? "on"
                    : "off");
        }

        if (parameters.EnableCuts == false)
        {
            arguments.Add("-cuts");
            arguments.Add("off");
        }

        if (parameters.EnableHeuristics == false)
        {
            arguments.Add("-heuristicsOnOff");
            arguments.Add("off");
        }

        if (parameters.IterationLimit.HasValue)
        {
            messages.Add(
                "Generic parameter 'IterationLimit' is valid but is not " +
                "translated by the CBC command-line adapter.");
        }

        if (parameters.MemoryLimitMegabytes.HasValue)
        {
            messages.Add(
                "Generic parameter 'MemoryLimitMegabytes' is valid but is " +
                "not translated by the CBC command-line adapter.");
        }

        if (parameters.DeterministicMode)
        {
            messages.Add(
                "Generic parameter 'DeterministicMode' is valid but is not " +
                "translated by the CBC command-line adapter.");
        }

        foreach (KeyValuePair<string, string> parameter in
                 EnumerateNativeParameters(parameters.NativeParameters))
        {
            if (string.IsNullOrWhiteSpace(parameter.Key) ||
                parameter.Key.Any(char.IsWhiteSpace))
            {
                messages.Add(
                    $"Ignored invalid CBC native parameter name " +
                    $"'{parameter.Key}'.");
                continue;
            }

            arguments.Add(
                parameter.Key.StartsWith('-')
                    ? parameter.Key
                    : $"-{parameter.Key}");
            arguments.Add(parameter.Value);
        }

        arguments.Add("-printingOptions");
        arguments.Add("all");
        arguments.Add("-solve");
        arguments.Add("-solu");
        arguments.Add(solutionPath);
        arguments.Add("-quit");

        diagnostics = messages;
        return arguments;
    }

    private static void AddParameter<T>(
        ICollection<string> arguments,
        string name,
        T? value)
        where T : struct, IFormattable
    {
        if (!value.HasValue)
        {
            return;
        }

        arguments.Add($"-{name}");
        arguments.Add(
            value.Value.ToString(
                null,
                CultureInfo.InvariantCulture));
    }

    private static void PopulateStatistics(
        MathematicalModelSolveResult result,
        MathematicalModel model,
        IReadOnlyDictionary<int, double> values,
        string output,
        string solutionPath)
    {
        if (TryParseSolutionObjective(
                solutionPath,
                out double solutionObjective))
        {
            result.ObjectiveValue = solutionObjective;
        }
        else if (result.HasFeasibleSolution &&
                 ExternalSolverResultUtilities.TryEvaluateObjective(
                     model,
                     values,
                     out double recomputedObjective))
        {
            result.ObjectiveValue = recomputedObjective;
            result.AddDiagnostic(
                "CBC objective was recomputed from the returned solution " +
                "because no objective value could be parsed from the solution " +
                "header.");
        }

        Match boundMatch =
            BestPossibleRegex().Match(output);

        if (boundMatch.Success &&
            TryParseFinite(
                boundMatch.Groups[1].Value,
                out double bound))
        {
            result.BestBound = bound;
        }
        else if (result.IsOptimal &&
                 result.ObjectiveValue.HasValue)
        {
            result.BestBound = result.ObjectiveValue;
        }

        Match? nodesMatch =
            NodesRegex().Matches(output)
                .Cast<Match>()
                .LastOrDefault(match => match.Success);

        if (nodesMatch is not null &&
            nodesMatch.Success &&
            long.TryParse(
                nodesMatch.Groups[2].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long nodes))
        {
            result.ExploredNodeCount = nodes;
        }

        if (nodesMatch is not null &&
            nodesMatch.Success &&
            long.TryParse(
                nodesMatch.Groups[1].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long iterations))
        {
            result.IterationCount = iterations;
        }

        ExternalSolverResultUtilities.PopulateGapStatistics(
            result);
    }

    private static SolverTerminationReason MapTerminationReason(
        string output,
        string solutionPath,
        bool hasSolution,
        bool wasCancelled)
    {
        if (wasCancelled)
        {
            return SolverTerminationReason.UserInterrupted;
        }

        if (IsOptimal(output, solutionPath))
        {
            return SolverTerminationReason.Optimal;
        }

        if (ContainsAny(
                output,
                "infeasible or unbounded"))
        {
            return SolverTerminationReason.InfeasibleOrUnbounded;
        }

        if (ContainsAny(
                output,
                "problem is infeasible",
                "linear relaxation infeasible",
                "infeasible"))
        {
            return SolverTerminationReason.Infeasible;
        }

        if (ContainsAny(
                output,
                "unbounded"))
        {
            return SolverTerminationReason.Unbounded;
        }

        if (ContainsAny(
                output,
                "exiting on maximum time",
                "stopped on time"))
        {
            return SolverTerminationReason.TimeLimit;
        }

        if (ContainsAny(
                output,
                "exiting on maximum nodes"))
        {
            return SolverTerminationReason.NodeLimit;
        }

        if (ContainsAny(
                output,
                "exiting on maximum solutions"))
        {
            return SolverTerminationReason.SolutionLimit;
        }

        return hasSolution
            ? SolverTerminationReason.Feasible
            : SolverTerminationReason.Unknown;
    }

    private static bool IsOptimal(
        string output,
        string solutionPath)
    {
        if (ContainsAny(
                output,
                "optimal solution found",
                "result - optimal solution found"))
        {
            return true;
        }

        string firstLine =
            ReadFirstLine(solutionPath);

        return firstLine.StartsWith(
            "Optimal",
            StringComparison.OrdinalIgnoreCase);
    }

    private static bool SolutionFileSaysNoSolution(
        string solutionPath)
    {
        string firstLine =
            ReadFirstLine(solutionPath);

        return firstLine.Contains(
                   "no integer solution",
                   StringComparison.OrdinalIgnoreCase) ||
               firstLine.Contains(
                   "infeasible",
                   StringComparison.OrdinalIgnoreCase) ||
               firstLine.Contains(
                   "unbounded",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseSolutionObjective(
        string solutionPath,
        out double objective)
    {
        objective = default;

        string firstLine =
            ReadFirstLine(solutionPath);

        return NamedSolutionValueParser.TryParseNumberAfter(
            firstLine,
            @"objective\s+value\s*",
            out objective);
    }

    private static string ReadFirstLine(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            !File.Exists(path))
        {
            return string.Empty;
        }

        using var reader =
            new StreamReader(path);

        return reader.ReadLine() ?? string.Empty;
    }

    private string ResolveExecutablePath()
    {
        return ExternalSolverExecutableLocator.Resolve(
            ExplicitExecutableEnvironmentVariable,
            ["CBC_HOME", "COINOR_HOME"],
            [
                Path.Combine("bin", "cbc.exe"),
                Path.Combine("bin", "cbc"),
                "cbc.exe",
                "cbc"
            ],
            ["cbc.exe", "cbc"]);
    }

    private static string ParseVersion(
        string text)
    {
        Match match =
            VersionRegex().Match(text ?? string.Empty);

        return match.Success
            ? match.Groups[1].Value
            : string.Empty;
    }

    private static string JoinOutput(
        ExternalSolverProcessResult processResult)
    {
        return string.Join(
            Environment.NewLine,
            new[]
            {
                processResult.StandardOutput,
                processResult.StandardError
            }.Where(text => !string.IsNullOrWhiteSpace(text)));
    }

    private static string? FirstMeaningfulLine(
        string text)
    {
        return SplitLines(text).FirstOrDefault();
    }

    private static string? LastMeaningfulLine(
        string text)
    {
        return SplitLines(text).LastOrDefault();
    }

    private static IEnumerable<string> SplitLines(
        string text)
    {
        return (text ?? string.Empty)
            .Split(
                ['\r', '\n'],
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line));
    }

    private static bool TryParseFinite(
        string text,
        out double value)
    {
        return double.TryParse(
                   text,
                   NumberStyles.Float,
                   CultureInfo.InvariantCulture,
                   out value) &&
               double.IsFinite(value);
    }

    private static bool ContainsAny(
        string text,
        params string[] patterns)
    {
        return patterns.Any(
            pattern =>
                text.Contains(
                    pattern,
                    StringComparison.OrdinalIgnoreCase));
    }

    [GeneratedRegex(
        @"(?:Version|version)\s*:?\s*([0-9]+(?:\.[0-9]+)+)",
        RegexOptions.CultureInvariant)]
    private static partial Regex VersionRegex();

    [GeneratedRegex(
        @"best possible\s+([+\-0-9.eE]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BestPossibleRegex();

    [GeneratedRegex(
        @"took\s+([0-9]+)\s+iterations\s+and\s+([0-9]+)\s+nodes",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NodesRegex();
    private static SolverProgressStage ResolveOptimizationProgressStage()
    {
        foreach (string candidate in
                 new[] { "Optimizing", "Solving", "Running" })
        {
            if (Enum.TryParse(
                    candidate,
                    ignoreCase: true,
                    out SolverProgressStage stage))
            {
                return stage;
            }
        }

        // BuildingModel exists in the current solver contract and is the
        // safest fallback for older contracts that do not expose a dedicated
        // optimization/running stage. The accompanying message still reports
        // the actual activity precisely.
        return SolverProgressStage.BuildingModel;
    }

    private static IEnumerable<KeyValuePair<string, string>>
        EnumerateNativeParameters(
            System.Collections.IEnumerable? nativeParameters)
    {
        if (nativeParameters is null)
        {
            yield break;
        }

        foreach (object? item in nativeParameters)
        {
            if (item is null)
            {
                continue;
            }

            if (item is KeyValuePair<string, string> pair)
            {
                yield return pair;
                continue;
            }

            object? nameValue =
                ReadNativeParameterMember(
                    item,
                    "Name",
                    "Key",
                    "ParameterName",
                    "Parameter");

            object? parameterValue =
                ReadNativeParameterMember(
                    item,
                    "Value",
                    "ParameterValue");

            string name =
                Convert.ToString(
                    nameValue,
                    CultureInfo.InvariantCulture) ??
                string.Empty;

            string value =
                Convert.ToString(
                    parameterValue,
                    CultureInfo.InvariantCulture) ??
                string.Empty;

            if (!string.IsNullOrWhiteSpace(name))
            {
                yield return new KeyValuePair<string, string>(
                    name,
                    value);
            }
        }
    }

    private static object? ReadNativeParameterMember(
        object instance,
        params string[] candidateNames)
    {
        Type type = instance.GetType();

        foreach (string candidateName in candidateNames)
        {
            var property =
                type.GetProperties()
                    .FirstOrDefault(
                        candidate =>
                            candidate.CanRead &&
                            string.Equals(
                                candidate.Name,
                                candidateName,
                                StringComparison.OrdinalIgnoreCase));

            if (property is not null)
            {
                return property.GetValue(instance);
            }

            var field =
                type.GetFields()
                    .FirstOrDefault(
                        candidate =>
                            string.Equals(
                                candidate.Name,
                                candidateName,
                                StringComparison.OrdinalIgnoreCase));

            if (field is not null)
            {
                return field.GetValue(instance);
            }
        }

        return null;
    }

}
