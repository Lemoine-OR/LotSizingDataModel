using LotSizingDataModel.Instance.Descriptors.Temporal;

namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Qualifies one universal model-parameter family with a generic temporal
/// pattern.
/// </summary>
public sealed record UniversalTemporalQualifier
{
    public UniversalTemporalQualifier(
        UniversalTemporalParameter parameter,
        TemporalPatternType pattern)
    {
        if (!Enum.IsDefined(parameter))
        {
            throw new ArgumentOutOfRangeException(
                nameof(parameter),
                parameter,
                "Unknown universal temporal parameter.");
        }

        if (!Enum.IsDefined(pattern))
        {
            throw new ArgumentOutOfRangeException(
                nameof(pattern),
                pattern,
                "Unknown temporal pattern.");
        }

        Parameter = parameter;
        Pattern = pattern;
    }

    public UniversalTemporalParameter Parameter { get; }
    public TemporalPatternType Pattern { get; }
}
