using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Concrete dimensions of an LSI-classified instance (sigma).
/// </summary>
[Serializable]
[XmlType(TypeName = "lsiInstanceSizeSignature")]
public sealed class InstanceSizeSignature : ModelObject
{
    private int _periods;
    private int _items;
    private int _plants;
    private int _workCenters;
    private int _warehouses;
    private int _suppliers;
    private int _distributionCenters;
    private int _transportResources;
    private int _bomRelationships;
    private int _maximumBomDepth;

    [XmlAttribute("periods")]
    public int Periods
    {
        get => _periods;
        set => SetCount(ref _periods, value, nameof(Periods));
    }

    [XmlAttribute("items")]
    public int Items
    {
        get => _items;
        set => SetCount(ref _items, value, nameof(Items));
    }

    [XmlAttribute("plants")]
    public int Plants
    {
        get => _plants;
        set => SetCount(ref _plants, value, nameof(Plants));
    }

    [XmlAttribute("workCenters")]
    public int WorkCenters
    {
        get => _workCenters;
        set => SetCount(
            ref _workCenters, value, nameof(WorkCenters));
    }

    [XmlAttribute("warehouses")]
    public int Warehouses
    {
        get => _warehouses;
        set => SetCount(
            ref _warehouses, value, nameof(Warehouses));
    }

    [XmlAttribute("suppliers")]
    public int Suppliers
    {
        get => _suppliers;
        set => SetCount(
            ref _suppliers, value, nameof(Suppliers));
    }

    [XmlAttribute("distributionCenters")]
    public int DistributionCenters
    {
        get => _distributionCenters;
        set => SetCount(
            ref _distributionCenters,
            value,
            nameof(DistributionCenters));
    }

    [XmlAttribute("transportResources")]
    public int TransportResources
    {
        get => _transportResources;
        set => SetCount(
            ref _transportResources,
            value,
            nameof(TransportResources));
    }

    [XmlAttribute("bomRelationships")]
    public int BomRelationships
    {
        get => _bomRelationships;
        set => SetCount(
            ref _bomRelationships,
            value,
            nameof(BomRelationships));
    }

    [XmlAttribute("maximumBomDepth")]
    public int MaximumBomDepth
    {
        get => _maximumBomDepth;
        set => SetCount(
            ref _maximumBomDepth,
            value,
            nameof(MaximumBomDepth));
    }

    private void SetCount(
        ref int field,
        int value,
        string propertyName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(
                propertyName,
                value,
                "LSI size values cannot be negative.");
        }

        SetProperty(ref field, value, propertyName);
    }
}
