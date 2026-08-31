using LotSizingDataModel.Solver.Algorithms.Relaxation;
using LotSizingDataModel.Solver.Modeling;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class LagrangianRelaxationFoundationTests
{
    [Fact]
    public void LessThanRelaxation_DisablesConstraintAndAddsResidualToObjective()
    {
        MathematicalModel source =
            CreateToyModel(
                MathematicalConstraintSense.LessThanOrEqual);

        var specification =
            new LagrangianRelaxationSpecification(
                new[]
                {
                    new LagrangianMultiplier(
                        10,
                        2.0)
                });

        var builder =
            new LagrangianRelaxationModelBuilder();

        LagrangianRelaxationBuildResult result =
            builder.Build(
                source,
                specification);

        Assert.True(
            source.Constraints.Single().IsEnabled);

        Assert.False(
            result.RelaxedModel.Constraints.Single().IsEnabled);

        LinearExpression objective =
            result.RelaxedModel.Objective.Expression;

        Assert.Equal(
            -10.0,
            objective.Constant,
            10);

        LinearTerm term =
            Assert.Single(
                objective.Terms);

        Assert.Equal(
            1,
            term.VariableId);

        Assert.Equal(
            5.0,
            term.Coefficient,
            10);
    }

    [Fact]
    public void MultiplierDomain_UsesCorrectMinimizationSigns()
    {
        LagrangianMultiplierDomain.EnsureValid(
            MathematicalConstraintSense.LessThanOrEqual,
            1.0);

        LagrangianMultiplierDomain.EnsureValid(
            MathematicalConstraintSense.GreaterThanOrEqual,
            -1.0);

        LagrangianMultiplierDomain.EnsureValid(
            MathematicalConstraintSense.Equal,
            -7.0);

        Assert.Throws<InvalidOperationException>(
            () =>
                LagrangianMultiplierDomain.EnsureValid(
                    MathematicalConstraintSense.LessThanOrEqual,
                    -1.0));

        Assert.Throws<InvalidOperationException>(
            () =>
                LagrangianMultiplierDomain.EnsureValid(
                    MathematicalConstraintSense.GreaterThanOrEqual,
                    1.0));
    }

    [Fact]
    public void SubgradientUpdate_ProjectsOntoMultiplierDomain()
    {
        Assert.Equal(
            0.0,
            LagrangianSubgradientUpdater.Update(
                1.0,
                -10.0,
                1.0,
                MathematicalConstraintSense.LessThanOrEqual),
            10);

        Assert.Equal(
            0.0,
            LagrangianSubgradientUpdater.Update(
                -1.0,
                10.0,
                1.0,
                MathematicalConstraintSense.GreaterThanOrEqual),
            10);

        Assert.Equal(
            3.0,
            LagrangianSubgradientUpdater.Update(
                1.0,
                2.0,
                1.0,
                MathematicalConstraintSense.Equal),
            10);
    }

    [Fact]
    public void ResidualEvaluator_UsesLeftHandSideMinusRightHandSide()
    {
        LinearConstraint constraint =
            CreateToyModel(
                    MathematicalConstraintSense.LessThanOrEqual)
                .Constraints
                .Single();

        double residual =
            LagrangianConstraintResidualEvaluator.Evaluate(
                constraint,
                new Dictionary<int, double>
                {
                    [1] = 7.0
                });

        Assert.Equal(
            2.0,
            residual,
            10);
    }

    [Fact]
    public void BoundTracker_KeepsBestMinimizationBounds()
    {
        var tracker =
            new LagrangianBoundTracker();

        tracker.RegisterDualLowerBound(
            4.0);

        tracker.RegisterDualLowerBound(
            5.0);

        tracker.RegisterPrimalUpperBound(
            10.0);

        tracker.RegisterPrimalUpperBound(
            8.0);

        Assert.Equal(
            5.0,
            tracker.BestDualLowerBound);

        Assert.Equal(
            8.0,
            tracker.BestPrimalUpperBound);

        Assert.Equal(
            3.0,
            tracker.AbsoluteGap);

        Assert.True(
            tracker.RelativeGap.HasValue);

        Assert.Equal(
            0.375,
            tracker.RelativeGap.Value,
            10);

        Assert.Throws<InvalidOperationException>(
            () =>
                tracker.RegisterDualLowerBound(
                    9.0));
    }

    [Fact]
    public void MaximizationModel_IsExplicitlyUnsupported()
    {
        MathematicalModel source =
            CreateToyModel(
                MathematicalConstraintSense.Equal);

        source.Objective.Sense =
            ObjectiveSense.Maximize;

        var specification =
            new LagrangianRelaxationSpecification(
                new[]
                {
                    new LagrangianMultiplier(
                        10,
                        0.0)
                });

        Assert.Throws<NotSupportedException>(
            () =>
                specification.EnsureValidAgainst(
                    source));
    }

    private static MathematicalModel CreateToyModel(
        MathematicalConstraintSense sense)
    {
        var model =
            new MathematicalModel
            {
                Name =
                    "lagrangian-toy"
            };

        model.AddVariable(
            new MathematicalVariable(
                1,
                "x",
                MathematicalVariableType.Continuous,
                0.0,
                100.0));

        model.Objective.Expression.AddTerm(
            1,
            3.0);

        var leftHandSide =
            new LinearExpression();

        leftHandSide.AddTerm(
            1,
            1.0);

        model.AddConstraint(
            new LinearConstraint(
                10,
                "relax-me",
                leftHandSide,
                sense,
                5.0));

        model.EnsureValid();

        return model;
    }
}
