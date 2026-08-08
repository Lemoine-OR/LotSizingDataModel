using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Configures how a solver adapter is selected when automatic
/// solver selection is requested.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverSelectionOptions")]
public sealed class SolverSelectionOptions
{
    private readonly List<SolverKind> _solverPriority =
        new();

    private readonly List<SolverCapability>
        _requiredCapabilities =
            new();

    /// <summary>
    /// Initializes solver-selection options with the default
    /// solver priority.
    /// </summary>
    public SolverSelectionOptions()
    {
        _solverPriority.Add(
            SolverKind.Cplex);

        _solverPriority.Add(
            SolverKind.Gurobi);

        _solverPriority.Add(
            SolverKind.Xpress);

        _solverPriority.Add(
            SolverKind.CoinOrCbc);

        AllowLimitedAvailability =
            true;

        RequireExactSolverKind =
            false;
    }

    /// <summary>
    /// Gets the solver priority used during automatic
    /// selection.
    /// </summary>
    /// <remarks>
    /// Earlier entries have higher priority.
    /// </remarks>
    [XmlArray("solverPriority")]
    [XmlArrayItem("solver")]
    public List<SolverKind> SolverPriority =>
        _solverPriority;

    /// <summary>
    /// Gets the capabilities that a selected adapter must
    /// support.
    /// </summary>
    [XmlArray("requiredCapabilities")]
    [XmlArrayItem("capability")]
    public List<SolverCapability> RequiredCapabilities =>
        _requiredCapabilities;

    /// <summary>
    /// Gets or sets whether adapters reported as available with
    /// limitations may be selected.
    /// </summary>
    [XmlAttribute("allowLimitedAvailability")]
    public bool AllowLimitedAvailability
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether a preferred concrete solver must be
    /// used without falling back to another solver.
    /// </summary>
    [XmlAttribute("requireExactSolverKind")]
    public bool RequireExactSolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Validates the solver-selection options.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the priority or capability collections are
    /// invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (_solverPriority.Count == 0)
        {
            throw new InvalidOperationException(
                "At least one solver kind must be present in " +
                "the automatic-selection priority.");
        }

        var seenSolvers =
            new HashSet<SolverKind>();

        foreach (
            SolverKind solverKind
            in _solverPriority)
        {
            if (solverKind is
                SolverKind.Unknown or
                SolverKind.Automatic)
            {
                throw new InvalidOperationException(
                    "The solver priority must contain only " +
                    "concrete solver kinds.");
            }

            if (!seenSolvers.Add(
                    solverKind))
            {
                throw new InvalidOperationException(
                    $"Solver kind '{solverKind}' appears more " +
                    "than once in the selection priority.");
            }
        }

        var seenCapabilities =
            new HashSet<SolverCapability>();

        foreach (
            SolverCapability capability
            in _requiredCapabilities)
        {
            if (capability ==
                SolverCapability.Unknown)
            {
                throw new InvalidOperationException(
                    "The required-capability collection cannot " +
                    "contain Unknown.");
            }

            if (!seenCapabilities.Add(
                    capability))
            {
                throw new InvalidOperationException(
                    $"Capability '{capability}' appears more " +
                    "than once in the required-capability " +
                    "collection.");
            }
        }
    }
}
