using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Describes the result of loading a solver adapter plugin.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverAdapterLoadStatus")]
public enum SolverAdapterLoadStatus
{
    /// <summary>
    /// The adapter load status is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The adapter was discovered but has not been loaded yet.
    /// </summary>
    Discovered = 1,

    /// <summary>
    /// The adapter assembly file could not be found.
    /// </summary>
    AssemblyNotFound = 2,

    /// <summary>
    /// The adapter assembly could not be loaded.
    /// </summary>
    AssemblyLoadFailure = 3,

    /// <summary>
    /// The requested adapter type could not be found in the
    /// loaded assembly.
    /// </summary>
    AdapterTypeNotFound = 4,

    /// <summary>
    /// The adapter type does not implement the required solver
    /// adapter contract.
    /// </summary>
    InvalidAdapterType = 5,

    /// <summary>
    /// The adapter could not be instantiated.
    /// </summary>
    InstantiationFailure = 6,

    /// <summary>
    /// One or more native solver dependencies could not be
    /// loaded.
    /// </summary>
    NativeDependencyFailure = 7,

    /// <summary>
    /// The adapter was loaded successfully.
    /// </summary>
    Loaded = 8
}
