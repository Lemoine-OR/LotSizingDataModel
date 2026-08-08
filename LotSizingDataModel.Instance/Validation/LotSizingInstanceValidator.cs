using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Creation;
using LotSizingDataModel.Instance.Metadata;
using LotSizingDataModel.Instance.Recommendation;
using LotSizingDataModel.Instance.Results;

namespace LotSizingDataModel.Instance.Validation;

/// <summary>
/// Validates the structural and referential consistency of a
/// complete lot-sizing problem instance.
/// </summary>
/// <remarks>
/// The validator checks instance-level information such as:
/// <list type="bullet">
/// <item>
/// <description>instance identity and metadata;</description>
/// </item>
/// <item>
/// <description>product-structure analysis consistency;</description>
/// </item>
/// <item>
/// <description>problem-classification consistency;</description>
/// </item>
/// <item>
/// <description>known-result identifiers and references;</description>
/// </item>
/// <item>
/// <description>best-known-result eligibility;</description>
/// </item>
/// <item>
/// <description>solution-method recommendation reports;</description>
/// </item>
/// <item>
/// <description>supply-chain fingerprints.</description>
/// </item>
/// </list>
///
/// An instance does not need to contain a known result or a
/// detailed solution in order to be valid.
///
/// Validation of the internal mathematical consistency of the
/// supply-chain model and validation of detailed solution
/// decisions remain the responsibility of their dedicated
/// validators.
/// </remarks>
public static class LotSizingInstanceValidator
{
    /// <summary>
    /// Validates a lot-sizing problem instance.
    /// </summary>
    /// <param name="instance">
    /// Instance to validate.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether the current supply-chain
    /// fingerprint must be calculated and compared with
    /// recorded fingerprints.
    /// </param>
    /// <returns>
    /// Ordered collection of validation errors. An empty
    /// collection indicates that the instance is structurally
    /// valid.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static IReadOnlyList<string> Validate(
        LotSizingInstance instance,
        bool validateCurrentFingerprint = true)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var errors =
            new List<string>();

        ValidateInstanceMetadata(
            instance,
            errors);

        ValidateProductStructure(
            instance.ProductStructure,
            errors);

        ValidateProblemClassification(
            instance.ProblemClassification,
            errors);

        ValidateRecommendationReport(
            instance,
            errors);

        Dictionary<string, KnownResult> knownResultsById =
            ValidateKnownResults(
                instance,
                errors);

        ValidateBestKnownResult(
            instance,
            knownResultsById,
            errors);

        ValidateSupersedingReferences(
            instance.KnownResults,
            knownResultsById,
            errors);

        ValidateSupersedingCycles(
            knownResultsById,
            errors);

        if (validateCurrentFingerprint)
        {
            ValidateFingerprints(
                instance,
                errors);
        }

        return NormalizeMessages(
            errors);
    }

    /// <summary>
    /// Returns non-fatal consistency and quality warnings for
    /// a lot-sizing problem instance.
    /// </summary>
    /// <param name="instance">
    /// Instance to examine.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether the current supply-chain
    /// fingerprint must be calculated and compared with
    /// fingerprints recorded by analyses, known results and
    /// solution-method recommendations.
    /// </param>
    /// <returns>
    /// Ordered collection of warning messages.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static IReadOnlyList<string> GetWarnings(
        LotSizingInstance instance,
        bool validateCurrentFingerprint = true)
    {
        ArgumentNullException.ThrowIfNull(instance);

        var warnings =
            new List<string>();

        if (!instance.HasName)
        {
            warnings.Add(
                "The instance does not have a human-readable " +
                "name.");
        }

        if (!instance.HasCreationDate)
        {
            warnings.Add(
                "The instance creation date has not been " +
                "recorded.");
        }

        if (!instance.HasSourceInformation)
        {
            warnings.Add(
                "No source information has been recorded for " +
                "the instance.");
        }

        AddProductStructureWarnings(
            instance.ProductStructure,
            warnings);

        AddClassificationWarnings(
            instance.ProblemClassification,
            warnings);

        AddRecommendationWarnings(
            instance,
            warnings);

        foreach (KnownResult result
                 in instance.KnownResults
                     .Where(result => result is not null))
        {
            AddKnownResultWarnings(
                result,
                warnings);
        }

        if (instance.HasEligibleBestKnownResult)
        {
            KnownResult? bestKnownResult =
                instance.BestKnownResult;

            if (bestKnownResult is not null &&
                bestKnownResult.VerificationStatus ==
                KnownResultVerificationStatus.NotVerified)
            {
                warnings.Add(
                    $"Best known result " +
                    $"'{bestKnownResult.ResultId}' has not " +
                    "been verified.");
            }

            if (bestKnownResult is not null &&
                bestKnownResult.VerificationStatus ==
                KnownResultVerificationStatus.SourceReported)
            {
                warnings.Add(
                    $"Best known result " +
                    $"'{bestKnownResult.ResultId}' is based " +
                    "only on information reported by its " +
                    "source.");
            }
        }

        if (validateCurrentFingerprint)
        {
            AddFingerprintWarnings(
                instance,
                warnings);
        }

        return NormalizeMessages(
            warnings);
    }

    /// <summary>
    /// Determines whether a lot-sizing problem instance is
    /// structurally valid.
    /// </summary>
    /// <param name="instance">
    /// Instance to validate.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether recorded fingerprints must
    /// be compared with the current supply-chain state.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when no validation error is
    /// found; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool IsValid(
        LotSizingInstance instance,
        bool validateCurrentFingerprint = true)
    {
        return Validate(
            instance,
            validateCurrentFingerprint).Count == 0;
    }

    /// <summary>
    /// Validates a lot-sizing instance and throws an
    /// exception when at least one error is found.
    /// </summary>
    /// <param name="instance">
    /// Instance to validate.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether recorded fingerprints must
    /// be compared with the current supply-chain state.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the instance is invalid.
    /// </exception>
    public static void EnsureValid(
        LotSizingInstance instance,
        bool validateCurrentFingerprint = true)
    {
        IReadOnlyList<string> errors =
            Validate(
                instance,
                validateCurrentFingerprint);

        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "The lot-sizing instance is invalid:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                errors.Select(
                    error =>
                        "- " + error)));
    }

    private static void ValidateInstanceMetadata(
        LotSizingInstance instance,
        ICollection<string> errors)
    {
        if (!instance.HasFormatVersion)
        {
            errors.Add(
                "The instance format version is missing.");
        }

        if (!instance.HasInstanceId)
        {
            errors.Add(
                "The instance identifier is missing.");
        }

        if (instance.SupplyChain is null)
        {
            errors.Add(
                "The supply-chain model is missing.");
        }

        if (instance.ProductStructure is null)
        {
            errors.Add(
                "The product-structure descriptor is " +
                "missing.");
        }

        if (instance.ProblemClassification is null)
        {
            errors.Add(
                "The problem-classification object is " +
                "missing.");
        }

        if (instance.CreatedAtUtc.HasValue &&
            instance.ModifiedAtUtc.HasValue &&
            instance.ModifiedAtUtc.Value <
            instance.CreatedAtUtc.Value)
        {
            errors.Add(
                "The instance modification date precedes its " +
                "creation date.");
        }
    }

    private static void ValidateProductStructure(
        ProductStructureDescriptor descriptor,
        ICollection<string> errors)
    {
        if (descriptor is null)
        {
            return;
        }

        ValidateItemIdentifierCollection(
            descriptor.RootItemIds,
            "product-structure root-item",
            errors);

        ValidateItemIdentifierCollection(
            descriptor.LeafItemIds,
            "product-structure leaf-item",
            errors);

        ValidateItemIdentifierCollection(
            descriptor.SharedComponentItemIds,
            "shared-component item",
            errors);

        if (descriptor.HasCycle &&
            descriptor.CheckStatus !=
                ProductStructureCheckStatus.Invalid)
        {
            errors.Add(
                "The product structure contains a cycle but " +
                "is not marked as invalid.");
        }

        if (descriptor.HasCycle &&
            descriptor.DetectedType !=
                ProductStructureType.Unknown)
        {
            errors.Add(
                "A cyclic product structure cannot have a " +
                "valid detected product-structure type.");
        }

        switch (descriptor.CheckStatus)
        {
            case ProductStructureCheckStatus.NotAnalyzed:
                ValidateNotAnalyzedProductStructure(
                    descriptor,
                    errors);
                break;

            case ProductStructureCheckStatus.DeclaredOnly:
                ValidateDeclaredOnlyProductStructure(
                    descriptor,
                    errors);
                break;

            case ProductStructureCheckStatus.DetectedOnly:
                ValidateDetectedOnlyProductStructure(
                    descriptor,
                    errors);
                break;

            case ProductStructureCheckStatus
                .DeclaredAndConfirmed:
                ValidateConfirmedProductStructure(
                    descriptor,
                    errors);
                break;

            case ProductStructureCheckStatus
                .DeclaredAndContradicted:
                ValidateContradictedProductStructure(
                    descriptor,
                    errors);
                break;

            case ProductStructureCheckStatus.Invalid:
                ValidateInvalidProductStructure(
                    descriptor,
                    errors);
                break;

            case ProductStructureCheckStatus.Outdated:
                ValidateOutdatedProductStructure(
                    descriptor,
                    errors);
                break;

            default:
                errors.Add(
                    $"Unsupported product-structure check " +
                    $"status '{descriptor.CheckStatus}'.");
                break;
        }
    }

    private static void ValidateNotAnalyzedProductStructure(
        ProductStructureDescriptor descriptor,
        ICollection<string> errors)
    {
        if (descriptor.HasDeclaredType)
        {
            errors.Add(
                "A declared product-structure type exists but " +
                "the descriptor status is NotAnalyzed instead " +
                "of DeclaredOnly.");
        }

        if (descriptor.HasDetectedType)
        {
            errors.Add(
                "A product-structure type has been detected " +
                "but the descriptor status is NotAnalyzed.");
        }

        if (descriptor.HasBeenAnalyzed)
        {
            errors.Add(
                "A product-structure analysis date exists but " +
                "the descriptor status is NotAnalyzed.");
        }
    }

    private static void ValidateDeclaredOnlyProductStructure(
        ProductStructureDescriptor descriptor,
        ICollection<string> errors)
    {
        if (!descriptor.HasDeclaredType)
        {
            errors.Add(
                "The product-structure status is DeclaredOnly " +
                "but no declared type is available.");
        }

        if (descriptor.HasDetectedType)
        {
            errors.Add(
                "The product-structure status is DeclaredOnly " +
                "but an automatically detected type exists.");
        }

        if (descriptor.HasBeenAnalyzed)
        {
            errors.Add(
                "The product-structure status is DeclaredOnly " +
                "but an automatic analysis date exists.");
        }
    }

    private static void ValidateDetectedOnlyProductStructure(
        ProductStructureDescriptor descriptor,
        ICollection<string> errors)
    {
        if (descriptor.HasDeclaredType)
        {
            errors.Add(
                "The product-structure status is DetectedOnly " +
                "but a declared type also exists.");
        }

        if (!descriptor.HasDetectedType)
        {
            errors.Add(
                "The product-structure status is DetectedOnly " +
                "but no detected type is available.");
        }

        ValidateCompletedProductStructureAnalysis(
            descriptor,
            errors);
    }

    private static void ValidateConfirmedProductStructure(
        ProductStructureDescriptor descriptor,
        ICollection<string> errors)
    {
        if (!descriptor.HasDeclaredType ||
            !descriptor.HasDetectedType)
        {
            errors.Add(
                "A confirmed product structure requires both " +
                "declared and detected types.");
        }

        if (descriptor.DeclaredType !=
            descriptor.DetectedType)
        {
            errors.Add(
                "The product structure is marked as confirmed " +
                "but the declared and detected types differ.");
        }

        ValidateCompletedProductStructureAnalysis(
            descriptor,
            errors);
    }

    private static void ValidateContradictedProductStructure(
        ProductStructureDescriptor descriptor,
        ICollection<string> errors)
    {
        if (!descriptor.HasDeclaredType ||
            !descriptor.HasDetectedType)
        {
            errors.Add(
                "A contradicted product structure requires " +
                "both declared and detected types.");
        }

        if (descriptor.DeclaredType ==
            descriptor.DetectedType)
        {
            errors.Add(
                "The product structure is marked as " +
                "contradicted but the declared and detected " +
                "types are identical.");
        }

        ValidateCompletedProductStructureAnalysis(
            descriptor,
            errors);
    }

    private static void ValidateInvalidProductStructure(
        ProductStructureDescriptor descriptor,
        ICollection<string> errors)
    {
        if (descriptor.DetectedType !=
            ProductStructureType.Unknown)
        {
            errors.Add(
                "An invalid product structure must not retain " +
                "a valid detected structure type.");
        }

        if (!descriptor.HasBeenAnalyzed)
        {
            errors.Add(
                "The product structure is marked as invalid " +
                "but no analysis date has been recorded.");
        }
    }

    private static void ValidateOutdatedProductStructure(
        ProductStructureDescriptor descriptor,
        ICollection<string> errors)
    {
        if (!descriptor.HasBeenAnalyzed &&
            !descriptor.HasDetectedType)
        {
            errors.Add(
                "The product structure is marked as outdated " +
                "but no previous automatic analysis exists.");
        }
    }

    private static void
        ValidateCompletedProductStructureAnalysis(
            ProductStructureDescriptor descriptor,
            ICollection<string> errors)
    {
        if (!descriptor.HasBeenAnalyzed)
        {
            errors.Add(
                "A completed product-structure analysis must " +
                "have an analysis date.");
        }

        if (string.IsNullOrWhiteSpace(
                descriptor.AnalyzerVersion))
        {
            errors.Add(
                "A completed product-structure analysis must " +
                "record its analyzer version.");
        }
    }

    private static void ValidateProblemClassification(
        LotSizingProblemClassification classification,
        ICollection<string> errors)
    {
        if (classification is null)
        {
            return;
        }

        if (classification.Features is null)
        {
            errors.Add(
                "The problem classification does not contain " +
                "a feature profile.");

            return;
        }

        ValidateClassificationMatches(
            classification,
            errors);

        switch (classification.Status)
        {
            case ProblemClassificationStatus.NotAnalyzed:
                ValidateNotAnalyzedClassification(
                    classification,
                    errors);
                break;

            case ProblemClassificationStatus.Classified:
                ValidateClassifiedClassification(
                    classification,
                    errors);
                break;

            case ProblemClassificationStatus
                .PartiallyClassified:
                ValidatePartiallyClassifiedClassification(
                    classification,
                    errors);
                break;

            case ProblemClassificationStatus.Ambiguous:
                ValidateAmbiguousClassification(
                    classification,
                    errors);
                break;

            case ProblemClassificationStatus.Unclassified:
                ValidateUnclassifiedClassification(
                    classification,
                    errors);
                break;

            case ProblemClassificationStatus.Invalid:
                ValidateInvalidClassification(
                    classification,
                    errors);
                break;

            case ProblemClassificationStatus.Outdated:
                ValidateOutdatedClassification(
                    classification,
                    errors);
                break;

            default:
                errors.Add(
                    $"Unsupported problem-classification " +
                    $"status '{classification.Status}'.");
                break;
        }
    }

    private static void ValidateClassificationMatches(
        LotSizingProblemClassification classification,
        ICollection<string> errors)
    {
        for (int index = 0;
             index < classification.Matches.Count;
             index++)
        {
            KnownProblemTypeMatch? match =
                classification.Matches[index];

            if (match is null)
            {
                errors.Add(
                    $"Problem-classification match at index " +
                    $"{index} is null.");

                continue;
            }

            if (!match.HasProblemTypeCode)
            {
                errors.Add(
                    $"Problem-classification match at index " +
                    $"{index} has no problem-family code.");
            }

            if (!double.IsFinite(match.Score) ||
                match.Score < 0.0 ||
                match.Score > 1.0)
            {
                errors.Add(
                    $"Problem-classification match " +
                    $"'{DisplayProblemTypeCode(match)}' has " +
                    "an invalid score.");
            }

            if (match.MatchKind ==
                    ProblemMatchKind.Exact &&
                match.HasBlockingMismatches)
            {
                errors.Add(
                    $"Exact problem-family match " +
                    $"'{DisplayProblemTypeCode(match)}' " +
                    "contains at least one blocking " +
                    "mismatch.");
            }

            if (match.MatchKind ==
                    ProblemMatchKind.Unknown)
            {
                errors.Add(
                    $"Problem-family match " +
                    $"'{DisplayProblemTypeCode(match)}' has " +
                    "an unknown match kind.");
            }

            if (match.Scope ==
                ProblemClassificationScope.Unknown)
            {
                errors.Add(
                    $"Problem-family match " +
                    $"'{DisplayProblemTypeCode(match)}' has " +
                    "an unknown classification scope.");
            }

            ValidateStringCollection(
                match.AffectedEntityKeys,
                $"affected entity key of match " +
                $"'{DisplayProblemTypeCode(match)}'",
                errors);

            ValidateStringCollection(
                match.AdditionalFeatureCodes,
                $"additional feature code of match " +
                $"'{DisplayProblemTypeCode(match)}'",
                errors);

            ValidateEvidence(
                match,
                errors);
        }

        if (classification.HasPrimaryProblemType &&
            classification.PrimaryMatch is null)
        {
            errors.Add(
                $"Primary problem-family code " +
                $"'{classification.PrimaryProblemTypeCode}' " +
                "does not reference a reported match.");
        }
    }

    private static void ValidateEvidence(
        KnownProblemTypeMatch match,
        ICollection<string> errors)
    {
        for (int index = 0;
             index < match.Evidence.Count;
             index++)
        {
            ClassificationEvidence? evidence =
                match.Evidence[index];

            if (evidence is null)
            {
                errors.Add(
                    $"Evidence at index {index} of match " +
                    $"'{DisplayProblemTypeCode(match)}' is " +
                    "null.");

                continue;
            }

            if (string.IsNullOrWhiteSpace(
                    evidence.FeatureCode))
            {
                errors.Add(
                    $"Evidence at index {index} of match " +
                    $"'{DisplayProblemTypeCode(match)}' has " +
                    "no feature code.");
            }

            if (!double.IsFinite(evidence.Weight) ||
                evidence.Weight < 0.0)
            {
                errors.Add(
                    $"Evidence at index {index} of match " +
                    $"'{DisplayProblemTypeCode(match)}' has " +
                    "an invalid weight.");
            }
        }
    }

    private static void ValidateNotAnalyzedClassification(
        LotSizingProblemClassification classification,
        ICollection<string> errors)
    {
        if (classification.HasBeenAnalyzed)
        {
            errors.Add(
                "The problem classification is NotAnalyzed " +
                "but a classification date is present.");
        }

        if (classification.HasMatches)
        {
            errors.Add(
                "The problem classification is NotAnalyzed " +
                "but problem-family matches are present.");
        }

        if (classification.HasPrimaryProblemType)
        {
            errors.Add(
                "The problem classification is NotAnalyzed " +
                "but a primary problem family is present.");
        }
    }

    private static void ValidateClassifiedClassification(
        LotSizingProblemClassification classification,
        ICollection<string> errors)
    {
        ValidateCompletedClassificationMetadata(
            classification,
            errors);

        if (!classification.HasPrimaryProblemType)
        {
            errors.Add(
                "A classified problem must have a primary " +
                "problem-family code.");
        }

        if (classification.PrimaryMatch is null)
        {
            errors.Add(
                "A classified problem must have a valid " +
                "primary problem-family match.");
        }
        else if (!classification.PrimaryMatch.IsExactMatch)
        {
            errors.Add(
                "A fully classified problem must use an exact " +
                "primary problem-family match.");
        }

        if (classification.HasUnclassifiedFeatures)
        {
            errors.Add(
                "A fully classified problem cannot contain " +
                "globally unclassified features.");
        }

        if (classification.HasErrors)
        {
            errors.Add(
                "A fully classified problem cannot contain " +
                "classification errors.");
        }
    }

    private static void
        ValidatePartiallyClassifiedClassification(
            LotSizingProblemClassification classification,
            ICollection<string> errors)
    {
        ValidateCompletedClassificationMetadata(
            classification,
            errors);

        if (!classification.HasMatches)
        {
            errors.Add(
                "A partially classified problem must contain " +
                "at least one problem-family match.");
        }

        if (!classification.HasPrimaryProblemType &&
            !classification.Matches.Any(
                match =>
                    match is not null &&
                    (
                        match.MatchKind ==
                            ProblemMatchKind
                                .RecognizedRelaxation ||
                        match.MatchKind ==
                            ProblemMatchKind
                                .RecognizedSubproblem
                    )))
        {
            errors.Add(
                "A partially classified problem must either " +
                "have a primary family or contain a " +
                "recognized relaxation or subproblem.");
        }
    }

    private static void ValidateAmbiguousClassification(
        LotSizingProblemClassification classification,
        ICollection<string> errors)
    {
        ValidateCompletedClassificationMetadata(
            classification,
            errors);

        if (classification.HasPrimaryProblemType)
        {
            errors.Add(
                "An ambiguous classification must not select " +
                "a primary problem family.");
        }

        int candidateCount =
            classification.Matches.Count(
                match =>
                    match is not null &&
                    match.IsDirectMatch &&
                    match.AppliesToCompleteProblem);

        if (candidateCount < 2)
        {
            errors.Add(
                "An ambiguous classification must contain at " +
                "least two direct complete-problem matches.");
        }
    }

    private static void ValidateUnclassifiedClassification(
        LotSizingProblemClassification classification,
        ICollection<string> errors)
    {
        ValidateCompletedClassificationMetadata(
            classification,
            errors);

        if (classification.HasPrimaryProblemType)
        {
            errors.Add(
                "An unclassified problem must not have a " +
                "primary problem-family code.");
        }

        if (classification.Matches.Any(
                match =>
                    match is not null &&
                    match.IsDirectMatch &&
                    match.AppliesToCompleteProblem))
        {
            errors.Add(
                "An unclassified problem cannot contain a " +
                "direct complete-problem match.");
        }
    }

    private static void ValidateInvalidClassification(
        LotSizingProblemClassification classification,
        ICollection<string> errors)
    {
        if (!classification.HasErrors)
        {
            errors.Add(
                "An invalid problem classification must " +
                "contain at least one explanatory error.");
        }
    }

    private static void ValidateOutdatedClassification(
        LotSizingProblemClassification classification,
        ICollection<string> errors)
    {
        if (!classification.HasBeenAnalyzed &&
            !classification.HasMatches)
        {
            errors.Add(
                "The problem classification is marked as " +
                "outdated but no previous classification " +
                "result exists.");
        }
    }

    private static void
        ValidateCompletedClassificationMetadata(
            LotSizingProblemClassification classification,
            ICollection<string> errors)
    {
        if (!classification.HasBeenAnalyzed)
        {
            errors.Add(
                "A completed problem classification must " +
                "record its classification date.");
        }

        if (!classification.HasClassifierVersion)
        {
            errors.Add(
                "A completed problem classification must " +
                "record its classifier version.");
        }

        if (!classification.HasCatalogInformation)
        {
            errors.Add(
                "A completed problem classification must " +
                "record its catalog name and version.");
        }
    }

    private static void ValidateRecommendationReport(
        LotSizingInstance instance,
        ICollection<string> errors)
    {
        if (!instance.HasSolutionMethodRecommendationReport)
        {
            return;
        }

        SolutionMethodRecommendationReport report =
            instance.SolutionMethodRecommendationReport;

        IReadOnlyList<string> reportErrors =
            report.Validate();

        foreach (string reportError in reportErrors)
        {
            errors.Add(
                "Solution-method recommendation report: " +
                reportError);
        }
    }

    private static void AddRecommendationWarnings(
        LotSizingInstance instance,
        ICollection<string> warnings)
    {
        if (!instance.HasSolutionMethodRecommendationReport)
        {
            return;
        }

        SolutionMethodRecommendationReport report =
            instance.SolutionMethodRecommendationReport;

        if (report.HasBeenGenerated &&
            !report.HasRecommendations)
        {
            warnings.Add(
                "The solution-method recommendation report " +
                "was generated but contains no " +
                "recommendation.");
        }

        if (report.HasRecommendations &&
            !report.HasApplicableMethod)
        {
            warnings.Add(
                "The solution-method recommendation report " +
                "does not identify an applicable method.");
        }

        foreach (SolutionMethodRecommendation recommendation
                 in report.Recommendations.Where(
                     recommendation =>
                         recommendation is not null))
        {
            if (!recommendation.HasBeenEvaluated)
            {
                warnings.Add(
                    $"Method recommendation " +
                    $"'{recommendation.MethodCode}' has not " +
                    "been evaluated.");
            }

            foreach (string recommendationWarning
                     in recommendation.Warnings)
            {
                if (!string.IsNullOrWhiteSpace(
                        recommendationWarning))
                {
                    warnings.Add(
                        $"Method '{recommendation.MethodCode}': " +
                        recommendationWarning.Trim());
                }
            }
        }
    }

    private static Dictionary<string, KnownResult>
        ValidateKnownResults(
            LotSizingInstance instance,
            ICollection<string> errors)
    {
        var knownResultsById =
            new Dictionary<string, KnownResult>(
                StringComparer.OrdinalIgnoreCase);

        for (int index = 0;
             index < instance.KnownResults.Count;
             index++)
        {
            KnownResult? result =
                instance.KnownResults[index];

            if (result is null)
            {
                errors.Add(
                    $"Known result at index {index} is null.");

                continue;
            }

            if (!result.HasResultId)
            {
                errors.Add(
                    $"Known result at index {index} has no " +
                    "stable identifier.");

                continue;
            }

            if (!knownResultsById.TryAdd(
                    result.ResultId,
                    result))
            {
                errors.Add(
                    $"Known-result identifier " +
                    $"'{result.ResultId}' is used more than " +
                    "once.");
            }

            ValidateKnownResult(
                result,
                errors);
        }

        return knownResultsById;
    }

    private static void ValidateKnownResult(
        KnownResult result,
        ICollection<string> errors)
    {
        bool hasMeaningfulInformation =
            result.HasReportedObjectiveValue ||
            result.HasDetailedSolution ||
            result.HasFeasibilityInformation ||
            result.HasOptimalityInformation ||
            result.HasMethodInformation ||
            result.HasSourceInformation ||
            result.HasComment;

        if (!hasMeaningfulInformation)
        {
            errors.Add(
                $"Known result '{result.ResultId}' does not " +
                "contain any result, claim, source or " +
                "descriptive information.");
        }

        if (result.IsSuperseded &&
            string.Equals(
                result.ResultId,
                result.SupersededByResultId,
                StringComparison.OrdinalIgnoreCase))
        {
            errors.Add(
                $"Known result '{result.ResultId}' cannot " +
                "supersede itself.");
        }

        if (result.ReportedObjectiveValue.HasValue &&
            !double.IsFinite(
                result.ReportedObjectiveValue.Value))
        {
            errors.Add(
                $"Known result '{result.ResultId}' contains " +
                "a non-finite objective value.");
        }
    }

    private static void ValidateBestKnownResult(
        LotSizingInstance instance,
        IReadOnlyDictionary<string, KnownResult>
            knownResultsById,
        ICollection<string> errors)
    {
        if (!instance.HasBestKnownResultId)
        {
            return;
        }

        if (!knownResultsById.TryGetValue(
                instance.BestKnownResultId,
                out KnownResult? bestKnownResult))
        {
            errors.Add(
                $"Best-known-result identifier " +
                $"'{instance.BestKnownResultId}' does not " +
                "reference a known result.");

            return;
        }

        if (!bestKnownResult
                .CanBeSelectedAsBestKnownResult)
        {
            errors.Add(
                $"Known result '{bestKnownResult.ResultId}' " +
                "is not eligible to be selected as the best " +
                "known result.");
        }
    }

    private static void ValidateSupersedingReferences(
        IEnumerable<KnownResult> knownResults,
        IReadOnlyDictionary<string, KnownResult>
            knownResultsById,
        ICollection<string> errors)
    {
        foreach (KnownResult result
                 in knownResults.Where(
                     result =>
                         result is not null &&
                         result.IsSuperseded))
        {
            if (!knownResultsById.ContainsKey(
                    result.SupersededByResultId))
            {
                errors.Add(
                    $"Known result '{result.ResultId}' is " +
                    $"superseded by unknown result " +
                    $"'{result.SupersededByResultId}'.");
            }
        }
    }

    private static void ValidateSupersedingCycles(
        IReadOnlyDictionary<string, KnownResult>
            knownResultsById,
        ICollection<string> errors)
    {
        var reportedCycles =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (KnownResult startResult
                 in knownResultsById.Values)
        {
            var positionByResultId =
                new Dictionary<string, int>(
                    StringComparer.OrdinalIgnoreCase);

            var path =
                new List<string>();

            KnownResult? currentResult =
                startResult;

            while (currentResult is not null &&
                   currentResult.IsSuperseded)
            {
                if (positionByResultId.TryGetValue(
                        currentResult.ResultId,
                        out int cycleStartIndex))
                {
                    string[] cycleResultIds =
                        path
                            .Skip(cycleStartIndex)
                            .Concat(
                                new[]
                                {
                                    currentResult.ResultId
                                })
                            .ToArray();

                    string cycleDescription =
                        string.Join(
                            " -> ",
                            cycleResultIds);

                    if (reportedCycles.Add(
                            cycleDescription))
                    {
                        errors.Add(
                            "The known-result supersession " +
                            $"graph contains a cycle: " +
                            $"{cycleDescription}.");
                    }

                    break;
                }

                positionByResultId.Add(
                    currentResult.ResultId,
                    path.Count);

                path.Add(
                    currentResult.ResultId);

                if (!knownResultsById.TryGetValue(
                        currentResult
                            .SupersededByResultId,
                        out KnownResult? nextResult))
                {
                    break;
                }

                currentResult =
                    nextResult;
            }
        }
    }

    private static void ValidateFingerprints(
        LotSizingInstance instance,
        ICollection<string> errors)
    {
        string currentFingerprint;

        try
        {
            currentFingerprint =
                LotSizingInstanceFactory
                    .ComputeSupplyChainFingerprint(
                        instance.SupplyChain);
        }
        catch (InvalidOperationException exception)
        {
            errors.Add(
                "The current supply-chain fingerprint could " +
                $"not be calculated: {exception.Message}");

            return;
        }

        ProductStructureDescriptor descriptor =
            instance.ProductStructure;

        bool productStructureShouldBeCurrent =
            descriptor.CheckStatus !=
                ProductStructureCheckStatus.NotAnalyzed &&
            descriptor.CheckStatus !=
                ProductStructureCheckStatus.DeclaredOnly &&
            descriptor.CheckStatus !=
                ProductStructureCheckStatus.Outdated;

        if (productStructureShouldBeCurrent &&
            !string.IsNullOrWhiteSpace(
                descriptor.SupplyChainFingerprint) &&
            !string.Equals(
                descriptor.SupplyChainFingerprint,
                currentFingerprint,
                StringComparison.Ordinal))
        {
            errors.Add(
                "The product-structure analysis does not " +
                "match the current supply-chain fingerprint " +
                "and should be marked as outdated.");
        }

        LotSizingProblemClassification classification =
            instance.ProblemClassification;

        bool classificationShouldBeCurrent =
            classification.Status !=
                ProblemClassificationStatus.NotAnalyzed &&
            classification.Status !=
                ProblemClassificationStatus.Outdated;

        if (classificationShouldBeCurrent &&
            classification.HasSupplyChainFingerprint &&
            !classification
                .MatchesSupplyChainFingerprint(
                    currentFingerprint))
        {
            errors.Add(
                "The problem classification does not match " +
                "the current supply-chain fingerprint and " +
                "should be marked as outdated.");
        }

        SolutionMethodRecommendationReport recommendationReport =
            instance.SolutionMethodRecommendationReport;

        if (instance.HasSolutionMethodRecommendationReport &&
            recommendationReport.HasSupplyChainFingerprint &&
            !recommendationReport
                .MatchesSupplyChainFingerprint(
                    currentFingerprint))
        {
            errors.Add(
                "The solution-method recommendation report " +
                "does not match the current supply-chain " +
                "fingerprint and must be regenerated.");
        }
    }

    private static void AddProductStructureWarnings(
        ProductStructureDescriptor descriptor,
        ICollection<string> warnings)
    {
        if (descriptor.CheckStatus ==
            ProductStructureCheckStatus
                .DeclaredAndContradicted)
        {
            warnings.Add(
                $"The declared product-structure type " +
                $"'{descriptor.DeclaredType}' differs from " +
                $"the detected type " +
                $"'{descriptor.DetectedType}'.");
        }

        if (descriptor.CheckStatus ==
            ProductStructureCheckStatus.Outdated)
        {
            warnings.Add(
                "The stored product-structure analysis is " +
                "outdated.");
        }

        if (descriptor.HasBeenAnalyzed &&
            string.IsNullOrWhiteSpace(
                descriptor.SupplyChainFingerprint))
        {
            warnings.Add(
                "The product-structure analysis does not " +
                "record a supply-chain fingerprint.");
        }
    }

    private static void AddClassificationWarnings(
        LotSizingProblemClassification classification,
        ICollection<string> warnings)
    {
        if (classification.Status ==
            ProblemClassificationStatus.Outdated)
        {
            warnings.Add(
                "The stored problem classification is " +
                "outdated.");
        }

        if (classification.Status ==
            ProblemClassificationStatus
                .PartiallyClassified)
        {
            warnings.Add(
                "The problem is only partially classified.");
        }

        if (classification.Status ==
            ProblemClassificationStatus.Ambiguous)
        {
            warnings.Add(
                "The problem classification is ambiguous.");
        }

        if (classification.HasPrimaryProblemType &&
            !classification
                .HasPrimaryProblemTypeName)
        {
            warnings.Add(
                $"Primary problem family " +
                $"'{classification.PrimaryProblemTypeCode}' " +
                "does not have a human-readable name.");
        }

        if (classification.HasBeenAnalyzed &&
            !classification
                .HasSupplyChainFingerprint)
        {
            warnings.Add(
                "The problem classification does not record " +
                "a supply-chain fingerprint.");
        }

        foreach (string warning
                 in classification.Warnings)
        {
            if (!string.IsNullOrWhiteSpace(warning))
            {
                warnings.Add(
                    "Classifier: " +
                    warning.Trim());
            }
        }
    }

    private static void AddKnownResultWarnings(
        KnownResult result,
        ICollection<string> warnings)
    {
        if (result.HasReportedObjectiveValue &&
            !result.HasObjectiveName)
        {
            warnings.Add(
                $"Known result '{result.ResultId}' has an " +
                "objective value but no objective name.");
        }

        if (result.HasReportedObjectiveValue &&
            !result.HasObjectiveUnit)
        {
            warnings.Add(
                $"Known result '{result.ResultId}' has an " +
                "objective value but no objective unit.");
        }

        if (result.VerificationStatus ==
                KnownResultVerificationStatus
                    .SourceReported &&
            !result.HasSourceInformation)
        {
            warnings.Add(
                $"Known result '{result.ResultId}' is marked " +
                "as source-reported but has no source " +
                "information.");
        }

        if (result.HasDetailedSolution &&
            result.VerificationStatus ==
                KnownResultVerificationStatus.NotVerified)
        {
            warnings.Add(
                $"Known result '{result.ResultId}' contains " +
                "a detailed solution that has not been " +
                "verified.");
        }

        if (!result.HasSupplyChainFingerprint)
        {
            warnings.Add(
                $"Known result '{result.ResultId}' does not " +
                "record a supply-chain fingerprint.");
        }
    }

    private static void AddFingerprintWarnings(
        LotSizingInstance instance,
        ICollection<string> warnings)
    {
        string currentFingerprint;

        try
        {
            currentFingerprint =
                LotSizingInstanceFactory
                    .ComputeSupplyChainFingerprint(
                        instance.SupplyChain);
        }
        catch (InvalidOperationException)
        {
            return;
        }

        foreach (KnownResult result
                 in instance.KnownResults.Where(
                     result =>
                         result is not null &&
                         result.HasSupplyChainFingerprint))
        {
            if (!result.MatchesSupplyChainFingerprint(
                    currentFingerprint))
            {
                warnings.Add(
                    $"Known result '{result.ResultId}' was " +
                    "recorded for a different supply-chain " +
                    "fingerprint.");
            }
        }

        SolutionMethodRecommendationReport recommendationReport =
            instance.SolutionMethodRecommendationReport;

        if (instance.HasSolutionMethodRecommendationReport &&
            recommendationReport.HasSupplyChainFingerprint &&
            !recommendationReport
                .MatchesSupplyChainFingerprint(
                    currentFingerprint))
        {
            warnings.Add(
                "The solution-method recommendation report " +
                "was generated for a different supply-chain " +
                "fingerprint.");
        }
    }

    private static void ValidateItemIdentifierCollection(
        IEnumerable<int> itemIds,
        string collectionDescription,
        ICollection<string> errors)
    {
        int[] normalizedItemIds =
            itemIds.ToArray();

        if (normalizedItemIds.Any(
                itemId =>
                    itemId <= 0))
        {
            errors.Add(
                $"Every {collectionDescription} identifier " +
                "must be strictly positive.");
        }

        int[] duplicateItemIds =
            normalizedItemIds
                .GroupBy(
                    itemId =>
                        itemId)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    itemId =>
                        itemId)
                .ToArray();

        if (duplicateItemIds.Length > 0)
        {
            errors.Add(
                $"Duplicate {collectionDescription} " +
                "identifiers: " +
                string.Join(
                    ", ",
                    duplicateItemIds) +
                ".");
        }
    }

    private static void ValidateStringCollection(
        IEnumerable<string> values,
        string valueDescription,
        ICollection<string> errors)
    {
        string[] normalizedValues =
            values.ToArray();

        if (normalizedValues.Any(
                value =>
                    string.IsNullOrWhiteSpace(value)))
        {
            errors.Add(
                $"Every {valueDescription} must be " +
                "non-empty.");
        }

        string[] duplicateValues =
            normalizedValues
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(value))
                .GroupBy(
                    value =>
                        value.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    value =>
                        value,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (duplicateValues.Length > 0)
        {
            errors.Add(
                $"Duplicate {valueDescription} values: " +
                string.Join(
                    ", ",
                    duplicateValues) +
                ".");
        }
    }

    private static IReadOnlyList<string> NormalizeMessages(
        IEnumerable<string> messages)
    {
        return messages
            .Where(
                message =>
                    !string.IsNullOrWhiteSpace(message))
            .Select(
                message =>
                    message.Trim())
            .Distinct(
                StringComparer.Ordinal)
            .OrderBy(
                message =>
                    message,
                StringComparer.Ordinal)
            .ToArray();
    }

    private static string DisplayProblemTypeCode(
        KnownProblemTypeMatch match)
    {
        return string.IsNullOrWhiteSpace(
                match.ProblemTypeCode)
            ? "<missing code>"
            : match.ProblemTypeCode;
    }
}