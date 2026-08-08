using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Monitoring;

/// <summary>
/// Configures how solver progress snapshots are retained during
/// an optimization run.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverProgressRecordingOptions")]
public sealed class SolverProgressRecordingOptions
{
    /// <summary>
    /// Initializes progress-recording options with recommended
    /// defaults.
    /// </summary>
    public SolverProgressRecordingOptions()
    {
        Mode =
            SolverProgressRecordingMode.SignificantChanges;

        TimeIntervalSeconds =
            1.0;

        NodeInterval =
            100;

        MinimumObjectiveChange =
            0.0;

        MinimumBoundChange =
            0.0;

        MinimumRelativeGapChange =
            0.0001;

        MaximumRecordedPointCount =
            10000;
    }

    /// <summary>
    /// Gets or sets the progress-recording mode.
    /// </summary>
    [XmlAttribute("mode")]
    public SolverProgressRecordingMode Mode
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the minimum elapsed-time interval, in
    /// seconds, between two retained snapshots when
    /// <see cref="SolverProgressRecordingMode.TimeInterval"/>
    /// is selected.
    /// </summary>
    [XmlAttribute("timeIntervalSeconds")]
    public double TimeIntervalSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the minimum number of newly explored nodes
    /// between two retained snapshots when
    /// <see cref="SolverProgressRecordingMode.NodeInterval"/>
    /// is selected.
    /// </summary>
    [XmlAttribute("nodeInterval")]
    public long NodeInterval
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the minimum absolute incumbent-objective
    /// change considered significant.
    /// </summary>
    [XmlAttribute("minimumObjectiveChange")]
    public double MinimumObjectiveChange
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the minimum absolute best-bound change
    /// considered significant.
    /// </summary>
    [XmlAttribute("minimumBoundChange")]
    public double MinimumBoundChange
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the minimum absolute relative-gap change
    /// considered significant.
    /// </summary>
    [XmlAttribute("minimumRelativeGapChange")]
    public double MinimumRelativeGapChange
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the maximum number of progress points that
    /// may be retained.
    /// </summary>
    /// <remarks>
    /// A value of zero disables the limit.
    /// </remarks>
    [XmlAttribute("maximumRecordedPointCount")]
    public int MaximumRecordedPointCount
    {
        get;
        set;
    }

    /// <summary>
    /// Validates the progress-recording options.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when one or more option values are invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (TimeIntervalSeconds < 0.0 ||
            double.IsNaN(
                TimeIntervalSeconds) ||
            double.IsInfinity(
                TimeIntervalSeconds))
        {
            throw new InvalidOperationException(
                "The progress-recording time interval must be " +
                "a finite non-negative number.");
        }

        if (NodeInterval < 0)
        {
            throw new InvalidOperationException(
                "The progress-recording node interval cannot " +
                "be negative.");
        }

        ValidateFiniteNonNegativeValue(
            MinimumObjectiveChange,
            nameof(MinimumObjectiveChange));

        ValidateFiniteNonNegativeValue(
            MinimumBoundChange,
            nameof(MinimumBoundChange));

        ValidateFiniteNonNegativeValue(
            MinimumRelativeGapChange,
            nameof(MinimumRelativeGapChange));

        if (MaximumRecordedPointCount < 0)
        {
            throw new InvalidOperationException(
                "The maximum recorded-point count cannot be " +
                "negative.");
        }

        if (Mode ==
                SolverProgressRecordingMode.TimeInterval &&
            TimeIntervalSeconds <= 0.0)
        {
            throw new InvalidOperationException(
                "A strictly positive time interval is required " +
                "when time-interval progress recording is " +
                "selected.");
        }

        if (Mode ==
                SolverProgressRecordingMode.NodeInterval &&
            NodeInterval <= 0)
        {
            throw new InvalidOperationException(
                "A strictly positive node interval is required " +
                "when node-interval progress recording is " +
                "selected.");
        }
    }

    private static void ValidateFiniteNonNegativeValue(
        double value,
        string propertyName)
    {
        if (value < 0.0 ||
            double.IsNaN(
                value) ||
            double.IsInfinity(
                value))
        {
            throw new InvalidOperationException(
                $"{propertyName} must be a finite " +
                "non-negative number.");
        }
    }
}
