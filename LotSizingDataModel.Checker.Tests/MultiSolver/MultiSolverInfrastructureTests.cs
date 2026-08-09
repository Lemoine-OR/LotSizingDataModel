using LotSizingDataModel.Checker.Tests.Infrastructure;
using LotSizingDataModel.Solver.CoinOrCbc;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.External;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Gurobi;
using LotSizingDataModel.Solver.Modeling;
using LotSizingDataModel.Solver.Xpress;

namespace LotSizingDataModel.Checker.Tests.MultiSolver;

/// <summary>
/// Covers the solver-independent infrastructure used by the optional Gurobi,
/// FICO Xpress MP, and COIN-OR CBC adapters.
/// </summary>
public sealed class MultiSolverInfrastructureTests
{
    /// <summary>
    /// Ensures that the portable LP writer emits the sections and stable
    /// variable identifiers needed by all external solver adapters.
    /// </summary>
    [Fact]
    public async Task PortableLpWriter_WritesReferenceModelWithStableNames()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        MathematicalModel model =
            await BuildModelAsync(
                data.Instance);

        string temporaryDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "LotSizingDataModel.Tests",
                Guid.NewGuid().ToString("N"));

        string path =
            Path.Combine(
                temporaryDirectory,
                "reference.lp");

        try
        {
            new PortableLpModelWriter().Write(
                model,
                path);

            string text =
                await File.ReadAllTextAsync(path);

            Assert.Contains("Minimize", text);
            Assert.Contains("Subject To", text);
            Assert.Contains("Bounds", text);
            Assert.Contains("Binaries", text);
            Assert.Contains("v_1", text);
            Assert.EndsWith(
    "End" + Environment.NewLine,
    text,
    StringComparison.Ordinal);
        }
        finally
        {
            if (Directory.Exists(temporaryDirectory))
            {
                Directory.Delete(
                    temporaryDirectory,
                    recursive: true);
            }
        }
    }

    /// <summary>
    /// Ensures that the common solution parser accepts both a plain Gurobi
    /// SOL row and an indexed CBC solution row.
    /// </summary>
    [Fact]
    public void NamedSolutionParser_ParsesGurobiAndCbcRows()
    {
        IReadOnlyDictionary<int, double> values =
            NamedSolutionValueParser.ParseLines(
            [
                "# Objective value = 377.68",
                "v_12 1.25",
                "      17 v_93                 1                       40"
            ]);

        Assert.Equal(2, values.Count);
        Assert.Equal(1.25, values[12]);
        Assert.Equal(1.0, values[93]);
    }

    /// <summary>
    /// Verifies the documented Xpress ASCII solution layout in which the
    /// column name is field 2 and the activity is field 5.
    /// </summary>
    [Fact]
    public void XpressAsciiSolutionParser_UsesActivityField()
    {
        IReadOnlyDictionary<int, double> values =
            XpressAsciiSolutionValueParser.ParseLines(
            [
                "1 v_27 C BS 12.75 0.0"
            ]);

        Assert.Single(values);
        Assert.Equal(12.75, values[27]);
    }

    /// <summary>
    /// Verifies that all optional adapters can be instantiated without their
    /// native solver runtimes being installed on the test machine.
    /// </summary>
    [Fact]
    public void OptionalAdapters_HaveExpectedSolverKindsWithoutNativeRuntime()
    {
        var gurobi =
            new GurobiSolverAdapter();

        var xpress =
            new XpressSolverAdapter();

        var cbc =
            new CoinOrCbcSolverAdapter();

        Assert.Equal(SolverKind.Gurobi, gurobi.SolverKind);
        Assert.Equal(SolverKind.Xpress, xpress.SolverKind);
        Assert.Equal(SolverKind.CoinOrCbc, cbc.SolverKind);

        Assert.Equal(
            "LotSizingDataModel.Solver.Gurobi",
            gurobi.AdapterId);
        Assert.Equal(
            "LotSizingDataModel.Solver.Xpress",
            xpress.AdapterId);
        Assert.Equal(
            "LotSizingDataModel.Solver.CoinOrCbc",
            cbc.AdapterId);
    }

    private static async Task<MathematicalModel> BuildModelAsync(
        LotSizingDataModel.Instance.LotSizingInstance instance)
    {
        MathematicalModelFormulationRegistry formulationRegistry =
            StandardLotSizingFormulationRegistryFactory.Create(
                new StandardLotSizingFormulationOptions());

        var buildOptions =
            new MathematicalModelBuildOptions
            {
                RequestedFormulationId =
                    StandardLotSizingFormulation.StandardFormulationId,
                AllowFallback = false,
                ValidateGeneratedModel = true,
                CloneGeneratedModel = false
            };

        var buildService =
            new MathematicalModelBuildService();

        MathematicalModelBuildResult buildResult =
            await buildService.BuildAsync(
                instance,
                formulationRegistry,
                buildOptions,
                CancellationToken.None);

        Assert.True(
            buildResult.IsSuccessful,
            buildResult.FailureMessage ??
            string.Join(" | ", buildResult.Diagnostics));
        Assert.NotNull(buildResult.Model);

        return buildResult.Model!;
    }
}
