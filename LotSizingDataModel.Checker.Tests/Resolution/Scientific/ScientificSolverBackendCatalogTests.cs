using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Resolution.Scientific;

namespace LotSizingDataModel.Checker.Tests.Resolution.Scientific;

public sealed class ScientificSolverBackendCatalogTests
{
    [Fact]
    public void BackendCatalog_ContainsFourConcreteMilpSolvers()
    {
        Assert.Equal(
            4,
            ScientificSolverBackendCatalog.All.Count);

        Assert.Contains(
            ScientificSolverBackendCatalog.All,
            backend => backend.SolverKind == SolverKind.Cplex);

        Assert.Contains(
            ScientificSolverBackendCatalog.All,
            backend => backend.SolverKind == SolverKind.Gurobi);

        Assert.Contains(
            ScientificSolverBackendCatalog.All,
            backend => backend.SolverKind == SolverKind.Xpress);

        Assert.Contains(
            ScientificSolverBackendCatalog.All,
            backend => backend.SolverKind == SolverKind.CoinOrCbc);
    }

    [Fact]
    public void Automatic_IsSelectionModeNotBackend()
    {
        Assert.Null(
            ScientificSolverBackendCatalog.Find(
                SolverKind.Automatic));

        Assert.Null(
            ScientificSolverBackendCatalog.Find(
                SolverKind.Unknown));
    }

    [Fact]
    public void AllCurrentBackendsSupportGeneralMilpMethod()
    {
        Assert.All(
            ScientificSolverBackendCatalog.All,
            backend =>
                Assert.True(
                    backend.Supports(
                        ScientificSolutionMethodCatalog.GeneralMilp)));
    }
}
