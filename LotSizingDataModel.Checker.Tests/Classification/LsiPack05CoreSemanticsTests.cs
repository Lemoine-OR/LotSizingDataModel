using LotSizingDataModel.Core;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Core.DecisionModel.Planning;
using LotSizingDataModel.Instance.Classification.Notation;
using LotSizingDataModel.Core.Serialization;

namespace LotSizingDataModel.Checker.Tests.Classification;

public sealed class LsiPack05CoreSemanticsTests
{
    [Theory]
    [InlineData(PlanningBucketMode.BigBucket, BucketStructureKind.BigBucket)]
    [InlineData(PlanningBucketMode.SmallBucket, BucketStructureKind.SmallBucket)]
    [InlineData(PlanningBucketMode.MacroMicro, BucketStructureKind.MacroMicro)]
    [InlineData(PlanningBucketMode.Hybrid, BucketStructureKind.Hybrid)]
    public void ExplicitPlanningContext_PopulatesPiBucket(
        PlanningBucketMode mode,
        BucketStructureKind expected)
    {
        var chain = new SupplyChain
        {
            PlanningHorizon = 2,
            PlanningContext =
                new LotSizingPlanningContext
                {
                    BucketMode = mode
                }
        };

        LotSizingInstanceSignature signature =
            LotSizingInstanceSignatureExtractor.Extract(chain);

        Assert.Equal(
            expected,
            signature.Planning.BucketStructure);
    }

    [Fact]
    public void ExplicitObjectivePolicy_PopulatesGamma()
    {
        var chain = new SupplyChain
        {
            PlanningHorizon = 2
        };

        var policy = new OptimizationObjectivePolicy
        {
            AggregationMode = ObjectiveAggregationMode.WeightedSum
        };

        policy.Criteria.Add(
            new OptimizationObjectiveCriterion
            {
                Kind = OptimizationObjectiveKind.Economic,
                Weight = 1.0,
                Priority = 0
            });

        policy.Criteria.Add(
            new OptimizationObjectiveCriterion
            {
                Kind = OptimizationObjectiveKind.Financial,
                Weight = 1.0,
                Priority = 1
            });

        chain.ObjectivePolicy = policy;

        LotSizingInstanceSignature signature =
            LotSizingInstanceSignatureExtractor.Extract(chain);

        Assert.Equal(
            FeatureState.Present,
            signature.Objective.State);

        Assert.Equal(
            ObjectiveAggregationKind.WeightedSum,
            signature.Objective.Aggregation);

        Assert.Contains(
            ObjectiveComponentKind.Economic,
            signature.Objective.Components);

        Assert.Contains(
            ObjectiveComponentKind.Financial,
            signature.Objective.Components);
    }

    [Fact]
    public void ExplicitSemantics_RoundTripThroughCoreXml()
    {
        var chain = new SupplyChain
        {
            PlanningHorizon = 2,
            PlanningContext =
                new LotSizingPlanningContext
                {
                    BucketMode =
                        PlanningBucketMode.BigBucket
                }
        };

        var policy = new OptimizationObjectivePolicy
        {
            AggregationMode =
                ObjectiveAggregationMode.Single
        };

        policy.Criteria.Add(
            new OptimizationObjectiveCriterion
            {
                Kind =
                    OptimizationObjectiveKind.Economic,
                Priority = 0
            });

        chain.ObjectivePolicy = policy;

        var serializer =
            new SupplyChainXmlSerializer();

        string xml =
            serializer.SerializeToString(
                chain,
                validateBeforeSerialization: false);

        Assert.Contains(
            "<planningContext",
            xml,
            StringComparison.Ordinal);

        Assert.Contains(
            "<objectivePolicy",
            xml,
            StringComparison.Ordinal);

        SupplyChain clone =
            serializer.DeserializeFromString(
                xml,
                validateAfterDeserialization: false);

        Assert.Equal(
            PlanningBucketMode.BigBucket,
            clone.PlanningContext!.BucketMode);

        Assert.Equal(
            OptimizationObjectiveKind.Economic,
            clone.ObjectivePolicy!.PrimaryObjectiveKind);
    }

    [Fact]
    public void NullExplicitSemantics_PreservesUnknownLsiFields()
    {
        var chain = new SupplyChain
        {
            PlanningHorizon = 2
        };

        LotSizingInstanceSignature signature =
            LotSizingInstanceSignatureExtractor.Extract(chain);

        Assert.Equal(
            BucketStructureKind.Unknown,
            signature.Planning.BucketStructure);

        Assert.Equal(
            FeatureState.Unknown,
            signature.Objective.State);
    }
}
