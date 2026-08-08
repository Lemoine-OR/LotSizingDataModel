using System;
using System.Collections.Generic;

namespace LotSizingDataModel.Solver.Evaluation;

/// <summary>
/// Stores the result of objective-value post-processing.
/// </summary>
public sealed class MathematicalObjectiveRecalculationResult
{
    private readonly List<string> _diagnostics = new();

    /// <summary>Gets or sets the verification status.</summary>
    public ObjectiveVerificationStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the objective value reported by the solver.
    /// </summary>
    public double? SolverObjectiveValue { get; set; }

    /// <summary>
    /// Gets or sets the objective value recomputed from normalized
    /// mathematical-variable values.
    /// </summary>
    public double? RecomputedObjectiveValue { get; set; }

    /// <summary>
    /// Gets or sets the absolute difference between the two
    /// objective values.
    /// </summary>
    public double? AbsoluteDifference { get; set; }

    /// <summary>
    /// Gets or sets the effective comparison tolerance.
    /// </summary>
    public double? ComparisonTolerance { get; set; }

    /// <summary>
    /// Gets the diagnostics produced during post-processing.
    /// </summary>
    public List<string> Diagnostics => _diagnostics;

    /// <summary>
    /// Gets whether the recalculation completed successfully.
    /// </summary>
    public bool IsSuccessful =>
        Status is
            ObjectiveVerificationStatus.Consistent or
            ObjectiveVerificationStatus.Inconsistent;
}
