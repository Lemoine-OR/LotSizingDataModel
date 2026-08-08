using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Represents the result of building the solver runtime from
/// discovery information and dynamically loaded adapter
/// plugins.
/// </summary>
public sealed class SolverRuntimeBuildResult
{
    private readonly List<SolverAdapterLoadResult>
        _adapterLoadResults =
            new();

    private readonly List<string>
        _diagnostics =
            new();

    /// <summary>
    /// Initializes an empty solver-runtime build result.
    /// </summary>
    public SolverRuntimeBuildResult()
    {
    }

    /// <summary>
    /// Gets or sets the discovery result used to build the
    /// runtime.
    /// </summary>
    public SolverDiscoveryResult? DiscoveryResult
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the runtime context created from the
    /// discovered and loaded solver infrastructure.
    /// </summary>
    public SolverRuntimeContext? RuntimeContext
    {
        get;
        set;
    }

    /// <summary>
    /// Gets all adapter-loading attempts performed while
    /// building the runtime.
    /// </summary>
    public List<SolverAdapterLoadResult>
        AdapterLoadResults =>
            _adapterLoadResults;

    /// <summary>
    /// Gets diagnostic messages produced while building the
    /// runtime.
    /// </summary>
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets a value indicating whether a runtime context was
    /// created successfully.
    /// </summary>
    public bool IsSuccessful =>
        RuntimeContext is not null;

    /// <summary>
    /// Gets a value indicating whether at least one adapter was
    /// loaded successfully.
    /// </summary>
    public bool HasLoadedAdapter =>
        _adapterLoadResults.Any(
            result =>
                result.IsLoaded);

    /// <summary>
    /// Gets the number of successfully loaded adapters.
    /// </summary>
    public int LoadedAdapterCount =>
        _adapterLoadResults.Count(
            result =>
                result.IsLoaded);

    /// <summary>
    /// Gets a value indicating whether the resulting runtime
    /// can currently execute at least one solver.
    /// </summary>
    public bool CanSolve =>
        RuntimeContext?.CanSolve ==
            true;

    /// <summary>
    /// Adds an adapter-loading result.
    /// </summary>
    /// <param name="loadResult">
    /// Adapter-loading result to retain.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="loadResult"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void AddAdapterLoadResult(
        SolverAdapterLoadResult loadResult)
    {
        ArgumentNullException.ThrowIfNull(
            loadResult);

        _adapterLoadResults.Add(
            loadResult);
    }

    /// <summary>
    /// Adds a diagnostic message.
    /// </summary>
    /// <param name="message">
    /// Diagnostic message.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="message"/> is empty.
    /// </exception>
    public void AddDiagnostic(
        string message)
    {
        if (string.IsNullOrWhiteSpace(
                message))
        {
            throw new ArgumentException(
                "A solver-runtime diagnostic cannot be empty.",
                nameof(message));
        }

        _diagnostics.Add(
            message.Trim());
    }

    /// <summary>
    /// Adds a collection of diagnostic messages.
    /// </summary>
    /// <param name="diagnostics">
    /// Diagnostic messages to append.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="diagnostics"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void AddDiagnostics(
        IEnumerable<string> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(
            diagnostics);

        foreach (
            string diagnostic
            in diagnostics)
        {
            if (!string.IsNullOrWhiteSpace(
                    diagnostic))
            {
                _diagnostics.Add(
                    diagnostic.Trim());
            }
        }
    }
}
