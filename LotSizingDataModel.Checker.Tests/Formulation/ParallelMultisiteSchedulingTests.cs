using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;
using LotSizingDataModel.Instance.Analysis;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class ParallelMultisiteSchedulingTests
{
    [Fact]
    public void Analyzer_DetectsMultiSiteParallelRoutingsWithoutReinterpretingWorkCenters()
    {
        ProductionRouting first =
            CreateRouting(
                routingId: 1,
                itemId: 7,
                plantId: 10,
                workCenterIds:
                    new[]
                    {
                        1,
                        2
                    });

        ProductionRouting second =
            CreateRouting(
                routingId: 2,
                itemId: 7,
                plantId: 20,
                workCenterIds:
                    new[]
                    {
                        3
                    });

        IReadOnlyList<ParallelRoutingTopologyDescriptor> analysis =
            ParallelSchedulingTopologyAnalyzer.Analyze(
                new[]
                {
                    first,
                    second
                });

        ParallelRoutingTopologyDescriptor descriptor =
            Assert.Single(
                analysis);

        Assert.True(
            descriptor.IsMultiSite);

        Assert.False(
            descriptor.HasMultipleRoutingsWithinPlant);

        Assert.Equal(
            2,
            first.WorkCenters.Count);
    }

    [Fact]
    public void Decorator_AddsAcrossSiteSetupStartLimitAndPreservesSource()
    {
        ProductionRouting first =
            CreateRouting(
                1,
                7,
                10,
                new[]
                {
                    1
                });

        ProductionRouting second =
            CreateRouting(
                2,
                7,
                20,
                new[]
                {
                    1
                });

        MathematicalModel source =
            CreateSetupModel(
                new[]
                {
                    first,
                    second
                },
                periods:
                    2);

        var decorator =
            new ParallelRoutingSetupStartModelDecorator();

        MathematicalModel decorated =
            decorator.Apply(
                source,
                new[]
                {
                    first,
                    second
                },
                new ParallelRoutingSetupStartLimitPolicy
                {
                    MaximumConcurrentSetupStartsPerItem =
                        1,

                    Scope =
                        ParallelSchedulingCoordinationScope.AcrossAllSites
                });

        Assert.Empty(
            source.Constraints);

        Assert.Equal(
            2,
            decorated.Constraints.Count);

        foreach (LinearConstraint constraint
                 in decorated.Constraints)
        {
            Assert.Equal(
                MathematicalConstraintSense.LessThanOrEqual,
                constraint.Sense);

            Assert.Equal(
                1.0,
                constraint.RightHandSide,
                10);

            Assert.Equal(
                2,
                constraint.LeftHandSide.Terms.Count);
        }
    }

    [Fact]
    public void Decorator_WithinPlantDoesNotCoupleDifferentSites()
    {
        ProductionRouting first =
            CreateRouting(
                1,
                7,
                10,
                new[]
                {
                    1
                });

        ProductionRouting second =
            CreateRouting(
                2,
                7,
                20,
                new[]
                {
                    1
                });

        MathematicalModel source =
            CreateSetupModel(
                new[]
                {
                    first,
                    second
                },
                periods:
                    1);

        var decorator =
            new ParallelRoutingSetupStartModelDecorator();

        MathematicalModel decorated =
            decorator.Apply(
                source,
                new[]
                {
                    first,
                    second
                },
                new ParallelRoutingSetupStartLimitPolicy
                {
                    MaximumConcurrentSetupStartsPerItem =
                        1,

                    Scope =
                        ParallelSchedulingCoordinationScope.WithinEachPlant
                });

        Assert.Empty(
            decorated.Constraints);
    }

    [Fact]
    public void Decorator_LimitsSetupStartsNotPersistentState()
    {
        ProductionRouting first =
            CreateRouting(
                1,
                7,
                10,
                new[]
                {
                    1
                });

        ProductionRouting second =
            CreateRouting(
                2,
                7,
                10,
                new[]
                {
                    2
                });

        MathematicalModel source =
            CreateSetupModel(
                new[]
                {
                    first,
                    second
                },
                periods:
                    1);

        source.AddVariable(
            new MathematicalVariable(
                99,
                "persistentState",
                MathematicalVariableType.Binary,
                0.0,
                1.0)
            {
                DomainKey =
                    "auxiliaryPersistentSetupState|routing=1|period=1"
            });

        var decorator =
            new ParallelRoutingSetupStartModelDecorator();

        MathematicalModel decorated =
            decorator.Apply(
                source,
                new[]
                {
                    first,
                    second
                },
                new ParallelRoutingSetupStartLimitPolicy
                {
                    MaximumConcurrentSetupStartsPerItem =
                        1
                });

        LinearConstraint constraint =
            Assert.Single(
                decorated.Constraints);

        Assert.DoesNotContain(
            constraint.LeftHandSide.Terms,
            term =>
                term.VariableId ==
                99);
    }

    private static ProductionRouting CreateRouting(
        int routingId,
        int itemId,
        int plantId,
        IReadOnlyList<int> workCenterIds)
    {
        var routing =
            new ProductionRouting(
                routingId,
                itemId,
                plantId,
                leadTime:
                    0);

        foreach (int workCenterId
                 in workCenterIds)
        {
            routing.AddWorkCenter(
                new WorkCenterReference(
                    plantId,
                    workCenterId));
        }

        return routing;
    }

    private static MathematicalModel CreateSetupModel(
        IReadOnlyList<ProductionRouting> routings,
        int periods)
    {
        var model =
            new MathematicalModel
            {
                Name =
                    "parallel-scheduling-test"
            };

        int variableId =
            1;

        foreach (ProductionRouting routing
                 in routings)
        {
            for (int period = 1;
                 period <= periods;
                 period++)
            {
                model.AddVariable(
                    new MathematicalVariable(
                        variableId++,
                        $"y_r{routing.Id}_t{period}",
                        MathematicalVariableType.Binary,
                        0.0,
                        1.0)
                    {
                        DomainKey =
                            $"{MathematicalDecisionCategory.Setup}" +
                            $"|{MathematicalDomainKeySegment.Routing}={routing.Id}" +
                            $"|{MathematicalDomainKeySegment.Period}={period}"
                    });
            }
        }

        model.EnsureValid();

        return model;
    }
}
