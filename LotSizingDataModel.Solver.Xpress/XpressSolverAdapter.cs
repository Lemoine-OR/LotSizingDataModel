using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using LotSizingDataModel.Solver.Adapters;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Configuration;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.External;
using LotSizingDataModel.Solver.Modeling;
using LotSizingDataModel.Solver.Monitoring;

namespace LotSizingDataModel.Solver.Xpress;

/// <summary>
/// Solves generic linear and mixed-integer linear models with FICO Xpress MP.
/// </summary>
/// <remarks>
/// <para>
/// The official Xpress .NET Optimizer assembly is loaded at runtime through
/// reflection. This keeps Xpress optional: the project itself compiles when no
/// Xpress installation is present.
/// </para>
/// <para>
/// The adapter uses the long-standing <c>Optimizer.XPRS</c> and
/// <c>Optimizer.XPRSprob</c> API surface (Init, ReadProb, Optimize,
/// GetSolution, Destroy and Free).
/// </para>
/// </remarks>
public sealed class XpressSolverAdapter :
    MathematicalModelSolverPluginBase
{
    private static readonly SemaphoreSlim RuntimeGate =
        new(1, 1);

    private readonly object _nativeSyncRoot =
        new();

    private object? _activeProblem;

    private string _cachedVersion =
        string.Empty;

    /// <summary>
    /// Initializes the FICO Xpress MP adapter.
    /// </summary>
    public XpressSolverAdapter()
        : base(
        [
            SolverCapability.LinearProgramming,
            SolverCapability.MixedIntegerLinearProgramming,
            SolverCapability.LpExport,
            SolverCapability.OptimalityGapReporting,
            SolverCapability.SearchStatistics
        ])
    {
    }

    /// <inheritdoc />
    public override SolverKind SolverKind =>
        SolverKind.Xpress;

    /// <inheritdoc />
    public override string SolverName =>
        "FICO Xpress MP";

    /// <inheritdoc />
    public override string SolverVersion =>
        _cachedVersion;

    /// <inheritdoc />
    public override string AdapterId =>
        "LotSizingDataModel.Solver.Xpress";

    /// <inheritdoc />
    public override string AdapterName =>
        "LotSizingDataModel Xpress Adapter";

    /// <inheritdoc />
    public override string AdapterVersion =>
        "1.0.0";

    /// <inheritdoc />
    public override string MinimumSupportedSolverVersion =>
        "9.0";

    /// <inheritdoc />
    public override async ValueTask<SolverAvailabilityInfo>
        CheckAvailabilityAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        Assembly? assembly;
        string resolvedPath;

        try
        {
            assembly =
                XpressRuntimeLocator.TryLoad(
                    out resolvedPath);
        }
        catch (Exception exception)
        {
            return CreateFailureAvailability(
                exception.Message);
        }

        if (assembly is null)
        {
            var notInstalled =
                new SolverAvailabilityInfo(
                    SolverKind.Xpress,
                    SolverAvailabilityStatus.NotInstalled)
                {
                    SolverName = SolverName
                };

            notInstalled.AddDiagnostic(
                "The FICO Xpress Optimizer .NET assembly was not found. " +
                "Define XPRESSDIR, make Optimizer.dll probeable, or set " +
                $"{XpressRuntimeLocator.ExplicitAssemblyEnvironmentVariable}.");

            return notInstalled;
        }

        await RuntimeGate.WaitAsync(
            cancellationToken);

        try
        {
            XpressReflectionApi api =
                XpressReflectionApi.Create(
                    assembly);

            api.Initialize();

            try
            {
                _cachedVersion =
                    api.TryGetVersion();

                var available =
                    new SolverAvailabilityInfo(
                        SolverKind.Xpress,
                        SolverAvailabilityStatus.Available)
                    {
                        SolverName = SolverName,
                        SolverVersion = _cachedVersion
                    };

                available.AddDiagnostic(
                    $"FICO Xpress Optimizer managed runtime loaded from " +
                    $"'{resolvedPath}'.");

                available.AddDiagnostic(
                    "XPRS.Init completed successfully, including native " +
                    "runtime/license initialization.");

                return available;
            }
            finally
            {
                api.Free();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return CreateFailureAvailability(
                Unwrap(exception).Message);
        }
        finally
        {
            RuntimeGate.Release();
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

        Assembly? assembly =
            XpressRuntimeLocator.TryLoad(
                out _);

        if (assembly is null)
        {
            throw new InvalidOperationException(
                "FICO Xpress MP is not available. Call solver discovery/" +
                "availability before attempting to solve.");
        }

        string temporaryDirectory =
            ExternalSolverResultUtilities.CreateTemporaryDirectory(
                "xpress");

        string modelPath =
            Path.Combine(
                temporaryDirectory,
                "model.lp");

        string solutionPath =
            Path.Combine(
                temporaryDirectory,
                "solution.asc");

        var stopwatch =
            Stopwatch.StartNew();

        await RuntimeGate.WaitAsync(
            cancellationToken);

        try
        {
            await PublishProgressAsync(
                request,
                new SolverProgressSnapshot
                {
                    Stage = SolverProgressStage.BuildingModel,
                    ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                    Message = "Writing the portable LP model for FICO Xpress."
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

            XpressReflectionApi api =
                XpressReflectionApi.Create(
                    assembly);

            api.Initialize();

            object? problem =
                null;

            try
            {
                problem =
                    api.CreateProblem();

                SetActiveProblem(problem);

                using CancellationTokenRegistration cancellationRegistration =
                    cancellationToken.Register(RequestNativeStop);

                api.ReadProblem(
                    problem,
                    modelPath);

                IReadOnlyList<string> parameterDiagnostics =
                    api.ApplyParameters(
                        problem,
                        request.Parameters);

                await PublishProgressAsync(
                    request,
                    new SolverProgressSnapshot
                    {
                        Stage = ResolveOptimizationProgressStage(),
                        ElapsedSeconds = stopwatch.Elapsed.TotalSeconds,
                        Message = "FICO Xpress optimization started."
                    },
                    cancellationToken);

                // Optimize is a native blocking operation. Run it off the
                // caller's continuation thread while RequestNativeStop remains
                // able to make a best-effort interrupt request.
                await Task.Run(
                    () => api.Optimize(problem),
                    CancellationToken.None);

                stopwatch.Stop();

                double[]? solution =
                    api.TryGetSolution(problem);

                IReadOnlyDictionary<int, double> values =
                    api.GetValuesByPortableName(
                        problem,
                        request.Model,
                        solution,
                        solutionPath,
                        out string mappingDiagnostic);

                bool hasSolution =
                    solution is not null &&
                    values.Count > 0;

                string status =
                    api.TryGetStatus(problem);

                var result =
                    new MathematicalModelSolveResult
                    {
                        SolverKind = SolverKind.Xpress,
                        SolverName = SolverName,
                        SolverVersion =
                            !string.IsNullOrWhiteSpace(_cachedVersion)
                                ? _cachedVersion
                                : api.TryGetVersion(),
                        SolveDuration = stopwatch.Elapsed,
                        HasFeasibleSolution = hasSolution,
                        IsOptimal =
                            IsOptimal(
                                status,
                                problem,
                                api,
                                hasSolution),
                        TerminationReason =
                            MapTerminationReason(
                                status,
                                problem,
                                api,
                                hasSolution,
                                cancellationToken.IsCancellationRequested),
                        ExploredNodeCount =
                            api.TryGetInt64Property(
                                problem,
                                "Nodes",
                                "NodeCount"),
                        IterationCount =
                            api.TryGetInt64Property(
                                problem,
                                "SimplexIter",
                                "SimplexIterations",
                                "LPIterations")
                    };

                ExternalSolverResultUtilities.PopulateVariableValues(
                    result,
                    request.Model,
                    values);

                PopulateObjectiveAndGap(
                    result,
                    request.Model,
                    values,
                    problem,
                    api);

                foreach (string diagnostic in parameterDiagnostics)
                {
                    result.AddDiagnostic(diagnostic);
                }

                if (!string.IsNullOrWhiteSpace(mappingDiagnostic))
                {
                    result.AddDiagnostic(mappingDiagnostic);
                }

                result.AddDiagnostic(
                    string.IsNullOrWhiteSpace(status)
                        ? "Xpress status was not exposed by the loaded API surface."
                        : $"Xpress status: {status}.");

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
                            $"FICO Xpress terminated with reason " +
                            $"'{result.TerminationReason}'."
                    },
                    CancellationToken.None);

                return result;
            }
            finally
            {
                if (problem is not null)
                {
                    ClearActiveProblem(problem);
                    api.DestroyProblem(problem);
                }

                api.Free();
            }
        }
        finally
        {
            RuntimeGate.Release();

            ExternalSolverResultUtilities.TryDeleteDirectory(
                temporaryDirectory);
        }
    }

    /// <inheritdoc />
    protected override void RequestNativeStop()
    {
        object? problem;

        lock (_nativeSyncRoot)
        {
            problem = _activeProblem;
        }

        if (problem is null)
        {
            return;
        }

        XpressReflectionApi.TryInterrupt(problem);
    }

    private static void PopulateObjectiveAndGap(
        MathematicalModelSolveResult result,
        MathematicalModel model,
        IReadOnlyDictionary<int, double> values,
        object problem,
        XpressReflectionApi api)
    {
        double? objective =
            api.TryGetDoubleProperty(
                problem,
                "MIPBestObjVal",
                "MIPObjVal",
                "LPObjVal",
                "ObjVal");

        if (objective.HasValue)
        {
            result.ObjectiveValue =
                objective.Value;
        }
        else if (result.HasFeasibleSolution &&
                 ExternalSolverResultUtilities.TryEvaluateObjective(
                     model,
                     values,
                     out double recomputedObjective))
        {
            result.ObjectiveValue =
                recomputedObjective;

            result.AddDiagnostic(
                "Xpress objective was recomputed from the returned solution " +
                "because the loaded Optimizer API exposed no recognized " +
                "objective attribute/property.");
        }

        result.BestBound =
            api.TryGetDoubleProperty(
                problem,
                "BestBound",
                "MIPBestBound");

        if (!result.BestBound.HasValue &&
            result.IsOptimal &&
            result.ObjectiveValue.HasValue)
        {
            result.BestBound =
                result.ObjectiveValue;
        }

        ExternalSolverResultUtilities.PopulateGapStatistics(
            result);
    }

    private static bool IsOptimal(
        string status,
        object problem,
        XpressReflectionApi api,
        bool hasSolution)
    {
        if (status.Contains(
                "optimal",
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!hasSolution)
        {
            return false;
        }

        double? objective =
            api.TryGetDoubleProperty(
                problem,
                "MIPBestObjVal",
                "MIPObjVal",
                "LPObjVal",
                "ObjVal");

        double? bound =
            api.TryGetDoubleProperty(
                problem,
                "BestBound",
                "MIPBestBound");

        return objective.HasValue &&
               bound.HasValue &&
               Math.Abs(objective.Value - bound.Value) <=
               1.0e-10 *
               Math.Max(
                   1.0,
                   Math.Abs(objective.Value));
    }

    private static SolverTerminationReason MapTerminationReason(
        string status,
        object problem,
        XpressReflectionApi api,
        bool hasSolution,
        bool cancellationRequested)
    {
        if (cancellationRequested)
        {
            return SolverTerminationReason.UserInterrupted;
        }

        if (IsOptimal(status, problem, api, hasSolution))
        {
            return SolverTerminationReason.Optimal;
        }

        if (status.Contains(
                "infeasible",
                StringComparison.OrdinalIgnoreCase) &&
            status.Contains(
                "unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.InfeasibleOrUnbounded;
        }

        if (status.Contains(
                "infeasible",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.Infeasible;
        }

        if (status.Contains(
                "unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.Unbounded;
        }

        if (status.Contains(
                "time",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.TimeLimit;
        }

        if (status.Contains(
                "node",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.NodeLimit;
        }

        if (status.Contains(
                "solution",
                StringComparison.OrdinalIgnoreCase) &&
            status.Contains(
                "limit",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.SolutionLimit;
        }

        return hasSolution
            ? SolverTerminationReason.Feasible
            : SolverTerminationReason.Unknown;
    }

    private SolverAvailabilityInfo CreateFailureAvailability(
        string diagnostic)
    {
        var failed =
            new SolverAvailabilityInfo(
                SolverKind.Xpress,
                SolverAvailabilityStatus.LoadFailure)
            {
                SolverName = SolverName
            };

        failed.AddDiagnostic(diagnostic);
        return failed;
    }

    private void SetActiveProblem(
        object problem)
    {
        lock (_nativeSyncRoot)
        {
            _activeProblem = problem;
        }
    }

    private void ClearActiveProblem(
        object problem)
    {
        lock (_nativeSyncRoot)
        {
            if (ReferenceEquals(
                    _activeProblem,
                    problem))
            {
                _activeProblem = null;
            }
        }
    }

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

        // BuildingModel is known to exist in the current solver contract.
        // Older contracts may not expose a dedicated optimization/running
        // stage, so this is the safest binary-compatible fallback.
        return SolverProgressStage.BuildingModel;
    }

    private static Exception Unwrap(
        Exception exception)
    {
        if (exception is TargetInvocationException invocation &&
            invocation.InnerException is Exception innerException)
        {
            return innerException;
        }

        return exception;
    }
}

/// <summary>
/// Reflection bridge over the optional official FICO Xpress Optimizer .NET
/// assembly.
/// </summary>
internal sealed class XpressReflectionApi
{
    private readonly Type _xprsType;
    private readonly Type _problemType;

    private XpressReflectionApi(
        Type xprsType,
        Type problemType)
    {
        _xprsType = xprsType;
        _problemType = problemType;
    }

    internal static XpressReflectionApi Create(
        Assembly assembly)
    {
        ArgumentNullException.ThrowIfNull(assembly);

        Type xprsType =
            assembly.GetType(
                "Optimizer.XPRS",
                throwOnError: true,
                ignoreCase: false)!;

        Type problemType =
            assembly.GetType(
                "Optimizer.XPRSprob",
                throwOnError: true,
                ignoreCase: false)!;

        return new XpressReflectionApi(
            xprsType,
            problemType);
    }

    internal void Initialize()
    {
        InvokeRequiredStatic(
            _xprsType,
            "Init",
            string.Empty);
    }

    internal void Free()
    {
        try
        {
            InvokeRequiredStatic(
                _xprsType,
                "Free");
        }
        catch
        {
            // Resource cleanup must not mask the primary result/error.
        }
    }

    internal object CreateProblem()
    {
        return Activator.CreateInstance(_problemType) ??
               throw new InvalidOperationException(
                   "Unable to create Optimizer.XPRSprob.");
    }

    internal void ReadProblem(
        object problem,
        string modelPath)
    {
        TrySetProperty(
            problem,
            "MPSFormat",
            -1);

        InvokeRequired(
            problem,
            "ReadProb",
            modelPath,
            string.Empty);
    }

    internal void Optimize(
        object problem)
    {
        InvokeRequired(
            problem,
            "Optimize");
    }

    internal double[]? TryGetSolution(
        object problem)
    {
        try
        {
            object? value =
                InvokeRequired(
                    problem,
                    "GetSolution");

            return value as double[];
        }
        catch
        {
            return null;
        }
    }

    internal IReadOnlyList<string> ApplyParameters(
        object problem,
        SolverParameters parameters)
    {
        var diagnostics =
            new List<string>();

        if (parameters.TimeLimitSeconds.HasValue)
        {
            int seconds =
                Math.Max(
                    1,
                    (int)Math.Ceiling(
                        parameters.TimeLimitSeconds.Value));

            if (!TrySetProperty(
                    problem,
                    "MaxTime",
                    -seconds))
            {
                diagnostics.Add(
                    "Xpress generic parameter 'TimeLimitSeconds' could not " +
                    "be mapped through the loaded API surface.");
            }
        }

        TrySetWithDiagnostic(
            problem,
            "Threads",
            parameters.ThreadCount,
            diagnostics,
            nameof(parameters.ThreadCount));

        TrySetWithDiagnostic(
            problem,
            "MaxNode",
            parameters.NodeLimit,
            diagnostics,
            nameof(parameters.NodeLimit));

        TrySetWithDiagnostic(
            problem,
            "MaxMipSol",
            parameters.SolutionLimit,
            diagnostics,
            nameof(parameters.SolutionLimit));

        TrySetWithDiagnostic(
            problem,
            "MIPRelStop",
            parameters.RelativeMipGap,
            diagnostics,
            nameof(parameters.RelativeMipGap));

        TrySetWithDiagnostic(
            problem,
            "MIPAbsStop",
            parameters.AbsoluteMipGap,
            diagnostics,
            nameof(parameters.AbsoluteMipGap));

        if (parameters.EnablePresolve.HasValue &&
            !TrySetProperty(
                problem,
                "Presolve",
                parameters.EnablePresolve.Value ? 1 : 0))
        {
            diagnostics.Add(
                "Xpress generic parameter 'EnablePresolve' could not be " +
                "mapped through the loaded API surface.");
        }

        if (parameters.EnableCuts == false &&
            !TrySetProperty(
                problem,
                "CutStrategy",
                0))
        {
            diagnostics.Add(
                "Xpress generic parameter 'EnableCuts' could not be mapped " +
                "through the loaded API surface.");
        }

        if (parameters.EnableHeuristics == false &&
            !TrySetProperty(
                problem,
                "HeurEmphasis",
                0))
        {
            diagnostics.Add(
                "Xpress generic parameter 'EnableHeuristics' could not be " +
                "mapped through the loaded API surface.");
        }

        AddDeferred(
            diagnostics,
            parameters.RandomSeed.HasValue,
            nameof(parameters.RandomSeed));
        AddDeferred(
            diagnostics,
            parameters.IterationLimit.HasValue,
            nameof(parameters.IterationLimit));
        AddDeferred(
            diagnostics,
            parameters.MemoryLimitMegabytes.HasValue,
            nameof(parameters.MemoryLimitMegabytes));
        AddDeferred(
            diagnostics,
            parameters.DeterministicMode,
            nameof(parameters.DeterministicMode));

        foreach (KeyValuePair<string, string> parameter in
                 EnumerateNativeParameters(parameters.NativeParameters))
        {
            if (!TrySetProperty(
                    problem,
                    parameter.Key,
                    parameter.Value))
            {
                diagnostics.Add(
                    $"Xpress native parameter '{parameter.Key}' could not be " +
                    "mapped as a public XPRSprob property.");
            }
        }

        return diagnostics;
    }

    internal IReadOnlyDictionary<int, double> GetValuesByPortableName(
        object problem,
        MathematicalModel model,
        double[]? solution,
        string solutionPath,
        out string diagnostic)
    {
        diagnostic = string.Empty;

        if (solution is null)
        {
            return new Dictionary<int, double>();
        }

        var byIndex =
            new Dictionary<int, double>();

        bool allIndexesResolved =
            true;

        foreach (MathematicalVariable variable in model.Variables)
        {
            if (!TryGetColumnIndex(
                    problem,
                    PortableLpModelWriter.GetVariableName(variable.Id),
                    out int columnIndex) ||
                columnIndex < 0 ||
                columnIndex >= solution.Length)
            {
                allIndexesResolved = false;
                break;
            }

            byIndex[variable.Id] =
                solution[columnIndex];
        }

        if (allIndexesResolved &&
            byIndex.Count == model.VariableCount)
        {
            diagnostic =
                "Xpress solution values were mapped by column name through " +
                "XPRSprob.GetIndex.";
            return byIndex;
        }

        if (TryWriteSolution(
                problem,
                solutionPath))
        {
            IReadOnlyDictionary<int, double> parsed =
                XpressAsciiSolutionValueParser.ParseFile(
                    solutionPath);

            if (parsed.Count > 0)
            {
                diagnostic =
                    "Xpress solution values were mapped from the native ASCII " +
                    "solution output using portable variable names.";
                return parsed;
            }
        }

        if (solution.Length == model.VariableCount)
        {
            var positional =
                new Dictionary<int, double>();

            for (int index = 0; index < model.VariableCount; index++)
            {
                positional[model.Variables[index].Id] =
                    solution[index];
            }

            diagnostic =
                "Xpress solution values used the LP column-order fallback " +
                "because the loaded API exposed neither GetIndex nor a " +
                "parseable ASCII solution. The independent checker remains " +
                "the final validity guard.";

            return positional;
        }

        diagnostic =
            "Xpress returned a solution vector, but variable names could not " +
            "be mapped safely to the generic mathematical model.";

        return new Dictionary<int, double>();
    }

    internal string TryGetStatus(
        object problem)
    {
        object? value =
            TryGetProperty(
                problem,
                "SolStatus",
                "MIPStatus",
                "LPStatus");

        return value?.ToString() ??
               string.Empty;
    }

    internal string TryGetVersion()
    {
        object? value =
            TryGetStaticProperty(
                _xprsType,
                "Version",
                "VERSION");

        return value?.ToString() ??
               string.Empty;
    }

    internal double? TryGetDoubleProperty(
        object problem,
        params string[] names)
    {
        object? value =
            TryGetProperty(
                problem,
                names);

        if (value is null)
        {
            return null;
        }

        try
        {
            double converted =
                Convert.ToDouble(
                    value,
                    CultureInfo.InvariantCulture);

            return double.IsFinite(converted)
                ? converted
                : null;
        }
        catch
        {
            return null;
        }
    }

    internal long? TryGetInt64Property(
        object problem,
        params string[] names)
    {
        object? value =
            TryGetProperty(
                problem,
                names);

        if (value is null)
        {
            return null;
        }

        try
        {
            return Convert.ToInt64(
                value,
                CultureInfo.InvariantCulture);
        }
        catch
        {
            return null;
        }
    }

    internal void DestroyProblem(
        object problem)
    {
        try
        {
            InvokeRequired(
                problem,
                "Destroy");
        }
        catch
        {
            if (problem is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    internal static void TryInterrupt(
        object problem)
    {
        foreach (string methodName in new[]
                 {
                     "Interrupt",
                     "Stop"
                 })
        {
            try
            {
                MethodInfo? method =
                    FindMethod(
                        problem.GetType(),
                        methodName,
                        parameterCount: 0);

                if (method is null)
                {
                    continue;
                }

                method.Invoke(
                    problem,
                    null);
                return;
            }
            catch
            {
                // Best-effort interruption only.
            }
        }
    }

    private static bool TryGetColumnIndex(
        object problem,
        string name,
        out int index)
    {
        index = -1;

        MethodInfo? twoParameter =
            FindMethod(
                problem.GetType(),
                "GetIndex",
                parameterCount: 2);

        if (twoParameter is not null)
        {
            try
            {
                object? value =
                    twoParameter.Invoke(
                        problem,
                        [2, name]);

                if (value is not null)
                {
                    index =
                        Convert.ToInt32(
                            value,
                            CultureInfo.InvariantCulture);
                    return index >= 0;
                }
            }
            catch
            {
                // Try the out-parameter shape below.
            }
        }

        MethodInfo? threeParameter =
            FindMethod(
                problem.GetType(),
                "GetIndex",
                parameterCount: 3);

        if (threeParameter is not null)
        {
            try
            {
                object?[] arguments =
                [2, name, 0];

                threeParameter.Invoke(
                    problem,
                    arguments);

                index =
                    Convert.ToInt32(
                        arguments[2],
                        CultureInfo.InvariantCulture);

                return index >= 0;
            }
            catch
            {
                // Fall through to alternative mapping methods.
            }
        }

        return false;
    }

    private static bool TryWriteSolution(
        object problem,
        string solutionPath)
    {
        foreach (int parameterCount in new[] { 2, 1 })
        {
            MethodInfo? method =
                FindMethod(
                    problem.GetType(),
                    "WriteSol",
                    parameterCount);

            if (method is null)
            {
                continue;
            }

            try
            {
                object?[] arguments =
                    parameterCount == 2
                        ? [solutionPath, string.Empty]
                        : [solutionPath];

                method.Invoke(
                    problem,
                    arguments);

                if (File.Exists(solutionPath))
                {
                    return true;
                }

                string? directory =
                    Path.GetDirectoryName(solutionPath);

                string stem =
                    Path.GetFileNameWithoutExtension(solutionPath);

                if (!string.IsNullOrWhiteSpace(directory))
                {
                    string? candidate =
                        Directory
                            .EnumerateFiles(
                                directory,
                                stem + "*",
                                SearchOption.TopDirectoryOnly)
                            .FirstOrDefault();

                    if (!string.IsNullOrWhiteSpace(candidate))
                    {
                        File.Copy(
                            candidate,
                            solutionPath,
                            overwrite: true);
                        return true;
                    }
                }
            }
            catch
            {
                // Try another method shape or mapping technique.
            }
        }

        return false;
    }

    private static void TrySetWithDiagnostic<T>(
        object problem,
        string propertyName,
        T? value,
        ICollection<string> diagnostics,
        string genericParameterName)
        where T : struct
    {
        if (!value.HasValue)
        {
            return;
        }

        if (!TrySetProperty(
                problem,
                propertyName,
                value.Value))
        {
            diagnostics.Add(
                $"Xpress generic parameter '{genericParameterName}' could " +
                "not be mapped through the loaded API surface.");
        }
    }

    private static void AddDeferred(
        ICollection<string> diagnostics,
        bool condition,
        string parameterName)
    {
        if (!condition)
        {
            return;
        }

        diagnostics.Add(
            $"Generic parameter '{parameterName}' is valid but is not yet " +
            "translated by the reflection-based Xpress adapter.");
    }

    private static bool TrySetProperty(
        object target,
        string name,
        object value)
    {
        PropertyInfo? property =
            target.GetType()
                .GetProperties(
                    BindingFlags.Instance |
                    BindingFlags.Public)
                .FirstOrDefault(
                    candidate =>
                        candidate.CanWrite &&
                        string.Equals(
                            candidate.Name,
                            name,
                            StringComparison.OrdinalIgnoreCase));

        if (property is null)
        {
            return false;
        }

        try
        {
            object converted =
                ConvertForMember(
                    value,
                    property.PropertyType);

            property.SetValue(
                target,
                converted);

            return true;
        }
        catch
        {
            return false;
        }
    }

    private static object? TryGetProperty(
        object target,
        params string[] names)
    {
        foreach (string name in names)
        {
            PropertyInfo? property =
                target.GetType()
                    .GetProperties(
                        BindingFlags.Instance |
                        BindingFlags.Public)
                    .FirstOrDefault(
                        candidate =>
                            candidate.CanRead &&
                            string.Equals(
                                candidate.Name,
                                name,
                                StringComparison.OrdinalIgnoreCase));

            if (property is null)
            {
                continue;
            }

            try
            {
                return property.GetValue(target);
            }
            catch
            {
                // Try next alias.
            }
        }

        return null;
    }

    private static object? TryGetStaticProperty(
        Type type,
        params string[] names)
    {
        foreach (string name in names)
        {
            PropertyInfo? property =
                type.GetProperties(
                        BindingFlags.Static |
                        BindingFlags.Public)
                    .FirstOrDefault(
                        candidate =>
                            candidate.CanRead &&
                            string.Equals(
                                candidate.Name,
                                name,
                                StringComparison.OrdinalIgnoreCase));

            if (property is null)
            {
                continue;
            }

            try
            {
                return property.GetValue(null);
            }
            catch
            {
                // Try next alias.
            }
        }

        return null;
    }

    private static object? InvokeRequired(
        object target,
        string methodName,
        params object?[] arguments)
    {
        MethodInfo method =
            FindCompatibleMethod(
                target.GetType(),
                methodName,
                arguments,
                isStatic: false) ??
            throw new MissingMethodException(
                target.GetType().FullName,
                methodName);

        return method.Invoke(
            target,
            ConvertArguments(
                method,
                arguments));
    }

    private static object? InvokeRequiredStatic(
        Type type,
        string methodName,
        params object?[] arguments)
    {
        MethodInfo method =
            FindCompatibleMethod(
                type,
                methodName,
                arguments,
                isStatic: true) ??
            throw new MissingMethodException(
                type.FullName,
                methodName);

        return method.Invoke(
            null,
            ConvertArguments(
                method,
                arguments));
    }

    private static MethodInfo? FindCompatibleMethod(
        Type type,
        string methodName,
        object?[] arguments,
        bool isStatic)
    {
        BindingFlags flags =
            BindingFlags.Public |
            (isStatic
                ? BindingFlags.Static
                : BindingFlags.Instance);

        return type
            .GetMethods(flags)
            .Where(
                method =>
                    string.Equals(
                        method.Name,
                        methodName,
                        StringComparison.OrdinalIgnoreCase))
            .Where(
                method =>
                    method.GetParameters().Length ==
                    arguments.Length)
            .FirstOrDefault();
    }

    private static MethodInfo? FindMethod(
        Type type,
        string methodName,
        int parameterCount)
    {
        return type
            .GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance)
            .FirstOrDefault(
                method =>
                    string.Equals(
                        method.Name,
                        methodName,
                        StringComparison.OrdinalIgnoreCase) &&
                    method.GetParameters().Length == parameterCount);
    }

    private static object?[] ConvertArguments(
        MethodInfo method,
        object?[] arguments)
    {
        ParameterInfo[] parameters =
            method.GetParameters();

        var converted =
            new object?[arguments.Length];

        for (int index = 0; index < arguments.Length; index++)
        {
            object? value =
                arguments[index];

            converted[index] =
                value is null
                    ? null
                    : ConvertForMember(
                        value,
                        parameters[index].ParameterType);
        }

        return converted;
    }

    private static object ConvertForMember(
        object value,
        Type targetType)
    {
        Type effectiveTarget =
            targetType.IsByRef
                ? targetType.GetElementType() ?? targetType
                : targetType;

        if (effectiveTarget.IsInstanceOfType(value))
        {
            return value;
        }

        if (effectiveTarget.IsEnum)
        {
            if (value is string text)
            {
                return Enum.Parse(
                    effectiveTarget,
                    text,
                    ignoreCase: true);
            }

            object underlying =
                Convert.ChangeType(
                    value,
                    Enum.GetUnderlyingType(effectiveTarget),
                    CultureInfo.InvariantCulture);

            return Enum.ToObject(
                effectiveTarget,
                underlying);
        }

        return Convert.ChangeType(
            value,
            effectiveTarget,
            CultureInfo.InvariantCulture);
    }
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
