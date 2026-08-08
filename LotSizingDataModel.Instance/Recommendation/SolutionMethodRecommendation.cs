using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Solution.Common;

namespace LotSizingDataModel.Instance.Recommendation;

/// <summary>
/// Represents the evaluated compatibility of one solution
/// method with a specific lot-sizing problem instance.
/// </summary>
/// <remarks>
/// A recommendation records:
/// <list type="bullet">
/// <item>
/// <description>
/// the evaluated solution method;
/// </description>
/// </item>
/// <item>
/// <description>
/// the compatibility level and numerical score;
/// </description>
/// </item>
/// <item>
/// <description>
/// the scope to which the method can be applied;
/// </description>
/// </item>
/// <item>
/// <description>
/// evidence supporting or limiting compatibility;
/// </description>
/// </item>
/// <item>
/// <description>
/// adaptations required before the method can be used;
/// </description>
/// </item>
/// <item>
/// <description>
/// the catalog, advisor and supply-chain versions used during
/// evaluation.
/// </description>
/// </item>
/// </list>
///
/// The recommendation describes technical compatibility. It
/// does not guarantee that the method will outperform other
/// compatible methods.
/// </remarks>
[Serializable]
[XmlType(TypeName = "solutionMethodRecommendation")]
public sealed class SolutionMethodRecommendation : ModelObject
{
    private string _methodCode =
        string.Empty;

    private string _methodName =
        string.Empty;

    private string _methodVersion =
        string.Empty;

    private SolutionMethodKind _methodKind =
        default;

    private MethodCompatibilityLevel _compatibilityLevel =
        MethodCompatibilityLevel.NotEvaluated;

    private ProblemClassificationScope _scope =
        ProblemClassificationScope.Unknown;

    private string _scopeDescription =
        string.Empty;

    private double _score;

    private int? _rank;

    private DateTime? _evaluatedAtUtc;

    private string _advisorVersion =
        string.Empty;

    private string _methodCatalogName =
        string.Empty;

    private string _methodCatalogVersion =
        string.Empty;

    private string _supplyChainFingerprint =
        string.Empty;

    private string _summary =
        string.Empty;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty solution-method recommendation.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public SolutionMethodRecommendation()
    {
    }

    /// <summary>
    /// Initializes a recommendation for a solution method.
    /// </summary>
    /// <param name="methodCode">
    /// Stable code identifying the method.
    /// </param>
    /// <param name="methodName">
    /// Human-readable method name.
    /// </param>
    /// <param name="methodKind">
    /// General category of the solution method.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="methodCode"/> or
    /// <paramref name="methodName"/> is empty.
    /// </exception>
    public SolutionMethodRecommendation(
        string methodCode,
        string methodName,
        SolutionMethodKind methodKind)
    {
        if (string.IsNullOrWhiteSpace(methodCode))
        {
            throw new ArgumentException(
                "A solution-method code is required.",
                nameof(methodCode));
        }

        if (string.IsNullOrWhiteSpace(methodName))
        {
            throw new ArgumentException(
                "A solution-method name is required.",
                nameof(methodName));
        }

        MethodCode =
            methodCode;

        MethodName =
            methodName;

        MethodKind =
            methodKind;
    }

    /// <summary>
    /// Initializes a recommendation from a method definition.
    /// </summary>
    /// <param name="methodDefinition">
    /// Method definition to evaluate.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="methodDefinition"/> is
    /// <see langword="null"/>.
    /// </exception>
    public SolutionMethodRecommendation(
        SolutionMethodDefinition methodDefinition)
    {
        ArgumentNullException.ThrowIfNull(
            methodDefinition);

        MethodCode =
            methodDefinition.MethodCode;

        MethodName =
            methodDefinition.Name;

        MethodVersion =
            methodDefinition.MethodVersion;

        MethodKind =
            methodDefinition.MethodKind;
    }

    /// <summary>
    /// Gets or sets the stable code identifying the evaluated
    /// solution method.
    /// </summary>
    [XmlAttribute("methodCode")]
    public string MethodCode
    {
        get => _methodCode;
        set
        {
            if (SetProperty(
                    ref _methodCode,
                    NormalizeCode(value)))
            {
                OnPropertyChanged(
                    nameof(HasMethodCode));

                OnPropertyChanged(
                    nameof(IsValidRecommendation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable name of the evaluated
    /// solution method.
    /// </summary>
    [XmlAttribute("methodName")]
    public string MethodName
    {
        get => _methodName;
        set
        {
            if (SetProperty(
                    ref _methodName,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasMethodName));

                OnPropertyChanged(
                    nameof(IsValidRecommendation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the evaluated method
    /// definition.
    /// </summary>
    [XmlAttribute("methodVersion")]
    public string MethodVersion
    {
        get => _methodVersion;
        set
        {
            if (SetProperty(
                    ref _methodVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasMethodVersion));
            }
        }
    }

    /// <summary>
    /// Gets or sets the general category of the evaluated
    /// solution method.
    /// </summary>
    [XmlAttribute("methodKind")]
    public SolutionMethodKind MethodKind
    {
        get => _methodKind;
        set => SetProperty(
            ref _methodKind,
            value);
    }

    /// <summary>
    /// Gets or sets the evaluated compatibility level.
    /// </summary>
    [XmlAttribute("compatibilityLevel")]
    public MethodCompatibilityLevel CompatibilityLevel
    {
        get => _compatibilityLevel;
        set
        {
            if (SetProperty(
                    ref _compatibilityLevel,
                    value))
            {
                NotifyCompatibilityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the problem scope to which the method can
    /// be applied.
    /// </summary>
    /// <remarks>
    /// A method may be applicable to the complete problem, a
    /// relaxation, an item subset or another subproblem.
    /// </remarks>
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
                NotifyScopeProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a human-readable explanation of the
    /// applicable problem scope.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>Complete problem</c>;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>Single-item subproblems after capacity
    /// relaxation</c>;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>Production-planning subproblem without
    /// transportation</c>.
    /// </description>
    /// </item>
    /// </list>
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
    /// Gets or sets the normalized compatibility score.
    /// </summary>
    /// <remarks>
    /// The value must lie between zero and one:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// zero indicates that none of the weighted criteria are
    /// satisfied;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// one indicates that all weighted criteria are
    /// satisfied.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Blocking evidence remains blocking independently of
    /// this numerical score.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied score is not finite or does
    /// not lie between zero and one.
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
                    "The compatibility score must be finite " +
                    "and lie between zero and one.");
            }

            if (SetProperty(
                    ref _score,
                    value))
            {
                OnPropertyChanged(
                    nameof(IsValidRecommendation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the rank assigned to the recommendation.
    /// </summary>
    /// <remarks>
    /// A null value means that the recommendation has not yet
    /// been ranked against other methods.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied rank is not strictly positive.
    /// </exception>
    [XmlElement("rank", IsNullable = true)]
    public int? Rank
    {
        get => _rank;
        set
        {
            if (value.HasValue &&
                value.Value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "A recommendation rank must be strictly " +
                    "positive.");
            }

            if (SetProperty(
                    ref _rank,
                    value))
            {
                OnPropertyChanged(
                    nameof(HasRank));
            }
        }
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the
    /// recommendation was evaluated.
    /// </summary>
    [XmlElement("evaluatedAtUtc", IsNullable = true)]
    public DateTime? EvaluatedAtUtc
    {
        get => _evaluatedAtUtc;
        set
        {
            DateTime? normalizedValue =
                value.HasValue
                    ? ConvertToUtc(value.Value)
                    : null;

            if (SetProperty(
                    ref _evaluatedAtUtc,
                    normalizedValue))
            {
                OnPropertyChanged(
                    nameof(HasEvaluationDate));
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the method advisor used to
    /// produce the recommendation.
    /// </summary>
    [XmlAttribute("advisorVersion")]
    public string AdvisorVersion
    {
        get => _advisorVersion;
        set
        {
            if (SetProperty(
                    ref _advisorVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasAdvisorVersion));
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the method catalog used during
    /// evaluation.
    /// </summary>
    [XmlAttribute("methodCatalogName")]
    public string MethodCatalogName
    {
        get => _methodCatalogName;
        set
        {
            if (SetProperty(
                    ref _methodCatalogName,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasMethodCatalogInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the method catalog used
    /// during evaluation.
    /// </summary>
    [XmlAttribute("methodCatalogVersion")]
    public string MethodCatalogVersion
    {
        get => _methodCatalogVersion;
        set
        {
            if (SetProperty(
                    ref _methodCatalogVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasMethodCatalogInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the fingerprint of the supply-chain data
    /// evaluated by the method advisor.
    /// </summary>
    [XmlAttribute("supplyChainFingerprint")]
    public string SupplyChainFingerprint
    {
        get => _supplyChainFingerprint;
        set
        {
            if (SetProperty(
                    ref _supplyChainFingerprint,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasSupplyChainFingerprint));
            }
        }
    }

    /// <summary>
    /// Gets or sets a concise human-readable summary of the
    /// recommendation.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// <c>Recommended exact method for this MLLP
    /// instance.</c>;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>Applicable only after relaxing production
    /// capacity.</c>;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// <c>Incompatible because transportation decisions are
    /// unsupported.</c>.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [XmlElement("summary")]
    public string Summary
    {
        get => _summary;
        set
        {
            if (SetProperty(
                    ref _summary,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasSummary));
            }
        }
    }

    /// <summary>
    /// Gets the evidence used to evaluate the compatibility
    /// of the method.
    /// </summary>
    [XmlArray("evidence")]
    [XmlArrayItem("criterion")]
    public List<MethodCompatibilityEvidence> Evidence
    {
        get;
    } = new();

    /// <summary>
    /// Gets the adaptations required before the method can be
    /// applied.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// relaxing shared capacity constraints;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// decomposing the instance by item;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// extending the method to represent backlogging;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// embedding the method inside a matheuristic.
    /// </description>
    /// </item>
    /// </list>
    /// </remarks>
    [XmlArray("requiredAdaptations")]
    [XmlArrayItem("adaptation")]
    public List<string> RequiredAdaptations { get; } =
        new();

    /// <summary>
    /// Gets the non-fatal warnings produced during method
    /// evaluation.
    /// </summary>
    [XmlArray("warnings")]
    [XmlArrayItem("warning")]
    public List<string> Warnings { get; } =
        new();

    /// <summary>
    /// Gets or sets an optional explanatory comment.
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
    /// Gets a value indicating whether a stable method code
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasMethodCode =>
        !string.IsNullOrWhiteSpace(
            MethodCode);

    /// <summary>
    /// Gets a value indicating whether a human-readable method
    /// name has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasMethodName =>
        !string.IsNullOrWhiteSpace(
            MethodName);

    /// <summary>
    /// Gets a value indicating whether a method version has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasMethodVersion =>
        !string.IsNullOrWhiteSpace(
            MethodVersion);

    /// <summary>
    /// Gets a value indicating whether the method has been
    /// evaluated.
    /// </summary>
    [XmlIgnore]
    public bool HasBeenEvaluated =>
        CompatibilityLevel !=
        MethodCompatibilityLevel.NotEvaluated;

    /// <summary>
    /// Gets a value indicating whether an applicable problem
    /// scope has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasScope =>
        Scope !=
        ProblemClassificationScope.Unknown;

    /// <summary>
    /// Gets a value indicating whether the recommendation
    /// applies to the complete problem.
    /// </summary>
    [XmlIgnore]
    public bool AppliesToCompleteProblem =>
        Scope ==
        ProblemClassificationScope.CompleteProblem;

    /// <summary>
    /// Gets a value indicating whether a human-readable scope
    /// description has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasScopeDescription =>
        !string.IsNullOrWhiteSpace(
            ScopeDescription);

    /// <summary>
    /// Gets a value indicating whether the recommendation has
    /// been ranked.
    /// </summary>
    [XmlIgnore]
    public bool HasRank =>
        Rank.HasValue;

    /// <summary>
    /// Gets a value indicating whether an evaluation date has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasEvaluationDate =>
        EvaluatedAtUtc.HasValue;

    /// <summary>
    /// Gets a value indicating whether an advisor version has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasAdvisorVersion =>
        !string.IsNullOrWhiteSpace(
            AdvisorVersion);

    /// <summary>
    /// Gets a value indicating whether method-catalog
    /// information has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasMethodCatalogInformation =>
        !string.IsNullOrWhiteSpace(
            MethodCatalogName) &&
        !string.IsNullOrWhiteSpace(
            MethodCatalogVersion);

    /// <summary>
    /// Gets a value indicating whether a supply-chain
    /// fingerprint has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSupplyChainFingerprint =>
        !string.IsNullOrWhiteSpace(
            SupplyChainFingerprint);

    /// <summary>
    /// Gets a value indicating whether a human-readable
    /// summary has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSummary =>
        !string.IsNullOrWhiteSpace(
            Summary);

    /// <summary>
    /// Gets a value indicating whether compatibility evidence
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasEvidence =>
        Evidence.Count > 0;

    /// <summary>
    /// Gets the number of compatibility-evidence records.
    /// </summary>
    [XmlIgnore]
    public int EvidenceCount =>
        Evidence.Count;

    /// <summary>
    /// Gets the number of satisfied compatibility criteria.
    /// </summary>
    [XmlIgnore]
    public int SatisfiedEvidenceCount =>
        Evidence.Count(
            criterion =>
                criterion is not null &&
                criterion.IsSatisfied);

    /// <summary>
    /// Gets the number of unsatisfied compatibility criteria.
    /// </summary>
    [XmlIgnore]
    public int MismatchCount =>
        Evidence.Count(
            criterion =>
                criterion is not null &&
                criterion.IsMismatch);

    /// <summary>
    /// Gets the number of blocking incompatibilities.
    /// </summary>
    [XmlIgnore]
    public int BlockingMismatchCount =>
        Evidence.Count(
            criterion =>
                criterion is not null &&
                criterion.IsBlockingMismatch);

    /// <summary>
    /// Gets the number of unsatisfied required criteria.
    /// </summary>
    [XmlIgnore]
    public int RequiredMismatchCount =>
        Evidence.Count(
            criterion =>
                criterion is not null &&
                criterion.IsRequiredMismatch);

    /// <summary>
    /// Gets the number of unsatisfied optional criteria.
    /// </summary>
    [XmlIgnore]
    public int OptionalMismatchCount =>
        Evidence.Count(
            criterion =>
                criterion is not null &&
                criterion.IsOptionalMismatch);

    /// <summary>
    /// Gets a value indicating whether at least one blocking
    /// incompatibility has been found.
    /// </summary>
    [XmlIgnore]
    public bool HasBlockingMismatches =>
        BlockingMismatchCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one required
    /// compatibility criterion is not satisfied.
    /// </summary>
    [XmlIgnore]
    public bool HasRequiredMismatches =>
        RequiredMismatchCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one optional
    /// criterion is not satisfied.
    /// </summary>
    [XmlIgnore]
    public bool HasOptionalMismatches =>
        OptionalMismatchCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one adaptation
    /// is required before applying the method.
    /// </summary>
    [XmlIgnore]
    public bool HasRequiredAdaptations =>
        RequiredAdaptations.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one warning
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasWarnings =>
        Warnings.Count > 0;

    /// <summary>
    /// Gets a value indicating whether an explanatory comment
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    /// <summary>
    /// Gets a value indicating whether the method is
    /// incompatible with the complete problem.
    /// </summary>
    [XmlIgnore]
    public bool IsIncompatible =>
        CompatibilityLevel ==
        MethodCompatibilityLevel.Incompatible;

    /// <summary>
    /// Gets a value indicating whether the method is only
    /// partially compatible.
    /// </summary>
    [XmlIgnore]
    public bool IsPartiallyCompatible =>
        CompatibilityLevel ==
        MethodCompatibilityLevel.PartiallyCompatible;

    /// <summary>
    /// Gets a value indicating whether the method is directly
    /// compatible.
    /// </summary>
    [XmlIgnore]
    public bool IsCompatible =>
        CompatibilityLevel ==
            MethodCompatibilityLevel.Compatible ||
        CompatibilityLevel ==
            MethodCompatibilityLevel.Recommended;

    /// <summary>
    /// Gets a value indicating whether the method is
    /// recommended.
    /// </summary>
    [XmlIgnore]
    public bool IsRecommended =>
        CompatibilityLevel ==
        MethodCompatibilityLevel.Recommended;

    /// <summary>
    /// Gets a value indicating whether the method may be
    /// applied directly to the complete problem.
    /// </summary>
    [XmlIgnore]
    public bool CanSolveCompleteProblemDirectly =>
        AppliesToCompleteProblem &&
        IsCompatible &&
        !HasBlockingMismatches &&
        !HasRequiredMismatches;

    /// <summary>
    /// Gets a value indicating whether decomposition,
    /// relaxation or another adaptation is necessary.
    /// </summary>
    [XmlIgnore]
    public bool RequiresAdaptation =>
        IsPartiallyCompatible ||
        HasRequiredAdaptations ||
        !AppliesToCompleteProblem;

    /// <summary>
    /// Gets a value indicating whether the recommendation
    /// contains the minimum coherent information required for
    /// presentation or ranking.
    /// </summary>
    [XmlIgnore]
    public bool IsValidRecommendation =>
        HasMethodCode &&
        HasMethodName &&
        double.IsFinite(Score) &&
        Score >= 0.0 &&
        Score <= 1.0 &&
        (
            !HasBeenEvaluated ||
            HasScope
        );

    /// <summary>
    /// Adds one compatibility-evidence record.
    /// </summary>
    /// <param name="evidence">
    /// Evidence to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="evidence"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the evidence is structurally invalid.
    /// </exception>
    public void AddEvidence(
        MethodCompatibilityEvidence evidence)
    {
        ArgumentNullException.ThrowIfNull(
            evidence);

        if (!evidence.IsValidEvidence)
        {
            throw new ArgumentException(
                "The compatibility evidence is invalid.",
                nameof(evidence));
        }

        Evidence.Add(
            evidence);

        NotifyEvidenceProperties();
    }

    /// <summary>
    /// Replaces all compatibility-evidence records.
    /// </summary>
    /// <param name="evidence">
    /// New evidence collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="evidence"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null or invalid
    /// evidence record.
    /// </exception>
    public void ReplaceEvidence(
        IEnumerable<MethodCompatibilityEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(
            evidence);

        var normalizedEvidence =
            new List<MethodCompatibilityEvidence>();

        foreach (MethodCompatibilityEvidence? criterion
                 in evidence)
        {
            if (criterion is null)
            {
                throw new ArgumentException(
                    "The evidence collection cannot contain " +
                    "a null element.",
                    nameof(evidence));
            }

            if (!criterion.IsValidEvidence)
            {
                throw new ArgumentException(
                    $"Compatibility evidence " +
                    $"'{criterion.CriterionCode}' is invalid.",
                    nameof(evidence));
            }

            normalizedEvidence.Add(
                criterion);
        }

        Evidence.Clear();

        Evidence.AddRange(
            normalizedEvidence);

        NotifyEvidenceProperties();
    }

    /// <summary>
    /// Removes all compatibility evidence.
    /// </summary>
    public void ClearEvidence()
    {
        Evidence.Clear();

        NotifyEvidenceProperties();
    }

    /// <summary>
    /// Replaces the adaptations required before applying the
    /// method.
    /// </summary>
    /// <param name="adaptations">
    /// New adaptation descriptions.
    /// </param>
    public void ReplaceRequiredAdaptations(
        IEnumerable<string> adaptations)
    {
        ReplaceNormalizedStrings(
            RequiredAdaptations,
            adaptations,
            nameof(adaptations));

        OnPropertyChanged(
            nameof(RequiredAdaptations));

        OnPropertyChanged(
            nameof(HasRequiredAdaptations));

        OnPropertyChanged(
            nameof(RequiresAdaptation));
    }

    /// <summary>
    /// Replaces the warnings associated with this
    /// recommendation.
    /// </summary>
    /// <param name="warnings">
    /// New warning messages.
    /// </param>
    public void ReplaceWarnings(
        IEnumerable<string> warnings)
    {
        ReplaceNormalizedStrings(
            Warnings,
            warnings,
            nameof(warnings));

        OnPropertyChanged(
            nameof(Warnings));

        OnPropertyChanged(
            nameof(HasWarnings));
    }

    /// <summary>
    /// Calculates a normalized score from the current
    /// compatibility evidence.
    /// </summary>
    /// <returns>
    /// Weighted proportion of satisfied criteria, between zero
    /// and one.
    /// </returns>
    /// <remarks>
    /// Evidence with a zero weight is retained for
    /// traceability but does not influence the score.
    ///
    /// Blocking mismatches are not given special numerical
    /// treatment by this method. They are handled when the
    /// compatibility level is determined.
    /// </remarks>
    public double CalculateScore()
    {
        MethodCompatibilityEvidence[] weightedEvidence =
            Evidence
                .Where(
                    criterion =>
                        criterion is not null &&
                        criterion.IsValidEvidence &&
                        criterion.Weight > 0.0)
                .ToArray();

        double totalWeight =
            weightedEvidence.Sum(
                criterion =>
                    criterion.Weight);

        if (totalWeight <= 0.0)
        {
            return 0.0;
        }

        double satisfiedWeight =
            weightedEvidence
                .Where(
                    criterion =>
                        criterion.IsSatisfied)
                .Sum(
                    criterion =>
                        criterion.Weight);

        double calculatedScore =
            satisfiedWeight /
            totalWeight;

        return Math.Clamp(
            calculatedScore,
            0.0,
            1.0);
    }

    /// <summary>
    /// Recalculates and stores the compatibility score.
    /// </summary>
    /// <returns>
    /// Newly calculated score.
    /// </returns>
    public double UpdateScore()
    {
        double calculatedScore =
            CalculateScore();

        Score =
            calculatedScore;

        return calculatedScore;
    }

    /// <summary>
    /// Determines the compatibility level from the current
    /// evidence, scope and score.
    /// </summary>
    /// <param name="recommendedScoreThreshold">
    /// Minimum score required for a directly compatible method
    /// to be marked as recommended.
    /// </param>
    /// <param name="updateEvaluationDate">
    /// Value indicating whether the evaluation date must be
    /// set to the current UTC date and time.
    /// </param>
    /// <returns>
    /// Newly assigned compatibility level.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when
    /// <paramref name="recommendedScoreThreshold"/> is not
    /// finite or does not lie between zero and one.
    /// </exception>
    public MethodCompatibilityLevel
        UpdateCompatibilityFromEvidence(
            double recommendedScoreThreshold = 0.85,
            bool updateEvaluationDate = true)
    {
        if (!double.IsFinite(
                recommendedScoreThreshold) ||
            recommendedScoreThreshold < 0.0 ||
            recommendedScoreThreshold > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(recommendedScoreThreshold),
                recommendedScoreThreshold,
                "The recommended-score threshold must be " +
                "finite and lie between zero and one.");
        }

        UpdateScore();

        MethodCompatibilityLevel level;

        if (!HasEvidence ||
            Scope ==
                ProblemClassificationScope.Unknown)
        {
            level =
                MethodCompatibilityLevel.NotEvaluated;
        }
        else if (HasBlockingMismatches)
        {
            level =
                MethodCompatibilityLevel.Incompatible;
        }
        else if (HasRequiredMismatches ||
                 !AppliesToCompleteProblem ||
                 HasRequiredAdaptations)
        {
            level =
                MethodCompatibilityLevel
                    .PartiallyCompatible;
        }
        else if (Score >=
                 recommendedScoreThreshold)
        {
            level =
                MethodCompatibilityLevel.Recommended;
        }
        else
        {
            level =
                MethodCompatibilityLevel.Compatible;
        }

        CompatibilityLevel =
            level;

        if (updateEvaluationDate &&
            level !=
                MethodCompatibilityLevel.NotEvaluated)
        {
            EvaluatedAtUtc =
                DateTime.UtcNow;
        }

        return level;
    }

    /// <summary>
    /// Determines whether the recommendation was produced for
    /// the supplied supply-chain fingerprint.
    /// </summary>
    /// <param name="currentFingerprint">
    /// Current supply-chain fingerprint.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when both fingerprints are
    /// present and equal; otherwise, <see langword="false"/>.
    /// </returns>
    public bool MatchesSupplyChainFingerprint(
        string currentFingerprint)
    {
        if (!HasSupplyChainFingerprint ||
            string.IsNullOrWhiteSpace(
                currentFingerprint))
        {
            return false;
        }

        return string.Equals(
            SupplyChainFingerprint,
            currentFingerprint.Trim(),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Clears the evaluated compatibility while preserving the
    /// identity of the solution method.
    /// </summary>
    public void ClearEvaluation()
    {
        CompatibilityLevel =
            MethodCompatibilityLevel.NotEvaluated;

        Scope =
            ProblemClassificationScope.Unknown;

        ScopeDescription =
            string.Empty;

        Score =
            0.0;

        Rank =
            null;

        EvaluatedAtUtc =
            null;

        Summary =
            string.Empty;

        Evidence.Clear();
        RequiredAdaptations.Clear();
        Warnings.Clear();

        NotifyEvidenceProperties();

        OnPropertyChanged(
            nameof(RequiredAdaptations));

        OnPropertyChanged(
            nameof(HasRequiredAdaptations));

        OnPropertyChanged(
            nameof(Warnings));

        OnPropertyChanged(
            nameof(HasWarnings));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string scoreDescription =
            Score.ToString(
                "0.000",
                CultureInfo.InvariantCulture);

        string rankDescription =
            Rank is int rank
                ? $"; rank {rank}"
                : string.Empty;

        return
            $"{MethodCode} — {MethodName}; " +
            $"{CompatibilityLevel}; " +
            $"score {scoreDescription}" +
            rankDescription;
    }

    private static string NormalizeCode(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }

    private static DateTime ConvertToUtc(
        DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc =>
                value,

            DateTimeKind.Local =>
                value.ToUniversalTime(),

            _ =>
                DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc)
        };
    }

    private static void ReplaceNormalizedStrings(
        ICollection<string> destination,
        IEnumerable<string> source,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(
            source,
            parameterName);

        string[] normalizedValues =
            source
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(
                            value))
                .Select(
                    value =>
                        value.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    value =>
                        value,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        destination.Clear();

        foreach (string value in normalizedValues)
        {
            destination.Add(
                value);
        }
    }

    private void NotifyCompatibilityProperties()
    {
        OnPropertyChanged(
            nameof(HasBeenEvaluated));

        OnPropertyChanged(
            nameof(IsIncompatible));

        OnPropertyChanged(
            nameof(IsPartiallyCompatible));

        OnPropertyChanged(
            nameof(IsCompatible));

        OnPropertyChanged(
            nameof(IsRecommended));

        OnPropertyChanged(
            nameof(CanSolveCompleteProblemDirectly));

        OnPropertyChanged(
            nameof(RequiresAdaptation));

        OnPropertyChanged(
            nameof(IsValidRecommendation));
    }

    private void NotifyScopeProperties()
    {
        OnPropertyChanged(
            nameof(HasScope));

        OnPropertyChanged(
            nameof(AppliesToCompleteProblem));

        OnPropertyChanged(
            nameof(CanSolveCompleteProblemDirectly));

        OnPropertyChanged(
            nameof(RequiresAdaptation));

        OnPropertyChanged(
            nameof(IsValidRecommendation));
    }

    private void NotifyEvidenceProperties()
    {
        OnPropertyChanged(
            nameof(Evidence));

        OnPropertyChanged(
            nameof(HasEvidence));

        OnPropertyChanged(
            nameof(EvidenceCount));

        OnPropertyChanged(
            nameof(SatisfiedEvidenceCount));

        OnPropertyChanged(
            nameof(MismatchCount));

        OnPropertyChanged(
            nameof(BlockingMismatchCount));

        OnPropertyChanged(
            nameof(RequiredMismatchCount));

        OnPropertyChanged(
            nameof(OptionalMismatchCount));

        OnPropertyChanged(
            nameof(HasBlockingMismatches));

        OnPropertyChanged(
            nameof(HasRequiredMismatches));

        OnPropertyChanged(
            nameof(HasOptionalMismatches));

        OnPropertyChanged(
            nameof(CanSolveCompleteProblemDirectly));
    }
}