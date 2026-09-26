using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Core.Serialization;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Serialization;
using LotSizingDataModel.Solution.Creation;
using LotSizingDataModel.Solution.Validation;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class TransportAssignmentFormulationTests
{
    private static WarehouseReference W(int id) => WarehouseReference.ForStandaloneWarehouse(id);
    private static SupplyChain Chain(int horizon = 4)
    {
        var chain = new SupplyChain(horizon);
        chain.AddItem(new Item(1, "Product", 0));
        for (int i = 1; i <= 3; i++)
        {
            chain.AddStandaloneWarehouse(new StandaloneWarehouse(i, $"W{i}"));
            chain.AddInventory(new Inventory(1, W(i), i == 1 ? 20 : 0));
        }
        chain.AddTransportResource(new TransportResource(1, "Truck")
        {
            CapacityConstraint = new CapacityConstraint(horizon, 10),
            FixedUsageCost = new FixedUsageCost(horizon, 7),
            AdditionalCapacity = new AdditionalCapacity(horizon, 2)
        });
        chain.AddTransportResource(new TransportResource(2, "Train"));
        chain.AddTransportCharacteristic(new TransportCharacteristic(1, 1) { UnitUsageCost = new UnitUsageCost(horizon, 2) });
        chain.AddTransportCharacteristic(new TransportCharacteristic(1, 2));
        chain.AddTransportLane(new TransportLane(10, W(1), W(2)));
        chain.AddTransportLane(new TransportLane(20, W(1), W(3)));
        chain.AddTransportAssignment(new TransportAssignment(10, 1, 2));
        chain.AddTransportAssignment(new TransportAssignment(10, 2, 3));
        chain.AddTransportAssignment(new TransportAssignment(20, 1, 0));
        return chain;
    }

    private static Task<MathematicalModel> Build(SupplyChain chain) =>
        StandardLotSizingFormulationFactory.CreateDefault().BuildAsync(new LotSizingInstance(chain, "transport-regression")).AsTask();
    private static MathematicalVariable Departure(MathematicalModel model, int resource, int destination, int period) =>
        Assert.Single(model.Variables, v => v.Name == $"T_i1_r{resource}_l{(destination == 2 ? 10 : 20)}_t{period}");

    [Fact]
    public async Task SharedCapacityAndFixedCost_AreCountedOncePerResourcePeriod()
    {
        var model = await Build(Chain());
        var a = Departure(model, 1, 2, 1);
        var b = Departure(model, 1, 3, 1);
        var capacity = Assert.Single(model.Constraints, c => c.Name == "transportResourceCapacity_r1_t1");
        Assert.Equal(10, capacity.RightHandSide);
        Assert.Contains(capacity.LeftHandSide.Terms, t => t.VariableId == a.Id && t.Coefficient == 1);
        Assert.Contains(capacity.LeftHandSide.Terms, t => t.VariableId == b.Id && t.Coefficient == 1);
        // Six units on each lane violate the same regular capacity (12 > 10), requiring two extra units.
        double load = capacity.LeftHandSide.Terms.Where(t => t.VariableId == a.Id || t.VariableId == b.Id).Sum(t => 6 * t.Coefficient);
        Assert.Equal(12, load);
        Assert.True(load > capacity.RightHandSide);
        var activation = Assert.Single(model.Variables, v => v.Name == "YR_r1_t1");
        Assert.Equal(7, Assert.Single(model.Objective.Expression.Terms, t => t.VariableId == activation.Id).Coefficient);
        var links = model.Constraints.Where(c => c.Name.StartsWith("transportResourceActivationLink_") && c.Name.EndsWith("_t1")).ToArray();
        Assert.Equal(2, links.Length);
        Assert.All(links, c => Assert.Contains(c.LeftHandSide.Terms, t => t.VariableId == activation.Id && t.Coefficient == -12));
        Assert.Equal(2, Assert.Single(model.Objective.Expression.Terms, t => t.VariableId == a.Id).Coefficient);
    }

    [Fact]
    public async Task DifferentDelays_ZeroDelayAndHorizonEdges_UseDeparturePeriods()
    {
        var model = await Build(Chain());
        var truck = Departure(model, 1, 2, 1);
        var train = Departure(model, 2, 2, 1);
        var instant = Departure(model, 1, 3, 1);
        var arrival3 = Assert.Single(model.Constraints, c => c.Name == "inventoryBalance_i1_w2_t3");
        var arrival4 = Assert.Single(model.Constraints, c => c.Name == "inventoryBalance_i1_w2_t4");
        Assert.Contains(arrival3.LeftHandSide.Terms, t => t.VariableId == truck.Id && t.Coefficient == -1);
        Assert.DoesNotContain(arrival3.LeftHandSide.Terms, t => t.VariableId == train.Id);
        Assert.Contains(arrival4.LeftHandSide.Terms, t => t.VariableId == train.Id && t.Coefficient == -1);
        Assert.Contains(Assert.Single(model.Constraints, c => c.Name == "inventoryBalance_i1_w3_t1").LeftHandSide.Terms,
            t => t.VariableId == instant.Id && t.Coefficient == -1);
        Assert.Contains(Assert.Single(model.Constraints, c => c.Name == "inventoryBalance_i1_w1_t1").LeftHandSide.Terms,
            t => t.VariableId == instant.Id && t.Coefficient == 1);
        var late = Departure(model, 1, 2, 4);
        Assert.Contains(Assert.Single(model.Constraints, c => c.Name == "inventoryBalance_i1_w1_t4").LeftHandSide.Terms,
            t => t.VariableId == late.Id && t.Coefficient == 1);
        Assert.DoesNotContain(model.Constraints.Where(c => c.Name.StartsWith("inventoryBalance_i1_w2_")),
            c => c.LeftHandSide.Terms.Any(t => t.VariableId == late.Id));
        var early = Assert.Single(model.Constraints, c => c.Name == "inventoryBalance_i1_w2_t1");
        var transportIds = model.Variables.Where(v => v.DomainKey.StartsWith(MathematicalDecisionCategory.Transport + "|")).Select(v => v.Id).ToHashSet();
        Assert.DoesNotContain(early.LeftHandSide.Terms, t => transportIds.Contains(t.VariableId));
    }

    [Fact]
    public async Task ForbiddenAssignmentAndIncompatibleItem_CreateNoVariables()
    {
        var chain = Chain();
        var model = await Build(chain);
        Assert.DoesNotContain(model.Variables, v => v.Name == "T_i1_r2_l20_t1");
        chain.TransportCharacteristics.RemoveAll(c => c.TransportResourceId == 2);
        model = await Build(chain);
        Assert.DoesNotContain(model.Variables, v => v.Name == "T_i1_r2_l10_t1");
    }

    [Fact]
    public async Task AdditionalItemCapacity_HasOneSharedBudget()
    {
        var chain = Chain();
        var characteristic = chain.TransportCharacteristics[0];
        characteristic.CapacityConstraint = new CapacityConstraint(4, 5);
        characteristic.AdditionalCapacity = new AdditionalCapacity(4, 2);
        var model = await Build(chain);
        var budget = Assert.Single(model.Constraints, c => c.Name == "transportAdditionalCapacityBudget_i1_r1_t1");
        Assert.Equal(2, budget.RightHandSide);
        Assert.Equal(2, budget.LeftHandSide.Terms.Count);
        Assert.All(budget.LeftHandSide.Terms, t => Assert.Equal(1, t.Coefficient));
        Assert.True(budget.LeftHandSide.Terms.Sum(t => 2 * t.Coefficient) > budget.RightHandSide);
    }

    [Fact]
    public async Task ExistingPerLaneSetup_IsLinkedWithoutDuplicatingResourceActivation()
    {
        var chain = Chain();
        chain.TransportCharacteristics[0].FixedSetupCost = new FixedSetupCost(4, 3);
        var model = await Build(chain);
        Assert.Equal(8, model.Constraints.Count(c => c.Name.StartsWith("transportSetupLink_")));
        Assert.Equal(4, model.Variables.Count(v => v.Name.StartsWith("YR_r1_")));
    }

    [Fact]
    public async Task UnboundedActivation_IsExplicitlyRejectedUnlessBoundIsSupplied()
    {
        var chain = Chain();
        chain.TransportResources[0].CapacityConstraint = null;
        chain.TransportResources[0].AdditionalCapacity = null;
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => Build(chain));
        Assert.Contains("TransportActivationBigM", exception.Message);
        var model = await StandardLotSizingFormulationFactory.Create(new StandardLotSizingFormulationOptions { TransportActivationBigM = 100 })
            .BuildAsync(new LotSizingInstance(chain, "explicit-bound"));
        Assert.Contains(model.Constraints, c => c.Name.StartsWith("transportResourceActivationLink_"));
    }

    [Fact]
    public async Task ScheduledReceiptsAndExtremeDelay_DoNotCreateNegativePeriodVariables()
    {
        var chain = Chain();
        chain.TransportAssignments[0].LeadTime = int.MaxValue;
        chain.Inventories[1].ScheduledReceipt = new ScheduledReceipt(4, 2);
        var model = await Build(chain);
        var first = Assert.Single(model.Constraints, c => c.Name == "inventoryBalance_i1_w2_t1");
        Assert.Equal(2, first.RightHandSide);
        var truckIds = Enumerable.Range(1, 4).Select(t => Departure(model, 1, 2, t).Id).ToHashSet();
        Assert.DoesNotContain(model.Constraints.Where(c => c.Name.StartsWith("inventoryBalance_i1_w2_")),
            c => c.LeftHandSide.Terms.Any(t => truckIds.Contains(t.VariableId)));
    }

    [Fact]
    public async Task LegacySingleLane_ProducesSameModelAsExplicitCentralModel()
    {
        var chain = Chain();
        chain.TransportAssignments.RemoveAll(a => a.TransportResourceId == 2 || a.LaneId == 20);
        chain.TransportLanes.RemoveAll(l => l.Id == 20);
        var serializer = new SupplyChainXmlSerializer();
        var xml = System.Xml.Linq.XDocument.Parse(serializer.SerializeToString(chain, false));
        xml.Root!.Attribute("transportFormatVersion")!.Remove();
        var lane = xml.Root.Element("transportLanes")!.Element("transportLane")!;
        lane.Attribute("id")!.Remove();
        lane.Attribute("name")?.Remove();
        lane.SetAttributeValue("leadTime", 2);
        var legacyLane = new System.Xml.Linq.XElement(lane);
        xml.Root.Element("transportLanes")!.Remove();
        xml.Root.Element("transportAssignments")!.Remove();
        xml.Root.Element("transportResources")!.Elements().First().Add(new System.Xml.Linq.XElement("lanes", legacyLane));
        var migrated = serializer.DeserializeFromString(xml.ToString(), false);
        var before = await Build(chain);
        var after = await Build(migrated);
        Assert.Equal(before.Variables.Select(v => v.DomainKey), after.Variables.Select(v => v.DomainKey));
        Assert.Equal(before.Objective.Expression.Terms.Select(t => (t.VariableId, t.Coefficient)), after.Objective.Expression.Terms.Select(t => (t.VariableId, t.Coefficient)));
        Assert.Equal(before.Constraints.Select(c => (c.RightHandSide, string.Join(";", c.LeftHandSide.Terms.Select(t => $"{t.VariableId}:{t.Coefficient}")))),
            after.Constraints.Select(c => (c.RightHandSide, string.Join(";", c.LeftHandSide.Terms.Select(t => $"{t.VariableId}:{t.Coefficient}")))));
    }

    [Fact]
    public void InstanceSerialization_RoundTripsAssignments()
    {
        var instance = new LotSizingInstance(Chain(), "transport-xml");
        string xml = LotSizingInstanceXmlSerializer.SerializeToString(instance, validateBeforeSerialization: false);
        var copy = LotSizingInstanceXmlSerializer.DeserializeFromString(xml, validateAfterDeserialization: false);
        Assert.Equal(3, copy.SupplyChain.TransportAssignments.Count);
        Assert.Equal(2, copy.SupplyChain.TransportLanes.Count);
    }

    [Fact]
    public void SolutionReferences_RejectRemovedAssignment()
    {
        var chain = Chain();
        var solution = LotSizingSolutionFactory.Create(chain);
        Assert.Equal(3, solution.TransportDecisions.Count);
        Assert.DoesNotContain(new LotSizingSolutionValidator().Validate(solution, chain), i => i.Code == "REF303");
        chain.RemoveTransportAssignment(10, 2);
        Assert.Contains(new LotSizingSolutionValidator().Validate(solution, chain), i => i.Code == "REF303");
    }

    [Fact]
    public async Task InvalidAssignmentCannotBeSilentlyIgnoredByFormulation()
    {
        var chain = Chain();
        chain.TransportAssignments.Add(new TransportAssignment(10, 999, 0));
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => Build(chain));
        Assert.Contains("Invalid transport structure", error.Message);
    }
}
