using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Relaxation;

/// <summary>
/// Builds a source-preserving Lagrangian relaxation for a
/// minimization model.
/// </summary>
/// <remarks>
/// For every selected constraint the residual is
/// <c>lhs - rhs</c>. The term
/// <c>lambda * (lhs - rhs)</c> is added to the objective and the
/// cloned constraint is disabled. Multiplier signs are validated
/// by <see cref="LagrangianMultiplierDomain"/>.
/// </remarks>
public sealed class LagrangianRelaxationModelBuilder
{
    public LagrangianRelaxationBuildResult Build(
        MathematicalModel sourceModel,
        LagrangianRelaxationSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(
            sourceModel);

        ArgumentNullException.ThrowIfNull(
            specification);

        sourceModel.EnsureValid();

        specification.EnsureValidAgainst(
            sourceModel);

        MathematicalModel relaxedModel =
            sourceModel.Clone();

        var relaxedIdentifiers =
            new List<int>();

        foreach (LagrangianMultiplier multiplier
                 in specification.Multipliers)
        {
            LinearConstraint sourceConstraint =
                sourceModel.Constraints.Single(
                    constraint =>
                        constraint.Id ==
                        multiplier.ConstraintId);

            LinearConstraint relaxedConstraint =
                relaxedModel.Constraints.Single(
                    constraint =>
                        constraint.Id ==
                        multiplier.ConstraintId);

            var residual =
                sourceConstraint.LeftHandSide.Clone();

            residual.AddConstant(
                -sourceConstraint.RightHandSide);

            residual.MultiplyBy(
                multiplier.Value);

            relaxedModel.Objective.Expression.Add(
                residual);

            relaxedConstraint.IsEnabled =
                false;

            relaxedIdentifiers.Add(
                sourceConstraint.Id);
        }

        relaxedModel.Description =
            string.IsNullOrWhiteSpace(
                sourceModel.Description)
                ? "Lagrangian-relaxed clone."
                : sourceModel.Description.Trim() +
                  " Lagrangian-relaxed clone.";

        relaxedModel.EnsureValid();

        return new LagrangianRelaxationBuildResult(
            relaxedModel,
            relaxedIdentifiers);
    }
}
