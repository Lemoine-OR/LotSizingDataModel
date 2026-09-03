namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// Stable feature codes used by LSI/1 beta signatures.
/// </summary>
public static class LsiFeatureCodes
{
    public const string Demand = "DEM";
    public const string Production = "PROD";

    public const string ProductionCapacity = "CAP.P";
    public const string SharedProductionCapacity = "CAP.P.SH";
    public const string WarehouseCapacity = "CAP.W";
    public const string SupplierCapacity = "CAP.S";
    public const string TransportCapacity = "CAP.T";

    public const string AdditionalProductionCapacity = "CAP.ADD.P";
    public const string AdditionalWarehouseCapacity = "CAP.ADD.W";
    public const string AdditionalTransportCapacity = "CAP.ADD.T";

    public const string SetupCost = "SET.C";
    public const string SetupTime = "SET.T";
    public const string SequenceDependentSetupTime = "SET.SD.T";
    public const string SequenceDependentSetupCost = "SET.SD.C";
    public const string ProductionSetupFamily = "SET.FAM";
    public const string ProductionSetupFamilyTime = "SET.FAM.T";
    public const string StartUpCost = "SET.SU.C";
    public const string StartUpTime = "SET.SU.T";
    public const string InitialSetupState = "SET.INIT";
    public const string SetupCarryOver = "SET.CO";
    public const string SequenceDependentChangeoverTime = "SET.SD.T";
    public const string SequenceDependentChangeoverCost = "SET.SD.C";

    public const string MinimumLotSize = "LOT.MIN";
    public const string MaximumLotSize = "LOT.MAX";
    public const string LotSizeMultiple = "LOT.MUL";
    public const string GroupingConstraint = "LOT.GRP";

    public const string ProductionLeadTime = "LT.P";
    public const string SupplierLeadTime = "LT.S";
    public const string TransportLeadTime = "LT.T";

    public const string InitialInventory = "INV.I0";
    public const string SafetyStock = "INV.SS";

    public const string Backlogging = "SHORT.BO";
    public const string LostSales = "SHORT.LS";

    public const string Purchasing = "SRC.BUY";
    public const string Transportation = "TRANS";
    public const string Distribution = "DIST";
    public const string MultiSite = "NET.MULTI";
    public const string FinancialConstraints = "FIN";
    public const string MultipleObjectives = "OBJ.MULTI";

    public const string IntegratedScheduling = "SCH";
    public const string BigBucketScheduling = "SCH.BB";
    public const string SmallBucketScheduling = "SCH.SB";
    public const string MacroMicroScheduling = "SCH.MM";
    public const string MaximumSetupCount = "SCH.MAXSET";
}
