using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Core.PhysicalModel;

namespace LotSizingDataModel.Core.Validation;

/// <summary>
/// Validates work-center setup carry-over and sequence-dependent transitions.
/// </summary>
public static class ProductionSetupTransitionValidator
{
    public static void AppendIssues(
        SupplyChain supplyChain,
        ICollection<SupplyChainValidator.ValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);
        ArgumentNullException.ThrowIfNull(issues);

        foreach (var entry in
                 supplyChain.Plants.SelectMany(
                     plant => plant.WorkCenters.Select(
                         workCenter =>
                             (PlantId: plant.Id, WorkCenter: workCenter))))
        {
            ProductionSetupTransitionProfile? profile =
                entry.WorkCenter.SetupTransitionProfile;

            if (profile is null)
            {
                continue;
            }

            string path =
                "plants[" + entry.PlantId + "].workCenters[" +
                entry.WorkCenter.Id + "].setupTransitionProfile";

            if (!profile.HasConsistentPlanningHorizon)
            {
                AddError(
                    issues,
                    "SETUP_TRANSITION.HORIZON_MISMATCH",
                    path,
                    "All sequence-dependent transition parameters must use one planning horizon.");
            }

            foreach (ProductionChangeover changeover in profile.Changeovers)
            {
                if (changeover.FromItemId <= 0 ||
                    changeover.ToItemId <= 0 ||
                    changeover.FromItemId == changeover.ToItemId)
                {
                    AddError(
                        issues,
                        "SETUP_TRANSITION.INVALID_PAIR",
                        path + ".changeovers",
                        "A changeover requires two distinct strictly positive item identifiers.");

                    continue;
                }

                bool fromExists =
                    supplyChain.Items.Any(
                        item => item.Id == changeover.FromItemId);
                bool toExists =
                    supplyChain.Items.Any(
                        item => item.Id == changeover.ToItemId);

                if (!fromExists || !toExists)
                {
                    AddError(
                        issues,
                        "SETUP_TRANSITION.ITEM_NOT_FOUND",
                        path + ".changeovers",
                        "A changeover references an item that does not exist.");
                }

                bool fromRoutable =
                    supplyChain.ProductionRoutings.Any(
                        routing =>
                            routing.ItemId == changeover.FromItemId &&
                            routing.PlantId == entry.PlantId &&
                            routing.UsesWorkCenter(
                                entry.WorkCenter.Id));

                bool toRoutable =
                    supplyChain.ProductionRoutings.Any(
                        routing =>
                            routing.ItemId == changeover.ToItemId &&
                            routing.PlantId == entry.PlantId &&
                            routing.UsesWorkCenter(
                                entry.WorkCenter.Id));

                if (!fromRoutable || !toRoutable)
                {
                    AddError(
                        issues,
                        "SETUP_TRANSITION.ROUTING_NOT_FOUND",
                        path + ".changeovers",
                        "Both changeover items must be producible on the referenced work center.");
                }
            }

            int duplicateCount =
                profile.Changeovers
                    .GroupBy(changeover =>
                        (changeover.FromItemId, changeover.ToItemId))
                    .Count(group => group.Count() > 1);

            if (duplicateCount > 0)
            {
                AddError(
                    issues,
                    "SETUP_TRANSITION.DUPLICATE_PAIR",
                    path + ".changeovers",
                    "A directed changeover pair may only be declared once.");
            }
        }
    }

    private static void AddError(
        ICollection<SupplyChainValidator.ValidationIssue> issues,
        string code,
        string path,
        string message)
    {
        issues.Add(
            new SupplyChainValidator.ValidationIssue(
                SupplyChainValidator.ValidationSeverity.Error,
                code,
                path,
                message));
    }
}
