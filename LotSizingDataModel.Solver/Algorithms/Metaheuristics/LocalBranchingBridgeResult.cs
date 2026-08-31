namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class LocalBranchingBridgeResult
{
    public LocalBranchingBridgeResult(
        string algorithmId,
        IReadOnlyList<double> bestValues,
        double bestObjective,
        int exactSolves,
        int relaxationSolves,
        int iterations,
        ulong seed,
        IReadOnlyList<string> trace)
    {
        ArgumentNullException.ThrowIfNull(
            bestValues);

        ArgumentNullException.ThrowIfNull(
            trace);

        AlgorithmId =
            algorithmId?.Trim() ??
            string.Empty;

        BestValues =
            bestValues.ToArray();

        BestObjective =
            bestObjective;

        ExactSolves =
            exactSolves;

        RelaxationSolves =
            relaxationSolves;

        Iterations =
            iterations;

        Seed =
            seed;

        Trace =
            trace.ToArray();

        if (string.IsNullOrWhiteSpace(
                AlgorithmId) ||
            BestValues.Length == 0 ||
            !double.IsFinite(
                BestObjective) ||
            ExactSolves < 0 ||
            RelaxationSolves < 0 ||
            Iterations < 0)
        {
            throw new InvalidOperationException(
                "Local Branching bridge result is invalid.");
        }
    }

    public string AlgorithmId
    {
        get;
    }

    public double[] BestValues
    {
        get;
    }

    public double BestObjective
    {
        get;
    }

    public int ExactSolves
    {
        get;
    }

    public int RelaxationSolves
    {
        get;
    }

    public int Iterations
    {
        get;
    }

    public ulong Seed
    {
        get;
    }

    public string[] Trace
    {
        get;
    }
}
