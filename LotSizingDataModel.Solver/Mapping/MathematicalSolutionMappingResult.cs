using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Solver.Mapping;

/// <summary>
/// Describes the result of mapping a mathematical solver result
/// back to a normalized lot-sizing solution.
/// </summary>
[Serializable]
[XmlType(TypeName = "mathematicalSolutionMappingResult")]
public sealed class MathematicalSolutionMappingResult
{
    private readonly List<string> _diagnostics =
        new();

    /// <summary>
    /// Initializes an empty mathematical-solution mapping result.
    /// </summary>
    public MathematicalSolutionMappingResult()
    {
        FailureMessage =
            string.Empty;
    }

    /// <summary>
    /// Gets or sets a value indicating whether mapping
    /// succeeded.
    /// </summary>
    [XmlAttribute("isSuccessful")]
    public bool IsSuccessful
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of mathematical variable values
    /// processed during mapping.
    /// </summary>
    [XmlAttribute("processedValueCount")]
    public int ProcessedValueCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of mathematical variable values
    /// ignored during mapping.
    /// </summary>
    [XmlAttribute("ignoredValueCount")]
    public int IgnoredValueCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the number of decision categories processed
    /// during mapping.
    /// </summary>
    [XmlAttribute("processedCategoryCount")]
    public int ProcessedCategoryCount
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mapping duration.
    /// </summary>
    [XmlIgnore]
    public TimeSpan MappingDuration
    {
        get;
        set;
    }

    /// <summary>
    /// Gets or sets the mapping duration in milliseconds for XML
    /// serialization.
    /// </summary>
    [XmlAttribute("mappingDurationMilliseconds")]
    public double MappingDurationMilliseconds
    {
        get =>
            MappingDuration.TotalMilliseconds;

        set =>
            MappingDuration =
                TimeSpan.FromMilliseconds(
                    value);
    }

    /// <summary>
    /// Gets or sets the mapping failure message.
    /// </summary>
    [XmlElement("failureMessage")]
    public string FailureMessage
    {
        get;
        set;
    }

    /// <summary>
    /// Gets the mapping diagnostics.
    /// </summary>
    [XmlArray("diagnostics")]
    [XmlArrayItem("diagnostic")]
    public List<string> Diagnostics =>
        _diagnostics;

    /// <summary>
    /// Gets or sets the normalized lot-sizing solution produced
    /// by the mapping process.
    /// </summary>
    [XmlElement("solution")]
    public LotSizingSolution? Solution
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
    /// Creates a successful mathematical-solution mapping result.
    /// </summary>
    /// <param name="solution">
    /// Mapped lot-sizing solution.
    /// </param>
    /// <param name="mappingDuration">
    /// Mapping duration.
    /// </param>
    /// <returns>
    /// Successful mapping result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="solution"/> is
    /// <see langword="null"/>.
    /// </exception>
    public static MathematicalSolutionMappingResult Success(
        LotSizingSolution solution,
        TimeSpan mappingDuration)
    {
        ArgumentNullException.ThrowIfNull(
            solution);

        return new MathematicalSolutionMappingResult
        {
            IsSuccessful =
                true,

            MappingDuration =
                mappingDuration,

            Solution =
                solution
        };
    }

    /// <summary>
    /// Creates a failed mathematical-solution mapping result.
    /// </summary>
    /// <param name="failureMessage">
    /// Mapping failure message.
    /// </param>
    /// <param name="mappingDuration">
    /// Elapsed mapping duration.
    /// </param>
    /// <returns>
    /// Failed mapping result.
    /// </returns>
    public static MathematicalSolutionMappingResult Failure(
        string failureMessage,
        TimeSpan mappingDuration)
    {
        var result =
            new MathematicalSolutionMappingResult
            {
                IsSuccessful =
                    false,

                MappingDuration =
                    mappingDuration,

                FailureMessage =
                    failureMessage?.Trim() ??
                    string.Empty
            };

        result.AddDiagnostic(
            failureMessage);

        return result;
    }

    /// <summary>
    /// Validates the mathematical-solution mapping result.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the result is inconsistent.
    /// </exception>
    public void EnsureValid()
    {
        if (MappingDuration < TimeSpan.Zero)
        {
            throw new InvalidOperationException(
                "Mapping duration cannot be negative.");
        }

        if (ProcessedValueCount < 0 ||
            IgnoredValueCount < 0 ||
            ProcessedCategoryCount < 0)
        {
            throw new InvalidOperationException(
                "Mapping counters cannot be negative.");
        }

        if (IsSuccessful &&
            Solution is null)
        {
            throw new InvalidOperationException(
                "A successful mapping result must contain a " +
                "lot-sizing solution.");
        }

        if (!IsSuccessful &&
            string.IsNullOrWhiteSpace(
                FailureMessage))
        {
            throw new InvalidOperationException(
                "A failed mapping result must contain a failure " +
                "message.");
        }
    }
}
