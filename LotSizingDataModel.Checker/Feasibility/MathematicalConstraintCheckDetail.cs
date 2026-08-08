using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Feasibility;

/// <summary>
/// Describes the numerical evaluation of one mathematical constraint.
/// </summary>
public sealed class MathematicalConstraintCheckDetail
{
    /// <summary>
    /// Gets or sets the constraint identifier.
    /// </summary>
    public int ConstraintId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the constraint name.
    /// </summary>
    public string ConstraintName
    {
        get;
        set;
    } = string.Empty;

    /// <summary>
    /// Gets or sets the constraint domain key, when available.
    /// </summary>
    public string? DomainKey
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the evaluated left-hand-side value.
    /// </summary>
    public double LeftHandSideValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the constraint sense.
    /// </summary>
    public MathematicalConstraintSense Sense
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the right-hand-side value.
    /// </summary>
    public double RightHandSideValue
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the positive violation magnitude.
    /// </summary>
    public double Violation
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether the constraint is feasible within tolerance.
    /// </summary>
    public bool IsSatisfied
    {
        get;
        set;
    }
}
