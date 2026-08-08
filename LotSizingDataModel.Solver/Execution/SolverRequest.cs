using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Configuration;
using LotSizingDataModel.Solver.Contracts;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Describes a request to solve a lot-sizing instance.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverRequest")]
public sealed class SolverRequest
{
    private readonly List<ISolverProgressObserver>
        _progressObservers =
            new();

    /// <summary>
    /// Initializes an empty solver request.
    /// </summary>
    public SolverRequest()
    {
        PreferredSolver =
            SolverKind.Automatic;

        FormulationName =
            string.Empty;

        RunName =
            string.Empty;

        Parameters =
            new SolverParameters();
    }

    /// <summary>
    /// Initializes a solver request for an instance.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to solve.
    /// </param>
    public SolverRequest(
        LotSizingInstance instance)
        : this()
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        Instance =
            instance;
    }

    /// <summary>
    /// Gets or sets the lot-sizing instance to solve.
    /// </summary>
    [XmlIgnore]
    public LotSizingInstance? Instance
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the preferred solver.
    /// </summary>
    /// <remarks>
    /// <see cref="SolverKind.Automatic"/> requests automatic
    /// selection among the adapters available on the current
    /// computer.
    /// </remarks>
    [XmlAttribute("preferredSolver")]
    public SolverKind PreferredSolver
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the formulation name to use.
    /// </summary>
    /// <remarks>
    /// An empty value requests the default formulation selected
    /// by the formulation service.
    /// </remarks>
    [XmlElement("formulationName")]
    public string FormulationName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets a user-defined name for the solver run.
    /// </summary>
    [XmlElement("runName")]
    public string RunName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver parameters.
    /// </summary>
    [XmlElement("parameters")]
    public SolverParameters Parameters
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the progress observers attached to this request.
    /// </summary>
    [XmlIgnore]
    public IList<ISolverProgressObserver> ProgressObservers =>
        _progressObservers;

    /// <summary>
    /// Validates the solver request.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the request is incomplete or invalid.
    /// </exception>
    public void EnsureValid()
    {
        if (Instance is null)
        {
            throw new InvalidOperationException(
                "A lot-sizing instance is required.");
        }

        if (PreferredSolver ==
            SolverKind.Unknown)
        {
            throw new InvalidOperationException(
                "The preferred solver cannot be Unknown.");
        }

        ArgumentNullException.ThrowIfNull(
            Parameters);

        Parameters.EnsureValid();

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

    /// <summary>
    /// Adds a progress observer.
    /// </summary>
    /// <param name="observer">
    /// Observer to attach.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="observer"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void AddProgressObserver(
        ISolverProgressObserver observer)
    {
        ArgumentNullException.ThrowIfNull(
            observer);

        _progressObservers.Add(
            observer);
    }
}
