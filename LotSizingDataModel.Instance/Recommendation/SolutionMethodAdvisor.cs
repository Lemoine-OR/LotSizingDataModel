using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Creation;

namespace LotSizingDataModel.Instance.Recommendation;

/// <summary>
/// Evaluates and ranks solution methods for lot-sizing
/// problem instances.
/// </summary>
/// <remarks>
/// The advisor evaluates:
/// <list type="bullet">
/// <item>
/// <description>
/// problem-family compatibility;
/// </description>
/// </item>
/// <item>
/// <description>
/// product-structure compatibility;
/// </description>
/// </item>
/// <item>
/// <description>
/// required, supported and unsupported features;
/// </description>
/// </item>
/// <item>
/// <description>
/// hard and recommended instance-size limits;
/// </description>
/// </item>
/// <item>
/// <description>
/// applicability to the complete problem, a relaxation or a
/// subproblem.
/// </description>
/// </item>
/// </list>
///
/// The advisor does not execute the recommended methods and
/// does not guarantee their practical performance.
/// </remarks>
public static class SolutionMethodAdvisor
{
    /// <summary>
    /// Gets the current version of the solution-method advisor
    /// evaluation semantics.
    /// </summary>
    public const string CurrentVersion =
        "1.0";

    /// <summary>
    /// Gets the default score above which a directly
    /// compatible method may be marked as recommended.
    /// </summary>
    public const double DefaultRecommendedScoreThreshold =
        0.85;

    /// <summary>
    /// Evaluates and ranks every usable method contained in a
    /// solution-method catalog.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing problem instance to analyze.
    /// </param>
    /// <param name="catalog">
    /// Catalog containing the solution-method definitions.
    /// </param>
    /// <param name="recommendedScoreThreshold">
    /// Minimum score required for a directly compatible
    /// method to be marked as recommended.
    /// </param>
    /// <param name="includeIncompatibleMethods">
    /// Value indicating whether incompatible methods must be
    /// included in the returned collection.
    /// </param>
    /// <returns>
    /// Ranked method recommendations.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> or
    /// <paramref name="catalog"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when
    /// <paramref name="recommendedScoreThreshold"/> is not
    /// finite or does not lie between zero and one.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the method catalog is invalid or the
    /// supply-chain fingerprint cannot be calculated.
    /// </exception>
    public static IReadOnlyList<SolutionMethodRecommendation>
        Recommend(
            LotSizingInstance instance,
            SolutionMethodCatalog catalog,
            double recommendedScoreThreshold =
                DefaultRecommendedScoreThreshold,
            bool includeIncompatibleMethods = true)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(catalog);

        ValidateRecommendedScoreThreshold(
            recommendedScoreThreshold);

        catalog.EnsureValid();

        string supplyChainFingerprint =
            LotSizingInstanceFactory
                .ComputeSupplyChainFingerprint(
                    instance.SupplyChain);

        IReadOnlyList<SolutionMethodDefinition>
            methodDefinitions =
                catalog.GetMethodsForEvaluation();

        var recommendations =
            new List<SolutionMethodRecommendation>(
                methodDefinitions.Count);

        foreach (SolutionMethodDefinition methodDefinition
                 in methodDefinitions)
        {
            SolutionMethodRecommendation recommendation =
                EvaluateMethod(
                    instance:
                        instance,

                    methodDefinition:
                        methodDefinition,

                    methodCatalogName:
                        catalog.CatalogName,

                    methodCatalogVersion:
                        catalog.CatalogVersion,

                    supplyChainFingerprint:
                        supplyChainFingerprint,

                    recommendedScoreThreshold:
                        recommendedScoreThreshold);

            if (includeIncompatibleMethods ||
                !recommendation.IsIncompatible)
            {
                recommendations.Add(
                    recommendation);
            }
        }

        return RankRecommendations(
            recommendations,
            methodDefinitions);
    }

    /// <summary>
    /// Evaluates one solution method for a lot-sizing problem
    /// instance.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing problem instance to analyze.
    /// </param>
    /// <param name="methodDefinition">
    /// Solution-method definition to evaluate.
    /// </param>
    /// <param name="methodCatalogName">
    /// Optional name of the catalog containing the method.
    /// </param>
    /// <param name="methodCatalogVersion">
    /// Optional version of the catalog containing the method.
    /// </param>
    /// <param name="recommendedScoreThreshold">
    /// Minimum score required for a directly compatible
    /// method to be marked as recommended.
    /// </param>
    /// <returns>
    /// Compatibility recommendation for the supplied method.
    /// </returns>
    public static SolutionMethodRecommendation EvaluateMethod(
        LotSizingInstance instance,
        SolutionMethodDefinition methodDefinition,
        string methodCatalogName = "",
        string methodCatalogVersion = "",
        double recommendedScoreThreshold =
            DefaultRecommendedScoreThreshold)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(methodDefinition);

        ValidateRecommendedScoreThreshold(
            recommendedScoreThreshold);

        methodDefinition.EnsureValid();

        string supplyChainFingerprint =
            LotSizingInstanceFactory
                .ComputeSupplyChainFingerprint(
                    instance.SupplyChain);

        return EvaluateMethod(
            instance:
                instance,

            methodDefinition:
                methodDefinition,

            methodCatalogName:
                methodCatalogName,

            methodCatalogVersion:
                methodCatalogVersion,

            supplyChainFingerprint:
                supplyChainFingerprint,

            recommendedScoreThreshold:
                recommendedScoreThreshold);
    }

    /// <summary>
    /// Ranks an existing collection of method
    /// recommendations.
    /// </summary>
    /// <param name="recommendations">
    /// Recommendations to rank.
    /// </param>
    /// <returns>
    /// Recommendations ordered from the most to the least
    /// appropriate.
    /// </returns>
    /// <remarks>
    /// Ranking uses:
    /// <list type="number">
    /// <item>
    /// <description>compatibility level;</description>
    /// </item>
    /// <item>
    /// <description>compatibility score;</description>
    /// </item>
    /// <item>
    /// <description>method name;</description>
    /// </item>
    /// <item>
    /// <description>method code.</description>
    /// </item>
    /// </list>
    /// </remarks>
    public static IReadOnlyList<SolutionMethodRecommendation>
        RankRecommendations(
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

        SolutionMethodRecommendation[] ordered =
            materialized
                .OrderByDescending(
                    recommendation =>
                        recommendation.CompatibilityLevel)
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

        AssignSequentialRanks(
            ordered);

        return ordered;
    }

    private static SolutionMethodRecommendation EvaluateMethod(
        LotSizingInstance instance,
        SolutionMethodDefinition methodDefinition,
        string methodCatalogName,
        string methodCatalogVersion,
        string supplyChainFingerprint,
        double recommendedScoreThreshold)
    {
        LotSizingProblemClassification classification =
            instance.ProblemClassification;

        LotSizingProblemFeatures features =
            classification.Features;

        var recommendation =
            new SolutionMethodRecommendation(
                methodDefinition)
            {
                AdvisorVersion =
                    CurrentVersion,

                MethodCatalogName =
                    methodCatalogName?.Trim() ??
                    string.Empty,

                MethodCatalogVersion =
                    methodCatalogVersion?.Trim() ??
                    string.Empty,

                SupplyChainFingerprint =
                    supplyChainFingerprint
            };

        var evidence =
            new List<MethodCompatibilityEvidence>();

        var adaptations =
            new List<string>();

        var warnings =
            new List<string>();

        bool supportsAlternativeScope =
            methodDefinition.SupportsRelaxations ||
            methodDefinition.SupportsSubproblems;

        AddClassificationStatusWarnings(
            classification,
            warnings);

        EvaluateProblemFamily(
            classification:
                classification,

            methodDefinition:
                methodDefinition,

            supportsAlternativeScope:
                supportsAlternativeScope,

            evidence:
                evidence,

            adaptations:
                adaptations);

        EvaluateProductStructure(
            features:
                features,

            methodDefinition:
                methodDefinition,

            supportsAlternativeScope:
                supportsAlternativeScope,

            evidence:
                evidence,

            adaptations:
                adaptations);

        EvaluateRequiredFeatures(
            features:
                features,

            methodDefinition:
                methodDefinition,

            evidence:
                evidence,

            warnings:
                warnings);

        EvaluateUnsupportedFeatures(
            features:
                features,

            methodDefinition:
                methodDefinition,

            supportsAlternativeScope:
                supportsAlternativeScope,

            evidence:
                evidence,

            adaptations:
                adaptations,

            warnings:
                warnings);

        EvaluatePartiallySupportedFeatures(
            features:
                features,

            methodDefinition:
                methodDefinition,

            evidence:
                evidence,

            adaptations:
                adaptations,

            warnings:
                warnings);

        EvaluateSupportedFeatures(
            features:
                features,

            methodDefinition:
                methodDefinition,

            evidence:
                evidence,

            warnings:
                warnings);

        EvaluatePreferredFeatures(
            features:
                features,

            methodDefinition:
                methodDefinition,

            evidence:
                evidence,

            warnings:
                warnings);

        EvaluateSizeLimits(
            features:
                features,

            methodDefinition:
                methodDefinition,

            supportsAlternativeScope:
                supportsAlternativeScope,

            evidence:
                evidence,

            adaptations:
                adaptations,

            warnings:
                warnings);

        recommendation.ReplaceEvidence(
            evidence);

        recommendation.ReplaceRequiredAdaptations(
            adaptations);

        recommendation.ReplaceWarnings(
            warnings);

        recommendation.Scope =
            DetermineScope(
                features:
                    features,

                methodDefinition:
                    methodDefinition,

                recommendation:
                    recommendation,

                adaptations:
                    adaptations);

        recommendation.ScopeDescription =
            CreateScopeDescription(
                recommendation.Scope);

        recommendation.UpdateCompatibilityFromEvidence(
            recommendedScoreThreshold:
                recommendedScoreThreshold,

            updateEvaluationDate:
                true);

        recommendation.Summary =
            CreateSummary(
                recommendation,
                classification);

        return recommendation;
    }

    private static IReadOnlyList<SolutionMethodRecommendation>
        RankRecommendations(
            IEnumerable<SolutionMethodRecommendation>
                recommendations,
            IEnumerable<SolutionMethodDefinition>
                methodDefinitions)
    {
        Dictionary<string, int> priorityByMethodCode =
            methodDefinitions
                .Where(
                    method =>
                        method is not null &&
                        method.HasMethodCode)
                .GroupBy(
                    method =>
                        method.MethodCode,
                    StringComparer.OrdinalIgnoreCase)
                .ToDictionary(
                    group =>
                        group.Key,

                    group =>
                        group.First().Priority,

                    StringComparer.OrdinalIgnoreCase);

        SolutionMethodRecommendation[] ordered =
            recommendations
                .OrderByDescending(
                    recommendation =>
                        recommendation.CompatibilityLevel)
                .ThenByDescending(
                    recommendation =>
                        recommendation.Score)
                .ThenByDescending(
                    recommendation =>
                        priorityByMethodCode.TryGetValue(
                            recommendation.MethodCode,
                            out int priority)
                            ? priority
                            : 0)
                .ThenBy(
                    recommendation =>
                        recommendation.MethodName,
                    StringComparer.OrdinalIgnoreCase)
                .ThenBy(
                    recommendation =>
                        recommendation.MethodCode,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        AssignSequentialRanks(
            ordered);

        return ordered;
    }

    private static void EvaluateProblemFamily(
        LotSizingProblemClassification classification,
        SolutionMethodDefinition methodDefinition,
        bool supportsAlternativeScope,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> adaptations)
    {
        if (methodDefinition.SupportsAnyProblemFamily)
        {
            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        "FAMILY.ANY_SUPPORTED",

                    description:
                        "The method is independent from a " +
                        "specific problem-family code.",

                    isSatisfied:
                        true,

                    isRequired:
                        true,

                    isBlocking:
                        false,

                    expectedValue:
                        "any",

                    observedValue:
                        GetObservedProblemFamilyDescription(
                            classification),

                    weight:
                        1.5));

            return;
        }

        IReadOnlyList<string> candidateCodes =
            GetCandidateProblemTypeCodes(
                classification);

        if (candidateCodes.Count == 0)
        {
            bool supportsUnclassified =
                methodDefinition
                    .SupportsUnclassifiedProblems;

            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        "FAMILY.CLASSIFICATION_AVAILABLE",

                    description:
                        supportsUnclassified
                            ? "The method accepts problems " +
                              "without a recognized family."
                            : "The method requires a " +
                              "recognized supported problem " +
                              "family.",

                    isSatisfied:
                        supportsUnclassified,

                    isRequired:
                        true,

                    isBlocking:
                        !supportsUnclassified &&
                        !supportsAlternativeScope,

                    expectedValue:
                        supportsUnclassified
                            ? "classified or unclassified"
                            : string.Join(
                                ";",
                                methodDefinition
                                    .SupportedProblemTypeCodes),

                    observedValue:
                        classification.Status,

                    weight:
                        2.0));

            if (!supportsUnclassified &&
                supportsAlternativeScope)
            {
                adaptations.Add(
                    "Identify a supported relaxation or " +
                    "subproblem before applying the method.");
            }

            return;
        }

        bool isAmbiguous =
            classification.Status ==
            ProblemClassificationStatus.Ambiguous ||
            candidateCodes.Count > 1;

        if (isAmbiguous &&
            !methodDefinition
                .SupportsAmbiguousClassifications)
        {
            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        "FAMILY.AMBIGUOUS_CLASSIFICATION",

                    description:
                        "The method definition does not " +
                        "accept an ambiguous problem-family " +
                        "classification.",

                    isSatisfied:
                        false,

                    isRequired:
                        true,

                    isBlocking:
                        !supportsAlternativeScope,

                    expectedValue:
                        "unambiguous classification",

                    observedValue:
                        string.Join(
                            ";",
                            candidateCodes),

                    weight:
                        1.5));

            if (supportsAlternativeScope)
            {
                adaptations.Add(
                    "Resolve the family ambiguity or apply " +
                    "the method only to a supported " +
                    "subproblem.");
            }
        }

        string[] supportedCandidateCodes =
            candidateCodes
                .Where(
                    methodDefinition
                        .SupportsProblemTypeCode)
                .ToArray();

        bool familySupported =
            supportedCandidateCodes.Length > 0;

        evidence.Add(
            CreateEvidence(
                criterionCode:
                    "FAMILY.SUPPORTED",

                description:
                    familySupported
                        ? "The method supports at least one " +
                          "recognized problem family."
                        : "The method does not directly " +
                          "support the recognized problem " +
                          "family.",

                isSatisfied:
                    familySupported,

                isRequired:
                    true,

                isBlocking:
                    !familySupported &&
                    !supportsAlternativeScope,

                expectedValue:
                    string.Join(
                        ";",
                        methodDefinition
                            .SupportedProblemTypeCodes),

                observedValue:
                    string.Join(
                        ";",
                        candidateCodes),

                weight:
                    3.0));

        if (!familySupported &&
            supportsAlternativeScope)
        {
            adaptations.Add(
                "Apply the method to a supported relaxation " +
                "or subproblem rather than to the complete " +
                "problem.");
        }

        string? preferredCandidateCode =
            supportedCandidateCodes
                .FirstOrDefault(
                    methodDefinition
                        .PrefersProblemTypeCode);

        if (!string.IsNullOrWhiteSpace(
                preferredCandidateCode))
        {
            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        "FAMILY.PREFERRED",

                    description:
                        "The recognized problem family is " +
                        "identified as particularly " +
                        "appropriate for the method.",

                    isSatisfied:
                        true,

                    isRequired:
                        false,

                    isBlocking:
                        false,

                    expectedValue:
                        string.Join(
                            ";",
                            methodDefinition
                                .PreferredProblemTypeCodes),

                    observedValue:
                        preferredCandidateCode,

                    weight:
                        1.5));
        }
    }

    private static void EvaluateProductStructure(
        LotSizingProblemFeatures features,
        SolutionMethodDefinition methodDefinition,
        bool supportsAlternativeScope,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> adaptations)
    {
        ProductStructureType observedType =
            features.ProductStructureType;

        if (methodDefinition.SupportsAnyProductStructure)
        {
            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        "STRUCTURE.ANY_SUPPORTED",

                    description:
                        "The method supports every valid " +
                        "product-structure type.",

                    isSatisfied:
                        observedType !=
                        ProductStructureType.Unknown,

                    isRequired:
                        true,

                    isBlocking:
                        observedType ==
                            ProductStructureType.Unknown,

                    expectedValue:
                        "any valid structure",

                    observedValue:
                        observedType,

                    weight:
                        2.0));

            return;
        }

        bool structureSupported =
            observedType !=
                ProductStructureType.Unknown &&
            methodDefinition
                .SupportsProductStructureType(
                    observedType);

        evidence.Add(
            CreateEvidence(
                criterionCode:
                    "STRUCTURE.SUPPORTED",

                description:
                    structureSupported
                        ? "The method supports the detected " +
                          "product structure."
                        : "The method does not directly " +
                          "support the detected product " +
                          "structure.",

                isSatisfied:
                    structureSupported,

                isRequired:
                    true,

                isBlocking:
                    !structureSupported &&
                    !supportsAlternativeScope,

                expectedValue:
                    string.Join(
                        ";",
                        methodDefinition
                            .SupportedProductStructureTypes),

                observedValue:
                    observedType,

                weight:
                    2.5));

        if (!structureSupported &&
            supportsAlternativeScope)
        {
            adaptations.Add(
                "Decompose or relax the product structure " +
                "before applying the method.");
        }
    }

    private static void EvaluateRequiredFeatures(
        LotSizingProblemFeatures features,
        SolutionMethodDefinition methodDefinition,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> warnings)
    {
        foreach (string featureCode
                 in DistinctCodes(
                     methodDefinition
                         .RequiredFeatureCodes))
        {
            if (!TryGetBooleanFeature(
                    features,
                    featureCode,
                    out bool observedValue))
            {
                evidence.Add(
                    CreateEvidence(
                        criterionCode:
                            CreateFeatureCriterionCode(
                                "REQUIRED",
                                featureCode),

                        description:
                            $"Required feature " +
                            $"'{featureCode}' could not be " +
                            "evaluated.",

                        isSatisfied:
                            false,

                        isRequired:
                            true,

                        isBlocking:
                            true,

                        expectedValue:
                            true,

                        observedValue:
                            "unavailable",

                        featureCode:
                            featureCode,

                        weight:
                            2.0));

                warnings.Add(
                    $"Required feature '{featureCode}' " +
                    "could not be read from the feature " +
                    "profile.");

                continue;
            }

            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        CreateFeatureCriterionCode(
                            "REQUIRED",
                            featureCode),

                    description:
                        observedValue
                            ? $"Required feature " +
                              $"'{featureCode}' is active."
                            : $"The method requires feature " +
                              $"'{featureCode}', but it is " +
                              "not active.",

                    isSatisfied:
                        observedValue,

                    isRequired:
                        true,

                    isBlocking:
                        true,

                    expectedValue:
                        true,

                    observedValue:
                        observedValue,

                    featureCode:
                        featureCode,

                    weight:
                        2.0));
        }
    }

    private static void EvaluateUnsupportedFeatures(
        LotSizingProblemFeatures features,
        SolutionMethodDefinition methodDefinition,
        bool supportsAlternativeScope,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> adaptations,
        ICollection<string> warnings)
    {
        foreach (string featureCode
                 in DistinctCodes(
                     methodDefinition
                         .UnsupportedFeatureCodes))
        {
            if (!TryGetBooleanFeature(
                    features,
                    featureCode,
                    out bool observedValue))
            {
                warnings.Add(
                    $"Unsupported feature criterion " +
                    $"'{featureCode}' could not be read from " +
                    "the feature profile.");

                continue;
            }

            bool criterionSatisfied =
                !observedValue;

            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        CreateFeatureCriterionCode(
                            "UNSUPPORTED",
                            featureCode),

                    description:
                        criterionSatisfied
                            ? $"Unsupported feature " +
                              $"'{featureCode}' is absent."
                            : $"Feature '{featureCode}' is " +
                              "active but is not supported " +
                              "by the method.",

                    isSatisfied:
                        criterionSatisfied,

                    isRequired:
                        true,

                    isBlocking:
                        !criterionSatisfied &&
                        !supportsAlternativeScope,

                    expectedValue:
                        false,

                    observedValue:
                        observedValue,

                    featureCode:
                        featureCode,

                    weight:
                        2.0));

            if (!criterionSatisfied &&
                supportsAlternativeScope)
            {
                adaptations.Add(
                    $"Remove, relax or separately handle " +
                    $"feature '{featureCode}'.");
            }
        }
    }

    private static void
        EvaluatePartiallySupportedFeatures(
            LotSizingProblemFeatures features,
            SolutionMethodDefinition methodDefinition,
            ICollection<MethodCompatibilityEvidence> evidence,
            ICollection<string> adaptations,
            ICollection<string> warnings)
    {
        foreach (string featureCode
                 in DistinctCodes(
                     methodDefinition
                         .PartiallySupportedFeatureCodes))
        {
            if (!TryGetBooleanFeature(
                    features,
                    featureCode,
                    out bool observedValue))
            {
                warnings.Add(
                    $"Partially supported feature " +
                    $"'{featureCode}' could not be read from " +
                    "the feature profile.");

                continue;
            }

            if (!observedValue)
            {
                continue;
            }

            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        CreateFeatureCriterionCode(
                            "PARTIAL",
                            featureCode),

                    description:
                        $"Feature '{featureCode}' is active " +
                        "and requires an adaptation of the " +
                        "method.",

                    isSatisfied:
                        false,

                    isRequired:
                        true,

                    isBlocking:
                        false,

                    expectedValue:
                        "absent or adapted",

                    observedValue:
                        true,

                    featureCode:
                        featureCode,

                    weight:
                        1.5));

            adaptations.Add(
                $"Use an adapted implementation for feature " +
                $"'{featureCode}'.");
        }
    }

    private static void EvaluateSupportedFeatures(
        LotSizingProblemFeatures features,
        SolutionMethodDefinition methodDefinition,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> warnings)
    {
        HashSet<string> requiredFeatures =
            methodDefinition
                .RequiredFeatureCodes
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        foreach (string featureCode
                 in DistinctCodes(
                     methodDefinition
                         .SupportedFeatureCodes))
        {
            if (requiredFeatures.Contains(
                    featureCode))
            {
                continue;
            }

            if (!TryGetBooleanFeature(
                    features,
                    featureCode,
                    out bool observedValue))
            {
                warnings.Add(
                    $"Supported feature '{featureCode}' " +
                    "could not be read from the feature " +
                    "profile.");

                continue;
            }

            if (!observedValue)
            {
                continue;
            }

            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        CreateFeatureCriterionCode(
                            "SUPPORTED",
                            featureCode),

                    description:
                        $"Active feature '{featureCode}' is " +
                        "supported by the method.",

                    isSatisfied:
                        true,

                    isRequired:
                        false,

                    isBlocking:
                        false,

                    expectedValue:
                        true,

                    observedValue:
                        true,

                    featureCode:
                        featureCode,

                    weight:
                        1.0));
        }
    }

    private static void EvaluatePreferredFeatures(
        LotSizingProblemFeatures features,
        SolutionMethodDefinition methodDefinition,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> warnings)
    {
        foreach (string featureCode
                 in DistinctCodes(
                     methodDefinition
                         .PreferredFeatureCodes))
        {
            if (!TryGetBooleanFeature(
                    features,
                    featureCode,
                    out bool observedValue))
            {
                warnings.Add(
                    $"Preferred feature '{featureCode}' " +
                    "could not be read from the feature " +
                    "profile.");

                continue;
            }

            evidence.Add(
                CreateEvidence(
                    criterionCode:
                        CreateFeatureCriterionCode(
                            "PREFERRED",
                            featureCode),

                    description:
                        observedValue
                            ? $"Preferred feature " +
                              $"'{featureCode}' is active."
                            : $"Preferred feature " +
                              $"'{featureCode}' is not active.",

                    isSatisfied:
                        observedValue,

                    isRequired:
                        false,

                    isBlocking:
                        false,

                    expectedValue:
                        true,

                    observedValue:
                        observedValue,

                    featureCode:
                        featureCode,

                    weight:
                        0.75));
        }
    }

    private static void EvaluateSizeLimits(
        LotSizingProblemFeatures features,
        SolutionMethodDefinition methodDefinition,
        bool supportsAlternativeScope,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> adaptations,
        ICollection<string> warnings)
    {
        EvaluateHardLimit(
            features,
            methodDefinition.MaximumItemCount,
            "LIMIT.MAX_ITEMS",
            "item count",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "ItemCount");

        EvaluateHardLimit(
            features,
            methodDefinition.MaximumPlanningHorizon,
            "LIMIT.MAX_HORIZON",
            "planning horizon",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "PlanningHorizon");

        EvaluateHardLimit(
            features,
            methodDefinition.MaximumPlantCount,
            "LIMIT.MAX_PLANTS",
            "plant count",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "PlantCount");

        EvaluateHardLimit(
            features,
            methodDefinition.MaximumWorkCenterCount,
            "LIMIT.MAX_WORK_CENTERS",
            "work-center count",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "WorkCenterCount");

        EvaluateHardLimit(
            features,
            methodDefinition.MaximumWarehouseCount,
            "LIMIT.MAX_WAREHOUSES",
            "warehouse count",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "WarehouseCount");

        EvaluateHardLimit(
            features,
            methodDefinition.MaximumSupplierCount,
            "LIMIT.MAX_SUPPLIERS",
            "supplier count",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "SupplierCount");

        EvaluateHardLimit(
            features,
            methodDefinition.MaximumTransportResourceCount,
            "LIMIT.MAX_TRANSPORT_RESOURCES",
            "transport-resource count",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "TransportResourceCount");

        EvaluateHardLimit(
            features,
            methodDefinition
                .MaximumBillOfMaterialsRelationshipCount,
            "LIMIT.MAX_BOM_RELATIONSHIPS",
            "bill-of-materials relationship count",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "ProductStructureRelationshipCount",
            "BillOfMaterialsRelationshipCount",
            "ComponentRequirementCount");

        EvaluateHardLimit(
            features,
            methodDefinition.MaximumProductStructureDepth,
            "LIMIT.MAX_STRUCTURE_DEPTH",
            "product-structure depth",
            supportsAlternativeScope,
            evidence,
            adaptations,
            warnings,
            "ProductStructureDepth",
            "MaximumProductStructureDepth",
            "BillOfMaterialsDepth");

        EvaluateRecommendedLimit(
            features,
            methodDefinition.RecommendedMaximumItemCount,
            "LIMIT.RECOMMENDED_MAX_ITEMS",
            "recommended item count",
            evidence,
            warnings,
            "ItemCount");

        EvaluateRecommendedLimit(
            features,
            methodDefinition
                .RecommendedMaximumPlanningHorizon,
            "LIMIT.RECOMMENDED_MAX_HORIZON",
            "recommended planning horizon",
            evidence,
            warnings,
            "PlanningHorizon");

        EvaluateRecommendedLimit(
            features,
            methodDefinition
                .RecommendedMaximumBillOfMaterialsRelationshipCount,
            "LIMIT.RECOMMENDED_MAX_BOM_RELATIONSHIPS",
            "recommended bill-of-materials relationship count",
            evidence,
            warnings,
            "ProductStructureRelationshipCount",
            "BillOfMaterialsRelationshipCount",
            "ComponentRequirementCount");

        EvaluateRecommendedLimit(
            features,
            methodDefinition
                .RecommendedMaximumProductStructureDepth,
            "LIMIT.RECOMMENDED_MAX_STRUCTURE_DEPTH",
            "recommended product-structure depth",
            evidence,
            warnings,
            "ProductStructureDepth",
            "MaximumProductStructureDepth",
            "BillOfMaterialsDepth");
    }

    private static void EvaluateHardLimit(
        LotSizingProblemFeatures features,
        int? maximumValue,
        string criterionCode,
        string description,
        bool supportsAlternativeScope,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> adaptations,
        ICollection<string> warnings,
        params string[] featurePropertyNames)
    {
        if (!maximumValue.HasValue)
        {
            return;
        }

        if (!TryGetIntegerFeature(
                features,
                featurePropertyNames,
                out int observedValue,
                out string resolvedFeatureCode))
        {
            warnings.Add(
                $"Hard limit '{criterionCode}' could not be " +
                "evaluated because the corresponding " +
                "feature value is unavailable.");

            return;
        }

        bool limitSatisfied =
            observedValue <=
            maximumValue.Value;

        evidence.Add(
            CreateEvidence(
                criterionCode:
                    criterionCode,

                description:
                    limitSatisfied
                        ? $"The {description} is within the " +
                          "method's hard limit."
                        : $"The {description} exceeds the " +
                          "method's hard limit.",

                isSatisfied:
                    limitSatisfied,

                isRequired:
                    true,

                isBlocking:
                    !limitSatisfied &&
                    !supportsAlternativeScope,

                expectedValue:
                    $"<= {maximumValue.Value.ToString(
                        CultureInfo.InvariantCulture)}",

                observedValue:
                    observedValue,

                featureCode:
                    resolvedFeatureCode,

                weight:
                    2.0));

        if (!limitSatisfied &&
            supportsAlternativeScope)
        {
            adaptations.Add(
                $"Decompose the instance so that the " +
                $"{description} does not exceed " +
                $"{maximumValue.Value.ToString(
                    CultureInfo.InvariantCulture)}.");
        }
    }

    private static void EvaluateRecommendedLimit(
        LotSizingProblemFeatures features,
        int? maximumValue,
        string criterionCode,
        string description,
        ICollection<MethodCompatibilityEvidence> evidence,
        ICollection<string> warnings,
        params string[] featurePropertyNames)
    {
        if (!maximumValue.HasValue)
        {
            return;
        }

        if (!TryGetIntegerFeature(
                features,
                featurePropertyNames,
                out int observedValue,
                out string resolvedFeatureCode))
        {
            warnings.Add(
                $"Recommended limit '{criterionCode}' could " +
                "not be evaluated because the corresponding " +
                "feature value is unavailable.");

            return;
        }

        bool limitSatisfied =
            observedValue <=
            maximumValue.Value;

        evidence.Add(
            CreateEvidence(
                criterionCode:
                    criterionCode,

                description:
                    limitSatisfied
                        ? $"The {description} is within the " +
                          "effective operating range."
                        : $"The {description} exceeds the " +
                          "method's recommended operating " +
                          "range.",

                isSatisfied:
                    limitSatisfied,

                isRequired:
                    false,

                isBlocking:
                    false,

                expectedValue:
                    $"<= {maximumValue.Value.ToString(
                        CultureInfo.InvariantCulture)}",

                observedValue:
                    observedValue,

                featureCode:
                    resolvedFeatureCode,

                weight:
                    0.75));

        if (!limitSatisfied)
        {
            warnings.Add(
                $"The {description} exceeds the recommended " +
                $"maximum of {maximumValue.Value.ToString(
                    CultureInfo.InvariantCulture)} for this " +
                "method.");
        }
    }

    private static ProblemClassificationScope DetermineScope(
        LotSizingProblemFeatures features,
        SolutionMethodDefinition methodDefinition,
        SolutionMethodRecommendation recommendation,
        ICollection<string> adaptations)
    {
        bool directCompleteProblemUse =
            methodDefinition.SupportsCompleteProblems &&
            !recommendation.HasBlockingMismatches &&
            !recommendation.HasRequiredMismatches &&
            adaptations.Count == 0;

        if (directCompleteProblemUse)
        {
            return ProblemClassificationScope.CompleteProblem;
        }

        if (recommendation.HasBlockingMismatches)
        {
            return methodDefinition.SupportsCompleteProblems
                ? ProblemClassificationScope.CompleteProblem
                : ProblemClassificationScope.CustomSubset;
        }

        if (methodDefinition.SupportsRelaxations)
        {
            if (!adaptations.Contains(
                    "Apply the method to a relaxation of the " +
                    "complete problem.",
                    StringComparer.OrdinalIgnoreCase))
            {
                adaptations.Add(
                    "Apply the method to a relaxation of the " +
                    "complete problem.");
            }

            return ProblemClassificationScope
                .ProblemRelaxation;
        }

        if (methodDefinition.SupportsSubproblems)
        {
            if (!adaptations.Contains(
                    "Apply the method to suitable " +
                    "subproblems.",
                    StringComparer.OrdinalIgnoreCase))
            {
                adaptations.Add(
                    "Apply the method to suitable " +
                    "subproblems.");
            }

            return features.IsMultiItem
                ? ProblemClassificationScope.SingleItem
                : ProblemClassificationScope.CustomSubset;
        }

        return methodDefinition.SupportsCompleteProblems
            ? ProblemClassificationScope.CompleteProblem
            : ProblemClassificationScope.Unknown;
    }

    private static string CreateScopeDescription(
        ProblemClassificationScope scope)
    {
        return scope switch
        {
            ProblemClassificationScope.CompleteProblem =>
                "Complete problem",

            ProblemClassificationScope.ProblemRelaxation =>
                "Relaxation of the complete problem",

            ProblemClassificationScope.SingleItem =>
                "Single-item subproblems",

            ProblemClassificationScope.ItemSubset =>
                "Subset of items",

            ProblemClassificationScope.Plant =>
                "Plant-level subproblem",

            ProblemClassificationScope.WorkCenter =>
                "Work-center subproblem",

            ProblemClassificationScope.Warehouse =>
                "Warehouse subproblem",

            ProblemClassificationScope.TransportResource =>
                "Transport-resource subproblem",

            ProblemClassificationScope.TransportLane =>
                "Transport-lane subproblem",

            ProblemClassificationScope.Supplier =>
                "Supplier subproblem",

            ProblemClassificationScope.DistributionCenter =>
                "Distribution-center subproblem",

            ProblemClassificationScope.SupplyChainSegment =>
                "Supply-chain segment",

            ProblemClassificationScope.CustomSubset =>
                "Custom problem subset",

            _ =>
                string.Empty
        };
    }

    private static string CreateSummary(
        SolutionMethodRecommendation recommendation,
        LotSizingProblemClassification classification)
    {
        string familyDescription =
            classification.HasPrimaryProblemType
                ? $" for family " +
                  $"'{classification.PrimaryProblemTypeCode}'"
                : string.Empty;

        return recommendation.CompatibilityLevel switch
        {
            MethodCompatibilityLevel.Recommended =>
                "The method is recommended for the complete " +
                $"problem{familyDescription}.",

            MethodCompatibilityLevel.Compatible =>
                "The method is compatible with the complete " +
                $"problem{familyDescription}.",

            MethodCompatibilityLevel.PartiallyCompatible =>
                $"The method is partially compatible and " +
                $"should be applied to: " +
                $"{recommendation.ScopeDescription}.",

            MethodCompatibilityLevel.Incompatible =>
                CreateIncompatibilitySummary(
                    recommendation),

            _ =>
                "The method compatibility has not been " +
                "evaluated."
        };
    }

    private static string CreateIncompatibilitySummary(
        SolutionMethodRecommendation recommendation)
    {
        MethodCompatibilityEvidence? principalMismatch =
            recommendation.Evidence
                .FirstOrDefault(
                    item =>
                        item is not null &&
                        item.IsBlockingMismatch)
            ??
            recommendation.Evidence
                .FirstOrDefault(
                    item =>
                        item is not null &&
                        item.IsRequiredMismatch);

        return principalMismatch is null
            ? "The method is incompatible with the complete " +
              "problem."
            : "The method is incompatible with the complete " +
              $"problem: {principalMismatch.Description}";
    }

    private static void AddClassificationStatusWarnings(
        LotSizingProblemClassification classification,
        ICollection<string> warnings)
    {
        switch (classification.Status)
        {
            case ProblemClassificationStatus.NotAnalyzed:
                warnings.Add(
                    "The problem has not been automatically " +
                    "classified.");
                break;

            case ProblemClassificationStatus
                .PartiallyClassified:
                warnings.Add(
                    "The problem classification is partial.");
                break;

            case ProblemClassificationStatus.Ambiguous:
                warnings.Add(
                    "The problem-family classification is " +
                    "ambiguous.");
                break;

            case ProblemClassificationStatus.Unclassified:
                warnings.Add(
                    "No known problem family was identified.");
                break;

            case ProblemClassificationStatus.Invalid:
                warnings.Add(
                    "The problem classification is invalid.");
                break;

            case ProblemClassificationStatus.Outdated:
                warnings.Add(
                    "The problem classification is outdated.");
                break;
        }

        foreach (string classificationWarning
                 in classification.Warnings)
        {
            if (!string.IsNullOrWhiteSpace(
                    classificationWarning))
            {
                warnings.Add(
                    "Classifier: " +
                    classificationWarning.Trim());
            }
        }
    }

    private static IReadOnlyList<string>
        GetCandidateProblemTypeCodes(
            LotSizingProblemClassification classification)
    {
        if (classification.HasPrimaryProblemType)
        {
            return new[]
            {
                classification.PrimaryProblemTypeCode
            };
        }

        return classification.Matches
            .Where(
                match =>
                    match is not null &&
                    match.IsDirectMatch &&
                    match.AppliesToCompleteProblem &&
                    !string.IsNullOrWhiteSpace(
                        match.ProblemTypeCode))
            .Select(
                match =>
                    match.ProblemTypeCode.Trim())
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                code =>
                    code,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string
        GetObservedProblemFamilyDescription(
            LotSizingProblemClassification classification)
    {
        IReadOnlyList<string> codes =
            GetCandidateProblemTypeCodes(
                classification);

        return codes.Count > 0
            ? string.Join(
                ";",
                codes)
            : classification.Status.ToString();
    }

    private static MethodCompatibilityEvidence
        CreateEvidence(
            string criterionCode,
            string description,
            bool isSatisfied,
            bool isRequired,
            bool isBlocking,
            object? expectedValue,
            object? observedValue,
            string featureCode = "",
            double weight = 1.0)
    {
        var evidence =
            new MethodCompatibilityEvidence(
                criterionCode:
                    criterionCode,

                description:
                    description,

                isSatisfied:
                    isSatisfied,

                isRequired:
                    isRequired,

                isBlocking:
                    isBlocking,

                weight:
                    weight)
            {
                FeatureCode =
                    featureCode?.Trim() ??
                    string.Empty
            };

        evidence.SetComparedValues(
            expectedValue,
            observedValue);

        return evidence;
    }

    private static bool TryGetBooleanFeature(
        LotSizingProblemFeatures features,
        string featureCode,
        out bool value)
    {
        value =
            false;

        if (string.IsNullOrWhiteSpace(
                featureCode))
        {
            return false;
        }

        PropertyInfo? property =
            typeof(LotSizingProblemFeatures)
                .GetProperty(
                    featureCode.Trim(),
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.IgnoreCase);

        if (property is null ||
            !property.CanRead ||
            property.GetIndexParameters().Length != 0 ||
            property.PropertyType != typeof(bool))
        {
            return false;
        }

        object? propertyValue =
            property.GetValue(
                features);

        if (propertyValue is not bool booleanValue)
        {
            return false;
        }

        value =
            booleanValue;

        return true;
    }

    private static bool TryGetIntegerFeature(
        LotSizingProblemFeatures features,
        IEnumerable<string> candidatePropertyNames,
        out int value,
        out string resolvedFeatureCode)
    {
        value =
            0;

        resolvedFeatureCode =
            string.Empty;

        foreach (string candidatePropertyName
                 in candidatePropertyNames)
        {
            if (string.IsNullOrWhiteSpace(
                    candidatePropertyName))
            {
                continue;
            }

            PropertyInfo? property =
                typeof(LotSizingProblemFeatures)
                    .GetProperty(
                        candidatePropertyName.Trim(),
                        BindingFlags.Instance |
                        BindingFlags.Public |
                        BindingFlags.IgnoreCase);

            if (property is null ||
                !property.CanRead ||
                property.GetIndexParameters().Length != 0)
            {
                continue;
            }

            object? rawValue =
                property.GetValue(
                    features);

            if (!TryConvertToInt32(
                    rawValue,
                    out int convertedValue))
            {
                continue;
            }

            value =
                convertedValue;

            resolvedFeatureCode =
                property.Name;

            return true;
        }

        return false;
    }

    private static bool TryConvertToInt32(
        object? value,
        out int convertedValue)
    {
        convertedValue =
            0;

        switch (value)
        {
            case int integerValue:
                convertedValue =
                    integerValue;
                return true;

            case short shortValue:
                convertedValue =
                    shortValue;
                return true;

            case byte byteValue:
                convertedValue =
                    byteValue;
                return true;

            case long longValue
                when longValue >= int.MinValue &&
                     longValue <= int.MaxValue:
                convertedValue =
                    (int)longValue;
                return true;

            case uint unsignedIntegerValue
                when unsignedIntegerValue <= int.MaxValue:
                convertedValue =
                    (int)unsignedIntegerValue;
                return true;

            default:
                return false;
        }
    }

    private static IReadOnlyList<string> DistinctCodes(
        IEnumerable<string> codes)
    {
        return codes
            .Where(
                code =>
                    !string.IsNullOrWhiteSpace(code))
            .Select(
                code =>
                    code.Trim())
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                code =>
                    code,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static string CreateFeatureCriterionCode(
        string category,
        string featureCode)
    {
        char[] normalizedCharacters =
            featureCode
                .Trim()
                .ToUpperInvariant()
                .Select(
                    character =>
                        char.IsLetterOrDigit(character)
                            ? character
                            : '_')
                .ToArray();

        return
            $"FEATURE.{category.ToUpperInvariant()}." +
            new string(
                normalizedCharacters);
    }

    private static void AssignSequentialRanks(
        IReadOnlyList<SolutionMethodRecommendation>
            recommendations)
    {
        for (int index = 0;
             index < recommendations.Count;
             index++)
        {
            recommendations[index].Rank =
                index + 1;
        }
    }

    private static void
        ValidateRecommendedScoreThreshold(
            double recommendedScoreThreshold)
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
    }
}