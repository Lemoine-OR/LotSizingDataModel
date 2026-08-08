using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Represents one factual element used to evaluate whether
/// a lot-sizing instance matches a known problem family.
/// </summary>
/// <remarks>
/// A classification match normally contains several evidence
/// objects.
///
/// Each object records:
/// <list type="bullet">
/// <item>
/// <description>the feature that was evaluated;</description>
/// </item>
/// <item>
/// <description>the expected value;</description>
/// </item>
/// <item>
/// <description>the observed value;</description>
/// </item>
/// <item>
/// <description>whether the condition was satisfied;</description>
/// </item>
/// <item>
/// <description>whether the condition was mandatory.</description>
/// </item>
/// </list>
///
/// This class records evaluation results only. It does not
/// independently determine whether a complete problem-family
/// match is exact, partial or approximate.
/// </remarks>
[Serializable]
[XmlType(TypeName = "classificationEvidence")]
public sealed class ClassificationEvidence : ModelObject
{
    private string _ruleCode =
        string.Empty;

    private string _featureCode =
        string.Empty;

    private string _description =
        string.Empty;

    private string _expectedValue =
        string.Empty;

    private string _observedValue =
        string.Empty;

    private bool _isSatisfied;
    private bool _isRequired = true;
    private double _weight = 1.0;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty classification-evidence object.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public ClassificationEvidence()
    {
    }

    /// <summary>
    /// Initializes a classification-evidence object.
    /// </summary>
    /// <param name="featureCode">
    /// Stable code identifying the evaluated feature.
    /// </param>
    /// <param name="expectedValue">
    /// Value expected by the known problem-family rule.
    /// </param>
    /// <param name="observedValue">
    /// Value observed in the analyzed instance.
    /// </param>
    /// <param name="isSatisfied">
    /// Value indicating whether the expected condition
    /// is satisfied.
    /// </param>
    /// <param name="isRequired">
    /// Value indicating whether failure of this condition
    /// prevents a direct match.
    /// </param>
    /// <param name="description">
    /// Optional human-readable description of the evaluated
    /// condition.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="featureCode"/> is null,
    /// empty or composed only of white-space characters.
    /// </exception>
    public ClassificationEvidence(
        string featureCode,
        string expectedValue,
        string observedValue,
        bool isSatisfied,
        bool isRequired = true,
        string description = "")
    {
        if (string.IsNullOrWhiteSpace(featureCode))
        {
            throw new ArgumentException(
                "A classification feature code is required.",
                nameof(featureCode));
        }

        FeatureCode = featureCode;

        ExpectedValue =
            expectedValue;

        ObservedValue =
            observedValue;

        IsSatisfied =
            isSatisfied;

        IsRequired =
            isRequired;

        Description =
            description;
    }

    /// <summary>
    /// Gets or sets an optional stable identifier for the
    /// classification rule that produced this evidence.
    /// </summary>
    /// <remarks>
    /// A rule code is useful when several conditions evaluate
    /// the same feature.
    ///
    /// Examples include:
    /// <c>LSU.ITEM_COUNT</c> and
    /// <c>CLSP.MULTI_ITEM</c>.
    /// </remarks>
    [XmlAttribute("ruleCode")]
    public string RuleCode
    {
        get => _ruleCode;
        set
        {
            if (SetProperty(
                    ref _ruleCode,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasRuleCode));
            }
        }
    }

    /// <summary>
    /// Gets or sets the stable code of the feature that
    /// was evaluated.
    /// </summary>
    /// <remarks>
    /// The code should normally correspond to a property of
    /// <see cref="LotSizingProblemFeatures"/>.
    ///
    /// Examples include:
    /// <c>itemCount</c>,
    /// <c>isMultiLevel</c>,
    /// <c>isCapacitated</c> and
    /// <c>hasTransportation</c>.
    /// </remarks>
    [XmlAttribute("featureCode")]
    public string FeatureCode
    {
        get => _featureCode;
        set => SetProperty(
            ref _featureCode,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets a human-readable description of the
    /// evaluated condition.
    /// </summary>
    /// <remarks>
    /// This text is intended for reports and user interfaces.
    /// The stable programmatic identifier remains
    /// <see cref="FeatureCode"/>.
    /// </remarks>
    [XmlElement("description")]
    public string Description
    {
        get => _description;
        set => SetProperty(
            ref _description,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the value expected by the known
    /// problem-family rule.
    /// </summary>
    /// <remarks>
    /// The value is stored as text so that the evidence can
    /// represent Boolean, numerical, textual and enumeration
    /// conditions without depending on a specific condition
    /// implementation.
    /// </remarks>
    [XmlElement("expectedValue")]
    public string ExpectedValue
    {
        get => _expectedValue;
        set => SetProperty(
            ref _expectedValue,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the value observed in the analyzed
    /// lot-sizing instance.
    /// </summary>
    [XmlElement("observedValue")]
    public string ObservedValue
    {
        get => _observedValue;
        set => SetProperty(
            ref _observedValue,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the observed
    /// feature satisfies the expected condition.
    /// </summary>
    [XmlAttribute("satisfied")]
    public bool IsSatisfied
    {
        get => _isSatisfied;
        set
        {
            if (SetProperty(
                    ref _isSatisfied,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the evaluated
    /// condition is mandatory for a direct match.
    /// </summary>
    /// <remarks>
    /// An unsatisfied required condition normally prevents
    /// an exact match.
    ///
    /// An unsatisfied optional condition may instead identify
    /// an extension, an additional feature or a difference
    /// that does not invalidate the underlying family.
    /// </remarks>
    [XmlAttribute("required")]
    public bool IsRequired
    {
        get => _isRequired;
        set
        {
            if (SetProperty(
                    ref _isRequired,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the relative importance assigned to this
    /// evidence during approximate matching.
    /// </summary>
    /// <remarks>
    /// A weight of zero means that the evidence is recorded
    /// for information only.
    ///
    /// Required conditions remain required regardless of
    /// their weight.
    /// </remarks>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the supplied value is negative, not finite
    /// or not a number.
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
                    "The evidence weight must be finite " +
                    "and non-negative.");
            }

            SetProperty(
                ref _weight,
                value);
        }
    }

    /// <summary>
    /// Gets or sets an optional explanatory comment about
    /// the evidence.
    /// </summary>
    /// <remarks>
    /// The comment may describe how the observed value was
    /// extracted or why an unsatisfied condition is treated
    /// as an accepted extension.
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
    /// Gets a value indicating whether a classification-rule
    /// code has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasRuleCode =>
        !string.IsNullOrWhiteSpace(
            RuleCode);

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
    /// the evaluated problem-family match.
    /// </summary>
    [XmlIgnore]
    public bool SupportsMatch =>
        IsSatisfied;

    /// <summary>
    /// Gets a value indicating whether a required condition
    /// is not satisfied.
    /// </summary>
    /// <remarks>
    /// A blocking mismatch normally prevents an exact or
    /// direct match with the evaluated problem family.
    /// </remarks>
    [XmlIgnore]
    public bool IsBlockingMismatch =>
        IsRequired &&
        !IsSatisfied;

    /// <summary>
    /// Gets a value indicating whether an optional condition
    /// is not satisfied.
    /// </summary>
    /// <remarks>
    /// An optional mismatch may identify a known extension
    /// or an additional feature of the complete problem.
    /// </remarks>
    [XmlIgnore]
    public bool IsOptionalMismatch =>
        !IsRequired &&
        !IsSatisfied;

    /// <inheritdoc/>
    public override string ToString()
    {
        string result =
            IsSatisfied
                ? "satisfied"
                : "not satisfied";

        string requirement =
            IsRequired
                ? "required"
                : "optional";

        return
            $"{FeatureCode}: expected " +
            $"'{ExpectedValue}', observed " +
            $"'{ObservedValue}' — {result}, " +
            $"{requirement}";
    }

    private void NotifyDerivedProperties()
    {
        OnPropertyChanged(
            nameof(SupportsMatch));

        OnPropertyChanged(
            nameof(IsBlockingMismatch));

        OnPropertyChanged(
            nameof(IsOptionalMismatch));
    }
}