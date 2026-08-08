using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Stores the persistent result of the automatic
/// classification of a lot-sizing problem instance.
/// </summary>
/// <remarks>
/// A classification contains the factual feature profile,
/// all detected known-problem-family matches and an optional
/// primary family selected by the classifier.
///
/// The classification also records the supply-chain
/// fingerprint, classifier version and catalog version used
/// to produce the result.
/// </remarks>
[Serializable]
[XmlType(TypeName = "lotSizingProblemClassification")]
public sealed class LotSizingProblemClassification :
    ModelObject
{
    private ProblemClassificationStatus _status =
        ProblemClassificationStatus.NotAnalyzed;

    private LotSizingProblemFeatures _features =
        new();

    private string _primaryProblemTypeCode =
        string.Empty;

    private string _primaryProblemTypeName =
        string.Empty;

    private string _classifierVersion =
        string.Empty;

    private string _catalogName =
        string.Empty;

    private string _catalogVersion =
        string.Empty;

    private DateTime? _classifiedAtUtc;

    private string _supplyChainFingerprint =
        string.Empty;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty lot-sizing problem
    /// classification.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public LotSizingProblemClassification()
    {
    }

    /// <summary>
    /// Initializes a lot-sizing problem classification with
    /// an extracted feature profile.
    /// </summary>
    /// <param name="features">
    /// Factual features extracted from the supply-chain
    /// instance.
    /// </param>
    public LotSizingProblemClassification(
        LotSizingProblemFeatures features)
    {
        ArgumentNullException.ThrowIfNull(features);

        Features = features;
    }

    /// <summary>
    /// Gets or sets the current global classification status.
    /// </summary>
    [XmlAttribute("status")]
    public ProblemClassificationStatus Status
    {
        get => _status;
        set
        {
            if (SetProperty(
                    ref _status,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the factual lot-sizing problem features
    /// used during classification.
    /// </summary>
    [XmlElement("features")]
    public LotSizingProblemFeatures Features
    {
        get => _features;
        set
        {
            LotSizingProblemFeatures normalizedValue =
                value ??
                new LotSizingProblemFeatures();

            if (SetProperty(
                    ref _features,
                    normalizedValue))
            {
                OnPropertyChanged(
                    nameof(HasUsableFeatures));

                OnPropertyChanged(
                    nameof(CanBeUsedForMethodSelection));
            }
        }
    }

    /// <summary>
    /// Gets or sets the stable code of the primary recognized
    /// problem family.
    /// </summary>
    /// <remarks>
    /// The value is empty when no unique primary family has
    /// been selected.
    /// </remarks>
    [XmlAttribute("primaryProblemTypeCode")]
    public string PrimaryProblemTypeCode
    {
        get => _primaryProblemTypeCode;
        set
        {
            if (SetProperty(
                    ref _primaryProblemTypeCode,
                    NormalizeProblemTypeCode(value)))
            {
                NotifyPrimaryMatchProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable name of the primary
    /// recognized problem family.
    /// </summary>
    [XmlAttribute("primaryProblemTypeName")]
    public string PrimaryProblemTypeName
    {
        get => _primaryProblemTypeName;
        set
        {
            if (SetProperty(
                    ref _primaryProblemTypeName,
                    value?.Trim() ?? string.Empty))
            {
                NotifyPrimaryMatchProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the software classifier
    /// that produced this result.
    /// </summary>
    [XmlAttribute("classifierVersion")]
    public string ClassifierVersion
    {
        get => _classifierVersion;
        set
        {
            if (SetProperty(
                    ref _classifierVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasClassifierVersion));
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable name of the
    /// problem-type catalog used during classification.
    /// </summary>
    [XmlAttribute("catalogName")]
    public string CatalogName
    {
        get => _catalogName;
        set
        {
            if (SetProperty(
                    ref _catalogName,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasCatalogInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the problem-type catalog
    /// used during classification.
    /// </summary>
    [XmlAttribute("catalogVersion")]
    public string CatalogVersion
    {
        get => _catalogVersion;
        set
        {
            if (SetProperty(
                    ref _catalogVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasCatalogInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the
    /// classification was produced.
    /// </summary>
    /// <remarks>
    /// A null value means that no completed classification
    /// date has been recorded.
    /// </remarks>
    [XmlElement("classifiedAtUtc", IsNullable = true)]
    public DateTime? ClassifiedAtUtc
    {
        get => _classifiedAtUtc;
        set
        {
            DateTime? utcValue =
                value.HasValue
                    ? ConvertToUtc(value.Value)
                    : null;

            if (SetProperty(
                    ref _classifiedAtUtc,
                    utcValue))
            {
                OnPropertyChanged(
                    nameof(HasBeenAnalyzed));
            }
        }
    }

    /// <summary>
    /// Gets or sets the fingerprint of the supply-chain data
    /// used to produce this classification.
    /// </summary>
    /// <remarks>
    /// The fingerprint allows validation services to detect
    /// that the supply chain has changed after classification.
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
    /// Gets all known-problem-family matches detected for
    /// the analyzed instance.
    /// </summary>
    /// <remarks>
    /// The collection may contain complete-problem matches,
    /// relaxations, subproblems and closest-family matches.
    /// </remarks>
    [XmlArray("matches")]
    [XmlArrayItem("knownProblemTypeMatch")]
    public List<KnownProblemTypeMatch> Matches { get; } =
        new();

    /// <summary>
    /// Gets the codes of detected features that are not
    /// adequately represented by any reported family match.
    /// </summary>
    /// <remarks>
    /// These codes help distinguish a fully classified
    /// problem from a partially classified extension.
    /// </remarks>
    [XmlArray("unclassifiedFeatureCodes")]
    [XmlArrayItem("featureCode")]
    public List<string> UnclassifiedFeatureCodes { get; } =
        new();

    /// <summary>
    /// Gets the warnings produced during feature extraction
    /// or problem classification.
    /// </summary>
    [XmlArray("warnings")]
    [XmlArrayItem("warning")]
    public List<string> Warnings { get; } =
        new();

    /// <summary>
    /// Gets the errors produced during feature extraction or
    /// problem classification.
    /// </summary>
    [XmlArray("errors")]
    [XmlArrayItem("error")]
    public List<string> Errors { get; } =
        new();

    /// <summary>
    /// Gets or sets an optional human-readable comment about
    /// the complete classification.
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
    /// Gets a value indicating whether the extracted feature
    /// profile contains the minimum information required by
    /// the classifier.
    /// </summary>
    [XmlIgnore]
    public bool HasUsableFeatures =>
        Features.IsStructurallyUsable;

    /// <summary>
    /// Gets a value indicating whether a primary
    /// problem-family code has been selected.
    /// </summary>
    [XmlIgnore]
    public bool HasPrimaryProblemType =>
        !string.IsNullOrWhiteSpace(
            PrimaryProblemTypeCode);

    /// <summary>
    /// Gets a value indicating whether the primary
    /// problem-family name has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasPrimaryProblemTypeName =>
        !string.IsNullOrWhiteSpace(
            PrimaryProblemTypeName);

    /// <summary>
    /// Gets a value indicating whether the classifier
    /// version has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasClassifierVersion =>
        !string.IsNullOrWhiteSpace(
            ClassifierVersion);

    /// <summary>
    /// Gets a value indicating whether both catalog name and
    /// catalog version have been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasCatalogInformation =>
        !string.IsNullOrWhiteSpace(
            CatalogName) &&
        !string.IsNullOrWhiteSpace(
            CatalogVersion);

    /// <summary>
    /// Gets a value indicating whether a supply-chain
    /// fingerprint has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSupplyChainFingerprint =>
        !string.IsNullOrWhiteSpace(
            SupplyChainFingerprint);

    /// <summary>
    /// Gets a value indicating whether a completed analysis
    /// date has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasBeenAnalyzed =>
        ClassifiedAtUtc.HasValue;

    /// <summary>
    /// Gets a value indicating whether an explanatory comment
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    /// <summary>
    /// Gets the total number of reported problem-family
    /// matches.
    /// </summary>
    [XmlIgnore]
    public int MatchCount =>
        Matches.Count;

    /// <summary>
    /// Gets the number of direct exact or known-extension
    /// matches without blocking conditions.
    /// </summary>
    [XmlIgnore]
    public int DirectMatchCount =>
        Matches.Count(
            match =>
                match is not null &&
                match.IsDirectMatch);

    /// <summary>
    /// Gets the number of exact matches.
    /// </summary>
    [XmlIgnore]
    public int ExactMatchCount =>
        Matches.Count(
            match =>
                match is not null &&
                match.IsExactMatch &&
                !match.HasBlockingMismatches);

    /// <summary>
    /// Gets the number of recognized relaxations.
    /// </summary>
    [XmlIgnore]
    public int RelaxationMatchCount =>
        Matches.Count(
            match =>
                match is not null &&
                match.MatchKind ==
                ProblemMatchKind.RecognizedRelaxation);

    /// <summary>
    /// Gets the number of recognized subproblems.
    /// </summary>
    [XmlIgnore]
    public int SubproblemMatchCount =>
        Matches.Count(
            match =>
                match is not null &&
                match.MatchKind ==
                ProblemMatchKind.RecognizedSubproblem);

    /// <summary>
    /// Gets a value indicating whether at least one
    /// problem-family match has been reported.
    /// </summary>
    [XmlIgnore]
    public bool HasMatches =>
        MatchCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one direct
    /// known-family match has been reported.
    /// </summary>
    [XmlIgnore]
    public bool HasDirectMatches =>
        DirectMatchCount > 0;

    /// <summary>
    /// Gets a value indicating whether unclassified features
    /// remain after classification.
    /// </summary>
    [XmlIgnore]
    public bool HasUnclassifiedFeatures =>
        UnclassifiedFeatureCodes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one warning
    /// was produced.
    /// </summary>
    [XmlIgnore]
    public bool HasWarnings =>
        Warnings.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one error
    /// was produced.
    /// </summary>
    [XmlIgnore]
    public bool HasErrors =>
        Errors.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the stored
    /// classification is marked as outdated.
    /// </summary>
    [XmlIgnore]
    public bool IsOutdated =>
        Status ==
        ProblemClassificationStatus.Outdated;

    /// <summary>
    /// Gets a value indicating whether classification failed
    /// because the analyzed instance was invalid.
    /// </summary>
    [XmlIgnore]
    public bool IsInvalid =>
        Status ==
        ProblemClassificationStatus.Invalid;

    /// <summary>
    /// Gets a value indicating whether the classification can
    /// currently contribute to method selection.
    /// </summary>
    /// <remarks>
    /// A partially classified or ambiguous instance may still
    /// provide useful matches, provided that at least one
    /// direct match exists and the stored result is neither
    /// invalid nor outdated.
    /// </remarks>
    [XmlIgnore]
    public bool CanBeUsedForMethodSelection =>
        HasUsableFeatures &&
        HasDirectMatches &&
        !HasErrors &&
        (
            Status ==
                ProblemClassificationStatus.Classified ||
            Status ==
                ProblemClassificationStatus
                    .PartiallyClassified ||
            Status ==
                ProblemClassificationStatus.Ambiguous
        );

    /// <summary>
    /// Gets the match corresponding to the selected primary
    /// problem-family code.
    /// </summary>
    /// <remarks>
    /// Complete-problem direct matches are preferred when
    /// several matches use the same family code.
    /// </remarks>
    [XmlIgnore]
    public KnownProblemTypeMatch? PrimaryMatch
    {
        get
        {
            if (!HasPrimaryProblemType)
            {
                return null;
            }

            return Matches
                .Where(
                    match =>
                        match is not null &&
                        string.Equals(
                            match.ProblemTypeCode,
                            PrimaryProblemTypeCode,
                            StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(
                    match =>
                        match.AppliesToCompleteProblem)
                .ThenByDescending(
                    match =>
                        match.IsDirectMatch)
                .ThenByDescending(
                    match =>
                        match.Score)
                .FirstOrDefault();
        }
    }

    /// <summary>
    /// Adds one known-problem-family match.
    /// </summary>
    /// <param name="match">
    /// Match to add.
    /// </param>
    public void AddMatch(
        KnownProblemTypeMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);

        Matches.Add(match);

        NotifyMatchProperties();
    }

    /// <summary>
    /// Replaces all known-problem-family matches.
    /// </summary>
    /// <param name="matches">
    /// New match collection.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null element.
    /// </exception>
    public void ReplaceMatches(
        IEnumerable<KnownProblemTypeMatch> matches)
    {
        ArgumentNullException.ThrowIfNull(matches);

        KnownProblemTypeMatch[] normalizedMatches =
            matches.ToArray();

        if (normalizedMatches.Any(
                match =>
                    match is null))
        {
            throw new ArgumentException(
                "The match collection cannot contain a " +
                "null element.",
                nameof(matches));
        }

        Matches.Clear();
        Matches.AddRange(normalizedMatches);

        NotifyMatchProperties();
    }

    /// <summary>
    /// Removes all known-problem-family matches and clears the
    /// selected primary family.
    /// </summary>
    public void ClearMatches()
    {
        Matches.Clear();

        ClearPrimaryProblemType();

        NotifyMatchProperties();
    }

    /// <summary>
    /// Selects one existing match as the primary recognized
    /// problem family.
    /// </summary>
    /// <param name="match">
    /// Match to select.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the supplied match is not contained in the
    /// current classification.
    /// </exception>
    public void SetPrimaryMatch(
        KnownProblemTypeMatch match)
    {
        ArgumentNullException.ThrowIfNull(match);

        if (!Matches.Contains(match))
        {
            throw new InvalidOperationException(
                "The primary match must belong to the " +
                "classification match collection.");
        }

        if (string.IsNullOrWhiteSpace(
                match.ProblemTypeCode))
        {
            throw new InvalidOperationException(
                "The primary match must have a known " +
                "problem-type code.");
        }

        PrimaryProblemTypeCode =
            match.ProblemTypeCode;

        PrimaryProblemTypeName =
            match.ProblemTypeName;
    }

    /// <summary>
    /// Removes the currently selected primary
    /// problem-family information.
    /// </summary>
    public void ClearPrimaryProblemType()
    {
        PrimaryProblemTypeCode =
            string.Empty;

        PrimaryProblemTypeName =
            string.Empty;
    }

    /// <summary>
    /// Replaces the globally unclassified feature codes.
    /// </summary>
    /// <param name="featureCodes">
    /// Feature codes not covered by the reported matches.
    /// </param>
    public void ReplaceUnclassifiedFeatureCodes(
        IEnumerable<string> featureCodes)
    {
        ReplaceNormalizedStrings(
            UnclassifiedFeatureCodes,
            featureCodes,
            nameof(featureCodes));

        OnPropertyChanged(
            nameof(UnclassifiedFeatureCodes));

        OnPropertyChanged(
            nameof(HasUnclassifiedFeatures));
    }

    /// <summary>
    /// Replaces all classification warnings.
    /// </summary>
    /// <param name="warnings">
    /// Warning messages.
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
    /// Replaces all classification errors.
    /// </summary>
    /// <param name="errors">
    /// Error messages.
    /// </param>
    public void ReplaceErrors(
        IEnumerable<string> errors)
    {
        ReplaceNormalizedStrings(
            Errors,
            errors,
            nameof(errors));

        OnPropertyChanged(
            nameof(Errors));

        OnPropertyChanged(
            nameof(HasErrors));

        OnPropertyChanged(
            nameof(CanBeUsedForMethodSelection));
    }

    /// <summary>
    /// Determines whether the stored classification was
    /// produced from the supplied supply-chain fingerprint.
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
        if (string.IsNullOrWhiteSpace(
                currentFingerprint) ||
            !HasSupplyChainFingerprint)
        {
            return false;
        }

        return string.Equals(
            SupplyChainFingerprint,
            currentFingerprint.Trim(),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Marks the stored classification as outdated.
    /// </summary>
    /// <remarks>
    /// Existing features, matches and evidence are preserved
    /// for traceability but must not be used until the
    /// instance is classified again.
    /// </remarks>
    public void MarkAsOutdated()
    {
        if (Status ==
                ProblemClassificationStatus.NotAnalyzed ||
            Status ==
                ProblemClassificationStatus.Invalid)
        {
            return;
        }

        Status =
            ProblemClassificationStatus.Outdated;
    }

    /// <summary>
    /// Clears all generated classification information while
    /// preserving the current feature object.
    /// </summary>
    public void ClearClassificationResult()
    {
        Matches.Clear();
        UnclassifiedFeatureCodes.Clear();
        Warnings.Clear();
        Errors.Clear();

        PrimaryProblemTypeCode =
            string.Empty;

        PrimaryProblemTypeName =
            string.Empty;

        ClassifierVersion =
            string.Empty;

        CatalogName =
            string.Empty;

        CatalogVersion =
            string.Empty;

        ClassifiedAtUtc =
            null;

        SupplyChainFingerprint =
            string.Empty;

        Comment =
            string.Empty;

        Status =
            ProblemClassificationStatus.NotAnalyzed;

        NotifyMatchProperties();

        OnPropertyChanged(
            nameof(UnclassifiedFeatureCodes));

        OnPropertyChanged(
            nameof(Warnings));

        OnPropertyChanged(
            nameof(Errors));

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string primaryDescription =
            HasPrimaryProblemType
                ? PrimaryProblemTypeCode
                : "no primary family";

        return
            $"{Status}; {primaryDescription}; " +
            $"{MatchCount} match(es)";
    }

    private static string NormalizeProblemTypeCode(
        string? value)
    {
        return value?
            .Trim()
            .ToUpperInvariant() ??
            string.Empty;
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

    private void NotifyPrimaryMatchProperties()
    {
        OnPropertyChanged(
            nameof(HasPrimaryProblemType));

        OnPropertyChanged(
            nameof(HasPrimaryProblemTypeName));

        OnPropertyChanged(
            nameof(PrimaryMatch));

        OnPropertyChanged(
            nameof(CanBeUsedForMethodSelection));
    }

    private void NotifyMatchProperties()
    {
        OnPropertyChanged(
            nameof(Matches));

        OnPropertyChanged(
            nameof(MatchCount));

        OnPropertyChanged(
            nameof(DirectMatchCount));

        OnPropertyChanged(
            nameof(ExactMatchCount));

        OnPropertyChanged(
            nameof(RelaxationMatchCount));

        OnPropertyChanged(
            nameof(SubproblemMatchCount));

        OnPropertyChanged(
            nameof(HasMatches));

        OnPropertyChanged(
            nameof(HasDirectMatches));

        OnPropertyChanged(
            nameof(PrimaryMatch));

        OnPropertyChanged(
            nameof(CanBeUsedForMethodSelection));
    }

    private void NotifyDerivedProperties()
    {
        OnPropertyChanged(
            nameof(IsOutdated));

        OnPropertyChanged(
            nameof(IsInvalid));

        OnPropertyChanged(
            nameof(CanBeUsedForMethodSelection));
    }
}