using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Instance.ClosedLoop;

namespace LotSizingDataModel.Instance;

/// <summary>
/// Adds explicit closed-loop return streams to a lot-sizing
/// instance without changing the historical SupplyChain schema.
/// </summary>
public sealed partial class LotSizingInstance
{
    [XmlArray("closedLoopReturnStreams")]
    [XmlArrayItem("closedLoopReturnStream")]
    public List<ClosedLoopReturnStream>
        ClosedLoopReturnStreams
    {
        get;
    } =
        new();

    [XmlIgnore]
    public bool HasClosedLoopReturnStreams =>
        ClosedLoopReturnStreams.Count > 0;

    public void AddClosedLoopReturnStream(
        ClosedLoopReturnStream stream)
    {
        ArgumentNullException.ThrowIfNull(
            stream);

        if (ClosedLoopReturnStreams.Any(
                existing =>
                    existing.Id ==
                    stream.Id))
        {
            throw new InvalidOperationException(
                $"Closed-loop return-stream identifier '{stream.Id}' is already used.");
        }

        stream.ResizeTimeSeries(
            PlanningHorizon);

        stream.EnsureValid();

        ClosedLoopReturnStreams.Add(
            stream);

        OnPropertyChanged(
            nameof(ClosedLoopReturnStreams));

        OnPropertyChanged(
            nameof(HasClosedLoopReturnStreams));
    }

    public void SynchronizeClosedLoopPlanningHorizon()
    {
        foreach (ClosedLoopReturnStream stream
                 in ClosedLoopReturnStreams)
        {
            stream.ResizeTimeSeries(
                PlanningHorizon);
        }

        OnPropertyChanged(
            nameof(ClosedLoopReturnStreams));
    }

    public void EnsureClosedLoopValid()
    {
        var streamIdentifiers =
            new HashSet<int>();

        foreach (ClosedLoopReturnStream stream
                 in ClosedLoopReturnStreams)
        {
            ArgumentNullException.ThrowIfNull(
                stream);

            stream.EnsureValid();

            if (!streamIdentifiers.Add(
                    stream.Id))
            {
                throw new InvalidOperationException(
                    $"Closed-loop return-stream identifier '{stream.Id}' is duplicated.");
            }

            if (stream.PlanningHorizon !=
                PlanningHorizon)
            {
                throw new InvalidOperationException(
                    $"Closed-loop return stream '{stream.Id}' uses horizon {stream.PlanningHorizon}, expected {PlanningHorizon}.");
            }

            if (!SupplyChain.Items.Any(
                    item =>
                        item.Id ==
                        stream.ItemId))
            {
                throw new InvalidOperationException(
                    $"Closed-loop return stream '{stream.Id}' references unknown item '{stream.ItemId}'.");
            }

            if (!SupplyChain.DistributionCenters.Any(
                    distributionCenter =>
                        distributionCenter.Id ==
                        stream.DistributionCenterId))
            {
                throw new InvalidOperationException(
                    $"Closed-loop return stream '{stream.Id}' references unknown distribution center '{stream.DistributionCenterId}'.");
            }

            bool hasTargetInventory =
                SupplyChain.Inventories.Any(
                    inventory =>
                        inventory.ItemId ==
                            stream.ItemId &&
                        inventory.Warehouse.Kind ==
                            stream.RecoveryWarehouse.Kind &&
                        inventory.Warehouse.ReferenceId ==
                            stream.RecoveryWarehouse.ReferenceId);

            if (!hasTargetInventory)
            {
                throw new InvalidOperationException(
                    $"Closed-loop return stream '{stream.Id}' has no matching target inventory for recovered item '{stream.ItemId}'.");
            }
        }
    }
}
