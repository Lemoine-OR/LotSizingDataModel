using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Identifies modeled extensions outside the six currently executable
/// canonical lot-sizing core signatures.
/// </summary>
public sealed class LotSizingProblemClassExtensionAnalyzer
{
    public IReadOnlyList<LotSizingProblemClassExtensionKind> Analyze(
        LotSizingProblemDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        var extensions =
            new List<LotSizingProblemClassExtensionKind>();

        Add(
            descriptor.InventoryService.HasInitialInventory,
            LotSizingProblemClassExtensionKind.InitialInventory,
            extensions);

        Add(
            descriptor.InventoryService.HasSafetyStockRequirements,
            LotSizingProblemClassExtensionKind.SafetyStock,
            extensions);

        Add(
            descriptor.InventoryService.HasBacklogging,
            LotSizingProblemClassExtensionKind.Backlogging,
            extensions);

        Add(
            descriptor.InventoryService.HasLostSales,
            LotSizingProblemClassExtensionKind.LostSales,
            extensions);

        Add(
            descriptor.Setup.HasSetupTimes,
            LotSizingProblemClassExtensionKind.SetupTimes,
            extensions);

        Add(
            descriptor.Setup.HasStartUpCosts,
            LotSizingProblemClassExtensionKind.StartUpCosts,
            extensions);

        Add(
            descriptor.Production.HasLeadTimes,
            LotSizingProblemClassExtensionKind.ProductionLeadTimes,
            extensions);

        Add(
            descriptor.Production.HasMinimumLotSizes,
            LotSizingProblemClassExtensionKind.MinimumLotSize,
            extensions);

        Add(
            descriptor.Production.HasMaximumLotSizes,
            LotSizingProblemClassExtensionKind.MaximumLotSize,
            extensions);

        Add(
            descriptor.Production.HasLotSizeMultiples,
            LotSizingProblemClassExtensionKind.LotSizeMultiple,
            extensions);

        Add(
            descriptor.Capacity.HasAdditionalProductionCapacity,
            LotSizingProblemClassExtensionKind.AdditionalProductionCapacity,
            extensions);

        Add(
            descriptor.Capacity.HasAdditionalWarehouseCapacity,
            LotSizingProblemClassExtensionKind.AdditionalWarehouseCapacity,
            extensions);

        Add(
            descriptor.Capacity.HasAdditionalTransportCapacity,
            LotSizingProblemClassExtensionKind.AdditionalTransportCapacity,
            extensions);

        Add(
            descriptor.Procurement.HasPurchasing,
            LotSizingProblemClassExtensionKind.Purchasing,
            extensions);

        Add(
            descriptor.Capacity.HasSupplierCapacity,
            LotSizingProblemClassExtensionKind.SupplierCapacity,
            extensions);

        Add(
            descriptor.Procurement.HasSupplierLeadTimes,
            LotSizingProblemClassExtensionKind.SupplierLeadTime,
            extensions);

        Add(
            descriptor.TransportationDistribution.HasTransportation,
            LotSizingProblemClassExtensionKind.Transportation,
            extensions);

        Add(
            descriptor.Capacity.HasTransportCapacity,
            LotSizingProblemClassExtensionKind.TransportCapacity,
            extensions);

        Add(
            descriptor.TransportationDistribution.HasTransportLeadTimes,
            LotSizingProblemClassExtensionKind.TransportLeadTime,
            extensions);

        Add(
            descriptor.TransportationDistribution.HasDistribution,
            LotSizingProblemClassExtensionKind.Distribution,
            extensions);

        Add(
            descriptor.Capacity.HasWarehouseCapacity,
            LotSizingProblemClassExtensionKind.WarehouseCapacity,
            extensions);

        Add(
            descriptor.Structure.IsMultiSite,
            LotSizingProblemClassExtensionKind.MultiSite,
            extensions);

        Add(
            descriptor.ObjectiveFinance.HasFinancialConstraints,
            LotSizingProblemClassExtensionKind.FinancialConstraints,
            extensions);

        Add(
            descriptor.ObjectiveFinance.HasMultipleObjectives,
            LotSizingProblemClassExtensionKind.MultipleObjectives,
            extensions);

        return extensions
            .Distinct()
            .OrderBy(extension => (int)extension)
            .ToArray();
    }

    private static void Add(
        bool condition,
        LotSizingProblemClassExtensionKind extension,
        ICollection<LotSizingProblemClassExtensionKind> target)
    {
        if (condition)
        {
            target.Add(extension);
        }
    }
}
