using System;
using System.Collections.Generic;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Building;

/// <summary>
/// Builds a corrected inventory-balance constraint for one
/// item, location, and period.
/// </summary>
/// <remarks>
/// The generated balance follows the convention:
/// <code>
/// I(t) - B(t) + S(t)
/// = I(t-1) - B(t-1) + S(t-1)
///   + inflows - outflows - demand.
/// </code>
/// After moving all variable terms to the left-hand side, the
/// generated constraint is:
/// <code>
/// I(t) - B(t) + S(t)
/// - I(t-1) + B(t-1) - S(t-1)
/// - inflows + outflows = -demand.
/// </code>
/// </remarks>
public sealed class InventoryBalanceConstraintBuilder
{
    private readonly List<LinearTerm> _inflows =
        new();

    private readonly List<LinearTerm> _outflows =
        new();

    private MathematicalVariable? _currentInventory;

    private MathematicalVariable? _currentBacklog;

    private MathematicalVariable? _currentShortage;

    private MathematicalVariable? _previousInventory;

    private MathematicalVariable? _previousBacklog;

    private MathematicalVariable? _previousShortage;

    /// <summary>
    /// Gets or sets the external demand for the period.
    /// </summary>
    public double Demand
    {
        get;
        set;
    }

    /// <summary>
    /// Assigns the current-period inventory state variables.
    /// </summary>
    /// <param name="inventory">
    /// Current-period inventory variable.
    /// </param>
    /// <param name="backlog">
    /// Optional current-period backlog variable.
    /// </param>
    /// <param name="shortage">
    /// Optional current-period shortage variable.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="inventory"/> is
    /// <see langword="null"/>.
    /// </exception>
    public InventoryBalanceConstraintBuilder SetCurrentState(
        MathematicalVariable inventory,
        MathematicalVariable? backlog = null,
        MathematicalVariable? shortage = null)
    {
        ArgumentNullException.ThrowIfNull(
            inventory);

        inventory.EnsureValid();
        backlog?.EnsureValid();
        shortage?.EnsureValid();

        _currentInventory =
            inventory;

        _currentBacklog =
            backlog;

        _currentShortage =
            shortage;

        return this;
    }

    /// <summary>
    /// Assigns the previous-period inventory state variables.
    /// </summary>
    /// <param name="inventory">
    /// Optional previous-period inventory variable.
    /// </param>
    /// <param name="backlog">
    /// Optional previous-period backlog variable.
    /// </param>
    /// <param name="shortage">
    /// Optional previous-period shortage variable.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public InventoryBalanceConstraintBuilder SetPreviousState(
        MathematicalVariable? inventory,
        MathematicalVariable? backlog = null,
        MathematicalVariable? shortage = null)
    {
        inventory?.EnsureValid();
        backlog?.EnsureValid();
        shortage?.EnsureValid();

        _previousInventory =
            inventory;

        _previousBacklog =
            backlog;

        _previousShortage =
            shortage;

        return this;
    }

    /// <summary>
    /// Adds an inflow variable to the balance.
    /// </summary>
    /// <param name="variable">
    /// Inflow variable.
    /// </param>
    /// <param name="coefficient">
    /// Positive conversion or quantity coefficient.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public InventoryBalanceConstraintBuilder AddInflow(
        MathematicalVariable variable,
        double coefficient = 1.0)
    {
        AddFlowTerm(
            _inflows,
            variable,
            coefficient,
            nameof(coefficient));

        return this;
    }

    /// <summary>
    /// Adds an outflow variable to the balance.
    /// </summary>
    /// <param name="variable">
    /// Outflow variable.
    /// </param>
    /// <param name="coefficient">
    /// Positive conversion or quantity coefficient.
    /// </param>
    /// <returns>
    /// Current builder.
    /// </returns>
    public InventoryBalanceConstraintBuilder AddOutflow(
        MathematicalVariable variable,
        double coefficient = 1.0)
    {
        AddFlowTerm(
            _outflows,
            variable,
            coefficient,
            nameof(coefficient));

        return this;
    }

    /// <summary>
    /// Builds the corrected inventory-balance expression.
    /// </summary>
    /// <returns>
    /// Left-hand-side expression of the corrected balance.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the current inventory variable is missing or
    /// the demand is invalid.
    /// </exception>
    public LinearExpression BuildExpression()
    {
        EnsureValid();

        var expression =
            new LinearExpression();

        expression.AddTerm(
            _currentInventory!.Id,
            1.0);

        if (_currentBacklog is not null)
        {
            expression.AddTerm(
                _currentBacklog.Id,
                -1.0);
        }

        if (_currentShortage is not null)
        {
            expression.AddTerm(
                _currentShortage.Id,
                1.0);
        }

        if (_previousInventory is not null)
        {
            expression.AddTerm(
                _previousInventory.Id,
                -1.0);
        }

        if (_previousBacklog is not null)
        {
            expression.AddTerm(
                _previousBacklog.Id,
                1.0);
        }

        if (_previousShortage is not null)
        {
            expression.AddTerm(
                _previousShortage.Id,
                -1.0);
        }

        foreach (
            LinearTerm inflow
            in _inflows)
        {
            expression.AddTerm(
                inflow.VariableId,
                -inflow.Coefficient);
        }

        foreach (
            LinearTerm outflow
            in _outflows)
        {
            expression.AddTerm(
                outflow.VariableId,
                outflow.Coefficient);
        }

        expression.EnsureValid();

        return expression;
    }

    /// <summary>
    /// Adds the corrected inventory-balance constraint to a
    /// mathematical model.
    /// </summary>
    /// <param name="modelBuilder">
    /// Mathematical model builder.
    /// </param>
    /// <param name="name">
    /// Constraint name.
    /// </param>
    /// <param name="domainKey">
    /// Optional business-domain key.
    /// </param>
    /// <param name="description">
    /// Optional description.
    /// </param>
    /// <returns>
    /// Created inventory-balance constraint.
    /// </returns>
    public LinearConstraint AddToModel(
        MathematicalModelBuilder modelBuilder,
        string name,
        string domainKey = "",
        string description = "")
    {
        ArgumentNullException.ThrowIfNull(
            modelBuilder);

        return modelBuilder.AddConstraint(
            name,
            BuildExpression(),
            MathematicalConstraintSense.Equal,
            -Demand,
            domainKey,
            description);
    }

    /// <summary>
    /// Validates the inventory-balance definition.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the definition is incomplete or invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (_currentInventory is null)
        {
            throw new InvalidOperationException(
                "A current-period inventory variable is " +
                "required.");
        }

        if (double.IsNaN(
                Demand) ||
            double.IsInfinity(
                Demand))
        {
            throw new InvalidOperationException(
                "Inventory-balance demand must be finite.");
        }

        foreach (
            LinearTerm term
            in _inflows)
        {
            term.EnsureValid();
        }

        foreach (
            LinearTerm term
            in _outflows)
        {
            term.EnsureValid();
        }
    }

    private static void AddFlowTerm(
        ICollection<LinearTerm> target,
        MathematicalVariable variable,
        double coefficient,
        string coefficientName)
    {
        ArgumentNullException.ThrowIfNull(
            variable);

        variable.EnsureValid();

        if (double.IsNaN(
                coefficient) ||
            double.IsInfinity(
                coefficient) ||
            coefficient <= 0.0)
        {
            throw new ArgumentOutOfRangeException(
                coefficientName,
                coefficient,
                "A flow coefficient must be finite and " +
                "strictly positive.");
        }

        target.Add(
            new LinearTerm(
                variable.Id,
                coefficient));
    }
}
