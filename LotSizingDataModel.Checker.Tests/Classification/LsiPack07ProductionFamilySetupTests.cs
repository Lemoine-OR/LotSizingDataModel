using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Core.Validation;
using LotSizingDataModel.Instance.Classification.Notation;
using LotSizingDataModel.Checker.Feasibility;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class LsiPack07ProductionFamilySetupTests
{
    [Fact]
    public void EmptyFamilyCollection_DoesNotSerializeLegacyNode()
    {
        var chain = new SupplyChain
        {
            PlanningHorizon = 2
        };

        Assert.False(
            chain.ShouldSerializeProductionSetupFamilies());
    }

    [Fact]
    public void FamilySetupTime_ResizesAndIsNonNegative()
    {
        var time = new ProductionFamilySetupTime(2, 3.0);

        time.ResizeTimeSeries(4);

        Assert.Equal(4, time.PlanningHorizon);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => time.SetSetupTime(1, -1.0));
    }

    [Fact]
    public void Lsi_ReportsProductionFamilySetup()
    {
        SupplyChain chain = BuildValidChain();

        LotSizingInstanceSignature signature =
            LotSizingInstanceSignatureExtractor.Extract(chain);

        Assert.Equal(
            FeatureState.Present,
            signature.Features.Find(
                LsiFeatureCodes.ProductionSetupFamily)!.State);

        Assert.Equal(
            FeatureState.Present,
            signature.Features.Find(
                LsiFeatureCodes.ProductionSetupFamilyTime)!.State);
    }

    [Fact]
    public void Validator_AcceptsValidFamily()
    {
        SupplyChain chain = BuildValidChain();

        var issues =
            new List<SupplyChainValidator.ValidationIssue>();

        ProductionSetupFamilyValidator.AppendIssues(
            chain,
            issues);

        Assert.DoesNotContain(
            issues,
            issue =>
                issue.Severity ==
                SupplyChainValidator.ValidationSeverity.Error);
    }

    [Fact]
    public void DerivedActivation_IsOrOfMemberItemSetups()
    {
        SupplyChain chain = BuildValidChain();

        var solution = new LotSizingSolution(2);

        var decision1 = new ProductionDecision(1, 2);
        decision1.SetSetupActivated(1, true);

        var decision2 = new ProductionDecision(2, 2);
        decision2.SetSetupActivated(1, false);

        solution.ProductionDecisions.Add(decision1);
        solution.ProductionDecisions.Add(decision2);

        ProductionSetupFamily family =
            Assert.Single(chain.ProductionSetupFamilies);

        Assert.True(
            ProductionSetupFamilyDerivedSemantics
                .IsFamilySetupActivated(
                    chain,
                    solution,
                    family,
                    1));

        Assert.Equal(
            2.5,
            ProductionSetupFamilyDerivedSemantics
                .GetFamilySetupCapacityConsumption(
                    chain,
                    solution,
                    family,
                    1));
    }

    private static SupplyChain BuildValidChain()
    {
        var chain = new SupplyChain
        {
            PlanningHorizon = 2
        };

        chain.Items.Add(new Item(1, "A", 0));
        chain.Items.Add(new Item(2, "B", 0));

        var plant = new Plant(1, "P1", new PlantWarehouse("P1-WH"));
        var workCenter = new WorkCenter(1, "WC1")
        {
            CapacityConstraint =
                new CapacityConstraint(2, 100.0)
        };

        plant.WorkCenters.Add(workCenter);
        chain.Plants.Add(plant);

        chain.ProductionRoutings.Add(
            new ProductionRouting
            {
                Id = 1,
                ItemId = 1,
                WorkCenters =
                {
                    new WorkCenterReference(1, 1)
                }
            });

        chain.ProductionRoutings.Add(
            new ProductionRouting
            {
                Id = 2,
                ItemId = 2,
                WorkCenters =
                {
                    new WorkCenterReference(1, 1)
                }
            });

        var family = new ProductionSetupFamily
        {
            Id = 1,
            Name = "F1",
            WorkCenter =
                new WorkCenterReference(1, 1),
            SetupTime =
                new ProductionFamilySetupTime(2, 2.5)
        };

        family.MemberItemIds.Add(1);
        family.MemberItemIds.Add(2);

        chain.AddProductionSetupFamily(family);

        return chain;
    }
}
