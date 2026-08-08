namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Provides the canonical mathematical domain-key segment names
/// used by lot-sizing variables and constraints.
/// </summary>
public static class MathematicalDomainKeySegment
{
    /// <summary>
    /// Item or product identifier segment.
    /// </summary>
    public const string Item =
        "item";

    /// <summary>
    /// Product identifier segment.
    /// </summary>
    /// <remarks>
    /// This alias is provided for formulations that use the term
    /// product rather than item.
    /// </remarks>
    public const string Product =
        "product";

    /// <summary>
    /// Plant identifier segment.
    /// </summary>
    public const string Plant =
        "plant";

    /// <summary>
    /// Warehouse identifier segment.
    /// </summary>
    public const string Warehouse =
        "warehouse";

    /// <summary>
    /// Distribution-center identifier segment.
    /// </summary>
    public const string DistributionCenter =
        "distributionCenter";

    /// <summary>
    /// Work-center identifier segment.
    /// </summary>
    public const string WorkCenter =
        "workCenter";

    /// <summary>
    /// Resource identifier segment.
    /// </summary>
    public const string Resource =
        "resource";

    /// <summary>
    /// Transport-resource identifier segment.
    /// </summary>
    public const string TransportResource =
        "transportResource";

    /// <summary>
    /// Routing identifier segment.
    /// </summary>
    public const string Routing =
        "routing";

    /// <summary>
    /// Generic source-location identifier segment.
    /// </summary>
    public const string Source =
        "source";

    /// <summary>
    /// Generic destination-location identifier segment.
    /// </summary>
    public const string Destination =
        "destination";

    /// <summary>
    /// Standalone origin-warehouse identifier segment.
    /// </summary>
    public const string OriginWarehouse =
        "originWarehouse";

    /// <summary>
    /// Origin-plant identifier segment.
    /// </summary>
    /// <remarks>
    /// The segment identifies the warehouse attached to the
    /// specified plant.
    /// </remarks>
    public const string OriginPlant =
        "originPlant";

    /// <summary>
    /// Standalone destination-warehouse identifier segment.
    /// </summary>
    public const string DestinationWarehouse =
        "destinationWarehouse";

    /// <summary>
    /// Destination-plant identifier segment.
    /// </summary>
    /// <remarks>
    /// The segment identifies the warehouse attached to the
    /// specified plant.
    /// </remarks>
    public const string DestinationPlant =
        "destinationPlant";

    /// <summary>
    /// Supplier identifier segment.
    /// </summary>
    public const string Supplier =
        "supplier";

    /// <summary>
    /// Period index segment.
    /// </summary>
    public const string Period =
        "period";

    /// <summary>
    /// Operation identifier segment.
    /// </summary>
    public const string Operation =
        "operation";
}
