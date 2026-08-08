using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Configures how solver adapter plugins and native solver
/// installations are discovered on the current computer.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverDiscoveryOptions")]
public sealed class SolverDiscoveryOptions
{
    private readonly List<string> _adapterSearchDirectories =
        new();

    private readonly List<string> _solverSearchDirectories =
        new();

    /// <summary>
    /// Initializes solver-discovery options with recommended
    /// defaults.
    /// </summary>
    public SolverDiscoveryOptions()
    {
        SearchApplicationDirectory =
            true;

        SearchPluginSubdirectory =
            true;

        PluginSubdirectoryName =
            "Plugins";

        SearchEnvironmentVariables =
            true;

        SearchSystemPath =
            true;

        SearchCommonInstallationDirectories =
            true;

        RecursiveAdapterSearch =
            false;

        ValidateLicenses =
            true;
    }

    /// <summary>
    /// Gets or sets whether the application directory is
    /// searched for solver adapter assemblies.
    /// </summary>
    [XmlAttribute("searchApplicationDirectory")]
    public bool SearchApplicationDirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether a plugin subdirectory of the
    /// application directory is searched.
    /// </summary>
    [XmlAttribute("searchPluginSubdirectory")]
    public bool SearchPluginSubdirectory
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the plugin subdirectory name.
    /// </summary>
    [XmlElement("pluginSubdirectoryName")]
    public string PluginSubdirectoryName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether known solver environment variables
    /// are inspected.
    /// </summary>
    [XmlAttribute("searchEnvironmentVariables")]
    public bool SearchEnvironmentVariables
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether directories listed in the operating
    /// system PATH are inspected.
    /// </summary>
    [XmlAttribute("searchSystemPath")]
    public bool SearchSystemPath
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether common installation directories are
    /// inspected.
    /// </summary>
    [XmlAttribute("searchCommonInstallationDirectories")]
    public bool SearchCommonInstallationDirectories
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether adapter search directories are
    /// scanned recursively.
    /// </summary>
    [XmlAttribute("recursiveAdapterSearch")]
    public bool RecursiveAdapterSearch
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether solver-license availability is
    /// checked during discovery.
    /// </summary>
    [XmlAttribute("validateLicenses")]
    public bool ValidateLicenses
    {
        get;
        set;
    }

    /// <summary>
    /// Gets additional directories searched for solver adapter
    /// assemblies.
    /// </summary>
    [XmlArray("adapterSearchDirectories")]
    [XmlArrayItem("directory")]
    public List<string> AdapterSearchDirectories =>
        _adapterSearchDirectories;

    /// <summary>
    /// Gets additional directories searched for native solver
    /// installations, libraries, and executables.
    /// </summary>
    [XmlArray("solverSearchDirectories")]
    [XmlArrayItem("directory")]
    public List<string> SolverSearchDirectories =>
        _solverSearchDirectories;

    /// <summary>
    /// Validates the solver-discovery options.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when one or more configured values are invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (SearchPluginSubdirectory &&
            string.IsNullOrWhiteSpace(
                PluginSubdirectoryName))
        {
            throw new InvalidOperationException(
                "A plugin subdirectory name is required when " +
                "plugin-subdirectory discovery is enabled.");
        }

        ValidateDirectoryCollection(
            _adapterSearchDirectories,
            "adapter search");

        ValidateDirectoryCollection(
            _solverSearchDirectories,
            "solver search");
    }

    private static void ValidateDirectoryCollection(
        IEnumerable<string> directories,
        string collectionName)
    {
        foreach (
            string directory
            in directories)
        {
            if (string.IsNullOrWhiteSpace(
                    directory))
            {
                throw new InvalidOperationException(
                    $"The {collectionName} directory collection " +
                    "cannot contain an empty value.");
            }
        }
    }
}
