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

namespace LotSizingDataModel.Solver.Gurobi;

/// <summary>
/// Solves generic linear and mixed-integer linear models with Gurobi through
/// the official <c>gurobi_cl</c> command-line executable.
/// </summary>
/// <remarks>
/// The adapter deliberately has no compile-time dependency on the Gurobi .NET
/// assembly. A machine without Gurobi can therefore build and use the rest of
/// LotSizingDataModel unchanged.
/// </remarks>
public sealed partial class GurobiSolverAdapter :
    MathematicalModelSolverPluginBase
{
    private const string ExplicitExecutableEnvironmentVariable =
        "LOTSIZING_GUROBI_EXECUTABLE";

    private readonly ExternalSolverProcessRunner _processRunner =
        new();

    private string _cachedExecutablePath =
        string.Empty;

    private string _cachedVersion =
        string.Empty;

    /// <summary>
    /// Initializes the Gurobi solver adapter.
    /// </summary>
    public GurobiSolverAdapter()
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
        SolverKind.Gurobi;

    /// <inheritdoc />
    public override string SolverName =>
        "Gurobi Optimizer";

    /// <inheritdoc />
    public override string SolverVersion =>
        _cachedVersion;

    /// <inheritdoc />
    public override string AdapterId =>
        "LotSizingDataModel.Solver.Gurobi";

    /// <inheritdoc />
    public override string AdapterName =>
        "LotSizingDataModel Gurobi Adapter";

    /// <inheritdoc />
    public override string AdapterVersion =>
        "1.0.0";

    /// <inheritdoc />
    public override string MinimumSupportedSolverVersion =>
        "12.0";

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
                    SolverKind.Gurobi,
                    SolverAvailabilityStatus.NotInstalled)
                {
                    SolverName = SolverName
                };

            notInstalled.AddDiagnostic(
                "gurobi_cl was not found. Install Gurobi, put gurobi_cl " +
                "on PATH, define GUROBI_HOME, or set " +
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
                    ["--version"],
                    workingDirectory,
                    standardInput: null,
                    cancellationToken);

            if (processResult.WasCancelled)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            string text =
                JoinOutput(processResult);

            if (processResult.ExitCode != 0)
            {
                var failed =
                    new SolverAvailabilityInfo(
                        SolverKind.Gurobi,
                        SolverAvailabilityStatus.LoadFailure)
                    {
                        SolverName = SolverName
                    };

                failed.AddDiagnostic(
                    FirstMeaningfulLine(text) ??
                    $"gurobi_cl exited with code {processResult.ExitCode}.");

                return failed;
            }

            ExternalSolverProcessResult licenseResult =
                await _processRunner.RunAsync(
                    executablePath,
                    ["--license"],
                    workingDirectory,
                    standardInput: null,
                    cancellationToken);

            if (licenseResult.WasCancelled)
            {
                cancellationToken.ThrowIfCancellationRequested();
            }

            string licenseText =
                JoinOutput(licenseResult);

            if (licenseResult.ExitCode != 0)
            {
                var failed =
                    new SolverAvailabilityInfo(
                        SolverKind.Gurobi,
                        SolverAvailabilityStatus.LoadFailure)
                    {
                        SolverName = SolverName,
                        SolverVersion = ParseVersion(text)
                    };

                failed.AddDiagnostic(
                    FirstMeaningfulLine(licenseText) ??
                    "gurobi_cl could be executed, but its license " +
                    "information could not be resolved.");

                return failed;
            }

            _cachedExecutablePath =
                executablePath;

            _cachedVersion =
                ParseVersion(text);

            var available =
                new SolverAvailabilityInfo(
                    SolverKind.Gurobi,
                    SolverAvailabilityStatus.Available)
                {
                    SolverName = SolverName,
                    SolverVersion = _cachedVersion
                };

            available.AddDiagnostic(
                $"Gurobi command-line executable detected at " +
                $"'{executablePath}'.");

            if (!string.IsNullOrWhiteSpace(text))
            {
                available.AddDiagnostic(
                    FirstMeaningfulLine(text) ?? text.Trim());
            }

            if (!string.IsNullOrWhiteSpace(licenseText))
            {
                available.AddDiagnostic(
                    FirstMeaningfulLine(licenseText) ?? licenseText.Trim());
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
                    SolverKind.Gurobi,
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
                "Gurobi is not available. Call solver discovery/availability " +
                "before attempting to solve.");
        }

        string temporaryDirectory =
            ExternalSolverResultUtilities.CreateTemporaryDirectory(
                "gurobi");

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
                    Message = "Writing the portable LP model for Gurobi."
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
                    Message = "Gurobi optimization started."
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

            IReadOnlyDictionary<int, double> values =
                NamedSolutionValueParser.ParseFile(
                    solutionPath);

            bool hasSolution =
                values.Count > 0 ||
                (request.Model.VariableCount == 0 &&
                 File.Exists(solutionPath));

            var result =
                new MathematicalModelSolveResult
                {
                    SolverKind = SolverKind.Gurobi,
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
                            hasSolution,
                            processResult.WasCancelled),
                    IsOptimal = IsOptimal(output)
                };

            ExternalSolverResultUtilities.PopulateVariableValues(
                result,
                request.Model,
                values);

            PopulateStatistics(
                result,
                request.Model,
                values,
                output);

            foreach (string diagnostic in parameterDiagnostics)
            {
                result.AddDiagnostic(diagnostic);
            }

            result.AddDiagnostic(
                $"gurobi_cl exit code: {processResult.ExitCode}.");

            string? finalLine =
                LastMeaningfulLine(output);

            if (!string.IsNullOrWhiteSpace(finalLine))
            {
                result.AddDiagnostic(
                    $"Gurobi final log: {finalLine}");
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
                        $"Gurobi terminated with reason " +
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
                $"ResultFile={solutionPath}"
            };

        var messages =
            new List<string>();

        AddParameter(
            arguments,
            "TimeLimit",
            parameters.TimeLimitSeconds);
        AddParameter(
            arguments,
            "Threads",
            parameters.ThreadCount);
        AddParameter(
            arguments,
            "MIPGap",
            parameters.RelativeMipGap);
        AddParameter(
            arguments,
            "MIPGapAbs",
            parameters.AbsoluteMipGap);
        AddParameter(
            arguments,
            "NodeLimit",
            parameters.NodeLimit);
        AddParameter(
            arguments,
            "SolutionLimit",
            parameters.SolutionLimit);
        AddParameter(
            arguments,
            "IterationLimit",
            parameters.IterationLimit);
        AddParameter(
            arguments,
            "Seed",
            parameters.RandomSeed);

        if (parameters.EnablePresolve == false)
        {
            arguments.Add("Presolve=0");
        }

        if (parameters.EnableCuts == false)
        {
            arguments.Add("Cuts=0");
        }

        if (parameters.EnableHeuristics == false)
        {
            arguments.Add("Heuristics=0");
        }

        if (parameters.MemoryLimitMegabytes.HasValue)
        {
            messages.Add(
                "Generic parameter 'MemoryLimitMegabytes' is valid but is " +
                "not translated by the command-line Gurobi adapter.");
        }

        if (parameters.DeterministicMode)
        {
            messages.Add(
                "Generic parameter 'DeterministicMode' is valid but is not " +
                "translated by the command-line Gurobi adapter.");
        }

        foreach (KeyValuePair<string, string> parameter in
                 EnumerateNativeParameters(parameters.NativeParameters))
        {
            if (string.IsNullOrWhiteSpace(parameter.Key) ||
                parameter.Key.Contains('=') ||
                parameter.Key.Any(char.IsWhiteSpace))
            {
                messages.Add(
                    $"Ignored invalid Gurobi native parameter name " +
                    $"'{parameter.Key}'.");
                continue;
            }

            arguments.Add(
                $"{parameter.Key}={parameter.Value}");
        }

        arguments.Add(modelPath);
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

        arguments.Add(
            $"{name}={value.Value.ToString(null, CultureInfo.InvariantCulture)}");
    }

    private static void PopulateStatistics(
        MathematicalModelSolveResult result,
        MathematicalModel model,
        IReadOnlyDictionary<int, double> values,
        string output)
    {
        if (TryParseBestObjectiveAndBound(
                output,
                out double incumbent,
                out double bound))
        {
            result.ObjectiveValue = incumbent;
            result.BestBound = bound;
        }
        else if (TryParseOptimalObjective(
                     output,
                     out double optimalObjective))
        {
            result.ObjectiveValue = optimalObjective;
            result.BestBound = optimalObjective;
        }
        else if (result.HasFeasibleSolution &&
                 ExternalSolverResultUtilities.TryEvaluateObjective(
                     model,
                     values,
                     out double recomputedObjective))
        {
            result.ObjectiveValue = recomputedObjective;
            result.AddDiagnostic(
                "Gurobi objective was recomputed from the returned solution " +
                "because no objective value could be parsed from the log.");
        }

        Match nodeMatch =
            NodeCountRegex().Match(output);

        if (nodeMatch.Success &&
            long.TryParse(
                nodeMatch.Groups[1].Value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out long nodes))
        {
            result.ExploredNodeCount = nodes;
        }

        Match iterationMatch =
            IterationCountRegex().Match(output);

        if (iterationMatch.Success &&
            long.TryParse(
                iterationMatch.Groups[1].Value,
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
        bool hasSolution,
        bool wasCancelled)
    {
        if (wasCancelled)
        {
            return SolverTerminationReason.UserInterrupted;
        }

        if (IsOptimal(output))
        {
            return SolverTerminationReason.Optimal;
        }

        if (output.Contains(
                "infeasible or unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.InfeasibleOrUnbounded;
        }

        if (output.Contains(
                "model is infeasible",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.Infeasible;
        }

        if (output.Contains(
                "model is unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.Unbounded;
        }

        if (output.Contains(
                "time limit reached",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.TimeLimit;
        }

        if (output.Contains(
                "node limit reached",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.NodeLimit;
        }

        if (output.Contains(
                "iteration limit reached",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.IterationLimit;
        }

        if (output.Contains(
                "solution limit reached",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.SolutionLimit;
        }

        return hasSolution
            ? SolverTerminationReason.Feasible
            : SolverTerminationReason.Unknown;
    }

    private static bool IsOptimal(
        string output)
    {
        return output.Contains(
                   "optimal solution found",
                   StringComparison.OrdinalIgnoreCase) ||
               output.Contains(
                   "optimal objective",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryParseBestObjectiveAndBound(
        string output,
        out double objective,
        out double bound)
    {
        objective = default;
        bound = default;

        Match match =
            BestObjectiveRegex().Match(output);

        return match.Success &&
               TryParseFinite(match.Groups[1].Value, out objective) &&
               TryParseFinite(match.Groups[2].Value, out bound);
    }

    private static bool TryParseOptimalObjective(
        string output,
        out double objective)
    {
        objective = default;

        Match match =
            OptimalObjectiveRegex().Match(output);

        return match.Success &&
               TryParseFinite(match.Groups[1].Value, out objective);
    }

    private string ResolveExecutablePath()
    {
        return ExternalSolverExecutableLocator.Resolve(
            ExplicitExecutableEnvironmentVariable,
            ["GUROBI_HOME"],
            [
                Path.Combine("bin", "gurobi_cl.exe"),
                Path.Combine("bin", "gurobi_cl")
            ],
            ["gurobi_cl.exe", "gurobi_cl"]);
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

    private static string ParseVersion(
        string text)
    {
        Match match =
            VersionRegex().Match(text ?? string.Empty);

        return match.Success
            ? match.Groups[1].Value
            : string.Empty;
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

    [GeneratedRegex(
        @"Gurobi Optimizer version\s+([0-9]+(?:\.[0-9]+)+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex VersionRegex();

    [GeneratedRegex(
        @"Best objective\s+([+\-0-9.eE]+),\s*best bound\s+([+\-0-9.eE]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex BestObjectiveRegex();

    [GeneratedRegex(
        @"Optimal objective\s+([+\-0-9.eE]+)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex OptimalObjectiveRegex();

    [GeneratedRegex(
        @"Explored\s+([0-9]+)\s+nodes",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex NodeCountRegex();

    [GeneratedRegex(
        @"([0-9]+)\s+simplex iterations",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex IterationCountRegex();
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
