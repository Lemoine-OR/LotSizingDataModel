namespace LotSizingDataModel.Instance.Notation.Matching;

/// <summary>
/// Describes the semantic relationship between an instance descriptor
/// and a universal-notation problem specification.
/// </summary>
public enum UniversalNotationMatchKind
{
    /// <summary>
    /// The specification is the exact canonical notation generated from
    /// the descriptor.
    /// </summary>
    Exact,

    /// <summary>
    /// Every explicit specification requirement is satisfied, but the
    /// descriptor contains additional information not required by the
    /// specification.
    /// </summary>
    Compatible,

    /// <summary>
    /// No contradiction is known, but at least one explicit specification
    /// requirement cannot be decided from the currently known descriptor.
    /// </summary>
    Incomplete,

    /// <summary>
    /// At least one explicit specification requirement contradicts a known
    /// descriptor characteristic.
    /// </summary>
    Contradiction
}
