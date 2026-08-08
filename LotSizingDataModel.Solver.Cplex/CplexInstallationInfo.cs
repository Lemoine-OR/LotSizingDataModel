using System;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Represents one compatible CPLEX installation discovered on
/// the current machine.
/// </summary>
public sealed class CplexInstallationInfo
{
    /// <summary>
    /// Initializes installation information.
    /// </summary>
    /// <param name="version">Detected installation family.</param>
    /// <param name="rootDirectory">CPLEX Studio root directory.</param>
    /// <param name="managedAssemblyDirectory">
    /// Directory containing ILOG.Concert.dll and ILOG.CPLEX.dll.
    /// </param>
    /// <param name="discoverySource">Discovery source.</param>
    public CplexInstallationInfo(
        string version,
        string rootDirectory,
        string managedAssemblyDirectory,
        string discoverySource)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(version);
        ArgumentException.ThrowIfNullOrWhiteSpace(rootDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(managedAssemblyDirectory);
        ArgumentException.ThrowIfNullOrWhiteSpace(discoverySource);

        Version = version;
        RootDirectory = rootDirectory;
        ManagedAssemblyDirectory = managedAssemblyDirectory;
        DiscoverySource = discoverySource;
    }

    /// <summary>
    /// Gets the detected CPLEX version family.
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Gets the CPLEX Studio root directory.
    /// </summary>
    public string RootDirectory { get; }

    /// <summary>
    /// Gets the managed-assembly directory.
    /// </summary>
    public string ManagedAssemblyDirectory { get; }

    /// <summary>
    /// Gets the source through which the installation was found.
    /// </summary>
    public string DiscoverySource { get; }
}
