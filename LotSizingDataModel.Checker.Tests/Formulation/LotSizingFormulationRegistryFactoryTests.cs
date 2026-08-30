using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class LotSizingFormulationRegistryFactoryTests
{
    [Fact]
    public void DefaultRegistry_ContainsStandardDlspAndCslp()
    {
        MathematicalModelFormulationRegistry registry =
            LotSizingFormulationRegistryFactory.CreateDefault();

        Assert.True(
            registry.TryGet(
                StandardLotSizingFormulation.StandardFormulationId,
                out _));

        Assert.True(
            registry.TryGet(
                SmallBucketSchedulingFormulation.DlspFormulationId,
                out _));

        Assert.True(
            registry.TryGet(
                SmallBucketSchedulingFormulation.CslpFormulationId,
                out _));

        Assert.Equal(
            3,
            registry.GetAll().Count);
    }
}
