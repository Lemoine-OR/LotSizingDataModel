using System;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Links production quantities to binary setup variables.
/// </summary>
public sealed class ProductionSetupLinkConstraintFamilyBuilder :
    StandardLotSizingConstraintFamilyBuilderBase
{
    private readonly IProductionSetupBigMEstimator
        _bigMEstimator;

    /// <summary>
    /// Initializes the constraint-family builder with the default
    /// generic Big-M estimator.
    /// </summary>
    public ProductionSetupLinkConstraintFamilyBuilder()
        : this(new GenericProductionSetupBigMEstimator())
    {
    }

    /// <summary>
    /// Initializes the constraint-family builder with an explicit
    /// Big-M estimator.
    /// </summary>
    /// <param name="bigMEstimator">
    /// Production/setup Big-M estimator.
    /// </param>
    public ProductionSetupLinkConstraintFamilyBuilder(
        IProductionSetupBigMEstimator bigMEstimator)
    {
        ArgumentNullException.ThrowIfNull(bigMEstimator);
        _bigMEstimator = bigMEstimator;
    }

    /// <summary>Gets the family identifier.</summary>
    public override string ConstraintFamilyId =>
        "productionSetupLink";

    /// <summary>Determines whether the family is enabled.</summary>
    public override bool IsEnabled(
        LotSizingInstance instance,
        StandardLotSizingFormulationOptions options)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(options);

        return options.IncludeProductionSetups &&
               instance.SupplyChain.ProductionRoutings.Count > 0;
    }

    /// <summary>Builds production/setup linking constraints.</summary>
    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        StandardLotSizingFormulationOptions options,
        CancellationToken cancellationToken)
    {
        foreach (
            ProductionRouting routing
            in instance.SupplyChain.ProductionRoutings)
        {
            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                MathematicalVariable production =
                    context.GetVariable(
                        StandardFormulationVariableKeyFactory
                            .CreateProductionKey(
                                routing.Id,
                                period));

                MathematicalVariable setup =
                    context.GetVariable(
                        new MathematicalDomainKeyBuilder(
                            MathematicalDecisionCategory.Setup)
                            .Add(
                                MathematicalDomainKeySegment.Routing,
                                routing.Id)
                            .Add(
                                MathematicalDomainKeySegment.Period,
                                period)
                            .Build());

                ProductionSetupBigMEstimate estimate =
                    _bigMEstimator.Estimate(
                        instance,
                        routing,
                        period,
                        options);

                var expression =
                    new LinearExpressionBuilder()
                        .Add(production)
                        .Subtract(setup, estimate.Value);

                AddConstraint(
                    context,
                    $"productionSetupLink_r{routing.Id}_t{period}",
                    expression.Build(),
                    MathematicalConstraintSense.LessThanOrEqual,
                    0.0,
                    description:
                        "Positive production requires an active " +
                        $"setup. Big-M={estimate.Value:G17}. " +
                        estimate.Source);
            }
        }

        return ValueTask.CompletedTask;
    }
}
