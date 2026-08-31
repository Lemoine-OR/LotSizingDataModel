namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class DebConstraintGaBridgeOptions
{
    public int PopulationSize
    {
        get;
        init;
    } =
        40;

    public int MaximumGenerations
    {
        get;
        init;
    } =
        50;

    public double CrossoverProbability
    {
        get;
        init;
    } =
        0.9;

    public double MutationProbability
    {
        get;
        init;
    } =
        -1.0;

    public double DistributionIndex
    {
        get;
        init;
    } =
        20.0;

    public ulong Seed
    {
        get;
        init;
    }

    public double EqualityTolerance
    {
        get;
        init;
    } =
        1.0e-6;

    public MathematicalModelMetaheuristicEncodingOptions
        Encoding
    {
        get;
        init;
    } =
        new();
}
