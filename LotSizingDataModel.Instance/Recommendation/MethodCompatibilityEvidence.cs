using System;
using System.Globalization;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Recommendation;

/// <summary>
/// Represents one piece of evidence used to evaluate the
/// compatibility between a solution method and a lot-sizing
/// problem instance.
/// </summary>
/// <remarks>
/// Compatibility evidence may describe:
/// <list type="bullet">
/// <item>
/// <description>
/// a supported or unsupported problem family;
/// </description>
/// </item>
/// <item>
/// <description>
/// a required problem feature;
/// </description>
/// </item>
/// <item>
/// <description>
/// an excluded modeling characteristic;
/// </description>
/// </item>
/// <item>
/// <description>
/// a method or implementation size limit;
/// </description>
/// </item>
/// <item>
/// <description>
/// a favorable structural characteristic that can be
/// exploited by the method.
/// </description>
/// </item>
/// </list>
///
/// Evidence is deliberately independent from the mechanism
/// used to evaluate the criterion. The expected and observed
/// values are stored as text for traceability.
/// </remarks>
[Serializable]
[XmlType(TypeName = "methodCompatibilityEvidence")]
public sealed class MethodCompatibilityEvidence : ModelObject
{
    private string _criterionCode =
        string.Empty;

    private string _featureCode =
        string.Empty;

    private string _description =
        string.Empty;

    private string _expectedValue =
        string.Empty;

    private string _observedValue =
        string.Empty;

    private string _affectedEntityKey =
        string.Empty;

    private bool _isSatisfied;

    private bool _isRequired =
        true;

    private bool _isBlocking;

    private double _weight =
        1.0;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty compatibility-evidence record.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public MethodCompatibilityEvidence()
    {
    }

    /// <summary>
    /// Initializes compatibility evidence for one evaluated
    /// criterion.
    /// </summary>
    /// <param name="criterionCode">
    /// Stable code identifying the evaluated criterion.
    /// </param>
    /// <param name="description">
    /// Human-readable explanation of the criterion.
    /// </param>
    /// <param name="isSatisfied">
    /// Value indicating whether the criterion is satisfied.
    /// </param>
    /// <param name="isRequired">
    /// Value indicating whether the criterion represents a
    /// required compatibility condition.
    /// </param>
    /// <param name="isBlocking">
    /// Value indicating whether failure of the criterion makes
    /// the method incompatible with the complete problem.
    /// </param>
    /// <param name="weight">
    /// Non-negative importance assigned to the criterion.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="criterionCode"/> or
    /// <paramref name="description"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="weight"/> is negative or
    /// not finite.
    /// </exception>
    public MethodCompatibilityEvidence(
        string criterionCode,
        string description,
        bool isSatisfied,
        bool isRequired = true,
        bool isBlocking = false,
        double weight = 1.0)
    {
        if (string.IsNullOrWhiteSpace(criterionCode))
        {
            throw new ArgumentException(
                "A compatibility criterion code is required.",
                nameof(criterionCode));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "A compatibility criterion description is " +
                "required.",
                nameof(description));
        }

        CriterionCode =
            criterionCode;

        Description =
            description;

        IsSatisfied =
            isSatisfied;

        IsRequired =
            isRequired;

        IsBlocking =
            isBlocking;

        Weight =
            weight;
    }

    /// <summary>
    /// Gets or sets the stable code identifying the evaluated
    /// compatibility criterion.
    /// </summary>
    /// <remarks>
    /// Examples include:
    /// <list type="bullet">
    /// <item>
    /// <description><c>FAMILY.SUPPORTS_MLLP</c>;</description>
    /// </item>
    /// <item>
    /// <description><c>FEATURE.NO_BACKLOGGING</c>;</description>
    /// </item>
    /// <item>
    /// <description><c>LIMIT.MAX_ITEMS</c>;</description>
    /// </item>
    /// <item>
    /// <description><c>STRUCTURE.SERIAL</c>.</description>
    /// </item>
    /// </list>
    /// </remarks>
    [XmlAttribute("criterionCode")]
    public string CriterionCode
    {
        get => _criterionCode;
        set
        {
            if (SetProperty(
                    ref _criterionCode,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasCriterionCode));

                OnPropertyChanged(
                    nameof(IsValidEvidence));
            }
        }
    }

    /// <summary>
    /// Gets or sets the code of the problem feature evaluated
    /// by this evidence.
    /// </summary>
    /// <remarks>
    /// This property is optional because some criteria concern
    /// a problem family, a method capability or a size limit
    /// rather than one property of
    /// <c>LotSizingProblemFeatures</c>.
    /// </remarks>
    [XmlAttribute("featureCode")]
    public string FeatureCode
    {
        get => _featureCode;
        set
        {
            if (SetProperty(
                    ref _featureCode,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasFeatureCode));
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable explanation of the
    /// compatibility criterion.
    /// </summary>
    /// <remarks>
    /// The description should explain why the observed
    /// characteristic supports or limits the applicability of
    /// the method.
    /// </remarks>
    [XmlElement("description")]
    public string Description
    {
        get => _description;
        set
        {
            if (SetProperty(
                    ref _description,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasDescription));

                OnPropertyChanged(
                    nameof(IsValidEvidence));
            }
        }
    }

    /// <summary>
    /// Gets or sets the value expected by the method or
    /// compatibility criterion.
    /// </summary>
    /// <remarks>
    /// Examples include <c>true</c>, <c>MLLP</c>,
    /// <c>singleItem</c> and <c>&lt;= 100</c>.
    /// </remarks>
    [XmlAttribute("expectedValue")]
    public string ExpectedValue
    {
        get => _expectedValue;
        set
        {
            if (SetProperty(
                    ref _expectedValue,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasExpectedValue));
            }
        }
    }

    /// <summary>
    /// Gets or sets the value observed in the analyzed
    /// instance.
    /// </summary>
    [XmlAttribute("observedValue")]
    public string ObservedValue
    {
        get => _observedValue;
        set
        {
            if (SetProperty(
                    ref _observedValue,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasObservedValue));
            }
        }
    }

    /// <summary>
    /// Gets or sets an optional key identifying the entity to
    /// which the evidence applies.
    /// </summary>
    /// <remarks>
    /// Examples include <c>item:12</c>,
    /// <c>plant:3</c>, <c>workCenter:3/7</c> and
    /// <c>transportResource:5</c>.
    /// </remarks>
    [XmlAttribute("affectedEntityKey")]
    public string AffectedEntityKey
    {
        get => _affectedEntityKey;
        set
        {
            if (SetProperty(
                    ref _affectedEntityKey,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasAffectedEntityKey));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the
    /// compatibility criterion is satisfied.
    /// </summary>
    [XmlAttribute("isSatisfied")]
    public bool IsSatisfied
    {
        get => _isSatisfied;
        set
        {
            if (SetProperty(
                    ref _isSatisfied,
                    value))
            {
                NotifyEvaluationProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the criterion
    /// represents a required condition.
    /// </summary>
    /// <remarks>
    /// A required mismatch indicates that the method cannot
    /// be applied directly under its standard assumptions.
    ///
    /// It does not necessarily make the method entirely
    /// unusable when decomposition, relaxation or adaptation
    /// is possible.
    /// </remarks>
    [XmlAttribute("isRequired")]
    public bool IsRequired
    {
        get => _isRequired;
        set
        {
            if (SetProperty(
                    ref _isRequired,
                    value))
            {
                NotifyEvaluationProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether failure of the
    /// criterion makes the method incompatible with the
    /// complete problem.
    /// </summary>
    /// <remarks>
    /// Examples of blocking criteria include an unsupported
    /// product structure, an excluded modeling feature or a
    /// hard implementation limit.
    /// </remarks>
    [XmlAttribute("isBlocking")]
    public bool IsBlocking
    {
        get => _isBlocking;
        set
        {
            if (SetProperty(
                    ref _isBlocking,
                    value))
            {
                NotifyEvaluationProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the non-negative importance assigned to
    /// the criterion.
    /// </summary>
    /// <remarks>
    /// The advisor may use this weight when calculating an
    /// overall compatibility score.
    ///
    /// Blocking mismatches remain blocking independently of
    /// their numerical weight.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied value is negative or not
    /// finite.
    /// </exception>
    [XmlAttribute("weight")]
    public double Weight
    {
        get => _weight;
        set
        {
            if (!double.IsFinite(value) ||
                value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The compatibility-evidence weight must " +
                    "be finite and non-negative.");
            }

            if (SetProperty(
                    ref _weight,
                    value))
            {
                OnPropertyChanged(
                    nameof(ScoreContribution));

                OnPropertyChanged(
                    nameof(IsValidEvidence));
            }
        }
    }

    /// <summary>
    /// Gets or sets an optional explanatory comment.
    /// </summary>
    /// <remarks>
    /// The comment may describe an adaptation, approximation
    /// or implementation-specific limitation.
    /// </remarks>
    [XmlElement("comment")]
    public string Comment
    {
        get => _comment;
        set
        {
            if (SetProperty(
                    ref _comment,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasComment));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether a criterion code has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasCriterionCode =>
        !string.IsNullOrWhiteSpace(
            CriterionCode);

    /// <summary>
    /// Gets a value indicating whether a problem-feature code
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasFeatureCode =>
        !string.IsNullOrWhiteSpace(
            FeatureCode);

    /// <summary>
    /// Gets a value indicating whether a human-readable
    /// description has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasDescription =>
        !string.IsNullOrWhiteSpace(
            Description);

    /// <summary>
    /// Gets a value indicating whether an expected value has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasExpectedValue =>
        !string.IsNullOrWhiteSpace(
            ExpectedValue);

    /// <summary>
    /// Gets a value indicating whether an observed value has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasObservedValue =>
        !string.IsNullOrWhiteSpace(
            ObservedValue);

    /// <summary>
    /// Gets a value indicating whether an affected entity key
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasAffectedEntityKey =>
        !string.IsNullOrWhiteSpace(
            AffectedEntityKey);

    /// <summary>
    /// Gets a value indicating whether an explanatory comment
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    /// <summary>
    /// Gets a value indicating whether this evidence supports
    /// the compatibility of the method.
    /// </summary>
    [XmlIgnore]
    public bool SupportsCompatibility =>
        IsSatisfied;

    /// <summary>
    /// Gets a value indicating whether the evaluated
    /// criterion is not satisfied.
    /// </summary>
    [XmlIgnore]
    public bool IsMismatch =>
        !IsSatisfied;

    /// <summary>
    /// Gets a value indicating whether the evidence represents
    /// a blocking incompatibility.
    /// </summary>
    [XmlIgnore]
    public bool IsBlockingMismatch =>
        !IsSatisfied &&
        IsBlocking;

    /// <summary>
    /// Gets a value indicating whether a required criterion
    /// is not satisfied.
    /// </summary>
    [XmlIgnore]
    public bool IsRequiredMismatch =>
        !IsSatisfied &&
        IsRequired;

    /// <summary>
    /// Gets a value indicating whether a non-required and
    /// non-blocking criterion is not satisfied.
    /// </summary>
    [XmlIgnore]
    public bool IsOptionalMismatch =>
        !IsSatisfied &&
        !IsRequired &&
        !IsBlocking;

    /// <summary>
    /// Gets the signed numerical contribution of this
    /// evidence to a compatibility score.
    /// </summary>
    /// <remarks>
    /// Satisfied evidence contributes a positive value and
    /// unsatisfied evidence contributes a negative value.
    ///
    /// This value does not replace the explicit handling of
    /// blocking criteria.
    /// </remarks>
    [XmlIgnore]
    public double ScoreContribution =>
        IsSatisfied
            ? Weight
            : -Weight;

    /// <summary>
    /// Gets a value indicating whether this evidence contains
    /// the minimum information required for use by the method
    /// advisor.
    /// </summary>
    [XmlIgnore]
    public bool IsValidEvidence =>
        HasCriterionCode &&
        HasDescription &&
        double.IsFinite(Weight) &&
        Weight >= 0.0;

    /// <summary>
    /// Sets the textual expected and observed values of the
    /// evaluated criterion.
    /// </summary>
    /// <param name="expectedValue">
    /// Value expected by the method.
    /// </param>
    /// <param name="observedValue">
    /// Value observed in the problem instance.
    /// </param>
    public void SetComparedValues(
        object? expectedValue,
        object? observedValue)
    {
        ExpectedValue =
            ConvertToInvariantString(
                expectedValue);

        ObservedValue =
            ConvertToInvariantString(
                observedValue);
    }

    /// <summary>
    /// Marks the criterion as satisfied.
    /// </summary>
    /// <param name="observedValue">
    /// Optional observed value.
    /// </param>
    public void MarkAsSatisfied(
        object? observedValue = null)
    {
        if (observedValue is not null)
        {
            ObservedValue =
                ConvertToInvariantString(
                    observedValue);
        }

        IsSatisfied =
            true;
    }

    /// <summary>
    /// Marks the criterion as unsatisfied.
    /// </summary>
    /// <param name="observedValue">
    /// Optional observed value.
    /// </param>
    /// <param name="comment">
    /// Optional explanation of the mismatch.
    /// </param>
    public void MarkAsUnsatisfied(
        object? observedValue = null,
        string comment = "")
    {
        if (observedValue is not null)
        {
            ObservedValue =
                ConvertToInvariantString(
                    observedValue);
        }

        Comment =
            comment;

        IsSatisfied =
            false;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string state =
            IsSatisfied
                ? "satisfied"
                : IsBlocking
                    ? "blocking mismatch"
                    : IsRequired
                        ? "required mismatch"
                        : "optional mismatch";

        return
            $"{CriterionCode}: {state}; " +
            $"{Description}";
    }

    private static string ConvertToInvariantString(
        object? value)
    {
        if (value is null)
        {
            return string.Empty;
        }

        if (value is bool booleanValue)
        {
            return booleanValue
                ? "true"
                : "false";
        }

        if (value is DateTime dateTimeValue)
        {
            return dateTimeValue.ToString(
                "O",
                CultureInfo.InvariantCulture);
        }

        if (value is DateTimeOffset dateTimeOffsetValue)
        {
            return dateTimeOffsetValue.ToString(
                "O",
                CultureInfo.InvariantCulture);
        }

        if (value is IFormattable formattableValue)
        {
            return formattableValue.ToString(
                       null,
                       CultureInfo.InvariantCulture) ??
                   string.Empty;
        }

        return value.ToString() ??
               string.Empty;
    }

    private void NotifyEvaluationProperties()
    {
        OnPropertyChanged(
            nameof(SupportsCompatibility));

        OnPropertyChanged(
            nameof(IsMismatch));

        OnPropertyChanged(
            nameof(IsBlockingMismatch));

        OnPropertyChanged(
            nameof(IsRequiredMismatch));

        OnPropertyChanged(
            nameof(IsOptionalMismatch));

        OnPropertyChanged(
            nameof(ScoreContribution));
    }
}