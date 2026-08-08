using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using LotSizingDataModel.Core;
using LotSizingDataModel.Instance.Analysis;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Metadata;

namespace LotSizingDataModel.Instance.Creation;

/// <summary>
/// Creates lot-sizing problem instances and initializes their
/// automatically derived metadata.
/// </summary>
/// <remarks>
/// The factory can:
/// <list type="bullet">
/// <item>
/// <description>
/// create a root <see cref="LotSizingInstance"/> object;
/// </description>
/// </item>
/// <item>
/// <description>
/// calculate a reproducible fingerprint of the supply-chain
/// data;
/// </description>
/// </item>
/// <item>
/// <description>
/// analyze the bill-of-materials graph;
/// </description>
/// </item>
/// <item>
/// <description>
/// classify the problem using a known-problem-type catalog.
/// </description>
/// </item>
/// </list>
///
/// Known results are not created automatically. An instance
/// containing no known result remains valid.
/// </remarks>
public static class LotSizingInstanceFactory
{
    /// <summary>
    /// Gets the identifier of the fingerprint algorithm and
    /// serialization convention used by this factory.
    /// </summary>
    public const string FingerprintScheme =
        "SHA256-XML-1";

    /// <summary>
    /// Creates and automatically analyzes a lot-sizing
    /// problem instance.
    /// </summary>
    /// <param name="instanceId">
    /// Stable identifier of the instance.
    /// </param>
    /// <param name="supplyChain">
    /// Supply-chain model defining the problem data.
    /// </param>
    /// <param name="name">
    /// Optional human-readable name of the instance.
    /// </param>
    /// <param name="declaredProductStructureType">
    /// Optional product-structure type declared by the
    /// instance author or source.
    /// </param>
    /// <param name="catalog">
    /// Optional problem-type catalog.
    ///
    /// When this argument is <see langword="null"/> and
    /// classification is enabled, the standard catalog is
    /// created automatically.
    /// </param>
    /// <param name="analyzeProductStructure">
    /// Value indicating whether the bill-of-materials graph
    /// must be analyzed automatically.
    /// </param>
    /// <param name="classifyProblem">
    /// Value indicating whether the problem must be classified
    /// automatically.
    /// </param>
    /// <param name="createdBy">
    /// Optional name of the person, organization or software
    /// component creating the instance.
    /// </param>
    /// <param name="numericalTolerance">
    /// Non-negative finite tolerance used during numerical
    /// feature extraction.
    /// </param>
    /// <returns>
    /// Newly created lot-sizing instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="instanceId"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplyChain"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="numericalTolerance"/> is
    /// negative or not finite.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the supply chain cannot be serialized for
    /// fingerprint generation or when the selected catalog is
    /// invalid.
    /// </exception>
    public static LotSizingInstance Create(
        string instanceId,
        SupplyChain supplyChain,
        string name = "",
        ProductStructureType declaredProductStructureType =
            ProductStructureType.Unknown,
        KnownProblemTypeCatalog? catalog = null,
        bool analyzeProductStructure = true,
        bool classifyProblem = true,
        string createdBy = "",
        double numericalTolerance =
            LotSizingProblemFeatureExtractor
                .DefaultNumericalTolerance)
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            throw new ArgumentException(
                "A lot-sizing instance identifier is required.",
                nameof(instanceId));
        }

        ArgumentNullException.ThrowIfNull(supplyChain);

        ValidateNumericalTolerance(
            numericalTolerance);

        string supplyChainFingerprint =
            ComputeSupplyChainFingerprint(
                supplyChain);

        var instance =
            new LotSizingInstance(
                instanceId:
                    instanceId,

                supplyChain:
                    supplyChain,

                name:
                    name)
            {
                CreatedBy =
                    createdBy?.Trim() ??
                    string.Empty,

                ProductStructure =
                    new ProductStructureDescriptor(
                        declaredProductStructureType)
            };

        if (analyzeProductStructure)
        {
            ProductStructureAnalyzer.AnalyzeAndUpdate(
                supplyChain:
                    instance.SupplyChain,

                descriptor:
                    instance.ProductStructure,

                supplyChainFingerprint:
                    supplyChainFingerprint);
        }

        if (classifyProblem)
        {
            KnownProblemTypeCatalog effectiveCatalog =
                catalog ??
                KnownProblemTypeCatalogFactory
                    .CreateStandardCatalog();

            instance.ProblemClassification =
                LotSizingProblemClassifier.Classify(
                    supplyChain:
                        instance.SupplyChain,

                    catalog:
                        effectiveCatalog,

                    supplyChainFingerprint:
                        supplyChainFingerprint,

                    numericalTolerance:
                        numericalTolerance);
        }

        return instance;
    }

    /// <summary>
    /// Creates a lot-sizing instance without automatically
    /// analyzing or classifying it.
    /// </summary>
    /// <param name="instanceId">
    /// Stable identifier of the instance.
    /// </param>
    /// <param name="supplyChain">
    /// Supply-chain model defining the problem data.
    /// </param>
    /// <param name="name">
    /// Optional human-readable name of the instance.
    /// </param>
    /// <param name="declaredProductStructureType">
    /// Optional product-structure type declared by the source.
    /// </param>
    /// <param name="createdBy">
    /// Optional creator information.
    /// </param>
    /// <returns>
    /// Newly created, unanalyzed lot-sizing instance.
    /// </returns>
    public static LotSizingInstance CreateUnanalyzed(
        string instanceId,
        SupplyChain supplyChain,
        string name = "",
        ProductStructureType declaredProductStructureType =
            ProductStructureType.Unknown,
        string createdBy = "")
    {
        return Create(
            instanceId:
                instanceId,

            supplyChain:
                supplyChain,

            name:
                name,

            declaredProductStructureType:
                declaredProductStructureType,

            catalog:
                null,

            analyzeProductStructure:
                false,

            classifyProblem:
                false,

            createdBy:
                createdBy);
    }

    /// <summary>
    /// Recalculates the product-structure analysis and problem
    /// classification of an existing instance.
    /// </summary>
    /// <param name="instance">
    /// Instance whose derived data must be refreshed.
    /// </param>
    /// <param name="catalog">
    /// Optional problem-type catalog.
    ///
    /// When this argument is <see langword="null"/> and
    /// classification is enabled, the standard catalog is
    /// used.
    /// </param>
    /// <param name="analyzeProductStructure">
    /// Value indicating whether the persistent
    /// product-structure descriptor must be updated.
    /// </param>
    /// <param name="classifyProblem">
    /// Value indicating whether the problem classification
    /// must be recalculated.
    /// </param>
    /// <param name="numericalTolerance">
    /// Non-negative finite tolerance used during numerical
    /// feature extraction.
    /// </param>
    /// <returns>
    /// Fingerprint calculated for the current supply-chain
    /// data.
    /// </returns>
    /// <remarks>
    /// When one of the two analyses is not refreshed, an
    /// existing result based on a different fingerprint is
    /// marked as outdated.
    ///
    /// Known results are not modified.
    /// </remarks>
    public static string RefreshDerivedData(
        LotSizingInstance instance,
        KnownProblemTypeCatalog? catalog = null,
        bool analyzeProductStructure = true,
        bool classifyProblem = true,
        double numericalTolerance =
            LotSizingProblemFeatureExtractor
                .DefaultNumericalTolerance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        ValidateNumericalTolerance(
            numericalTolerance);

        string supplyChainFingerprint =
            ComputeSupplyChainFingerprint(
                instance.SupplyChain);

        if (analyzeProductStructure)
        {
            ProductStructureAnalyzer.AnalyzeAndUpdate(
                supplyChain:
                    instance.SupplyChain,

                descriptor:
                    instance.ProductStructure,

                supplyChainFingerprint:
                    supplyChainFingerprint);
        }
        else
        {
            MarkProductStructureAsOutdatedWhenRequired(
                instance.ProductStructure,
                supplyChainFingerprint);
        }

        if (classifyProblem)
        {
            KnownProblemTypeCatalog effectiveCatalog =
                catalog ??
                KnownProblemTypeCatalogFactory
                    .CreateStandardCatalog();

            instance.ProblemClassification =
                LotSizingProblemClassifier.Classify(
                    supplyChain:
                        instance.SupplyChain,

                    catalog:
                        effectiveCatalog,

                    supplyChainFingerprint:
                        supplyChainFingerprint,

                    numericalTolerance:
                        numericalTolerance);
        }
        else
        {
            MarkClassificationAsOutdatedWhenRequired(
                instance.ProblemClassification,
                supplyChainFingerprint);
        }

        instance.ClearSolutionMethodRecommendationReport();

        instance.ModifiedAtUtc =
            DateTime.UtcNow;

        return supplyChainFingerprint;
    }

    /// <summary>
    /// Recalculates only the persistent product-structure
    /// analysis of an instance.
    /// </summary>
    /// <param name="instance">
    /// Instance to analyze.
    /// </param>
    /// <returns>
    /// Detailed product-structure analysis.
    /// </returns>
    public static ProductStructureAnalysis
        RefreshProductStructure(
            LotSizingInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        string fingerprint =
            ComputeSupplyChainFingerprint(
                instance.SupplyChain);

        ProductStructureAnalysis analysis =
            ProductStructureAnalyzer.AnalyzeAndUpdate(
                supplyChain:
                    instance.SupplyChain,

                descriptor:
                    instance.ProductStructure,

                supplyChainFingerprint:
                    fingerprint);

        instance.ClearSolutionMethodRecommendationReport();

        instance.ModifiedAtUtc =
            DateTime.UtcNow;

        return analysis;
    }

    /// <summary>
    /// Recalculates only the problem classification of an
    /// instance.
    /// </summary>
    /// <param name="instance">
    /// Instance to classify.
    /// </param>
    /// <param name="catalog">
    /// Optional problem-type catalog.
    ///
    /// When omitted, the standard catalog is used.
    /// </param>
    /// <param name="numericalTolerance">
    /// Non-negative finite tolerance used during numerical
    /// feature extraction.
    /// </param>
    /// <returns>
    /// Newly generated problem classification.
    /// </returns>
    public static LotSizingProblemClassification
        RefreshProblemClassification(
            LotSizingInstance instance,
            KnownProblemTypeCatalog? catalog = null,
            double numericalTolerance =
                LotSizingProblemFeatureExtractor
                    .DefaultNumericalTolerance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        ValidateNumericalTolerance(
            numericalTolerance);

        KnownProblemTypeCatalog effectiveCatalog =
            catalog ??
            KnownProblemTypeCatalogFactory
                .CreateStandardCatalog();

        string fingerprint =
            ComputeSupplyChainFingerprint(
                instance.SupplyChain);

        LotSizingProblemClassification classification =
            LotSizingProblemClassifier.Classify(
                supplyChain:
                    instance.SupplyChain,

                catalog:
                    effectiveCatalog,

                supplyChainFingerprint:
                    fingerprint,

                numericalTolerance:
                    numericalTolerance);

        instance.ProblemClassification =
            classification;

        instance.ClearSolutionMethodRecommendationReport();

        instance.ModifiedAtUtc =
            DateTime.UtcNow;

        return classification;
    }

    /// <summary>
    /// Calculates a SHA-256 fingerprint of a supply-chain
    /// object using its compact XML serialization.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain object whose fingerprint must be
    /// calculated.
    /// </param>
    /// <returns>
    /// Fingerprint formatted as
    /// <c>SHA256-XML-1:HEXADECIMAL_HASH</c>.
    /// </returns>
    /// <remarks>
    /// Collection order is significant. Two semantically
    /// equivalent models whose collections are ordered
    /// differently may therefore produce different
    /// fingerprints.
    ///
    /// This behavior is intentional because collection order
    /// is part of the serialized instance representation.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplyChain"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the supply chain cannot be serialized.
    /// </exception>
    public static string ComputeSupplyChainFingerprint(
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        byte[] serializedData =
            SerializeSupplyChainForFingerprint(
                supplyChain);

        byte[] hash;

        using (SHA256 sha256 =
               SHA256.Create())
        {
            hash =
                sha256.ComputeHash(
                    serializedData);
        }

        return
            $"{FingerprintScheme}:" +
            Convert.ToHexString(hash);
    }

    private static byte[]
        SerializeSupplyChainForFingerprint(
            SupplyChain supplyChain)
    {
        var serializer =
            new XmlSerializer(
                typeof(SupplyChain));

        var namespaces =
            new XmlSerializerNamespaces();

        namespaces.Add(
            string.Empty,
            string.Empty);

        var settings =
            new XmlWriterSettings
            {
                Encoding =
                    new UTF8Encoding(
                        encoderShouldEmitUTF8Identifier:
                            false),

                Indent =
                    false,

                OmitXmlDeclaration =
                    false,

                NewLineHandling =
                    NewLineHandling.None,

                CloseOutput =
                    false
            };

        try
        {
            using var stream =
                new MemoryStream();

            using (XmlWriter writer =
                   XmlWriter.Create(
                       stream,
                       settings))
            {
                serializer.Serialize(
                    writer,
                    supplyChain,
                    namespaces);
            }

            return stream.ToArray();
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException(
                "The supply-chain model could not be " +
                "serialized for fingerprint generation.",
                exception);
        }
    }

    private static void
        MarkProductStructureAsOutdatedWhenRequired(
            ProductStructureDescriptor descriptor,
            string currentFingerprint)
    {
        if (!descriptor.HasBeenAnalyzed &&
            !descriptor.HasDetectedType)
        {
            return;
        }

        bool fingerprintMatches =
            !string.IsNullOrWhiteSpace(
                descriptor.SupplyChainFingerprint) &&
            string.Equals(
                descriptor.SupplyChainFingerprint,
                currentFingerprint,
                StringComparison.Ordinal);

        if (!fingerprintMatches)
        {
            descriptor.MarkAsOutdated();
        }
    }

    private static void
        MarkClassificationAsOutdatedWhenRequired(
            LotSizingProblemClassification classification,
            string currentFingerprint)
    {
        if (classification.Status ==
            ProblemClassificationStatus.NotAnalyzed)
        {
            return;
        }

        if (!classification
                .MatchesSupplyChainFingerprint(
                    currentFingerprint))
        {
            classification.MarkAsOutdated();
        }
    }

    private static void ValidateNumericalTolerance(
        double numericalTolerance)
    {
        if (!double.IsFinite(numericalTolerance) ||
            numericalTolerance < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(numericalTolerance),
                numericalTolerance,
                "The numerical tolerance must be finite " +
                "and non-negative.");
        }
    }
}