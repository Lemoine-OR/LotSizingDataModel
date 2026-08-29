namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Relationship between an instance descriptor and one canonical problem
/// class.
/// </summary>
public enum LotSizingProblemClassMatchKind
{
    ExactCore,
    CompatibleExtension,
    Incomplete,
    NotApplicable,
    NotRepresentable
}
