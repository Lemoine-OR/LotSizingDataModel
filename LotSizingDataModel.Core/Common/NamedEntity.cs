using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.Common;

/// <summary>
/// Base class for model entities that have a name.
///
/// This class does not define an identifier because some entities in the
/// source UML model, such as the generic Warehouse class, only define a name.
/// </summary>
[Serializable]
[XmlType(TypeName = "namedEntity")]
public abstract class NamedEntity : ModelObject
{
    private string _name = string.Empty;

    /// <summary>
    /// Initializes a new instance of the <see cref="NamedEntity"/> class.
    ///
    /// Derived serializable classes must expose their own public
    /// parameterless constructor.
    /// </summary>
    protected NamedEntity()
    {
    }

    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get => _name;
        set => SetProperty(
            ref _name,
            value ?? string.Empty);  // Ensure the name is never null
    }

    /// <summary>
    /// Returns the entity name.
    /// </summary>
    public override string ToString()
    {
        return Name;
    }
}