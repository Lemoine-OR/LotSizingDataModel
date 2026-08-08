using System;
using System.Collections.Generic;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Represents the result of compatible CPLEX installation
/// discovery.
/// </summary>
public sealed class CplexInstallationDiscoveryResult
{
    /// <summary>
    /// Initializes a discovery result.
    /// </summary>
    /// <param name="installation">
    /// Selected compatible installation, or
    /// <see langword="null"/>.
    /// </param>
    /// <param name="diagnostics">Discovery diagnostics.</param>
    public CplexInstallationDiscoveryResult(
        CplexInstallationInfo? installation,
        IReadOnlyList<string> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(diagnostics);

        Installation = installation;
        Diagnostics = diagnostics;
    }

    /// <summary>
    /// Gets the selected compatible installation.
    /// </summary>
    public CplexInstallationInfo? Installation { get; }

    /// <summary>
    /// Gets discovery diagnostics.
    /// </summary>
    public IReadOnlyList<string> Diagnostics { get; }

    /// <summary>
    /// Gets a value indicating whether a compatible installation
    /// was found.
    /// </summary>
    public bool IsFound =>
        Installation is not null;
}
