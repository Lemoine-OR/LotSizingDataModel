namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// Current implementation support of one scientific solution-method family.
/// </summary>
public enum ScientificSolutionMethodSupportLevel
{
    /// <summary>
    /// The current LotSizingDataModel solver stack can execute this method.
    /// </summary>
    Executable,

    /// <summary>
    /// Scientifically catalogued but not connected to this repository's
    /// execution pipeline yet.
    /// </summary>
    CatalogOnly
}
