using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Monitoring;

namespace LotSizingDataModel.Solver.Configuration;

/// <summary>
/// Defines solver-independent parameters for an optimization
/// run.
/// </summary>
/// <remarks>
/// Solver adapters translate these generic parameters to their
/// native CPLEX, Gurobi, Xpress, or COIN-OR CBC equivalents.
///
/// Additional solver-specific values can be supplied through
/// <see cref="NativeParameters"/>.
/// </remarks>
[Serializable]
[XmlType(TypeName = "solverParameters")]
public sealed class SolverParameters
{
    private readonly List<SolverNativeParameter>
        _nativeParameters =
            new();

    /// <summary>
    /// Initializes solver parameters with recommended default
    /// values.
    /// </summary>
    public SolverParameters()
    {
        TimeLimitSeconds =
            null;

        ThreadCount =
            null;

        RandomSeed =
            null;

        RelativeMipGap =
            null;

        AbsoluteMipGap =
            null;

        NodeLimit =
            null;

        IterationLimit =
            null;

        SolutionLimit =
            null;

        MemoryLimitMegabytes =
            null;

        LogLevel =
            SolverLogLevel.Information;

        EnablePresolve =
            null;

        EnableCuts =
            null;

        EnableHeuristics =
            null;

        DeterministicMode =
            false;

        WriteLogFile =
            false;

        LogFilePath =
            string.Empty;

        ExportModel =
            false;

        ExportModelPath =
            string.Empty;

        ProgressRecording =
            new SolverProgressRecordingOptions();
    }

    /// <summary>
    /// Gets or sets the wall-clock time limit, in seconds.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value leaves the solver default
    /// unchanged.
    /// </remarks>
    [XmlElement("timeLimitSeconds")]
    public double? TimeLimitSeconds
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the maximum number of solver threads.
    /// </summary>
    [XmlElement("threadCount")]
    public int? ThreadCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver random seed.
    /// </summary>
    [XmlElement("randomSeed")]
    public int? RandomSeed
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the relative mixed-integer optimality gap.
    /// </summary>
    [XmlElement("relativeMipGap")]
    public double? RelativeMipGap
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the absolute mixed-integer optimality gap.
    /// </summary>
    [XmlElement("absoluteMipGap")]
    public double? AbsoluteMipGap
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the maximum number of explored search-tree
    /// nodes.
    /// </summary>
    [XmlElement("nodeLimit")]
    public long? NodeLimit
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the maximum number of solver iterations.
    /// </summary>
    [XmlElement("iterationLimit")]
    public long? IterationLimit
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the maximum number of feasible solutions to
    /// search for.
    /// </summary>
    [XmlElement("solutionLimit")]
    public int? SolutionLimit
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the approximate memory limit, in megabytes.
    /// </summary>
    [XmlElement("memoryLimitMegabytes")]
    public double? MemoryLimitMegabytes
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the normalized solver log level.
    /// </summary>
    [XmlAttribute("logLevel")]
    public SolverLogLevel LogLevel
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether solver presolve is enabled.
    /// </summary>
    /// <remarks>
    /// A <see langword="null"/> value leaves the solver default
    /// unchanged.
    /// </remarks>
    [XmlElement("enablePresolve")]
    public bool? EnablePresolve
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether automatic cut generation is
    /// enabled.
    /// </summary>
    [XmlElement("enableCuts")]
    public bool? EnableCuts
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether primal heuristics are enabled.
    /// </summary>
    [XmlElement("enableHeuristics")]
    public bool? EnableHeuristics
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether deterministic execution is
    /// requested when supported by the selected solver.
    /// </summary>
    [XmlAttribute("deterministicMode")]
    public bool DeterministicMode
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether solver output is written to a log
    /// file.
    /// </summary>
    [XmlAttribute("writeLogFile")]
    public bool WriteLogFile
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the solver log file path.
    /// </summary>
    [XmlElement("logFilePath")]
    public string LogFilePath
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets whether the generated mathematical model is
    /// exported before solving.
    /// </summary>
    [XmlAttribute("exportModel")]
    public bool ExportModel
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the destination path used to export the
    /// generated model.
    /// </summary>
    [XmlElement("exportModelPath")]
    public string ExportModelPath
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the progress-history recording options.
    /// </summary>
    [XmlElement("progressRecording")]
    public SolverProgressRecordingOptions ProgressRecording
    {
        get;
        set;
    }

    /// <summary>
    /// Gets solver-specific parameters that are passed directly
    /// to the selected adapter.
    /// </summary>
    [XmlArray("nativeParameters")]
    [XmlArrayItem("parameter")]
    public List<SolverNativeParameter> NativeParameters =>
        _nativeParameters;

    /// <summary>
    /// Validates the solver parameters.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when one or more parameter values are invalid.
    /// </exception>
    public void EnsureValid()
    {
        ValidateFiniteStrictlyPositiveNullable(
            TimeLimitSeconds,
            nameof(TimeLimitSeconds));

        ValidateStrictlyPositiveNullable(
            ThreadCount,
            nameof(ThreadCount));

        ValidateFiniteNonNegativeNullable(
            RelativeMipGap,
            nameof(RelativeMipGap));

        ValidateFiniteNonNegativeNullable(
            AbsoluteMipGap,
            nameof(AbsoluteMipGap));

        ValidateNonNegativeNullable(
            NodeLimit,
            nameof(NodeLimit));

        ValidateNonNegativeNullable(
            IterationLimit,
            nameof(IterationLimit));

        ValidateStrictlyPositiveNullable(
            SolutionLimit,
            nameof(SolutionLimit));

        ValidateFiniteStrictlyPositiveNullable(
            MemoryLimitMegabytes,
            nameof(MemoryLimitMegabytes));

        if (WriteLogFile &&
            string.IsNullOrWhiteSpace(
                LogFilePath))
        {
            throw new InvalidOperationException(
                "A log file path is required when solver log " +
                "file writing is enabled.");
        }

        if (ExportModel &&
            string.IsNullOrWhiteSpace(
                ExportModelPath))
        {
            throw new InvalidOperationException(
                "A model export path is required when model " +
                "export is enabled.");
        }

        ArgumentNullException.ThrowIfNull(
            ProgressRecording);

        ProgressRecording.EnsureValid();

        foreach (
            SolverNativeParameter parameter
            in _nativeParameters)
        {
            if (parameter is null)
            {
                throw new InvalidOperationException(
                    "The native-parameter collection cannot " +
                    "contain a null entry.");
            }

            parameter.EnsureValid();
        }
    }

    private static void
        ValidateFiniteStrictlyPositiveNullable(
            double? value,
            string propertyName)
    {
        if (!value.HasValue)
        {
            return;
        }

        if (value.Value <= 0.0 ||
            double.IsNaN(
                value.Value) ||
            double.IsInfinity(
                value.Value))
        {
            throw new InvalidOperationException(
                $"{propertyName} must be a finite strictly " +
                "positive number when specified.");
        }
    }

    private static void ValidateFiniteNonNegativeNullable(
        double? value,
        string propertyName)
    {
        if (!value.HasValue)
        {
            return;
        }

        if (value.Value < 0.0 ||
            double.IsNaN(
                value.Value) ||
            double.IsInfinity(
                value.Value))
        {
            throw new InvalidOperationException(
                $"{propertyName} must be a finite " +
                "non-negative number when specified.");
        }
    }

    private static void ValidateStrictlyPositiveNullable(
        int? value,
        string propertyName)
    {
        if (value.HasValue &&
            value.Value <= 0)
        {
            throw new InvalidOperationException(
                $"{propertyName} must be strictly positive " +
                "when specified.");
        }
    }

    private static void ValidateNonNegativeNullable(
        long? value,
        string propertyName)
    {
        if (value.HasValue &&
            value.Value < 0)
        {
            throw new InvalidOperationException(
                $"{propertyName} cannot be negative when " +
                "specified.");
        }
    }
}
