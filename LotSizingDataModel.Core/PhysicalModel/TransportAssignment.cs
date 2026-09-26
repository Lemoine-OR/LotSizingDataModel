using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.PhysicalModel;

/// <summary>An authorized lane-resource pair, with its delay in periods.</summary>
[Serializable]
[XmlType(TypeName = "transportAssignment")]
public sealed class TransportAssignment : ModelObject
{
    private int _laneId;
    private int _transportResourceId;
    private int _leadTime;
    public TransportAssignment() { }
    public TransportAssignment(int laneId, int transportResourceId, int leadTime)
    { LaneId = laneId; TransportResourceId = transportResourceId; LeadTime = leadTime; }
    [XmlAttribute("laneId")]
    public int LaneId { get => _laneId; set => SetProperty(ref _laneId, value); }
    [XmlAttribute("transportResourceId")]
    public int TransportResourceId { get => _transportResourceId; set => SetProperty(ref _transportResourceId, value); }
    [XmlAttribute("leadTime")]
    public int LeadTime
    {
        get => _leadTime;
        set { ArgumentOutOfRangeException.ThrowIfNegative(value); SetProperty(ref _leadTime, value); }
    }
}

/// <summary>A runtime join of the central lane and assignment; stores no copied business data.</summary>
public sealed class AssignedTransportLane
{
    public AssignedTransportLane(TransportLane lane, TransportAssignment assignment)
    {
        ArgumentNullException.ThrowIfNull(lane);
        ArgumentNullException.ThrowIfNull(assignment);
        if (lane.Id != assignment.LaneId) throw new ArgumentException("Lane and assignment do not match.");
        Lane = lane; Assignment = assignment;
    }
    public TransportLane Lane { get; }
    public TransportAssignment Assignment { get; }
    public int Id => Lane.Id;
    public WarehouseReference Origin => Lane.Origin;
    public WarehouseReference Destination => Lane.Destination;
    public int LeadTime => Assignment.LeadTime;
}
