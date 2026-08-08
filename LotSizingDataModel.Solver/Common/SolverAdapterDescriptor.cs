using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Describes a solver adapter without loading or executing its
/// native optimization engine.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverAdapterDescriptor")]
public sealed class SolverAdapterDescriptor
{
    private readonly List<SolverCapability> _capabilities =
        new();

    /// <summary>
    /// Initializes an empty solver-adapter descriptor.
    /// </summary>
    public SolverAdapterDescriptor()
    {
        AdapterId =
            string.Empty;

        AdapterName =
            string.Empty;

        AdapterVersion =
            string.Empty;

        MinimumSupportedSolverVersion =
            string.Empty;

        AssemblyPath =
            string.Empty;

        TypeName =
            string.Empty;
    }

    /// <summary>
    /// Gets or sets the unique adapter identifier.
    /// </summary>
    [XmlAttribute("adapterId")]
    public string AdapterId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the adapter display name.
    /// </summary>
    [XmlElement("adapterName")]
    public string AdapterName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the adapter implementation version.
    /// </summary>
    [XmlElement("adapterVersion")]
    public string AdapterVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver kind implemented by the adapter.
    /// </summary>
    [XmlAttribute("solverKind")]
    public SolverKind SolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the minimum supported native solver
    /// version.
    /// </summary>
    [XmlElement("minimumSupportedSolverVersion")]
    public string MinimumSupportedSolverVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the full path of the adapter assembly.
    /// </summary>
    [XmlElement("assemblyPath")]
    public string AssemblyPath
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the assembly-qualified adapter type name.
    /// </summary>
    [XmlElement("typeName")]
    public string TypeName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the capabilities declared by the adapter.
    /// </summary>
    [XmlArray("capabilities")]
    [XmlArrayItem("capability")]
    public List<SolverCapability> Capabilities =>
        _capabilities;

    /// <summary>
    /// Validates the adapter descriptor.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when required metadata is missing or invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(
                AdapterId))
        {
            throw new InvalidOperationException(
                "A solver adapter identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(
                AdapterName))
        {
            throw new InvalidOperationException(
                "A solver adapter name is required.");
        }

        if (string.IsNullOrWhiteSpace(
                AdapterVersion))
        {
            throw new InvalidOperationException(
                "A solver adapter version is required.");
        }

        if (SolverKind is
            SolverKind.Unknown or
            SolverKind.Automatic)
        {
            throw new InvalidOperationException(
                "A solver adapter must target a concrete " +
                "solver kind.");
        }

        foreach (
            SolverCapability capability
            in _capabilities)
        {
            if (capability ==
                SolverCapability.Unknown)
            {
                throw new InvalidOperationException(
                    "The capability collection cannot contain " +
                    "Unknown.");
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether the adapter declares the
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
}
