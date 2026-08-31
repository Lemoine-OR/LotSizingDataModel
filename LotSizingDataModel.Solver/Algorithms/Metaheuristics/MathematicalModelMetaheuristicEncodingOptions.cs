namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class MathematicalModelMetaheuristicEncodingOptions
{
    public IReadOnlyDictionary<int, MetaheuristicVariableBounds>
        BoundOverrides
    {
        get;
        init;
    } =
        new Dictionary<int, MetaheuristicVariableBounds>();
}
