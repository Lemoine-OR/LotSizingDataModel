using System;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Describes one production/setup Big-M estimate.
/// </summary>
public sealed class ProductionSetupBigMEstimate
{
    /// <summary>
    /// Initializes a production/setup Big-M estimate.
    /// </summary>
    /// <param name="value">Finite non-negative upper bound.</param>
    /// <param name="source">Human-readable derivation source.</param>
    /// <param name="isFallback">
    /// Indicates whether the configured fallback was used.
    /// </param>
    public ProductionSetupBigMEstimate(
        double value,
        string source,
        bool isFallback)
    {
        if (!double.IsFinite(value) || value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "A Big-M estimate must be finite and non-negative.");
        }

        Value = value;
        Source = source ?? string.Empty;
        IsFallback = isFallback;
    }

    /// <summary>Gets the estimated upper bound.</summary>
    public double Value { get; }

    /// <summary>Gets the derivation description.</summary>
    public string Source { get; }

    /// <summary>
    /// Gets a value indicating whether the configured fallback
    /// was used.
    /// </summary>
    public bool IsFallback { get; }
}
