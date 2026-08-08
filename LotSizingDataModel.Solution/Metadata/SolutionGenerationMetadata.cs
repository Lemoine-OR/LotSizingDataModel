using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Solution.Common;

namespace LotSizingDataModel.Solution.Metadata;

/// <summary>
/// Describes how a lot-sizing solution was generated.
/// </summary>
/// <remarks>
/// This class is independent of any particular solver,
/// heuristic, metaheuristic or software implementation.
/// </remarks>
[Serializable]
[XmlType(TypeName = "solutionGenerationMetadata")]
public sealed class SolutionGenerationMetadata : ModelObject
{
    private SolutionMethodKind _methodKind =
        SolutionMethodKind.Unknown;

    private TerminationReason _terminationReason =
        TerminationReason.Unknown;

    private string _methodName = string.Empty;
    private string _methodVersion = string.Empty;
    private string _implementationName = string.Empty;
    private string _implementationVersion = string.Empty;
    private string _comment = string.Empty;

    private DateTime _createdAtUtc =
        DateTime.UtcNow;

    private double? _durationSeconds;
    private int? _randomSeed;
    private long? _iterationCount;
    private long? _evaluationCount;
    private bool? _isDeterministic;

    /// <summary>
    /// Initializes empty solution-generation metadata.
    /// </summary>
    /// <remarks>
    /// The creation date is initialized with the current
    /// Coordinated Universal Time.
    /// </remarks>
    public SolutionGenerationMetadata()
    {
    }

    /// <summary>
    /// Initializes solution-generation metadata for a method.
    /// </summary>
    /// <param name="methodKind">
    /// General category of the solution-generation method.
    /// </param>
    /// <param name="methodName">
    /// Human-readable name of the method.
    /// </param>
    public SolutionGenerationMetadata(
        SolutionMethodKind methodKind,
        string methodName)
        : this()
    {
        MethodKind = methodKind;
        MethodName = methodName;
    }

    /// <summary>
    /// Gets or sets the general category of the method
    /// used to generate the solution.
    /// </summary>
    [XmlAttribute("methodKind")]
    public SolutionMethodKind MethodKind
    {
        get => _methodKind;
        set => SetProperty(
            ref _methodKind,
            value);
    }

    /// <summary>
    /// Gets or sets the reason why the execution stopped.
    /// </summary>
    [XmlAttribute("terminationReason")]
    public TerminationReason TerminationReason
    {
        get => _terminationReason;
        set => SetProperty(
            ref _terminationReason,
            value);
    }

    /// <summary>
    /// Gets or sets the human-readable name of the algorithm
    /// or solution-generation method.
    /// </summary>
    /// <example>
    /// Genetic algorithm, branch-and-cut, Silver-Meal heuristic
    /// or manual construction.
    /// </example>
    [XmlAttribute("methodName")]
    public string MethodName
    {
        get => _methodName;
        set => SetProperty(
            ref _methodName,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the version of the algorithm
    /// or solution-generation method.
    /// </summary>
    [XmlAttribute("methodVersion")]
    public string MethodVersion
    {
        get => _methodVersion;
        set => SetProperty(
            ref _methodVersion,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the name of the software implementation
    /// used to execute the method.
    /// </summary>
    /// <example>
    /// IBM ILOG CPLEX, Custom genetic algorithm
    /// or LotSizingDataModel.Heuristics.
    /// </example>
    [XmlAttribute("implementationName")]
    public string ImplementationName
    {
        get => _implementationName;
        set => SetProperty(
            ref _implementationName,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the version of the software implementation.
    /// </summary>
    [XmlAttribute("implementationVersion")]
    public string ImplementationVersion
    {
        get => _implementationVersion;
        set => SetProperty(
            ref _implementationVersion,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which
    /// the solution was generated.
    /// </summary>
    [XmlElement("createdAtUtc")]
    public DateTime CreatedAtUtc
    {
        get => _createdAtUtc;
        set
        {
            DateTime utcValue =
                value.Kind switch
                {
                    DateTimeKind.Utc => value,

                    DateTimeKind.Local =>
                        value.ToUniversalTime(),

                    _ => DateTime.SpecifyKind(
                        value,
                        DateTimeKind.Utc)
                };

            SetProperty(
                ref _createdAtUtc,
                utcValue);
        }
    }

    /// <summary>
    /// Gets or sets the execution duration in seconds.
    /// </summary>
    /// <remarks>
    /// A null value means that the duration was not recorded.
    /// </remarks>
    [XmlElement("durationSeconds", IsNullable = true)]
    public double? DurationSeconds
    {
        get => _durationSeconds;
        set
        {
            if (value.HasValue &&
                (!double.IsFinite(value.Value) ||
                 value.Value < 0.0))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The execution duration must be finite " +
                    "and non-negative.");
            }

            SetProperty(
                ref _durationSeconds,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the random seed used by the method.
    /// </summary>
    /// <remarks>
    /// A null value means that no seed was used
    /// or that it was not recorded.
    /// </remarks>
    [XmlElement("randomSeed", IsNullable = true)]
    public int? RandomSeed
    {
        get => _randomSeed;
        set => SetProperty(
            ref _randomSeed,
            value);
    }

    /// <summary>
    /// Gets or sets the number of algorithm iterations
    /// performed before termination.
    /// </summary>
    /// <remarks>
    /// A null value means that the iteration count
    /// is not applicable or was not recorded.
    /// </remarks>
    [XmlElement("iterationCount", IsNullable = true)]
    public long? IterationCount
    {
        get => _iterationCount;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The iteration count cannot be negative.");
            }

            SetProperty(
                ref _iterationCount,
                value);
        }
    }

    /// <summary>
    /// Gets or sets the number of candidate-solution
    /// or objective evaluations performed.
    /// </summary>
    /// <remarks>
    /// A null value means that the evaluation count
    /// is not applicable or was not recorded.
    /// </remarks>
    [XmlElement("evaluationCount", IsNullable = true)]
    public long? EvaluationCount
    {
        get => _evaluationCount;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The evaluation count cannot be negative.");
            }

            SetProperty(
                ref _evaluationCount,
                value);
        }
    }

    /// <summary>
    /// Gets or sets whether the method is deterministic
    /// under the recorded configuration.
    /// </summary>
    /// <remarks>
    /// A null value means that determinism is unknown
    /// or has not been specified.
    /// </remarks>
    [XmlElement("isDeterministic", IsNullable = true)]
    public bool? IsDeterministic
    {
        get => _isDeterministic;
        set => SetProperty(
            ref _isDeterministic,
            value);
    }

    /// <summary>
    /// Gets or sets an optional human-readable comment
    /// about the solution-generation execution.
    /// </summary>
    [XmlElement("comment")]
    public string Comment
    {
        get => _comment;
        set => SetProperty(
            ref _comment,
            value ?? string.Empty);
    }

    /// <summary>
    /// Gets the parameters used by the algorithm,
    /// solver or solution-generation method.
    /// </summary>
    [XmlArray("parameters")]
    [XmlArrayItem("parameter")]
    public List<AlgorithmParameter> Parameters { get; } =
        new();

    /// <summary>
    /// Gets a value indicating whether at least one
    /// algorithm parameter is recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasParameters =>
        Parameters.Count > 0;

    /// <summary>
    /// Adds an algorithm parameter.
    /// </summary>
    /// <param name="parameter">
    /// Parameter to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when the parameter is null.
    /// </exception>
    /// <exception cref="InvalidOperationException">
    /// Thrown when another parameter has the same name.
    /// </exception>
    public void AddParameter(
        AlgorithmParameter parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        if (Parameters.Any(
                existing =>
                    string.Equals(
                        existing.Name,
                        parameter.Name,
                        StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException(
                $"An algorithm parameter named " +
                $"'{parameter.Name}' already exists.");
        }

        Parameters.Add(parameter);

        OnPropertyChanged(
            nameof(Parameters));

        OnPropertyChanged(
            nameof(HasParameters));
    }

    /// <summary>
    /// Adds or replaces an algorithm parameter.
    /// </summary>
    /// <param name="parameter">
    /// Parameter to add or use as a replacement.
    /// </param>
    public void SetParameter(
        AlgorithmParameter parameter)
    {
        ArgumentNullException.ThrowIfNull(parameter);

        int index =
            Parameters.FindIndex(
                existing =>
                    string.Equals(
                        existing.Name,
                        parameter.Name,
                        StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            Parameters[index] = parameter;
        }
        else
        {
            Parameters.Add(parameter);
        }

        OnPropertyChanged(
            nameof(Parameters));

        OnPropertyChanged(
            nameof(HasParameters));
    }

    /// <summary>
    /// Finds an algorithm parameter by name.
    /// </summary>
    /// <param name="name">
    /// Name of the parameter to find.
    /// </param>
    /// <returns>
    /// The matching parameter, or null when it does not exist.
    /// </returns>
    public AlgorithmParameter? FindParameter(
        string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return Parameters.FirstOrDefault(
            parameter =>
                string.Equals(
                    parameter.Name,
                    name,
                    StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Removes an algorithm parameter by name.
    /// </summary>
    /// <param name="name">
    /// Name of the parameter to remove.
    /// </param>
    /// <returns>
    /// True when a parameter was removed; otherwise, false.
    /// </returns>
    public bool RemoveParameter(
        string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        AlgorithmParameter? parameter =
            FindParameter(name);

        if (parameter is null)
        {
            return false;
        }

        bool removed =
            Parameters.Remove(parameter);

        if (removed)
        {
            OnPropertyChanged(
                nameof(Parameters));

            OnPropertyChanged(
                nameof(HasParameters));
        }

        return removed;
    }

    /// <summary>
    /// Removes every recorded algorithm parameter.
    /// </summary>
    public void ClearParameters()
    {
        if (Parameters.Count == 0)
        {
            return;
        }

        Parameters.Clear();

        OnPropertyChanged(
            nameof(Parameters));

        OnPropertyChanged(
            nameof(HasParameters));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string name =
            string.IsNullOrWhiteSpace(MethodName)
                ? MethodKind.ToString()
                : MethodName;

        return
            $"{name} — {TerminationReason}";
    }
}