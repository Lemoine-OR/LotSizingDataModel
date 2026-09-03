using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Instance.Analysis;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Classification.Notation;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Classifies lot-sizing supply-chain instances using a
/// catalog of known problem-family definitions.
/// </summary>
/// <remarks>
/// The classifier first extracts a factual feature profile
/// from the supplied instance.
///
/// It then evaluates every enabled catalog definition and
/// produces:
/// <list type="bullet">
/// <item>
/// <description>exact known-family matches;</description>
/// </item>
/// <item>
/// <description>known extensions;</description>
/// </item>
/// <item>
/// <description>closest-known-family matches;</description>
/// </item>
/// <item>
/// <description>a primary classification when one can be selected unambiguously.</description>
/// </item>
/// </list>
///
/// This standard classifier evaluates the complete problem.
/// Specialized relaxation and subproblem recognizers may add
/// further matches to the resulting classification later.
/// </remarks>
public static class LotSizingProblemClassifier
{
    /// <summary>
    /// Gets the current version of the automatic
    /// problem-classification algorithm.
    /// </summary>
    public const string CurrentVersion = "1.0";

    private const double ScoreComparisonTolerance =
        1e-12;

    /// <summary>
    /// Classifies a supply-chain instance using a known
    /// problem-type catalog.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain instance to classify.
    /// </param>
    /// <param name="catalog">
    /// Catalog containing known problem families and
    /// classification rules.
    /// </param>
    /// <param name="supplyChainFingerprint">
    /// Optional fingerprint identifying the analyzed
    /// supply-chain state.
    /// </param>
    /// <param name="numericalTolerance">
    /// Non-negative finite tolerance used while extracting
    /// numerical features.
    /// </param>
    /// <returns>
    /// Persistent lot-sizing problem classification.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplyChain"/> or
    /// <paramref name="catalog"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the supplied catalog is structurally
    /// invalid.
    /// </exception>
    public static LotSizingProblemClassification Classify(
        SupplyChain supplyChain,
        KnownProblemTypeCatalog catalog,
        string supplyChainFingerprint = "",
        double numericalTolerance =
            LotSizingProblemFeatureExtractor
                .DefaultNumericalTolerance)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);
        ArgumentNullException.ThrowIfNull(catalog);

        catalog.EnsureValid();

        ProductStructureAnalysis productStructureAnalysis =
            ProductStructureAnalyzer.Analyze(
                supplyChain);

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(
                supplyChain,
                productStructureAnalysis,
                numericalTolerance);

        LotSizingProblemClassification classification =
            ClassifyCore(
                features,
                catalog,
                supplyChainFingerprint,
                productStructureAnalysis.Warnings,
                productStructureAnalysis.Errors);

        classification.Signature =
            LotSizingInstanceSignatureExtractor.Extract(
                supplyChain,
                features,
                productStructureAnalysis,
                numericalTolerance);

        return classification;
    }

    /// <summary>
    /// Classifies an existing lot-sizing problem-feature
    /// profile using a known problem-type catalog.
    /// </summary>
    /// <param name="features">
    /// Factual feature profile to classify.
    /// </param>
    /// <param name="catalog">
    /// Catalog containing known problem families and
    /// classification rules.
    /// </param>
    /// <param name="supplyChainFingerprint">
    /// Optional fingerprint identifying the supply-chain
    /// state from which the features were extracted.
    /// </param>
    /// <returns>
    /// Persistent lot-sizing problem classification.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="features"/> or
    /// <paramref name="catalog"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the supplied catalog is structurally
    /// invalid.
    /// </exception>
    public static LotSizingProblemClassification Classify(
        LotSizingProblemFeatures features,
        KnownProblemTypeCatalog catalog,
        string supplyChainFingerprint = "")
    {
        ArgumentNullException.ThrowIfNull(features);
        ArgumentNullException.ThrowIfNull(catalog);

        catalog.EnsureValid();

        return ClassifyCore(
            features,
            catalog,
            supplyChainFingerprint,
            Array.Empty<string>(),
            Array.Empty<string>());
    }

    private static LotSizingProblemClassification ClassifyCore(
        LotSizingProblemFeatures features,
        KnownProblemTypeCatalog catalog,
        string supplyChainFingerprint,
        IEnumerable<string> initialWarnings,
        IEnumerable<string> initialErrors)
    {
        var classification =
            new LotSizingProblemClassification(features)
            {
                ClassifierVersion =
                    CurrentVersion,

                CatalogName =
                    catalog.CatalogName,

                CatalogVersion =
                    catalog.CatalogVersion,

                ClassifiedAtUtc =
                    DateTime.UtcNow,

                SupplyChainFingerprint =
                    supplyChainFingerprint?.Trim() ??
                    string.Empty
            };

        var warnings =
            NormalizeMessages(initialWarnings);

        var errors =
            NormalizeMessages(initialErrors);

        if (!features.IsStructurallyUsable)
        {
            errors.Add(
                "The extracted feature profile does not " +
                "contain the minimum structural information " +
                "required for problem classification.");
        }

        if (errors.Count > 0)
        {
            classification.ReplaceWarnings(warnings);
            classification.ReplaceErrors(errors);

            classification.Status =
                ProblemClassificationStatus.Invalid;

            return classification;
        }

        var evaluatedMatches =
            new List<EvaluatedProblemTypeMatch>();

        foreach (KnownProblemTypeDefinition definition
                 in catalog.GetDefinitionsForClassification())
        {
            EvaluatedProblemTypeMatch? evaluatedMatch =
                EvaluateDefinition(
                    definition,
                    catalog,
                    features,
                    errors);

            if (evaluatedMatch is not null)
            {
                evaluatedMatches.Add(
                    evaluatedMatch);
            }
        }

        if (errors.Count > 0)
        {
            classification.ReplaceWarnings(warnings);
            classification.ReplaceErrors(errors);

            classification.Status =
                ProblemClassificationStatus.Invalid;

            return classification;
        }

        KnownProblemTypeMatch[] orderedMatches =
            evaluatedMatches
                .Select(
                    evaluatedMatch =>
                        evaluatedMatch.Match)
                .OrderBy(
                    match =>
                        GetMatchDisplayOrder(
                            match.MatchKind))
                .ThenByDescending(
                    match =>
                        match.Score)
                .ThenBy(
                    match =>
                        match.ProblemTypeCode,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        classification.ReplaceMatches(
            orderedMatches);

        SelectPrimaryMatchAndStatus(
            classification,
            evaluatedMatches);

        classification.ReplaceWarnings(
            warnings);

        classification.ReplaceErrors(
            errors);

        return classification;
    }

    private static EvaluatedProblemTypeMatch?
        EvaluateDefinition(
            KnownProblemTypeDefinition definition,
            KnownProblemTypeCatalog catalog,
            LotSizingProblemFeatures features,
            ICollection<string> errors)
    {
        var evidence =
            new List<ClassificationEvidence>();

        var additionalFeatureCodes =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        bool evaluationFailed =
            false;

        EvaluateStandardRuleCategory(
            definition.RequiredRuleCodes,
            catalog,
            features,
            isRequired: true,
            evidence,
            errors,
            ref evaluationFailed);

        EvaluateStandardRuleCategory(
            definition.OptionalRuleCodes,
            catalog,
            features,
            isRequired: false,
            evidence,
            errors,
            ref evaluationFailed);

        EvaluateExtensionRules(
            definition.ExtensionRuleCodes,
            catalog,
            features,
            evidence,
            additionalFeatureCodes,
            errors,
            ref evaluationFailed);

        EvaluateExclusionRules(
            definition.ExclusionRuleCodes,
            catalog,
            features,
            evidence,
            errors,
            ref evaluationFailed);

        if (evaluationFailed)
        {
            return null;
        }

        var match =
            new KnownProblemTypeMatch(
                problemTypeCode:
                    definition.Code,

                problemTypeName:
                    definition.Name,

                matchKind:
                    ProblemMatchKind.Unknown,

                scope:
                    definition.DefaultScope)
            {
                DefinitionVersion =
                    definition.DefinitionVersion
            };

        match.ReplaceEvidence(
            evidence);

        match.ReplaceAdditionalFeatureCodes(
            additionalFeatureCodes);

        match.UpdateScoreFromEvidence();

        if (!match.HasBlockingMismatches)
        {
            match.MatchKind =
                match.HasAdditionalFeatures
                    ? ProblemMatchKind.KnownExtension
                    : ProblemMatchKind.Exact;

            return new EvaluatedProblemTypeMatch(
                definition,
                match);
        }

        if (match.Score + ScoreComparisonTolerance <
            definition.ClosestMatchThreshold)
        {
            return null;
        }

        match.MatchKind =
            ProblemMatchKind.ClosestKnownFamily;

        return new EvaluatedProblemTypeMatch(
            definition,
            match);
    }

    private static void EvaluateStandardRuleCategory(
        IEnumerable<string> ruleCodes,
        KnownProblemTypeCatalog catalog,
        LotSizingProblemFeatures features,
        bool isRequired,
        ICollection<ClassificationEvidence> evidence,
        ICollection<string> errors,
        ref bool evaluationFailed)
    {
        foreach (string ruleCode in ruleCodes)
        {
            KnownProblemRuleDefinition? rule =
                catalog.FindRule(ruleCode);

            if (rule is null)
            {
                errors.Add(
                    $"Classification rule '{ruleCode}' " +
                    "could not be resolved.");

                evaluationFailed = true;
                continue;
            }

            bool succeeded =
                KnownProblemRuleEvaluator.TryEvaluate(
                    rule,
                    features,
                    isRequired,
                    out ClassificationEvidence?
                        evaluatedEvidence,
                    out string errorMessage);

            if (!succeeded ||
                evaluatedEvidence is null)
            {
                errors.Add(
                    $"Rule '{ruleCode}' could not be " +
                    $"evaluated: {errorMessage}");

                evaluationFailed = true;
                continue;
            }

            evidence.Add(
                evaluatedEvidence);
        }
    }

    private static void EvaluateExtensionRules(
        IEnumerable<string> ruleCodes,
        KnownProblemTypeCatalog catalog,
        LotSizingProblemFeatures features,
        ICollection<ClassificationEvidence> evidence,
        ISet<string> additionalFeatureCodes,
        ICollection<string> errors,
        ref bool evaluationFailed)
    {
        foreach (string ruleCode in ruleCodes)
        {
            KnownProblemRuleDefinition? rule =
                catalog.FindRule(ruleCode);

            if (rule is null)
            {
                errors.Add(
                    $"Extension rule '{ruleCode}' could not " +
                    "be resolved.");

                evaluationFailed = true;
                continue;
            }

            bool succeeded =
                KnownProblemRuleEvaluator.TryEvaluate(
                    rule,
                    features,
                    isRequired: false,
                    out ClassificationEvidence?
                        extensionEvidence,
                    out string errorMessage);

            if (!succeeded ||
                extensionEvidence is null)
            {
                errors.Add(
                    $"Extension rule '{ruleCode}' could not " +
                    $"be evaluated: {errorMessage}");

                evaluationFailed = true;
                continue;
            }

            /*
             * The absence of an extension must not reduce
             * the similarity score of the classical family.
             */
            extensionEvidence.Weight = 0.0;

            if (extensionEvidence.IsSatisfied)
            {
                additionalFeatureCodes.Add(
                    rule.FeatureCode);

                extensionEvidence.Comment =
                    "The condition identifies an additional " +
                    "feature not included in the classical " +
                    "problem-family definition.";
            }
            else
            {
                extensionEvidence.Comment =
                    "The possible extension is not present " +
                    "in the analyzed instance.";
            }

            evidence.Add(
                extensionEvidence);
        }
    }

    private static void EvaluateExclusionRules(
        IEnumerable<string> ruleCodes,
        KnownProblemTypeCatalog catalog,
        LotSizingProblemFeatures features,
        ICollection<ClassificationEvidence> evidence,
        ICollection<string> errors,
        ref bool evaluationFailed)
    {
        foreach (string ruleCode in ruleCodes)
        {
            KnownProblemRuleDefinition? rule =
                catalog.FindRule(ruleCode);

            if (rule is null)
            {
                errors.Add(
                    $"Exclusion rule '{ruleCode}' could not " +
                    "be resolved.");

                evaluationFailed = true;
                continue;
            }

            bool succeeded =
                KnownProblemRuleEvaluator.TryEvaluate(
                    rule,
                    features,
                    isRequired: true,
                    out ClassificationEvidence?
                        rawEvidence,
                    out string errorMessage);

            if (!succeeded ||
                rawEvidence is null)
            {
                errors.Add(
                    $"Exclusion rule '{ruleCode}' could not " +
                    $"be evaluated: {errorMessage}");

                evaluationFailed = true;
                continue;
            }

            /*
             * An exclusion rule describes a condition whose
             * presence contradicts the family.
             *
             * Therefore the evidence supports the match only
             * when the exclusion condition is absent.
             */
            var exclusionEvidence =
                new ClassificationEvidence(
                    featureCode:
                        rawEvidence.FeatureCode,

                    expectedValue:
                        "exclusion condition absent",

                    observedValue:
                        rawEvidence.IsSatisfied
                            ? "exclusion condition present"
                            : "exclusion condition absent",

                    isSatisfied:
                        !rawEvidence.IsSatisfied,

                    isRequired:
                        true,

                    description:
                        string.IsNullOrWhiteSpace(
                            rawEvidence.Description)
                            ? "The exclusion condition must " +
                              "be absent."
                            : rawEvidence.Description)
                {
                    RuleCode =
                        rawEvidence.RuleCode,

                    Weight =
                        0.0,

                    Comment =
                        $"Evaluated exclusion condition: " +
                        $"expected '{rawEvidence.ExpectedValue}', " +
                        $"observed '{rawEvidence.ObservedValue}'."
                };

            evidence.Add(
                exclusionEvidence);
        }
    }

    private static void SelectPrimaryMatchAndStatus(
        LotSizingProblemClassification classification,
        IReadOnlyCollection<EvaluatedProblemTypeMatch>
            evaluatedMatches)
    {
        EvaluatedProblemTypeMatch[] directCandidates =
            evaluatedMatches
                .Where(
                    evaluatedMatch =>
                        evaluatedMatch
                            .Definition
                            .CanBePrimaryMatch &&
                        evaluatedMatch
                            .Match
                            .IsDirectMatch &&
                        evaluatedMatch
                            .Match
                            .AppliesToCompleteProblem)
                .OrderByDescending(
                    evaluatedMatch =>
                        GetDirectMatchQuality(
                            evaluatedMatch.Match))
                .ThenByDescending(
                    evaluatedMatch =>
                        evaluatedMatch.Match.Score)
                .ThenByDescending(
                    evaluatedMatch =>
                        evaluatedMatch
                            .Definition
                            .RequiredRuleCodes
                            .Count)
                .ThenByDescending(
                    evaluatedMatch =>
                        evaluatedMatch
                            .Definition
                            .Priority)
                .ThenBy(
                    evaluatedMatch =>
                        evaluatedMatch.Match
                            .ProblemTypeCode,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (directCandidates.Length == 0)
        {
            bool hasRecognizedPartialStructure =
                classification.Matches.Any(
                    match =>
                        match.MatchKind ==
                            ProblemMatchKind
                                .RecognizedRelaxation ||
                        match.MatchKind ==
                            ProblemMatchKind
                                .RecognizedSubproblem);

            classification.Status =
                hasRecognizedPartialStructure
                    ? ProblemClassificationStatus
                        .PartiallyClassified
                    : ProblemClassificationStatus
                        .Unclassified;

            return;
        }

        EvaluatedProblemTypeMatch bestCandidate =
            directCandidates[0];

        EvaluatedProblemTypeMatch[] equivalentCandidates =
            directCandidates
                .Where(
                    candidate =>
                        AreEquivalentPrimaryCandidates(
                            bestCandidate,
                            candidate))
                .ToArray();

        if (equivalentCandidates.Length > 1)
        {
            classification.Status =
                ProblemClassificationStatus.Ambiguous;

            classification.Comment =
                "Several known problem families have the " +
                "same primary-selection rank: " +
                string.Join(
                    ", ",
                    equivalentCandidates.Select(
                        candidate =>
                            candidate.Match
                                .ProblemTypeCode)) +
                ".";

            return;
        }

        classification.SetPrimaryMatch(
            bestCandidate.Match);

        classification.ReplaceUnclassifiedFeatureCodes(
            bestCandidate.Match
                .AdditionalFeatureCodes);

        classification.Status =
            bestCandidate.Match.MatchKind ==
                ProblemMatchKind.Exact &&
            !classification.HasUnclassifiedFeatures
                ? ProblemClassificationStatus.Classified
                : ProblemClassificationStatus
                    .PartiallyClassified;
    }

    private static bool AreEquivalentPrimaryCandidates(
        EvaluatedProblemTypeMatch first,
        EvaluatedProblemTypeMatch second)
    {
        return
            GetDirectMatchQuality(first.Match) ==
                GetDirectMatchQuality(second.Match) &&

            Math.Abs(
                first.Match.Score -
                second.Match.Score) <=
                ScoreComparisonTolerance &&

            first.Definition.RequiredRuleCodes.Count ==
                second.Definition.RequiredRuleCodes.Count &&

            first.Definition.Priority ==
                second.Definition.Priority;
    }

    private static int GetDirectMatchQuality(
        KnownProblemTypeMatch match)
    {
        return match.MatchKind switch
        {
            ProblemMatchKind.Exact =>
                2,

            ProblemMatchKind.KnownExtension =>
                1,

            _ =>
                0
        };
    }

    private static int GetMatchDisplayOrder(
        ProblemMatchKind matchKind)
    {
        return matchKind switch
        {
            ProblemMatchKind.Exact =>
                0,

            ProblemMatchKind.KnownExtension =>
                1,

            ProblemMatchKind
                .RecognizedSubproblem =>
                    2,

            ProblemMatchKind
                .RecognizedRelaxation =>
                    3,

            ProblemMatchKind
                .ClosestKnownFamily =>
                    4,

            _ =>
                5
        };
    }

    private static List<string> NormalizeMessages(
        IEnumerable<string> messages)
    {
        ArgumentNullException.ThrowIfNull(messages);

        return messages
            .Where(
                message =>
                    !string.IsNullOrWhiteSpace(
                        message))
            .Select(
                message =>
                    message.Trim())
            .Distinct(
                StringComparer.Ordinal)
            .OrderBy(
                message =>
                    message,
                StringComparer.Ordinal)
            .ToList();
    }

    private sealed class EvaluatedProblemTypeMatch
    {
        public EvaluatedProblemTypeMatch(
            KnownProblemTypeDefinition definition,
            KnownProblemTypeMatch match)
        {
            Definition =
                definition ??
                throw new ArgumentNullException(
                    nameof(definition));

            Match =
                match ??
                throw new ArgumentNullException(
                    nameof(match));
        }

        public KnownProblemTypeDefinition Definition
        {
            get;
        }

        public KnownProblemTypeMatch Match
        {
            get;
        }
    }
}