using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Validation;

namespace LotSizingDataModel.Core.Serialization;

/// <summary>
/// Serializes and deserializes complete supply-chain models
/// using the XML representation defined by the domain classes.
/// </summary>
public sealed class SupplyChainXmlSerializer
{
    // UTF-8 without byte-order mark for clean XML files.
    private static readonly Encoding XmlEncoding =
        new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false);

    private static readonly XmlSerializer Serializer =
        new(typeof(SupplyChain));

    private readonly SupplyChainValidator _validator;

    /// <summary>
    /// Initializes the serializer with a default
    /// supply-chain validator.
    /// </summary>
    public SupplyChainXmlSerializer()
        : this(new SupplyChainValidator())
    {
    }

    /// <summary>
    /// Initializes the serializer with the specified validator.
    /// </summary>
    /// <param name="validator">
    /// Validator used before serialization and after deserialization.
    /// </param>
    public SupplyChainXmlSerializer(
        SupplyChainValidator validator)
    {
        _validator = validator ??
            throw new ArgumentNullException(nameof(validator));
    }

    /// <summary>
    /// Saves a supply chain to an XML file.
    ///
    /// The model is validated before serialization by default.
    /// The file is first written to a temporary location and then
    /// moved to the destination to reduce the risk of leaving a
    /// partially written XML file.
    /// </summary>
    /// <param name="filePath">
    /// Destination XML file path.
    /// </param>
    /// <param name="supplyChain">
    /// Supply chain to serialize.
    /// </param>
    /// <param name="validateBeforeSave">
    /// Indicates whether the model must be validated before writing.
    /// </param>
    public void Save(
        string filePath,
        SupplyChain supplyChain,
        bool validateBeforeSave = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);
        ArgumentNullException.ThrowIfNull(supplyChain);

        string fullPath =
            Path.GetFullPath(filePath);

        string? directoryPath =
            Path.GetDirectoryName(fullPath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new InvalidOperationException(
                "The destination directory cannot be determined.");
        }

        Directory.CreateDirectory(directoryPath);

        // Generate a unique temporary file name in the destination directory.
        string temporaryFilePath =
            Path.Combine(
                directoryPath,
                "." +
                Path.GetFileName(fullPath) +
                "." +
                Guid.NewGuid().ToString("N") +
                ".tmp");

        try
        {
            using (var stream = new FileStream(
                       temporaryFilePath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None))
            {
                Serialize(
                    stream,
                    supplyChain,
                    validateBeforeSave);
            }

            // Replace the destination file atomically.
            File.Move(
                temporaryFilePath,
                fullPath,
                overwrite: true);
        }
        finally
        {
            /*
             * The temporary file remains only if serialization
             * or replacement of the destination file failed.
             */
            if (File.Exists(temporaryFilePath))
            {
                File.Delete(temporaryFilePath);
            }
        }
    }

    /// <summary>
    /// Loads a supply chain from an XML file.
    /// </summary>
    /// <param name="filePath">
    /// Source XML file path.
    /// </param>
    /// <param name="validateAfterLoad">
    /// Indicates whether the deserialized model must be validated.
    /// </param>
    /// <param name="synchronizePlanningHorizon">
    /// Indicates whether all time series must be resized to the
    /// global planning horizon before validation.
    ///
    /// Leave this option false to detect inconsistent series
    /// in the source XML instead of silently correcting them.
    /// </param>
    /// <returns>
    /// Deserialized supply-chain model.
    /// </returns>
    public SupplyChain Load(
        string filePath,
        bool validateAfterLoad = true,
        bool synchronizePlanningHorizon = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        string fullPath =
            Path.GetFullPath(filePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                "The supply-chain XML file does not exist.",
                fullPath);
        }

        using var stream = new FileStream(
            fullPath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read);

        return Deserialize(
            stream,
            validateAfterLoad,
            synchronizePlanningHorizon);
    }

    /// <summary>
    /// Serializes a supply chain to an output stream.
    ///
    /// The stream remains open after this method returns.
    /// </summary>
    /// <param name="output">
    /// Writable output stream.
    /// </param>
    /// <param name="supplyChain">
    /// Supply chain to serialize.
    /// </param>
    /// <param name="validateBeforeSerialization">
    /// Indicates whether the model must be validated first.
    /// </param>
    public void Serialize(
        Stream output,
        SupplyChain supplyChain,
        bool validateBeforeSerialization = true)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(supplyChain);

        if (!output.CanWrite)
        {
            throw new ArgumentException(
                "The output stream must be writable.",
                nameof(output));
        }

        if (validateBeforeSerialization)
        {
            _validator.ThrowIfInvalid(supplyChain);
        }

        // Prepare namespace settings to omit default xsi/xsd declarations.
        var namespaces =
            new XmlSerializerNamespaces();

        /*
         * Prevents the unnecessary xsi and xsd namespace
         * declarations from being written to the root element.
         */
        namespaces.Add(
            string.Empty,
            string.Empty);

        var settings = new XmlWriterSettings
        {
            Encoding = XmlEncoding,
            Indent = true,
            IndentChars = "  ",
            NewLineChars = Environment.NewLine,
            NewLineHandling = NewLineHandling.Replace,
            OmitXmlDeclaration = false,
            CloseOutput = false  // Keep the stream open after writing.
        };

        using XmlWriter writer =
            XmlWriter.Create(
                output,
                settings);

        Serializer.Serialize(
            writer,
            supplyChain,
            namespaces);
    }

    /// <summary>
    /// Deserializes a supply chain from an input stream.
    ///
    /// The stream remains open after this method returns.
    /// </summary>
    /// <param name="input">
    /// Readable input stream.
    /// </param>
    /// <param name="validateAfterDeserialization">
    /// Indicates whether the model must be validated after loading.
    /// </param>
    /// <param name="synchronizePlanningHorizon">
    /// Indicates whether all time series must be resized to the
    /// global planning horizon before validation.
    /// </param>
    /// <returns>
    /// Deserialized supply-chain model.
    /// </returns>
    public SupplyChain Deserialize(
        Stream input,
        bool validateAfterDeserialization = true,
        bool synchronizePlanningHorizon = false)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (!input.CanRead)
        {
            throw new ArgumentException(
                "The input stream must be readable.",
                nameof(input));
        }

        var settings = new XmlReaderSettings
        {
            /*
             * DTD processing and external entity resolution are
             * disabled to prevent XML external-entity attacks.
             */
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null,
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            CloseInput = false  // Keep the stream open after reading.
        };

        using XmlReader reader =
            XmlReader.Create(
                input,
                settings);

        object? result =
            Serializer.Deserialize(reader);

        if (result is not SupplyChain supplyChain)
        {
            throw new InvalidOperationException(
                "The XML document does not contain a valid " +
                "supply-chain root object.");
        }

        /*
         * In strict mode, synchronization is not performed.
         * This allows the validator to report malformed or
         * inconsistent planning horizons from the XML file.
         */
        if (synchronizePlanningHorizon)
        {
            supplyChain.SynchronizePlanningHorizon();
        }

        if (validateAfterDeserialization)
        {
            _validator.ThrowIfInvalid(supplyChain);
        }

        return supplyChain;
    }

    /// <summary>
    /// Serializes a supply chain to an XML string.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply chain to serialize.
    /// </param>
    /// <param name="validateBeforeSerialization">
    /// Indicates whether the model must be validated first.
    /// </param>
    /// <returns>
    /// UTF-8 XML representation of the supply chain.
    /// </returns>
    public string SerializeToString(
        SupplyChain supplyChain,
        bool validateBeforeSerialization = true)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        using var stream = new MemoryStream();

        Serialize(
            stream,
            supplyChain,
            validateBeforeSerialization);

        return XmlEncoding.GetString(
            stream.ToArray());
    }

    /// <summary>
    /// Deserializes a supply chain from an XML string.
    /// </summary>
    /// <param name="xml">
    /// XML representation of the supply chain.
    /// </param>
    /// <param name="validateAfterDeserialization">
    /// Indicates whether the resulting model must be validated.
    /// </param>
    /// <param name="synchronizePlanningHorizon">
    /// Indicates whether all time series must be resized to the
    /// global planning horizon before validation.
    /// </param>
    /// <returns>
    /// Deserialized supply-chain model.
    /// </returns>
    public SupplyChain DeserializeFromString(
        string xml,
        bool validateAfterDeserialization = true,
        bool synchronizePlanningHorizon = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(xml);

        byte[] xmlBytes =
            XmlEncoding.GetBytes(xml);

        using var stream =
            new MemoryStream(
                xmlBytes,
                writable: false);

        return Deserialize(
            stream,
            validateAfterDeserialization,
            synchronizePlanningHorizon);
    }
}