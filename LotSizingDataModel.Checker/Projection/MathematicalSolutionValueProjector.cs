using LotSizingDataModel.Checker.Contracts;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Checker.Projection;

/// <summary>
/// Projects a normalized <see cref="LotSizingSolution"/> onto the
/// variables of a <see cref="MathematicalModel"/> by using the
/// canonical mathematical domain-key convention.
/// </summary>
/// <remarks>
/// <para>
/// This is intentionally the inverse bridge of the Solver mapping
/// layer. The mathematical formulation remains the single source
/// of truth for variable meaning.
/// </para>
/// <para>
/// The projector does not test feasibility. It only resolves the
/// candidate value associated with each mathematical variable.
/// Constraint and objective checking are implemented by later
/// checker stages.
/// </para>
/// </remarks>
public sealed class MathematicalSolutionValueProjector :
    IMathematicalSolutionValueProjector
{
    /// <inheritdoc/>
    public MathematicalSolutionProjectionResult Project(
        MathematicalModel model,
        LotSizingSolution solution)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(solution);

        model.EnsureValid();

        var result =
            new MathematicalSolutionProjectionResult();

        foreach (
            MathematicalVariable variable
            in model.Variables)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(
                        variable.DomainKey))
                {
                    throw new InvalidOperationException(
                        "The mathematical variable has no domain key.");
                }

                MathematicalDomainKey domainKey =
                    MathematicalDomainKey.Parse(
                        variable.DomainKey);

                double value =
                    ResolveValue(
                        solution,
                        domainKey);

                result.AddValue(
                    variable.Id,
                    value);
            }
            catch (Exception exception)
            {
                result.AddIssue(
                    new MathematicalSolutionProjectionIssue
                    {
                        VariableId =
                            variable.Id,
                        VariableName =
                            variable.Name,
                        DomainKey =
                            variable.DomainKey,
                        Message =
                            exception.Message
                    });
            }
        }

        return result;
    }

    private static double ResolveValue(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        return key.Category switch
        {
            MathematicalDecisionCategory.Production =>
                ResolveProduction(
                    solution,
                    key,
                    production =>
                        production.GetQuantity(
                            GetPeriod(key))),

            MathematicalDecisionCategory.Setup =>
                ResolveProduction(
                    solution,
                    key,
                    production =>
                        production.IsSetupActivated(
                            GetPeriod(key))
                            ? 1.0
                            : 0.0),

            MathematicalDecisionCategory.AuxiliaryLotSizeMultiplier =>
                ResolveProduction(
                    solution,
                    key,
                    production =>
                        production.GetLotMultipleCount(
                            GetPeriod(key))),

            MathematicalDecisionCategory
                .AuxiliarySmallBucketProductionActivation =>
                    ResolveProduction(
                        solution,
                        key,
                        production =>
                            Math.Abs(
                                production.GetQuantity(
                                    GetPeriod(key))) > 1e-9
                                ? 1.0
                                : 0.0),

            MathematicalDecisionCategory
                .AuxiliarySchedulingSetupStart =>
                    ResolveProduction(
                        solution,
                        key,
                        production =>
                            IsSchedulingSetupStart(
                                production,
                                key)
                                ? 1.0
                                : 0.0),

            MathematicalDecisionCategory.MicroPeriodProduction =>
                ResolveMicroProduction(solution, key),

            MathematicalDecisionCategory.MicroPeriodSetupState =>
                ResolveMicroSetupState(solution, key),

            MathematicalDecisionCategory.AuxiliaryMicroPeriodChangeover =>
                ResolveMicroChangeover(solution, key),

            MathematicalDecisionCategory.AuxiliaryMicroPeriodSetupStart =>
                ResolveMicroSetupStart(solution, key),

            MathematicalDecisionCategory.AuxiliaryMacroProductionActivation =>
                ResolveProduction(solution, key, production => Math.Abs(production.GetQuantity(GetPeriod(key))) > 1e-9 ? 1.0 : 0.0),

            MathematicalDecisionCategory.AuxiliaryProductionStartUp =>
                ResolveProductionStartUp(solution, key),

            MathematicalDecisionCategory.InitialInventory =>
                ResolveInitialInventory(solution, key),

            MathematicalDecisionCategory.Inventory =>
                ResolveInventory(
                    solution,
                    key,
                    inventory =>
                        inventory.GetLevel(
                            GetPeriod(key))),

            MathematicalDecisionCategory.InventorySetup =>
                ResolveInventory(
                    solution,
                    key,
                    inventory =>
                        inventory.IsSetupActivated(
                            GetPeriod(key))
                            ? 1.0
                            : 0.0),

            MathematicalDecisionCategory.InventorySafetyStockViolation =>
                ResolveInventory(
                    solution,
                    key,
                    inventory =>
                        inventory.GetSafetyStockViolation(
                            GetPeriod(key))),

            MathematicalDecisionCategory.InventoryAdditionalCapacity =>
                ResolveInventory(
                    solution,
                    key,
                    inventory =>
                        inventory.GetAdditionalCapacityUsed(
                            GetPeriod(key))),

            MathematicalDecisionCategory.Delivery =>
                ResolveDistribution(
                    solution,
                    key,
                    decision =>
                        decision.GetDeliveredQuantity(
                            GetPeriod(key))),

            MathematicalDecisionCategory.Backlog =>
                ResolveDistribution(
                    solution,
                    key,
                    decision =>
                        decision.GetBacklogLevel(
                            GetPeriod(key))),

            MathematicalDecisionCategory.Shortage =>
                ResolveDistribution(
                    solution,
                    key,
                    decision =>
                        decision.GetShortageQuantity(
                            GetPeriod(key))),

            MathematicalDecisionCategory.Transport =>
                ResolveTransport(
                    solution,
                    key,
                    decision =>
                        decision.GetTransportedQuantity(
                            GetPeriod(key))),

            MathematicalDecisionCategory.TransportSetup =>
                ResolveTransport(
                    solution,
                    key,
                    decision =>
                        decision.IsSetupActivated(
                            GetPeriod(key))
                            ? 1.0
                            : 0.0),

            MathematicalDecisionCategory.TransportAdditionalCapacity =>
                ResolveTransport(
                    solution,
                    key,
                    decision =>
                        decision.GetAdditionalCapacityUsed(
                            GetPeriod(key))),

            MathematicalDecisionCategory.Procurement =>
                ResolveProcurement(
                    solution,
                    key),

            MathematicalDecisionCategory.WorkCenterActivation =>
                ResolveWorkCenterCapacity(
                    solution,
                    key,
                    decision =>
                        decision.IsActivated(
                            GetPeriod(key))
                            ? 1.0
                            : 0.0),

            MathematicalDecisionCategory.WorkCenterAdditionalCapacity =>
                ResolveWorkCenterCapacity(
                    solution,
                    key,
                    decision =>
                        decision.GetAdditionalCapacityUsed(
                            GetPeriod(key))),

            MathematicalDecisionCategory.WarehouseActivation =>
                ResolveWarehouseCapacity(
                    solution,
                    key,
                    decision =>
                        decision.IsActivated(
                            GetPeriod(key))
                            ? 1.0
                            : 0.0),

            MathematicalDecisionCategory.WarehouseAdditionalCapacity =>
                ResolveWarehouseCapacity(
                    solution,
                    key,
                    decision =>
                        decision.GetAdditionalCapacityUsed(
                            GetPeriod(key))),

            MathematicalDecisionCategory.TransportResourceActivation =>
                ResolveTransportResourceCapacity(
                    solution,
                    key,
                    decision =>
                        decision.IsActivated(
                            GetPeriod(key))
                            ? 1.0
                            : 0.0),

            MathematicalDecisionCategory.TransportResourceAdditionalCapacity =>
                ResolveTransportResourceCapacity(
                    solution,
                    key,
                    decision =>
                        decision.GetAdditionalCapacityUsed(
                            GetPeriod(key))),

            MathematicalDecisionCategory.Subcontracting =>
                throw new NotSupportedException(
                    "The current normalized LotSizingSolution model " +
                    "does not expose a subcontracting decision family."),

            _ =>
                throw new NotSupportedException(
                    $"Mathematical decision category " +
                    $"'{key.Category}' is not supported by the " +
                    "solution value projector.")
        };
    }

    private static double ResolveMicroProduction(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        int routingId = key.GetRequiredInt32(MathematicalDomainKeySegment.Routing);
        ProductionMicroPeriodDecision decision = ResolveMicroDecision(solution, key);
        return decision.RoutingId == routingId ? decision.Quantity : 0.0;
    }

    private static double ResolveMicroSetupState(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        int itemId = key.GetRequiredInt32(MathematicalDomainKeySegment.Item);
        ProductionMicroPeriodDecision decision = ResolveMicroDecision(solution, key);
        return decision.SetupItemId == itemId ? 1.0 : 0.0;
    }

    private static double ResolveMicroChangeover(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        int fromItemId = key.GetRequiredInt32(MathematicalDomainKeySegment.FromItem);
        int toItemId = key.GetRequiredInt32(MathematicalDomainKeySegment.ToItem);
        int plantId = key.GetRequiredInt32(MathematicalDomainKeySegment.Plant);
        int workCenterId = key.GetRequiredInt32(MathematicalDomainKeySegment.WorkCenter);
        int period = key.GetRequiredInt32(MathematicalDomainKeySegment.Period);
        int micro = key.GetRequiredInt32(MathematicalDomainKeySegment.MicroPeriod);
        WorkCenterSchedulingDecision schedule = solution.WorkCenterSchedulingDecisions.SingleOrDefault(candidate => candidate.WorkCenter.PlantId == plantId && candidate.WorkCenter.WorkCenterId == workCenterId) ?? throw new InvalidOperationException("No candidate macro/micro schedule matches the mathematical domain key.");
        ProductionMicroPeriodDecision[] ordered = schedule.MicroPeriods.OrderBy(candidate => candidate.MicroPeriod.MacroPeriod).ThenBy(candidate => candidate.MicroPeriod.MicroPeriodIndex).ToArray();
        int currentIndex = Array.FindIndex(ordered, candidate => candidate.MicroPeriod.MacroPeriod == period && candidate.MicroPeriod.MicroPeriodIndex == micro);
        if (currentIndex < 0) { throw new InvalidOperationException("No candidate micro-period decision matches the changeover domain key."); }
        if (currentIndex == 0) { return ordered[0].SetupItemId == toItemId ? 1.0 : 0.0; }
        return ordered[currentIndex - 1].SetupItemId == fromItemId && ordered[currentIndex].SetupItemId == toItemId ? 1.0 : 0.0;
    }

    private static double ResolveInitialInventory(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        int itemId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        bool hasWarehouse =
            key.TryGetInt32(
                MathematicalDomainKeySegment.Warehouse,
                out int warehouseId);

        bool hasPlant =
            key.TryGetInt32(
                MathematicalDomainKeySegment.Plant,
                out int plantId);

        if (hasWarehouse == hasPlant)
        {
            throw new InvalidOperationException(
                "An initial-inventory key must identify exactly one warehouse.");
        }

        var decision =
            solution.InventoryDecisions.SingleOrDefault(
                candidate =>
                    candidate.ItemId == itemId &&
                    (hasWarehouse
                        ? candidate.Warehouse.Kind ==
                            WarehouseReferenceKind.StandaloneWarehouse &&
                          candidate.Warehouse.ReferenceId == warehouseId
                        : candidate.Warehouse.Kind ==
                            WarehouseReferenceKind.PlantWarehouse &&
                          candidate.Warehouse.ReferenceId == plantId));

        return decision?.InitialInventoryLevel ?? 0.0;
    }
    private static double ResolveProductionStartUp(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        if (key.TryGetInt32(MathematicalDomainKeySegment.MicroPeriod, out _))
        {
            return ResolveMicroSetupStart(solution, key);
        }

        return ResolveProduction(
            solution,
            key,
            production =>
                IsSchedulingSetupStart(production, key)
                    ? 1.0
                    : 0.0);
    }

    private static double ResolveMicroSetupStart(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        int itemId = key.GetRequiredInt32(MathematicalDomainKeySegment.Item);
        int plantId = key.GetRequiredInt32(MathematicalDomainKeySegment.Plant);
        int workCenterId = key.GetRequiredInt32(MathematicalDomainKeySegment.WorkCenter);
        int period = key.GetRequiredInt32(MathematicalDomainKeySegment.Period);
        int micro = key.GetRequiredInt32(MathematicalDomainKeySegment.MicroPeriod);
        WorkCenterSchedulingDecision schedule = solution.WorkCenterSchedulingDecisions.SingleOrDefault(candidate => candidate.WorkCenter.PlantId == plantId && candidate.WorkCenter.WorkCenterId == workCenterId) ?? throw new InvalidOperationException("No candidate macro/micro schedule matches the mathematical domain key.");
        ProductionMicroPeriodDecision[] ordered = schedule.MicroPeriods.OrderBy(candidate => candidate.MicroPeriod.MacroPeriod).ThenBy(candidate => candidate.MicroPeriod.MicroPeriodIndex).ToArray();
        int currentIndex = Array.FindIndex(ordered, candidate => candidate.MicroPeriod.MacroPeriod == period && candidate.MicroPeriod.MicroPeriodIndex == micro);
        if (currentIndex < 0) { throw new InvalidOperationException("No candidate micro-period decision matches the setup-start domain key."); }
        if (ordered[currentIndex].SetupItemId != itemId) { return 0.0; }
        if (key.TryGetInt32(MathematicalDomainKeySegment.SetupReset, out int reset) && reset != 0) { return 1.0; }
        if (key.TryGetInt32(MathematicalDomainKeySegment.FromItem, out int fromItem)) { return itemId == fromItem ? 0.0 : 1.0; }
        if (currentIndex == 0) { return 1.0; }
        return ordered[currentIndex - 1].SetupItemId == itemId ? 0.0 : 1.0;
    }

    private static ProductionMicroPeriodDecision ResolveMicroDecision(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        int plantId = key.GetRequiredInt32(MathematicalDomainKeySegment.Plant);
        int workCenterId = key.GetRequiredInt32(MathematicalDomainKeySegment.WorkCenter);
        int period = key.GetRequiredInt32(MathematicalDomainKeySegment.Period);
        int micro = key.GetRequiredInt32(MathematicalDomainKeySegment.MicroPeriod);
        WorkCenterSchedulingDecision schedule = solution.WorkCenterSchedulingDecisions.SingleOrDefault(candidate => candidate.WorkCenter.PlantId == plantId && candidate.WorkCenter.WorkCenterId == workCenterId) ?? throw new InvalidOperationException("No candidate macro/micro schedule matches the mathematical domain key.");
        return schedule.MicroPeriods.SingleOrDefault(candidate => candidate.MicroPeriod.MacroPeriod == period && candidate.MicroPeriod.MicroPeriodIndex == micro) ?? throw new InvalidOperationException("No candidate micro-period decision matches the mathematical domain key.");
    }

    private static bool IsSchedulingSetupStart(
        ProductionDecision production,
        MathematicalDomainKey key)
    {
        int period = GetPeriod(key);
        if (!production.IsSetupActivated(period)) { return false; }
        if (key.TryGetInt32(MathematicalDomainKeySegment.SetupReset, out int reset) && reset != 0) { return true; }
        if (period == 1 && key.TryGetInt32(MathematicalDomainKeySegment.FromItem, out int fromItem) && key.TryGetInt32(MathematicalDomainKeySegment.Item, out int itemId)) { return itemId != fromItem; }
        return period == 1 || !production.IsSetupActivated(period - 1);
    }

    private static double ResolveProduction(
        LotSizingSolution solution,
        MathematicalDomainKey key,
        Func<ProductionDecision, double> selector)
    {
        int routingId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.Routing);

        ProductionDecision decision =
            solution.ProductionDecisions
                .SingleOrDefault(
                    candidate =>
                        candidate.RoutingId ==
                            routingId)
            ?? throw MissingDecision(
                key,
                $"production routing {routingId}");

        return selector(decision);
    }

    private static double ResolveInventory(
        LotSizingSolution solution,
        MathematicalDomainKey key,
        Func<InventoryDecision, double> selector)
    {
        int itemId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        WarehouseReference warehouse =
            ResolveWarehouse(
                key,
                MathematicalDomainKeySegment.Warehouse,
                MathematicalDomainKeySegment.Plant,
                "inventory");

        InventoryDecision decision =
            solution.InventoryDecisions
                .SingleOrDefault(
                    candidate =>
                        candidate.ItemId ==
                            itemId &&
                        SameWarehouse(
                            candidate.Warehouse,
                            warehouse))
            ?? throw MissingDecision(
                key,
                $"inventory item {itemId}");

        return selector(decision);
    }

    private static double ResolveDistribution(
        LotSizingSolution solution,
        MathematicalDomainKey key,
        Func<DistributionDecision, double> selector)
    {
        int distributionCenterId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.DistributionCenter);

        int itemId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        WarehouseReference warehouse =
            ResolveWarehouse(
                key,
                MathematicalDomainKeySegment.Warehouse,
                MathematicalDomainKeySegment.Plant,
                "distribution source");

        DistributionDecision decision =
            solution.DistributionDecisions
                .SingleOrDefault(
                    candidate =>
                        candidate.Matches(
                            distributionCenterId,
                            itemId,
                            warehouse))
            ?? throw MissingDecision(
                key,
                $"distribution center {distributionCenterId}, " +
                $"item {itemId}");

        return selector(decision);
    }

    private static double ResolveTransport(
        LotSizingSolution solution,
        MathematicalDomainKey key,
        Func<TransportDecision, double> selector)
    {
        int itemId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        int transportResourceId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.TransportResource);

        WarehouseReference origin =
            ResolveWarehouse(
                key,
                MathematicalDomainKeySegment.OriginWarehouse,
                MathematicalDomainKeySegment.OriginPlant,
                "transport origin");

        WarehouseReference destination =
            ResolveWarehouse(
                key,
                MathematicalDomainKeySegment.DestinationWarehouse,
                MathematicalDomainKeySegment.DestinationPlant,
                "transport destination");

        TransportDecision decision =
            solution.TransportDecisions
                .SingleOrDefault(
                    candidate =>
                        candidate.Matches(
                            itemId,
                            transportResourceId,
                            origin,
                            destination))
            ?? throw MissingDecision(
                key,
                $"transport item {itemId}, resource " +
                $"{transportResourceId}");

        return selector(decision);
    }

    private static double ResolveProcurement(
        LotSizingSolution solution,
        MathematicalDomainKey key)
    {
        int supplierId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.Supplier);

        int itemId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.Item);

        WarehouseReference destination =
            ResolveWarehouse(
                key,
                MathematicalDomainKeySegment.DestinationWarehouse,
                MathematicalDomainKeySegment.DestinationPlant,
                "procurement destination");

        PurchaseDecision decision =
            solution.PurchaseDecisions
                .SingleOrDefault(
                    candidate =>
                        candidate.Matches(
                            supplierId,
                            itemId,
                            destination))
            ?? throw MissingDecision(
                key,
                $"supplier {supplierId}, item {itemId}");

        return decision.GetPurchasedQuantity(
            GetPeriod(key));
    }

    private static double ResolveWorkCenterCapacity(
        LotSizingSolution solution,
        MathematicalDomainKey key,
        Func<WorkCenterCapacityDecision, double> selector)
    {
        int plantId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.Plant);

        int workCenterId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.WorkCenter);

        WorkCenterCapacityDecision decision =
            solution.WorkCenterCapacityDecisions
                .SingleOrDefault(
                    candidate =>
                        candidate.WorkCenter.PlantId ==
                            plantId &&
                        candidate.WorkCenter.WorkCenterId ==
                            workCenterId)
            ?? throw MissingDecision(
                key,
                $"work center {plantId}/{workCenterId}");

        return selector(decision);
    }

    private static double ResolveWarehouseCapacity(
        LotSizingSolution solution,
        MathematicalDomainKey key,
        Func<WarehouseCapacityDecision, double> selector)
    {
        WarehouseReference warehouse =
            ResolveWarehouse(
                key,
                MathematicalDomainKeySegment.Warehouse,
                MathematicalDomainKeySegment.Plant,
                "warehouse capacity");

        WarehouseCapacityDecision decision =
            solution.WarehouseCapacityDecisions
                .SingleOrDefault(
                    candidate =>
                        candidate.Matches(
                            warehouse))
            ?? throw MissingDecision(
                key,
                "warehouse capacity");

        return selector(decision);
    }

    private static double ResolveTransportResourceCapacity(
        LotSizingSolution solution,
        MathematicalDomainKey key,
        Func<TransportResourceCapacityDecision, double> selector)
    {
        int transportResourceId =
            key.GetRequiredInt32(
                MathematicalDomainKeySegment.TransportResource);

        TransportResourceCapacityDecision decision =
            solution.TransportResourceCapacityDecisions
                .SingleOrDefault(
                    candidate =>
                        candidate.Matches(
                            transportResourceId))
            ?? throw MissingDecision(
                key,
                $"transport resource {transportResourceId}");

        return selector(decision);
    }

    private static int GetPeriod(
        MathematicalDomainKey key)
    {
        return key.GetRequiredInt32(
            MathematicalDomainKeySegment.Period);
    }

    private static WarehouseReference ResolveWarehouse(
        MathematicalDomainKey key,
        string standaloneSegment,
        string plantSegment,
        string role)
    {
        bool hasStandalone =
            key.TryGetInt32(
                standaloneSegment,
                out int warehouseId);

        bool hasPlant =
            key.TryGetInt32(
                plantSegment,
                out int plantId);

        if (hasStandalone == hasPlant)
        {
            throw new InvalidOperationException(
                $"The domain key must identify exactly one {role} " +
                $"warehouse using either '{standaloneSegment}' or " +
                $"'{plantSegment}'.");
        }

        return hasStandalone
            ? WarehouseReference.ForStandaloneWarehouse(
                warehouseId)
            : WarehouseReference.ForPlantWarehouse(
                plantId);
    }

    private static bool SameWarehouse(
        WarehouseReference left,
        WarehouseReference right)
    {
        return left.Kind ==
                   right.Kind &&
               left.ReferenceId ==
                   right.ReferenceId;
    }

    private static InvalidOperationException MissingDecision(
        MathematicalDomainKey key,
        string decisionDescription)
    {
        return new InvalidOperationException(
            $"No candidate {decisionDescription} decision matches " +
            $"mathematical domain key '{FormatKey(key)}'.");
    }

    private static string FormatKey(
        MathematicalDomainKey key)
    {
        return string.Join(
            "|",
            new[]
            {
                key.Category
            }.Concat(
                key.Segments.Select(
                    pair =>
                        $"{pair.Key}={pair.Value}")));
    }
}
