using System;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Normalizes raw mathematical-variable values before they are
/// exposed through domain decision objects or used for objective
/// post-processing.
/// </summary>
/// <remarks>
/// <para>
/// Solver values are floating-point values and can contain small
/// numerical residuals such as -6E-13 or 180.00000000000006.
/// This component converts such residuals to stable business
/// values without rounding materially fractional quantities.
/// </para>
/// <para>
/// The same normalization service must be used by solution mapping
/// and objective-value recomputation so that the serialized
/// solution and the recomputed objective refer to exactly the same
/// numerical representation.
/// </para>
/// </remarks>
public sealed class MathematicalVariableValueNormalizer
{
    /// <summary>
    /// Gets the default tolerance used to identify numerical zero.
    /// </summary>
    public const double DefaultZeroTolerance = 1.0e-8;

    /// <summary>
    /// Gets the default tolerance used for integer-domain values.
    /// </summary>
    public const double DefaultIntegralityTolerance = 1.0e-7;

    /// <summary>
    /// Gets the default tolerance used to clean continuous values
    /// that are numerically indistinguishable from an integer.
    /// </summary>
    public const double DefaultNearIntegerTolerance = 1.0e-8;

    /// <summary>
    /// Initializes a value normalizer with default tolerances.
    /// </summary>
    public MathematicalVariableValueNormalizer()
        : this(
            DefaultZeroTolerance,
            DefaultIntegralityTolerance,
            DefaultNearIntegerTolerance)
    {
    }

    /// <summary>
    /// Initializes a value normalizer.
    /// </summary>
    /// <param name="zeroTolerance">
    /// Absolute tolerance used to identify numerical zero.
    /// </param>
    /// <param name="integralityTolerance">
    /// Absolute tolerance used for integer and semi-integer values.
    /// </param>
    /// <param name="nearIntegerTolerance">
    /// Absolute tolerance used to clean continuous values that are
    /// numerically indistinguishable from an integer.
    /// </param>
    public MathematicalVariableValueNormalizer(
        double zeroTolerance,
        double integralityTolerance,
        double nearIntegerTolerance)
    {
        ValidateTolerance(
            zeroTolerance,
            nameof(zeroTolerance));

        ValidateTolerance(
            integralityTolerance,
            nameof(integralityTolerance));

        ValidateTolerance(
            nearIntegerTolerance,
            nameof(nearIntegerTolerance));

        ZeroTolerance =
            zeroTolerance;

        IntegralityTolerance =
            integralityTolerance;

        NearIntegerTolerance =
            nearIntegerTolerance;
    }

    /// <summary>
    /// Gets the zero tolerance.
    /// </summary>
    public double ZeroTolerance { get; }

    /// <summary>
    /// Gets the integrality tolerance.
    /// </summary>
    public double IntegralityTolerance { get; }

    /// <summary>
    /// Gets the near-integer cleanup tolerance for continuous
    /// variables.
    /// </summary>
    public double NearIntegerTolerance { get; }

    /// <summary>
    /// Normalizes one raw variable value.
    /// </summary>
    /// <param name="variable">
    /// Mathematical variable defining the value domain.
    /// </param>
    /// <param name="rawValue">
    /// Raw floating-point value returned by the solver.
    /// </param>
    /// <returns>
    /// Normalized value suitable for domain mapping and objective
    /// post-processing.
    /// </returns>
    public double Normalize(
        MathematicalVariable variable,
        double rawValue)
    {
        ArgumentNullException.ThrowIfNull(
            variable);

        if (!double.IsFinite(rawValue))
        {
            throw new ArgumentOutOfRangeException(
                nameof(rawValue),
                rawValue,
                "A mathematical-variable value must be finite.");
        }

        if (Math.Abs(rawValue) <= ZeroTolerance)
        {
            return 0.0;
        }

        return variable.VariableType switch
        {
            MathematicalVariableType.Binary =>
                NormalizeBinary(
                    rawValue),

            MathematicalVariableType.Integer or
            MathematicalVariableType.SemiInteger =>
                NormalizeIntegerDomainValue(
                    rawValue),

            MathematicalVariableType.Continuous or
            MathematicalVariableType.SemiContinuous =>
                NormalizeContinuousValue(
                    rawValue),

            _ =>
                NormalizeContinuousValue(
                    rawValue)
        };
    }

    /// <summary>
    /// Returns a normalized copy of one solver value.
    /// </summary>
    public MathematicalVariableValue Normalize(
        MathematicalVariable variable,
        MathematicalVariableValue rawValue)
    {
        ArgumentNullException.ThrowIfNull(
            rawValue);

        return new MathematicalVariableValue(
            rawValue.VariableId,
            Normalize(
                variable,
                rawValue.Value),
            rawValue.VariableName,
            rawValue.DomainKey);
    }

    private double NormalizeBinary(
        double rawValue)
    {
        if (Math.Abs(rawValue) <= ZeroTolerance)
        {
            return 0.0;
        }

        if (Math.Abs(rawValue - 1.0) <= IntegralityTolerance)
        {
            return 1.0;
        }

        /*
         * A binary decision exposed by the domain layer must be
         * either 0 or 1. A materially fractional value indicates
         * an inconsistent solver result and must not be silently
         * rounded.
         */
        throw new InvalidOperationException(
            $"Binary solver value '{rawValue:G17}' is not within " +
            $"the configured integrality tolerance " +
            $"'{IntegralityTolerance:G17}' of 0 or 1.");
    }

    private double NormalizeIntegerDomainValue(
        double rawValue)
    {
        double nearestInteger =
            Math.Round(
                rawValue,
                MidpointRounding.AwayFromZero);

        if (Math.Abs(
                rawValue - nearestInteger) <=
            IntegralityTolerance)
        {
            return nearestInteger;
        }

        /*
         * Preserve the raw value instead of inventing an integer.
         * The mapper or validation layer can then reject it if the
         * corresponding domain decision requires integrality.
         */
        return rawValue;
    }

    private double NormalizeContinuousValue(
        double rawValue)
    {
        double nearestInteger =
            Math.Round(
                rawValue,
                MidpointRounding.AwayFromZero);

        if (Math.Abs(
                rawValue - nearestInteger) <=
            NearIntegerTolerance)
        {
            return nearestInteger;
        }

        return rawValue;
    }

    private static void ValidateTolerance(
        double tolerance,
        string parameterName)
    {
        if (!double.IsFinite(tolerance) ||
            tolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                tolerance,
                "A numerical tolerance must be finite and " +
                "non-negative.");
        }
    }
}
