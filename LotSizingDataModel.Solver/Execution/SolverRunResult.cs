using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Monitoring;
using LotSizingDataModel.Solver.Evaluation;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Represents the normalized result of a solver execution.
/// </summary>
/// <remarks>
/// This class is independent of CPLEX, Gurobi, Xpress, and
/// COIN-OR CBC. Each solver adapter converts its native status,
/// statistics, and solution values into this common result.
/// </remarks>
[Serializable]
[XmlType(TypeName = "solverRunResult")]
public sealed class SolverRunResult
{
    private readonly List<string> _diagnostics =
        new();

    private readonly List<SolverProgressSnapshot>
        _progressHistory =
            new();

    /// <summary>
    /// Initializes an empty solver run result.
    /// </summary>
    public SolverRunResult()
    {
        RunId =
            Guid.NewGuid().ToString("D");

        RunName =
            string.Empty;

        SolverName =
            string.Empty;

        SolverVersion =
            string.Empty;

        FormulationName =
            string.Empty;

        StartedAtUtc =
            DateTime.UtcNow;

        TerminationReason =
            SolverTerminationReason.Unknown;
    }

    /// <summary>
    /// Gets or sets the unique solver-run identifier.
    /// </summary>
    [XmlAttribute("runId")]
    public string RunId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the user-defined solver-run name.
    /// </summary>
    [XmlElement("runName")]
    public string RunName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver kind used for the run.
    /// </summary>
    [XmlAttribute("solverKind")]
    public SolverKind SolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver display name.
    /// </summary>
    [XmlElement("solverName")]
    public string SolverName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver version.
    /// </summary>
    [XmlElement("solverVersion")]
    public string SolverVersion
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mathematical formulation name.
    /// </summary>
    [XmlElement("formulationName")]
    public string FormulationName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the run
    /// started.
    /// </summary>
    [XmlElement("startedAtUtc")]
    public DateTime StartedAtUtc
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the run
    /// completed.
    /// </summary>
    [XmlElement("completedAtUtc")]
    public DateTime? CompletedAtUtc
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the elapsed wall-clock time in seconds.
    /// </summary>
    [XmlElement("elapsedSeconds")]
    public double ElapsedSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the elapsed processor time in seconds,
    /// when available.
    /// </summary>
    [XmlElement("processorSeconds")]
    public double? ProcessorSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the normalized termination reason.
    /// </summary>
    [XmlAttribute("terminationReason")]
    public SolverTerminationReason TerminationReason
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the final incumbent objective value.
    /// </summary>
    [XmlElement("objectiveValue")]
    public double? ObjectiveValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the objective value independently recomputed
    /// from the normalized decision-variable values.
    /// </summary>
    [XmlElement("recomputedObjectiveValue")]
    public double? RecomputedObjectiveValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the absolute difference between the
    /// solver-reported and recomputed objective values.
    /// </summary>
    [XmlElement("objectiveDifference")]
    public double? ObjectiveDifference
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the result of objective-value verification.
    /// </summary>
    [XmlAttribute("objectiveVerificationStatus")]
    public ObjectiveVerificationStatus ObjectiveVerificationStatus
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the final best objective bound.
    /// </summary>
    [XmlElement("bestBound")]
    public double? BestBound
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the final absolute optimality gap.
    /// </summary>
    [XmlElement("absoluteGap")]
    public double? AbsoluteGap
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the final relative optimality gap.
    /// </summary>
    [XmlElement("relativeGap")]
    public double? RelativeGap
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of explored search-tree nodes.
    /// </summary>
    [XmlElement("exploredNodeCount")]
    public long? ExploredNodeCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of solver iterations.
    /// </summary>
    [XmlElement("iterationCount")]
    public long? IterationCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of feasible solutions found.
    /// </summary>
    [XmlElement("solutionCount")]
    public int SolutionCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the normalized lot-sizing solution created
    /// from the native solver result.
    /// </summary>
    [XmlElement("solution")]
    public LotSizingSolution? Solution
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the retained convergence history.
    /// </summary>
    [XmlArray("progressHistory")]
    [XmlArrayItem("point")]
    public List<SolverProgressSnapshot> ProgressHistory =>
        _progressHistory;

    /// <summary>
    /// Gets the diagnostics produced during model generation,
    /// solver execution, and solution extraction.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets a value indicating whether a normalized solution is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasSolution =>
        Solution is not null;

    /// <summary>
    /// Gets a value indicating whether the solver run completed
    /// without an execution failure.
    /// </summary>
    [XmlIgnore]
    public bool IsSuccessful =>
        TerminationReason is
            SolverTerminationReason.Optimal or
            SolverTerminationReason.Feasible or
            SolverTerminationReason.TimeLimit or
            SolverTerminationReason.NodeLimit or
            SolverTerminationReason.IterationLimit or
            SolverTerminationReason.SolutionLimit or
            SolverTerminationReason.RelativeGapLimit or
            SolverTerminationReason.AbsoluteGapLimit;

    /// <summary>
    /// Adds a diagnostic message.
    /// </summary>
    /// <param name="message">
    /// Diagnostic message.
    /// </param>
    public void AddDiagnostic(
        string message)
    {
        if (string.IsNullOrWhiteSpace(
                message))
        {
            throw new ArgumentException(
                "A diagnostic message cannot be empty.",
                nameof(message));
        }

        _diagnostics.Add(
            message.Trim());
    }

    /// <summary>
    /// Copies retained progress snapshots into the result.
    /// </summary>
    /// <param name="snapshots">
    /// Progress snapshots to copy.
    /// </param>
    public void SetProgressHistory(
        IEnumerable<SolverProgressSnapshot> snapshots)
    {
        ArgumentNullException.ThrowIfNull(
            snapshots);

        _progressHistory.Clear();

        foreach (
            SolverProgressSnapshot snapshot
            in snapshots)
        {
            ArgumentNullException.ThrowIfNull(
                snapshot);

            _progressHistory.Add(
                snapshot);
        }
    }
}
