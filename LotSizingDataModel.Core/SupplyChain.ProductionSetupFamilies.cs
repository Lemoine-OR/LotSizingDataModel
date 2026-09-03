using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core;

/// <summary>
/// Adds generic shared production-setup families to SupplyChain.
/// </summary>
public sealed partial class SupplyChain
{
    [XmlArray("productionSetupFamilies")]
    [XmlArrayItem("productionSetupFamily")]
    public List<ProductionSetupFamily> ProductionSetupFamilies
    {
        get;
    } = new();

    /// <summary>
    /// Adds a setup family while preserving identifier uniqueness
    /// and synchronizing period-dependent data.
    /// </summary>
    public void AddProductionSetupFamily(
        ProductionSetupFamily setupFamily)
    {
        ArgumentNullException.ThrowIfNull(setupFamily);

        if (setupFamily.Id <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(setupFamily),
                "Production setup-family identifiers must be strictly positive.");
        }

        if (ProductionSetupFamilies.Any(
                family => family.Id == setupFamily.Id))
        {
            throw new InvalidOperationException(
                "A production setup family with the same identifier already exists.");
        }

        if (PlanningHorizon > 0 &&
            setupFamily.SetupTime is not null)
        {
            setupFamily.ResizeTimeSeries(PlanningHorizon);
        }

        ProductionSetupFamilies.Add(setupFamily);

        OnPropertyChanged(nameof(ProductionSetupFamilies));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }

    /// <summary>
    /// Prevents empty family collections from changing the XML of
    /// legacy instances during a round trip.
    /// </summary>
    public bool ShouldSerializeProductionSetupFamilies() =>
        ProductionSetupFamilies.Count > 0;
}
