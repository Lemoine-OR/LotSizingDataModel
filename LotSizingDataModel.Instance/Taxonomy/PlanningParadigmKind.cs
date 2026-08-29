namespace LotSizingDataModel.Instance.Taxonomy;

/// <summary>
/// Identifies planning paradigms that may use lot-sizing models without
/// themselves being lot-sizing problem classes.
/// </summary>
public enum PlanningParadigmKind
{
    Unspecified,

    /// <summary>
    /// Material Requirements Planning.
    /// </summary>
    MaterialRequirementsPlanning,

    /// <summary>
    /// Distribution Requirements Planning.
    /// </summary>
    DistributionRequirementsPlanning
}
