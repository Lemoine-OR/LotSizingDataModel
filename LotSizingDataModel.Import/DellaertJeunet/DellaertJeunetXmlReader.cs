using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using LotSizingDataModel.Import.Common;
using LotSizingDataModel.Import.DellaertJeunet.XmlModel;

namespace LotSizingDataModel.Import.DellaertJeunet;

/// <summary>
/// Reads and deserializes Dellaert–Jeunet XML benchmark
/// instances.
/// </summary>
/// <remarks>
/// This reader applies restrictive XML settings:
/// <list type="bullet">
/// <item>
/// <description>
/// document type definitions are prohibited;
/// </description>
/// </item>
/// <item>
/// <description>
/// external XML resource resolution is disabled;
/// </description>
/// </item>
/// <item>
/// <description>
/// insignificant white-space and comments are ignored;
/// </description>
/// </item>
/// <item>
/// <description>
/// caller-owned streams remain open;
/// </description>
/// </item>
/// <item>
/// <description>
/// document size is limited to reduce the risk of excessive
/// memory consumption.
/// </description>
/// </item>
/// </list>
///
/// The reader only creates source-format DTOs. It does not
/// validate the benchmark semantics and does not create domain
/// objects.
/// </remarks>
public sealed class DellaertJeunetXmlReader
{
    private const long DefaultMaximumDocumentCharacters =
        100_000_000L;

    private static readonly XmlSerializer Serializer =
        new(
            typeof(DellaertJeunetXmlInstance));

    private long _maximumDocumentCharacters =
        DefaultMaximumDocumentCharacters;

    /// <summary>
    /// Initializes a Dellaert–Jeunet XML reader with the
    /// default security limits.
    /// </summary>
    public DellaertJeunetXmlReader()
    {
    }

    /// <summary>
    /// Initializes a Dellaert–Jeunet XML reader.
    /// </summary>
    /// <param name="maximumDocumentCharacters">
    /// Maximum number of characters allowed in one XML
    /// document.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when
    /// <paramref name="maximumDocumentCharacters"/> is not
    /// strictly positive.
    /// </exception>
    public DellaertJeunetXmlReader(
        long maximumDocumentCharacters)
    {
        MaximumDocumentCharacters =
            maximumDocumentCharacters;
    }

    /// <summary>
    /// Gets or sets the maximum number of characters allowed
    /// in one XML document.
    /// </summary>
    /// <remarks>
    /// The default value is 100 million characters.
    ///
    /// This limit concerns the expanded XML document and not
    /// only the physical file size.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied value is not strictly
    /// positive.
    /// </exception>
    public long MaximumDocumentCharacters
    {
        get =>
            _maximumDocumentCharacters;

        set
        {
            if (value <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The maximum XML document size must be " +
                    "strictly positive.");
            }

            _maximumDocumentCharacters =
                value;
        }
    }

    /// <summary>
    /// Reads a Dellaert–Jeunet XML instance from a file.
    /// </summary>
    /// <param name="filePath">
    /// Path of the XML file to read.
    /// </param>
    /// <returns>
    /// Deserialized source instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the file does not exist.
    /// </exception>
    /// <exception cref="DellaertJeunetXmlReadException">
    /// Thrown when the document cannot be deserialized.
    /// </exception>
    public DellaertJeunetXmlInstance Read(
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

        return Read(
            stream,
            filePath);
    }

    /// <summary>
    /// Reads a Dellaert–Jeunet XML instance from a stream.
    /// </summary>
    /// <param name="stream">
    /// Readable XML source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name used in exception messages.
    /// </param>
    /// <returns>
    /// Deserialized source instance.
    /// </returns>
    /// <remarks>
    /// The supplied stream is not closed or disposed.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not readable.
    /// </exception>
    /// <exception cref="DellaertJeunetXmlReadException">
    /// Thrown when the document cannot be deserialized.
    /// </exception>
    public DellaertJeunetXmlInstance Read(
        Stream stream,
        string? sourceName = null)
    {
        ValidateReadableStream(
            stream);

        long? initialPosition =
            GetCurrentPosition(
                stream);

        try
        {
            using XmlReader xmlReader =
                CreateXmlReader(
                    stream);

            object? deserializedObject =
                Serializer.Deserialize(
                    xmlReader);

            if (deserializedObject is not
                DellaertJeunetXmlInstance instance)
            {
                throw new DellaertJeunetXmlReadException(
                    BuildSourceMessage(
                        sourceName,
                        "The XML document did not produce a " +
                        "Dellaert–Jeunet source instance."));
            }

            return instance;
        }
        catch (DellaertJeunetXmlReadException)
        {
            throw;
        }
        catch (InvalidOperationException exception)
        {
            throw BuildReadException(
                sourceName,
                exception);
        }
        catch (XmlException exception)
        {
            throw BuildReadException(
                sourceName,
                exception);
        }
        catch (IOException exception)
        {
            throw BuildReadException(
                sourceName,
                exception);
        }
        catch (UnauthorizedAccessException exception)
        {
            throw BuildReadException(
                sourceName,
                exception);
        }
        finally
        {
            RestorePosition(
                stream,
                initialPosition);
        }
    }

    /// <summary>
    /// Attempts to read a Dellaert–Jeunet XML instance from a
    /// file without throwing for expected parsing errors.
    /// </summary>
    /// <param name="filePath">
    /// Path of the XML file to read.
    /// </param>
    /// <param name="instance">
    /// Deserialized source instance when reading succeeds.
    /// </param>
    /// <param name="diagnostics">
    /// Diagnostics describing the result.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the document is read
    /// successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryRead(
        string filePath,
        out DellaertJeunetXmlInstance? instance,
        out IReadOnlyList<ImportDiagnostic> diagnostics)
    {
        instance =
            null;

        var mutableDiagnostics =
            new List<ImportDiagnostic>();

        if (string.IsNullOrWhiteSpace(
                filePath))
        {
            mutableDiagnostics.Add(
                ImportDiagnostic.Fatal(
                    "DJ2901",
                    "A source file path is required."));

            diagnostics =
                mutableDiagnostics;

            return false;
        }

        if (!File.Exists(
                filePath))
        {
            mutableDiagnostics.Add(
                new ImportDiagnostic(
                    ImportSeverity.Fatal,
                    "DJ2902",
                    "The source XML file does not exist.",
                    filePath));

            diagnostics =
                mutableDiagnostics;

            return false;
        }

        try
        {
            instance =
                Read(
                    filePath);

            mutableDiagnostics.Add(
                new ImportDiagnostic(
                    ImportSeverity.Information,
                    "DJ0901",
                    "The Dellaert–Jeunet XML document was " +
                    "deserialized successfully.",
                    filePath));

            diagnostics =
                mutableDiagnostics;

            return true;
        }
        catch (DellaertJeunetXmlReadException exception)
        {
            mutableDiagnostics.Add(
                CreateDiagnostic(
                    exception,
                    filePath));

            diagnostics =
                mutableDiagnostics;

            return false;
        }
    }

    /// <summary>
    /// Attempts to read a Dellaert–Jeunet XML instance from a
    /// stream without throwing for expected parsing errors.
    /// </summary>
    /// <param name="stream">
    /// Readable XML source stream.
    /// </param>
    /// <param name="sourceName">
    /// Optional source name.
    /// </param>
    /// <param name="instance">
    /// Deserialized source instance when reading succeeds.
    /// </param>
    /// <param name="diagnostics">
    /// Diagnostics describing the result.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the document is read
    /// successfully; otherwise, <see langword="false"/>.
    /// </returns>
    public bool TryRead(
        Stream stream,
        string? sourceName,
        out DellaertJeunetXmlInstance? instance,
        out IReadOnlyList<ImportDiagnostic> diagnostics)
    {
        instance =
            null;

        var mutableDiagnostics =
            new List<ImportDiagnostic>();

        if (stream is null)
        {
            mutableDiagnostics.Add(
                ImportDiagnostic.Fatal(
                    "DJ2903",
                    "A source stream is required."));

            diagnostics =
                mutableDiagnostics;

            return false;
        }

        if (!stream.CanRead)
        {
            mutableDiagnostics.Add(
                ImportDiagnostic.Fatal(
                    "DJ2904",
                    "The source stream is not readable."));

            diagnostics =
                mutableDiagnostics;

            return false;
        }

        try
        {
            instance =
                Read(
                    stream,
                    sourceName);

            mutableDiagnostics.Add(
                new ImportDiagnostic(
                    ImportSeverity.Information,
                    "DJ0901",
                    "The Dellaert–Jeunet XML document was " +
                    "deserialized successfully.",
                    sourceName ??
                    string.Empty));

            diagnostics =
                mutableDiagnostics;

            return true;
        }
        catch (DellaertJeunetXmlReadException exception)
        {
            mutableDiagnostics.Add(
                CreateDiagnostic(
                    exception,
                    sourceName));

            diagnostics =
                mutableDiagnostics;

            return false;
        }
    }

    /// <summary>
    /// Determines whether a file appears to contain a
    /// Dellaert–Jeunet XML document.
    /// </summary>
    /// <param name="filePath">
    /// Path of the XML file to inspect.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the root element is named
    /// <c>Instance</c>; otherwise, <see langword="false"/>.
    /// </returns>
    public bool HasExpectedRootElement(
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

        return HasExpectedRootElement(
            stream);
    }

    /// <summary>
    /// Determines whether a stream appears to contain a
    /// Dellaert–Jeunet XML document.
    /// </summary>
    /// <param name="stream">
    /// Readable XML stream to inspect.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the root element is named
    /// <c>Instance</c>; otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// When the stream supports seeking, its original position
    /// is restored before this method returns.
    /// </remarks>
    public bool HasExpectedRootElement(
        Stream stream)
    {
        ValidateReadableStream(
            stream);

        long? initialPosition =
            GetCurrentPosition(
                stream);

        try
        {
            using XmlReader reader =
                CreateXmlReader(
                    stream);

            while (reader.Read())
            {
                if (reader.NodeType !=
                    XmlNodeType.Element)
                {
                    continue;
                }

                return
                    string.Equals(
                        reader.LocalName,
                        "Instance",
                        StringComparison.Ordinal) &&
                    string.IsNullOrEmpty(
                        reader.NamespaceURI);
            }

            return false;
        }
        catch (
            XmlException)
        {
            return false;
        }
        finally
        {
            RestorePosition(
                stream,
                initialPosition);
        }
    }

    private XmlReader CreateXmlReader(
        Stream stream)
    {
        XmlReaderSettings settings =
            CreateSettings();

        return XmlReader.Create(
            stream,
            settings);
    }

    private XmlReaderSettings CreateSettings()
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

            MaxCharactersInDocument =
                MaximumDocumentCharacters,

            MaxCharactersFromEntities =
                0,

            CheckCharacters =
                true,

            ConformanceLevel =
                ConformanceLevel.Document
        };
    }

    private static DellaertJeunetXmlReadException
        BuildReadException(
            string? sourceName,
            Exception exception)
    {
        Exception effectiveException =
            exception.InnerException ??
            exception;

        string message =
            BuildSourceMessage(
                sourceName,
                "The Dellaert–Jeunet XML document could not " +
                "be read.");

        if (effectiveException is
            XmlException xmlException)
        {
            return new DellaertJeunetXmlReadException(
                message,
                xmlException.LineNumber,
                xmlException.LinePosition,
                effectiveException);
        }

        return new DellaertJeunetXmlReadException(
            message,
            effectiveException);
    }

    private static ImportDiagnostic CreateDiagnostic(
        DellaertJeunetXmlReadException exception,
        string? sourceName)
    {
        var diagnostic =
            new ImportDiagnostic(
                ImportSeverity.Fatal,
                "DJ2905",
                exception.Message,
                sourceName ??
                string.Empty,
                exception.LineNumber,
                exception.LinePosition)
            {
                ExceptionType =
                    exception.GetType().FullName ??
                    exception.GetType().Name,

                TechnicalDetails =
                    exception.InnerException?.Message ??
                    string.Empty
            };

        return diagnostic;
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
                "The source XML file does not exist.",
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
        if (!stream.CanSeek)
        {
            return null;
        }

        return stream.Position;
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

    private static string BuildSourceMessage(
        string? sourceName,
        string message)
    {
        if (string.IsNullOrWhiteSpace(
                sourceName))
        {
            return message;
        }

        return
            message +
            " Source: " +
            sourceName.Trim();
    }
}