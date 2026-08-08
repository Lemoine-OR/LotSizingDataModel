using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solver.Contracts;

namespace LotSizingDataModel.Solver.Common;

/// <summary>
/// Represents the result of attempting to load a solver adapter
/// plugin.
/// </summary>
[Serializable]
[XmlType(TypeName = "solverAdapterLoadResult")]
public sealed class SolverAdapterLoadResult
{
    private readonly List<string> _diagnostics =
        new();

    /// <summary>
    /// Initializes an empty solver-adapter load result.
    /// </summary>
    public SolverAdapterLoadResult()
    {
        AssemblyPath =
            string.Empty;

        TypeName =
            string.Empty;

        ExceptionType =
            string.Empty;

        ExceptionMessage =
            string.Empty;

        Status =
            SolverAdapterLoadStatus.Unknown;
    }

    /// <summary>
    /// Gets or sets the descriptor associated with the adapter
    /// loading attempt.
    /// </summary>
    [XmlElement("descriptor")]
    public SolverAdapterDescriptor? Descriptor
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the final adapter loading status.
    /// </summary>
    [XmlAttribute("status")]
    public SolverAdapterLoadStatus Status
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the full path of the adapter assembly.
    /// </summary>
    [XmlElement("assemblyPath")]
    public string AssemblyPath
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the requested adapter type name.
    /// </summary>
    [XmlElement("typeName")]
    public string TypeName
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the loaded adapter instance.
    /// </summary>
    /// <remarks>
    /// The adapter instance is excluded from XML serialization
    /// because it represents a live runtime object.
    /// </remarks>
    [XmlIgnore]
    public ISolverAdapter? Adapter
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the exception type associated with a load
    /// failure.
    /// </summary>
    [XmlElement("exceptionType")]
    public string ExceptionType
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the exception message associated with a
    /// load failure.
    /// </summary>
    [XmlElement("exceptionMessage")]
    public string ExceptionMessage
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the diagnostics produced during adapter loading.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets a value indicating whether the adapter was loaded
    /// successfully.
    /// </summary>
    [XmlIgnore]
    public bool IsLoaded =>
        Status ==
            SolverAdapterLoadStatus.Loaded &&
        Adapter is not null;

    /// <summary>
    /// Adds a diagnostic message.
    /// </summary>
    /// <param name="message">
    /// Diagnostic message.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="message"/> is empty.
    /// </exception>
    public void AddDiagnostic(
        string message)
    {
        if (string.IsNullOrWhiteSpace(
                message))
        {
            throw new ArgumentException(
                "A diagnostic message cannot be empty.",
                nameof(message));
        }

        _diagnostics.Add(
            message.Trim());
    }

    /// <summary>
    /// Stores exception information without retaining the live
    /// exception object.
    /// </summary>
    /// <param name="exception">
    /// Exception to describe.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="exception"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void SetException(
        Exception exception)
    {
        ArgumentNullException.ThrowIfNull(
            exception);

        ExceptionType =
            exception.GetType().FullName ??
            exception.GetType().Name;

        ExceptionMessage =
            exception.Message;
    }
}
