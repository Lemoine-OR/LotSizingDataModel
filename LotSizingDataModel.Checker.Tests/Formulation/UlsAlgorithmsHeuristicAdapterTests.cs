using LotSizingDataModel.Solver.Algorithms.Uls;
using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Results;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class UlsAlgorithmsHeuristicAdapterTests
{
    private static readonly string[] ExpectedIds =
    [
        "chiu-modified-least-unit-cost",
        "chiu-ting-modified-part-period-balancing",
        "freeland-colley",
        "groff",
        "ho-chang-solis-improved-net-least-period-cost",
        "ho-chang-solis-net-least-period-cost",
        "karni-maximum-part-period-gain",
        "least-unit-cost",
        "lot-for-lot",
        "mclaren-order-moment",
        "part-period-balancing",
        "part-period-simplified",
        "patterson-laforge-incremental-part-period",
        "periodic-order-quantity",
        "segerstedt-reformulated-silver-meal",
        "silver-meal",
        "wemmerlov-modified-ppb",
        "wemmerlov-modified-ppb-lalb",
        "wemmerlov-ppb-lalb"
    ];

    [Fact]
    public void Catalog_MatchesPinnedV110HeuristicInventory()
    {
        Assert.Equal(
            ExpectedIds,
            UlsAlgorithmsHeuristicCatalog.SolverIds);

        var descriptors =
            UlsAlgorithmsHeuristicCatalog
                .GetPinnedDescriptors();

        Assert.Equal(
            ExpectedIds.Length,
            descriptors.Count);

        foreach (var descriptor
                 in descriptors)
        {
            Assert.Equal(
                UlsSolverKind.Heuristic,
                descriptor.Kind);
        }
    }

    [Fact]
    public void AllPinnedHeuristics_ReturnValidatedFeasiblePlans()
    {
        var runner =
            new UlsAlgorithmsHeuristicBatchRunner();

        IReadOnlyList<UlsAlgorithmsHeuristicBridgeResult>
            results =
                runner.SolveAll(
                    CreateStationaryPositiveDemandProblem());

        Assert.Equal(
            ExpectedIds.Length,
            results.Count);

        Assert.Equal(
            ExpectedIds,
            results
                .Select(
                    result =>
                        result.SolverId)
                .ToArray());

        foreach (UlsAlgorithmsHeuristicBridgeResult result
                 in results)
        {
            Assert.Equal(
                UlsSolveStatus.Feasible,
                result.ExternalStatus);

            Assert.Equal(
                6,
                result.Horizon);

            Assert.True(
                double.IsFinite(
                    result.ObjectiveValue));
        }
    }

    [Fact]
    public void AllPinnedHeuristics_RespectExactOptimalLowerBound()
    {
        UlsAlgorithmsExactProblemData problem =
            CreateStationaryPositiveDemandProblem();

        var exactBridge =
            new UlsAlgorithmsExactBridge();

        UlsAlgorithmsExactBridgeResult optimum =
            exactBridge.Solve(
                problem,
                UlsAlgorithmsExactMethod.WagnerWhitinClassical);

        var heuristicRunner =
            new UlsAlgorithmsHeuristicBatchRunner();

        foreach (UlsAlgorithmsHeuristicBridgeResult heuristic
                 in heuristicRunner.SolveAll(
                     problem))
        {
            Assert.True(
                heuristic.ObjectiveValue +
                    1.0e-7 >=
                optimum.ObjectiveValue,
                $"Heuristic '{heuristic.SolverId}' reported {heuristic.ObjectiveValue:G17}, below exact optimum {optimum.ObjectiveValue:G17}.");
        }
    }

    [Fact]
    public void ExactMethodId_IsRejectedByHeuristicCatalog()
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                UlsAlgorithmsHeuristicCatalog.GetRequired(
                    "wagner-whitin-classical"));
    }

    private static UlsAlgorithmsExactProblemData
        CreateStationaryPositiveDemandProblem()
    {
        return new UlsAlgorithmsExactProblemData(
            new[]
            {
                4.0,
                6.0,
                3.0,
                5.0,
                7.0,
                2.0
            },
            new[]
            {
                20.0,
                20.0,
                20.0,
                20.0,
                20.0,
                20.0
            },
            new[]
            {
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0
            },
            new[]
            {
                1.0,
                1.0,
                1.0,
                1.0,
                1.0,
                1.0
            });
    }
}
