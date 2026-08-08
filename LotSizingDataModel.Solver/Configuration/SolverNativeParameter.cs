using System;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Configuration;

/// <summary>
/// Represents a solver-specific parameter that is passed to a
/// solver adapter without being interpreted by the generic
/// solver layer.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverNativeParameter")]
public sealed class SolverNativeParameter
{
    /// <summary>
    /// Initializes an empty native solver parameter.
    /// </summary>
    public SolverNativeParameter()
    {
        Name =
            string.Empty;

        Value =
            string.Empty;
    }

    /// <summary>
    /// Initializes a native solver parameter.
    /// </summary>
    /// <param name="name">
    /// Native parameter name.
    /// </param>
    /// <param name="value">
    /// Native parameter value represented using invariant text.
    /// </param>
    /// <param name="solverKind">
    /// Solver to which the parameter applies.
    /// </param>
    public SolverNativeParameter(
        string name,
        string value,
        SolverKind solverKind = SolverKind.Unknown)
        : this()
    {
        Name =
            name;

        Value =
            value;

        SolverKind =
            solverKind;

        EnsureValid();
    }

    /// <summary>
    /// Gets or sets the solver to which the parameter applies.
    /// </summary>
    /// <remarks>
    /// <see cref="Common.SolverKind.Unknown"/> means that the
    /// selected adapter decides whether the parameter applies.
    /// </remarks>
    [XmlAttribute("solverKind")]
    public SolverKind SolverKind
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the native solver parameter name.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the native solver parameter value.
    /// </summary>
    [XmlAttribute("value")]
    public string Value
    {
        get;
        set;
    }

    /// <summary>
    /// Validates the native solver parameter.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the parameter name or value is empty.
    /// </exception>
    public void EnsureValid()
    {
        if (string.IsNullOrWhiteSpace(
                Name))
        {
            throw new InvalidOperationException(
                "A native solver parameter name is required.");
        }

        if (string.IsNullOrWhiteSpace(
                Value))
        {
            throw new InvalidOperationException(
                $"A value is required for native solver " +
                $"parameter '{Name}'.");
        }
    }
}
