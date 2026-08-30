namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Creates the complete built-in formulation registry.
/// </summary>
public static class LotSizingFormulationRegistryFactory
{
    public static MathematicalModelFormulationRegistry CreateDefault()
    {
        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            GlspSchedulingFormulationFactory.CreateDefault());

        registry.Register(
            SmallBucketSchedulingFormulationFactory.CreatePlsp());

        registry.Register(
            SmallBucketSchedulingFormulationFactory.CreateCslp());

        registry.Register(
            SmallBucketSchedulingFormulationFactory.CreateDlsp());

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        return registry;
    }
}
