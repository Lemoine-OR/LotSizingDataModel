using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Recommendation;

/// <summary>
/// Stores the solution-method recommendations generated for
/// one lot-sizing problem instance.
/// </summary>
/// <remarks>
/// The report preserves:
/// <list type="bullet">
/// <item>
/// <description>
/// the advisor and catalog versions used during evaluation;
/// </description>
/// </item>
/// <item>
/// <description>
/// the fingerprint of the evaluated supply chain;
/// </description>
/// </item>
/// <item>
/// <description>
/// the recommendation score threshold;
/// </description>
/// </item>
/// <item>
/// <description>
/// the ranked solution-method recommendations.
/// </description>
/// </item>
/// </list>
///
/// A report becomes potentially outdated when the current
/// supply-chain fingerprint differs from
/// <see cref="SupplyChainFingerprint"/>.
/// </remarks>
[Serializable]
[XmlType(TypeName = "solutionMethodRecommendationReport")]
public sealed class SolutionMethodRecommendationReport :
    ModelObject
{
    private string _advisorVersion =
        string.Empty;

    private string _catalogName =
        string.Empty;

    private string _catalogVersion =
        string.Empty;

    private string _supplyChainFingerprint =
        string.Empty;

    private DateTime? _generatedAtUtc;

    private double _recommendedScoreThreshold =
        SolutionMethodAdvisor
            .DefaultRecommendedScoreThreshold;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty solution-method recommendation
    /// report.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public SolutionMethodRecommendationReport()
    {
    }

    /// <summary>
    /// Initializes a solution-method recommendation report.
    /// </summary>
    /// <param name="advisorVersion">
    /// Version of the advisor used to produce the report.
    /// </param>
    /// <param name="catalogName">
    /// Name of the solution-method catalog.
    /// </param>
    /// <param name="catalogVersion">
    /// Version of the solution-method catalog.
    /// </param>
    /// <param name="supplyChainFingerprint">
    /// Fingerprint of the evaluated supply chain.
    /// </param>
    /// <param name="recommendedScoreThreshold">
    /// Score threshold used to mark directly compatible
    /// methods as recommended.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when a required textual value is empty.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when
    /// <paramref name="recommendedScoreThreshold"/> does not
    /// lie between zero and one.
    /// </exception>
    public SolutionMethodRecommendationReport(
        string advisorVersion,
        string catalogName,
        string catalogVersion,
        string supplyChainFingerprint,
        double recommendedScoreThreshold =
            SolutionMethodAdvisor
                .DefaultRecommendedScoreThreshold)
    {
        if (string.IsNullOrWhiteSpace(advisorVersion))
        {
            throw new ArgumentException(
                "An advisor version is required.",
                nameof(advisorVersion));
        }

        if (string.IsNullOrWhiteSpace(catalogName))
        {
            throw new ArgumentException(
                "A solution-method catalog name is required.",
                nameof(catalogName));
        }

        if (string.IsNullOrWhiteSpace(catalogVersion))
        {
            throw new ArgumentException(
                "A solution-method catalog version is " +
                "required.",
                nameof(catalogVersion));
        }

        if (string.IsNullOrWhiteSpace(
                supplyChainFingerprint))
        {
            throw new ArgumentException(
                "A supply-chain fingerprint is required.",
                nameof(supplyChainFingerprint));
        }

        AdvisorVersion =
            advisorVersion;

        CatalogName =
            catalogName;

        CatalogVersion =
            catalogVersion;

        SupplyChainFingerprint =
            supplyChainFingerprint;

        RecommendedScoreThreshold =
            recommendedScoreThreshold;

        GeneratedAtUtc =
            DateTime.UtcNow;
    }

    /// <summary>
    /// Gets or sets the version of the advisor used to produce
    /// the recommendations.
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

                NotifyValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the name of the solution-method catalog
    /// used during evaluation.
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

                NotifyValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the solution-method catalog
    /// used during evaluation.
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

                NotifyValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the fingerprint of the supply chain that
    /// was evaluated.
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

                NotifyValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the report
    /// was generated.
    /// </summary>
    [XmlElement("generatedAtUtc", IsNullable = true)]
    public DateTime? GeneratedAtUtc
    {
        get => _generatedAtUtc;
        set
        {
            DateTime? normalizedValue =
                value.HasValue
                    ? ConvertToUtc(value.Value)
                    : null;

            if (SetProperty(
                    ref _generatedAtUtc,
                    normalizedValue))
            {
                OnPropertyChanged(
                    nameof(HasGenerationDate));

                OnPropertyChanged(
                    nameof(HasBeenGenerated));

                NotifyValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the score threshold used to mark a
    /// directly compatible method as recommended.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied value is not finite or does
    /// not lie between zero and one.
    /// </exception>
    [XmlAttribute("recommendedScoreThreshold")]
    public double RecommendedScoreThreshold
    {
        get => _recommendedScoreThreshold;
        set
        {
            if (!double.IsFinite(value) ||
                value < 0.0 ||
                value > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The recommended-score threshold must " +
                    "be finite and lie between zero and one.");
            }

            if (SetProperty(
                    ref _recommendedScoreThreshold,
                    value))
            {
                NotifyValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets the ranked solution-method recommendations.
    /// </summary>
    [XmlArray("recommendations")]
    [XmlArrayItem("recommendation")]
    public List<SolutionMethodRecommendation>
        Recommendations
    { get; } =
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
    /// Gets a value indicating whether an advisor version has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasAdvisorVersion =>
        !string.IsNullOrWhiteSpace(
            AdvisorVersion);

    /// <summary>
    /// Gets a value indicating whether complete catalog
    /// information has been recorded.
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
    /// Gets a value indicating whether a generation date has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasGenerationDate =>
        GeneratedAtUtc.HasValue;

    /// <summary>
    /// Gets a value indicating whether this report has been
    /// generated.
    /// </summary>
    [XmlIgnore]
    public bool HasBeenGenerated =>
        HasGenerationDate &&
        HasAdvisorVersion &&
        HasCatalogInformation &&
        HasSupplyChainFingerprint;

    /// <summary>
    /// Gets a value indicating whether at least one
    /// recommendation has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasRecommendations =>
        Recommendations.Count > 0;

    /// <summary>
    /// Gets the total number of recommendations.
    /// </summary>
    [XmlIgnore]
    public int RecommendationCount =>
        Recommendations.Count;

    /// <summary>
    /// Gets the number of evaluated recommendations.
    /// </summary>
    [XmlIgnore]
    public int EvaluatedRecommendationCount =>
        Recommendations.Count(
            recommendation =>
                recommendation is not null &&
                recommendation.HasBeenEvaluated);

    /// <summary>
    /// Gets the number of methods marked as recommended.
    /// </summary>
    [XmlIgnore]
    public int RecommendedMethodCount =>
        Recommendations.Count(
            recommendation =>
                recommendation is not null &&
                recommendation.IsRecommended);

    /// <summary>
    /// Gets the number of directly compatible methods,
    /// including methods marked as recommended.
    /// </summary>
    [XmlIgnore]
    public int CompatibleMethodCount =>
        Recommendations.Count(
            recommendation =>
                recommendation is not null &&
                recommendation.IsCompatible);

    /// <summary>
    /// Gets the number of partially compatible methods.
    /// </summary>
    [XmlIgnore]
    public int PartiallyCompatibleMethodCount =>
        Recommendations.Count(
            recommendation =>
                recommendation is not null &&
                recommendation.IsPartiallyCompatible);

    /// <summary>
    /// Gets the number of incompatible methods.
    /// </summary>
    [XmlIgnore]
    public int IncompatibleMethodCount =>
        Recommendations.Count(
            recommendation =>
                recommendation is not null &&
                recommendation.IsIncompatible);

    /// <summary>
    /// Gets a value indicating whether at least one directly
    /// compatible method exists.
    /// </summary>
    [XmlIgnore]
    public bool HasDirectlyCompatibleMethod =>
        CompatibleMethodCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one method is
    /// directly recommended.
    /// </summary>
    [XmlIgnore]
    public bool HasRecommendedMethod =>
        RecommendedMethodCount > 0;

    /// <summary>
    /// Gets a value indicating whether at least one method can
    /// be applied directly or after adaptation.
    /// </summary>
    [XmlIgnore]
    public bool HasApplicableMethod =>
        CompatibleMethodCount > 0 ||
        PartiallyCompatibleMethodCount > 0;

    /// <summary>
    /// Gets the best applicable recommendation.
    /// </summary>
    /// <remarks>
    /// Directly recommended and compatible methods are
    /// preferred to partially compatible methods.
    ///
    /// Incompatible and non-evaluated methods are never
    /// returned by this property.
    /// </remarks>
    [XmlIgnore]
    public SolutionMethodRecommendation?
        BestRecommendation =>
            Recommendations
                .Where(
                    recommendation =>
                        recommendation is not null &&
                        (
                            recommendation.IsCompatible ||
                            recommendation
                                .IsPartiallyCompatible
                        ))
                .OrderBy(
                    recommendation =>
                        recommendation.Rank ??
                        int.MaxValue)
                .ThenByDescending(
                    recommendation =>
                        recommendation
                            .CompatibilityLevel)
                .ThenByDescending(
                    recommendation =>
                        recommendation.Score)
                .ThenBy(
                    recommendation =>
                        recommendation.MethodCode,
                    StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

    /// <summary>
    /// Gets a value indicating whether a best applicable
    /// recommendation exists.
    /// </summary>
    [XmlIgnore]
    public bool HasBestRecommendation =>
        BestRecommendation is not null;

    /// <summary>
    /// Gets a value indicating whether an explanatory comment
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    /// <summary>
    /// Gets a value indicating whether the report is
    /// structurally valid.
    /// </summary>
    [XmlIgnore]
    public bool IsValidReport =>
        Validate().Count == 0;

    /// <summary>
    /// Adds a recommendation to the report.
    /// </summary>
    /// <param name="recommendation">
    /// Recommendation to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="recommendation"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the recommendation is invalid or another
    /// recommendation uses the same method code.
    /// </exception>
    public void AddRecommendation(
        SolutionMethodRecommendation recommendation)
    {
        ArgumentNullException.ThrowIfNull(
            recommendation);

        if (!recommendation.IsValidRecommendation)
        {
            throw new ArgumentException(
                "The solution-method recommendation is " +
                "invalid.",
                nameof(recommendation));
        }

        if (ContainsMethodCode(
                recommendation.MethodCode))
        {
            throw new ArgumentException(
                $"A recommendation already exists for method " +
                $"'{recommendation.MethodCode}'.",
                nameof(recommendation));
        }

        Recommendations.Add(
            recommendation);

        RefreshRanks();

        NotifyRecommendationProperties();
    }

    /// <summary>
    /// Replaces all recommendations contained in the report.
    /// </summary>
    /// <param name="recommendations">
    /// New recommendation collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="recommendations"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null or invalid
    /// recommendation, or duplicate method codes.
    /// </exception>
    public void ReplaceRecommendations(
        IEnumerable<SolutionMethodRecommendation>
            recommendations)
    {
        ArgumentNullException.ThrowIfNull(
            recommendations);

        SolutionMethodRecommendation[] materialized =
            recommendations.ToArray();

        if (materialized.Any(
                recommendation =>
                    recommendation is null))
        {
            throw new ArgumentException(
                "The recommendation collection cannot " +
                "contain a null element.",
                nameof(recommendations));
        }

        SolutionMethodRecommendation? invalidRecommendation =
            materialized.FirstOrDefault(
                recommendation =>
                    !recommendation
                        .IsValidRecommendation);

        if (invalidRecommendation is not null)
        {
            throw new ArgumentException(
                $"Recommendation for method " +
                $"'{invalidRecommendation.MethodCode}' is " +
                "invalid.",
                nameof(recommendations));
        }

        string[] duplicateMethodCodes =
            materialized
                .GroupBy(
                    recommendation =>
                        recommendation.MethodCode,
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    methodCode =>
                        methodCode,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (duplicateMethodCodes.Length > 0)
        {
            throw new ArgumentException(
                "Duplicate recommendation method codes: " +
                string.Join(
                    ", ",
                    duplicateMethodCodes) +
                ".",
                nameof(recommendations));
        }

        Recommendations.Clear();

        Recommendations.AddRange(
            materialized);

        RefreshRanks();

        NotifyRecommendationProperties();
    }

    /// <summary>
    /// Finds the recommendation associated with a method code.
    /// </summary>
    /// <param name="methodCode">
    /// Method code to search for.
    /// </param>
    /// <returns>
    /// Matching recommendation, or <see langword="null"/> when
    /// no recommendation uses the supplied code.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when several recommendations use the same
    /// method code.
    /// </exception>
    public SolutionMethodRecommendation? FindRecommendation(
        string methodCode)
    {
        if (string.IsNullOrWhiteSpace(methodCode))
        {
            return null;
        }

        string normalizedMethodCode =
            methodCode.Trim();

        SolutionMethodRecommendation[] matches =
            Recommendations
                .Where(
                    recommendation =>
                        recommendation is not null &&
                        string.Equals(
                            recommendation.MethodCode,
                            normalizedMethodCode,
                            StringComparison.OrdinalIgnoreCase))
                .Take(2)
                .ToArray();

        return matches.Length switch
        {
            0 =>
                null,

            1 =>
                matches[0],

            _ =>
                throw new InvalidOperationException(
                    $"Method code '{methodCode}' is " +
                    "ambiguous in the recommendation report.")
        };
    }

    /// <summary>
    /// Determines whether a recommendation exists for a
    /// method code.
    /// </summary>
    /// <param name="methodCode">
    /// Method code to search for.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a matching recommendation
    /// exists; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ContainsMethodCode(
        string methodCode)
    {
        return FindRecommendation(
            methodCode) is not null;
    }

    /// <summary>
    /// Removes a recommendation from the report.
    /// </summary>
    /// <param name="methodCode">
    /// Code of the method whose recommendation must be
    /// removed.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a recommendation was
    /// removed; otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveRecommendation(
        string methodCode)
    {
        SolutionMethodRecommendation? recommendation =
            FindRecommendation(
                methodCode);

        if (recommendation is null)
        {
            return false;
        }

        bool removed =
            Recommendations.Remove(
                recommendation);

        if (removed)
        {
            RefreshRanks();

            NotifyRecommendationProperties();
        }

        return removed;
    }

    /// <summary>
    /// Removes every recommendation while preserving the
    /// report-generation metadata.
    /// </summary>
    public void ClearRecommendations()
    {
        if (Recommendations.Count == 0)
        {
            return;
        }

        Recommendations.Clear();

        NotifyRecommendationProperties();
    }

    /// <summary>
    /// Clears the recommendations and all generation metadata.
    /// </summary>
    public void ClearReport()
    {
        Recommendations.Clear();

        AdvisorVersion =
            string.Empty;

        CatalogName =
            string.Empty;

        CatalogVersion =
            string.Empty;

        SupplyChainFingerprint =
            string.Empty;

        GeneratedAtUtc =
            null;

        RecommendedScoreThreshold =
            SolutionMethodAdvisor
                .DefaultRecommendedScoreThreshold;

        Comment =
            string.Empty;

        NotifyRecommendationProperties();
    }

    /// <summary>
    /// Reorders the recommendations and assigns sequential
    /// ranks starting at one.
    /// </summary>
    public void RefreshRanks()
    {
        SolutionMethodRecommendation[] ordered =
            Recommendations
                .Where(
                    recommendation =>
                        recommendation is not null)
                .OrderByDescending(
                    recommendation =>
                        recommendation
                            .CompatibilityLevel)
                .ThenByDescending(
                    recommendation =>
                        recommendation.Score)
                .ThenBy(
                    recommendation =>
                        recommendation.MethodName,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    recommendation =>
                        recommendation.MethodCode,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        Recommendations.Clear();

        for (int index = 0;
             index < ordered.Length;
             index++)
        {
            ordered[index].Rank =
                index + 1;

            Recommendations.Add(
                ordered[index]);
        }

        NotifyRecommendationProperties();
    }

    /// <summary>
    /// Determines whether the report was generated for the
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
    /// Validates the structural consistency of the report.
    /// </summary>
    /// <returns>
    /// Ordered validation-error collection. An empty
    /// collection indicates that the report is valid.
    /// </returns>
    public IReadOnlyList<string> Validate()
    {
        var errors =
            new List<string>();

        if (HasRecommendations ||
            HasBeenGenerated)
        {
            if (!HasAdvisorVersion)
            {
                errors.Add(
                    "The recommendation report advisor " +
                    "version is missing.");
            }

            if (!HasCatalogInformation)
            {
                errors.Add(
                    "The recommendation report catalog " +
                    "information is incomplete.");
            }

            if (!HasSupplyChainFingerprint)
            {
                errors.Add(
                    "The recommendation report supply-chain " +
                    "fingerprint is missing.");
            }

            if (!HasGenerationDate)
            {
                errors.Add(
                    "The recommendation report generation " +
                    "date is missing.");
            }
        }

        ValidateRecommendations(
            errors);

        ValidateMethodCodeUniqueness(
            errors);

        ValidateRanks(
            errors);

        ValidateRecommendationMetadata(
            errors);

        return errors
            .Where(
                error =>
                    !string.IsNullOrWhiteSpace(error))
            .Select(
                error =>
                    error.Trim())
            .Distinct(
                StringComparer.Ordinal)
            .OrderBy(
                error =>
                    error,
                StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Validates the report and throws an exception when at
    /// least one error is found.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the report is invalid.
    /// </exception>
    public void EnsureValid()
    {
        IReadOnlyList<string> errors =
            Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "The solution-method recommendation report is " +
            "invalid:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                errors.Select(
                    error =>
                        "- " + error)));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string generationDescription =
            GeneratedAtUtc is DateTime generatedAtUtc
                ? generatedAtUtc.ToString(
                    "O",
                    CultureInfo.InvariantCulture)
                : "not generated";

        return
            $"{RecommendationCount} recommendation(s); " +
            $"{RecommendedMethodCount} recommended; " +
            $"{CompatibleMethodCount} compatible; " +
            $"{generationDescription}";
    }

    private void ValidateRecommendations(
        ICollection<string> errors)
    {
        for (int index = 0;
             index < Recommendations.Count;
             index++)
        {
            SolutionMethodRecommendation? recommendation =
                Recommendations[index];

            if (recommendation is null)
            {
                errors.Add(
                    $"Recommendation at index {index} is " +
                    "null.");

                continue;
            }

            if (!recommendation.IsValidRecommendation)
            {
                errors.Add(
                    $"Recommendation for method " +
                    $"'{DisplayMethodCode(recommendation)}' " +
                    "is invalid.");
            }
        }
    }

    private void ValidateMethodCodeUniqueness(
        ICollection<string> errors)
    {
        string[] duplicateMethodCodes =
            Recommendations
                .Where(
                    recommendation =>
                        recommendation is not null &&
                        recommendation.HasMethodCode)
                .GroupBy(
                    recommendation =>
                        recommendation.MethodCode,
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    methodCode =>
                        methodCode,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (duplicateMethodCodes.Length > 0)
        {
            errors.Add(
                "Duplicate recommendation method codes: " +
                string.Join(
                    ", ",
                    duplicateMethodCodes) +
                ".");
        }
    }

    private void ValidateRanks(
        ICollection<string> errors)
    {
        SolutionMethodRecommendation[] rankedRecommendations =
            Recommendations
                .Where(
                    recommendation =>
                        recommendation is not null &&
                        recommendation.Rank.HasValue)
                .ToArray();

        int[] duplicateRanks =
            rankedRecommendations
                .GroupBy(
                    recommendation =>
                        recommendation.Rank!.Value)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    rank =>
                        rank)
                .ToArray();

        if (duplicateRanks.Length > 0)
        {
            errors.Add(
                "Duplicate recommendation ranks: " +
                string.Join(
                    ", ",
                    duplicateRanks) +
                ".");
        }

        if (rankedRecommendations.Length > 0 &&
            rankedRecommendations.Length !=
            Recommendations.Count)
        {
            errors.Add(
                "Recommendation ranks are only partially " +
                "defined.");
        }
    }

    private void ValidateRecommendationMetadata(
        ICollection<string> errors)
    {
        foreach (SolutionMethodRecommendation recommendation
                 in Recommendations.Where(
                     recommendation =>
                         recommendation is not null))
        {
            if (recommendation
                    .HasSupplyChainFingerprint &&
                HasSupplyChainFingerprint &&
                !string.Equals(
                    recommendation
                        .SupplyChainFingerprint,
                    SupplyChainFingerprint,
                    StringComparison.Ordinal))
            {
                errors.Add(
                    $"Recommendation for method " +
                    $"'{DisplayMethodCode(recommendation)}' " +
                    "uses a different supply-chain " +
                    "fingerprint.");
            }

            if (recommendation
                    .HasAdvisorVersion &&
                HasAdvisorVersion &&
                !string.Equals(
                    recommendation.AdvisorVersion,
                    AdvisorVersion,
                    StringComparison.Ordinal))
            {
                errors.Add(
                    $"Recommendation for method " +
                    $"'{DisplayMethodCode(recommendation)}' " +
                    "uses a different advisor version.");
            }

            if (recommendation
                    .HasMethodCatalogInformation &&
                HasCatalogInformation &&
                (
                    !string.Equals(
                        recommendation.MethodCatalogName,
                        CatalogName,
                        StringComparison.Ordinal) ||
                    !string.Equals(
                        recommendation.MethodCatalogVersion,
                        CatalogVersion,
                        StringComparison.Ordinal)
                ))
            {
                errors.Add(
                    $"Recommendation for method " +
                    $"'{DisplayMethodCode(recommendation)}' " +
                    "uses different catalog information.");
            }
        }
    }

    private static string DisplayMethodCode(
        SolutionMethodRecommendation recommendation)
    {
        return recommendation.HasMethodCode
            ? recommendation.MethodCode
            : "<missing code>";
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

    private void NotifyRecommendationProperties()
    {
        OnPropertyChanged(
            nameof(Recommendations));

        OnPropertyChanged(
            nameof(HasRecommendations));

        OnPropertyChanged(
            nameof(RecommendationCount));

        OnPropertyChanged(
            nameof(EvaluatedRecommendationCount));

        OnPropertyChanged(
            nameof(RecommendedMethodCount));

        OnPropertyChanged(
            nameof(CompatibleMethodCount));

        OnPropertyChanged(
            nameof(PartiallyCompatibleMethodCount));

        OnPropertyChanged(
            nameof(IncompatibleMethodCount));

        OnPropertyChanged(
            nameof(HasDirectlyCompatibleMethod));

        OnPropertyChanged(
            nameof(HasRecommendedMethod));

        OnPropertyChanged(
            nameof(HasApplicableMethod));

        OnPropertyChanged(
            nameof(BestRecommendation));

        OnPropertyChanged(
            nameof(HasBestRecommendation));

        NotifyValidityProperties();
    }

    private void NotifyValidityProperties()
    {
        OnPropertyChanged(
            nameof(IsValidReport));
    }
}