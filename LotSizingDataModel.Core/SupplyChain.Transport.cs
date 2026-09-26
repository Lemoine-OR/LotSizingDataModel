using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Indexing;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core;

public sealed partial class SupplyChain
{
    [XmlAttribute("transportFormatVersion")]
    public int TransportFormatVersion { get; set; } = 2;

    [XmlArray("transportLanes"), XmlArrayItem("transportLane")]
    public List<TransportLane> TransportLanes { get; } = new();

    [XmlArray("transportAssignments"), XmlArrayItem("transportAssignment")]
    public List<TransportAssignment> TransportAssignments { get; } = new();

    public bool ShouldSerializeTransportFormatVersion() => TransportResources.Count > 0 || TransportLanes.Count > 0 || TransportAssignments.Count > 0;
    public bool ShouldSerializeTransportLanes() => TransportLanes.Count > 0;
    public bool ShouldSerializeTransportAssignments() => TransportAssignments.Count > 0;

    public TransportLane? FindTransportLane(WarehouseReference origin, WarehouseReference destination) =>
        TransportLanes.SingleOrDefault(l => SameTransportWarehouse(l.Origin, origin) && SameTransportWarehouse(l.Destination, destination));

    public TransportAssignment? FindTransportAssignment(int laneId, int resourceId) =>
        TransportAssignments.SingleOrDefault(a => a.LaneId == laneId && a.TransportResourceId == resourceId);

    public IEnumerable<AssignedTransportLane> GetTransportLanes(int resourceId) =>
        TransportAssignments.Where(a => a.TransportResourceId == resourceId)
            .Select(a => new AssignedTransportLane(TransportLanes.Single(l => l.Id == a.LaneId), a));

    public void AddTransportLane(TransportLane lane)
    {
        ArgumentNullException.ThrowIfNull(lane);
        if (lane.Id < 0) throw new ArgumentOutOfRangeException(nameof(lane));
        var index = new SupplyChainIndex(this);
        index.GetRequiredWarehouse(lane.Origin);
        index.GetRequiredWarehouse(lane.Destination);
        if (SameTransportWarehouse(lane.Origin, lane.Destination))
            throw new InvalidOperationException("A transport lane must connect different warehouses.");
        if (TransportLanes.Any(l => l.Id == lane.Id) || FindTransportLane(lane.Origin, lane.Destination) is not null)
            throw new InvalidOperationException("Duplicate transport lane identifier or directed warehouse pair.");
        TransportLanes.Add(lane);
        OnPropertyChanged(nameof(TransportLanes));
    }

    public void AddTransportAssignment(TransportAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(assignment);
        if (!TransportLanes.Any(l => l.Id == assignment.LaneId) ||
            !TransportResources.Any(r => r.Id == assignment.TransportResourceId))
            throw new InvalidOperationException("The assignment references a missing lane or resource.");
        if (FindTransportAssignment(assignment.LaneId, assignment.TransportResourceId) is not null)
            throw new InvalidOperationException("Duplicate lane-resource assignment.");
        TransportAssignments.Add(assignment);
        OnPropertyChanged(nameof(TransportAssignments));
    }

    public bool RemoveTransportAssignment(int laneId, int resourceId)
    {
        var assignment = FindTransportAssignment(laneId, resourceId);
        if (assignment is null) return false;
        TransportAssignments.Remove(assignment);
        OnPropertyChanged(nameof(TransportAssignments));
        return true;
    }

    public bool RemoveTransportLane(int laneId)
    {
        if (TransportAssignments.Any(a => a.LaneId == laneId))
            throw new InvalidOperationException("Remove referencing assignments explicitly before removing the lane.");
        var lane = TransportLanes.SingleOrDefault(l => l.Id == laneId);
        if (lane is null) return false;
        TransportLanes.Remove(lane);
        OnPropertyChanged(nameof(TransportLanes));
        return true;
    }

    public bool RemoveTransportResource(int resourceId)
    {
        if (TransportAssignments.Any(a => a.TransportResourceId == resourceId) ||
            TransportCharacteristics.Any(c => c.TransportResourceId == resourceId))
            throw new InvalidOperationException("Remove referencing assignments and characteristics explicitly before removing the resource.");
        var resource = TransportResources.SingleOrDefault(r => r.Id == resourceId);
        if (resource is null) return false;
        TransportResources.Remove(resource);
        OnPropertyChanged(nameof(TransportResources));
        return true;
    }

    private static bool SameTransportWarehouse(WarehouseReference first, WarehouseReference second) =>
        first.Kind == second.Kind && first.ReferenceId == second.ReferenceId;
}
