using LotSizingDataModel.Solver.Algorithms.Uls;
using ULSAlgorithms.Abstractions;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class UlsAlgorithmsExactAdapterTests
{
    [Fact]
    public void Catalog_ExposesOnlyPinnedExactRoadmapMethods()
    {
        Assert.Equal(
            "wagner-whitin-classical",
            UlsAlgorithmsExactMethodCatalog.GetSolverId(
                UlsAlgorithmsExactMethod.WagnerWhitinClassical));

        Assert.Equal(
            "zangwill-network",
            UlsAlgorithmsExactMethodCatalog.GetSolverId(
                UlsAlgorithmsExactMethod.ZangwillNetwork));

        Assert.Equal(
            UlsSolverKind.Exact,
            UlsAlgorithmsExactMethodCatalog
                .GetExactDescriptor(
                    UlsAlgorithmsExactMethod.WagnerWhitinClassical)
                .Kind);

        Assert.Equal(
            UlsSolverKind.Exact,
            UlsAlgorithmsExactMethodCatalog
                .GetExactDescriptor(
                    UlsAlgorithmsExactMethod.ZangwillNetwork)
                .Kind);
    }

    [Fact]
    public void WagnerWhitinAndZangwill_ReturnSameExactObjective()
    {
        var problem =
            CreateToyProblem();

        var bridge =
            new UlsAlgorithmsExactBridge();

        UlsAlgorithmsExactBridgeResult wagnerWhitin =
            bridge.Solve(
                problem,
                UlsAlgorithmsExactMethod.WagnerWhitinClassical);

        UlsAlgorithmsExactBridgeResult zangwill =
            bridge.Solve(
                problem,
                UlsAlgorithmsExactMethod.ZangwillNetwork);

        Assert.Equal(
            problem.Horizon,
            wagnerWhitin.Horizon);

        Assert.Equal(
            problem.Horizon,
            zangwill.Horizon);

        Assert.Equal(
            wagnerWhitin.ObjectiveValue,
            zangwill.ObjectiveValue,
            7);
    }

    [Fact]
    public void ExplicitMethodSelection_HasNoFallbackToken()
    {
        var problem =
            CreateToyProblem();

        var bridge =
            new UlsAlgorithmsExactBridge();

        Assert.Throws<ArgumentOutOfRangeException>(
            () =>
                bridge.Solve(
                    problem,
                    (UlsAlgorithmsExactMethod)999));
    }

    [Fact]
    public void ProblemData_RejectsInconsistentHorizons()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new UlsAlgorithmsExactProblemData(
                    new[] { 1.0, 2.0 },
                    new[] { 3.0 },
                    new[] { 0.0, 0.0 },
                    new[] { 1.0, 1.0 }));
    }

    private static UlsAlgorithmsExactProblemData
        CreateToyProblem()
    {
        return new UlsAlgorithmsExactProblemData(
            new[]
            {
                4.0,
                6.0,
                3.0,
                5.0
            },
            new[]
            {
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
                1.0
            },
            new[]
            {
                1.0,
                1.0,
                1.0,
                0.0
            });
    }
}
