using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Describes the result of selecting a mathematical formulation
/// for a lot-sizing instance.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalModelFormulationSelectionResult")]
public sealed class MathematicalModelFormulationSelectionResult
{
    private readonly List<string> _diagnostics =
        new();

    /// <summary>
    /// Initializes an empty formulation-selection result.
    /// </summary>
    public MathematicalModelFormulationSelectionResult()
    {
        RequestedFormulationId =
            string.Empty;

        SelectedFormulationId =
            string.Empty;

        SelectedFormulationName =
            string.Empty;
    }

    /// <summary>
    /// Gets or sets the requested formulation identifier.
    /// </summary>
    [XmlAttribute("requestedFormulationId")]
    public string RequestedFormulationId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the selected formulation identifier.
    /// </summary>
    [XmlAttribute("selectedFormulationId")]
    public string SelectedFormulationId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the selected formulation name.
    /// </summary>
    [XmlAttribute("selectedFormulationName")]
    public string SelectedFormulationName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether a compatible
    /// formulation was selected.
    /// </summary>
    [XmlAttribute("isSuccessful")]
    public bool IsSuccessful
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the selected
    /// formulation differs from the requested formulation.
    /// </summary>
    [XmlAttribute("usedFallback")]
    public bool UsedFallback
    {
        get;
        set;
    }

    /// <summary>
    /// Gets diagnostic messages produced during formulation
    /// selection.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets or sets the selected formulation for the current
    /// process.
    /// </summary>
    /// <remarks>
    /// Interface instances are intentionally excluded from XML
    /// serialization. The formulation can be resolved again from
    /// <see cref="SelectedFormulationId"/>.
    /// </remarks>
    [XmlIgnore]
    public IMathematicalModelFormulation? Formulation
    {
        get;
        set;
    }

    /// <summary>
    /// Adds a non-empty diagnostic message.
    /// </summary>
    /// <param name="diagnostic">
    /// Diagnostic message.
    /// </param>
    public void AddDiagnostic(
        string diagnostic)
    {
        if (!string.IsNullOrWhiteSpace(
                diagnostic))
        {
            _diagnostics.Add(
                diagnostic.Trim());
        }
    }

    /// <summary>
    /// Creates a successful formulation-selection result.
    /// </summary>
    /// <param name="requestedFormulationId">
    /// Requested formulation identifier.
    /// </param>
    /// <param name="formulation">
    /// Selected formulation.
    /// </param>
    /// <param name="usedFallback">
    /// Indicates whether fallback selection was used.
    /// </param>
    /// <returns>
    /// Successful selection result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="formulation"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static MathematicalModelFormulationSelectionResult
        Success(
            string requestedFormulationId,
            IMathematicalModelFormulation formulation,
            bool usedFallback)
    {
        ArgumentNullException.ThrowIfNull(
            formulation);

        return new MathematicalModelFormulationSelectionResult
        {
            RequestedFormulationId =
                requestedFormulationId?.Trim() ??
                string.Empty,

            SelectedFormulationId =
                formulation.FormulationId,

            SelectedFormulationName =
                formulation.Name,

            IsSuccessful =
                true,

            UsedFallback =
                usedFallback,

            Formulation =
                formulation
        };
    }

    /// <summary>
    /// Creates a failed formulation-selection result.
    /// </summary>
    /// <param name="requestedFormulationId">
    /// Requested formulation identifier.
    /// </param>
    /// <param name="diagnostic">
    /// Failure diagnostic.
    /// </param>
    /// <returns>
    /// Failed selection result.
    /// </returns>
    public static MathematicalModelFormulationSelectionResult
        Failure(
            string requestedFormulationId,
            string diagnostic)
    {
        var result =
            new MathematicalModelFormulationSelectionResult
            {
                RequestedFormulationId =
                    requestedFormulationId?.Trim() ??
                    string.Empty,

                IsSuccessful =
                    false
            };

        result.AddDiagnostic(
            diagnostic);

        return result;
    }
}
