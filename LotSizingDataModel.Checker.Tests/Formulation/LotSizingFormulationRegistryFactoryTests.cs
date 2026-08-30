using LotSizingDataModel.Solver.Formulation;

namespace LotSizingDataModel.Checker.Tests.Formulation;

public sealed class LotSizingFormulationRegistryFactoryTests
{
    [Fact]
    public void DefaultRegistry_ContainsStandardDlspCslpAndPlsp()
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

        Assert.True(
            registry.TryGet(
                SmallBucketSchedulingFormulation.PlspFormulationId,
                out _));

        Assert.Equal(
            4,
            registry.GetAll().Count);
    }
}
