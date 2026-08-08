using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Describes the result of building a solver-independent
/// mathematical model from a lot-sizing instance.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalModelBuildResult")]
public sealed class MathematicalModelBuildResult
{
    private readonly List<string> _diagnostics =
        new();

    /// <summary>
    /// Initializes an empty mathematical-model build result.
    /// </summary>
    public MathematicalModelBuildResult()
    {
        RequestedFormulationId =
            string.Empty;

        SelectedFormulationId =
            string.Empty;

        SelectedFormulationName =
            string.Empty;

        FailureMessage =
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
    /// Gets or sets a value indicating whether model
    /// construction succeeded.
    /// </summary>
    [XmlAttribute("isSuccessful")]
    public bool IsSuccessful
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a value indicating whether formulation
    /// fallback was used.
    /// </summary>
    [XmlAttribute("usedFallback")]
    public bool UsedFallback
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the model-construction duration.
    /// </summary>
    [XmlIgnore]
    public TimeSpan BuildDuration
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the model-construction duration in
    /// milliseconds for XML serialization.
    /// </summary>
    [XmlAttribute("buildDurationMilliseconds")]
    public double BuildDurationMilliseconds
    {
        get =>
            BuildDuration.TotalMilliseconds;

        set =>
            BuildDuration =
                TimeSpan.FromMilliseconds(
                    value);
    }

    /// <summary>
    /// Gets or sets the failure message.
    /// </summary>
    [XmlElement("failureMessage")]
    public string FailureMessage
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the diagnostic messages produced during selection
    /// and model construction.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets or sets the generated mathematical model.
    /// </summary>
    [XmlElement("model")]
    public MathematicalModel? Model
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
        string? diagnostic)
    {
        if (!string.IsNullOrWhiteSpace(
                diagnostic))
        {
            _diagnostics.Add(
                diagnostic.Trim());
        }
    }

    /// <summary>
    /// Adds several diagnostic messages.
    /// </summary>
    /// <param name="diagnostics">
    /// Diagnostic messages.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="diagnostics"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void AddDiagnostics(
        IEnumerable<string> diagnostics)
    {
        ArgumentNullException.ThrowIfNull(
            diagnostics);

        foreach (
            string diagnostic
            in diagnostics)
        {
            AddDiagnostic(
                diagnostic);
        }
    }

    /// <summary>
    /// Creates a successful mathematical-model build result.
    /// </summary>
    /// <param name="selection">
    /// Formulation-selection result.
    /// </param>
    /// <param name="model">
    /// Generated mathematical model.
    /// </param>
    /// <param name="buildDuration">
    /// Model-construction duration.
    /// </param>
    /// <returns>
    /// Successful build result.
    /// </returns>
    public static MathematicalModelBuildResult Success(
        MathematicalModelFormulationSelectionResult selection,
        MathematicalModel model,
        TimeSpan buildDuration)
    {
        ArgumentNullException.ThrowIfNull(
            selection);

        ArgumentNullException.ThrowIfNull(
            model);

        var result =
            new MathematicalModelBuildResult
            {
                RequestedFormulationId =
                    selection.RequestedFormulationId,

                SelectedFormulationId =
                    selection.SelectedFormulationId,

                SelectedFormulationName =
                    selection.SelectedFormulationName,

                IsSuccessful =
                    true,

                UsedFallback =
                    selection.UsedFallback,

                BuildDuration =
                    buildDuration,

                Model =
                    model
            };

        result.AddDiagnostics(
            selection.Diagnostics);

        return result;
    }

    /// <summary>
    /// Creates a failed mathematical-model build result.
    /// </summary>
    /// <param name="selection">
    /// Optional formulation-selection result.
    /// </param>
    /// <param name="failureMessage">
    /// Failure message.
    /// </param>
    /// <param name="buildDuration">
    /// Elapsed model-construction duration.
    /// </param>
    /// <returns>
    /// Failed build result.
    /// </returns>
    public static MathematicalModelBuildResult Failure(
        MathematicalModelFormulationSelectionResult? selection,
        string failureMessage,
        TimeSpan buildDuration)
    {
        var result =
            new MathematicalModelBuildResult
            {
                RequestedFormulationId =
                    selection?.RequestedFormulationId ??
                    string.Empty,

                SelectedFormulationId =
                    selection?.SelectedFormulationId ??
                    string.Empty,

                SelectedFormulationName =
                    selection?.SelectedFormulationName ??
                    string.Empty,

                IsSuccessful =
                    false,

                UsedFallback =
                    selection?.UsedFallback ??
                    false,

                BuildDuration =
                    buildDuration,

                FailureMessage =
                    failureMessage?.Trim() ??
                    string.Empty
            };

        if (selection is not null)
        {
            result.AddDiagnostics(
                selection.Diagnostics);
        }

        result.AddDiagnostic(
            failureMessage);

        return result;
    }
}
