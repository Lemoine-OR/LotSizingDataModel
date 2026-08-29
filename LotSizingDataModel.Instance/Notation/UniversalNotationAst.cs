using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors.Temporal;

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

    private readonly IReadOnlyCollection<UniversalTemporalQualifier>
        _temporalQualifiers;

    private readonly IReadOnlyCollection<UniversalSemanticCondition>
        _semanticConditions;

    public UniversalNotationBeta(
        IEnumerable<UniversalNotationFeature>? features = null,
        IEnumerable<UniversalTemporalQualifier>? temporalQualifiers = null,
        IEnumerable<UniversalSemanticCondition>? semanticConditions = null)
    {
        _features =
            (features ?? Array.Empty<UniversalNotationFeature>())
                .Distinct()
                .OrderBy(feature => (int)feature)
                .ToArray();

        UniversalTemporalQualifier[] qualifierArray =
            (temporalQualifiers ??
             Array.Empty<UniversalTemporalQualifier>())
                .ToArray();

        var canonicalQualifiers =
            new List<UniversalTemporalQualifier>();

        foreach (
            IGrouping<
                UniversalTemporalParameter,
                UniversalTemporalQualifier> group
            in qualifierArray.GroupBy(
                qualifier => qualifier.Parameter))
        {
            TemporalPatternType[] patterns =
                group
                    .Select(qualifier => qualifier.Pattern)
                    .Distinct()
                    .ToArray();

            if (patterns.Length > 1)
            {
                throw new ArgumentException(
                    "A beta field cannot contain conflicting temporal " +
                    $"patterns for parameter '{group.Key}'.",
                    nameof(temporalQualifiers));
            }

            canonicalQualifiers.Add(
                new UniversalTemporalQualifier(
                    group.Key,
                    patterns[0]));
        }

        _temporalQualifiers =
            canonicalQualifiers
                .OrderBy(
                    qualifier =>
                        (int)qualifier.Parameter)
                .ToArray();

        _semanticConditions =
            (semanticConditions ??
             Array.Empty<UniversalSemanticCondition>())
                .Distinct()
                .OrderBy(condition => (int)condition)
                .ToArray();
    }

    public IReadOnlyCollection<UniversalNotationFeature> Features =>
        _features;

    public IReadOnlyCollection<UniversalTemporalQualifier>
        TemporalQualifiers =>
            _temporalQualifiers;

    public IReadOnlyCollection<UniversalSemanticCondition>
        SemanticConditions =>
            _semanticConditions;

    public bool Contains(UniversalNotationFeature feature) =>
        _features.Contains(feature);

    public bool ContainsSemanticCondition(
        UniversalSemanticCondition condition) =>
            _semanticConditions.Contains(condition);

    public bool TryGetTemporalQualifier(
        UniversalTemporalParameter parameter,
        out UniversalTemporalQualifier? qualifier)
    {
        qualifier =
            _temporalQualifiers.FirstOrDefault(
                candidate =>
                    candidate.Parameter == parameter);

        return qualifier is not null;
    }
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
