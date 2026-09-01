namespace LotSizingDataModel.Instance.Benchmark;

/// <summary>
/// Reproducibility metadata for one benchmark run.
/// </summary>
public sealed class BenchmarkRunProvenance
{
    public BenchmarkRunProvenance(
        string formulationId,
        string methodId,
        string methodVersion,
        string backendId,
        string backendVersion,
        bool isStochastic,
        ulong? seed,
        IReadOnlyDictionary<string, string>? parameters = null)
    {
        FormulationId =
            NormalizeRequired(
                formulationId,
                nameof(formulationId));

        MethodId =
            NormalizeRequired(
                methodId,
                nameof(methodId));

        MethodVersion =
            NormalizeRequired(
                methodVersion,
                nameof(methodVersion));

        BackendId =
            NormalizeRequired(
                backendId,
                nameof(backendId));

        BackendVersion =
            NormalizeRequired(
                backendVersion,
                nameof(backendVersion));

        IsStochastic =
            isStochastic;

        Seed =
            seed;

        var normalizedParameters =
            new SortedDictionary<string, string>(
                StringComparer.Ordinal);

        if (parameters is not null)
        {
            foreach (KeyValuePair<string, string> pair
                     in parameters)
            {
                string key =
                    NormalizeRequired(
                        pair.Key,
                        nameof(parameters));

                normalizedParameters.Add(
                    key,
                    pair.Value?.Trim() ??
                    string.Empty);
            }
        }

        Parameters =
            normalizedParameters;

        EnsureValid();
    }

    public string FormulationId
    {
        get;
    }

    public string MethodId
    {
        get;
    }

    public string MethodVersion
    {
        get;
    }

    public string BackendId
    {
        get;
    }

    public string BackendVersion
    {
        get;
    }

    public bool IsStochastic
    {
        get;
    }

    public ulong? Seed
    {
        get;
    }

    public IReadOnlyDictionary<string, string> Parameters
    {
        get;
    }

    public void EnsureValid()
    {
        if (IsStochastic &&
            !Seed.HasValue)
        {
            throw new InvalidOperationException(
                "A stochastic benchmark run requires an explicit seed.");
        }
    }

    private static string NormalizeRequired(
        string? value,
        string parameterName)
    {
        string normalized =
            value?.Trim() ??
            string.Empty;

        if (normalized.Length == 0)
        {
            throw new ArgumentException(
                "A non-empty benchmark provenance value is required.",
                parameterName);
        }

        return normalized;
    }
}
