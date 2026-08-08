using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Defines the options used when mapping a mathematical solver
/// result back to a normalized lot-sizing solution.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalSolutionMappingOptions")]
public sealed class MathematicalSolutionMappingOptions
{
    /// <summary>
    /// Initializes default mathematical-solution mapping
    /// options.
    /// </summary>
    public MathematicalSolutionMappingOptions()
    {
        ZeroTolerance =
            1.0e-9;

        IncludeZeroValues =
            true;

        RequireKnownCategories =
            true;

        RequireCompleteVariableValues =
            false;
    }

    /// <summary>
    /// Gets or sets the absolute tolerance below which a solver
    /// value is considered equal to zero.
    /// </summary>
    [XmlAttribute("zeroTolerance")]
    public double ZeroTolerance
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether solver values
    /// considered equal to zero must also be mapped.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="true"/> so that a
    /// normalized solution explicitly represents modeled decision
    /// families even when all their values are zero. This allows
    /// callers to distinguish a modeled zero-valued decision from
    /// a decision family that was not part of the mathematical
    /// model.
    /// </remarks>
    [XmlAttribute("includeZeroValues")]
    public bool IncludeZeroValues
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether every mathematical
    /// domain-key category must have a registered decision mapper.
    /// </summary>
    [XmlAttribute("requireKnownCategories")]
    public bool RequireKnownCategories
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether every mathematical
    /// variable must have a corresponding solver value.
    /// </summary>
    /// <remarks>
    /// This option is normally disabled because some native
    /// adapters may omit fixed or unused zero-valued variables
    /// from their normalized result.
    /// </remarks>
    [XmlAttribute("requireCompleteVariableValues")]
    public bool RequireCompleteVariableValues
    {
        get;
        set;
    }

    /// <summary>
    /// Validates the mapping options.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the zero tolerance is invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (double.IsNaN(
                ZeroTolerance) ||
            double.IsInfinity(
                ZeroTolerance) ||
            ZeroTolerance < 0.0)
        {
            throw new InvalidOperationException(
                "The mathematical-solution mapping zero " +
                "tolerance must be finite and non-negative.");
        }
    }

    /// <summary>
    /// Creates an independent copy of the mapping options.
    /// </summary>
    /// <returns>
    /// Cloned mathematical-solution mapping options.
    /// </returns>
    public MathematicalSolutionMappingOptions Clone()
    {
        return new MathematicalSolutionMappingOptions
        {
            ZeroTolerance =
                ZeroTolerance,

            IncludeZeroValues =
                IncludeZeroValues,

            RequireKnownCategories =
                RequireKnownCategories,

            RequireCompleteVariableValues =
                RequireCompleteVariableValues
        };
    }
}
