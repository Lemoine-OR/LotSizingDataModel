using LotSizingDataModel.Checker.Common;

namespace LotSizingDataModel.Checker.Configuration;

/// <summary>
/// Configures generic lot-sizing solution checking.
/// </summary>
public sealed class SolutionCheckOptions
{
    /// <summary>
    /// Gets or sets the requested checking depth.
    /// </summary>
    public SolutionCheckLevel Level
    {
        get;
        set;
    } = SolutionCheckLevel.Full;

    /// <summary>
    /// Gets or sets the absolute tolerance used for constraint feasibility.
    /// </summary>
    public double FeasibilityTolerance
    {
        get;
        set;
    } = 1.0e-8;

    /// <summary>
    /// Gets or sets the absolute tolerance used to identify numerical zero.
    /// </summary>
    public double ZeroTolerance
    {
        get;
        set;
    } = 1.0e-9;

    /// <summary>
    /// Gets or sets the tolerance used for integer and binary values.
    /// </summary>
    public double IntegralityTolerance
    {
        get;
        set;
    } = 1.0e-7;

    /// <summary>
    /// Gets or sets the absolute tolerance used when comparing objective values.
    /// </summary>
    public double ObjectiveAbsoluteTolerance
    {
        get;
        set;
    } = 1.0e-8;

    /// <summary>
    /// Gets or sets the relative tolerance used when comparing objective values.
    /// </summary>
    public double ObjectiveRelativeTolerance
    {
        get;
        set;
    } = 1.0e-9;

    /// <summary>
    /// Gets or sets an optional externally reported objective value that
    /// takes precedence over the value stored in the candidate solution
    /// evaluation when objective consistency is checked.
    /// </summary>
    /// <remarks>
    /// This is primarily intended for independently checking a
    /// <c>KnownResult.ReportedObjectiveValue</c> produced by a solver.
    /// </remarks>
    public double? ReportedObjectiveValueOverride
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether disabled mathematical constraints must be ignored.
    /// </summary>
    public bool IgnoreDisabledConstraints
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Gets or sets whether checking should continue after a structural error.
    /// </summary>
    public bool ContinueAfterStructuralErrors
    {
        get;
        set;
    } = false;

    /// <summary>
    /// Validates the checker options.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when a tolerance is negative or non-finite.
    /// </exception>
    public void EnsureValid()
    {
        if (!Enum.IsDefined(
                typeof(SolutionCheckLevel),
                Level))
        {
            throw new ArgumentOutOfRangeException(
                nameof(Level),
                Level,
                "The requested solution-check level is not defined.");
        }

        ValidateTolerance(
            FeasibilityTolerance,
            nameof(FeasibilityTolerance));

        ValidateTolerance(
            ZeroTolerance,
            nameof(ZeroTolerance));

        ValidateTolerance(
            IntegralityTolerance,
            nameof(IntegralityTolerance));

        ValidateTolerance(
            ObjectiveAbsoluteTolerance,
            nameof(ObjectiveAbsoluteTolerance));

        ValidateTolerance(
            ObjectiveRelativeTolerance,
            nameof(ObjectiveRelativeTolerance));

        if (ReportedObjectiveValueOverride.HasValue &&
            !double.IsFinite(
                ReportedObjectiveValueOverride.Value))
        {
            throw new ArgumentOutOfRangeException(
                nameof(ReportedObjectiveValueOverride),
                ReportedObjectiveValueOverride,
                "The reported objective override must be finite when specified.");
        }
    }

    private static void ValidateTolerance(
        double value,
        string parameterName)
    {
        if (!double.IsFinite(value) ||
            value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A numerical tolerance must be finite and non-negative.");
        }
    }
}
