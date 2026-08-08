using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a serializable reference to a work center.
///
/// A work center is identified by:
/// - the identifier of its owning plant;
/// - its identifier inside that plant.
///
/// This class avoids duplicating complete Plant and WorkCenter objects
/// inside association classes.
/// </summary>
[Serializable]
[XmlType(TypeName = "workCenterReference")]
public sealed class WorkCenterReference : ModelObject
{
    private int _plantId;
    private int _workCenterId;

    /// <summary>
    /// Initializes an empty work-center reference.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public WorkCenterReference()
    {
    }

    /// <summary>
    /// Initializes a work-center reference.
    /// </summary>
    /// <param name="plantId">
    /// Identifier of the plant owning the work center.
    /// </param>
    /// <param name="workCenterId">
    /// Identifier of the work center inside the plant.
    /// </param>
    public WorkCenterReference(
        int plantId,
        int workCenterId)
    {
        // Initialize work center reference properties (validation occurs in setters)
        PlantId = plantId;
        WorkCenterId = workCenterId;
    }

    /// <summary>
    /// Gets or sets the identifier of the plant
    /// owning the referenced work center.
    /// </summary>
    [XmlAttribute("plantId")]
    public int PlantId
    {
        get => _plantId;
        set
        {
            // Validate that the plant identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The plant identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _plantId, value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the referenced work center.
    /// </summary>
    [XmlAttribute("workCenterId")]
    public int WorkCenterId
    {
        get => _workCenterId;
        set
        {
            // Validate that the work center identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The work-center identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _workCenterId, value);
        }
    }

    /// <summary>
    /// Determines whether this reference identifies the same
    /// work center as another reference.
    /// </summary>
    public bool RefersToSameWorkCenter(
        WorkCenterReference? other)
    {
        // Compare both plant ID and work center ID for equality
        return other is not null
               && PlantId == other.PlantId
               && WorkCenterId == other.WorkCenterId;
    }

    /// <summary>
    /// Returns a readable representation of the reference.
    /// </summary>
    public override string ToString()
    {
        return $"Plant:{PlantId}/WorkCenter:{WorkCenterId}";
    }
}