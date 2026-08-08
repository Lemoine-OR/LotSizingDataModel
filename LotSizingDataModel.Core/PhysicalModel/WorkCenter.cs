using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a production work center belonging to a plant.
///
/// Corresponds to the UML class "Poste de Charge".
/// </summary>
[Serializable]
[XmlType(TypeName = "workCenter")]
public sealed partial class WorkCenter : IdentifiedEntity
{
    /// <summary>
    /// Initializes an empty work center.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public WorkCenter()
    {
    }

    /// <summary>
    /// Initializes a work center with an identifier and a name.
    /// </summary>
    /// <param name="id">
    /// Numerical identifier corresponding to p in the source model.
    /// </param>
    /// <param name="name">
    /// Work-center name.
    /// </param>
    public WorkCenter(int id, string name)
    {
        // Initialize work center properties
        Id = id;
        Name = name;
    }
}