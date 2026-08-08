using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Contracts;
using LotSizingDataModel.Solver.Modeling;
using LotSizingDataModel.Solver.Configuration;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Describes a request to solve an already built,
/// solver-independent mathematical model.
/// </summary>
/// <remarks>
/// This request is used after formulation selection and model
/// construction. Solver adapters receive the same mathematical
/// model and are therefore responsible only for translating and
/// solving it with their native optimization engine.
/// </remarks>
[Serializable]
[XmlType(TypeName = "mathematicalModelSolveRequest")]
public sealed class MathematicalModelSolveRequest
{
    private readonly List<ISolverProgressObserver> _progressObservers =
        new();

    /// <summary>
    /// Initializes an empty mathematical-model solve request.
    /// </summary>
    public MathematicalModelSolveRequest()
    {
        RunName =
            string.Empty;

        FormulationId =
            string.Empty;

        Parameters =
            new SolverParameters();
    }

    /// <summary>
    /// Gets or sets the mathematical model to solve.
    /// </summary>
    [XmlIgnore]
    public MathematicalModel? Model
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets an optional run name.
    /// </summary>
    [XmlAttribute("runName")]
    public string RunName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the identifier of the formulation that
    /// generated the mathematical model.
    /// </summary>
    [XmlAttribute("formulationId")]
    public string FormulationId
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the generic solver parameters.
    /// </summary>
    [XmlElement("parameters")]
    public SolverParameters Parameters
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the progress observers attached to the solve run.
    /// </summary>
    /// <remarks>
    /// Runtime observer objects are intentionally excluded from
    /// XML serialization.
    /// </remarks>
    [XmlIgnore]
    public List<ISolverProgressObserver> ProgressObservers =>
        _progressObservers;

    /// <summary>
    /// Validates the mathematical-model solve request.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the mathematical model or solver parameters
    /// are missing or invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (Model is null)
        {
            throw new InvalidOperationException(
                "A mathematical model is required.");
        }

        Model.EnsureValid();

        if (Parameters is null)
        {
            throw new InvalidOperationException(
                "Solver parameters are required.");
        }

        Parameters.EnsureValid();

        RunName =
            RunName?.Trim() ??
            string.Empty;

        FormulationId =
            FormulationId?.Trim() ??
            string.Empty;

        foreach (
            ISolverProgressObserver observer
            in _progressObservers)
        {
            if (observer is null)
            {
                throw new InvalidOperationException(
                    "The progress-observer collection cannot " +
                    "contain a null entry.");
            }
        }
    }
}
