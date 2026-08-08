using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Contracts;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Discovers solver infrastructure, loads adapter plugins, and
/// validates loaded adapters against their native solver
/// runtimes.
/// </summary>
public sealed class SolverRuntimeBuilder
{
    private readonly ISolverDiscoveryService
        _discoveryService;

    private readonly SolverAdapterLoader
        _adapterLoader;

    /// <summary>
    /// Initializes a solver-runtime builder with default
    /// discovery and adapter-loading services.
    /// </summary>
    public SolverRuntimeBuilder()
        : this(
            new DefaultSolverDiscoveryService(),
            new SolverAdapterLoader())
    {
    }

    /// <summary>
    /// Initializes a solver-runtime builder with the supplied
    /// discovery service and the default adapter loader.
    /// </summary>
    /// <param name="discoveryService">
    /// Service responsible for solver discovery.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="discoveryService"/> is
    /// <see langword="null"/>.
    /// </exception>
    public SolverRuntimeBuilder(
        ISolverDiscoveryService discoveryService)
        : this(
            discoveryService,
            new SolverAdapterLoader())
    {
    }

    /// <summary>
    /// Initializes a solver-runtime builder with explicit
    /// dependencies.
    /// </summary>
    /// <param name="discoveryService">
    /// Service responsible for solver discovery.
    /// </param>
    /// <param name="adapterLoader">
    /// Loader used to instantiate discovered adapter plugins.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when a dependency is <see langword="null"/>.
    /// </exception>
    public SolverRuntimeBuilder(
        ISolverDiscoveryService discoveryService,
        SolverAdapterLoader adapterLoader)
    {
        ArgumentNullException.ThrowIfNull(
            discoveryService);

        ArgumentNullException.ThrowIfNull(
            adapterLoader);

        _discoveryService =
            discoveryService;

        _adapterLoader =
            adapterLoader;
    }

    /// <summary>
    /// Discovers solver infrastructure, loads adapters, and
    /// performs adapter-level native availability validation.
    /// </summary>
    /// <param name="options">
    /// Solver-discovery options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel discovery and validation.
    /// </param>
    /// <returns>
    /// Complete solver-runtime build result.
    /// </returns>
    public async ValueTask<SolverRuntimeBuildResult> BuildAsync(
        SolverDiscoveryOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        options.EnsureValid();

        var buildResult =
            new SolverRuntimeBuildResult();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            SolverDiscoveryResult discoveryResult =
                await _discoveryService.DiscoverAsync(
                    options,
                    cancellationToken);

            buildResult.DiscoveryResult =
                discoveryResult;

            buildResult.AddDiagnostics(
                discoveryResult.Diagnostics);

            var registry =
                new SolverAdapterRegistry();

            var validatedAvailability =
                discoveryResult.AvailabilityInformation
                    .ToDictionary(
                        item =>
                            item.SolverKind,
                        item =>
                            item);

            foreach (
                SolverAdapterDescriptor descriptor
                in discoveryResult.AdapterDescriptors)
            {
                cancellationToken.ThrowIfCancellationRequested();

                SolverAdapterLoadResult loadResult =
                    _adapterLoader.Load(
                        descriptor);

                buildResult.AddAdapterLoadResult(
                    loadResult);

                buildResult.AddDiagnostics(
                    loadResult.Diagnostics);

                if (!loadResult.IsLoaded ||
                    loadResult.Adapter is null)
                {
                    buildResult.AddDiagnostic(
                        $"Solver adapter '{descriptor.AdapterId}' " +
                        $"was not loaded. Status: " +
                        $"{loadResult.Status}.");

                    continue;
                }

                ISolverAdapter adapter =
                    loadResult.Adapter;

                SolverAvailabilityInfo availability;

                try
                {
                    availability =
                        await adapter.CheckAvailabilityAsync(
                            cancellationToken);

                    if (availability.SolverKind !=
                        adapter.SolverKind)
                    {
                        buildResult.AddDiagnostic(
                            $"Adapter '{adapter.AdapterId}' returned " +
                            $"availability for {availability.SolverKind} " +
                            $"instead of {adapter.SolverKind}. The " +
                            "availability record was normalized.");

                        availability.SolverKind =
                            adapter.SolverKind;
                    }

                    validatedAvailability[
                        adapter.SolverKind] =
                            availability;

                    buildResult.AddDiagnostics(
                        availability.Diagnostics);

                    if (!availability.IsUsable)
                    {
                        buildResult.AddDiagnostic(
                            $"Solver adapter '{adapter.AdapterId}' was " +
                            $"loaded, but native solver " +
                            $"'{adapter.SolverName}' is not usable. " +
                            $"Status: {availability.Status}.");

                        continue;
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    availability =
                        new SolverAvailabilityInfo(
                            adapter.SolverKind,
                            SolverAvailabilityStatus.LoadFailure)
                        {
                            SolverName =
                                adapter.SolverName,

                            SolverVersion =
                                adapter.SolverVersion
                        };

                    availability.AddDiagnostic(
                        $"Adapter-level availability validation " +
                        $"failed: {exception.Message}");

                    validatedAvailability[
                        adapter.SolverKind] =
                            availability;

                    buildResult.AddDiagnostic(
                        $"Solver adapter '{adapter.AdapterId}' could " +
                        "not validate its native solver runtime.");

                    continue;
                }

                if (!registry.TryRegister(
                        loadResult))
                {
                    buildResult.AddDiagnostic(
                        $"Solver adapter '{adapter.AdapterId}' was " +
                        "loaded and validated but could not be " +
                        "registered, most likely because another " +
                        "adapter with the same identifier is already " +
                        "registered.");

                    continue;
                }

                buildResult.AddDiagnostic(
                    $"Solver adapter '{adapter.AdapterId}' for " +
                    $"{adapter.SolverKind} was loaded, validated, " +
                    "and registered successfully.");
            }

            SolverAvailabilityInfo[] finalAvailability =
                GetConcreteSolverKinds()
                    .Select(
                        solverKind =>
                            validatedAvailability.TryGetValue(
                                solverKind,
                                out SolverAvailabilityInfo? info)
                                ? info
                                : new SolverAvailabilityInfo(
                                    solverKind,
                                    SolverAvailabilityStatus.NotInstalled))
                    .ToArray();

            buildResult.RuntimeContext =
                new SolverRuntimeContext(
                    registry,
                    finalAvailability);

            if (registry.Count == 0)
            {
                buildResult.AddDiagnostic(
                    "No usable solver adapter was registered.");
            }

            if (!buildResult.RuntimeContext.CanSolve)
            {
                buildResult.AddDiagnostic(
                    "The current runtime cannot solve a model " +
                    "because no loaded and validated adapter " +
                    "matches a usable native solver.");
            }

            return buildResult;
        }
        catch (OperationCanceledException)
        {
            buildResult.AddDiagnostic(
                "Solver-runtime construction was cancelled.");

            return buildResult;
        }
        catch (Exception exception)
        {
            buildResult.AddDiagnostic(
                $"Solver-runtime construction failed: " +
                $"{exception.Message}");

            buildResult.AddDiagnostic(
                exception.ToString());

            return buildResult;
        }
    }

    /// <summary>
    /// Returns every concrete solver kind supported by the
    /// generic runtime.
    /// </summary>
    private static IReadOnlyList<SolverKind>
        GetConcreteSolverKinds()
    {
        return
        [
            SolverKind.Cplex,
            SolverKind.Gurobi,
            SolverKind.Xpress,
            SolverKind.CoinOrCbc
        ];
    }
}
