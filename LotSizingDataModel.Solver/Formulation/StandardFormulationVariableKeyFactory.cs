using System;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Creates canonical variable domain keys used by the standard
/// formulation constraint builders.
/// </summary>
internal static class StandardFormulationVariableKeyFactory
{
    /// <summary>
    /// Creates an inventory-like key for an item, warehouse,
    /// period, and decision category.
    /// </summary>
    public static string CreateInventoryKey(
        string category,
        int itemId,
        WarehouseReference warehouse,
        int period)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentNullException.ThrowIfNull(warehouse);

        var builder =
            new MathematicalDomainKeyBuilder(category)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    itemId);

        StandardFormulationDomainKeyFactory.AddWarehouse(
            builder,
            warehouse);

        return builder
            .Add(
                MathematicalDomainKeySegment.Period,
                period)
            .Build();
    }

    /// <summary>
    /// Creates a distribution decision key.
    /// </summary>
    public static string CreateDistributionKey(
        string category,
        int distributionCenterId,
        int itemId,
        WarehouseReference warehouse,
        int period)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(category);
        ArgumentNullException.ThrowIfNull(warehouse);

        var builder =
            new MathematicalDomainKeyBuilder(category)
                .Add(
                    MathematicalDomainKeySegment.DistributionCenter,
                    distributionCenterId)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    itemId);

        StandardFormulationDomainKeyFactory.AddWarehouse(
            builder,
            warehouse);

        return builder
            .Add(
                MathematicalDomainKeySegment.Period,
                period)
            .Build();
    }

    /// <summary>
    /// Creates a production decision key.
    /// </summary>
    public static string CreateProductionKey(
        int routingId,
        int period)
    {
        return new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.Production)
            .Add(
                MathematicalDomainKeySegment.Routing,
                routingId)
            .Add(
                MathematicalDomainKeySegment.Period,
                period)
            .Build();
    }

    /// <summary>
    /// Creates a procurement decision key.
    /// </summary>
    public static string CreateProcurementKey(
        int supplierId,
        int itemId,
        WarehouseReference destination,
        int period)
    {
        ArgumentNullException.ThrowIfNull(destination);

        var builder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.Procurement)
                .Add(
                    MathematicalDomainKeySegment.Supplier,
                    supplierId)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    itemId);

        StandardFormulationDomainKeyFactory.AddDestinationWarehouse(
            builder,
            destination);

        return builder
            .Add(
                MathematicalDomainKeySegment.Period,
                period)
            .Build();
    }

    /// <summary>
    /// Creates a transport quantity decision key.
    /// </summary>
    public static string CreateTransportKey(
        int itemId,
        int transportResourceId,
        WarehouseReference origin,
        WarehouseReference destination,
        int period)
    {
        ArgumentNullException.ThrowIfNull(origin);
        ArgumentNullException.ThrowIfNull(destination);

        var builder =
            new MathematicalDomainKeyBuilder(
                MathematicalDecisionCategory.Transport)
                .Add(
                    MathematicalDomainKeySegment.Item,
                    itemId)
                .Add(
                    MathematicalDomainKeySegment.TransportResource,
                    transportResourceId);

        StandardFormulationDomainKeyFactory.AddOriginWarehouse(
            builder,
            origin);

        StandardFormulationDomainKeyFactory.AddDestinationWarehouse(
            builder,
            destination);

        return builder
            .Add(
                MathematicalDomainKeySegment.Period,
                period)
            .Build();
    }
}
