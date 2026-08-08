using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Estimates a valid finite upper bound for one production
/// quantity used in a production/setup linking constraint.
/// </summary>
public interface IProductionSetupBigMEstimator
{
    /// <summary>
    /// Estimates the production upper bound for one routing and
    /// planning period.
    /// </summary>
    /// <param name="instance">Source lot-sizing instance.</param>
    /// <param name="routing">Production routing.</param>
    /// <param name="period">One-based planning period.</param>
    /// <param name="options">Standard formulation options.</param>
    /// <returns>Computed Big-M estimate.</returns>
    ProductionSetupBigMEstimate Estimate(
        LotSizingInstance instance,
        ProductionRouting routing,
        int period,
        StandardLotSizingFormulationOptions options);
}
