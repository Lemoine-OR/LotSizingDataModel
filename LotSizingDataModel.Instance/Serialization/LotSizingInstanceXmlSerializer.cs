using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using LotSizingDataModel.Instance.Validation;

namespace LotSizingDataModel.Instance.Serialization;

/// <summary>
/// Serializes and deserializes complete lot-sizing problem
/// instances using XML.
/// </summary>
/// <remarks>
/// The serializer supports:
/// <list type="bullet">
/// <item>
/// <description>serialization to a file;</description>
/// </item>
/// <item>
/// <description>serialization to a stream;</description>
/// </item>
/// <item>
/// <description>serialization to an XML string;</description>
/// </item>
/// <item>
/// <description>deserialization from the same sources;</description>
/// </item>
/// <item>
/// <description>
/// optional validation before writing and after reading.
/// </description>
/// </item>
/// </list>
///
/// File serialization uses UTF-8 without a byte-order mark.
/// Files are first written to a temporary file and then moved
/// to their final location in order to reduce the risk of
/// leaving a partially written instance file.
/// </remarks>
public static class LotSizingInstanceXmlSerializer
{
    /// <summary>
    /// Gets the encoding used when writing XML instance files.
    /// </summary>
    public static Encoding XmlEncoding { get; } =
        new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false);

    /// <summary>
    /// Serializes an instance to an XML file.
    /// </summary>
    /// <param name="instance">
    /// Instance to serialize.
    /// </param>
    /// <param name="filePath">
    /// Destination XML file path.
    /// </param>
    /// <param name="validateBeforeSerialization">
    /// Value indicating whether the instance must be validated
    /// before serialization.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether validation must compare
    /// recorded fingerprints with the current supply-chain
    /// fingerprint.
    /// </param>
    /// <param name="indent">
    /// Value indicating whether the generated XML must be
    /// indented.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="DirectoryNotFoundException">
    /// Thrown when the destination directory does not exist.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when validation fails or the instance cannot be
    /// serialized.
    /// </exception>
    public static void SerializeToFile(
        LotSizingInstance instance,
        string filePath,
        bool validateBeforeSerialization = true,
        bool validateCurrentFingerprint = true,
        bool indent = true)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "A destination file path is required.",
                nameof(filePath));
        }

        string fullFilePath =
            Path.GetFullPath(
                filePath.Trim());

        string? directoryPath =
            Path.GetDirectoryName(
                fullFilePath);

        if (!string.IsNullOrWhiteSpace(directoryPath) &&
            !Directory.Exists(directoryPath))
        {
            throw new DirectoryNotFoundException(
                $"The destination directory " +
                $"'{directoryPath}' does not exist.");
        }

        string temporaryFilePath =
            fullFilePath +
            "." +
            Guid.NewGuid().ToString("N") +
            ".tmp";

        try
        {
            using (var stream =
                   new FileStream(
                       temporaryFilePath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None))
            {
                Serialize(
                    instance:
                        instance,

                    stream:
                        stream,

                    validateBeforeSerialization:
                        validateBeforeSerialization,

                    validateCurrentFingerprint:
                        validateCurrentFingerprint,

                    indent:
                        indent);

                stream.Flush(
                    flushToDisk: true);
            }

            File.Move(
                temporaryFilePath,
                fullFilePath,
                overwrite: true);
        }
        finally
        {
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(
                    temporaryFilePath);
            }
        }
    }

    /// <summary>
    /// Serializes an instance to a writable stream.
    /// </summary>
    /// <param name="instance">
    /// Instance to serialize.
    /// </param>
    /// <param name="stream">
    /// Writable destination stream.
    /// </param>
    /// <param name="validateBeforeSerialization">
    /// Value indicating whether the instance must be validated
    /// before serialization.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether validation must compare
    /// recorded fingerprints with the current supply-chain
    /// fingerprint.
    /// </param>
    /// <param name="indent">
    /// Value indicating whether the generated XML must be
    /// indented.
    /// </param>
    /// <remarks>
    /// Serialization begins at the current stream position.
    ///
    /// The supplied stream remains open after serialization.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> or
    /// <paramref name="stream"/> is <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not writable.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when validation fails or the instance cannot be
    /// serialized.
    /// </exception>
    public static void Serialize(
        LotSizingInstance instance,
        Stream stream,
        bool validateBeforeSerialization = true,
        bool validateCurrentFingerprint = true,
        bool indent = true)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanWrite)
        {
            throw new ArgumentException(
                "The destination stream must be writable.",
                nameof(stream));
        }

        if (validateBeforeSerialization)
        {
            LotSizingInstanceValidator.EnsureValid(
                instance:
                    instance,

                validateCurrentFingerprint:
                    validateCurrentFingerprint);
        }

        XmlWriterSettings writerSettings =
            CreateWriterSettings(
                indent);

        XmlSerializerNamespaces namespaces =
            CreateEmptyNamespaces();

        XmlSerializer serializer =
            CreateSerializer();

        try
        {
            using XmlWriter writer =
                XmlWriter.Create(
                    stream,
                    writerSettings);

            serializer.Serialize(
                writer,
                instance,
                namespaces);

            writer.Flush();
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException(
                "The lot-sizing instance could not be " +
                "serialized to XML.",
                exception);
        }
    }

    /// <summary>
    /// Serializes an instance to an XML string.
    /// </summary>
    /// <param name="instance">
    /// Instance to serialize.
    /// </param>
    /// <param name="validateBeforeSerialization">
    /// Value indicating whether the instance must be validated
    /// before serialization.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether validation must compare
    /// recorded fingerprints with the current supply-chain
    /// fingerprint.
    /// </param>
    /// <param name="indent">
    /// Value indicating whether the generated XML must be
    /// indented.
    /// </param>
    /// <returns>
    /// UTF-8 XML representation of the instance.
    /// </returns>
    public static string SerializeToString(
        LotSizingInstance instance,
        bool validateBeforeSerialization = true,
        bool validateCurrentFingerprint = true,
        bool indent = true)
    {
        ArgumentNullException.ThrowIfNull(instance);

        using var stream =
            new MemoryStream();

        Serialize(
            instance:
                instance,

            stream:
                stream,

            validateBeforeSerialization:
                validateBeforeSerialization,

            validateCurrentFingerprint:
                validateCurrentFingerprint,

            indent:
                indent);

        return XmlEncoding.GetString(
            stream.ToArray());
    }

    /// <summary>
    /// Deserializes a lot-sizing instance from an XML file.
    /// </summary>
    /// <param name="filePath">
    /// Source XML file path.
    /// </param>
    /// <param name="validateAfterDeserialization">
    /// Value indicating whether the reconstructed instance
    /// must be validated.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether validation must compare
    /// recorded fingerprints with the reconstructed current
    /// supply-chain fingerprint.
    /// </param>
    /// <returns>
    /// Deserialized lot-sizing instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="filePath"/> is empty.
    /// </exception>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the source file does not exist.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the XML cannot be deserialized or when the
    /// reconstructed instance is invalid.
    /// </exception>
    public static LotSizingInstance DeserializeFromFile(
        string filePath,
        bool validateAfterDeserialization = true,
        bool validateCurrentFingerprint = true)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException(
                "A source file path is required.",
                nameof(filePath));
        }

        string fullFilePath =
            Path.GetFullPath(
                filePath.Trim());

        if (!File.Exists(fullFilePath))
        {
            throw new FileNotFoundException(
                "The lot-sizing instance file was not found.",
                fullFilePath);
        }

        using var stream =
            new FileStream(
                fullFilePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        return Deserialize(
            stream:
                stream,

            validateAfterDeserialization:
                validateAfterDeserialization,

            validateCurrentFingerprint:
                validateCurrentFingerprint);
    }

    /// <summary>
    /// Deserializes a lot-sizing instance from a readable
    /// stream.
    /// </summary>
    /// <param name="stream">
    /// Readable stream containing the XML document.
    /// </param>
    /// <param name="validateAfterDeserialization">
    /// Value indicating whether the reconstructed instance
    /// must be validated.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether validation must compare
    /// recorded fingerprints with the reconstructed current
    /// supply-chain fingerprint.
    /// </param>
    /// <returns>
    /// Deserialized lot-sizing instance.
    /// </returns>
    /// <remarks>
    /// Reading begins at the current stream position.
    ///
    /// The supplied stream remains open after deserialization.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="stream"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the stream is not readable.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the XML cannot be deserialized or when the
    /// reconstructed instance is invalid.
    /// </exception>
    public static LotSizingInstance Deserialize(
        Stream stream,
        bool validateAfterDeserialization = true,
        bool validateCurrentFingerprint = true)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException(
                "The source stream must be readable.",
                nameof(stream));
        }

        XmlReaderSettings readerSettings =
            CreateReaderSettings();

        using XmlReader reader =
            XmlReader.Create(
                stream,
                readerSettings);

        return DeserializeFromReader(
            reader:
                reader,

            validateAfterDeserialization:
                validateAfterDeserialization,

            validateCurrentFingerprint:
                validateCurrentFingerprint);
    }

    /// <summary>
    /// Deserializes a lot-sizing instance from an XML string.
    /// </summary>
    /// <param name="xml">
    /// XML representation of the instance.
    /// </param>
    /// <param name="validateAfterDeserialization">
    /// Value indicating whether the reconstructed instance
    /// must be validated.
    /// </param>
    /// <param name="validateCurrentFingerprint">
    /// Value indicating whether validation must compare
    /// recorded fingerprints with the reconstructed current
    /// supply-chain fingerprint.
    /// </param>
    /// <returns>
    /// Deserialized lot-sizing instance.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="xml"/> is empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the XML cannot be deserialized or when the
    /// reconstructed instance is invalid.
    /// </exception>
    public static LotSizingInstance DeserializeFromString(
        string xml,
        bool validateAfterDeserialization = true,
        bool validateCurrentFingerprint = true)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            throw new ArgumentException(
                "An XML document is required.",
                nameof(xml));
        }

        using var stringReader =
            new StringReader(
                xml);

        XmlReaderSettings readerSettings =
            CreateReaderSettings();

        using XmlReader reader =
            XmlReader.Create(
                stringReader,
                readerSettings);

        return DeserializeFromReader(
            reader:
                reader,

            validateAfterDeserialization:
                validateAfterDeserialization,

            validateCurrentFingerprint:
                validateCurrentFingerprint);
    }

    private static LotSizingInstance DeserializeFromReader(
        XmlReader reader,
        bool validateAfterDeserialization,
        bool validateCurrentFingerprint)
    {
        XmlSerializer serializer =
            CreateSerializer();

        object? deserializedObject;

        try
        {
            deserializedObject =
                serializer.Deserialize(
                    reader);
        }
        catch (InvalidOperationException exception)
        {
            throw new InvalidOperationException(
                "The XML document could not be deserialized " +
                "as a lot-sizing instance.",
                exception);
        }

        if (deserializedObject is not
            LotSizingInstance instance)
        {
            throw new InvalidOperationException(
                "The XML document does not contain a valid " +
                "lot-sizing instance root object.");
        }

        if (validateAfterDeserialization)
        {
            LotSizingInstanceValidator.EnsureValid(
                instance:
                    instance,

                validateCurrentFingerprint:
                    validateCurrentFingerprint);
        }

        return instance;
    }

    private static XmlSerializer CreateSerializer()
    {
        return new XmlSerializer(
            typeof(LotSizingInstance));
    }

    private static XmlSerializerNamespaces
        CreateEmptyNamespaces()
    {
        var namespaces =
            new XmlSerializerNamespaces();

        namespaces.Add(
            string.Empty,
            string.Empty);

        return namespaces;
    }

    private static XmlWriterSettings CreateWriterSettings(
        bool indent)
    {
        return new XmlWriterSettings
        {
            Encoding =
                XmlEncoding,

            Indent =
                indent,

            IndentChars =
                "  ",

            OmitXmlDeclaration =
                false,

            NewLineHandling =
                NewLineHandling.None,

            CloseOutput =
                false
        };
    }

    private static XmlReaderSettings CreateReaderSettings()
    {
        return new XmlReaderSettings
        {
            DtdProcessing =
                DtdProcessing.Prohibit,

            XmlResolver =
                null,

            IgnoreComments =
                false,

            IgnoreWhitespace =
                false,

            CloseInput =
                false
        };
    }
}