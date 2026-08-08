using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a production plant.
///
/// A plant owns exactly one plant warehouse and one or more work centers.
///
/// Corresponds to the UML class "Usine".
/// </summary>
[Serializable]
[XmlType(TypeName = "plant")]
public sealed partial class Plant : IdentifiedEntity
{
    private PlantWarehouse _warehouse = new();
    private List<WorkCenter> _workCenters = new();

    /// <summary>
    /// Initializes an empty plant.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public Plant()
    {
    }

    /// <summary>
    /// Initializes a plant with an identifier, a name
    /// and its associated warehouse.
    /// </summary>
    /// <param name="id">
    /// Numerical identifier corresponding to u in the source model.
    /// </param>
    /// <param name="name">
    /// Plant name.
    /// </param>
    /// <param name="warehouse">
    /// Warehouse attached to the plant.
    /// </param>
    public Plant(
        int id,
        string name,
        PlantWarehouse warehouse)
    {
        // Initialize all plant properties
        Id = id;
        Name = name;
        Warehouse = warehouse;  // Validation occurs in the setter
    }

    /// <summary>
    /// Gets or sets the warehouse attached to the plant.
    ///
    /// The source UML model defines exactly one non-standalone
    /// warehouse for each plant.
    /// </summary>
    [XmlElement("warehouse")]
    public PlantWarehouse Warehouse
    {
        get => _warehouse;
        set
        {
            // Validate that the warehouse is not null
            ArgumentNullException.ThrowIfNull(value);

            // Update the backing field and notify property change if value differs
            SetProperty(
                ref _warehouse,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the work centers belonging to the plant.
    ///
    /// The source UML cardinality is 1..*.
    /// This cardinality will be checked by the global validator.
    /// </summary>
    [XmlArray("workCenters")]
    [XmlArrayItem("workCenter")]
    public List<WorkCenter> WorkCenters
    {
        get => _workCenters;
        set
        {
            // Ensure the list is never null
            List<WorkCenter> newValue =
                value ?? new List<WorkCenter>();

            // Avoid unnecessary notifications if the reference is the same
            if (ReferenceEquals(_workCenters, newValue))
            {
                return;
            }

            _workCenters = newValue;

            OnPropertyChanged(nameof(WorkCenters));
        }
    }

    /// <summary>
    /// Adds a work center to the plant.
    /// </summary>
    /// <param name="workCenter">
    /// Work center to add.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when another work center already uses the same
    /// strictly positive identifier in this plant.
    /// </exception>
    public void AddWorkCenter(WorkCenter workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        // Check if a work center with the same strictly positive ID already exists
        bool identifierAlreadyUsed =
            workCenter.Id > 0 &&
            _workCenters.Any(
                current => current.Id == workCenter.Id);

        if (identifierAlreadyUsed)
        {
            throw new InvalidOperationException(
                $"A work center with identifier {workCenter.Id} " +
                $"already exists in plant {Id}.");
        }

        // Add the work center and notify property change
        _workCenters.Add(workCenter);

        OnPropertyChanged(nameof(WorkCenters));
    }

    /// <summary>
    /// Removes a work center from the plant.
    /// </summary>
    /// <param name="workCenter">
    /// Work center to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the work center was found
    /// and removed; otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveWorkCenter(WorkCenter workCenter)
    {
        ArgumentNullException.ThrowIfNull(workCenter);

        // Attempt to remove the work center from the collection
        bool removed = _workCenters.Remove(workCenter);

        // Notify property change only if removal was successful
        if (removed)
        {
            OnPropertyChanged(nameof(WorkCenters));
        }

        return removed;
    }

    /// <summary>
    /// Finds a work center by its identifier.
    /// </summary>
    /// <param name="workCenterId">
    /// Identifier of the requested work center.
    /// </param>
    /// <returns>
    /// The matching work center, or <see langword="null"/>
    /// when no match exists.
    /// </returns>
    public WorkCenter? FindWorkCenter(int workCenterId)
    {
        return _workCenters.FirstOrDefault(
            workCenter => workCenter.Id == workCenterId);
    }
}