using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Common;

namespace LotSizingDataModel.Instance.Results;

/// <summary>
/// Represents a known result associated with a lot-sizing
/// problem instance.
/// </summary>
/// <remarks>
/// A known result may contain:
/// <list type="bullet">
/// <item>
/// <description>
/// only an objective value reported by an external source;
/// </description>
/// </item>
/// <item>
/// <description>
/// feasibility or optimality claims without detailed
/// decisions;
/// </description>
/// </item>
/// <item>
/// <description>
/// a complete solver-independent
/// <see cref="LotSizingSolution"/>;
/// </description>
/// </item>
/// <item>
/// <description>
/// source, authorship and verification information.
/// </description>
/// </item>
/// </list>
///
/// A detailed solution is deliberately optional. Published
/// benchmark results frequently provide only an objective
/// value and a description of the method used.
/// </remarks>
[Serializable]
[XmlType(TypeName = "knownResult")]
public sealed class KnownResult : ModelObject
{
    private string _resultId =
        string.Empty;

    private string _name =
        string.Empty;

    private double? _reportedObjectiveValue;

    private string _objectiveName =
        string.Empty;

    private string _objectiveUnit =
        string.Empty;

    /*
     * The first member of each enumeration is treated as the
     * default state indicating that no information has been
     * recorded.
     */
    private FeasibilityStatus _feasibilityStatus =
        default;

    private OptimalityStatus _optimalityStatus =
        default;

    private KnownResultVerificationStatus _verificationStatus =
        KnownResultVerificationStatus.NotVerified;

    private LotSizingSolution? _detailedSolution;

    private string _methodName =
        string.Empty;

    private string _sourceTitle =
        string.Empty;

    private string _sourceReference =
        string.Empty;

    private string _sourceUri =
        string.Empty;

    private string _digitalObjectIdentifier =
        string.Empty;

    private DateTime? _obtainedAtUtc;

    private DateTime? _recordedAtUtc;

    private string _supplyChainFingerprint =
        string.Empty;

    private string _supersededByResultId =
        string.Empty;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty known result.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public KnownResult()
    {
    }

    /// <summary>
    /// Initializes a known result with a stable identifier.
    /// </summary>
    /// <param name="resultId">
    /// Stable identifier of the known result.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="resultId"/> is null, empty
    /// or composed only of white-space characters.
    /// </exception>
    public KnownResult(
        string resultId)
    {
        if (string.IsNullOrWhiteSpace(resultId))
        {
            throw new ArgumentException(
                "A known-result identifier is required.",
                nameof(resultId));
        }

        ResultId =
            resultId;
    }

    /// <summary>
    /// Initializes a known result containing a reported
    /// objective value.
    /// </summary>
    /// <param name="resultId">
    /// Stable identifier of the known result.
    /// </param>
    /// <param name="reportedObjectiveValue">
    /// Objective value reported for the instance.
    /// </param>
    /// <param name="name">
    /// Optional human-readable result name.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="resultId"/> is null, empty
    /// or composed only of white-space characters.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="reportedObjectiveValue"/>
    /// is not finite.
    /// </exception>
    public KnownResult(
        string resultId,
        double reportedObjectiveValue,
        string name = "")
        : this(resultId)
    {
        ReportedObjectiveValue =
            reportedObjectiveValue;

        Name =
            name;
    }

    /// <summary>
    /// Gets or sets the stable identifier of this result.
    /// </summary>
    /// <remarks>
    /// The identifier is used by
    /// <c>LotSizingInstance.BestKnownResultId</c> and by links
    /// between superseded results.
    /// </remarks>
    [XmlAttribute("resultId")]
    public string ResultId
    {
        get => _resultId;
        set
        {
            if (SetProperty(
                    ref _resultId,
                    value?.Trim() ?? string.Empty))
            {
                NotifyIdentityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable name of the result.
    /// </summary>
    /// <remarks>
    /// Examples include <c>Best value reported in 2025</c>,
    /// <c>CPLEX reference solution</c> and
    /// <c>Author-provided solution</c>.
    /// </remarks>
    [XmlAttribute("name")]
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(
                    ref _name,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasName));
            }
        }
    }

    /// <summary>
    /// Gets or sets the objective value explicitly reported
    /// for this result.
    /// </summary>
    /// <remarks>
    /// This property is independent from the optional
    /// detailed solution.
    ///
    /// It may therefore be populated even when
    /// <see cref="DetailedSolution"/> is
    /// <see langword="null"/>.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied value is not finite.
    /// </exception>
    [XmlElement(
        "reportedObjectiveValue",
        IsNullable = true)]
    public double? ReportedObjectiveValue
    {
        get => _reportedObjectiveValue;
        set
        {
            if (value.HasValue &&
                !double.IsFinite(value.Value))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The reported objective value must " +
                    "be finite.");
            }

            if (SetProperty(
                    ref _reportedObjectiveValue,
                    value))
            {
                NotifyResultDataProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the reported objective.
    /// </summary>
    /// <remarks>
    /// Examples include <c>TotalCost</c>,
    /// <c>NetPresentValue</c> and <c>Makespan</c>.
    ///
    /// This property is useful when the instance supports
    /// several possible evaluation criteria.
    /// </remarks>
    [XmlAttribute("objectiveName")]
    public string ObjectiveName
    {
        get => _objectiveName;
        set
        {
            if (SetProperty(
                    ref _objectiveName,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasObjectiveName));
            }
        }
    }

    /// <summary>
    /// Gets or sets the unit associated with the reported
    /// objective value.
    /// </summary>
    /// <remarks>
    /// Examples include <c>EUR</c>, <c>USD</c>,
    /// <c>hours</c> and <c>dimensionless</c>.
    /// </remarks>
    [XmlAttribute("objectiveUnit")]
    public string ObjectiveUnit
    {
        get => _objectiveUnit;
        set
        {
            if (SetProperty(
                    ref _objectiveUnit,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasObjectiveUnit));
            }
        }
    }

    /// <summary>
    /// Gets or sets the reported or verified feasibility
    /// status of the result.
    /// </summary>
    /// <remarks>
    /// This status may represent a claim from an external
    /// source when no detailed solution is available.
    ///
    /// The verification level of the claim is described by
    /// <see cref="VerificationStatus"/>.
    ///
    /// The default enumeration value is interpreted as the
    /// absence of feasibility information.
    /// </remarks>
    [XmlAttribute("feasibilityStatus")]
    public FeasibilityStatus FeasibilityStatus
    {
        get => _feasibilityStatus;
        set
        {
            if (SetProperty(
                    ref _feasibilityStatus,
                    value))
            {
                OnPropertyChanged(
                    nameof(HasFeasibilityInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the reported or verified optimality
    /// status of the result.
    /// </summary>
    /// <remarks>
    /// An external source may claim optimality even when the
    /// detailed solution or proof is unavailable.
    ///
    /// The default enumeration value is interpreted as the
    /// absence of optimality information.
    /// </remarks>
    [XmlAttribute("optimalityStatus")]
    public OptimalityStatus OptimalityStatus
    {
        get => _optimalityStatus;
        set
        {
            if (SetProperty(
                    ref _optimalityStatus,
                    value))
            {
                OnPropertyChanged(
                    nameof(HasOptimalityInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the level of verification applied to
    /// this result.
    /// </summary>
    [XmlAttribute("verificationStatus")]
    public KnownResultVerificationStatus VerificationStatus
    {
        get => _verificationStatus;
        set
        {
            if (SetProperty(
                    ref _verificationStatus,
                    value))
            {
                NotifyVerificationProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the optional detailed solution associated
    /// with this known result.
    /// </summary>
    /// <remarks>
    /// A null value is valid and commonly represents a
    /// published result for which only an objective value is
    /// available.
    ///
    /// When present, the detailed solution contains the
    /// production, inventory, purchasing, transportation,
    /// distribution and capacity decisions.
    /// </remarks>
    [XmlElement(
        "detailedSolution",
        IsNullable = true)]
    public LotSizingSolution? DetailedSolution
    {
        get => _detailedSolution;
        set
        {
            if (SetProperty(
                    ref _detailedSolution,
                    value))
            {
                NotifyResultDataProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the algorithm, solver or
    /// procedure that produced the result.
    /// </summary>
    /// <remarks>
    /// Examples include <c>Dynamic programming</c>,
    /// <c>Branch-and-cut</c>, <c>CPLEX 22.1</c> and
    /// <c>Author heuristic</c>.
    /// </remarks>
    [XmlElement("methodName")]
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
                    nameof(HasMethodInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the title of the publication, report,
    /// repository or other source reporting the result.
    /// </summary>
    [XmlElement("sourceTitle")]
    public string SourceTitle
    {
        get => _sourceTitle;
        set
        {
            if (SetProperty(
                    ref _sourceTitle,
                    value?.Trim() ?? string.Empty))
            {
                NotifySourceProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a bibliographic or documentary reference
    /// for the source reporting the result.
    /// </summary>
    /// <remarks>
    /// This property may contain a formatted citation,
    /// technical-report identifier, benchmark name or
    /// repository reference.
    /// </remarks>
    [XmlElement("sourceReference")]
    public string SourceReference
    {
        get => _sourceReference;
        set
        {
            if (SetProperty(
                    ref _sourceReference,
                    value?.Trim() ?? string.Empty))
            {
                NotifySourceProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets an optional URI identifying the source
    /// from which the result was obtained.
    /// </summary>
    /// <remarks>
    /// The URI is stored as text because benchmark sources
    /// may use HTTP addresses, repository identifiers or
    /// other URI schemes.
    /// </remarks>
    [XmlElement("sourceUri")]
    public string SourceUri
    {
        get => _sourceUri;
        set
        {
            if (SetProperty(
                    ref _sourceUri,
                    value?.Trim() ?? string.Empty))
            {
                NotifySourceProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the digital object identifier of the
    /// publication reporting the result.
    /// </summary>
    /// <remarks>
    /// The value should normally be stored without a
    /// <c>https://doi.org/</c> prefix.
    /// </remarks>
    [XmlAttribute("doi")]
    public string DigitalObjectIdentifier
    {
        get => _digitalObjectIdentifier;
        set
        {
            if (SetProperty(
                    ref _digitalObjectIdentifier,
                    NormalizeDigitalObjectIdentifier(value)))
            {
                NotifySourceProperties();
            }
        }
    }

    /// <summary>
    /// Gets the names of persons or organizations credited
    /// with obtaining or discovering the result.
    /// </summary>
    /// <remarks>
    /// These names may differ from the persons who imported
    /// the result into the current data file.
    /// </remarks>
    [XmlArray("discoverers")]
    [XmlArrayItem("discoverer")]
    public List<string> Discoverers { get; } =
        new();

    /// <summary>
    /// Gets the authors or organizations associated with the
    /// source reporting the result.
    /// </summary>
    [XmlArray("sourceAuthors")]
    [XmlArrayItem("author")]
    public List<string> SourceAuthors { get; } =
        new();

    /// <summary>
    /// Gets or sets the UTC date and time at which the result
    /// was obtained.
    /// </summary>
    /// <remarks>
    /// A null value means that the result date is unknown.
    ///
    /// For results extracted from publications, this property
    /// may represent the known publication or computation
    /// date when a more precise timestamp is unavailable.
    /// </remarks>
    [XmlElement(
        "obtainedAtUtc",
        IsNullable = true)]
    public DateTime? ObtainedAtUtc
    {
        get => _obtainedAtUtc;
        set => SetUtcDateTime(
            ref _obtainedAtUtc,
            value,
            nameof(ObtainedAtUtc));
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the result
    /// was recorded in the instance data.
    /// </summary>
    [XmlElement(
        "recordedAtUtc",
        IsNullable = true)]
    public DateTime? RecordedAtUtc
    {
        get => _recordedAtUtc;
        set => SetUtcDateTime(
            ref _recordedAtUtc,
            value,
            nameof(RecordedAtUtc));
    }

    /// <summary>
    /// Gets or sets the fingerprint of the supply-chain data
    /// to which this result applies.
    /// </summary>
    /// <remarks>
    /// A fingerprint prevents a result obtained for a
    /// previous instance version from being silently treated
    /// as valid for modified data.
    /// </remarks>
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
    /// Gets or sets the identifier of a newer result that
    /// supersedes this result.
    /// </summary>
    /// <remarks>
    /// The superseded result is retained for historical
    /// traceability.
    ///
    /// A superseded result should not normally be selected as
    /// the current best known result.
    /// </remarks>
    [XmlAttribute("supersededByResultId")]
    public string SupersededByResultId
    {
        get => _supersededByResultId;
        set
        {
            if (SetProperty(
                    ref _supersededByResultId,
                    value?.Trim() ?? string.Empty))
            {
                NotifyVerificationProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets an optional explanatory comment about the
    /// result, its source or its verification.
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
    /// Gets a value indicating whether the result has a
    /// stable identifier.
    /// </summary>
    [XmlIgnore]
    public bool HasResultId =>
        !string.IsNullOrWhiteSpace(ResultId);

    /// <summary>
    /// Gets a value indicating whether the result has a
    /// human-readable name.
    /// </summary>
    [XmlIgnore]
    public bool HasName =>
        !string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Gets a value indicating whether an explicit objective
    /// value has been reported.
    /// </summary>
    [XmlIgnore]
    public bool HasReportedObjectiveValue =>
        ReportedObjectiveValue.HasValue;

    /// <summary>
    /// Gets a value indicating whether the objective name has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasObjectiveName =>
        !string.IsNullOrWhiteSpace(ObjectiveName);

    /// <summary>
    /// Gets a value indicating whether the objective unit has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasObjectiveUnit =>
        !string.IsNullOrWhiteSpace(ObjectiveUnit);

    /// <summary>
    /// Gets a value indicating whether feasibility
    /// information has been recorded.
    /// </summary>
    /// <remarks>
    /// The default enumeration value is treated as the state
    /// in which no feasibility information is available.
    /// </remarks>
    [XmlIgnore]
    public bool HasFeasibilityInformation =>
        !EqualityComparer<FeasibilityStatus>
            .Default
            .Equals(
                FeasibilityStatus,
                default);

    /// <summary>
    /// Gets a value indicating whether optimality
    /// information has been recorded.
    /// </summary>
    /// <remarks>
    /// The default enumeration value is treated as the state
    /// in which no optimality information is available.
    /// </remarks>
    [XmlIgnore]
    public bool HasOptimalityInformation =>
        !EqualityComparer<OptimalityStatus>
            .Default
            .Equals(
                OptimalityStatus,
                default);

    /// <summary>
    /// Gets a value indicating whether a detailed solution is
    /// associated with the result.
    /// </summary>
    [XmlIgnore]
    public bool HasDetailedSolution =>
        DetailedSolution is not null;

    /// <summary>
    /// Gets a value indicating whether information about the
    /// solution method has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasMethodInformation =>
        !string.IsNullOrWhiteSpace(MethodName);

    /// <summary>
    /// Gets a value indicating whether at least one source
    /// field has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSourceInformation =>
        !string.IsNullOrWhiteSpace(SourceTitle) ||
        !string.IsNullOrWhiteSpace(SourceReference) ||
        !string.IsNullOrWhiteSpace(SourceUri) ||
        !string.IsNullOrWhiteSpace(
            DigitalObjectIdentifier) ||
        SourceAuthors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one
    /// discoverer has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasDiscoverers =>
        Discoverers.Count > 0;

    /// <summary>
    /// Gets a value indicating whether a supply-chain
    /// fingerprint has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSupplyChainFingerprint =>
        !string.IsNullOrWhiteSpace(
            SupplyChainFingerprint);

    /// <summary>
    /// Gets a value indicating whether an explanatory comment
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasComment =>
        !string.IsNullOrWhiteSpace(Comment);

    /// <summary>
    /// Gets a value indicating whether this result has been
    /// superseded by another result.
    /// </summary>
    [XmlIgnore]
    public bool IsSuperseded =>
        !string.IsNullOrWhiteSpace(
            SupersededByResultId);

    /// <summary>
    /// Gets a value indicating whether the result has been
    /// disputed.
    /// </summary>
    [XmlIgnore]
    public bool IsDisputed =>
        VerificationStatus ==
        KnownResultVerificationStatus.Disputed;

    /// <summary>
    /// Gets a value indicating whether the result has been
    /// invalidated.
    /// </summary>
    [XmlIgnore]
    public bool IsInvalidated =>
        VerificationStatus ==
        KnownResultVerificationStatus.Invalidated;

    /// <summary>
    /// Gets a value indicating whether the result contains
    /// usable numerical or detailed solution data.
    /// </summary>
    [XmlIgnore]
    public bool HasResultData =>
        HasReportedObjectiveValue ||
        HasDetailedSolution;

    /// <summary>
    /// Gets a value indicating whether this result may be
    /// considered when selecting the best known result.
    /// </summary>
    /// <remarks>
    /// This property only determines eligibility. Comparing
    /// objective values still requires knowledge of the
    /// objective direction and meaning.
    /// </remarks>
    [XmlIgnore]
    public bool CanBeSelectedAsBestKnownResult =>
        HasResultId &&
        HasResultData &&
        !IsSuperseded &&
        !IsDisputed &&
        !IsInvalidated;

    /// <summary>
    /// Replaces the persons or organizations credited with
    /// discovering or obtaining the result.
    /// </summary>
    /// <param name="discoverers">
    /// Discoverer names.
    /// </param>
    public void ReplaceDiscoverers(
        IEnumerable<string> discoverers)
    {
        ReplaceNormalizedStrings(
            Discoverers,
            discoverers,
            nameof(discoverers));

        OnPropertyChanged(
            nameof(Discoverers));

        OnPropertyChanged(
            nameof(HasDiscoverers));
    }

    /// <summary>
    /// Replaces the authors or organizations associated with
    /// the result source.
    /// </summary>
    /// <param name="sourceAuthors">
    /// Source-author names.
    /// </param>
    public void ReplaceSourceAuthors(
        IEnumerable<string> sourceAuthors)
    {
        ReplaceNormalizedStrings(
            SourceAuthors,
            sourceAuthors,
            nameof(sourceAuthors));

        OnPropertyChanged(
            nameof(SourceAuthors));

        OnPropertyChanged(
            nameof(HasSourceInformation));
    }

    /// <summary>
    /// Removes the explicitly reported objective value and
    /// its descriptive metadata.
    /// </summary>
    public void ClearReportedObjective()
    {
        ReportedObjectiveValue =
            null;

        ObjectiveName =
            string.Empty;

        ObjectiveUnit =
            string.Empty;
    }

    /// <summary>
    /// Removes the detailed solution while preserving all
    /// reported objective and source information.
    /// </summary>
    public void ClearDetailedSolution()
    {
        DetailedSolution =
            null;
    }

    /// <summary>
    /// Marks this result as superseded by another result.
    /// </summary>
    /// <param name="newResultId">
    /// Identifier of the newer result.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="newResultId"/> is empty or
    /// identifies this result itself.
    /// </exception>
    public void MarkAsSupersededBy(
        string newResultId)
    {
        if (string.IsNullOrWhiteSpace(newResultId))
        {
            throw new ArgumentException(
                "A superseding result identifier is required.",
                nameof(newResultId));
        }

        string normalizedResultId =
            newResultId.Trim();

        if (string.Equals(
                ResultId,
                normalizedResultId,
                StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(
                "A result cannot supersede itself.",
                nameof(newResultId));
        }

        SupersededByResultId =
            normalizedResultId;
    }

    /// <summary>
    /// Removes the link indicating that this result has been
    /// superseded.
    /// </summary>
    public void ClearSupersedingResult()
    {
        SupersededByResultId =
            string.Empty;
    }

    /// <summary>
    /// Determines whether this result was obtained for the
    /// supplied supply-chain fingerprint.
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
            string.IsNullOrWhiteSpace(currentFingerprint))
        {
            return false;
        }

        return string.Equals(
            SupplyChainFingerprint,
            currentFingerprint.Trim(),
            StringComparison.Ordinal);
    }

    /// <inheritdoc/>
    /// <inheritdoc/>
    public override string ToString()
    {
        string resultDescription =
            HasName
                ? Name
                : ResultId;

        string objectiveDescription =
            ReportedObjectiveValue switch
            {
                double objectiveValue =>
                    objectiveValue.ToString(
                        CultureInfo.InvariantCulture),

                null when HasDetailedSolution =>
                    "detailed solution",

                _ =>
                    "no result data"
            };

        return
            $"{resultDescription}; " +
            $"{objectiveDescription}; " +
            $"{VerificationStatus}";
    }

    private static string
        NormalizeDigitalObjectIdentifier(
            string? value)
    {
        string normalizedValue =
            value?.Trim() ??
            string.Empty;

        const string httpsPrefix =
            "https://doi.org/";

        const string httpPrefix =
            "http://doi.org/";

        const string doiPrefix =
            "doi:";

        if (normalizedValue.StartsWith(
                httpsPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return normalizedValue[
                httpsPrefix.Length..].Trim();
        }

        if (normalizedValue.StartsWith(
                httpPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return normalizedValue[
                httpPrefix.Length..].Trim();
        }

        if (normalizedValue.StartsWith(
                doiPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return normalizedValue[
                doiPrefix.Length..].Trim();
        }

        return normalizedValue;
    }

    private static void ReplaceNormalizedStrings(
        ICollection<string> destination,
        IEnumerable<string> source,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(source);

        string[] normalizedValues =
            source
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(value))
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
            destination.Add(value);
        }
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

    private void SetUtcDateTime(
        ref DateTime? storage,
        DateTime? value,
        string propertyName)
    {
        DateTime? normalizedValue =
            value.HasValue
                ? ConvertToUtc(value.Value)
                : null;

        SetProperty(
            ref storage,
            normalizedValue,
            propertyName);
    }

    private void NotifyIdentityProperties()
    {
        OnPropertyChanged(
            nameof(HasResultId));

        OnPropertyChanged(
            nameof(CanBeSelectedAsBestKnownResult));
    }

    private void NotifyResultDataProperties()
    {
        OnPropertyChanged(
            nameof(HasReportedObjectiveValue));

        OnPropertyChanged(
            nameof(HasDetailedSolution));

        OnPropertyChanged(
            nameof(HasResultData));

        OnPropertyChanged(
            nameof(CanBeSelectedAsBestKnownResult));
    }

    private void NotifySourceProperties()
    {
        OnPropertyChanged(
            nameof(HasSourceInformation));
    }

    private void NotifyVerificationProperties()
    {
        OnPropertyChanged(
            nameof(IsSuperseded));

        OnPropertyChanged(
            nameof(IsDisputed));

        OnPropertyChanged(
            nameof(IsInvalidated));

        OnPropertyChanged(
            nameof(CanBeSelectedAsBestKnownResult));
    }
}