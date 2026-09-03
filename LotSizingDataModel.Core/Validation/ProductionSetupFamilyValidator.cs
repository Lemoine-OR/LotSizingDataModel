using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Validation;

/// <summary>
/// Validates generic shared production-setup families.
/// </summary>
public static class ProductionSetupFamilyValidator
{
    public static void AppendIssues(
        SupplyChain supplyChain,
        ICollection<SupplyChainValidator.ValidationIssue> issues)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);
        ArgumentNullException.ThrowIfNull(issues);

        int[] duplicateIds =
            supplyChain.ProductionSetupFamilies
                .GroupBy(family => family.Id)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToArray();

        foreach (int duplicateId in duplicateIds)
        {
            AddError(
                issues,
                "PRODUCTION_SETUP_FAMILY.DUPLICATE_ID",
                "productionSetupFamilies",
                "Production setup-family identifier " +
                duplicateId +
                " is duplicated.");
        }

        foreach (ProductionSetupFamily family
                 in supplyChain.ProductionSetupFamilies)
        {
            string path =
                "productionSetupFamilies[" +
                family.Id +
                "]";

            if (family.Id <= 0)
            {
                AddError(
                    issues,
                    "PRODUCTION_SETUP_FAMILY.INVALID_ID",
                    path,
                    "A production setup-family identifier must be strictly positive.");
            }

            if (family.WorkCenter.PlantId <= 0 ||
                family.WorkCenter.WorkCenterId <= 0)
            {
                AddError(
                    issues,
                    "PRODUCTION_SETUP_FAMILY.INVALID_WORK_CENTER_REFERENCE",
                    path + ".workCenter",
                    "The work-center reference must contain strictly positive identifiers.");

                continue;
            }

            WorkCenter? workCenter =
                supplyChain.Plants
                    .Where(plant =>
                        plant.Id == family.WorkCenter.PlantId)
                    .SelectMany(plant =>
                        plant.WorkCenters)
                    .FirstOrDefault(candidate =>
                        candidate.Id ==
                        family.WorkCenter.WorkCenterId);

            if (workCenter is null)
            {
                AddError(
                    issues,
                    "PRODUCTION_SETUP_FAMILY.WORK_CENTER_NOT_FOUND",
                    path + ".workCenter",
                    "The referenced work center does not exist.");
            }

            if (!family.HasMembers)
            {
                AddError(
                    issues,
                    "PRODUCTION_SETUP_FAMILY.EMPTY",
                    path + ".memberItemIds",
                    "A production setup family must contain at least one item.");
            }

            if (!family.HasConsistentMemberIds)
            {
                AddError(
                    issues,
                    "PRODUCTION_SETUP_FAMILY.INVALID_MEMBERS",
                    path + ".memberItemIds",
                    "Member item identifiers must be strictly positive and unique.");
            }

            foreach (int itemId in
                     family.MemberItemIds.Distinct())
            {
                bool itemExists =
                    supplyChain.Items.Any(
                        item => item.Id == itemId);

                if (!itemExists)
                {
                    AddError(
                        issues,
                        "PRODUCTION_SETUP_FAMILY.ITEM_NOT_FOUND",
                        path + ".memberItemIds",
                        "Member item " +
                        itemId +
                        " does not exist.");

                    continue;
                }

                bool hasRoutingOnFamilyWorkCenter =
                    supplyChain.ProductionRoutings.Any(
                        routing =>
                            routing.ItemId == itemId &&
                            routing.WorkCenters.Any(
                                reference =>
                                    reference.PlantId ==
                                        family.WorkCenter.PlantId &&
                                    reference.WorkCenterId ==
                                        family.WorkCenter.WorkCenterId));

                if (!hasRoutingOnFamilyWorkCenter)
                {
                    AddError(
                        issues,
                        "PRODUCTION_SETUP_FAMILY.NO_MEMBER_ROUTING",
                        path + ".memberItemIds",
                        "Member item " +
                        itemId +
                        " has no production routing on the family's work center.");
                }
            }

            if (family.SetupTime is not null &&
                family.SetupTime.PlanningHorizon !=
                    supplyChain.PlanningHorizon)
            {
                AddError(
                    issues,
                    "PRODUCTION_SETUP_FAMILY.HORIZON_MISMATCH",
                    path + ".setupTime",
                    "Family setup time must use the global planning horizon.");
            }

            if (family.SetupTime is not null &&
                family.SetupTime.Values.Any(value => value > 0.0) &&
                workCenter is not null &&
                workCenter.CapacityConstraint is null)
            {
                AddError(
                    issues,
                    "PRODUCTION_SETUP_FAMILY.CAPACITY_REQUIRED",
                    path + ".setupTime",
                    "Positive family setup time requires a capacity-constrained work center.");
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
