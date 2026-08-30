namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Implementation support of one scientifically catalogued problem class.
/// </summary>
public enum LotSizingProblemClassSupportLevel
{
    /// <summary>
    /// Membership is assessable and the class belongs to the currently
    /// executable scientific formulation/solver scope.
    /// </summary>
    Executable = 0,

    /// <summary>
    /// Scientifically catalogued, but current model semantics are
    /// insufficient for membership assessment.
    /// </summary>
    CatalogOnly = 1,

    /// <summary>
    /// Current Core/Instance semantics are sufficient to assess canonical
    /// membership, but no executable formulation/solver support is claimed.
    /// </summary>
    Classifiable = 2
}
