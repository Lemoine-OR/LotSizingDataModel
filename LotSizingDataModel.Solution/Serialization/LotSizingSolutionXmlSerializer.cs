using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using LotSizingDataModel.Core;
using LotSizingDataModel.Solution.Validation;

namespace LotSizingDataModel.Solution.Serialization;

/// <summary>
/// Serializes and deserializes lot-sizing solutions
/// using the XML format.
/// </summary>
/// <remarks>
/// XML loading prohibits DTD processing and external
/// resource resolution.
///
/// After deserialization, nested property-change
/// notifications are reconnected automatically.
/// </remarks>
public sealed class LotSizingSolutionXmlSerializer
{
    private readonly XmlSerializer _serializer;
    private readonly LotSizingSolutionValidator _validator;

    /// <summary>
    /// Initializes a solution XML serializer using
    /// an indented XML representation.
    /// </summary>
    public LotSizingSolutionXmlSerializer()
        : this(
            new LotSizingSolutionValidator(),
            indentOutput: true)
    {
    }

    /// <summary>
    /// Initializes a solution XML serializer.
    /// </summary>
    /// <param name="validator">
    /// Validator used when validation is requested.
    /// </param>
    /// <param name="indentOutput">
    /// True to indent generated XML; otherwise, false.
    /// </param>
    public LotSizingSolutionXmlSerializer(
        LotSizingSolutionValidator validator,
        bool indentOutput = true)
    {
        ArgumentNullException.ThrowIfNull(validator);

        _validator = validator;
        _serializer =
            new XmlSerializer(
                typeof(LotSizingSolution));

        IndentOutput = indentOutput;
    }

    /// <summary>
    /// Gets a value indicating whether generated XML
    /// is indented.
    /// </summary>
    public bool IndentOutput { get; }

    /// <summary>
    /// Saves a solution to an XML file.
    /// </summary>
    /// <param name="filePath">
    /// Destination XML file path.
    /// </param>
    /// <param name="solution">
    /// Solution to serialize.
    /// </param>
    /// <param name="validateBeforeSave">
    /// True to validate the internal structure before
    /// serialization; otherwise, false.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the file path is empty.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when validation is requested and the solution
    /// contains at least one validation error.
    /// </exception>
    public void Save(
        string filePath,
        LotSizingSolution solution,
        bool validateBeforeSave = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            filePath);

        ArgumentNullException.ThrowIfNull(solution);

        string fullPath =
            Path.GetFullPath(filePath);

        string? directoryPath =
            Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(
                directoryPath))
        {
            Directory.CreateDirectory(
                directoryPath);
        }

        using var stream =
            new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

        Save(
            stream,
            solution,
            validateBeforeSave,
            leaveOpen: false);
    }

    /// <summary>
    /// Validates a solution against a supply-chain instance
    /// and saves it to an XML file.
    /// </summary>
    /// <param name="filePath">
    /// Destination XML file path.
    /// </param>
    /// <param name="solution">
    /// Solution to serialize.
    /// </param>
    /// <param name="supplyChain">
    /// Supply-chain instance against which the solution
    /// must be validated.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the solution is not compatible with
    /// the supply-chain instance.
    /// </exception>
    public void Save(
        string filePath,
        LotSizingSolution solution,
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(solution);
        ArgumentNullException.ThrowIfNull(supplyChain);

        _validator.ThrowIfInvalid(
            solution,
            supplyChain);

        Save(
            filePath,
            solution,
            validateBeforeSave: false);
    }

    /// <summary>
    /// Saves a solution to a writable stream.
    /// </summary>
    /// <param name="stream">
    /// Destination stream.
    /// </param>
    /// <param name="solution">
    /// Solution to serialize.
    /// </param>
    /// <param name="validateBeforeSave">
    /// True to validate the internal structure before
    /// serialization; otherwise, false.
    /// </param>
    /// <param name="leaveOpen">
    /// True to leave the stream open after serialization;
    /// otherwise, false.
    /// </param>
    public void Save(
        Stream stream,
        LotSizingSolution solution,
        bool validateBeforeSave = false,
        bool leaveOpen = true)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentNullException.ThrowIfNull(solution);

        if (!stream.CanWrite)
        {
            throw new ArgumentException(
                "The destination stream is not writable.",
                nameof(stream));
        }

        if (validateBeforeSave)
        {
            _validator.ThrowIfInvalid(solution);
        }

        XmlWriterSettings settings =
            CreateWriterSettings(leaveOpen);

        using XmlWriter writer =
            XmlWriter.Create(
                stream,
                settings);

        _serializer.Serialize(
            writer,
            solution);

        writer.Flush();
    }

    /// <summary>
    /// Loads a solution from an XML file.
    /// </summary>
    /// <param name="filePath">
    /// Source XML file path.
    /// </param>
    /// <param name="validateAfterLoad">
    /// True to validate the internal structure after
    /// deserialization; otherwise, false.
    /// </param>
    /// <returns>
    /// Deserialized lot-sizing solution.
    /// </returns>
    /// <exception cref="FileNotFoundException">
    /// Thrown when the source file does not exist.
    /// </exception>
    /// <exception cref="InvalidDataException">
    /// Thrown when the XML does not contain a valid
    /// lot-sizing solution document.
    /// </exception>
    public LotSizingSolution Load(
        string filePath,
        bool validateAfterLoad = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            filePath);

        string fullPath =
            Path.GetFullPath(filePath);

        using var stream =
            new FileStream(
                fullPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        return Load(
            stream,
            validateAfterLoad,
            leaveOpen: false);
    }

    /// <summary>
    /// Loads a solution from an XML file and validates it
    /// against a supply-chain instance.
    /// </summary>
    /// <param name="filePath">
    /// Source XML file path.
    /// </param>
    /// <param name="supplyChain">
    /// Supply-chain instance against which the loaded
    /// solution must be validated.
    /// </param>
    /// <returns>
    /// Deserialized and validated lot-sizing solution.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the solution is not compatible with
    /// the supply-chain instance.
    /// </exception>
    public LotSizingSolution Load(
        string filePath,
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        LotSizingSolution solution =
            Load(
                filePath,
                validateAfterLoad: false);

        _validator.ThrowIfInvalid(
            solution,
            supplyChain);

        return solution;
    }

    /// <summary>
    /// Loads a solution from a readable stream.
    /// </summary>
    /// <param name="stream">
    /// Source stream.
    /// </param>
    /// <param name="validateAfterLoad">
    /// True to validate the internal structure after
    /// deserialization; otherwise, false.
    /// </param>
    /// <param name="leaveOpen">
    /// True to leave the stream open after deserialization;
    /// otherwise, false.
    /// </param>
    /// <returns>
    /// Deserialized lot-sizing solution.
    /// </returns>
    /// <exception cref="InvalidDataException">
    /// Thrown when the stream does not contain a valid
    /// lot-sizing solution document.
    /// </exception>
    public LotSizingSolution Load(
        Stream stream,
        bool validateAfterLoad = false,
        bool leaveOpen = true)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (!stream.CanRead)
        {
            throw new ArgumentException(
                "The source stream is not readable.",
                nameof(stream));
        }

        XmlReaderSettings settings =
            CreateReaderSettings(leaveOpen);

        try
        {
            using XmlReader reader =
                XmlReader.Create(
                    stream,
                    settings);

            LotSizingSolution solution =
                Deserialize(reader);

            if (validateAfterLoad)
            {
                _validator.ThrowIfInvalid(solution);
            }

            return solution;
        }
        catch (XmlException exception)
        {
            throw new InvalidDataException(
                "The XML document is malformed.",
                exception);
        }
        catch (InvalidOperationException exception)
            when (exception.InnerException is XmlException)
        {
            throw new InvalidDataException(
                "The XML document does not contain a valid " +
                "lot-sizing solution.",
                exception);
        }
    }

    /// <summary>
    /// Serializes a solution to an XML string.
    /// </summary>
    /// <param name="solution">
    /// Solution to serialize.
    /// </param>
    /// <param name="validateBeforeSerialization">
    /// True to validate the solution before serialization;
    /// otherwise, false.
    /// </param>
    /// <returns>
    /// UTF-8 XML representation of the solution.
    /// </returns>
    public string SerializeToString(
        LotSizingSolution solution,
        bool validateBeforeSerialization = false)
    {
        ArgumentNullException.ThrowIfNull(solution);

        using var stream =
            new MemoryStream();

        Save(
            stream,
            solution,
            validateBeforeSerialization,
            leaveOpen: true);

        return Encoding.UTF8.GetString(
            stream.ToArray());
    }

    /// <summary>
    /// Deserializes a solution from an XML string.
    /// </summary>
    /// <param name="xml">
    /// XML representation of the solution.
    /// </param>
    /// <param name="validateAfterDeserialization">
    /// True to validate the internal structure after
    /// deserialization; otherwise, false.
    /// </param>
    /// <returns>
    /// Deserialized lot-sizing solution.
    /// </returns>
    public LotSizingSolution DeserializeFromString(
        string xml,
        bool validateAfterDeserialization = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        byte[] data =
            Encoding.UTF8.GetBytes(xml);

        using var stream =
            new MemoryStream(
                data,
                writable: false);

        return Load(
            stream,
            validateAfterDeserialization,
            leaveOpen: false);
    }

    private LotSizingSolution Deserialize(
        XmlReader reader)
    {
        object? deserializedObject =
            _serializer.Deserialize(reader);

        if (deserializedObject is not
            LotSizingSolution solution)
        {
            throw new InvalidDataException(
                "The XML document does not contain a " +
                "lot-sizing solution.");
        }

        solution.ReconnectNestedNotifications();

        return solution;
    }

    private XmlWriterSettings CreateWriterSettings(
        bool leaveOpen)
    {
        return new XmlWriterSettings
        {
            Encoding =
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: false),

            Indent = IndentOutput,
            OmitXmlDeclaration = false,
            CloseOutput = !leaveOpen,
            NewLineHandling =
                NewLineHandling.Entitize
        };
    }

    private static XmlReaderSettings CreateReaderSettings(
        bool leaveOpen)
    {
        return new XmlReaderSettings
        {
            DtdProcessing =
                DtdProcessing.Prohibit,

            XmlResolver = null,
            CloseInput = !leaveOpen,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true
        };
    }
}