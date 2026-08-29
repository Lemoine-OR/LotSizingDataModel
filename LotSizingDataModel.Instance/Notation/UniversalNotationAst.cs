using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Alpha field: structural/environment characteristics.
/// </summary>
public sealed class UniversalNotationAlpha
{
    public UniversalItemCardinality ItemCardinality { get; init; } =
        UniversalItemCardinality.Unknown;

    public UniversalProblemLevel ProblemLevel { get; init; } =
        UniversalProblemLevel.Unknown;

    public ProductStructureType ProductStructureType { get; init; } =
        ProductStructureType.Unknown;

    public UniversalNetworkNotation Network { get; init; } =
        new();
}

/// <summary>
/// Beta field: constraints and modeled extensions.
/// </summary>
public sealed class UniversalNotationBeta
{
    private readonly IReadOnlyCollection<UniversalNotationFeature> _features;

    public UniversalNotationBeta(
        IEnumerable<UniversalNotationFeature>? features = null)
    {
        _features =
            (features ?? Array.Empty<UniversalNotationFeature>())
                .Distinct()
                .OrderBy(feature => (int)feature)
                .ToArray();
    }

    public IReadOnlyCollection<UniversalNotationFeature> Features =>
        _features;

    public bool Contains(UniversalNotationFeature feature) =>
        _features.Contains(feature);
}

/// <summary>
/// Gamma field: objective family.
/// </summary>
public sealed class UniversalNotationGamma
{
    public UniversalObjectiveKind Objective { get; init; } =
        UniversalObjectiveKind.Unknown;
}

/// <summary>
/// Typed abstract syntax tree for the versioned alpha | beta | gamma scheme.
/// </summary>
public sealed class UniversalLotSizingNotation
{
    public UniversalLotSizingNotation(
        UniversalNotationAlpha alpha,
        UniversalNotationBeta beta,
        UniversalNotationGamma gamma,
        string? schemeVersion = null)
    {
        Alpha =
            alpha ??
            throw new ArgumentNullException(nameof(alpha));

        Beta =
            beta ??
            throw new ArgumentNullException(nameof(beta));

        Gamma =
            gamma ??
            throw new ArgumentNullException(nameof(gamma));

        SchemeVersion =
            schemeVersion ??
            UniversalNotationScheme.CurrentVersion;

        if (!UniversalNotationScheme.IsSupported(SchemeVersion))
        {
            throw new NotSupportedException(
                $"Universal notation scheme version '{SchemeVersion}' " +
                "is not supported.");
        }
    }

    public string SchemeVersion { get; }
    public UniversalNotationAlpha Alpha { get; }
    public UniversalNotationBeta Beta { get; }
    public UniversalNotationGamma Gamma { get; }

    public string Render() =>
        UniversalNotationRenderer.Render(this);

    public override string ToString() =>
        Render();
}
