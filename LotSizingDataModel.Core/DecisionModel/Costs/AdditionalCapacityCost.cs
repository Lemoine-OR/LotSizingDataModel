using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.DecisionModel;

namespace LotSizingDataModel.Core.DecisionModel.Costs;

/// <summary>
/// Represents the unit cost incurred when one unit of additional
/// capacity is used during a planning period.
///
/// Corresponds to the UML class "Cout capacite supplementaire".
/// Its values correspond to COV[t] in the source model.
/// </summary>
[Serializable]
[XmlType(TypeName = "additionalCapacityCost")]
public sealed class AdditionalCapacityCost :
    DoubleTimeSeriesParameter
{
    /// <summary>
    /// Initializes an empty additional-capacity-cost parameter.
    ///
    /// This public parameterless constructor is required
    /// by <see cref="XmlSerializer"/>.
    /// </summary>
    public AdditionalCapacityCost()
    {
    }

    /// <summary>
    /// Initializes an additional-capacity-cost parameter for
    /// the specified planning horizon.
    ///
    /// Every planning period is initially assigned a unit cost
    /// of zero.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    public AdditionalCapacityCost(int planningHorizon)
        : base(
            planningHorizon,
            defaultValue: 0.0)  // Initialize all periods with zero additional capacity unit cost
    {
    }

    /// <summary>
    /// Initializes an additional-capacity-cost parameter and
    /// assigns the same unit cost to every planning period.
    /// </summary>
    /// <param name="planningHorizon">
    /// Number of periods in the planning horizon.
    /// </param>
    /// <param name="defaultUnitCost">
    /// Additional-capacity unit cost initially assigned
    /// to every planning period.
    /// </param>
    public AdditionalCapacityCost(
        int planningHorizon,
        double defaultUnitCost)
        : base(
            planningHorizon,
            defaultUnitCost)  // Initialize all periods with the specified unit cost
    {
    }

    /// <summary>
    /// Gets the complete additional-capacity unit-cost series.
    ///
    /// This property is an alias for <see cref="LotSizingDataModel.Core.DecisionModel.DoubleTimeSeriesParameter.Values"/>
    /// and is not serialized separately.
    /// </summary>
    [XmlIgnore]
    public DoubleTimeSeries UnitCostByPeriod =>
        Values;

    /// <summary>
    /// Gets the cost of using one unit of additional capacity
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    public double GetUnitCost(int period)
    {
        return GetValue(period);
    }

    /// <summary>
    /// Sets the cost of using one unit of additional capacity
    /// during a planning period.
    /// </summary>
    /// <param name="period">
    /// One-based planning-period number.
    /// </param>
    /// <param name="unitCost">
    /// Non-negative finite cost per unit of additional capacity.
    /// </param>
    public void SetUnitCost(
        int period,
        double unitCost)
    {
        SetValue(period, unitCost);
    }

    /// <summary>
    /// Validates an additional-capacity unit cost.
    /// </summary>
    protected override void ValidateValue(
        double value,
        string parameterName)
    {
        // First validate that the value is finite (base class validation)
        base.ValidateValue(value, parameterName);

        // Then validate that the additional capacity unit cost is non-negative
        if (value < 0.0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "An additional-capacity unit cost cannot be negative.");
        }
    }
}