using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Core.PhysicalModel;
using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solution.Decisions;
using LotSizingDataModel.Solution.Evaluation;
using LotSizingDataModel.Solution.Metadata;
using LotSizingDataModel.Solution.Creation;

namespace LotSizingDataModel.Solution;

/// <summary>
/// Represents one candidate solution for a lot-sizing
/// supply-chain instance.
/// </summary>
/// <remarks>
/// The solution stores decision values independently of the
/// method used to generate them.
///
/// A solution may originate from an exact solver, a heuristic,
/// a metaheuristic, a matheuristic, a manual construction
/// or an external file.
/// </remarks>
[Serializable]
[XmlRoot("lotSizingSolution")]
[XmlType(TypeName = "lotSizingSolution")]
public sealed partial class LotSizingSolution :
    ModelObject,
    IPlanningHorizonAware
{
    private Guid _id =
        Guid.NewGuid();

    private string _name =
        string.Empty;

    private string _description =
        string.Empty;

    private string _instanceIdentifier =
        string.Empty;

    private string _instanceFingerprint =
        string.Empty;

    private int _planningHorizon;

    private SolutionCompleteness _completeness =
        SolutionCompleteness.Unknown;

    private SolutionGenerationMetadata _generationMetadata =
        new();

    private SolutionEvaluation _evaluation =
        new();

    /// <summary>
    /// Initializes an empty lot-sizing solution.
    /// </summary>
    /// <remarks>
    /// This constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public LotSizingSolution()
    {
        SubscribeToObject(_generationMetadata);
        SubscribeToObject(_evaluation);
    }

    /// <summary>
    /// Initializes a lot-sizing solution for a planning horizon.
    /// </summary>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public LotSizingSolution(
        int planningHorizon)
        : this()
    {
        if (planningHorizon <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(planningHorizon),
                planningHorizon,
                "The planning horizon must be strictly positive.");
        }

        ResizeTimeSeries(planningHorizon);
    }

    /// <summary>
    /// Initializes a named lot-sizing solution for an instance.
    /// </summary>
    /// <param name="name">
    /// Human-readable solution name.
    /// </param>
    /// <param name="instanceIdentifier">
    /// Identifier of the associated supply-chain instance.
    /// </param>
    /// <param name="planningHorizon">
    /// Strictly positive number of planning periods.
    /// </param>
    public LotSizingSolution(
        string name,
        string instanceIdentifier,
        int planningHorizon)
        : this(planningHorizon)
    {
        Name = name;
        InstanceIdentifier = instanceIdentifier;
    }

    /// <summary>
    /// Creates a zero-initialized solution whose decision
    /// structure matches the specified supply-chain instance.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain instance used to create the solution.
    /// </param>
    /// <param name="instanceIdentifier">
    /// Identifier of the associated supply-chain instance.
    /// </param>
    /// <param name="name">
    /// Optional human-readable solution name.
    /// </param>
    /// <param name="instanceFingerprint">
    /// Optional fingerprint of the associated instance.
    /// </param>
    /// <param name="validateSupplyChain">
    /// True to validate the supply-chain instance before
    /// creating the solution; otherwise, false.
    /// </param>
    /// <returns>
    /// A structurally complete, zero-initialized solution.
    /// </returns>
    public static LotSizingSolution CreateFor(
        SupplyChain supplyChain,
        string instanceIdentifier = "",
        string name = "",
        string instanceFingerprint = "",
        bool validateSupplyChain = true)
    {
        return LotSizingSolutionFactory.Create(
            supplyChain,
            instanceIdentifier,
            name,
            instanceFingerprint,
            validateSupplyChain);
    }


    /// <summary>
    /// Gets or sets the unique identifier of the solution.
    /// </summary>
    [XmlAttribute("id")]
    public Guid Id
    {
        get => _id;
        set => SetProperty(
            ref _id,
            value);
    }

    /// <summary>
    /// Gets or sets the human-readable name of the solution.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get => _name;
        set => SetProperty(
            ref _name,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets an optional human-readable description
    /// of the solution.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
        get => _description;
        set => SetProperty(
            ref _description,
            value ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the identifier of the supply-chain
    /// instance associated with the solution.
    /// </summary>
    /// <remarks>
    /// This identifier may correspond to a file name,
    /// benchmark name, database key or model identifier.
    /// </remarks>
    [XmlAttribute("instanceIdentifier")]
    public string InstanceIdentifier
    {
        get => _instanceIdentifier;
        set => SetProperty(
            ref _instanceIdentifier,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets an optional fingerprint of the associated
    /// supply-chain instance.
    /// </summary>
    /// <remarks>
    /// A fingerprint may be used to detect whether the instance
    /// has changed since the solution was generated.
    ///
    /// The fingerprint-generation mechanism is intentionally
    /// not imposed by this class.
    /// </remarks>
    [XmlAttribute("instanceFingerprint")]
    public string InstanceFingerprint
    {
        get => _instanceFingerprint;
        set => SetProperty(
            ref _instanceFingerprint,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the number of planning periods represented
    /// by the solution.
    /// </summary>
    /// <remarks>
    /// Changing this value resizes every decision time series.
    /// Existing values are preserved whenever possible.
    /// </remarks>
    [XmlAttribute("planningHorizon")]
    public int PlanningHorizon
    {
        get => _planningHorizon;
        set => ResizeTimeSeries(value);
    }

    /// <summary>
    /// Gets or sets whether the solution contains all
    /// decisions expected for the associated instance.
    /// </summary>
    [XmlAttribute("completeness")]
    public SolutionCompleteness Completeness
    {
        get => _completeness;
        set => SetProperty(
            ref _completeness,
            value);
    }

    /// <summary>
    /// Gets or sets the metadata describing how
    /// the solution was generated.
    /// </summary>
    [XmlElement("generationMetadata")]
    public SolutionGenerationMetadata GenerationMetadata
    {
        get => _generationMetadata;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _generationMetadata,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(
                _generationMetadata);

            SetProperty(
                ref _generationMetadata,
                value);

            SubscribeToObject(
                _generationMetadata);
        }
    }

    /// <summary>
    /// Gets or sets the independent evaluation
    /// of the solution.
    /// </summary>
    [XmlElement("evaluation")]
    public SolutionEvaluation Evaluation
    {
        get => _evaluation;
        set
        {
            ArgumentNullException.ThrowIfNull(value);

            if (ReferenceEquals(
                    _evaluation,
                    value))
            {
                return;
            }

            UnsubscribeFromObject(_evaluation);

            SetProperty(
                ref _evaluation,
                value);

            SubscribeToObject(_evaluation);
        }
    }

    /// <summary>
    /// Gets the production decisions contained in the solution.
    /// </summary>
    [XmlArray("productionDecisions")]
    [XmlArrayItem("productionDecision")]
    public List<ProductionDecision> ProductionDecisions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the inventory decisions contained in the solution.
    /// </summary>
    [XmlArray("inventoryDecisions")]
    [XmlArrayItem("inventoryDecision")]
    public List<InventoryDecision> InventoryDecisions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the transport decisions contained in the solution.
    /// </summary>
    [XmlArray("transportDecisions")]
    [XmlArrayItem("transportDecision")]
    public List<TransportDecision> TransportDecisions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the purchase decisions contained in the solution.
    /// </summary>
    [XmlArray("purchaseDecisions")]
    [XmlArrayItem("purchaseDecision")]
    public List<PurchaseDecision> PurchaseDecisions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the distribution decisions contained in the solution.
    /// </summary>
    [XmlArray("distributionDecisions")]
    [XmlArrayItem("distributionDecision")]
    public List<DistributionDecision> DistributionDecisions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the global work-center capacity decisions
    /// contained in the solution.
    /// </summary>
    [XmlArray("workCenterCapacityDecisions")]
    [XmlArrayItem("workCenterCapacityDecision")]
    public List<WorkCenterCapacityDecision>
        WorkCenterCapacityDecisions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the global warehouse-capacity decisions
    /// contained in the solution.
    /// </summary>
    [XmlArray("warehouseCapacityDecisions")]
    [XmlArrayItem("warehouseCapacityDecision")]
    public List<WarehouseCapacityDecision>
        WarehouseCapacityDecisions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the global transport-resource capacity decisions
    /// contained in the solution.
    /// </summary>
    [XmlArray("transportResourceCapacityDecisions")]
    [XmlArrayItem("transportResourceCapacityDecision")]
    public List<TransportResourceCapacityDecision>
        TransportResourceCapacityDecisions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the total number of decision objects
    /// contained in the solution.
    /// </summary>
    [XmlIgnore]
    public int DecisionCount =>
        ProductionDecisions.Count +
        WorkCenterSchedulingDecisions.Count +
        InventoryDecisions.Count +
        TransportDecisions.Count +
        PurchaseDecisions.Count +
        DistributionDecisions.Count +
        WorkCenterCapacityDecisions.Count +
        WarehouseCapacityDecisions.Count +
        TransportResourceCapacityDecisions.Count;

    /// <summary>
    /// Gets a value indicating whether the solution
    /// contains at least one decision object.
    /// </summary>
    [XmlIgnore]
    public bool HasDecisions =>
        DecisionCount > 0;

    /// <summary>
    /// Gets a value indicating whether every decision uses
    /// the same planning horizon as the solution.
    /// </summary>
    [XmlIgnore]
    public bool HasConsistentPlanningHorizon =>
        ProductionDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon) &&

        WorkCenterSchedulingDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon) &&

        InventoryDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon) &&

        TransportDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon) &&

        PurchaseDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon) &&

        DistributionDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon) &&

        WorkCenterCapacityDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon) &&

        WarehouseCapacityDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon) &&

        TransportResourceCapacityDecisions.All(
            decision =>
                decision.PlanningHorizon ==
                    PlanningHorizon);

    /// <summary>
    /// Gets a value indicating whether every decision object
    /// is internally valid.
    /// </summary>
    [XmlIgnore]
    public bool HasInternallyValidDecisions =>
        ProductionDecisions.All(
            decision =>
                decision.IsInternallyValid) &&

        WorkCenterSchedulingDecisions.All(
            decision =>
                decision.IsInternallyValid) &&

        InventoryDecisions.All(
            decision =>
                decision.IsInternallyValid) &&

        TransportDecisions.All(
            decision =>
                decision.IsInternallyValid) &&

        PurchaseDecisions.All(
            decision =>
                decision.IsInternallyValid) &&

        DistributionDecisions.All(
            decision =>
                decision.IsInternallyValid) &&

        WorkCenterCapacityDecisions.All(
            decision =>
                decision.IsInternallyValid) &&

        WarehouseCapacityDecisions.All(
            decision =>
                decision.IsInternallyValid) &&

        TransportResourceCapacityDecisions.All(
            decision =>
                decision.IsInternallyValid);

    /// <summary>
    /// Gets a value indicating whether every decision key
    /// is unique within its decision family.
    /// </summary>
    [XmlIgnore]
    public bool HasUniqueDecisionKeys =>
        ProductionDecisions
            .Select(
                decision =>
                    decision.RoutingId)
            .Distinct()
            .Count() ==
        ProductionDecisions.Count &&

        WorkCenterSchedulingDecisions.Select(decision => (decision.WorkCenter.PlantId, decision.WorkCenter.WorkCenterId)).Distinct().Count() ==
        WorkCenterSchedulingDecisions.Count &&

        InventoryDecisions
            .Select(
                decision =>
                    (
                        decision.ItemId,
                        decision.Warehouse.Kind,
                        decision.Warehouse.ReferenceId
                    ))
            .Distinct()
            .Count() ==
        InventoryDecisions.Count &&

        TransportDecisions
            .Select(
                decision =>
                    (
                        decision.ItemId,
                        decision.TransportResourceId,
                        decision.Origin.Kind,
                        decision.Origin.ReferenceId,
                        decision.Destination.Kind,
                        decision.Destination.ReferenceId
                    ))
            .Distinct()
            .Count() ==
        TransportDecisions.Count &&

        PurchaseDecisions
            .Select(
                decision =>
                    (
                        decision.SupplierId,
                        decision.ItemId,
                        decision.DestinationWarehouse.Kind,
                        decision.DestinationWarehouse.ReferenceId
                    ))
            .Distinct()
            .Count() ==
        PurchaseDecisions.Count &&

        DistributionDecisions
            .Select(
                decision =>
                    (
                        decision.DistributionCenterId,
                        decision.ItemId,
                        decision.Warehouse.Kind,
                        decision.Warehouse.ReferenceId
                    ))
            .Distinct()
            .Count() ==
        DistributionDecisions.Count &&

        WorkCenterCapacityDecisions
            .Select(
                decision =>
                    (
                        decision.WorkCenter.PlantId,
                        decision.WorkCenter.WorkCenterId
                    ))
            .Distinct()
            .Count() ==
        WorkCenterCapacityDecisions.Count &&

        WarehouseCapacityDecisions
            .Select(
                decision =>
                    (
                        decision.Warehouse.Kind,
                        decision.Warehouse.ReferenceId
                    ))
            .Distinct()
            .Count() ==
        WarehouseCapacityDecisions.Count &&

        TransportResourceCapacityDecisions
            .Select(
                decision =>
                    decision.TransportResourceId)
            .Distinct()
            .Count() ==
        TransportResourceCapacityDecisions.Count;

    /// <summary>
    /// Gets a value indicating whether the solution
    /// is internally consistent.
    /// </summary>
    /// <remarks>
    /// This property does not verify the solution against
    /// a particular supply-chain instance.
    ///
    /// Instance references, capacities, material balances,
    /// demand satisfaction and other model constraints will
    /// be checked by the solution validator.
    /// </remarks>
    [XmlIgnore]
    public bool IsInternallyValid =>
        Id != Guid.Empty &&
        PlanningHorizon > 0 &&
        HasDecisions &&
        HasConsistentPlanningHorizon &&
        HasInternallyValidDecisions &&
        HasUniqueDecisionKeys;

    /// <summary>
    /// Adds a production decision to the solution.
    /// </summary>
    /// <param name="decision">
    /// Production decision to add.
    /// </param>
    public void AddProductionDecision(
        ProductionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
        {
            throw new ArgumentException(
                "The production decision is not " +
                "internally valid.",
                nameof(decision));
        }

        if (ProductionDecisions.Any(
                existing =>
                    existing.RoutingId ==
                        decision.RoutingId))
        {
            throw new InvalidOperationException(
                $"A production decision already exists " +
                $"for routing {decision.RoutingId}.");
        }

        ProductionDecisions.Add(decision);
        SubscribeToObject(decision);

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Adds an inventory decision to the solution.
    /// </summary>
    /// <param name="decision">
    /// Inventory decision to add.
    /// </param>
    public void AddInventoryDecision(
        InventoryDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
        {
            throw new ArgumentException(
                "The inventory decision is not " +
                "internally valid.",
                nameof(decision));
        }

        if (InventoryDecisions.Any(
                existing =>
                    existing.ItemId ==
                        decision.ItemId &&
                    SameWarehouse(
                        existing.Warehouse,
                        decision.Warehouse)))
        {
            throw new InvalidOperationException(
                "An inventory decision already exists " +
                "for this item and warehouse.");
        }

        InventoryDecisions.Add(decision);
        SubscribeToObject(decision);

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Adds a transport decision to the solution.
    /// </summary>
    /// <param name="decision">
    /// Transport decision to add.
    /// </param>
    public void AddTransportDecision(
        TransportDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
        {
            throw new ArgumentException(
                "The transport decision is not " +
                "internally valid.",
                nameof(decision));
        }

        if (TransportDecisions.Any(
                existing =>
                    existing.Matches(
                        decision.ItemId,
                        decision.TransportResourceId,
                        decision.Origin,
                        decision.Destination)))
        {
            throw new InvalidOperationException(
                "A transport decision already exists " +
                "for this item, resource and lane.");
        }

        TransportDecisions.Add(decision);
        SubscribeToObject(decision);

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Adds a purchase decision to the solution.
    /// </summary>
    /// <param name="decision">
    /// Purchase decision to add.
    /// </param>
    public void AddPurchaseDecision(
        PurchaseDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
        {
            throw new ArgumentException(
                "The purchase decision is not " +
                "internally valid.",
                nameof(decision));
        }

        if (PurchaseDecisions.Any(
                existing =>
                    existing.Matches(
                        decision.SupplierId,
                        decision.ItemId,
                        decision.DestinationWarehouse)))
        {
            throw new InvalidOperationException(
                "A purchase decision already exists " +
                "for this supplier, item and warehouse.");
        }

        PurchaseDecisions.Add(decision);
        SubscribeToObject(decision);

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Adds a distribution decision to the solution.
    /// </summary>
    /// <param name="decision">
    /// Distribution decision to add.
    /// </param>
    public void AddDistributionDecision(
        DistributionDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
        {
            throw new ArgumentException(
                "The distribution decision is not " +
                "internally valid.",
                nameof(decision));
        }

        if (DistributionDecisions.Any(
                existing =>
                    existing.Matches(
                        decision.DistributionCenterId,
                        decision.ItemId,
                        decision.Warehouse)))
        {
            throw new InvalidOperationException(
                "A distribution decision already exists " +
                "for this distribution center, item " +
                "and warehouse.");
        }

        DistributionDecisions.Add(decision);
        SubscribeToObject(decision);

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Adds a work-center capacity decision to the solution.
    /// </summary>
    /// <param name="decision">
    /// Work-center capacity decision to add.
    /// </param>
    public void AddWorkCenterCapacityDecision(
        WorkCenterCapacityDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
        {
            throw new ArgumentException(
                "The work-center capacity decision is not " +
                "internally valid.",
                nameof(decision));
        }

        if (WorkCenterCapacityDecisions.Any(
                existing =>
                    existing.Matches(
                        decision.WorkCenter)))
        {
            throw new InvalidOperationException(
                "A capacity decision already exists " +
                "for this work center.");
        }

        WorkCenterCapacityDecisions.Add(decision);
        SubscribeToObject(decision);

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Adds a warehouse-capacity decision to the solution.
    /// </summary>
    /// <param name="decision">
    /// Warehouse-capacity decision to add.
    /// </param>
    public void AddWarehouseCapacityDecision(
        WarehouseCapacityDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
        {
            throw new ArgumentException(
                "The warehouse-capacity decision is not " +
                "internally valid.",
                nameof(decision));
        }

        if (WarehouseCapacityDecisions.Any(
                existing =>
                    existing.Matches(
                        decision.Warehouse)))
        {
            throw new InvalidOperationException(
                "A capacity decision already exists " +
                "for this warehouse.");
        }

        WarehouseCapacityDecisions.Add(decision);
        SubscribeToObject(decision);

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Adds a transport-resource capacity decision
    /// to the solution.
    /// </summary>
    /// <param name="decision">
    /// Transport-resource capacity decision to add.
    /// </param>
    public void AddTransportResourceCapacityDecision(
        TransportResourceCapacityDecision decision)
    {
        ArgumentNullException.ThrowIfNull(decision);

        PrepareDecision(
            decision.PlanningHorizon,
            decision.ResizeTimeSeries,
            nameof(decision));

        if (!decision.IsInternallyValid)
        {
            throw new ArgumentException(
                "The transport-resource capacity decision " +
                "is not internally valid.",
                nameof(decision));
        }

        if (TransportResourceCapacityDecisions.Any(
                existing =>
                    existing.Matches(
                        decision.TransportResourceId)))
        {
            throw new InvalidOperationException(
                "A capacity decision already exists " +
                "for this transport resource.");
        }

        TransportResourceCapacityDecisions.Add(decision);
        SubscribeToObject(decision);

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Resizes every decision time series to the specified
    /// planning horizon.
    /// </summary>
    /// <param name="periodCount">
    /// Non-negative number of planning periods.
    /// </param>
    /// <remarks>
    /// Existing values are preserved whenever possible.
    /// New periods are initialized with zero.
    /// </remarks>
    public void ResizeTimeSeries(int periodCount)
    {
        if (periodCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(periodCount),
                periodCount,
                "The period count cannot be negative.");
        }

        SetProperty(
            ref _planningHorizon,
            periodCount,
            nameof(PlanningHorizon));

        foreach (ProductionDecision decision
                 in ProductionDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        foreach (WorkCenterSchedulingDecision decision
                 in WorkCenterSchedulingDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        foreach (InventoryDecision decision
                 in InventoryDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        foreach (TransportDecision decision
                 in TransportDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        foreach (PurchaseDecision decision
                 in PurchaseDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        foreach (DistributionDecision decision
                 in DistributionDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        foreach (WorkCenterCapacityDecision decision
                 in WorkCenterCapacityDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        foreach (WarehouseCapacityDecision decision
                 in WarehouseCapacityDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        foreach (TransportResourceCapacityDecision decision
                 in TransportResourceCapacityDecisions)
        {
            decision.ResizeTimeSeries(periodCount);
        }

        NotifyDecisionProperties();
    }

    /// <summary>
    /// Removes every decision object from the solution.
    /// </summary>
    public void ClearDecisions()
    {
        UnsubscribeFromObjects(
            ProductionDecisions);

        UnsubscribeFromObjects(
            WorkCenterSchedulingDecisions);

        UnsubscribeFromObjects(
            InventoryDecisions);

        UnsubscribeFromObjects(
            TransportDecisions);

        UnsubscribeFromObjects(
            PurchaseDecisions);

        UnsubscribeFromObjects(
            DistributionDecisions);

        UnsubscribeFromObjects(
            WorkCenterCapacityDecisions);

        UnsubscribeFromObjects(
            WarehouseCapacityDecisions);

        UnsubscribeFromObjects(
            TransportResourceCapacityDecisions);

        ProductionDecisions.Clear();
        WorkCenterSchedulingDecisions.Clear();
        InventoryDecisions.Clear();
        TransportDecisions.Clear();
        PurchaseDecisions.Clear();
        DistributionDecisions.Clear();
        WorkCenterCapacityDecisions.Clear();
        WarehouseCapacityDecisions.Clear();
        TransportResourceCapacityDecisions.Clear();

        Completeness =
            SolutionCompleteness.Unknown;

        NotifyDecisionProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string solutionName =
            string.IsNullOrWhiteSpace(Name)
                ? Id.ToString()
                : Name;

        return
            $"{solutionName}: " +
            $"{DecisionCount} decisions; " +
            $"horizon {PlanningHorizon}; " +
            $"{Completeness}";
    }

    internal void ReconnectNestedNotifications()
    {
        ReconnectObject(_generationMetadata);
        ReconnectObject(_evaluation);

        ReconnectObjects(
            ProductionDecisions);

        ReconnectObjects(
            WorkCenterSchedulingDecisions);

        ReconnectObjects(
            InventoryDecisions);

        ReconnectObjects(
            TransportDecisions);

        ReconnectObjects(
            PurchaseDecisions);

        ReconnectObjects(
            DistributionDecisions);

        ReconnectObjects(
            WorkCenterCapacityDecisions);

        ReconnectObjects(
            WarehouseCapacityDecisions);

        ReconnectObjects(
            TransportResourceCapacityDecisions);

        NotifyDecisionProperties();
    }

    private void PrepareDecision(
        int decisionPlanningHorizon,
        Action<int> resizeAction,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(resizeAction);

        if (PlanningHorizon == 0)
        {
            if (decisionPlanningHorizon <= 0)
            {
                throw new ArgumentException(
                    "Neither the solution nor the decision " +
                    "defines a valid planning horizon.",
                    parameterName);
            }

            ResizeTimeSeries(
                decisionPlanningHorizon);

            return;
        }

        if (decisionPlanningHorizon == 0)
        {
            resizeAction(PlanningHorizon);

            return;
        }

        if (decisionPlanningHorizon !=
            PlanningHorizon)
        {
            throw new ArgumentException(
                $"The decision planning horizon " +
                $"({decisionPlanningHorizon}) differs from " +
                $"the solution planning horizon " +
                $"({PlanningHorizon}).",
                parameterName);
        }
    }

    private void SubscribeToObject(
        ModelObject modelObject)
    {
        modelObject.PropertyChanged +=
            OnNestedPropertyChanged;
    }

    private void UnsubscribeFromObject(
        ModelObject modelObject)
    {
        modelObject.PropertyChanged -=
            OnNestedPropertyChanged;
    }

    private void ReconnectObject(
        ModelObject modelObject)
    {
        UnsubscribeFromObject(modelObject);
        SubscribeToObject(modelObject);
    }

    private void ReconnectObjects<T>(
        IEnumerable<T> modelObjects)
        where T : ModelObject
    {
        foreach (T modelObject in modelObjects)
        {
            ReconnectObject(modelObject);
        }
    }

    private void UnsubscribeFromObjects<T>(
        IEnumerable<T> modelObjects)
        where T : ModelObject
    {
        foreach (T modelObject in modelObjects)
        {
            UnsubscribeFromObject(modelObject);
        }
    }

    private void OnNestedPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        NotifyDecisionProperties();
    }

    private void NotifyDecisionProperties()
    {
        OnPropertyChanged(
            nameof(DecisionCount));

        OnPropertyChanged(
            nameof(HasDecisions));

        OnPropertyChanged(
            nameof(HasConsistentPlanningHorizon));

        OnPropertyChanged(
            nameof(HasInternallyValidDecisions));

        OnPropertyChanged(
            nameof(HasUniqueDecisionKeys));

        OnPropertyChanged(
            nameof(IsInternallyValid));
    }

    private static bool SameWarehouse(
    WarehouseReference first,
    WarehouseReference second)
    {
        return first.Kind ==
                   second.Kind &&
               first.ReferenceId ==
                   second.ReferenceId;
    }
}