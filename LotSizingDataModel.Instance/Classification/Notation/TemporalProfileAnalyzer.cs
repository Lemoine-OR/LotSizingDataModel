using System;
using System.Collections.Generic;
using System.Linq;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Classifies numerical time series using the generalized
/// Bitran-Yanasse temporal profile vocabulary used by LSI.
/// </summary>
public static class TemporalProfileAnalyzer
{
    public const double DefaultTolerance = 1e-9;

    public static TemporalProfile Analyze(
        IEnumerable<double> values,
        double tolerance = DefaultTolerance)
    {
        ArgumentNullException.ThrowIfNull(values);
        ValidateTolerance(tolerance);

        double[] data = values.ToArray();

        if (data.Length == 0)
        {
            return Create(TemporalProfileKind.Unknown);
        }

        if (data.Any(value => !double.IsFinite(value)))
        {
            return Create(TemporalProfileKind.Unknown);
        }

        if (data.All(value => Math.Abs(value) <= tolerance))
        {
            return Create(TemporalProfileKind.Zero);
        }

        bool constant = true;
        bool nonIncreasing = true;
        bool nonDecreasing = true;

        for (int index = 1; index < data.Length; index++)
        {
            double delta = data[index] - data[index - 1];

            if (Math.Abs(delta) > tolerance)
            {
                constant = false;
            }

            if (delta > tolerance)
            {
                nonIncreasing = false;
            }

            if (delta < -tolerance)
            {
                nonDecreasing = false;
            }
        }

        if (constant)
        {
            return Create(TemporalProfileKind.Constant);
        }

        if (nonIncreasing)
        {
            return Create(TemporalProfileKind.NonIncreasing);
        }

        if (nonDecreasing)
        {
            return Create(TemporalProfileKind.NonDecreasing);
        }

        return Create(TemporalProfileKind.General);
    }

    public static TemporalProfile Analyze(
        IEnumerable<int> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        return Analyze(values.Select(value => (double)value));
    }

    public static TemporalProfile Combine(
        IEnumerable<TemporalProfile> profiles)
    {
        ArgumentNullException.ThrowIfNull(profiles);

        TemporalProfileKind[] kinds =
            profiles
                .Where(profile => profile is not null)
                .Select(profile => profile.Kind)
                .Where(kind =>
                    kind != TemporalProfileKind.Unknown &&
                    kind != TemporalProfileKind.NotApplicable)
                .Distinct()
                .OrderBy(kind => (int)kind)
                .ToArray();

        if (kinds.Length == 0)
        {
            return Create(TemporalProfileKind.Unknown);
        }

        if (kinds.Length == 1)
        {
            return Create(kinds[0]);
        }

        var result = Create(TemporalProfileKind.Mixed);
        result.ReplaceComponents(kinds);
        return result;
    }

    private static TemporalProfile Create(
        TemporalProfileKind kind)
    {
        return new TemporalProfile
        {
            Kind = kind
        };
    }

    private static void ValidateTolerance(double tolerance)
    {
        if (!double.IsFinite(tolerance) || tolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tolerance),
                tolerance,
                "The temporal-profile tolerance must be finite and non-negative.");
        }
    }
}
