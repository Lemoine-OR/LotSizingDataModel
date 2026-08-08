using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Modeling;

/// <summary>
/// Represents a linear expression composed of variable terms
/// and a constant value.
/// </summary>
[Serializable]
[XmlType(TypeName = "linearExpression")]
public sealed class LinearExpression
{
    private readonly List<LinearTerm> _terms =
        new();

    /// <summary>
    /// Initializes an empty linear expression.
    /// </summary>
    public LinearExpression()
    {
    }

    /// <summary>
    /// Initializes a linear expression with a constant value.
    /// </summary>
    /// <param name="constant">
    /// Constant value of the expression.
    /// </param>
    public LinearExpression(
        double constant)
    {
        Constant =
            constant;

        EnsureFiniteValue(
            constant,
            nameof(constant));
    }

    /// <summary>
    /// Gets the linear terms of the expression.
    /// </summary>
    [XmlArray("terms")]
    [XmlArrayItem("term")]
    public List<LinearTerm> Terms =>
        _terms;

    /// <summary>
    /// Gets or sets the constant value of the expression.
    /// </summary>
    [XmlAttribute("constant")]
    public double Constant
    {
        get;
        set;
    }

    /// <summary>
    /// Gets a value indicating whether the expression contains
    /// no variable term.
    /// </summary>
    [XmlIgnore]
    public bool IsConstant =>
        _terms.Count == 0;

    /// <summary>
    /// Gets the number of variable terms.
    /// </summary>
    [XmlIgnore]
    public int TermCount =>
        _terms.Count;

    /// <summary>
    /// Adds a linear term to the expression.
    /// </summary>
    /// <param name="variableId">
    /// Identifier of the referenced mathematical variable.
    /// </param>
    /// <param name="coefficient">
    /// Linear coefficient.
    /// </param>
    /// <remarks>
    /// If the expression already contains a term for the same
    /// variable, both coefficients are combined.
    /// </remarks>
    public void AddTerm(
        int variableId,
        double coefficient)
    {
        var term =
            new LinearTerm(
                variableId,
                coefficient);

        AddTerm(
            term);
    }

    /// <summary>
    /// Adds a linear term to the expression.
    /// </summary>
    /// <param name="term">
    /// Term to add.
    /// </param>
    /// <remarks>
    /// If the expression already contains a term for the same
    /// variable, both coefficients are combined.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="term"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void AddTerm(
        LinearTerm term)
    {
        ArgumentNullException.ThrowIfNull(
            term);

        term.EnsureValid();

        LinearTerm? existingTerm =
            _terms.FirstOrDefault(
                currentTerm =>
                    currentTerm.VariableId ==
                    term.VariableId);

        if (existingTerm is null)
        {
            if (term.Coefficient != 0.0)
            {
                _terms.Add(
                    new LinearTerm(
                        term.VariableId,
                        term.Coefficient));
            }

            return;
        }

        double combinedCoefficient =
            existingTerm.Coefficient +
            term.Coefficient;

        EnsureFiniteValue(
            combinedCoefficient,
            nameof(term));

        if (combinedCoefficient == 0.0)
        {
            _terms.Remove(
                existingTerm);

            return;
        }

        existingTerm.Coefficient =
            combinedCoefficient;
    }

    /// <summary>
    /// Adds another linear expression to this expression.
    /// </summary>
    /// <param name="expression">
    /// Expression to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="expression"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void Add(
        LinearExpression expression)
    {
        ArgumentNullException.ThrowIfNull(
            expression);

        foreach (
            LinearTerm term
            in expression.Terms)
        {
            AddTerm(
                term);
        }

        AddConstant(
            expression.Constant);
    }

    /// <summary>
    /// Adds a constant value to the expression.
    /// </summary>
    /// <param name="value">
    /// Constant value to add.
    /// </param>
    public void AddConstant(
        double value)
    {
        EnsureFiniteValue(
            value,
            nameof(value));

        double updatedConstant =
            Constant +
            value;

        EnsureFiniteValue(
            updatedConstant,
            nameof(value));

        Constant =
            updatedConstant;
    }

    /// <summary>
    /// Multiplies the entire expression by a scalar.
    /// </summary>
    /// <param name="factor">
    /// Scalar multiplier.
    /// </param>
    public void MultiplyBy(
        double factor)
    {
        EnsureFiniteValue(
            factor,
            nameof(factor));

        Constant *=
            factor;

        foreach (
            LinearTerm term
            in _terms)
        {
            term.Coefficient *=
                factor;
        }

        _terms.RemoveAll(
            term =>
                term.Coefficient == 0.0);
    }

    /// <summary>
    /// Removes all terms and resets the constant to zero.
    /// </summary>
    public void Clear()
    {
        _terms.Clear();

        Constant =
            0.0;
    }

    /// <summary>
    /// Creates an independent copy of this expression.
    /// </summary>
    /// <returns>
    /// Cloned linear expression.
    /// </returns>
    public LinearExpression Clone()
    {
        var clone =
            new LinearExpression(
                Constant);

        foreach (
            LinearTerm term
            in _terms)
        {
            clone.Terms.Add(
                new LinearTerm(
                    term.VariableId,
                    term.Coefficient));
        }

        return clone;
    }

    /// <summary>
    /// Validates the linear expression.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the expression contains an invalid value or
    /// duplicate variable reference.
    /// </exception>
    public void EnsureValid()
    {
        EnsureFiniteValue(
            Constant,
            nameof(Constant));

        var variableIds =
            new HashSet<int>();

        foreach (
            LinearTerm term
            in _terms)
        {
            if (term is null)
            {
                throw new InvalidOperationException(
                    "A linear expression cannot contain a null " +
                    "term.");
            }

            term.EnsureValid();

            if (!variableIds.Add(
                    term.VariableId))
            {
                throw new InvalidOperationException(
                    $"Variable identifier '{term.VariableId}' " +
                    "appears more than once in the linear " +
                    "expression.");
            }
        }
    }

    private static void EnsureFiniteValue(
        double value,
        string valueName)
    {
        if (double.IsNaN(
                value) ||
            double.IsInfinity(
                value))
        {
            throw new InvalidOperationException(
                $"{valueName} must be a finite number.");
        }
    }
}
