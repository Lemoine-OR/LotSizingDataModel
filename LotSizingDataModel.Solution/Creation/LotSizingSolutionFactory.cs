using System;
using System.Linq;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Validation;
using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solution.Decisions;

namespace LotSizingDataModel.Solution.Creation;

/// <summary>
/// Creates lot-sizing solutions whose decision structure
/// matches a supply-chain instance.
/// </summary>
/// <remarks>
/// The created solution does not retain a reference to the
/// supplied <see cref="SupplyChain"/> object.
///
/// Identifiers and warehouse references are copied into
/// independent solution objects.
/// </remarks>
public static class LotSizingSolutionFactory
{
    /// <summary>
    /// Creates a zero-initialized solution whose decision
    /// structure matches the specified supply-chain instance.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain instance used to create the decision
    /// structure.
    /// </param>
    /// <param name="instanceIdentifier">
    /// Identifier of the associated supply-chain instance.
    /// </param>
    /// <param name="name">
    /// Optional human-readable solution name.
    /// </param>
    /// <param name="instanceFingerprint">
    /// Optional fingerprint of the associated instance.
    /// </param>
    /// <param name="validateSupplyChain">
    /// True to validate the supply-chain instance before
    /// creating the solution; otherwise, false.
    /// </param>
    /// <returns>
    /// A structurally complete, zero-initialized solution.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplyChain"/> is null.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the instance planning horizon is not
    /// strictly positive.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the instance is invalid or contains an
    /// unresolved transport-resource reference.
    /// </exception>
    public static LotSizingSolution Create(
        SupplyChain supplyChain,
        string instanceIdentifier = "",
        string name = "",
        string instanceFingerprint = "",
        bool validateSupplyChain = true)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        if (supplyChain.PlanningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(supplyChain),
                supplyChain.PlanningHorizon,
                "The supply-chain planning horizon must be " +
                "strictly positive.");
        }

        if (validateSupplyChain)
        {
            var validator =
                new SupplyChainValidator();

            validator.ThrowIfInvalid(
                supplyChain);
        }

        var solution =
            new LotSizingSolution(
                name: name ?? string.Empty,
                instanceIdentifier:
                    instanceIdentifier ?? string.Empty,
                planningHorizon:
                    supplyChain.PlanningHorizon)
            {
                InstanceFingerprint =
                    instanceFingerprint ?? string.Empty,

                Completeness =
                    SolutionCompleteness.Unknown
            };

        CreateProductionDecisions(
            supplyChain,
            solution);

        CreateInventoryDecisions(
            supplyChain,
            solution);

        CreateTransportDecisions(
            supplyChain,
            solution);

        CreatePurchaseDecisions(
            supplyChain,
            solution);

        CreateDistributionDecisions(
            supplyChain,
            solution);

        CreateWorkCenterCapacityDecisions(
            supplyChain,
            solution);

        CreateWarehouseCapacityDecisions(
            supplyChain,
            solution);

        CreateTransportResourceCapacityDecisions(
            supplyChain,
            solution);

        /*
         * Every expected decision object has now been created.
         * This means that the solution is structurally complete.
         *
         * It does not mean that the all-zero solution is feasible.
         */
        solution.Completeness =
            SolutionCompleteness.Complete;

        return solution;
    }

    private static void CreateProductionDecisions(
        SupplyChain supplyChain,
        LotSizingSolution solution)
    {
        foreach (var routing
                 in supplyChain.ProductionRoutings)
        {
            var decision =
                new ProductionDecision(
                    routingId: routing.Id,
                    planningHorizon:
                        supplyChain.PlanningHorizon);

            solution.AddProductionDecision(
                decision);
        }
    }

    private static void CreateInventoryDecisions(
        SupplyChain supplyChain,
        LotSizingSolution solution)
    {
        foreach (var inventory
                 in supplyChain.Inventories)
        {
            var decision =
                new InventoryDecision(
                    itemId: inventory.ItemId,
                    warehouse:
                        CopyWarehouseReference(
                            inventory.Warehouse),
                    planningHorizon:
                        supplyChain.PlanningHorizon);

            solution.AddInventoryDecision(
                decision);
        }
    }

    private static void CreateTransportDecisions(
        SupplyChain supplyChain,
        LotSizingSolution solution)
    {
        /*
         * A transport characteristic identifies an allowed
         * item-resource combination.
         *
         * The lanes of the transport resource identify
         * the corresponding directed origin-destination pairs.
         */
        foreach (var characteristic
                 in supplyChain.TransportCharacteristics)
        {
            var transportResource =
                supplyChain.TransportResources
                    .FirstOrDefault(
                        resource =>
                            resource.Id ==
                            characteristic
                                .TransportResourceId);

            if (transportResource is null)
            {
                throw new InvalidOperationException(
                    $"Transport resource " +
                    $"{characteristic.TransportResourceId} " +
                    $"referenced by item " +
                    $"{characteristic.ItemId} does not exist.");
            }

            foreach (var lane
                     in transportResource.Lanes)
            {
                var decision =
                    new TransportDecision(
                        itemId:
                            characteristic.ItemId,

                        transportResourceId:
                            characteristic
                                .TransportResourceId,

                        origin:
                            CopyWarehouseReference(
                                lane.Origin),

                        destination:
                            CopyWarehouseReference(
                                lane.Destination),

                        planningHorizon:
                            supplyChain.PlanningHorizon);

                solution.AddTransportDecision(
                    decision);
            }
        }
    }

    private static void CreatePurchaseDecisions(
        SupplyChain supplyChain,
        LotSizingSolution solution)
    {
        foreach (var delivery
                 in supplyChain.SupplierDeliveries)
        {
            var decision =
                new PurchaseDecision(
                    supplierId:
                        delivery.SupplierId,

                    itemId:
                        delivery.ItemId,

                    destinationWarehouse:
                        CopyWarehouseReference(
                            delivery.Warehouse),

                    planningHorizon:
                        supplyChain.PlanningHorizon);

            solution.AddPurchaseDecision(
                decision);
        }
    }

    private static void CreateDistributionDecisions(
        SupplyChain supplyChain,
        LotSizingSolution solution)
    {
        foreach (var sourcing
                 in supplyChain
                     .DistributionCenterSourcings)
        {
            var decision =
                new DistributionDecision(
                    distributionCenterId:
                        sourcing.DistributionCenterId,

                    itemId:
                        sourcing.ItemId,

                    warehouse:
                        CopyWarehouseReference(
                            sourcing.Warehouse),

                    planningHorizon:
                        supplyChain.PlanningHorizon);

            solution.AddDistributionDecision(
                decision);
        }
    }

    private static void
        CreateWorkCenterCapacityDecisions(
            SupplyChain supplyChain,
            LotSizingSolution solution)
    {
        foreach (var plant
                 in supplyChain.Plants)
        {
            foreach (var workCenter
                     in plant.WorkCenters)
            {
                var reference =
                    new WorkCenterReference
                    {
                        PlantId =
                            plant.Id,

                        WorkCenterId =
                            workCenter.Id
                    };

                var decision =
                    new WorkCenterCapacityDecision(
                        workCenter: reference,
                        planningHorizon:
                            supplyChain.PlanningHorizon);

                solution.AddWorkCenterCapacityDecision(
                    decision);
            }
        }
    }

    private static void
        CreateWarehouseCapacityDecisions(
            SupplyChain supplyChain,
            LotSizingSolution solution)
    {
        /*
         * Every plant owns one internal warehouse whose
         * reference identifier is the plant identifier.
         */
        foreach (var plant
                 in supplyChain.Plants)
        {
            WarehouseReference reference =
                WarehouseReference
                    .ForPlantWarehouse(
                        plant.Id);

            var decision =
                new WarehouseCapacityDecision(
                    warehouse: reference,
                    planningHorizon:
                        supplyChain.PlanningHorizon);

            solution.AddWarehouseCapacityDecision(
                decision);
        }

        foreach (var warehouse
                 in supplyChain.StandaloneWarehouses)
        {
            WarehouseReference reference =
                WarehouseReference
                    .ForStandaloneWarehouse(
                        warehouse.Id);

            var decision =
                new WarehouseCapacityDecision(
                    warehouse: reference,
                    planningHorizon:
                        supplyChain.PlanningHorizon);

            solution.AddWarehouseCapacityDecision(
                decision);
        }
    }

    private static void
        CreateTransportResourceCapacityDecisions(
            SupplyChain supplyChain,
            LotSizingSolution solution)
    {
        foreach (var transportResource
                 in supplyChain.TransportResources)
        {
            var decision =
                new TransportResourceCapacityDecision(
                    transportResourceId:
                        transportResource.Id,

                    planningHorizon:
                        supplyChain.PlanningHorizon);

            solution
                .AddTransportResourceCapacityDecision(
                    decision);
        }
    }

    private static WarehouseReference
        CopyWarehouseReference(
            WarehouseReference source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return source.Kind switch
        {
            WarehouseReferenceKind
                .StandaloneWarehouse =>
                WarehouseReference
                    .ForStandaloneWarehouse(
                        source.ReferenceId),

            WarehouseReferenceKind
                .PlantWarehouse =>
                WarehouseReference
                    .ForPlantWarehouse(
                        source.ReferenceId),

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported warehouse-reference kind: " +
                    $"{source.Kind}.")
        };
    }
}