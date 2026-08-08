using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Configuration;
using LotSizingDataModel.Solver.Discovery;
using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Executes the complete standard lot-sizing workflow against an
/// existing instance.
/// </summary>
/// <remarks>
/// This helper is intended for integration validation. It checks
/// the same path used by an application:
/// formulation registration, solver discovery, adapter loading,
/// mathematical-model construction, native optimization, and
/// normalized solution mapping.
/// </remarks>
public static class StandardLotSizingIntegrationCheck
{
    /// <summary>
    /// Runs the integration check with default formulation,
    /// discovery, and solver parameters.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to solve.
    /// </param>
    /// <param name="preferredSolver">
    /// Preferred solver. The default is automatic selection.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Complete integration-check result.
    /// </returns>
    public static ValueTask<StandardLotSizingIntegrationCheckResult>
        RunAsync(
            LotSizingInstance instance,
            SolverKind preferredSolver = SolverKind.Automatic,
            CancellationToken cancellationToken = default)
    {
        return RunAsync(
            instance,
            new StandardLotSizingFormulationOptions(),
            new SolverDiscoveryOptions(),
            new SolverParameters(),
            preferredSolver,
            cancellationToken);
    }

    /// <summary>
    /// Runs the integration check with explicit configuration.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to solve.
    /// </param>
    /// <param name="formulationOptions">
    /// Standard formulation options.
    /// </param>
    /// <param name="discoveryOptions">
    /// Solver discovery options.
    /// </param>
    /// <param name="solverParameters">
    /// Normalized solver parameters.
    /// </param>
    /// <param name="preferredSolver">
    /// Preferred solver.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Complete integration-check result.
    /// </returns>
    public static async ValueTask<StandardLotSizingIntegrationCheckResult>
        RunAsync(
            LotSizingInstance instance,
            StandardLotSizingFormulationOptions formulationOptions,
            SolverDiscoveryOptions discoveryOptions,
            SolverParameters solverParameters,
            SolverKind preferredSolver,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(formulationOptions);
        ArgumentNullException.ThrowIfNull(discoveryOptions);
        ArgumentNullException.ThrowIfNull(solverParameters);

        StandardLotSizingSolverBootstrapResult bootstrap =
            await StandardLotSizingSolverBootstrapper.InitializeAsync(
                formulationOptions,
                discoveryOptions,
                cancellationToken);

        if (!bootstrap.CanSolve ||
            bootstrap.SolverService is null)
        {
            return new StandardLotSizingIntegrationCheckResult(
                bootstrap,
                null);
        }

        var request =
            new SolverRequest(instance)
            {
                PreferredSolver = preferredSolver,
                FormulationName = "standard",
                RunName = "Standard lot-sizing integration check",
                Parameters = solverParameters
            };

        SolverRunResult runResult =
            await bootstrap.SolverService.SolveAsync(
                request,
                cancellationToken);

        return new StandardLotSizingIntegrationCheckResult(
            bootstrap,
            runResult);
    }
}
