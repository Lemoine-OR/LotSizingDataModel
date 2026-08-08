using System.Xml;
using System.Xml.Serialization;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.Checker.Campaign;

/// <summary>
/// Reads serialized <see cref="LotSizingInstance"/> files using a hardened XML
/// reader configuration suitable for local validation campaigns.
/// </summary>
public sealed class LotSizingInstanceXmlFileReader
{
    private const string ExpectedRootElement =
        "lotSizingInstance";

    private readonly XmlSerializer _serializer =
        new(typeof(LotSizingInstance));

    /// <summary>
    /// Determines whether the file root element represents a serialized
    /// lot-sizing instance without deserializing the whole file.
    /// </summary>
    /// <param name="filePath">Absolute or relative XML file path.</param>
    /// <returns>
    /// <see langword="true"/> when the document root local name is
    /// <c>lotSizingInstance</c>.
    /// </returns>
    public bool HasLotSizingInstanceRoot(
        string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using XmlReader reader =
            XmlReader.Create(
                filePath,
                CreateReaderSettings());

        reader.MoveToContent();

        return
            reader.NodeType == XmlNodeType.Element &&
            string.Equals(
                reader.LocalName,
                ExpectedRootElement,
                StringComparison.Ordinal);
    }

    /// <summary>
    /// Deserializes one complete lot-sizing instance file.
    /// </summary>
    /// <param name="filePath">XML instance file path.</param>
    /// <returns>The deserialized instance.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the XML document cannot be deserialized into a non-null
    /// <see cref="LotSizingInstance"/>.
    /// </exception>
    public LotSizingInstance Read(
        string filePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        using XmlReader reader =
            XmlReader.Create(
                filePath,
                CreateReaderSettings());

        object? deserialized =
            _serializer.Deserialize(reader);

        return deserialized as LotSizingInstance ??
            throw new InvalidOperationException(
                "The XML document did not deserialize to a " +
                "LotSizingInstance object.");
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
                true
        };
    }
}
