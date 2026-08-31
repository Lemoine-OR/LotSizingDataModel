using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solution.Decisions;

/// <summary>
/// Adds the explicit period-zero stock decision to an inventory decision.
/// </summary>
public sealed partial class InventoryDecision
{
    private double _initialInventoryLevel;

    /// <summary>
    /// Gets or sets the optimized stock available before period 1.
    /// </summary>
    [XmlAttribute("initialInventoryLevel")]
    public double InitialInventoryLevel
    {
        get => _initialInventoryLevel;
        set
        {
            if (!double.IsFinite(value) || value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The initial inventory level must be finite and non-negative.");
            }

            SetProperty(ref _initialInventoryLevel, value);
        }
    }
}
