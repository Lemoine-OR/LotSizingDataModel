using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the fixed cost incurred when a resource is prepared
/// to manufacture, store or transport an item during a planning period.
///
/// This cost is incurred once when the corresponding item-resource
/// activity is activated, independently of the processed quantity.
///
/// Corresponds to the UML class "Cout fixe preparation".
/// Its values correspond to Cprep[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "fixedSetupCost")]
public sealed class FixedSetupCost :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty fixed-setup-cost parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public FixedSetupCost()
    {
    }

    /// <summary>
    /// Initializes a fixed-setup-cost parameter for the specified
    /// planning horizon.
    ///
    /// Every planning period is initially assigned a cost of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public FixedSetupCost(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero fixed setup cost
    {
    }

    /// <summary>
    /// Initializes a fixed-setup-cost parameter and assigns
    /// the same cost to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultFixedSetupCost">
    /// Fixed setup cost initially assigned to every period.
    /// </param>
    public FixedSetupCost(
        int planningHorizon,
        double defaultFixedSetupCost)
        : base(
            planningHorizon,
            defaultFixedSetupCost)  // Initialize all periods with the specified fixed setup cost
    {
    }

    /// <summary>
    /// Gets the complete fixed-setup-cost time series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries FixedSetupCostByPeriod =>
        Values;

    /// <summary>
    /// Gets the fixed setup cost for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetFixedSetupCost(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the fixed setup cost for a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="cost">
    /// Non-negative finite fixed setup cost.
    /// </param>
    public void SetFixedSetupCost(
        int period,
        double cost)
    {
        SetValue(period, cost);
    }

    /// <summary>
    /// Validates a fixed setup cost.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the fixed setup cost is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A fixed setup cost cannot be negative.");
        }
    }
}