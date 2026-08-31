using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Relaxation;

/// <summary>
/// Explicit immutable set of constraints and multipliers used to
/// construct one Lagrangian-relaxed mathematical model.
/// </summary>
public sealed class LagrangianRelaxationSpecification
{
    private readonly LagrangianMultiplier[] _multipliers;

    public LagrangianRelaxationSpecification(
        IEnumerable<LagrangianMultiplier> multipliers)
    {
        ArgumentNullException.ThrowIfNull(
            multipliers);

        _multipliers =
            multipliers.ToArray();

        if (_multipliers.Length == 0)
        {
            throw new InvalidOperationException(
                "At least one constraint must be selected for Lagrangian relaxation.");
        }

        EnsureValid();
    }

    public IReadOnlyList<LagrangianMultiplier> Multipliers =>
        _multipliers;

    public void EnsureValid()
    {
        var identifiers =
            new HashSet<int>();

        foreach (LagrangianMultiplier multiplier
                 in _multipliers)
        {
            ArgumentNullException.ThrowIfNull(
                multiplier);

            multiplier.EnsureValid();

            if (!identifiers.Add(
                    multiplier.ConstraintId))
            {
                throw new InvalidOperationException(
                    $"Constraint identifier '{multiplier.ConstraintId}' appears more than once in the Lagrangian specification.");
            }
        }
    }

    public void EnsureValidAgainst(
        MathematicalModel model)
    {
        ArgumentNullException.ThrowIfNull(
            model);

        EnsureValid();

        if (model.Objective.Sense !=
            ObjectiveSense.Minimize)
        {
            throw new NotSupportedException(
                "The alpha.36 Lagrangian foundation currently supports minimization models only.");
        }

        foreach (LagrangianMultiplier multiplier
                 in _multipliers)
        {
            LinearConstraint? constraint =
                model.Constraints.FirstOrDefault(
                    candidate =>
                        candidate.Id ==
                        multiplier.ConstraintId);

            if (constraint is null)
            {
                throw new InvalidOperationException(
                    $"Constraint identifier '{multiplier.ConstraintId}' does not exist in the source model.");
            }

            if (!constraint.IsEnabled)
            {
                throw new InvalidOperationException(
                    $"Constraint '{constraint.Name}' is already disabled and cannot be selected for relaxation.");
            }

            LagrangianMultiplierDomain.EnsureValid(
                constraint.Sense,
                multiplier.Value);
        }
    }
}
