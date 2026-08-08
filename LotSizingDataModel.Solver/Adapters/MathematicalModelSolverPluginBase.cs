using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Contracts;
using LotSizingDataModel.Solver.Events;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Monitoring;

namespace LotSizingDataModel.Solver.Adapters;

/// <summary>
/// Provides the common plugin contract shared by native
/// mathematical-model solver adapters.
/// </summary>
/// <remarks>
/// <para>
/// The class combines the dynamically discoverable
/// <see cref="ISolverAdapter"/> contract with the
/// solver-independent mathematical-model execution implemented
/// by <see cref="MathematicalModelSolverAdapterBase"/>.
/// </para>
/// <para>
/// Native plugins should derive from this class and implement
/// availability checking plus the native mathematical-model
/// translation and optimization logic.
/// </para>
/// <para>
/// High-level lot-sizing-instance orchestration remains the
/// responsibility of <see cref="LotSizingSolverService"/>.
/// Native plugins must not rebuild lot-sizing equations.
/// </para>
/// </remarks>
public abstract class MathematicalModelSolverPluginBase :
    MathematicalModelSolverAdapterBase,
    IMathematicalModelSolverAdapter
{
    private readonly IReadOnlyCollection<SolverCapability>
        _capabilities;

    /// <summary>
    /// Initializes the plugin base with its declared
    /// capabilities.
    /// </summary>
    /// <param name="capabilities">
    /// Solver capabilities implemented by the plugin.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="capabilities"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the collection contains
    /// <see cref="SolverCapability.Unknown"/>.
    /// </exception>
    protected MathematicalModelSolverPluginBase(
        IEnumerable<SolverCapability> capabilities)
    {
        ArgumentNullException.ThrowIfNull(
            capabilities);

        SolverCapability[] normalizedCapabilities =
            capabilities
                .Distinct()
                .ToArray();

        if (normalizedCapabilities.Contains(
                SolverCapability.Unknown))
        {
            throw new InvalidOperationException(
                "A solver plugin cannot declare the Unknown " +
                "capability.");
        }

        _capabilities =
            Array.AsReadOnly(
                normalizedCapabilities);
    }

    /// <summary>
    /// Occurs when a new solver progress snapshot is available.
    /// </summary>
    public event EventHandler<SolverProgressEventArgs>?
        ProgressChanged;

    /// <summary>
    /// Gets the unique adapter identifier.
    /// </summary>
    public abstract string AdapterId
    {
        get;
    }

    /// <summary>
    /// Gets the adapter display name.
    /// </summary>
    public abstract string AdapterName
    {
        get;
    }

    /// <summary>
    /// Gets the adapter implementation version.
    /// </summary>
    public abstract string AdapterVersion
    {
        get;
    }

    /// <summary>
    /// Gets the minimum supported native solver version.
    /// </summary>
    public abstract string MinimumSupportedSolverVersion
    {
        get;
    }

    /// <summary>
    /// Gets the capabilities implemented by this plugin.
    /// </summary>
    public IReadOnlyCollection<SolverCapability> Capabilities =>
        _capabilities;

    /// <summary>
    /// Gets a value indicating whether the plugin supports the
    /// specified capability.
    /// </summary>
    /// <param name="capability">
    /// Capability to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the capability is declared;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool SupportsCapability(
        SolverCapability capability)
    {
        return _capabilities.Contains(
            capability);
    }

    /// <summary>
    /// Checks whether the native solver is installed, loadable,
    /// and licensed when a license is required.
    /// </summary>
    /// <param name="cancellationToken">
    /// Token used to cancel the availability check.
    /// </param>
    /// <returns>
    /// Native solver availability information.
    /// </returns>
    public abstract ValueTask<SolverAvailabilityInfo>
        CheckAvailabilityAsync(
            CancellationToken cancellationToken = default);

    /// <summary>
    /// Rejects direct high-level instance solving through the
    /// plugin.
    /// </summary>
    /// <remarks>
    /// Native plugins receive an already built mathematical
    /// model. Applications should use
    /// <see cref="LotSizingSolverService"/> to build the selected
    /// formulation, select a plugin, solve the mathematical
    /// model, and map the solution back to
    /// LotSizingSolution.
    /// </remarks>
    /// <param name="request">
    /// High-level lot-sizing request.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// This method never returns successfully.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// Always thrown because a native mathematical-model plugin
    /// must not rebuild lot-sizing formulations.
    /// </exception>
    Task<SolverRunResult> ILotSizingSolver.SolveAsync(
        SolverRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        throw new NotSupportedException(
            $"Adapter '{AdapterName}' executes already-built " +
            "mathematical models. Use LotSizingSolverService " +
            "for complete lot-sizing-instance orchestration.");
    }

    /// <summary>
    /// Publishes a progress snapshot to the .NET event and all
    /// observers attached to the mathematical solve request.
    /// </summary>
    /// <param name="request">
    /// Active mathematical-model solve request.
    /// </param>
    /// <param name="snapshot">
    /// Progress snapshot to publish.
    /// </param>
    /// <param name="cancellationToken">
    /// Token observed while notifying observers.
    /// </param>
    /// <returns>
    /// Task representing observer notification.
    /// </returns>
    protected async ValueTask PublishProgressAsync(
        MathematicalModelSolveRequest request,
        SolverProgressSnapshot snapshot,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            request);

        ArgumentNullException.ThrowIfNull(
            snapshot);

        ProgressChanged?.Invoke(
            this,
            new SolverProgressEventArgs(
                snapshot));

        foreach (
            ISolverProgressObserver observer
            in request.ProgressObservers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await observer.OnProgressAsync(
                snapshot,
                cancellationToken);
        }
    }
}
