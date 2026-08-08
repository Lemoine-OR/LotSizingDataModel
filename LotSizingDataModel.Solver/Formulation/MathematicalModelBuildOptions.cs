using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Defines the options used to select a formulation and build a
/// solver-independent mathematical model.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalModelBuildOptions")]
public sealed class MathematicalModelBuildOptions
{
    /// <summary>
    /// Initializes default mathematical-model build options.
    /// </summary>
    public MathematicalModelBuildOptions()
    {
        RequestedFormulationId =
            string.Empty;

        AllowFallback =
            true;

        ValidateGeneratedModel =
            true;

        CloneGeneratedModel =
            false;
    }

    /// <summary>
    /// Gets or sets the requested formulation identifier.
    /// </summary>
    /// <remarks>
    /// An empty value enables automatic formulation selection.
    /// </remarks>
    [XmlAttribute("requestedFormulationId")]
    public string RequestedFormulationId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether another
    /// compatible formulation may be selected when the requested
    /// formulation is unavailable or incompatible.
    /// </summary>
    [XmlAttribute("allowFallback")]
    public bool AllowFallback
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the generated
    /// mathematical model must be validated before it is
    /// returned.
    /// </summary>
    [XmlAttribute("validateGeneratedModel")]
    public bool ValidateGeneratedModel
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether an independent
    /// clone of the generated model must be returned.
    /// </summary>
    /// <remarks>
    /// Cloning can protect a cached formulation model from later
    /// modifications, but it requires additional memory and
    /// processing time.
    /// </remarks>
    [XmlAttribute("cloneGeneratedModel")]
    public bool CloneGeneratedModel
    {
        get;
        set;
    }

    /// <summary>
    /// Validates and normalizes the build options.
    /// </summary>
    public void EnsureValid()
    {
        RequestedFormulationId =
            RequestedFormulationId?.Trim() ??
            string.Empty;
    }

    /// <summary>
    /// Creates an independent copy of the build options.
    /// </summary>
    /// <returns>
    /// Cloned mathematical-model build options.
    /// </returns>
    public MathematicalModelBuildOptions Clone()
    {
        return new MathematicalModelBuildOptions
        {
            RequestedFormulationId =
                RequestedFormulationId,

            AllowFallback =
                AllowFallback,

            ValidateGeneratedModel =
                ValidateGeneratedModel,

            CloneGeneratedModel =
                CloneGeneratedModel
        };
    }
}
