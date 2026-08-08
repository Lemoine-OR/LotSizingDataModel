using System;
using System.Collections.Generic;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Describes one supported CPLEX installation family.
/// </summary>
public sealed class CplexInstallationDefinition
{
    /// <summary>
    /// Initializes an installation definition.
    /// </summary>
    /// <param name="version">Display version.</param>
    /// <param name="environmentVariables">
    /// Environment variables accepted for this version.
    /// </param>
    /// <param name="windowsFolderNames">
    /// Common Windows installation folder names.
    /// </param>
    public CplexInstallationDefinition(
        string version,
        IReadOnlyList<string> environmentVariables,
        IReadOnlyList<string> windowsFolderNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentNullException.ThrowIfNull(environmentVariables);
        ArgumentNullException.ThrowIfNull(windowsFolderNames);

        Version = version;
        EnvironmentVariables = environmentVariables;
        WindowsFolderNames = windowsFolderNames;
    }

    /// <summary>
    /// Gets the display version.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets accepted environment-variable names.
    /// </summary>
    public IReadOnlyList<string> EnvironmentVariables { get; }

    /// <summary>
    /// Gets common Windows installation folder names.
    /// </summary>
    public IReadOnlyList<string> WindowsFolderNames { get; }
}
