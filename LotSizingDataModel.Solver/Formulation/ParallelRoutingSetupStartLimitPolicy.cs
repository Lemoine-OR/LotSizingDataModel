using LotSizingDataModel.Instance.Analysis;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class ParallelRoutingSetupStartLimitPolicy
{
    public int MaximumConcurrentSetupStartsPerItem
    {
        get;
        init;
    } =
        1;

    public ParallelSchedulingCoordinationScope Scope
    {
        get;
        init;
    } =
        ParallelSchedulingCoordinationScope.AcrossAllSites;

    public void EnsureValid()
    {
        if (MaximumConcurrentSetupStartsPerItem <= 0)
        {
            throw new InvalidOperationException(
                "Maximum concurrent setup starts per item must be strictly positive.");
        }
    }
}
