namespace LotSizingDataModel.Instance.Descriptors;

/// <summary>Describes setup semantics exposed by the current model.</summary>
public sealed partial class SetupDescriptor
{
    public bool HasSetupCosts { get; init; }
    public bool HasSetupTimes { get; init; }
    public bool HasStartUpCosts { get; init; }
    public bool HasStartUpTimes { get; init; }
}
