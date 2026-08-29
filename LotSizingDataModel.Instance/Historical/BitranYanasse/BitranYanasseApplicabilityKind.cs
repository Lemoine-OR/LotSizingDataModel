namespace LotSizingDataModel.Instance.Historical.BitranYanasse;

/// <summary>
/// Describes whether an instance descriptor belongs to the scientific domain
/// of the historical Bitran-Yanasse capacitated single-item classification.
/// </summary>
public enum BitranYanasseApplicabilityKind
{
    /// <summary>
    /// The descriptor does not contain enough basic information to assess
    /// applicability.
    /// </summary>
    Incomplete,

    /// <summary>
    /// The descriptor contradicts at least one defining domain condition.
    /// </summary>
    NotApplicable,

    /// <summary>
    /// The descriptor satisfies the classical domain conditions and contains
    /// no currently identified extension outside that domain.
    /// </summary>
    ExactHistoricalDomain,

    /// <summary>
    /// The classical domain is present, but the instance also contains
    /// extensions not represented by the historical classification.
    /// </summary>
    ExtendedButProjectable
}
