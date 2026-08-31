using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.Relationships;

/// <summary>
/// Specifies how the stock before period 1 is treated by the optimization model.
/// </summary>
public enum InitialInventoryDecisionMode
{
    /// <summary>
    /// <see cref="Inventory.InitialInventory"/> is a fixed input parameter.
    /// This preserves the historical LotSizingDataModel behavior.
    /// </summary>
    FixedParameter = 0,

    /// <summary>
    /// Initial inventory is an explicit non-negative optimization decision.
    /// The fixed <see cref="Inventory.InitialInventory"/> value must then be zero.
    /// </summary>
    VariableDecision = 1,

    /// <summary>
    /// No initial-inventory decision exists and the initial stock is fixed to zero.
    /// </summary>
    AbsentFixedZero = 2
}

/// <summary>
/// Adds explicit initial-stock decision semantics to <see cref="Inventory"/>.
/// </summary>
/// <remarks>
/// The distinction is generic and is not named after a historical classification.
/// It permits an exact projection of Wolsey's DLSI/DLS distinction without treating
/// an acronym as model evidence.
/// </remarks>
public sealed partial class Inventory
{
    private InitialInventoryDecisionMode _initialInventoryDecisionMode =
        InitialInventoryDecisionMode.FixedParameter;

    private double _initialInventoryDecisionUnitCost;

    /// <summary>
    /// Gets or sets whether initial inventory is fixed, a decision, or absent/fixed zero.
    /// </summary>
    [XmlAttribute("initialInventoryDecisionMode")]
    public InitialInventoryDecisionMode InitialInventoryDecisionMode
    {
        get => _initialInventoryDecisionMode;
        set => SetProperty(ref _initialInventoryDecisionMode, value);
    }

    /// <summary>
    /// Gets or sets the unit cost applied to an initial-inventory decision.
    /// </summary>
    /// <remarks>
    /// This coefficient corresponds to the period-zero stock coefficient when the
    /// initial stock is a decision variable. It is ignored for fixed/absent modes.
    /// </remarks>
    [XmlAttribute("initialInventoryDecisionUnitCost")]
    public double InitialInventoryDecisionUnitCost
    {
        get => _initialInventoryDecisionUnitCost;
        set
        {
            if (!double.IsFinite(value) || value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The initial-inventory decision unit cost must be finite and non-negative.");
            }

            SetProperty(ref _initialInventoryDecisionUnitCost, value);
        }
    }

    /// <summary>
    /// Gets whether the initial-inventory semantics are internally coherent.
    /// </summary>
    [XmlIgnore]
    public bool HasValidInitialInventoryDecisionSemantics =>
        InitialInventoryDecisionMode == InitialInventoryDecisionMode.FixedParameter ||
        InitialInventory == 0.0;
}
