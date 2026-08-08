using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Identifies how a solver installation or adapter location
/// was discovered.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverDiscoverySource")]
public enum SolverDiscoverySource
{
    /// <summary>
    /// The discovery source is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The location was supplied explicitly by the user or by
    /// application configuration.
    /// </summary>
    ExplicitConfiguration = 1,

    /// <summary>
    /// The location was obtained from a solver-specific
    /// environment variable.
    /// </summary>
    EnvironmentVariable = 2,

    /// <summary>
    /// The location was obtained from the operating system
    /// executable search path.
    /// </summary>
    SystemPath = 3,

    /// <summary>
    /// The location was found in the application directory.
    /// </summary>
    ApplicationDirectory = 4,

    /// <summary>
    /// The location was found in the configured plugin
    /// subdirectory.
    /// </summary>
    PluginDirectory = 5,

    /// <summary>
    /// The location was found in a common solver installation
    /// directory.
    /// </summary>
    CommonInstallationDirectory = 6,

    /// <summary>
    /// The location was discovered from an operating-system
    /// installation registry or equivalent mechanism.
    /// </summary>
    OperatingSystemRegistry = 7,

    /// <summary>
    /// The location was inferred from a loaded assembly.
    /// </summary>
    LoadedAssembly = 8
}
