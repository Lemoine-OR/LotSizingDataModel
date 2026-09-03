using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Constraints;
using LotSizingDataModel.Core.DecisionModel.Costs;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Core.Validation;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Notation;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class ProductionSetupFamilyStableIntegrationTests
{
    [Fact]
    public async Task SetupFamily_IsRepresentedClassifiedAndFormulated()
    {
        var chain = new SupplyChain(2);
        chain.Items.Add(new Item(1, "A", 0));
        chain.Items.Add(new Item(2, "B", 0));

        var plant =
            new Plant(
                1,
                "P1",
                new PlantWarehouse("P1-WH"));

        plant.WorkCenters.Add(new WorkCenter(1, "WC1"));
        chain.Plants.Add(plant);

        var r1 = new ProductionRouting(1, 1, 1, 0);
        r1.AddWorkCenter(1);

        var r2 = new ProductionRouting(2, 2, 1, 0);
        r2.AddWorkCenter(1);

        chain.ProductionRoutings.Add(r1);
        chain.ProductionRoutings.Add(r2);

        chain.ProductionCharacteristics.Add(
            new ProductionCharacteristic(1, 1, 1)
            {
                FixedSetupCost =
                    new FixedSetupCost(2, 1.0)
            });

        chain.ProductionCharacteristics.Add(
            new ProductionCharacteristic(2, 1, 1)
            {
                FixedSetupCost =
                    new FixedSetupCost(2, 1.0)
            });

        var family =
            new ProductionSetupFamily
            {
                Id = 10,
                Name = "F10",
                WorkCenter =
                    new WorkCenterReference(1, 1),
                SetupTime =
                    new ProductionFamilySetupTime(2, 0.0)
            };

        family.MemberItemIds.Add(1);
        family.MemberItemIds.Add(2);
        chain.AddProductionSetupFamily(family);

        var validation =
            new SupplyChainValidator().Validate(chain);

        Assert.DoesNotContain(
            validation,
            issue =>
                issue.Severity ==
                SupplyChainValidator.ValidationSeverity.Error);

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(chain);

        Assert.True(features.HasProductionSetupFamilies);
        Assert.True(features.HasProductionSetupFamilyTimes);

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features,
                chain);

        UniversalLotSizingNotation notation =
            new UniversalNotationGenerator().Generate(descriptor);

        Assert.Contains(
            UniversalNotationFeature.ProductionSetupFamily,
            notation.Beta.Features);

        Assert.Contains(
            UniversalNotationFeature.ProductionSetupFamilyTime,
            notation.Beta.Features);

        var instance =
            new LotSizingInstance
            {
                SupplyChain = chain
            };

        var model =
            await StandardLotSizingFormulationFactory
                .CreateDefault()
                .BuildAsync(instance);

        Assert.Contains(
            model.Variables,
            variable =>
                variable.DomainKey.StartsWith(
                    MathematicalDecisionCategory
                        .AuxiliaryProductionFamilySetup + "|",
                    StringComparison.Ordinal));
    }
}
