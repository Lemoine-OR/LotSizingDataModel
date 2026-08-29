namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Implementation support of one scientifically catalogued problem class.
/// </summary>
public enum LotSizingProblemClassSupportLevel
{
    /// <summary>
    /// The current descriptor/notation stack can assess membership.
    /// </summary>
    Executable,

    /// <summary>
    /// Scientifically catalogued, but current model semantics are
    /// insufficient for executable membership assessment.
    /// </summary>
    CatalogOnly
}
