namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes finance and objective semantics in the historical feature vector.</summary>
public sealed class ObjectiveFinanceDescriptor
{
    public bool HasFinancialConstraints { get; init; }
    public bool HasMultipleObjectives { get; init; }
}
