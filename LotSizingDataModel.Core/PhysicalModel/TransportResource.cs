using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>A transport resource with its own shared capacities and costs.</summary>
[Serializable]
[XmlType(TypeName = "transportResource")]
public sealed partial class TransportResource : IdentifiedEntity
{
    public TransportResource() { }
    public TransportResource(int id, string name) { Id = id; Name = name; }

    /// <summary>Rejects raw legacy XML; use the versioned model serializers to migrate it.</summary>
    [XmlElement("lanes")]
    public System.Xml.XmlElement? LegacyLanesGuard
    {
        get => null;
        set => throw new InvalidOperationException("Legacy nested lanes require SupplyChainXmlSerializer or LotSizingInstanceXmlSerializer migration.");
    }
}
