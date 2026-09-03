using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core;

public sealed partial class SupplyChain
{
    [XmlArray("productionSetupFamilies")]
    [XmlArrayItem("productionSetupFamily")]
    public List<ProductionSetupFamily> ProductionSetupFamilies
    {
        get;
    } = new();

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

        if (PlanningHorizon > 0)
        {
            setupFamily.ResizeTimeSeries(PlanningHorizon);
        }

        ProductionSetupFamilies.Add(setupFamily);
        OnPropertyChanged(nameof(ProductionSetupFamilies));
        OnPropertyChanged(nameof(HasConsistentPlanningHorizon));
    }

    public bool ShouldSerializeProductionSetupFamilies() =>
        ProductionSetupFamilies.Count > 0;
}
