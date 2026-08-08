using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a supplier in the supply chain.
///
/// Corresponds to the UML class "Fournisseur".
/// Its identifier corresponds to f in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "supplier")]
public sealed class Supplier : IdentifiedEntity
{
    /// <summary>
    /// Initializes an empty supplier.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public Supplier()
    {
    }

    /// <summary>
    /// Initializes a supplier with an identifier and a name.
    /// </summary>
    /// <param name="id">
    /// Numerical identifier corresponding to f
    /// in the source model.
    /// </param>
    /// <param name="name">
    /// Supplier name.
    /// </param>
    public Supplier(int id, string name)
    {
        // Initialize supplier properties
        Id = id;
        Name = name;
    }
}