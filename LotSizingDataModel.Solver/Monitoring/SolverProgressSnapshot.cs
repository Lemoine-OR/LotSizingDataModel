using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Monitoring;

/// <summary>
/// Represents an immutable snapshot of the current progress of
/// a mathematical optimization run.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverProgressSnapshot")]
public sealed class SolverProgressSnapshot
{
    /// <summary>
    /// Initializes an empty progress snapshot.
    /// </summary>
    public SolverProgressSnapshot()
    {
        Message =
            string.Empty;
    }

    /// <summary>
    /// Gets or sets the current solver execution stage.
    /// </summary>
    [XmlAttribute("stage")]
    public Common.SolverProgressStage Stage
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the elapsed wall-clock time in seconds.
    /// </summary>
    [XmlAttribute("elapsedSeconds")]
    public double ElapsedSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the elapsed processor time in seconds,
    /// when reported by the solver.
    /// </summary>
    [XmlAttribute("processorSeconds")]
    public double? ProcessorSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of explored search-tree nodes.
    /// </summary>
    [XmlAttribute("exploredNodeCount")]
    public long? ExploredNodeCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of open search-tree nodes.
    /// </summary>
    [XmlAttribute("openNodeCount")]
    public long? OpenNodeCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of simplex or barrier
    /// iterations reported by the solver.
    /// </summary>
    [XmlAttribute("iterationCount")]
    public long? IterationCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the best feasible objective value currently
    /// known.
    /// </summary>
    [XmlAttribute("incumbentObjective")]
    public double? IncumbentObjective
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the best objective bound currently known.
    /// </summary>
    [XmlAttribute("bestBound")]
    public double? BestBound
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
    /// Gets or sets the relative optimality gap.
    /// </summary>
    [XmlAttribute("relativeGap")]
    public double? RelativeGap
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of feasible solutions found so
    /// far.
    /// </summary>
    [XmlAttribute("solutionCount")]
    public int SolutionCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the amount of memory used by the solver in
    /// megabytes, when available.
    /// </summary>
    [XmlAttribute("memoryUsageMegabytes")]
    public double? MemoryUsageMegabytes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a solver or callback message associated
    /// with this snapshot.
    /// </summary>
    [XmlElement("message")]
    public string Message
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a value indicating whether a feasible incumbent
    /// objective is available.
    /// </summary>
    [XmlIgnore]
    public bool HasIncumbent =>
        IncumbentObjective.HasValue;
}
