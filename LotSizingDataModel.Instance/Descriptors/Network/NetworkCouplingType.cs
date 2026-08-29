namespace LotSizingDataModel.Instance.Descriptors.Network;

/// <summary>
/// Describes coupling between forward and reverse physical networks.
/// </summary>
/// <remarks>
/// Current Core data represents forward flows only. Alpha.5 therefore emits
/// <see cref="ForwardOnly"/>. ReverseOnly and ClosedLoop are part of the
/// explicit future contract but are not emitted until reverse-flow source
/// data exists.
/// </remarks>
public enum NetworkCouplingType
{
    ForwardOnly,
    ReverseOnly,
    ClosedLoop
}
