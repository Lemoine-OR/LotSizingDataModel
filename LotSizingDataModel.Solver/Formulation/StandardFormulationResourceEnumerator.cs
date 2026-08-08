using System;
using System.Collections.Generic;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Enumerates physical resources together with the canonical
/// references required by the mathematical formulation.
/// </summary>
internal static class StandardFormulationResourceEnumerator
{
    /// <summary>
    /// Enumerates every standalone and plant warehouse.
    /// </summary>
    /// <param name="supplyChain">
    /// Source supply chain.
    /// </param>
    /// <returns>
    /// Warehouse/reference pairs.
    /// </returns>
    public static IEnumerable<(
        WarehouseReference Reference,
        Warehouse Warehouse)> EnumerateWarehouses(
            SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(
            supplyChain);

        foreach (
            StandaloneWarehouse warehouse
            in supplyChain.StandaloneWarehouses)
        {
            yield return (
                WarehouseReference.ForStandaloneWarehouse(
                    warehouse.Id),
                warehouse);
        }

        foreach (
            Plant plant
            in supplyChain.Plants)
        {
            yield return (
                WarehouseReference.ForPlantWarehouse(
                    plant.Id),
                plant.Warehouse);
        }
    }

    /// <summary>
    /// Enumerates every work center with its containing plant
    /// identifier.
    /// </summary>
    /// <param name="supplyChain">
    /// Source supply chain.
    /// </param>
    /// <returns>
    /// Plant/work-center pairs.
    /// </returns>
    public static IEnumerable<(
        int PlantId,
        WorkCenter WorkCenter)> EnumerateWorkCenters(
            SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(
            supplyChain);

        foreach (
            Plant plant
            in supplyChain.Plants)
        {
            foreach (
                WorkCenter workCenter
                in plant.WorkCenters)
            {
                yield return (
                    plant.Id,
                    workCenter);
            }
        }
    }
}
