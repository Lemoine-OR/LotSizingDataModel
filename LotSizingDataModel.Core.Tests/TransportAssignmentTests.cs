using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Querying;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Core.Serialization;
using LotSizingDataModel.Core.Validation;

namespace LotSizingDataModel.Core.Tests;

public sealed class TransportAssignmentTests
{
    private static WarehouseReference W(int id) => WarehouseReference.ForStandaloneWarehouse(id);
    private static SupplyChain Chain()
    {
        var chain = new SupplyChain(4);
        for (int i = 1; i <= 3; i++) chain.AddStandaloneWarehouse(new StandaloneWarehouse(i, $"W{i}"));
        chain.AddTransportResource(new TransportResource(1, "Truck") { CapacityConstraint = new CapacityConstraint(4, 10), FixedUsageCost = new FixedUsageCost(4, 7) });
        chain.AddTransportResource(new TransportResource(2, "Train"));
        chain.AddTransportLane(new TransportLane(11, W(1), W(2)));
        chain.AddTransportLane(new TransportLane(12, W(1), W(3)));
        return chain;
    }

    [Fact]
    public void ManyToMany_UsesSingleCentralLaneAndPreservesResourceParameters()
    {
        var chain = Chain();
        chain.AddTransportAssignment(new TransportAssignment(11, 1, 2));
        chain.AddTransportAssignment(new TransportAssignment(11, 2, 3));
        chain.AddTransportAssignment(new TransportAssignment(12, 1, 1));
        Assert.Equal(2, chain.TransportLanes.Count);
        Assert.Equal(3, chain.TransportAssignments.Count);
        Assert.Same(chain.TransportLanes[0], chain.GetTransportLanes(1).First().Lane);
        Assert.Same(chain.TransportLanes[0], chain.GetTransportLanes(2).Single().Lane);
        Assert.Equal(10, chain.TransportResources[0].CapacityConstraint![1]);
        Assert.Equal(7, chain.TransportResources[0].FixedUsageCost![1]);
        chain.TransportAssignments[0].LeadTime = 0;
        Assert.Equal(0, chain.GetTransportLanes(1).First().LeadTime);
        Assert.Equal(3, chain.GetTransportLanes(2).Single().LeadTime);
    }

    [Fact]
    public void UnassignedConstruction_IsStructurallyValidButHasReadinessDiagnostics()
    {
        var chain = Chain();
        var validator = new SupplyChainValidator();
        Assert.DoesNotContain(validator.Validate(chain), i => i.Code.StartsWith("TRN"));
        Assert.Equal(4, validator.ValidateTransportReadiness(chain).Count);
    }

    [Fact]
    public void DirectedUniquenessAndReferenceIdentity_AreEnforced()
    {
        var chain = Chain();
        Assert.Throws<InvalidOperationException>(() => chain.AddTransportLane(new TransportLane(20, W(1), W(2))));
        Assert.Throws<InvalidOperationException>(() => chain.AddTransportLane(new TransportLane(11, W(2), W(3))));
        Assert.Throws<InvalidOperationException>(() => chain.AddTransportLane(new TransportLane(20, W(1), W(1))));
        Assert.Throws<KeyNotFoundException>(() => chain.AddTransportLane(new TransportLane(20, W(1), W(99))));
        chain.AddTransportLane(new TransportLane(20, W(2), W(1)));
        chain.Plants.Add(new Plant(1, "Plant", new PlantWarehouse("Plant warehouse")));
        chain.AddTransportLane(new TransportLane(21, WarehouseReference.ForPlantWarehouse(1), W(1)));
        Assert.Equal(4, chain.TransportLanes.Count);
    }

    [Fact]
    public void AssignmentUniquenessAndReferences_AreEnforced()
    {
        var chain = Chain();
        chain.AddTransportAssignment(new TransportAssignment(11, 1, 2));
        Assert.Throws<InvalidOperationException>(() => chain.AddTransportAssignment(new TransportAssignment(11, 1, 3)));
        Assert.Throws<InvalidOperationException>(() => chain.AddTransportAssignment(new TransportAssignment(99, 1, 0)));
        Assert.Throws<InvalidOperationException>(() => chain.AddTransportAssignment(new TransportAssignment(11, 99, 0)));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TransportAssignment(11, 1, -1));
        var index = new SupplyChainIndex(chain);
        Assert.Same(chain.TransportLanes[0], index.GetRequiredTransportLane(11));
        Assert.True(index.TryGetTransportAssignment(11, 1, out var assignment));
        Assert.Equal(2, assignment!.LeadTime);
    }

    [Fact]
    public void DirectCollectionEdits_AreValidated()
    {
        var chain = Chain();
        chain.TransportLanes.Add(new TransportLane(11, W(1), W(2)));
        chain.TransportAssignments.Add(new TransportAssignment(999, 1, 0));
        chain.TransportAssignments.Add(new TransportAssignment(999, 1, 0));
        var issues = new SupplyChainValidator().Validate(chain);
        foreach (string code in new[] { "TRN004", "TRN005", "TRN006", "TRN007" })
            Assert.Contains(issues, i => i.Code == code);
    }

    [Fact]
    public void Removal_RequiresExplicitReferenceRemoval()
    {
        var chain = Chain();
        chain.AddTransportAssignment(new TransportAssignment(11, 1, 2));
        Assert.Throws<InvalidOperationException>(() => chain.RemoveTransportLane(11));
        Assert.Throws<InvalidOperationException>(() => chain.RemoveTransportResource(1));
        Assert.True(chain.RemoveTransportAssignment(11, 1));
        chain.TransportCharacteristics.Add(new TransportCharacteristic(1, 1));
        Assert.Throws<InvalidOperationException>(() => chain.RemoveTransportResource(1));
        chain.TransportCharacteristics.Clear();
        Assert.True(chain.RemoveTransportResource(1));
        Assert.True(chain.RemoveTransportLane(11));
        Assert.False(chain.RemoveTransportLane(11));
        Assert.Single(chain.TransportResources);
        Assert.Single(chain.TransportLanes);
    }

    [Fact]
    public void FastestPath_PreservesResourceAssignmentAndZeroDelay()
    {
        var chain = Chain();
        chain.AddTransportAssignment(new TransportAssignment(11, 1, 2));
        chain.AddTransportAssignment(new TransportAssignment(11, 2, 3));
        chain.AddTransportLane(new TransportLane(13, W(2), W(3)));
        chain.AddTransportAssignment(new TransportAssignment(13, 2, 0));
        var finder = new TransportPathFinder(chain);
        var path = finder.GetRequiredFastestPath(W(1), W(3));
        Assert.Equal(2, path.TotalLeadTime);
        Assert.Equal(new[] { 1, 2 }, path.Legs.Select(l => l.TransportResource.Id));
        Assert.Equal(new[] { 11, 13 }, path.Legs.Select(l => l.Lane.Id));
        Assert.Null(finder.FindFastestPath(W(3), W(1)));
        var queries = new SupplyChainQueries(chain);
        Assert.Equal(2, queries.GetDirectTransportOptions(W(1), W(2)).Count);
    }

    [Fact]
    public void CentralXml_RoundTripsWithoutNestedLanes()
    {
        var chain = Chain();
        chain.AddTransportAssignment(new TransportAssignment(11, 1, 2));
        var serializer = new SupplyChainXmlSerializer();
        string xml = serializer.SerializeToString(chain, false);
        Assert.DoesNotContain("<lanes", xml);
        Assert.Contains("transportFormatVersion=\"2\"", xml);
        var copy = serializer.DeserializeFromString(xml, false);
        Assert.Equal(xml, serializer.SerializeToString(copy, false));
        Assert.Equal(2, copy.TransportAssignments.Single().LeadTime);
        Assert.Equal(10, copy.TransportResources[0].CapacityConstraint![1]);
        Assert.Equal(7, copy.TransportResources[0].FixedUsageCost![1]);
    }

    internal static string Legacy(string firstDelay = "2", string duplicate = "") => $$"""
        <supplyChain planningHorizon="4">
          <standaloneWarehouses><standaloneWarehouse id="1" name="Nantes"/><standaloneWarehouse id="2" name="Lyon"/><standaloneWarehouse id="3" name="Bordeaux"/></standaloneWarehouses>
          <transportResources>
            <transportResource id="9" name="Truck"><lanes>
              <transportLane leadTime="{{firstDelay}}"><origin kind="standaloneWarehouse" id="1"/><destination kind="standaloneWarehouse" id="2"/></transportLane>
              <transportLane leadTime="1"><origin kind="standaloneWarehouse" id="1"/><destination kind="standaloneWarehouse" id="3"/></transportLane>
              {{duplicate}}
            </lanes></transportResource>
            <transportResource id="8" name="Train"><lanes>
              <transportLane leadTime="3"><origin kind="standaloneWarehouse" id="1"/><destination kind="standaloneWarehouse" id="2"/></transportLane>
              <transportLane leadTime="0"><origin kind="standaloneWarehouse" id="2"/><destination kind="standaloneWarehouse" id="1"/></transportLane>
            </lanes></transportResource>
          </transportResources>
        </supplyChain>
        """;

    [Fact]
    public void LegacyMigration_IsDeterministicIdempotentAndPreservesDelays()
    {
        var serializer = new SupplyChainXmlSerializer();
        string xml = Legacy();
        var chain = serializer.DeserializeFromString(xml, false);
        Assert.Equal(3, chain.TransportLanes.Count);
        Assert.Equal(4, chain.TransportAssignments.Count);
        Assert.Equal(2, chain.FindTransportAssignment(1, 9)!.LeadTime);
        Assert.Equal(3, chain.FindTransportAssignment(1, 8)!.LeadTime);
        Assert.Equal(0, chain.FindTransportAssignment(3, 8)!.LeadTime);
        var document = XDocument.Parse(xml);
        var resources = document.Root!.Element("transportResources")!;
        var reversed = resources.Elements().Reverse().ToArray();
        resources.ReplaceNodes(reversed);
        var second = serializer.DeserializeFromString(document.ToString(), false);
        Assert.Equal(chain.TransportLanes.Select(l => (l.Id, l.Origin.ReferenceId, l.Destination.ReferenceId)),
            second.TransportLanes.Select(l => (l.Id, l.Origin.ReferenceId, l.Destination.ReferenceId)));
        string migrated = serializer.SerializeToString(chain, false);
        Assert.Equal(migrated, serializer.SerializeToString(serializer.DeserializeFromString(migrated, false), false));
        Assert.Contains("<lanes>", xml);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("1.5")]
    [InlineData("2147483648")]
    public void LegacyMigration_RejectsInvalidDelay(string delay) =>
        Assert.Throws<InvalidOperationException>(() => new SupplyChainXmlSerializer().DeserializeFromString(Legacy(delay), false));

    [Fact]
    public void LegacyMigration_RejectsConflictsAndMixedFormats()
    {
        string duplicate = "<transportLane leadTime=\"8\"><origin kind=\"standaloneWarehouse\" id=\"1\"/><destination kind=\"standaloneWarehouse\" id=\"2\"/></transportLane>";
        var serializer = new SupplyChainXmlSerializer();
        Assert.Throws<InvalidOperationException>(() => serializer.DeserializeFromString(Legacy(duplicate: duplicate), false));
        Assert.Throws<InvalidOperationException>(() => serializer.DeserializeFromString(Legacy().Replace("<supplyChain ", "<supplyChain transportFormatVersion=\"2\" "), false));
        Assert.Throws<InvalidOperationException>(() => serializer.DeserializeFromString(Legacy().Replace("<supplyChain ", "<supplyChain transportFormatVersion=\"3\" "), false));
    }

    [Fact]
    public void RawXmlSerializer_RejectsLegacyInsteadOfDiscardingLanes()
    {
        using var reader = new StringReader(Legacy());
        Assert.Throws<InvalidOperationException>(() => new XmlSerializer(typeof(SupplyChain)).Deserialize(reader));
    }

    [Fact]
    public void BuilderAndItemFilteredPaths_UseCentralAssignments()
    {
        var chain = Chain();
        chain.AddItem(new LotSizingDataModel.Core.LogicalModel.Item(1, "Item", 0));
        chain.AddTransportCharacteristic(new TransportCharacteristic(1, 2));
        var builder = new LotSizingDataModel.Core.Building.SupplyChainModelBuilder(chain);
        builder.AddTransportAssignment(11, 1, 0).AddTransportAssignment(11, 2, 3);
        var path = new TransportPathFinder(chain).GetRequiredFastestPathForItem(1, W(1), W(2));
        Assert.Equal(3, path.TotalLeadTime);
        Assert.Equal(2, Assert.Single(path.Legs).TransportResource.Id);
    }
}
