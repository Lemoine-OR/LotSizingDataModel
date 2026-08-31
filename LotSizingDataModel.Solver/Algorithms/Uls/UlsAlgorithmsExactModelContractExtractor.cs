using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Extracts the narrow canonical SI-ULS contract accepted by
/// the alpha.34 exact adapters.
/// </summary>
public sealed class UlsAlgorithmsExactModelContractExtractor
{
    private const double Tolerance = 1.0e-10;

    public UlsAlgorithmsExactProblemData Extract(
        LotSizingInstance instance,
        MathematicalModel model)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            model);

        if (instance.PlanningHorizon <= 0)
        {
            throw new NotSupportedException(
                "ULSAlgorithms exact adapters require a positive planning horizon.");
        }

        if (instance.SupplyChain.Demands.Count != 1 ||
            instance.SupplyChain.ProductionRoutings.Count != 1 ||
            instance.SupplyChain.Inventories.Count != 1)
        {
            throw new NotSupportedException(
                "ULSAlgorithms exact adapters require exactly one demand, one production routing and one inventory.");
        }

        var routing =
            instance.SupplyChain.ProductionRoutings[0];

        var inventory =
            instance.SupplyChain.Inventories[0];

        if (routing.LeadTime != 0)
        {
            throw new NotSupportedException(
                "The alpha.34 ULSAlgorithms bridge requires zero production lead time.");
        }

        if (Math.Abs(inventory.InitialInventory) >
            Tolerance)
        {
            throw new NotSupportedException(
                "The alpha.34 ULSAlgorithms bridge requires fixed zero initial inventory.");
        }

        if (inventory.ScheduledReceipt is not null)
        {
            for (int period = 1;
                 period <= instance.PlanningHorizon;
                 period++)
            {
                if (Math.Abs(
                        inventory.ScheduledReceipt[period]) >
                    Tolerance)
                {
                    throw new NotSupportedException(
                        "Scheduled receipts are outside the alpha.34 canonical ULS contract.");
                }
            }
        }

        if (instance.SupplyChain.ComponentRequirements.Count != 0 ||
            instance.SupplyChain.SupplierDeliveries.Count != 0 ||
            instance.SupplyChain.TransportCharacteristics.Count != 0)
        {
            throw new NotSupportedException(
                "BOM, procurement and transport extensions are outside the alpha.34 canonical ULS contract.");
        }

        if (model.Objective.Sense !=
            ObjectiveSense.Minimize)
        {
            throw new NotSupportedException(
                "ULSAlgorithms exact adapters require a minimization objective.");
        }

        if (Math.Abs(
                model.Objective.Expression.Constant) >
            Tolerance)
        {
            throw new NotSupportedException(
                "The alpha.34 canonical ULS objective cannot contain an additive constant.");
        }

        AssertCanonicalConstraintShape(
            model);

        int horizon =
            instance.PlanningHorizon;

        double[] demands =
            new double[horizon];

        var demand =
            instance.SupplyChain.Demands[0];

        for (int period = 1;
             period <= horizon;
             period++)
        {
            demands[period - 1] =
                demand.GetQuantity(
                    period);
        }

        double[] setupCosts =
            new double[horizon];

        double[] productionCosts =
            new double[horizon];

        double[] holdingCosts =
            new double[horizon];

        var coefficientByVariable =
            model.Objective.Expression.Terms.ToDictionary(
                term => term.VariableId,
                term => term.Coefficient);

        int[] productionCounts =
            new int[horizon];

        int[] setupCounts =
            new int[horizon];

        int[] inventoryCounts =
            new int[horizon];

        int[] deliveryCounts =
            new int[horizon];

        foreach (MathematicalVariable variable
                 in model.Variables)
        {
            if (!MathematicalDomainKey.TryParse(
                    variable.DomainKey,
                    out MathematicalDomainKey? key) ||
                key is null)
            {
                throw new NotSupportedException(
                    $"Variable '{variable.Name}' has no canonical domain key.");
            }

            if (!key.TryGetInt32(
                    "period",
                    out int period) ||
                period < 1 ||
                period > horizon)
            {
                throw new NotSupportedException(
                    $"Variable '{variable.Name}' has no valid canonical planning period.");
            }

            int index =
                period - 1;

            coefficientByVariable.TryGetValue(
                variable.Id,
                out double coefficient);

            switch (key.Category)
            {
                case MathematicalDecisionCategory.Production:
                    productionCounts[index]++;
                    productionCosts[index] += coefficient;
                    break;

                case MathematicalDecisionCategory.Setup:
                    setupCounts[index]++;
                    setupCosts[index] += coefficient;
                    break;

                case MathematicalDecisionCategory.Inventory:
                    inventoryCounts[index]++;
                    holdingCosts[index] += coefficient;
                    break;

                case MathematicalDecisionCategory.Delivery:
                    deliveryCounts[index]++;

                    if (Math.Abs(coefficient) >
                        Tolerance)
                    {
                        throw new NotSupportedException(
                            "Delivery-cost terms are outside the alpha.34 canonical ULS objective.");
                    }

                    break;

                default:
                    throw new NotSupportedException(
                        $"Decision category '{key.Category}' is outside the alpha.34 canonical ULS model shape.");
            }
        }

        for (int index = 0;
             index < horizon;
             index++)
        {
            if (productionCounts[index] != 1 ||
                setupCounts[index] != 1 ||
                inventoryCounts[index] != 1 ||
                deliveryCounts[index] > 1)
            {
                throw new NotSupportedException(
                    $"Period {index + 1} does not contain exactly one production/setup/inventory variable or contains multiple delivery variables.");
            }
        }

        return new UlsAlgorithmsExactProblemData(
            demands,
            setupCosts,
            productionCosts,
            holdingCosts);
    }

    private static void AssertCanonicalConstraintShape(
        MathematicalModel model)
    {
        foreach (LinearConstraint constraint
                 in model.Constraints)
        {
            string name =
                constraint.Name ?? string.Empty;

            bool allowed =
                name.StartsWith(
                    "inventoryBalance_",
                    StringComparison.Ordinal) ||
                name.StartsWith(
                    "demand_",
                    StringComparison.Ordinal) ||
                name.StartsWith(
                    "productionSetupLink_",
                    StringComparison.Ordinal);

            if (!allowed)
            {
                throw new NotSupportedException(
                    $"Constraint '{name}' is outside the alpha.34 canonical ULS formulation shape.");
            }
        }
    }
}
