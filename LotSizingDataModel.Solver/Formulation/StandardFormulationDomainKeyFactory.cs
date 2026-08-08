using System;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solver.Building;
using LotSizingDataModel.Solver.Mapping;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Creates canonical domain-key fragments shared by standard
/// formulation builders and provides warehouse-reference helpers.
/// </summary>
internal static class StandardFormulationDomainKeyFactory
{
    /// <summary>
    /// Determines whether two warehouse references identify the
    /// same physical warehouse.
    /// </summary>
    /// <param name="left">First warehouse reference.</param>
    /// <param name="right">Second warehouse reference.</param>
    /// <returns>
    /// <see langword="true"/> when kind and reference identifier
    /// are equal; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool AreSameWarehouse(
        WarehouseReference left,
        WarehouseReference right)
    {
        ArgumentNullException.ThrowIfNull(left);
        ArgumentNullException.ThrowIfNull(right);

        return
            left.Kind == right.Kind &&
            left.ReferenceId == right.ReferenceId;
    }

    /// <summary>
    /// Adds a warehouse reference using the generic warehouse
    /// or plant segment.
    /// </summary>
    public static MathematicalDomainKeyBuilder AddWarehouse(
        MathematicalDomainKeyBuilder builder,
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(warehouse);

        return warehouse.Kind switch
        {
            WarehouseReferenceKind.StandaloneWarehouse =>
                builder.Add(
                    MathematicalDomainKeySegment.Warehouse,
                    warehouse.ReferenceId),

            WarehouseReferenceKind.PlantWarehouse =>
                builder.Add(
                    MathematicalDomainKeySegment.Plant,
                    warehouse.ReferenceId),

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported warehouse-reference kind " +
                    $"'{warehouse.Kind}'.")
        };
    }

    /// <summary>
    /// Adds an origin warehouse reference.
    /// </summary>
    public static MathematicalDomainKeyBuilder AddOriginWarehouse(
        MathematicalDomainKeyBuilder builder,
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(warehouse);

        return warehouse.Kind switch
        {
            WarehouseReferenceKind.StandaloneWarehouse =>
                builder.Add(
                    MathematicalDomainKeySegment.OriginWarehouse,
                    warehouse.ReferenceId),

            WarehouseReferenceKind.PlantWarehouse =>
                builder.Add(
                    MathematicalDomainKeySegment.OriginPlant,
                    warehouse.ReferenceId),

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported origin warehouse-reference kind " +
                    $"'{warehouse.Kind}'.")
        };
    }

    /// <summary>
    /// Adds a destination warehouse reference.
    /// </summary>
    public static MathematicalDomainKeyBuilder AddDestinationWarehouse(
        MathematicalDomainKeyBuilder builder,
        WarehouseReference warehouse)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(warehouse);

        return warehouse.Kind switch
        {
            WarehouseReferenceKind.StandaloneWarehouse =>
                builder.Add(
                    MathematicalDomainKeySegment.DestinationWarehouse,
                    warehouse.ReferenceId),

            WarehouseReferenceKind.PlantWarehouse =>
                builder.Add(
                    MathematicalDomainKeySegment.DestinationPlant,
                    warehouse.ReferenceId),

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported destination warehouse-reference " +
                    $"kind '{warehouse.Kind}'.")
        };
    }
}
