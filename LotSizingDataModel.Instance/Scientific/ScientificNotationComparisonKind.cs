namespace LotSizingDataModel.Instance.Scientific;

public enum ScientificNotationComparisonKind
{
    NotDeclared,

    /// <summary>
    /// A declaration exists but scientific classification was blocked before
    /// the declaration could be evaluated.
    /// </summary>
    NotEvaluated,

    InvalidDeclaredNotation,
    Exact,
    Compatible,
    Incomplete,
    Contradiction
}
