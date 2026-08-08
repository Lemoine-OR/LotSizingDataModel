using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.Common;

/// <summary>
/// Base class for model entities that have both
/// a numerical identifier and a name.
/// </summary>
[Serializable]
[XmlType(TypeName = "identifiedEntity")]
public abstract class IdentifiedEntity : NamedEntity
{
    private int _id;

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="IdentifiedEntity"/> class.
    /// </summary>
    protected IdentifiedEntity()
    {
    }

    /// <summary>
    /// Gets or sets the numerical identifier of the entity.
    /// </summary>
    [XmlAttribute("id")]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);  // Notifies property change if value differs
    }
}