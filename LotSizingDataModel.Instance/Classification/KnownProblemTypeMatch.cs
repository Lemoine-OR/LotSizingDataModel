using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Represents a detected correspondence between a lot-sizing
/// instance and a known problem family.
/// </summary>
/// <remarks>
/// A complete problem classification may contain several
/// matches.
///
/// Each match identifies:
/// <list type="bullet">
/// <item>
/// <description>the recognized problem family;</description>
/// </item>
/// <item>
/// <description>the nature of the correspondence;</description>
/// </item>
/// <item>
/// <description>the part of the instance concerned;</description>
/// </item>
/// <item>
/// <description>the evidence supporting the match;</description>
/// </item>
/// <item>
/// <description>the additional features not covered by the family.</description>
/// </item>
/// </list>
/// </remarks>
[Serializable]
[XmlType(TypeName = "knownProblemTypeMatch")]
public sealed class KnownProblemTypeMatch : ModelObject
{
    private string _problemTypeCode =
        string.Empty;

    private string _problemTypeName =
        string.Empty;

    private string _definitionVersion =
        string.Empty;

    private ProblemMatchKind _matchKind =
        ProblemMatchKind.Unknown;

    private ProblemClassificationScope _scope =
        ProblemClassificationScope.Unknown;

    private double _score;

    private string _scopeDescription =
        string.Empty;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty known-problem-type match.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public KnownProblemTypeMatch()
    {
    }

    /// <summary>
    /// Initializes a known-problem-type match.
    /// </summary>
    /// <param name="problemTypeCode">
    /// Stable code identifying the known problem family.
    /// </param>
    /// <param name="problemTypeName">
    /// Human-readable name of the known problem family.
    /// </param>
    /// <param name="matchKind">
    /// Nature of the correspondence with the analyzed
    /// instance.
    /// </param>
    /// <param name="scope">
    /// Part of the analyzed instance to which the match
    /// applies.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="problemTypeCode"/> is null,
    /// empty or composed only of white-space characters.
    /// </exception>
    public KnownProblemTypeMatch(
        string problemTypeCode,
        string problemTypeName,
        ProblemMatchKind matchKind,
        ProblemClassificationScope scope)
    {
        if (string.IsNullOrWhiteSpace(
                problemTypeCode))
        {
            throw new ArgumentException(
                "A known problem-type code is required.",
                nameof(problemTypeCode));
        }

        ProblemTypeCode =
            problemTypeCode;

        ProblemTypeName =
            problemTypeName;

        MatchKind =
            matchKind;

        Scope =
            scope;
    }

    /// <summary>
    /// Gets or sets the stable code identifying the known
    /// lot-sizing problem family.
    /// </summary>
    /// <remarks>
    /// Examples include <c>LS-U</c>, <c>LS-C</c>,
    /// <c>CLSP</c> and <c>MLCLSP</c>.
    ///
    /// The code must be interpreted using the corresponding
    /// catalog definition and its version.
    /// </remarks>
    [XmlAttribute("problemTypeCode")]
    public string ProblemTypeCode
    {
        get => _problemTypeCode;
        set
        {
            if (SetProperty(
                    ref _problemTypeCode,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasProblemTypeCode));
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable name of the known
    /// lot-sizing problem family.
    /// </summary>
    [XmlAttribute("problemTypeName")]
    public string ProblemTypeName
    {
        get => _problemTypeName;
        set => SetProperty(
            ref _problemTypeName,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the version of the catalog definition
    /// used to produce this match.
    /// </summary>
    /// <remarks>
    /// Recording the definition version makes the
    /// classification reproducible when classification rules
    /// evolve.
    /// </remarks>
    [XmlAttribute("definitionVersion")]
    public string DefinitionVersion
    {
        get => _definitionVersion;
        set
        {
            if (SetProperty(
                    ref _definitionVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasDefinitionVersion));
            }
        }
    }

    /// <summary>
    /// Gets or sets the nature of the correspondence between
    /// the instance and the known problem family.
    /// </summary>
    [XmlAttribute("matchKind")]
    public ProblemMatchKind MatchKind
    {
        get => _matchKind;
        set
        {
            if (SetProperty(
                    ref _matchKind,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the part of the instance to which this
    /// problem-family match applies.
    /// </summary>
    [XmlAttribute("scope")]
    public ProblemClassificationScope Scope
    {
        get => _scope;
        set
        {
            if (SetProperty(
                    ref _scope,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the normalized score assigned to this
    /// match.
    /// </summary>
    /// <remarks>
    /// The score must be between zero and one:
    /// <list type="bullet">
    /// <item>
    /// <description>zero indicates no supporting similarity;</description>
    /// </item>
    /// <item>
    /// <description>one indicates complete weighted satisfaction.</description>
    /// </item>
    /// </list>
    ///
    /// The score does not override required conditions. A
    /// match containing a blocking mismatch cannot be treated
    /// as exact solely because it has a high score.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied value is not finite or is
    /// outside the interval from zero to one.
    /// </exception>
    [XmlAttribute("score")]
    public double Score
    {
        get => _score;
        set
        {
            if (!double.IsFinite(value) ||
                value < 0.0 ||
                value > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The match score must be finite and " +
                    "between zero and one.");
            }

            SetProperty(
                ref _score,
                value);
        }
    }

    /// <summary>
    /// Gets or sets a human-readable description of the
    /// classification scope.
    /// </summary>
    /// <remarks>
    /// This property supplements <see cref="Scope"/> when the
    /// affected subset needs further explanation.
    ///
    /// Examples include:
    /// <c>Per-item relaxation</c> and
    /// <c>Production subsystem of plant 2</c>.
    /// </remarks>
    [XmlElement("scopeDescription")]
    public string ScopeDescription
    {
        get => _scopeDescription;
        set
        {
            if (SetProperty(
                    ref _scopeDescription,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasScopeDescription));
            }
        }
    }

    /// <summary>
    /// Gets the stable keys of the entities affected by this
    /// classification match.
    /// </summary>
    /// <remarks>
    /// Keys should use an explicit and stable format.
    ///
    /// Examples include:
    /// <c>item:12</c>,
    /// <c>plant:3</c>,
    /// <c>workCenter:3/7</c> and
    /// <c>transportResource:2</c>.
    ///
    /// An empty collection normally means that the scope
    /// applies globally or that no more precise entity set
    /// has been recorded.
    /// </remarks>
    [XmlArray("affectedEntityKeys")]
    [XmlArrayItem("entityKey")]
    public List<string> AffectedEntityKeys { get; } =
        new();

    /// <summary>
    /// Gets the codes of additional instance features not
    /// covered by the recognized classical family.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// <c>transportation</c>,
    /// <c>financialConstraints</c>,
    /// <c>additionalCapacity</c> and
    /// <c>multiSite</c>.
    /// </remarks>
    [XmlArray("additionalFeatureCodes")]
    [XmlArrayItem("featureCode")]
    public List<string> AdditionalFeatureCodes { get; } =
        new();

    /// <summary>
    /// Gets the factual evidence supporting or contradicting
    /// this problem-family match.
    /// </summary>
    [XmlArray("evidence")]
    [XmlArrayItem("classificationEvidence")]
    public List<ClassificationEvidence> Evidence { get; } =
        new();

    /// <summary>
    /// Gets or sets an optional explanatory comment about
    /// the complete match.
    /// </summary>
    [XmlElement("comment")]
    public string Comment
    {
        get => _comment;
        set
        {
            if (SetProperty(
                    ref _comment,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasComment));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether a problem-family code
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasProblemTypeCode =>
        !string.IsNullOrWhiteSpace(
            ProblemTypeCode);

    /// <summary>
    /// Gets a value indicating whether a catalog-definition
    /// version has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasDefinitionVersion =>
        !string.IsNullOrWhiteSpace(
            DefinitionVersion);

    /// <summary>
    /// Gets a value indicating whether the classification
    /// scope contains a human-readable description.
    /// </summary>
    [XmlIgnore]
    public bool HasScopeDescription =>
        !string.IsNullOrWhiteSpace(
            ScopeDescription);

    /// <summary>
    /// Gets a value indicating whether the match contains an
    /// explanatory comment.
    /// </summary>
    [XmlIgnore]
    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    /// <summary>
    /// Gets a value indicating whether at least one affected
    /// entity key has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasAffectedEntities =>
        AffectedEntityKeys.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the instance contains
    /// additional features not covered by the recognized
    /// problem-family definition.
    /// </summary>
    [XmlIgnore]
    public bool HasAdditionalFeatures =>
        AdditionalFeatureCodes.Count > 0;

    /// <summary>
    /// Gets the total number of recorded evidence objects.
    /// </summary>
    [XmlIgnore]
    public int EvidenceCount =>
        Evidence.Count;

    /// <summary>
    /// Gets the number of satisfied evidence conditions.
    /// </summary>
    [XmlIgnore]
    public int SatisfiedEvidenceCount =>
        Evidence.Count(
            evidence =>
                evidence.IsSatisfied);

    /// <summary>
    /// Gets the number of required evidence conditions.
    /// </summary>
    [XmlIgnore]
    public int RequiredEvidenceCount =>
        Evidence.Count(
            evidence =>
                evidence.IsRequired);

    /// <summary>
    /// Gets the number of unsatisfied required conditions.
    /// </summary>
    [XmlIgnore]
    public int BlockingMismatchCount =>
        Evidence.Count(
            evidence =>
                evidence.IsBlockingMismatch);

    /// <summary>
    /// Gets the number of unsatisfied optional conditions.
    /// </summary>
    [XmlIgnore]
    public int OptionalMismatchCount =>
        Evidence.Count(
            evidence =>
                evidence.IsOptionalMismatch);

    /// <summary>
    /// Gets a value indicating whether at least one required
    /// classification condition is not satisfied.
    /// </summary>
    [XmlIgnore]
    public bool HasBlockingMismatches =>
        BlockingMismatchCount > 0;

    /// <summary>
    /// Gets a value indicating whether this match describes
    /// the complete problem directly.
    /// </summary>
    [XmlIgnore]
    public bool AppliesToCompleteProblem =>
        Scope ==
        ProblemClassificationScope.CompleteProblem;

    /// <summary>
    /// Gets a value indicating whether the match is declared
    /// as exact.
    /// </summary>
    [XmlIgnore]
    public bool IsExactMatch =>
        MatchKind ==
        ProblemMatchKind.Exact;

    /// <summary>
    /// Gets a value indicating whether the match describes
    /// an identified extension of a known family.
    /// </summary>
    [XmlIgnore]
    public bool IsKnownExtension =>
        MatchKind ==
        ProblemMatchKind.KnownExtension;

    /// <summary>
    /// Gets a value indicating whether the match can provide
    /// direct structural support for method selection.
    /// </summary>
    /// <remarks>
    /// Exact matches and known extensions are considered
    /// direct matches only when no required condition is
    /// violated.
    /// </remarks>
    [XmlIgnore]
    public bool IsDirectMatch =>
        !HasBlockingMismatches &&
        (
            MatchKind ==
                ProblemMatchKind.Exact ||
            MatchKind ==
                ProblemMatchKind.KnownExtension
        );

    /// <summary>
    /// Adds one evidence object to this match.
    /// </summary>
    /// <param name="evidence">
    /// Evidence object to add.
    /// </param>
    public void AddEvidence(
        ClassificationEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(
            evidence);

        Evidence.Add(
            evidence);

        NotifyEvidenceProperties();
    }

    /// <summary>
    /// Replaces all evidence objects associated with this
    /// match.
    /// </summary>
    /// <param name="evidence">
    /// New evidence collection.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the supplied collection contains a null
    /// element.
    /// </exception>
    public void ReplaceEvidence(
        IEnumerable<ClassificationEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(
            evidence);

        ClassificationEvidence[] normalizedEvidence =
            evidence.ToArray();

        if (normalizedEvidence.Any(
                item =>
                    item is null))
        {
            throw new ArgumentException(
                "The evidence collection cannot contain " +
                "a null element.",
                nameof(evidence));
        }

        Evidence.Clear();

        Evidence.AddRange(
            normalizedEvidence);

        NotifyEvidenceProperties();
    }

    /// <summary>
    /// Removes every evidence object associated with this
    /// match.
    /// </summary>
    public void ClearEvidence()
    {
        if (Evidence.Count == 0)
        {
            return;
        }

        Evidence.Clear();

        NotifyEvidenceProperties();
    }

    /// <summary>
    /// Replaces the stable keys of the entities affected by
    /// this match.
    /// </summary>
    /// <param name="entityKeys">
    /// Stable entity keys.
    /// </param>
    public void ReplaceAffectedEntityKeys(
        IEnumerable<string> entityKeys)
    {
        ReplaceNormalizedCodes(
            AffectedEntityKeys,
            entityKeys,
            nameof(entityKeys));

        OnPropertyChanged(
            nameof(AffectedEntityKeys));

        OnPropertyChanged(
            nameof(HasAffectedEntities));
    }

    /// <summary>
    /// Replaces the codes of the additional features not
    /// covered by the recognized problem family.
    /// </summary>
    /// <param name="featureCodes">
    /// Additional feature codes.
    /// </param>
    public void ReplaceAdditionalFeatureCodes(
        IEnumerable<string> featureCodes)
    {
        ReplaceNormalizedCodes(
            AdditionalFeatureCodes,
            featureCodes,
            nameof(featureCodes));

        OnPropertyChanged(
            nameof(AdditionalFeatureCodes));

        OnPropertyChanged(
            nameof(HasAdditionalFeatures));
    }

    /// <summary>
    /// Calculates the weighted proportion of satisfied
    /// evidence conditions.
    /// </summary>
    /// <returns>
    /// A value between zero and one.
    /// </returns>
    /// <remarks>
    /// Evidence objects having a weight of zero are ignored.
    ///
    /// A value of zero is returned when no positively weighted
    /// evidence exists.
    /// </remarks>
    public double CalculateWeightedEvidenceScore()
    {
        double totalWeight =
            Evidence
                .Where(
                    evidence =>
                        evidence.Weight > 0.0)
                .Sum(
                    evidence =>
                        evidence.Weight);

        if (totalWeight <= 0.0)
        {
            return 0.0;
        }

        double satisfiedWeight =
            Evidence
                .Where(
                    evidence =>
                        evidence.IsSatisfied &&
                        evidence.Weight > 0.0)
                .Sum(
                    evidence =>
                        evidence.Weight);

        return satisfiedWeight /
               totalWeight;
    }

    /// <summary>
    /// Recalculates <see cref="Score"/> from the current
    /// weighted evidence conditions.
    /// </summary>
    public void UpdateScoreFromEvidence()
    {
        Score =
            CalculateWeightedEvidenceScore();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string name =
            string.IsNullOrWhiteSpace(
                ProblemTypeName)
                ? ProblemTypeCode
                : ProblemTypeName;

        return
            $"{ProblemTypeCode} — {name}; " +
            $"{MatchKind}; {Scope}; " +
            $"score {Score:0.###}";
    }

    private static void ReplaceNormalizedCodes(
        ICollection<string> destination,
        IEnumerable<string> source,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        string[] normalizedCodes =
            source
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(
                            value))
                .Select(
                    value =>
                        value.Trim())
                .Distinct(
                    StringComparer.Ordinal)
                .OrderBy(
                    value =>
                        value,
                    StringComparer.Ordinal)
                .ToArray();

        destination.Clear();

        foreach (string normalizedCode
                 in normalizedCodes)
        {
            destination.Add(
                normalizedCode);
        }
    }

    private void NotifyEvidenceProperties()
    {
        OnPropertyChanged(
            nameof(Evidence));

        OnPropertyChanged(
            nameof(EvidenceCount));

        OnPropertyChanged(
            nameof(SatisfiedEvidenceCount));

        OnPropertyChanged(
            nameof(RequiredEvidenceCount));

        OnPropertyChanged(
            nameof(BlockingMismatchCount));

        OnPropertyChanged(
            nameof(OptionalMismatchCount));

        OnPropertyChanged(
            nameof(HasBlockingMismatches));

        OnPropertyChanged(
            nameof(IsDirectMatch));
    }

    private void NotifyDerivedProperties()
    {
        OnPropertyChanged(
            nameof(AppliesToCompleteProblem));

        OnPropertyChanged(
            nameof(IsExactMatch));

        OnPropertyChanged(
            nameof(IsKnownExtension));

        OnPropertyChanged(
            nameof(IsDirectMatch));
    }
}