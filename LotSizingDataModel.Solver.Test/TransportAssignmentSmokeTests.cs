using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Cplex;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation;

/// <summary>Optional native CPLEX regression, run with --transport-smoke.</summary>
public static class TransportAssignmentSmokeTests
{
    public static async Task RunAsync()
    {
        await CheckAsync(3, false, 19);
        await CheckAsync(6, false, null);
        await CheckAsync(6, true, 37);
        Console.WriteLine("Transport native regressions: 3/3 passed.");
    }

    private static async Task CheckAsync(int quantityPerLane, bool additional, double? expected)
    {
        var chain = new SupplyChain(1);
        chain.AddItem(new Item(1, "Item", 0));
        for (int i = 1; i <= 3; i++)
        {
            chain.AddStandaloneWarehouse(new StandaloneWarehouse(i, $"W{i}"));
            chain.AddInventory(Inventory.ForStandaloneWarehouse(1, i, i == 1 ? 20 : 0));
        }
        var truck = new TransportResource(1, "Truck")
        {
            CapacityConstraint = new CapacityConstraint(1, 10),
            FixedUsageCost = new FixedUsageCost(1, 7)
        };
        if (additional)
        {
            truck.AdditionalCapacity = new AdditionalCapacity(1, 2);
            truck.AdditionalCapacityCost = new AdditionalCapacityCost(1, 3);
        }
        chain.AddTransportResource(truck);
        chain.AddTransportCharacteristic(new TransportCharacteristic(1, 1) { UnitUsageCost = new UnitUsageCost(1, 2) });
        for (int i = 2; i <= 3; i++)
        {
            chain.AddTransportLane(new TransportLane(i, WarehouseReference.ForStandaloneWarehouse(1), WarehouseReference.ForStandaloneWarehouse(i)));
            chain.AddTransportAssignment(new TransportAssignment(i, 1, 0));
        }
        var model = await StandardLotSizingFormulationFactory.CreateDefault().BuildAsync(new LotSizingInstance(chain, "native-transport"));
        // Force the two planned departures; the actual MILP must enforce shared capacity and costs.
        foreach (var variable in model.Variables.Where(v => v.Name.StartsWith("T_i1_", StringComparison.Ordinal)))
            variable.LowerBound = quantityPerLane;
        var result = await new CplexSolverAdapter().SolveAsync(new MathematicalModelSolveRequest { Model = model });
        Console.WriteLine($"quantity/lane={quantityPerLane}; extra={additional}; status={result.TerminationReason}; objective={result.ObjectiveValue}");
        if (expected is double objective)
        {
            if (!result.IsOptimal || result.ObjectiveValue is not double actual || Math.Abs(actual - objective) > 1e-7)
                throw new InvalidOperationException($"Expected optimal objective {objective}. {string.Join("; ", result.Diagnostics)}");
        }
        else if (result.TerminationReason != LotSizingDataModel.Solver.Common.SolverTerminationReason.Infeasible)
            throw new InvalidOperationException($"Expected infeasible shared-capacity case. {string.Join("; ", result.Diagnostics)}");
    }
}
