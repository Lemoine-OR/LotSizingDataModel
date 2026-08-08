using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ILOG.CPLEX;
using NativeCplex = global::ILOG.CPLEX.Cplex;
using LotSizingDataModel.Solver.Adapters;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;
using LotSizingDataModel.Solver.Monitoring;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Solves generic linear and mixed-integer linear models with
/// IBM ILOG CPLEX.
/// </summary>
public sealed class CplexSolverAdapter :
    MathematicalModelSolverPluginBase
{
    private readonly object _nativeSyncRoot =
        new();

    private NativeCplex.Aborter? _activeAborter;

    /// <summary>
    /// Initializes the CPLEX solver adapter.
    /// </summary>
    public CplexSolverAdapter()
        : base(
        [
            SolverCapability.LinearProgramming,
            SolverCapability.MixedIntegerLinearProgramming,
            SolverCapability.Interruption,
            SolverCapability.LpExport,
            SolverCapability.MpsExport,
            SolverCapability.OptimalityGapReporting,
            SolverCapability.SearchStatistics
        ])
    {
    }

    /// <summary>
    /// Gets the generic solver kind.
    /// </summary>
    public override SolverKind SolverKind =>
        SolverKind.Cplex;

    /// <summary>
    /// Gets the solver display name.
    /// </summary>
    public override string SolverName =>
        "IBM ILOG CPLEX";

    /// <summary>
    /// Gets the native solver version when available.
    /// </summary>
    public override string SolverVersion
    {
        get
        {
            try
            {
                var cplex =
                    new NativeCplex();

                try
                {
                    return cplex.Version;
                }
                finally
                {
                    cplex.End();
                }
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Gets the unique adapter identifier.
    /// </summary>
    public override string AdapterId =>
        "LotSizingDataModel.Solver.Cplex";

    /// <summary>
    /// Gets the adapter display name.
    /// </summary>
    public override string AdapterName =>
        "LotSizingDataModel CPLEX Adapter";

    /// <summary>
    /// Gets the adapter implementation version.
    /// </summary>
    public override string AdapterVersion =>
        "1.0.0";

    /// <summary>
    /// Gets the minimum supported CPLEX version.
    /// </summary>
    public override string MinimumSupportedSolverVersion =>
        "22.1.1";

    /// <summary>
    /// Checks whether CPLEX can be instantiated and therefore
    /// loaded by the current process.
    /// </summary>
    public override ValueTask<SolverAvailabilityInfo>
        CheckAvailabilityAsync(
            CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        CplexInstallationDiscoveryResult installationDiscovery =
            CplexInstallationLocator.Discover();

        try
        {
            var cplex =
                new NativeCplex();

            try
            {
                var availability =
                    new SolverAvailabilityInfo(
                        SolverKind.Cplex,
                        SolverAvailabilityStatus.Available)
                    {
                        SolverName =
                            SolverName,

                        SolverVersion =
                            cplex.Version
                    };

                if (installationDiscovery.Installation is not null)
                {
                    availability.InstallationPath =
                        installationDiscovery.Installation.RootDirectory;

                    availability.ManagedAssemblyPath =
                        installationDiscovery.Installation
                            .ManagedAssemblyDirectory;

                    availability.AddDiagnostic(
                        $"Compatible CPLEX installation family " +
                        $"'{installationDiscovery.Installation.Version}' " +
                        $"was discovered from " +
                        $"'{installationDiscovery.Installation.DiscoverySource}'.");
                }

                foreach (
                    string diagnostic
                    in installationDiscovery.Diagnostics)
                {
                    availability.AddDiagnostic(
                        diagnostic);
                }

                availability.AddDiagnostic(
                    "The CPLEX managed and native runtime " +
                    "libraries were loaded successfully.");

                return ValueTask.FromResult(
                    availability);
            }
            finally
            {
                cplex.End();
            }
        }
        catch (Exception exception)
        {
            SolverAvailabilityStatus status =
                installationDiscovery.IsFound
                    ? SolverAvailabilityStatus.LoadFailure
                    : SolverAvailabilityStatus.NotInstalled;

            var availability =
                new SolverAvailabilityInfo(
                    SolverKind.Cplex,
                    status)
                {
                    SolverName =
                        SolverName
                };

            if (installationDiscovery.Installation is not null)
            {
                availability.InstallationPath =
                    installationDiscovery.Installation.RootDirectory;

                availability.ManagedAssemblyPath =
                    installationDiscovery.Installation
                        .ManagedAssemblyDirectory;
            }

            foreach (
                string diagnostic
                in installationDiscovery.Diagnostics)
            {
                availability.AddDiagnostic(
                    diagnostic);
            }

            availability.AddDiagnostic(
                exception.Message);

            return ValueTask.FromResult(
                availability);
        }
    }

    /// <summary>
    /// Translates and solves one generic mathematical model.
    /// </summary>
    protected override async ValueTask<MathematicalModelSolveResult>
        SolveCoreAsync(
            MathematicalModelSolveRequest request,
            CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Model);

        var stopwatch =
            Stopwatch.StartNew();

        await PublishProgressAsync(
            request,
            new SolverProgressSnapshot
            {
                Stage =
                    SolverProgressStage.BuildingModel,

                ElapsedSeconds =
                    stopwatch.Elapsed.TotalSeconds,

                Message =
                    "Translating the generic mathematical model " +
                    "to IBM ILOG Concert."
            },
            cancellationToken);

        var translator =
            new CplexModelTranslator();

        CplexModelTranslationResult translation =
            translator.Translate(
                request.Model);

        NativeCplex cplex =
            translation.Cplex;

        var aborter =
            new NativeCplex.Aborter();

        cplex.Use(
            aborter);

        SetActiveAborter(
            aborter);

        try
        {
            var parameterMapper =
                new CplexParameterMapper();

            IReadOnlyList<string> parameterDiagnostics =
                parameterMapper.Apply(
                    cplex,
                    request.Parameters);

            if (request.Parameters.ExportModel)
            {
                string path =
                    ResolveExportPath(
                        request.Parameters.ExportModelPath);

                cplex.ExportModel(
                    path);
            }

            using CancellationTokenRegistration cancellationRegistration =
                cancellationToken.Register(
                    static state =>
                    {
                        ((NativeCplex.Aborter)state!).Abort();
                    },
                    aborter);

            await PublishProgressAsync(
                request,
                new SolverProgressSnapshot
                {
                    Stage =
                        SolverProgressStage.Searching,

                    ElapsedSeconds =
                        stopwatch.Elapsed.TotalSeconds,

                    Message =
                        "CPLEX optimization started."
                },
                cancellationToken);

            bool hasSolution =
                await Task.Run(
                    () => cplex.Solve(),
                    CancellationToken.None);

            stopwatch.Stop();

            var result =
                BuildResult(
                    request.Model,
                    translation,
                    cplex,
                    hasSolution,
                    stopwatch.Elapsed,
                    cancellationToken.IsCancellationRequested);

            foreach (string diagnostic in parameterDiagnostics)
            {
                result.AddDiagnostic(
                    diagnostic);
            }

            await PublishProgressAsync(
                request,
                new SolverProgressSnapshot
                {
                    Stage =
                        SolverProgressStage.Completed,

                    ElapsedSeconds =
                        stopwatch.Elapsed.TotalSeconds,

                    IncumbentObjective =
                        result.ObjectiveValue,

                    BestBound =
                        result.BestBound,

                    AbsoluteGap =
                        result.AbsoluteGap,

                    RelativeGap =
                        result.RelativeGap,

                    ExploredNodeCount =
                        result.ExploredNodeCount,

                    IterationCount =
                        result.IterationCount,

                    Message =
                        $"CPLEX terminated with reason " +
                        $"'{result.TerminationReason}'."
                },
                CancellationToken.None);

            return result;
        }
        finally
        {
            ClearActiveAborter(
                aborter);

            try
            {
                cplex.Remove(
                    aborter);
            }
            catch
            {
                // Model disposal remains the priority.
            }

            aborter.End();
            cplex.End();
        }
    }

    /// <summary>
    /// Requests native CPLEX interruption.
    /// </summary>
    protected override void RequestNativeStop()
    {
        NativeCplex.Aborter? aborter;

        lock (_nativeSyncRoot)
        {
            aborter =
                _activeAborter;
        }

        aborter?.Abort();
    }

    private static MathematicalModelSolveResult BuildResult(
        MathematicalModel model,
        CplexModelTranslationResult translation,
        NativeCplex cplex,
        bool hasSolution,
        TimeSpan solveDuration,
        bool cancellationRequested)
    {
        var result =
            new MathematicalModelSolveResult
            {
                SolverKind =
                    SolverKind.Cplex,

                SolverName =
                    "IBM ILOG CPLEX",

                SolverVersion =
                    cplex.Version,

                SolveDuration =
                    solveDuration,

                HasFeasibleSolution =
                    hasSolution,

                IsOptimal =
                    string.Equals(
                        cplex.GetStatus().ToString(),
                        "Optimal",
                        StringComparison.OrdinalIgnoreCase),

                TerminationReason =
                    MapTerminationReason(
                        cplex.GetStatus().ToString(),
                        cplex.GetCplexStatus().ToString(),
                        hasSolution,
                        cancellationRequested),

                ExploredNodeCount =
                    cplex.Nnodes64,

                IterationCount =
                    cplex.Niterations64
            };

        if (hasSolution)
        {
            result.ObjectiveValue =
                cplex.ObjValue;

            foreach (MathematicalVariable variable in model.Variables)
            {
                double value =
                    cplex.GetValue(
                        translation.GetVariable(
                            variable.Id));

                result.AddVariableValue(
                    new MathematicalVariableValue(
                        variable.Id,
                        value,
                        variable.Name,
                        variable.DomainKey));
            }

            TryPopulateMipStatistics(
                result,
                cplex);
        }

        result.AddDiagnostic(
            $"CPLEX status: {cplex.GetStatus()}.");

        result.AddDiagnostic(
            $"CPLEX detailed status: {cplex.GetCplexStatus()}.");

        return result;
    }

    private static void TryPopulateMipStatistics(
        MathematicalModelSolveResult result,
        NativeCplex cplex)
    {
        try
        {
            result.BestBound =
                cplex.GetBestObjValue();

            if (result.ObjectiveValue.HasValue &&
                result.BestBound.HasValue)
            {
                result.AbsoluteGap =
                    Math.Abs(
                        result.ObjectiveValue.Value -
                        result.BestBound.Value);

                result.RelativeGap =
                    result.AbsoluteGap.Value /
                    (1e-10 +
                     Math.Abs(
                         result.ObjectiveValue.Value));
            }
        }
        catch
        {
            // LPs and some termination states may not expose
            // MIP-specific bound information.
        }
    }

    private static SolverTerminationReason MapTerminationReason(
        string status,
        string subStatus,
        bool hasSolution,
        bool cancellationRequested)
    {
        if (cancellationRequested)
        {
            return SolverTerminationReason.UserInterrupted;
        }

        if (status.Equals(
                "Optimal",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.Optimal;
        }

        if (status.Equals(
                "Infeasible",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.Infeasible;
        }

        if (status.Equals(
                "Unbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.Unbounded;
        }

        if (status.Contains(
                "InfeasibleOrUnbounded",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.InfeasibleOrUnbounded;
        }

        if (subStatus.Contains(
                "TimeLim",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.TimeLimit;
        }

        if (subStatus.Contains(
                "NodeLim",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.NodeLimit;
        }

        if (subStatus.Contains(
                "ItLim",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.IterationLimit;
        }

        if (subStatus.Contains(
                "SolLim",
                StringComparison.OrdinalIgnoreCase))
        {
            return SolverTerminationReason.SolutionLimit;
        }

        return hasSolution
            ? SolverTerminationReason.Feasible
            : SolverTerminationReason.Unknown;
    }

    private static string ResolveExportPath(
        string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(
                configuredPath))
        {
            return Path.GetFullPath(
                "LotSizingModel.lp");
        }

        string path =
            Path.GetFullPath(
                configuredPath);

        string? directory =
            Path.GetDirectoryName(
                path);

        if (!string.IsNullOrWhiteSpace(
                directory))
        {
            Directory.CreateDirectory(
                directory);
        }

        return path;
    }

    private void SetActiveAborter(
        NativeCplex.Aborter aborter)
    {
        lock (_nativeSyncRoot)
        {
            _activeAborter =
                aborter;
        }
    }

    private void ClearActiveAborter(
        NativeCplex.Aborter aborter)
    {
        lock (_nativeSyncRoot)
        {
            if (ReferenceEquals(
                    _activeAborter,
                    aborter))
            {
                _activeAborter =
                    null;
            }
        }
    }
}
