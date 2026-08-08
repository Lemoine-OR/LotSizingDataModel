using System;
using System.Collections.Generic;
using ILOG.CPLEX;
using NativeCplex = global::ILOG.CPLEX.Cplex;
using LotSizingDataModel.Solver.Configuration;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Maps normalized solver parameters to stable CPLEX
/// parameters.
/// </summary>
public sealed class CplexParameterMapper
{
    /// <summary>
    /// Applies normalized parameters and returns diagnostics for
    /// parameters intentionally deferred to later adapter
    /// versions.
    /// </summary>
    /// <param name="cplex">
    /// Native CPLEX model.
    /// </param>
    /// <param name="parameters">
    /// Normalized solver parameters.
    /// </param>
    /// <returns>
    /// Diagnostic messages for parameters not translated by this
    /// adapter version.
    /// </returns>
    public IReadOnlyList<string> Apply(
        NativeCplex cplex,
        SolverParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(cplex);
        ArgumentNullException.ThrowIfNull(parameters);

        parameters.EnsureValid();

        var diagnostics =
            new List<string>();

        if (parameters.TimeLimitSeconds.HasValue)
        {
            cplex.SetParam(
                NativeCplex.Param.TimeLimit,
                parameters.TimeLimitSeconds.Value);
        }

        if (parameters.ThreadCount.HasValue)
        {
            cplex.SetParam(
                NativeCplex.Param.Threads,
                parameters.ThreadCount.Value);
        }

        if (parameters.RelativeMipGap.HasValue)
        {
            cplex.SetParam(
                NativeCplex.Param.MIP.Tolerances.MIPGap,
                parameters.RelativeMipGap.Value);
        }

        if (parameters.AbsoluteMipGap.HasValue)
        {
            cplex.SetParam(
                NativeCplex.Param.MIP.Tolerances.AbsMIPGap,
                parameters.AbsoluteMipGap.Value);
        }

        if (parameters.NodeLimit.HasValue)
        {
            cplex.SetParam(
                NativeCplex.Param.MIP.Limits.Nodes,
                parameters.NodeLimit.Value);
        }

        if (parameters.SolutionLimit.HasValue)
        {
            cplex.SetParam(
                NativeCplex.Param.MIP.Limits.Solutions,
                (long)parameters.SolutionLimit.Value);
        }

        AddDeferredDiagnostic(
            diagnostics,
            parameters.RandomSeed.HasValue,
            nameof(parameters.RandomSeed));

        AddDeferredDiagnostic(
            diagnostics,
            parameters.IterationLimit.HasValue,
            nameof(parameters.IterationLimit));

        AddDeferredDiagnostic(
            diagnostics,
            parameters.MemoryLimitMegabytes.HasValue,
            nameof(parameters.MemoryLimitMegabytes));

        AddDeferredDiagnostic(
            diagnostics,
            parameters.EnablePresolve.HasValue,
            nameof(parameters.EnablePresolve));

        AddDeferredDiagnostic(
            diagnostics,
            parameters.EnableCuts.HasValue,
            nameof(parameters.EnableCuts));

        AddDeferredDiagnostic(
            diagnostics,
            parameters.EnableHeuristics.HasValue,
            nameof(parameters.EnableHeuristics));

        AddDeferredDiagnostic(
            diagnostics,
            parameters.DeterministicMode,
            nameof(parameters.DeterministicMode));

        if (parameters.NativeParameters.Count > 0)
        {
            diagnostics.Add(
                "CPLEX native string parameters are not yet " +
                "translated by this adapter version.");
        }

        return diagnostics;
    }

    private static void AddDeferredDiagnostic(
        ICollection<string> diagnostics,
        bool condition,
        string parameterName)
    {
        if (!condition)
        {
            return;
        }

        diagnostics.Add(
            $"Generic parameter '{parameterName}' is valid but " +
            "is not yet translated by this CPLEX adapter version.");
    }
}
