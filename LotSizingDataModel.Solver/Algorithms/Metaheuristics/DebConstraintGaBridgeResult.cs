namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class DebConstraintGaBridgeResult
{
    public DebConstraintGaBridgeResult(
        string algorithmId,
        IReadOnlyList<double> rawSearchPoint,
        IReadOnlyList<double> modelVariableValues,
        double objectiveValue,
        double totalConstraintViolation,
        bool isFeasible,
        int evaluations,
        int iterations,
        ulong seed,
        TimeSpan duration)
    {
        ArgumentNullException.ThrowIfNull(
            rawSearchPoint);

        ArgumentNullException.ThrowIfNull(
            modelVariableValues);

        AlgorithmId =
            algorithmId?.Trim() ??
            string.Empty;

        RawSearchPoint =
            rawSearchPoint.ToArray();

        ModelVariableValues =
            modelVariableValues.ToArray();

        ObjectiveValue =
            objectiveValue;

        TotalConstraintViolation =
            totalConstraintViolation;

        IsFeasible =
            isFeasible;

        Evaluations =
            evaluations;

        Iterations =
            iterations;

        Seed =
            seed;

        Duration =
            duration;

        EnsureValid();
    }

    public string AlgorithmId
    {
        get;
    }

    public double[] RawSearchPoint
    {
        get;
    }

    public double[] ModelVariableValues
    {
        get;
    }

    public double ObjectiveValue
    {
        get;
    }

    public double TotalConstraintViolation
    {
        get;
    }

    public bool IsFeasible
    {
        get;
    }

    public int Evaluations
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

    public TimeSpan Duration
    {
        get;
    }

    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(
                AlgorithmId))
        {
            throw new InvalidOperationException(
                "A metaheuristic algorithm identifier is required.");
        }

        if (!double.IsFinite(
                ObjectiveValue) ||
            !double.IsFinite(
                TotalConstraintViolation) ||
            TotalConstraintViolation < 0.0)
        {
            throw new InvalidOperationException(
                "Metaheuristic objective and violation values must be finite and valid.");
        }

        if (RawSearchPoint.Length == 0 ||
            ModelVariableValues.Length == 0)
        {
            throw new InvalidOperationException(
                "Metaheuristic result vectors cannot be empty.");
        }

        if (Evaluations <= 0 ||
            Iterations < 0 ||
            Duration < TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Metaheuristic execution statistics are invalid.");
        }
    }
}
