using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a standalone warehouse that is not attached to a plant.
///
/// Corresponds to the UML class "Autonome", derived from "Entrepot".
/// Its identifier corresponds to e in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "standaloneWarehouse")]
public sealed partial class StandaloneWarehouse : Warehouse
{
    private int _id;

    /// <summary>
    /// Initializes an empty standalone warehouse.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public StandaloneWarehouse()
    {
    }

    /// <summary>
    /// Initializes a standalone warehouse with an identifier and a name.
    /// </summary>
    /// <param name="id">
    /// Numerical identifier corresponding to e in the source model.
    /// </param>
    /// <param name="name">
    /// Warehouse name.
    /// </param>
    public StandaloneWarehouse(int id, string name)
        : base(name)  // Initialize base warehouse with name
    {
        // Initialize standalone warehouse identifier
        Id = id;
    }

    /// <summary>
    /// Gets or sets the numerical identifier of the standalone warehouse.
    ///
    /// Corresponds to e in the source model.
    /// </summary>
    [XmlAttribute("id")]
    public int Id
    {
        get => _id;
        set
        {
            // Validate that the identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The standalone warehouse identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _id, value);
        }
    }
}