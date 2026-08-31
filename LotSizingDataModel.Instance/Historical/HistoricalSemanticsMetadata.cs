using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Historical;

/// <summary>
/// Source-declared historical classification metadata.
/// </summary>
/// <remarks>
/// This metadata is never used as evidence for detected generic semantics.
/// It exists to preserve source labels and enable declared-vs-detected audits.
/// </remarks>
[Serializable]
[XmlType(TypeName = "historicalSemanticsMetadata")]
public sealed class HistoricalSemanticsMetadata : ModelObject
{
    private string _originalWolseyCode = string.Empty;
    private WolseyDeclaredMachineLabel _declaredWolseyMachineLabel;

    [XmlElement("originalWolseyCode")]
    public string OriginalWolseyCode
    {
        get => _originalWolseyCode;
        set => SetProperty(
            ref _originalWolseyCode,
            value?.Trim() ?? string.Empty);
    }

    [XmlAttribute("declaredWolseyMachineLabel")]
    public WolseyDeclaredMachineLabel DeclaredWolseyMachineLabel
    {
        get => _declaredWolseyMachineLabel;
        set => SetProperty(
            ref _declaredWolseyMachineLabel,
            value);
    }
}

/// <summary>
/// Wolsey machine labels preserved from the source without semantic inference.
/// </summary>
public enum WolseyDeclaredMachineLabel
{
    Unspecified = 0,
    IM = 1,
    VM = 2
}
