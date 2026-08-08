using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Describes the availability state of a solver adapter
/// on the current computer.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverAvailabilityStatus")]
public enum SolverAvailabilityStatus
{
    /// <summary>
    /// The solver availability has not been checked.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The solver is not installed or cannot be located.
    /// </summary>
    NotInstalled = 1,

    /// <summary>
    /// The solver installation was detected, but one or more
    /// required managed or native libraries are missing.
    /// </summary>
    LibrariesMissing = 2,

    /// <summary>
    /// The solver libraries were found, but they could not be
    /// loaded by the current process.
    /// </summary>
    LoadFailure = 3,

    /// <summary>
    /// The solver was detected, but no valid license is
    /// currently available.
    /// </summary>
    LicenseUnavailable = 4,

    /// <summary>
    /// The solver is available with functional limitations,
    /// such as a restricted license or reduced callback support.
    /// </summary>
    AvailableWithLimitations = 5,

    /// <summary>
    /// The solver is installed, loadable, licensed when
    /// required, and available for use.
    /// </summary>
    Available = 6
}
