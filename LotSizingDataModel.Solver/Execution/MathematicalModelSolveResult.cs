using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Stores the solver result for a solver-independent
/// mathematical model.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalModelSolveResult")]
public sealed class MathematicalModelSolveResult
{
    private readonly List<MathematicalVariableValue> _variableValues =
        new();

    private readonly List<string> _diagnostics =
        new();

    /// <summary>
    /// Initializes an empty mathematical-model solve result.
    /// </summary>
    public MathematicalModelSolveResult()
    {
        RunName =
            string.Empty;

        FormulationId =
            string.Empty;

        SolverName =
            string.Empty;

        SolverVersion =
            string.Empty;
    }

    /// <summary>
    /// Gets or sets the solve-run name.
    /// </summary>
    [XmlAttribute("runName")]
    public string RunName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the identifier of the formulation that
    /// generated the mathematical model.
    /// </summary>
    [XmlAttribute("formulationId")]
    public string FormulationId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver kind.
    /// </summary>
    [XmlAttribute("solverKind")]
    public SolverKind SolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver name.
    /// </summary>
    [XmlAttribute("solverName")]
    public string SolverName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver version.
    /// </summary>
    [XmlAttribute("solverVersion")]
    public string SolverVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the termination reason.
    /// </summary>
    [XmlAttribute("terminationReason")]
    public SolverTerminationReason TerminationReason
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the solver
    /// produced at least one feasible solution.
    /// </summary>
    [XmlAttribute("hasFeasibleSolution")]
    public bool HasFeasibleSolution
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether optimality was
    /// proven.
    /// </summary>
    [XmlAttribute("isOptimal")]
    public bool IsOptimal
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the objective value.
    /// </summary>
    [XmlAttribute("objectiveValue")]
    public double? ObjectiveValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the best objective bound.
    /// </summary>
    [XmlAttribute("bestBound")]
    public double? BestBound
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the relative optimality gap.
    /// </summary>
    [XmlAttribute("relativeGap")]
    public double? RelativeGap
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the absolute optimality gap.
    /// </summary>
    [XmlAttribute("absoluteGap")]
    public double? AbsoluteGap
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver wall-clock duration.
    /// </summary>
    [XmlIgnore]
    public TimeSpan SolveDuration
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver wall-clock duration in
    /// milliseconds for XML serialization.
    /// </summary>
    [XmlAttribute("solveDurationMilliseconds")]
    public double SolveDurationMilliseconds
    {
        get =>
            SolveDuration.TotalMilliseconds;

        set =>
            SolveDuration =
                TimeSpan.FromMilliseconds(
                    value);
    }

    /// <summary>
    /// Gets or sets the number of explored branch-and-bound
    /// nodes, when available.
    /// </summary>
    [XmlAttribute("exploredNodeCount")]
    public long? ExploredNodeCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of simplex or barrier
    /// iterations, when available.
    /// </summary>
    [XmlAttribute("iterationCount")]
    public long? IterationCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the mathematical-variable values returned by the
    /// solver.
    /// </summary>
    [XmlArray("variableValues")]
    [XmlArrayItem("variableValue")]
    public List<MathematicalVariableValue> VariableValues =>
        _variableValues;

    /// <summary>
    /// Gets diagnostic messages produced while solving.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Adds a variable value.
    /// </summary>
    /// <param name="variableValue">
    /// Variable value to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="variableValue"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the same mathematical variable identifier is
    /// already present.
    /// </exception>
    public void AddVariableValue(
        MathematicalVariableValue variableValue)
    {
        ArgumentNullException.ThrowIfNull(
            variableValue);

        variableValue.EnsureValid();

        if (_variableValues.Any(
                existingValue =>
                    existingValue.VariableId ==
                    variableValue.VariableId))
        {
            throw new InvalidOperationException(
                $"A value for mathematical variable identifier " +
                $"'{variableValue.VariableId}' is already stored.");
        }

        _variableValues.Add(
            variableValue);
    }

    /// <summary>
    /// Adds a non-empty diagnostic message.
    /// </summary>
    /// <param name="diagnostic">
    /// Diagnostic message.
    /// </param>
    public void AddDiagnostic(
        string diagnostic)
    {
        if (!string.IsNullOrWhiteSpace(
                diagnostic))
        {
            _diagnostics.Add(
                diagnostic.Trim());
        }
    }

    /// <summary>
    /// Finds a variable value by mathematical-variable
    /// identifier.
    /// </summary>
    /// <param name="variableId">
    /// Mathematical-variable identifier.
    /// </param>
    /// <returns>
    /// Matching value, or <see langword="null"/> when no value is
    /// stored for the supplied identifier.
    /// </returns>
    public MathematicalVariableValue? FindVariableValue(
        int variableId)
    {
        return _variableValues.FirstOrDefault(
            variableValue =>
                variableValue.VariableId ==
                variableId);
    }

    /// <summary>
    /// Validates the mathematical-model solve result.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result contains inconsistent numerical
    /// values or duplicate variable identifiers.
    /// </exception>
    public void EnsureValid()
    {
        EnsureFiniteNullable(
            ObjectiveValue,
            nameof(ObjectiveValue));

        EnsureFiniteNullable(
            BestBound,
            nameof(BestBound));

        EnsureFiniteNullable(
            RelativeGap,
            nameof(RelativeGap));

        EnsureFiniteNullable(
            AbsoluteGap,
            nameof(AbsoluteGap));

        if (SolveDuration < TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Solver duration cannot be negative.");
        }

        if (HasFeasibleSolution &&
            !ObjectiveValue.HasValue)
        {
            throw new InvalidOperationException(
                "A feasible solver result must provide an " +
                "objective value.");
        }

        if (IsOptimal &&
            !HasFeasibleSolution)
        {
            throw new InvalidOperationException(
                "An optimal solver result must contain a feasible " +
                "solution.");
        }

        var variableIds =
            new HashSet<int>();

        foreach (
            MathematicalVariableValue variableValue
            in _variableValues)
        {
            if (variableValue is null)
            {
                throw new InvalidOperationException(
                    "The variable-value collection cannot contain " +
                    "a null entry.");
            }

            variableValue.EnsureValid();

            if (!variableIds.Add(
                    variableValue.VariableId))
            {
                throw new InvalidOperationException(
                    $"Mathematical variable identifier " +
                    $"'{variableValue.VariableId}' appears more " +
                    "than once in the solve result.");
            }
        }
    }

    private static void EnsureFiniteNullable(
        double? value,
        string propertyName)
    {
        if (value.HasValue &&
            (double.IsNaN(
                 value.Value) ||
             double.IsInfinity(
                 value.Value)))
        {
            throw new InvalidOperationException(
                $"{propertyName} must be finite when specified.");
        }
    }
}
