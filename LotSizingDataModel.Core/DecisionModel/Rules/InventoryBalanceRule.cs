using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Rules;

/// <summary>
/// Represents the inventory-balance management rule.
///
/// For each item and warehouse, the ending inventory of a period
/// is equal to the previous inventory level plus incoming flows
/// minus outgoing flows.
///
/// Corresponds to the UML class "Equilibre des stocks".
///
/// This class intentionally contains no numerical parameter because
/// the balance equation is inherent to every inventory.
/// </summary>
[Serializable]
[XmlType(TypeName = "inventoryBalanceRule")]
public sealed class InventoryBalanceRule : ModelObject
{
    /// <summary>
    /// Initializes an inventory-balance rule.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public InventoryBalanceRule()
    {
    }
}