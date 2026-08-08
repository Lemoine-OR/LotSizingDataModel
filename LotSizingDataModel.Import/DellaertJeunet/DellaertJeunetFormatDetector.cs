using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using LotSizingDataModel.Import.Common;

namespace LotSizingDataModel.Import.DellaertJeunet;

/// <summary>
/// Detects whether an XML source appears to use the
/// Dellaert–Jeunet benchmark-instance format.
/// </summary>
/// <remarks>
/// Detection is intentionally lighter than full
/// deserialization.
///
/// The detector examines:
/// <list type="bullet">
/// <item>
/// <description>
/// the root element name;
/// </description>
/// </item>
/// <item>
/// <description>
/// the absence of an XML namespace;
/// </description>
/// </item>
/// <item>
/// <description>
/// characteristic root-level elements such as
/// <c>ID</c>, <c>Name</c>, <c>BOMType</c>,
/// <c>NBPeriods</c> and <c>Items</c>;
/// </description>
/// </item>
/// <item>
/// <description>
/// characteristic item-level elements such as
/// <c>DepthInBOM</c>, <c>Demand</c>,
/// <c>SetupCost</c> and
/// <c>ListOfComponents</c>.
/// </description>
/// </item>
/// </list>
///
/// A positive detection result indicates that the document is
/// probably compatible with the importer. It does not prove
/// that the document is complete or semantically valid.
///
/// Validation and complete deserialization remain the
/// responsibility of dedicated services.
/// </remarks>
public sealed class DellaertJeunetFormatDetector
{
    private const int DefaultMaximumElementsToInspect =
        256;

    private int _maximumElementsToInspect =
        DefaultMaximumElementsToInspect;

    /// <summary>
    /// Initializes a detector with the recommended default
    /// settings.
    /// </summary>
    public DellaertJeunetFormatDetector()
    {
    }

    /// <summary>
    /// Initializes a detector.
    /// </summary>
    /// <param name="maximumElementsToInspect">
    /// Maximum number of XML elements inspected before
    /// detection stops.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when
    /// <paramref name="maximumElementsToInspect"/> is not
    /// strictly positive.
    /// </exception>
    public DellaertJeunetFormatDetector(
        int maximumElementsToInspect)
    {
        MaximumElementsToInspect =
            maximumElementsToInspect;
    }

    /// <summary>
    /// Gets or sets the maximum number of XML elements
    /// inspected during format detection.
    /// </summary>
    /// <remarks>
    /// The default value is 256 elements.
    ///
    /// This limit prevents format detection from scanning an
    /// entire large benchmark instance.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied value is not strictly
    /// positive.
    /// </exception>
    public int MaximumElementsToInspect
    {
        get =>
            _maximumElementsToInspect;

        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The maximum number of inspected XML " +
                    "elements must be strictly positive.");
            }

            _maximumElementsToInspect =
                value;
        }
    }

    /// <summary>
    /// Determines whether a file appears to contain a
    /// Dellaert–Jeunet XML instance.
    /// </summary>
    /// <param name="filePath">
    /// Path of the file to inspect.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the document appears
    /// compatible; otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the file does not exist.
    /// </exception>
    public bool IsMatch(
        string filePath)
    {
        ValidateFilePath(
            filePath);

        using FileStream stream =
            new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        return IsMatch(
            stream);
    }

    /// <summary>
    /// Determines whether a stream appears to contain a
    /// Dellaert–Jeunet XML instance.
    /// </summary>
    /// <param name="stream">
    /// Readable stream to inspect.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the document appears
    /// compatible; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed.
    ///
    /// When the stream supports seeking, its original
    /// position is restored before the method returns.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not readable.
    /// </exception>
    public bool IsMatch(
        Stream stream)
    {
        return Analyze(
            stream).IsMatch;
    }

    /// <summary>
    /// Analyzes a file and returns detailed format-detection
    /// information.
    /// </summary>
    /// <param name="filePath">
    /// Path of the file to inspect.
    /// </param>
    /// <returns>
    /// Detection result containing the compatibility score and
    /// diagnostics.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the file does not exist.
    /// </exception>
    public DellaertJeunetFormatDetectionResult Analyze(
        string filePath)
    {
        ValidateFilePath(
            filePath);

        using FileStream stream =
            new(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        DellaertJeunetFormatDetectionResult result =
            Analyze(
                stream);

        result.SourceName =
            filePath;

        return result;
    }

    /// <summary>
    /// Analyzes a stream and returns detailed format-detection
    /// information.
    /// </summary>
    /// <param name="stream">
    /// Readable stream to inspect.
    /// </param>
    /// <returns>
    /// Detection result containing the compatibility score and
    /// diagnostics.
    /// </returns>
    /// <remarks>
    /// The supplied stream is never disposed.
    ///
    /// When the stream supports seeking, its original
    /// position is restored before the method returns.
    /// </remarks>
    public DellaertJeunetFormatDetectionResult Analyze(
        Stream stream)
    {
        ValidateReadableStream(
            stream);

        long? initialPosition =
            GetCurrentPosition(
                stream);

        var result =
            new DellaertJeunetFormatDetectionResult
            {
                Format =
                    InstanceFormat.DellaertJeunetXml
            };

        try
        {
            using XmlReader reader =
                XmlReader.Create(
                    stream,
                    CreateReaderSettings());

            InspectDocument(
                reader,
                result);
        }
        catch (XmlException exception)
        {
            result.AddDiagnostic(
                new ImportDiagnostic(
                    ImportSeverity.Warning,
                    "DJ3001",
                    "The source is not a well-formed XML " +
                    "document.",
                    string.Empty,
                    exception.LineNumber,
                    exception.LinePosition)
                {
                    ExceptionType =
                        exception.GetType().FullName ??
                        exception.GetType().Name,

                    TechnicalDetails =
                        exception.Message
                });
        }
        catch (IOException exception)
        {
            result.AddDiagnostic(
                ImportDiagnostic.FromException(
                ImportSeverity.Warning,
                "DJ3002",
                "The source could not be inspected.",
                exception));
        }
        finally
        {
            RestorePosition(
                stream,
                initialPosition);
        }

        result.Complete();

        return result;
    }

    private void InspectDocument(
        XmlReader reader,
        DellaertJeunetFormatDetectionResult result)
    {
        int inspectedElementCount =
            0;

        int currentDepth =
            -1;

        bool insideItem =
            false;

        var rootElements =
            new HashSet<string>(
                StringComparer.Ordinal);

        var itemElements =
            new HashSet<string>(
                StringComparer.Ordinal);

        while (reader.Read() &&
               inspectedElementCount <
               MaximumElementsToInspect)
        {
            if (reader.NodeType !=
                XmlNodeType.Element)
            {
                continue;
            }

            inspectedElementCount++;

            if (!result.HasRootElement)
            {
                result.RootElementName =
                    reader.LocalName;

                result.RootNamespace =
                    reader.NamespaceURI;

                currentDepth =
                    reader.Depth;

                result.HasExpectedRootName =
                    string.Equals(
                        reader.LocalName,
                        "Instance",
                        StringComparison.Ordinal);

                result.HasExpectedNamespace =
                    string.IsNullOrEmpty(
                        reader.NamespaceURI);

                continue;
            }

            if (reader.Depth ==
                currentDepth + 1)
            {
                rootElements.Add(
                    reader.LocalName);
            }

            if (reader.Depth ==
                    currentDepth + 2 &&
                string.Equals(
                    reader.LocalName,
                    "Item",
                    StringComparison.Ordinal))
            {
                insideItem =
                    true;

                continue;
            }

            if (insideItem &&
                reader.Depth ==
                currentDepth + 3)
            {
                itemElements.Add(
                    reader.LocalName);
            }

            if (insideItem &&
                reader.Depth <=
                currentDepth + 1)
            {
                insideItem =
                    false;
            }

            if (HasSufficientEvidence(
                    result,
                    rootElements,
                    itemElements))
            {
                break;
            }
        }

        result.InspectedElementCount =
            inspectedElementCount;

        result.DetectedRootElements =
            rootElements
                .OrderBy(
                    name =>
                        name,
                    StringComparer.Ordinal)
                .ToList();

        result.DetectedItemElements =
            itemElements
                .OrderBy(
                    name =>
                        name,
                    StringComparer.Ordinal)
                .ToList();

        EvaluateEvidence(
            result,
            rootElements,
            itemElements);
    }

    private static bool HasSufficientEvidence(
        DellaertJeunetFormatDetectionResult result,
        IReadOnlySet<string> rootElements,
        IReadOnlySet<string> itemElements)
    {
        if (!result.HasExpectedRootName ||
            !result.HasExpectedNamespace)
        {
            return false;
        }

        return
            HasRootEvidence(
                rootElements) &&
            HasItemEvidence(
                itemElements);
    }

    private static void EvaluateEvidence(
        DellaertJeunetFormatDetectionResult result,
        IReadOnlySet<string> rootElements,
        IReadOnlySet<string> itemElements)
    {
        int score =
            0;

        if (result.HasExpectedRootName)
        {
            score +=
                30;
        }

        if (result.HasExpectedNamespace)
        {
            score +=
                5;
        }

        score +=
            CountMatches(
                rootElements,
                RootCharacteristicElements) *
            7;

        score +=
            CountMatches(
                itemElements,
                ItemCharacteristicElements) *
            5;

        result.CompatibilityScore =
            Math.Min(
                100,
                score);

        result.IsMatch =
            result.HasExpectedRootName &&
            result.HasExpectedNamespace &&
            HasRootEvidence(
                rootElements) &&
            HasItemEvidence(
                itemElements);

        if (!result.HasRootElement)
        {
            result.AddDiagnostic(
                ImportDiagnostic.Warning(
                    "DJ3101",
                    "The XML document does not contain a root " +
                    "element."));

            return;
        }

        if (!result.HasExpectedRootName)
        {
            result.AddDiagnostic(
                ImportDiagnostic.Information(
                    "DJ0101",
                    $"The root element is " +
                    $"'{result.RootElementName}' instead of " +
                    "'Instance'."));
        }

        if (!result.HasExpectedNamespace)
        {
            result.AddDiagnostic(
                ImportDiagnostic.Information(
                    "DJ0102",
                    "The root element uses an XML namespace, " +
                    "while the supported Dellaert–Jeunet " +
                    "format uses no namespace."));
        }

        if (result.IsMatch)
        {
            result.AddDiagnostic(
                ImportDiagnostic.Information(
                    "DJ0006",
                    "The source appears compatible with the " +
                    "Dellaert–Jeunet XML format."));
        }
        else if (result.CompatibilityScore >= 50)
        {
            result.AddDiagnostic(
                ImportDiagnostic.Warning(
                    "DJ1106",
                    "The source partially resembles the " +
                    "Dellaert–Jeunet XML format but lacks " +
                    "sufficient characteristic elements."));
        }
        else
        {
            result.AddDiagnostic(
                ImportDiagnostic.Information(
                    "DJ0103",
                    "The source does not appear compatible " +
                    "with the Dellaert–Jeunet XML format."));
        }
    }

    private static bool HasRootEvidence(
        IReadOnlySet<string> rootElements)
    {
        return
            rootElements.Contains(
                "NBPeriods") &&
            rootElements.Contains(
                "Items") &&
            (
                rootElements.Contains(
                    "ID") ||
                rootElements.Contains(
                    "BOMType")
            );
    }

    private static bool HasItemEvidence(
        IReadOnlySet<string> itemElements)
    {
        return
            itemElements.Contains(
                "Id") &&
            itemElements.Contains(
                "Demand") &&
            (
                itemElements.Contains(
                    "DepthInBOM") ||
                itemElements.Contains(
                    "ListOfComponents")
            );
    }

    private static int CountMatches(
        IReadOnlySet<string> detectedElements,
        IEnumerable<string> expectedElements)
    {
        return expectedElements.Count(
            detectedElements.Contains);
    }

    private static XmlReaderSettings
        CreateReaderSettings()
    {
        return new XmlReaderSettings
        {
            DtdProcessing =
                DtdProcessing.Prohibit,

            XmlResolver =
                null,

            IgnoreComments =
                true,

            IgnoreProcessingInstructions =
                true,

            IgnoreWhitespace =
                true,

            ValidationType =
                ValidationType.None,

            CloseInput =
                false,

            MaxCharactersFromEntities =
                0,

            CheckCharacters =
                true,

            ConformanceLevel =
                ConformanceLevel.Document
        };
    }

    private static void ValidateFilePath(
        string filePath)
    {
        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            throw new ArgumentException(
                "A source file path is required.",
                nameof(filePath));
        }

        if (!File.Exists(
                filePath))
        {
            throw new FileNotFoundException(
                "The source file does not exist.",
                filePath);
        }
    }

    private static void ValidateReadableStream(
        Stream stream)
    {
        ArgumentNullException.ThrowIfNull(
            stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException(
                "The source stream must be readable.",
                nameof(stream));
        }
    }

    private static long? GetCurrentPosition(
        Stream stream)
    {
        return
            stream.CanSeek
                ? stream.Position
                : null;
    }

    private static void RestorePosition(
        Stream stream,
        long? initialPosition)
    {
        if (!initialPosition.HasValue ||
            !stream.CanSeek)
        {
            return;
        }

        stream.Position =
            initialPosition.Value;
    }

    private static readonly string[]
        RootCharacteristicElements =
        {
            "Article",
            "ID",
            "Name",
            "InstanceType",
            "BOMType",
            "NBPeriods",
            "Items"
        };

    private static readonly string[]
        ItemCharacteristicElements =
        {
            "Id",
            "Name",
            "DepthInBOM",
            "Demand",
            "SetupCost",
            "HoldingCost",
            "ProductionCost",
            "ListOfComponents"
        };
}