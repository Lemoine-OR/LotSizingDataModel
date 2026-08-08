using LotSizingDataModel.Checker.Tests.Infrastructure;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Tests.Solver;

/// <summary>
/// Tests numerical cleanup applied to raw solver values before
/// they are mapped to business-domain decisions.
/// </summary>
public sealed class MathematicalVariableValueNormalizerTests
{
    /// <summary>
    /// Reproduces the numerical residual observed on Dellaert-Jeunet
    /// instance ID62, where CPLEX returned a very small negative
    /// inventory value for a mathematically non-negative variable.
    /// </summary>
    [Fact]
    public async Task Id62SmallNegativeInventoryResidual_IsNormalizedToZero()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        MathematicalModel mathematicalModel =
            await BuildModelAsync(data.Instance);

        MathematicalVariable inventoryVariable =
            mathematicalModel.Variables.First(
                variable =>
                    variable.VariableType ==
                        MathematicalVariableType.Continuous &&
                    variable.DomainKey is not null &&
                    variable.DomainKey.StartsWith(
                        "inventory|",
                        StringComparison.Ordinal));

        var normalizer =
            new MathematicalVariableValueNormalizer();

        const double rawSolverValue =
            -4.999947122996673E-09;

        double normalizedValue =
            normalizer.Normalize(
                inventoryVariable,
                rawSolverValue);

        Assert.Equal(
            0.0,
            normalizedValue);
    }

    /// <summary>
    /// Ensures that a materially negative value is not silently
    /// converted to zero.
    /// </summary>
    [Fact]
    public async Task MateriallyNegativeContinuousValue_IsNotNormalizedToZero()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        MathematicalModel mathematicalModel =
            await BuildModelAsync(data.Instance);

        MathematicalVariable inventoryVariable =
            mathematicalModel.Variables.First(
                variable =>
                    variable.VariableType ==
                        MathematicalVariableType.Continuous &&
                    variable.DomainKey is not null &&
                    variable.DomainKey.StartsWith(
                        "inventory|",
                        StringComparison.Ordinal));

        var normalizer =
            new MathematicalVariableValueNormalizer();

        const double rawSolverValue =
            -1.0E-06;

        double normalizedValue =
            normalizer.Normalize(
                inventoryVariable,
                rawSolverValue);

        Assert.Equal(
            rawSolverValue,
            normalizedValue);
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
                AllowFallback =
                    false,
                ValidateGeneratedModel =
                    true,
                CloneGeneratedModel =
                    false
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
            string.Join(
                " | ",
                buildResult.Diagnostics));

        Assert.NotNull(
            buildResult.Model);

        return buildResult.Model!;
    }
}
