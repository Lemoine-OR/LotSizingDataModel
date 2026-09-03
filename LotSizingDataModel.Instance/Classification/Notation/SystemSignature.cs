using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// LSI physical and productive system block (alpha).
/// </summary>
[Serializable]
[XmlType(TypeName = "lsiSystemSignature")]
public sealed class SystemSignature : ModelObject
{
    private CardinalityKind _items = CardinalityKind.Unknown;
    private CardinalityKind _levels = CardinalityKind.Unknown;
    private ProductStructureType _productStructure =
        ProductStructureType.Unknown;
    private NetworkStructureKind _network =
        NetworkStructureKind.Unknown;
    private RoutingStructureKind _routing =
        RoutingStructureKind.Unknown;
    private ResourceEnvironmentKind _resourceEnvironment =
        ResourceEnvironmentKind.Unknown;

    [XmlAttribute("items")]
    public CardinalityKind Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    [XmlAttribute("levels")]
    public CardinalityKind Levels
    {
        get => _levels;
        set => SetProperty(ref _levels, value);
    }

    [XmlAttribute("productStructure")]
    public ProductStructureType ProductStructure
    {
        get => _productStructure;
        set => SetProperty(ref _productStructure, value);
    }

    [XmlAttribute("network")]
    public NetworkStructureKind Network
    {
        get => _network;
        set => SetProperty(ref _network, value);
    }

    [XmlAttribute("routing")]
    public RoutingStructureKind Routing
    {
        get => _routing;
        set => SetProperty(ref _routing, value);
    }

    [XmlAttribute("resourceEnvironment")]
    public ResourceEnvironmentKind ResourceEnvironment
    {
        get => _resourceEnvironment;
        set => SetProperty(ref _resourceEnvironment, value);
    }
}
