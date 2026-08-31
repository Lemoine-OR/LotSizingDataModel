namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class LocalBranchingBridgeOptions
{
    public int MaximumIterations
    {
        get;
        init;
    } =
        8;

    public int HammingRadius
    {
        get;
        init;
    } =
        2;

    public int NodeLimit
    {
        get;
        init;
    } =
        1000;

    public ulong Seed
    {
        get;
        init;
    }
}
