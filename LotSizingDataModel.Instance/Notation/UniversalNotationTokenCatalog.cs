using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors.Network;

namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Stable canonical token mapping for notation scheme version 1.
/// </summary>
internal static class UniversalNotationTokenCatalog
{
    private static readonly IReadOnlyDictionary<
        UniversalNotationFeature,
        string> FeatureToToken =
            new Dictionary<UniversalNotationFeature, string>
            {
                [UniversalNotationFeature.Demand] = "Dem",
                [UniversalNotationFeature.DeterministicDemand] = "Det",
                [UniversalNotationFeature.TimeVaryingDemand] = "DVar",
                [UniversalNotationFeature.Production] = "Prod",

                [UniversalNotationFeature.ProductionCapacity] = "Cap:P",
                [UniversalNotationFeature.SharedProductionCapacity] = "Cap:Shared",
                [UniversalNotationFeature.TimeVaryingProductionCapacity] = "Cap:Var",
                [UniversalNotationFeature.SupplierCapacity] = "Cap:S",
                [UniversalNotationFeature.TransportCapacity] = "Cap:T",
                [UniversalNotationFeature.WarehouseCapacity] = "Cap:W",

                [UniversalNotationFeature.SetupCost] = "SC",
                [UniversalNotationFeature.SetupTime] = "ST",
                [UniversalNotationFeature.StartUpCost] = "SU",
                [UniversalNotationFeature.ProductionLeadTime] = "LT:P",

                [UniversalNotationFeature.MinimumLotSize] = "MinLot",
                [UniversalNotationFeature.MaximumLotSize] = "MaxLot",
                [UniversalNotationFeature.LotSizeMultiple] = "LotMult",

                [UniversalNotationFeature.AdditionalProductionCapacity] = "AddCap:P",
                [UniversalNotationFeature.AdditionalWarehouseCapacity] = "AddCap:W",
                [UniversalNotationFeature.AdditionalTransportCapacity] = "AddCap:T",

                [UniversalNotationFeature.InitialInventory] = "InitInv",
                [UniversalNotationFeature.SafetyStock] = "SS",
                [UniversalNotationFeature.Backlogging] = "BL",
                [UniversalNotationFeature.LostSales] = "LS",

                [UniversalNotationFeature.Purchasing] = "Buy",
                [UniversalNotationFeature.SupplierLeadTime] = "LT:S",
                [UniversalNotationFeature.Transportation] = "Tr",
                [UniversalNotationFeature.TransportLeadTime] = "LT:T",
                [UniversalNotationFeature.Distribution] = "Dist",

                [UniversalNotationFeature.FinancialConstraint] = "Fin"
            };

    private static readonly IReadOnlyDictionary<
        string,
        UniversalNotationFeature> TokenToFeature =
            FeatureToToken.ToDictionary(
                pair => pair.Value,
                pair => pair.Key,
                StringComparer.OrdinalIgnoreCase);

    public static string GetFeatureToken(
        UniversalNotationFeature feature)
    {
        if (!FeatureToToken.TryGetValue(feature, out string? token))
        {
            throw new ArgumentOutOfRangeException(
                nameof(feature),
                feature,
                "No canonical beta token is registered.");
        }

        return token;
    }

    public static bool TryParseFeature(
        string token,
        out UniversalNotationFeature feature)
    {
        return TokenToFeature.TryGetValue(
            token,
            out feature);
    }

    public static string GetProductStructureCode(
        ProductStructureType type)
    {
        return type switch
        {
            ProductStructureType.Unknown => "?",
            ProductStructureType.IndependentItems => "IND",
            ProductStructureType.Serial => "SER",
            ProductStructureType.Assembly => "ASM",
            ProductStructureType.Arborescent => "ARB",
            ProductStructureType.General => "GEN",
            _ => throw new ArgumentOutOfRangeException(
                nameof(type),
                type,
                "Unknown product-structure type.")
        };
    }

    public static ProductStructureType ParseProductStructureCode(
        string code)
    {
        return code.ToUpperInvariant() switch
        {
            "?" => ProductStructureType.Unknown,
            "IND" => ProductStructureType.IndependentItems,
            "SER" => ProductStructureType.Serial,
            "ASM" => ProductStructureType.Assembly,
            "ARB" => ProductStructureType.Arborescent,
            "GEN" => ProductStructureType.General,
            _ => throw new FormatException(
                $"Unknown product-structure token '{code}'.")
        };
    }

    public static string GetNetworkTopologyCode(
        SupplyNetworkTopologyType topology)
    {
        return topology switch
        {
            SupplyNetworkTopologyType.Unknown => "UNK",
            SupplyNetworkTopologyType.Independent => "IND",
            SupplyNetworkTopologyType.Serial => "SER",
            SupplyNetworkTopologyType.Convergent => "CONV",
            SupplyNetworkTopologyType.Divergent => "DIV",
            SupplyNetworkTopologyType.Tree => "TREE",
            SupplyNetworkTopologyType.General => "GEN",
            _ => throw new ArgumentOutOfRangeException(
                nameof(topology),
                topology,
                "Unknown supply-network topology.")
        };
    }

    public static SupplyNetworkTopologyType ParseNetworkTopologyCode(
        string code)
    {
        return code.ToUpperInvariant() switch
        {
            "UNK" => SupplyNetworkTopologyType.Unknown,
            "IND" => SupplyNetworkTopologyType.Independent,
            "SER" => SupplyNetworkTopologyType.Serial,
            "CONV" => SupplyNetworkTopologyType.Convergent,
            "DIV" => SupplyNetworkTopologyType.Divergent,
            "TREE" => SupplyNetworkTopologyType.Tree,
            "GEN" => SupplyNetworkTopologyType.General,
            _ => throw new FormatException(
                $"Unknown supply-network topology token '{code}'.")
        };
    }
}
