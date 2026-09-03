using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Core.Relationships;

namespace LotSizingDataModel.Core.Validation;

/// <summary>
/// Validates the structural, referential and decision-model
/// consistency of a complete supply chain.
/// </summary>
public sealed class SupplyChainValidator
{
    /// <summary>
    /// Indicates the severity of a supply-chain validation issue.
    /// </summary>
    public enum ValidationSeverity
    {
        /// <summary>
        /// Indicates a non-blocking validation warning.
        /// </summary>
        Warning,

        /// <summary>
        /// Indicates a validation error that makes the model invalid.
        /// </summary>
        Error
    }

    /// <summary>
    /// Represents one issue detected during validation.
    /// </summary>
    public sealed class ValidationIssue
    {
        /// <summary>
        /// Initializes a new validation issue.
        /// </summary>
        /// <param name="severity">
        /// Severity of the detected issue.
        /// </param>
        /// <param name="code">
        /// Stable technical code identifying the validation rule.
        /// </param>
        /// <param name="path">
        /// Logical path of the object or property concerned
        /// by the issue.
        /// </param>
        /// <param name="message">
        /// Human-readable description of the issue.
        /// </param>
        public ValidationIssue(
            ValidationSeverity severity,
            string code,
            string path,
            string message)
        {
            Severity = severity;
            Code = code ??
                throw new ArgumentNullException(nameof(code));
            Path = path ??
                throw new ArgumentNullException(nameof(path));
            Message = message ??
                throw new ArgumentNullException(nameof(message));
        }

        /// <summary>
        /// Gets the severity of the validation issue.
        /// </summary>
        public ValidationSeverity Severity { get; }

        /// <summary>
        /// Gets the stable technical code identifying
        /// the validation rule.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets the logical path of the model element concerned
        /// by the validation issue.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets the human-readable description of the issue.
        /// </summary>
        public string Message { get; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return
                $"[{Severity}] {Code} at {Path}: {Message}";
        }
    }

    /// <summary>
    /// Validates a complete supply-chain model.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply chain to validate.
    /// </param>
    /// <returns>
    /// All errors and warnings detected in the model.
    /// </returns>
    public IReadOnlyList<ValidationIssue> Validate(
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        var issues = new List<ValidationIssue>();

        ValidateRoot(supplyChain, issues);
        ValidateLogicalSubsystem(supplyChain, issues);
        ValidatePhysicalSubsystem(supplyChain, issues);
        ValidateProductionRoutings(supplyChain, issues);
        ValidateProductionCharacteristics(supplyChain, issues);
        ValidateInventories(supplyChain, issues);
        ValidateTransportCharacteristics(supplyChain, issues);
        ValidateDemands(supplyChain, issues);
        ValidateDistributionCenterSourcings(
            supplyChain,
            issues);
        ValidateSupplierDeliveries(supplyChain, issues);
        ProductionSetupFamilyValidator.AppendIssues(
            supplyChain,
            issues);
        ProductionSetupTransitionValidator.AppendIssues(
            supplyChain,
            issues);

        return issues.AsReadOnly();
    }

    /// <summary>
    /// Determines whether a supply chain contains no
    /// validation error.
    ///
    /// Warnings do not make the model invalid.
    /// </summary>
    public bool IsValid(SupplyChain supplyChain)
    {
        return Validate(supplyChain).All(
            issue =>
                issue.Severity !=
                ValidationSeverity.Error);
    }

    /// <summary>
    /// Validates the model and throws an exception when at least
    /// one validation error is detected.
    /// </summary>
    public void ThrowIfInvalid(SupplyChain supplyChain)
    {
        // Collect only errors; warnings do not make the model invalid.
        ValidationIssue[] errors =
            Validate(supplyChain)
                .Where(
                    issue =>
                        issue.Severity ==
                        ValidationSeverity.Error)
                .ToArray();

        if (errors.Length == 0)
        {
            return;
        }

        string message = string.Join(
            Environment.NewLine,
            errors.Select(error => error.ToString()));

        throw new InvalidOperationException(
            "The supply-chain model is invalid." +
            Environment.NewLine +
            message);
    }

    private static void ValidateRoot(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        if (supplyChain.PlanningHorizon <= 0)
        {
            AddError(
                issues,
                "SC001",
                "supplyChain.planningHorizon",
                "The planning horizon must be strictly positive.");
        }

        if (supplyChain.Items.Count == 0)
        {
            AddError(
                issues,
                "SC002",
                "supplyChain.items",
                "The supply chain must contain at least one item.");
        }

        if (!supplyChain.HasConsistentPlanningHorizon)
        {
            AddError(
                issues,
                "SC003",
                "supplyChain",
                "At least one period-dependent object does not " +
                "use the global planning horizon.");
        }
    }

    private static void ValidateLogicalSubsystem(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        ValidateIdentifiedCollection(
            supplyChain.Items,
            item => item.Id,
            item => item.Name,
            "supplyChain.items",
            "item",
            issues);

        var requirementKeys =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0;
             index < supplyChain.ComponentRequirements.Count;
             index++)
        {
            var requirement =
                supplyChain.ComponentRequirements[index];

            string path =
                $"supplyChain.componentRequirements[{index}]";

            string key =
                $"{requirement.ParentItemId}:" +
                $"{requirement.ComponentItemId}";

            if (!requirementKeys.Add(key))
            {
                AddError(
                    issues,
                    "BOM001",
                    path,
                    "The same parent-item/component-item " +
                    "requirement is defined more than once.");
            }

            if (!ItemExists(
                    supplyChain,
                    requirement.ParentItemId))
            {
                AddError(
                    issues,
                    "BOM002",
                    path + ".parentItemId",
                    $"Item {requirement.ParentItemId} does not exist.");
            }

            if (!ItemExists(
                    supplyChain,
                    requirement.ComponentItemId))
            {
                AddError(
                    issues,
                    "BOM003",
                    path + ".componentItemId",
                    $"Item {requirement.ComponentItemId} does not exist.");
            }

            if (requirement.ParentItemId ==
                requirement.ComponentItemId)
            {
                AddError(
                    issues,
                    "BOM004",
                    path,
                    "An item cannot be its own component.");
            }

            if (requirement.Quantity <= 0)
            {
                AddError(
                    issues,
                    "BOM005",
                    path + ".quantity",
                    "A component quantity must be strictly positive.");
            }
        }

        ValidateBillOfMaterialsCycles(
            supplyChain,
            issues);
    }

    private static void ValidateBillOfMaterialsCycles(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        Dictionary<int, List<int>> graph =
            supplyChain.ComponentRequirements
                .GroupBy(
                    requirement =>
                        requirement.ParentItemId)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(
                            requirement =>
                                requirement.ComponentItemId)
                        .ToList());

        var states = new Dictionary<int, int>();

        foreach (int itemId in supplyChain.Items.Select(
                     item => item.Id))
        {
            if (ContainsCycle(
                    itemId,
                    graph,
                    states))
            {
                AddError(
                    issues,
                    "BOM006",
                    "supplyChain.componentRequirements",
                    "The bill of materials contains at least " +
                    "one circular dependency.");

                return;
            }
        }
    }

    private static bool ContainsCycle(
        int itemId,
        IReadOnlyDictionary<int, List<int>> graph,
        IDictionary<int, int> states)
    {
        if (states.TryGetValue(itemId, out int state))
        {
            return state == 1;
        }

        states[itemId] = 1;

        if (graph.TryGetValue(
                itemId,
                out List<int>? components))
        {
            foreach (int componentId in components)
            {
                if (ContainsCycle(
                        componentId,
                        graph,
                        states))
                {
                    return true;
                }
            }
        }

        states[itemId] = 2;

        return false;
    }

    private static void ValidatePhysicalSubsystem(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        ValidateIdentifiedCollection(
            supplyChain.Plants,
            plant => plant.Id,
            plant => plant.Name,
            "supplyChain.plants",
            "plant",
            issues);

        ValidateIdentifiedCollection(
            supplyChain.StandaloneWarehouses,
            warehouse => warehouse.Id,
            warehouse => warehouse.Name,
            "supplyChain.standaloneWarehouses",
            "standalone warehouse",
            issues);

        ValidateIdentifiedCollection(
            supplyChain.Suppliers,
            supplier => supplier.Id,
            supplier => supplier.Name,
            "supplyChain.suppliers",
            "supplier",
            issues);

        ValidateIdentifiedCollection(
            supplyChain.DistributionCenters,
            center => center.Id,
            center => center.Name,
            "supplyChain.distributionCenters",
            "distribution center",
            issues);

        ValidateIdentifiedCollection(
            supplyChain.TransportResources,
            resource => resource.Id,
            resource => resource.Name,
            "supplyChain.transportResources",
            "transport resource",
            issues);

        foreach (Plant plant in supplyChain.Plants)
        {
            string plantPath =
                $"supplyChain.plants[id={plant.Id}]";

            if (plant.WorkCenters.Count == 0)
            {
                AddError(
                    issues,
                    "PHY001",
                    plantPath + ".workCenters",
                    "A plant must contain at least one work center.");
            }

            ValidateIdentifiedCollection(
                plant.WorkCenters,
                workCenter => workCenter.Id,
                workCenter => workCenter.Name,
                plantPath + ".workCenters",
                "work center",
                issues);

            ValidateWarehouseDecisionModel(
                plant.Warehouse,
                plantPath + ".warehouse",
                supplyChain.PlanningHorizon,
                issues);

            foreach (WorkCenter workCenter
                     in plant.WorkCenters)
            {
                ValidateWorkCenterDecisionModel(
                    workCenter,
                    plantPath +
                    $".workCenters[id={workCenter.Id}]",
                    supplyChain.PlanningHorizon,
                    issues);
            }
        }

        foreach (StandaloneWarehouse warehouse
                 in supplyChain.StandaloneWarehouses)
        {
            ValidateWarehouseDecisionModel(
                warehouse,
                "supplyChain.standaloneWarehouses" +
                $"[id={warehouse.Id}]",
                supplyChain.PlanningHorizon,
                issues);
        }

        foreach (TransportResource resource
                 in supplyChain.TransportResources)
        {
            string path =
                "supplyChain.transportResources" +
                $"[id={resource.Id}]";

            ValidateTransportResourceDecisionModel(
                resource,
                path,
                supplyChain.PlanningHorizon,
                issues);

            if (resource.Lanes.Count == 0)
            {
                AddError(
                    issues,
                    "TRN001",
                    path + ".lanes",
                    "A transport resource must contain at least " +
                    "one transport lane.");
            }

            var laneKeys =
                new HashSet<string>(StringComparer.Ordinal);

            for (int index = 0;
                 index < resource.Lanes.Count;
                 index++)
            {
                TransportLane lane =
                    resource.Lanes[index];

                string lanePath =
                    path + $".lanes[{index}]";

                ValidateWarehouseReference(
                    supplyChain,
                    lane.Origin,
                    lanePath + ".origin",
                    issues);

                ValidateWarehouseReference(
                    supplyChain,
                    lane.Destination,
                    lanePath + ".destination",
                    issues);

                if (SameWarehouse(
                        lane.Origin,
                        lane.Destination))
                {
                    AddError(
                        issues,
                        "TRN002",
                        lanePath,
                        "The origin and destination warehouses " +
                        "must be different.");
                }

                if (lane.LeadTime < 0)
                {
                    AddError(
                        issues,
                        "TRN003",
                        lanePath + ".leadTime",
                        "A transport lead time cannot be negative.");
                }

                string key =
                    WarehouseKey(lane.Origin) +
                    "->" +
                    WarehouseKey(lane.Destination);

                if (!laneKeys.Add(key))
                {
                    AddError(
                        issues,
                        "TRN004",
                        lanePath,
                        "This transport resource contains the same " +
                        "origin-destination lane more than once.");
                }
            }
        }
    }

    private static void ValidateProductionRoutings(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        var routingIds = new HashSet<int>();

        for (int index = 0;
             index < supplyChain.ProductionRoutings.Count;
             index++)
        {
            ProductionRouting routing =
                supplyChain.ProductionRoutings[index];

            string path =
                $"supplyChain.productionRoutings[{index}]";

            if (routing.Id <= 0)
            {
                AddError(
                    issues,
                    "ROU001",
                    path + ".id",
                    "A production-routing identifier must be " +
                    "strictly positive.");
            }

            if (!routingIds.Add(routing.Id))
            {
                AddError(
                    issues,
                    "ROU002",
                    path + ".id",
                    $"Production-routing identifier {routing.Id} " +
                    "is duplicated.");
            }

            if (!ItemExists(
                    supplyChain,
                    routing.ItemId))
            {
                AddError(
                    issues,
                    "ROU003",
                    path + ".itemId",
                    $"Item {routing.ItemId} does not exist.");
            }

            Plant? plant =
                FindPlant(
                    supplyChain,
                    routing.PlantId);

            if (plant is null)
            {
                AddError(
                    issues,
                    "ROU004",
                    path + ".plantId",
                    $"Plant {routing.PlantId} does not exist.");
            }

            if (routing.LeadTime < 0)
            {
                AddError(
                    issues,
                    "ROU005",
                    path + ".leadTime",
                    "A production lead time cannot be negative.");
            }

            if (routing.WorkCenters.Count == 0)
            {
                AddError(
                    issues,
                    "ROU006",
                    path + ".workCenters",
                    "A production routing must use at least " +
                    "one work center.");
            }

            var workCenterKeys =
                new HashSet<string>(StringComparer.Ordinal);

            foreach (WorkCenterReference reference
                     in routing.WorkCenters)
            {
                string key =
                    WorkCenterKey(reference);

                if (!workCenterKeys.Add(key))
                {
                    AddError(
                        issues,
                        "ROU007",
                        path + ".workCenters",
                        "The same work center is referenced more " +
                        "than once by the routing.");
                }

                if (reference.PlantId != routing.PlantId)
                {
                    AddError(
                        issues,
                        "ROU008",
                        path + ".workCenters",
                        "Every referenced work center must belong " +
                        "to the routing plant.");
                }

                if (ResolveWorkCenter(
                        supplyChain,
                        reference) is null)
                {
                    AddError(
                        issues,
                        "ROU009",
                        path + ".workCenters",
                        $"Work center {key} does not exist.");
                }

                bool characteristicExists =
                    supplyChain.ProductionCharacteristics.Any(
                        characteristic =>
                            characteristic.ItemId ==
                                routing.ItemId &&
                            SameWorkCenter(
                                characteristic.WorkCenter,
                                reference));

                if (!characteristicExists)
                {
                    AddWarning(
                        issues,
                        "ROU010",
                        path + ".workCenters",
                        $"No production characteristic is defined " +
                        $"for item {routing.ItemId} and work " +
                        $"center {key}.");
                }
            }

            ValidatePlanningHorizon(
                path,
                routing.HasLotSizingConstraints,
                routing.PlanningHorizon,
                routing.HasConsistentPlanningHorizon,
                supplyChain.PlanningHorizon,
                issues);
        }
    }

    private static void ValidateProductionCharacteristics(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        var keys =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0;
             index <
                supplyChain.ProductionCharacteristics.Count;
             index++)
        {
            ProductionCharacteristic characteristic =
                supplyChain.ProductionCharacteristics[index];

            string path =
                "supplyChain.productionCharacteristics" +
                $"[{index}]";

            if (!ItemExists(
                    supplyChain,
                    characteristic.ItemId))
            {
                AddError(
                    issues,
                    "PRD001",
                    path + ".itemId",
                    $"Item {characteristic.ItemId} does not exist.");
            }

            if (characteristic.WorkCenter is null)
            {
                AddError(
                    issues,
                    "PRD002",
                    path + ".workCenter",
                    "A production characteristic must reference " +
                    "a work center.");

                continue;
            }

            string key =
                $"{characteristic.ItemId}:" +
                WorkCenterKey(characteristic.WorkCenter);

            if (!keys.Add(key))
            {
                AddError(
                    issues,
                    "PRD003",
                    path,
                    "A production characteristic is duplicated " +
                    "for this item and work center.");
            }

            WorkCenter? workCenter =
                ResolveWorkCenter(
                    supplyChain,
                    characteristic.WorkCenter);

            if (workCenter is null)
            {
                AddError(
                    issues,
                    "PRD004",
                    path + ".workCenter",
                    "The referenced work center does not exist.");
            }
            else if (
                characteristic
                    .RequiresCapacityConstrainedWorkCenter &&
                workCenter.CapacityConstraint is null)
            {
                AddError(
                    issues,
                    "PRD005",
                    path,
                    "Unit-capacity consumption or setup time is " +
                    "defined, but the referenced work center has " +
                    "no capacity constraint.");
            }

            ValidatePlanningHorizon(
                path,
                characteristic.HasDecisionParameters,
                characteristic.PlanningHorizon,
                characteristic.HasConsistentPlanningHorizon,
                supplyChain.PlanningHorizon,
                issues);
        }
    }

    private static void ValidateInventories(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        var keys =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0;
             index < supplyChain.Inventories.Count;
             index++)
        {
            Inventory inventory =
                supplyChain.Inventories[index];

            string path =
                $"supplyChain.inventories[{index}]";

            if (!ItemExists(
                    supplyChain,
                    inventory.ItemId))
            {
                AddError(
                    issues,
                    "INV001",
                    path + ".itemId",
                    $"Item {inventory.ItemId} does not exist.");
            }

            ValidateWarehouseReference(
                supplyChain,
                inventory.Warehouse,
                path + ".warehouse",
                issues);

            string key =
                $"{inventory.ItemId}:" +
                WarehouseKey(inventory.Warehouse);

            if (!keys.Add(key))
            {
                AddError(
                    issues,
                    "INV002",
                    path,
                    "An inventory is duplicated for this item " +
                    "and warehouse.");
            }

            if (!double.IsFinite(inventory.InitialInventory) ||
                inventory.InitialInventory < 0.0)
            {
                AddError(
                    issues,
                    "INV003",
                    path + ".initialInventory",
                    "Initial inventory must be finite and " +
                    "non-negative.");
            }

            if (inventory.InventoryBalanceRule is null)
            {
                AddError(
                    issues,
                    "INV004",
                    path + ".inventoryBalanceRule",
                    "Every inventory must contain an inventory-" +
                    "balance rule.");
            }

            if (!inventory.HasValidDecisionConfiguration)
            {
                AddError(
                    issues,
                    "INV005",
                    path,
                    "The dependencies between the inventory " +
                    "constraints and costs are invalid.");
            }

            Warehouse? warehouse =
                ResolveWarehouse(
                    supplyChain,
                    inventory.Warehouse);

            bool usesCapacity =
                inventory.UnitCapacityConsumption is not null ||
                inventory.SetupTime is not null;

            bool hasApplicableCapacity =
                inventory.CapacityConstraint is not null ||
                warehouse?.CapacityConstraint is not null;

            if (usesCapacity && !hasApplicableCapacity)
            {
                AddError(
                    issues,
                    "INV006",
                    path,
                    "Unit-capacity consumption or setup time is " +
                    "defined, but neither the inventory nor its " +
                    "warehouse has a capacity constraint.");
            }

            ValidatePlanningHorizon(
                path,
                inventory.HasDecisionParameters,
                inventory.PlanningHorizon,
                inventory.HasConsistentPlanningHorizon,
                supplyChain.PlanningHorizon,
                issues);
        }
    }

    private static void ValidateTransportCharacteristics(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        var keys =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0;
             index <
                supplyChain.TransportCharacteristics.Count;
             index++)
        {
            TransportCharacteristic characteristic =
                supplyChain.TransportCharacteristics[index];

            string path =
                "supplyChain.transportCharacteristics" +
                $"[{index}]";

            if (!ItemExists(
                    supplyChain,
                    characteristic.ItemId))
            {
                AddError(
                    issues,
                    "TCH001",
                    path + ".itemId",
                    $"Item {characteristic.ItemId} does not exist.");
            }

            TransportResource? resource =
                FindTransportResource(
                    supplyChain,
                    characteristic.TransportResourceId);

            if (resource is null)
            {
                AddError(
                    issues,
                    "TCH002",
                    path + ".transportResourceId",
                    "The referenced transport resource does not exist.");
            }

            string key =
                $"{characteristic.ItemId}:" +
                $"{characteristic.TransportResourceId}";

            if (!keys.Add(key))
            {
                AddError(
                    issues,
                    "TCH003",
                    path,
                    "A transport characteristic is duplicated " +
                    "for this item and transport resource.");
            }

            if (characteristic.AdditionalCapacity is not null &&
                characteristic.CapacityConstraint is null)
            {
                AddError(
                    issues,
                    "TCH004",
                    path + ".additionalCapacity",
                    "Item-specific additional capacity requires " +
                    "an item-specific capacity constraint.");
            }

            if (characteristic.AdditionalCapacityCost is not null &&
                characteristic.AdditionalCapacity is null)
            {
                AddError(
                    issues,
                    "TCH005",
                    path + ".additionalCapacityCost",
                    "An additional-capacity cost requires an " +
                    "additional-capacity constraint.");
            }

            bool usesCapacity =
                characteristic.UnitCapacityConsumption is not null ||
                characteristic.SetupTime is not null;

            bool hasApplicableCapacity =
                characteristic.CapacityConstraint is not null ||
                resource?.CapacityConstraint is not null;

            if (usesCapacity && !hasApplicableCapacity)
            {
                AddError(
                    issues,
                    "TCH006",
                    path,
                    "Unit-capacity consumption or setup time is " +
                    "defined, but neither the characteristic nor " +
                    "the transport resource has a capacity constraint.");
            }

            ValidatePlanningHorizon(
                path,
                characteristic.HasDecisionParameters,
                characteristic.PlanningHorizon,
                characteristic.HasConsistentPlanningHorizon,
                supplyChain.PlanningHorizon,
                issues);
        }
    }

    private static void ValidateDemands(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        var keys =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0;
             index < supplyChain.Demands.Count;
             index++)
        {
            Demand demand =
                supplyChain.Demands[index];

            string path =
                $"supplyChain.demands[{index}]";

            if (!ItemExists(
                    supplyChain,
                    demand.ItemId))
            {
                AddError(
                    issues,
                    "DEM001",
                    path + ".itemId",
                    $"Item {demand.ItemId} does not exist.");
            }

            if (!DistributionCenterExists(
                    supplyChain,
                    demand.DistributionCenterId))
            {
                AddError(
                    issues,
                    "DEM002",
                    path + ".distributionCenterId",
                    "The referenced distribution center does not exist.");
            }

            string key =
                $"{demand.DistributionCenterId}:" +
                $"{demand.ItemId}";

            if (!keys.Add(key))
            {
                AddError(
                    issues,
                    "DEM003",
                    path,
                    "A demand is duplicated for this distribution " +
                    "center and item.");
            }

            if (!demand.HasValidQuantities)
            {
                AddError(
                    issues,
                    "DEM004",
                    path + ".quantities",
                    "Demand quantities must be finite and non-negative.");
            }

            if (demand.PlanningHorizon !=
                supplyChain.PlanningHorizon)
            {
                AddError(
                    issues,
                    "DEM005",
                    path + ".quantities",
                    "The demand time series does not use the global " +
                    "planning horizon.");
            }
        }
    }

    private static void ValidateDistributionCenterSourcings(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        var keys =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0;
             index <
                supplyChain.DistributionCenterSourcings.Count;
             index++)
        {
            DistributionCenterSourcing sourcing =
                supplyChain.DistributionCenterSourcings[index];

            string path =
                "supplyChain.distributionCenterSourcings" +
                $"[{index}]";

            if (!ItemExists(
                    supplyChain,
                    sourcing.ItemId))
            {
                AddError(
                    issues,
                    "SRC001",
                    path + ".itemId",
                    $"Item {sourcing.ItemId} does not exist.");
            }

            if (!DistributionCenterExists(
                    supplyChain,
                    sourcing.DistributionCenterId))
            {
                AddError(
                    issues,
                    "SRC002",
                    path + ".distributionCenterId",
                    "The referenced distribution center does not exist.");
            }

            ValidateWarehouseReference(
                supplyChain,
                sourcing.Warehouse,
                path + ".warehouse",
                issues);

            string key =
                $"{sourcing.DistributionCenterId}:" +
                $"{sourcing.ItemId}:" +
                WarehouseKey(sourcing.Warehouse);

            if (!keys.Add(key))
            {
                AddError(
                    issues,
                    "SRC003",
                    path,
                    "A sourcing relationship is duplicated.");
            }

            bool inventoryExists =
                supplyChain.Inventories.Any(
                    inventory =>
                        inventory.ItemId ==
                            sourcing.ItemId &&
                        SameWarehouse(
                            inventory.Warehouse,
                            sourcing.Warehouse));

            if (!inventoryExists)
            {
                AddError(
                    issues,
                    "SRC004",
                    path + ".warehouse",
                    "The sourcing relationship references an " +
                    "item-warehouse inventory that does not exist.");
            }

            bool demandExists =
                supplyChain.Demands.Any(
                    demand =>
                        demand.ItemId ==
                            sourcing.ItemId &&
                        demand.DistributionCenterId ==
                            sourcing.DistributionCenterId);

            if (!demandExists)
            {
                AddWarning(
                    issues,
                    "SRC005",
                    path,
                    "No demand is defined for this distribution " +
                    "center and item.");
            }

            if (!sourcing.HasValidDecisionConfiguration)
            {
                AddError(
                    issues,
                    "SRC006",
                    path,
                    "A backlog or shortage cost is defined without " +
                    "the corresponding constraint.");
            }

            ValidatePlanningHorizon(
                path,
                sourcing.HasDecisionParameters,
                sourcing.PlanningHorizon,
                sourcing.HasConsistentPlanningHorizon,
                supplyChain.PlanningHorizon,
                issues);
        }
    }

    private static void ValidateSupplierDeliveries(
        SupplyChain supplyChain,
        ICollection<ValidationIssue> issues)
    {
        var keys =
            new HashSet<string>(StringComparer.Ordinal);

        for (int index = 0;
             index < supplyChain.SupplierDeliveries.Count;
             index++)
        {
            SupplierDelivery delivery =
                supplyChain.SupplierDeliveries[index];

            string path =
                $"supplyChain.supplierDeliveries[{index}]";

            if (!SupplierExists(
                    supplyChain,
                    delivery.SupplierId))
            {
                AddError(
                    issues,
                    "SUP001",
                    path + ".supplierId",
                    "The referenced supplier does not exist.");
            }

            if (!ItemExists(
                    supplyChain,
                    delivery.ItemId))
            {
                AddError(
                    issues,
                    "SUP002",
                    path + ".itemId",
                    $"Item {delivery.ItemId} does not exist.");
            }

            ValidateWarehouseReference(
                supplyChain,
                delivery.Warehouse,
                path + ".warehouse",
                issues);

            string key =
                $"{delivery.SupplierId}:" +
                $"{delivery.ItemId}:" +
                WarehouseKey(delivery.Warehouse);

            if (!keys.Add(key))
            {
                AddError(
                    issues,
                    "SUP003",
                    path,
                    "A supplier-delivery relationship is duplicated.");
            }

            if (delivery.LeadTime < 0)
            {
                AddError(
                    issues,
                    "SUP004",
                    path + ".leadTime",
                    "A supplier-delivery lead time cannot be negative.");
            }

            bool inventoryExists =
                supplyChain.Inventories.Any(
                    inventory =>
                        inventory.ItemId ==
                            delivery.ItemId &&
                        SameWarehouse(
                            inventory.Warehouse,
                            delivery.Warehouse));

            if (!inventoryExists)
            {
                AddError(
                    issues,
                    "SUP005",
                    path + ".warehouse",
                    "The delivery references an item-warehouse " +
                    "inventory that does not exist.");
            }

            if (delivery.HasDecisionParameters &&
                delivery.PlanningHorizon !=
                    supplyChain.PlanningHorizon)
            {
                AddError(
                    issues,
                    "SUP006",
                    path + ".purchasePrice",
                    "The purchase-price time series does not use " +
                    "the global planning horizon.");
            }
        }
    }

    private static void ValidateWarehouseDecisionModel(
        Warehouse warehouse,
        string path,
        int globalHorizon,
        ICollection<ValidationIssue> issues)
    {
        if (!warehouse.HasValidCapacityConfiguration)
        {
            AddError(
                issues,
                "CAP001",
                path,
                "The warehouse additional-capacity configuration " +
                "is invalid.");
        }

        ValidatePlanningHorizon(
            path,
            warehouse.HasDecisionParameters,
            warehouse.PlanningHorizon,
            warehouse.HasConsistentPlanningHorizon,
            globalHorizon,
            issues);
    }

    private static void ValidateWorkCenterDecisionModel(
        WorkCenter workCenter,
        string path,
        int globalHorizon,
        ICollection<ValidationIssue> issues)
    {
        if (!workCenter.HasValidCapacityConfiguration)
        {
            AddError(
                issues,
                "CAP002",
                path,
                "The work-center additional-capacity configuration " +
                "is invalid.");
        }

        ValidatePlanningHorizon(
            path,
            workCenter.HasDecisionParameters,
            workCenter.PlanningHorizon,
            workCenter.HasConsistentPlanningHorizon,
            globalHorizon,
            issues);
    }

    private static void ValidateTransportResourceDecisionModel(
        TransportResource resource,
        string path,
        int globalHorizon,
        ICollection<ValidationIssue> issues)
    {
        if (!resource.HasValidCapacityConfiguration)
        {
            AddError(
                issues,
                "CAP003",
                path,
                "The transport-resource additional-capacity " +
                "configuration is invalid.");
        }

        ValidatePlanningHorizon(
            path,
            resource.HasDecisionParameters,
            resource.PlanningHorizon,
            resource.HasConsistentPlanningHorizon,
            globalHorizon,
            issues);
    }

    private static void ValidatePlanningHorizon(
        string path,
        bool hasActiveParameters,
        int localHorizon,
        bool internallyConsistent,
        int globalHorizon,
        ICollection<ValidationIssue> issues)
    {
        if (!internallyConsistent)
        {
            AddError(
                issues,
                "HOR001",
                path,
                "The active time series do not all use the same " +
                "planning horizon.");
        }

        if (hasActiveParameters &&
            localHorizon != globalHorizon)
        {
            AddError(
                issues,
                "HOR002",
                path,
                $"The local planning horizon ({localHorizon}) " +
                $"does not match the global planning horizon " +
                $"({globalHorizon}).");
        }
    }

    private static void ValidateWarehouseReference(
        SupplyChain supplyChain,
        WarehouseReference? reference,
        string path,
        ICollection<ValidationIssue> issues)
    {
        if (reference is null)
        {
            AddError(
                issues,
                "REF001",
                path,
                "A warehouse reference is required.");

            return;
        }

        if (reference.ReferenceId <= 0)
        {
            AddError(
                issues,
                "REF002",
                path + ".referenceId",
                "A warehouse reference identifier must be " +
                "strictly positive.");
        }

        if (ResolveWarehouse(
                supplyChain,
                reference) is null)
        {
            AddError(
                issues,
                "REF003",
                path,
                $"Warehouse {WarehouseKey(reference)} does not exist.");
        }
    }

    private static void ValidateIdentifiedCollection<T>(
        IEnumerable<T> values,
        Func<T, int> idSelector,
        Func<T, string> nameSelector,
        string path,
        string entityName,
        ICollection<ValidationIssue> issues)
    {
        // Track identifiers to detect duplicates.
        var identifiers = new HashSet<int>();

        int index = 0;

        foreach (T value in values)
        {
            int id = idSelector(value);
            string name = nameSelector(value);

            string entityPath =
                $"{path}[{index}]";

            if (id <= 0)
            {
                AddError(
                    issues,
                    "ID001",
                    entityPath + ".id",
                    $"A {entityName} identifier must be " +
                    "strictly positive.");
            }

            if (!identifiers.Add(id))
            {
                AddError(
                    issues,
                    "ID002",
                    entityPath + ".id",
                    $"{entityName} identifier {id} is duplicated.");
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                AddError(
                    issues,
                    "ID003",
                    entityPath + ".name",
                    $"A {entityName} name is required.");
            }

            index++;
        }
    }

    private static bool ItemExists(
        SupplyChain supplyChain,
        int itemId)
    {
        return supplyChain.Items.Any(
            item => item.Id == itemId);
    }

    private static bool SupplierExists(
        SupplyChain supplyChain,
        int supplierId)
    {
        return supplyChain.Suppliers.Any(
            supplier => supplier.Id == supplierId);
    }

    private static bool DistributionCenterExists(
        SupplyChain supplyChain,
        int distributionCenterId)
    {
        return supplyChain.DistributionCenters.Any(
            center =>
                center.Id == distributionCenterId);
    }

    private static Plant? FindPlant(
        SupplyChain supplyChain,
        int plantId)
    {
        return supplyChain.Plants.FirstOrDefault(
            plant => plant.Id == plantId);
    }

    private static TransportResource? FindTransportResource(
        SupplyChain supplyChain,
        int transportResourceId)
    {
        return supplyChain.TransportResources.FirstOrDefault(
            resource =>
                resource.Id == transportResourceId);
    }

    private static WorkCenter? ResolveWorkCenter(
        SupplyChain supplyChain,
        WorkCenterReference? reference)
    {
        if (reference is null)
        {
            return null;
        }

        Plant? plant =
            FindPlant(
                supplyChain,
                reference.PlantId);

        return plant?.WorkCenters.FirstOrDefault(
            workCenter =>
                workCenter.Id ==
                    reference.WorkCenterId);
    }

    private static Warehouse? ResolveWarehouse(
        SupplyChain supplyChain,
        WarehouseReference? reference)
    {
        if (reference is null)
        {
            return null;
        }

        // Different lookup strategy per warehouse kind.
        return reference.Kind switch
        {
            WarehouseReferenceKind.StandaloneWarehouse =>
                supplyChain.StandaloneWarehouses
                    .FirstOrDefault(
                        warehouse =>
                            warehouse.Id ==
                                reference.ReferenceId),

            WarehouseReferenceKind.PlantWarehouse =>
                supplyChain.Plants
                    .FirstOrDefault(
                        plant =>
                            plant.Id ==
                                reference.ReferenceId)
                    ?.Warehouse,

            _ => null
        };
    }

    private static bool SameWarehouse(
        WarehouseReference? first,
        WarehouseReference? second)
    {
        // Null-safe: both null = equal, only one null = not equal.
        if (first is null || second is null)
        {
            return first is null && second is null;
        }

        return first.Kind == second.Kind &&
               first.ReferenceId == second.ReferenceId;
    }

    private static bool SameWorkCenter(
        WorkCenterReference? first,
        WorkCenterReference? second)
    {
        // Null-safe: both null = equal, only one null = not equal.
        if (first is null || second is null)
        {
            return first is null && second is null;
        }

        return first.PlantId == second.PlantId &&
               first.WorkCenterId == second.WorkCenterId;
    }

    private static string WarehouseKey(
        WarehouseReference? reference)
    {
        return reference is null
            ? "<null>"
            : $"{reference.Kind}:{reference.ReferenceId}";
    }

    private static string WorkCenterKey(
        WorkCenterReference? reference)
    {
        return reference is null
            ? "<null>"
            : $"{reference.PlantId}:" +
              $"{reference.WorkCenterId}";
    }

    private static void AddError(
        ICollection<ValidationIssue> issues,
        string code,
        string path,
        string message)
    {
        issues.Add(
            new ValidationIssue(
                ValidationSeverity.Error,
                code,
                path,
                message));
    }

    private static void AddWarning(
        ICollection<ValidationIssue> issues,
        string code,
        string path,
        string message)
    {
        issues.Add(
            new ValidationIssue(
                ValidationSeverity.Warning,
                code,
                path,
                message));
    }
}