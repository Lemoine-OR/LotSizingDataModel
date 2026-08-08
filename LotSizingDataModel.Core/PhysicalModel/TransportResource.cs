using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>
/// Represents a transport resource that can operate
/// one or more transport lanes.
///
/// Corresponds to the UML class "Moyen de transport".
/// </summary>
[Serializable]
[XmlType(TypeName = "transportResource")]
public sealed partial class TransportResource : IdentifiedEntity
{
    private List<TransportLane> _lanes = new();

    /// <summary>
    /// Initializes an empty transport resource.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public TransportResource()
    {
    }

    /// <summary>
    /// Initializes a transport resource with an identifier
    /// and a name.
    /// </summary>
    /// <param name="id">
    /// Numerical identifier of the transport resource.
    /// </param>
    /// <param name="name">
    /// Transport-resource name.
    /// </param>
    public TransportResource(int id, string name)
    {
        // Initialize transport resource properties
        Id = id;
        Name = name;
    }

    /// <summary>
    /// Gets or sets the transport lanes that can be operated
    /// by this transport resource.
    ///
    /// Each lane defines an origin warehouse,
    /// a destination warehouse and a transport lead time.
    /// </summary>
    [XmlArray("lanes")]
    [XmlArrayItem("transportLane")]
    public List<TransportLane> Lanes
    {
        get => _lanes;
        set
        {
            // Ensure the list is never null
            List<TransportLane> newValue =
                value ?? new List<TransportLane>();

            // Avoid unnecessary notifications if the reference is the same
            if (ReferenceEquals(_lanes, newValue))
            {
                return;
            }

            _lanes = newValue;

            OnPropertyChanged(nameof(Lanes));
        }
    }

    /// <summary>
    /// Adds a transport lane to the transport resource.
    /// </summary>
    /// <param name="lane">
    /// Transport lane to add.
    /// </param>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a lane with the same origin and destination
    /// is already assigned to this transport resource.
    /// </exception>
    public void AddLane(TransportLane lane)
    {
        ArgumentNullException.ThrowIfNull(lane);

        // Check if a lane with the same origin and destination already exists
        bool laneAlreadyExists = _lanes.Any(
            currentLane =>
                HaveSameReference(
                    currentLane.Origin,
                    lane.Origin)
                &&
                HaveSameReference(
                    currentLane.Destination,
                    lane.Destination));

        if (laneAlreadyExists)
        {
            throw new InvalidOperationException(
                "A transport lane with the same origin and " +
                "destination already exists for this transport resource.");
        }

        // Add the lane and notify property change
        _lanes.Add(lane);

        OnPropertyChanged(nameof(Lanes));
    }

    /// <summary>
    /// Removes a transport lane from the transport resource.
    /// </summary>
    /// <param name="lane">
    /// Transport lane to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the lane was found and removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveLane(TransportLane lane)
    {
        ArgumentNullException.ThrowIfNull(lane);

        // Attempt to remove the lane from the collection
        bool removed = _lanes.Remove(lane);

        // Notify property change only if removal was successful
        if (removed)
        {
            OnPropertyChanged(nameof(Lanes));
        }

        return removed;
    }

    /// <summary>
    /// Finds a transport lane from an origin warehouse
    /// to a destination warehouse.
    /// </summary>
    /// <param name="origin">
    /// Origin-warehouse reference.
    /// </param>
    /// <param name="destination">
    /// Destination-warehouse reference.
    /// </param>
    /// <returns>
    /// The matching lane, or <see langword="null"/>
    /// when no matching lane exists.
    /// </returns>
    public TransportLane? FindLane(
        WarehouseReference origin,
        WarehouseReference destination)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);

        // Search for a lane matching both origin and destination
        return _lanes.FirstOrDefault(
            lane =>
                HaveSameReference(lane.Origin, origin)
                &&
                HaveSameReference(
                    lane.Destination,
                    destination));
    }

    /// <summary>
    /// Determines whether two warehouse references point to the same warehouse.
    /// </summary>
    /// <param name="first">First warehouse reference.</param>
    /// <param name="second">Second warehouse reference.</param>
    /// <returns>
    /// <see langword="true"/> if both references have the same kind and ID;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    private static bool HaveSameReference(
        WarehouseReference first,
        WarehouseReference second)
    {
        // Compare both the kind and the reference ID
        return first.Kind == second.Kind
               && first.ReferenceId == second.ReferenceId;
    }
}