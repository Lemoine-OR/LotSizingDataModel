using System.Xml.Serialization;
using LotSizingDataModel.Instance.Historical;

namespace LotSizingDataModel.Instance;

/// <summary>
/// Adds optional source-declared historical metadata to a lot-sizing instance.
/// </summary>
public sealed partial class LotSizingInstance
{
    private HistoricalSemanticsMetadata? _historicalSemantics;

    /// <summary>
    /// Gets or sets historical source metadata.
    /// </summary>
    /// <remarks>
    /// Declared values are preserved independently of automatically detected semantics.
    /// </remarks>
    [XmlElement("historicalSemantics")]
    public HistoricalSemanticsMetadata? HistoricalSemantics
    {
        get => _historicalSemantics;
        set => SetProperty(ref _historicalSemantics, value);
    }
}
