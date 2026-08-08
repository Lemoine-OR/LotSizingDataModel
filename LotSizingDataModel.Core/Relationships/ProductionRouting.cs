using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Represents a production routing used to manufacture an item
/// in a plant through one or more work centers.
///
/// Corresponds to the UML class "Gamme".
/// </summary>
[Serializable]
[XmlType(TypeName = "productionRouting")]
public sealed partial class ProductionRouting : ModelObject
{
    private int _id;
    private int _itemId;
    private int _plantId;
    private int _leadTime;
    private List<WorkCenterReference> _workCenters = new();

    /// <summary>
    /// Initializes an empty production routing.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public ProductionRouting()
    {
    }

    /// <summary>
    /// Initializes a production routing.
    /// </summary>
    /// <param name="id">
    /// Routing identifier corresponding to g in the source model.
    /// </param>
    /// <param name="itemId">
    /// Identifier of the manufactured item.
    /// </param>
    /// <param name="plantId">
    /// Identifier of the plant using this routing.
    /// </param>
    /// <param name="leadTime">
    /// Production lead time, expressed in planning periods.
    /// Corresponds to l[g] in the source model.
    /// </param>
    public ProductionRouting(
        int id,
        int itemId,
        int plantId,
        int leadTime)
    {
        // Initialize all production routing properties (validation occurs in setters)
        Id = id;
        ItemId = itemId;
        PlantId = plantId;
        LeadTime = leadTime;
    }

    /// <summary>
    /// Gets or sets the routing identifier.
    ///
    /// Corresponds to g in the source model.
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
                    "The production-routing identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _id, value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the item manufactured
    /// through this routing.
    /// </summary>
    [XmlAttribute("itemId")]
    public int ItemId
    {
        get => _itemId;
        set
        {
            // Validate that the item identifier is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The item identifier cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _itemId, value);
        }
    }

    /// <summary>
    /// Gets or sets the identifier of the plant
    /// in which this routing is available.
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

            // Update and notify dependent property if value changed
            if (SetProperty(ref _plantId, value))
            {
                OnPropertyChanged(nameof(HasConsistentWorkCenterReferences));
            }
        }
    }

    /// <summary>
    /// Gets or sets the production lead time.
    ///
    /// Corresponds to l[g] in the source model.
    /// The value is expressed as a number of planning periods.
    /// </summary>
    [XmlAttribute("leadTime")]
    public int LeadTime
    {
        get => _leadTime;
        set
        {
            // Validate that the lead time is non-negative
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The production lead time cannot be negative.");
            }

            // Update the backing field and notify property change if value differs
            SetProperty(ref _leadTime, value);
        }
    }

    /// <summary>
    /// Gets or sets the work centers used by this routing.
    ///
    /// The UML cardinality is 1..*.
    /// The non-empty cardinality is checked by the global validator
    /// after object construction or XML deserialization.
    /// </summary>
    [XmlArray("workCenters")]
    [XmlArrayItem("workCenterReference")]
    public List<WorkCenterReference> WorkCenters
    {
        get => _workCenters;
        set
        {
            // Ensure the list is never null
            List<WorkCenterReference> newValue =
                value ?? new List<WorkCenterReference>();

            // Avoid unnecessary notifications if the reference is the same
            if (ReferenceEquals(_workCenters, newValue))
            {
                return;
            }

            _workCenters = newValue;

            // Notify both properties that depend on work centers
            OnPropertyChanged(nameof(WorkCenters));
            OnPropertyChanged(nameof(HasConsistentWorkCenterReferences));
        }
    }

    /// <summary>
    /// Gets a value indicating whether all referenced work centers
    /// belong to the plant associated with this routing.
    ///
    /// This property is calculated and is therefore not serialized.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentWorkCenterReferences =>
        _workCenters.All(
            reference =>
                reference.PlantId == PlantId);

    /// <summary>
    /// Adds a work center to the routing.
    /// </summary>
    /// <param name="workCenter">
    /// Reference to the work center to add.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the referenced work center belongs to another plant
    /// or is already present in the routing.
    /// </exception>
    public void AddWorkCenter(
        WorkCenterReference workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        // Validate that the work center belongs to the same plant
        if (PlantId > 0 &&
            workCenter.PlantId > 0 &&
            workCenter.PlantId != PlantId)
        {
            throw new InvalidOperationException(
                $"Work center {workCenter.WorkCenterId} belongs to plant " +
                $"{workCenter.PlantId}, whereas routing {Id} belongs " +
                $"to plant {PlantId}.");
        }

        // Check if the work center is already in the routing
        bool alreadyExists = _workCenters.Any(
            current =>
                current.RefersToSameWorkCenter(workCenter));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                $"Work center {workCenter.WorkCenterId} of plant " +
                $"{workCenter.PlantId} is already assigned " +
                $"to routing {Id}.");
        }

        // Add the work center and notify property changes
        _workCenters.Add(workCenter);

        OnPropertyChanged(nameof(WorkCenters));
        OnPropertyChanged(nameof(HasConsistentWorkCenterReferences));
    }

    /// <summary>
    /// Adds a work center by its identifier.
    ///
    /// The work center is assumed to belong to the plant
    /// associated with this routing.
    /// </summary>
    /// <param name="workCenterId">
    /// Identifier of the work center inside the plant.
    /// </param>
    public void AddWorkCenter(int workCenterId)
    {
        // Create a reference for the work center in this routing's plant
        AddWorkCenter(
            new WorkCenterReference(
                PlantId,
                workCenterId));
    }

    /// <summary>
    /// Removes a work-center reference from the routing.
    /// </summary>
    public bool RemoveWorkCenter(
        WorkCenterReference workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        // Find the matching work center reference in the collection
        WorkCenterReference? existingReference =
            _workCenters.FirstOrDefault(
                current =>
                    current.RefersToSameWorkCenter(workCenter));

        if (existingReference is null)
        {
            return false;
        }

        // Remove the reference and notify property changes if successful
        bool removed = _workCenters.Remove(existingReference);

        if (removed)
        {
            OnPropertyChanged(nameof(WorkCenters));
            OnPropertyChanged(
                nameof(HasConsistentWorkCenterReferences));
        }

        return removed;
    }

    /// <summary>
    /// Removes a work center by its identifier.
    /// </summary>
    public bool RemoveWorkCenter(int workCenterId)
    {
        // Find the work center reference matching the plant and work center ID
        WorkCenterReference? existingReference =
            _workCenters.FirstOrDefault(
                reference =>
                    reference.PlantId == PlantId &&
                    reference.WorkCenterId == workCenterId);

        if (existingReference is null)
        {
            return false;
        }

        // Delegate to the main remove method
        return RemoveWorkCenter(existingReference);
    }

    /// <summary>
    /// Determines whether this routing uses a given work center.
    /// </summary>
    public bool UsesWorkCenter(int workCenterId)
    {
        // Check if any work center reference matches the plant and work center ID
        return _workCenters.Any(
            reference =>
                reference.PlantId == PlantId &&
                reference.WorkCenterId == workCenterId);
    }
}