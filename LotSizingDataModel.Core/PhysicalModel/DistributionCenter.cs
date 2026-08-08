using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a distribution center in the supply chain.
///
/// Corresponds to the UML class "Centre de distribution".
/// Its identifier corresponds to c in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "distributionCenter")]
public sealed class DistributionCenter : IdentifiedEntity
{
    /// <summary>
    /// Initializes an empty distribution center.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public DistributionCenter()
    {
    }

    /// <summary>
    /// Initializes a distribution center with an identifier and a name.
    /// </summary>
    /// <param name="id">
    /// Numerical identifier corresponding to c
    /// in the source model.
    /// </param>
    /// <param name="name">
    /// Distribution-center name.
    /// </param>
    public DistributionCenter(int id, string name)
    {
        // Initialize distribution center properties
        Id = id;
        Name = name;
    }
}