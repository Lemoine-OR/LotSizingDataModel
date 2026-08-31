using System.Xml.Serialization;
using LotSizingDataModel.Core.DecisionModel.Finance;

namespace LotSizingDataModel.Core;

/// <summary>
/// Adds executable cash-flow semantics to the supply-chain root.
/// </summary>
public sealed partial class SupplyChain
{
    private CashFlowPolicy? _cashFlowPolicy;

    /// <summary>
    /// Gets or sets optional cash-flow and solvency semantics.
    /// </summary>
    [XmlElement("cashFlowPolicy")]
    public CashFlowPolicy? CashFlowPolicy
    {
        get => _cashFlowPolicy;
        set => SetProperty(ref _cashFlowPolicy, value);
    }
}
