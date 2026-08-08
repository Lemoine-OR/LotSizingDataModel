using System;
using System.Collections.Generic;

namespace LotSizingDataModel.Solver.Monitoring;

/// <summary>
/// Selects and retains solver progress snapshots according to
/// configured recording rules.
/// </summary>
public sealed class SolverProgressRecorder
{
    private readonly SolverProgressRecordingOptions _options;

    private readonly List<SolverProgressSnapshot> _history =
        new();

    private SolverProgressSnapshot? _lastRecordedSnapshot;

    /// <summary>
    /// Initializes a progress recorder.
    /// </summary>
    /// <param name="options">
    /// Progress-recording options.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public SolverProgressRecorder(
        SolverProgressRecordingOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        options.EnsureValid();

        _options =
            options;
    }

    /// <summary>
    /// Gets the retained progress history.
    /// </summary>
    public IReadOnlyList<SolverProgressSnapshot> History =>
        _history;

    /// <summary>
    /// Gets the number of retained progress snapshots.
    /// </summary>
    public int Count =>
        _history.Count;

    /// <summary>
    /// Attempts to retain a progress snapshot.
    /// </summary>
    /// <param name="snapshot">
    /// Solver progress snapshot.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the snapshot was retained;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="snapshot"/> is
    /// <see langword="null"/>.
    /// </exception>
    public bool TryRecord(
        SolverProgressSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(
            snapshot);

        if (_options.Mode ==
            SolverProgressRecordingMode.None)
        {
            return false;
        }

        if (HasReachedMaximumPointCount())
        {
            return false;
        }

        if (!ShouldRecord(
                snapshot))
        {
            return false;
        }

        SolverProgressSnapshot retainedSnapshot =
            CloneSnapshot(
                snapshot);

        _history.Add(
            retainedSnapshot);

        _lastRecordedSnapshot =
            retainedSnapshot;

        return true;
    }

    /// <summary>
    /// Removes all retained progress snapshots.
    /// </summary>
    public void Clear()
    {
        _history.Clear();

        _lastRecordedSnapshot =
            null;
    }

    private bool ShouldRecord(
        SolverProgressSnapshot snapshot)
    {
        if (_lastRecordedSnapshot is null)
        {
            return true;
        }

        return _options.Mode switch
        {
            SolverProgressRecordingMode.IncumbentOnly =>
                IsNewIncumbent(
                    snapshot,
                    _lastRecordedSnapshot),

            SolverProgressRecordingMode.TimeInterval =>
                HasReachedTimeInterval(
                    snapshot,
                    _lastRecordedSnapshot),

            SolverProgressRecordingMode.NodeInterval =>
                HasReachedNodeInterval(
                    snapshot,
                    _lastRecordedSnapshot),

            SolverProgressRecordingMode.SignificantChanges =>
                HasSignificantChange(
                    snapshot,
                    _lastRecordedSnapshot),

            SolverProgressRecordingMode.Full =>
                true,

            _ =>
                false
        };
    }

    private bool HasReachedMaximumPointCount()
    {
        return
            _options.MaximumRecordedPointCount > 0 &&
            _history.Count >=
                _options.MaximumRecordedPointCount;
    }

    private static bool IsNewIncumbent(
        SolverProgressSnapshot current,
        SolverProgressSnapshot previous)
    {
        if (!current.IncumbentObjective.HasValue)
        {
            return false;
        }

        if (!previous.IncumbentObjective.HasValue)
        {
            return true;
        }

        return
            !current.IncumbentObjective.Value.Equals(
                previous.IncumbentObjective.Value);
    }

    private bool HasReachedTimeInterval(
        SolverProgressSnapshot current,
        SolverProgressSnapshot previous)
    {
        return
            current.ElapsedSeconds -
                previous.ElapsedSeconds >=
            _options.TimeIntervalSeconds;
    }

    private bool HasReachedNodeInterval(
        SolverProgressSnapshot current,
        SolverProgressSnapshot previous)
    {
        if (!current.ExploredNodeCount.HasValue ||
            !previous.ExploredNodeCount.HasValue)
        {
            return false;
        }

        return
            current.ExploredNodeCount.Value -
                previous.ExploredNodeCount.Value >=
            _options.NodeInterval;
    }

    private bool HasSignificantChange(
        SolverProgressSnapshot current,
        SolverProgressSnapshot previous)
    {
        return
            HasChangedByAtLeast(
                current.IncumbentObjective,
                previous.IncumbentObjective,
                _options.MinimumObjectiveChange) ||
            HasChangedByAtLeast(
                current.BestBound,
                previous.BestBound,
                _options.MinimumBoundChange) ||
            HasChangedByAtLeast(
                current.RelativeGap,
                previous.RelativeGap,
                _options.MinimumRelativeGapChange) ||
            current.Stage !=
                previous.Stage ||
            current.SolutionCount !=
                previous.SolutionCount;
    }

    private static bool HasChangedByAtLeast(
        double? current,
        double? previous,
        double minimumChange)
    {
        if (current.HasValue !=
            previous.HasValue)
        {
            return true;
        }

        if (!current.HasValue ||
            !previous.HasValue)
        {
            return false;
        }

        return
            Math.Abs(
                current.Value -
                previous.Value) >=
            minimumChange;
    }

    private static SolverProgressSnapshot CloneSnapshot(
        SolverProgressSnapshot source)
    {
        return new SolverProgressSnapshot
        {
            Stage =
                source.Stage,

            ElapsedSeconds =
                source.ElapsedSeconds,

            ProcessorSeconds =
                source.ProcessorSeconds,

            ExploredNodeCount =
                source.ExploredNodeCount,

            OpenNodeCount =
                source.OpenNodeCount,

            IterationCount =
                source.IterationCount,

            IncumbentObjective =
                source.IncumbentObjective,

            BestBound =
                source.BestBound,

            AbsoluteGap =
                source.AbsoluteGap,

            RelativeGap =
                source.RelativeGap,

            SolutionCount =
                source.SolutionCount,

            MemoryUsageMegabytes =
                source.MemoryUsageMegabytes,

            Message =
                source.Message
        };
    }
}
