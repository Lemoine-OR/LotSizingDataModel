using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Defines one reusable condition used to classify
/// lot-sizing problem instances.
/// </summary>
/// <remarks>
/// A rule definition identifies a property of
/// <see cref="LotSizingProblemFeatures"/>, a comparison
/// operator and an optional expected value.
///
/// The rule definition does not contain an evaluation result.
/// Evaluating a rule against an instance produces a
/// <see cref="ClassificationEvidence"/> object.
/// </remarks>
[Serializable]
[XmlType(TypeName = "knownProblemRuleDefinition")]
public sealed class KnownProblemRuleDefinition : ModelObject
{
    /// <summary>
    /// Identifies an equality comparison.
    /// </summary>
    public const string EqualsOperator = "EQUALS";

    /// <summary>
    /// Identifies an inequality comparison.
    /// </summary>
    public const string NotEqualsOperator = "NOT_EQUALS";

    /// <summary>
    /// Identifies a strict greater-than comparison.
    /// </summary>
    public const string GreaterThanOperator = "GREATER_THAN";

    /// <summary>
    /// Identifies a greater-than-or-equal comparison.
    /// </summary>
    public const string GreaterThanOrEqualOperator =
        "GREATER_THAN_OR_EQUAL";

    /// <summary>
    /// Identifies a strict less-than comparison.
    /// </summary>
    public const string LessThanOperator = "LESS_THAN";

    /// <summary>
    /// Identifies a less-than-or-equal comparison.
    /// </summary>
    public const string LessThanOrEqualOperator =
        "LESS_THAN_OR_EQUAL";

    /// <summary>
    /// Identifies a collection or text containment test.
    /// </summary>
    public const string ContainsOperator = "CONTAINS";

    /// <summary>
    /// Identifies a negative collection or text
    /// containment test.
    /// </summary>
    public const string NotContainsOperator = "NOT_CONTAINS";

    /// <summary>
    /// Identifies a membership test against a collection
    /// of accepted values.
    /// </summary>
    public const string InOperator = "IN";

    /// <summary>
    /// Identifies a negative membership test.
    /// </summary>
    public const string NotInOperator = "NOT_IN";

    /// <summary>
    /// Identifies a Boolean condition requiring the
    /// observed value to be true.
    /// </summary>
    public const string IsTrueOperator = "IS_TRUE";

    /// <summary>
    /// Identifies a Boolean condition requiring the
    /// observed value to be false.
    /// </summary>
    public const string IsFalseOperator = "IS_FALSE";

    private static readonly HashSet<string>
        SupportedOperatorCodes =
            new(
                new[]
                {
                    EqualsOperator,
                    NotEqualsOperator,
                    GreaterThanOperator,
                    GreaterThanOrEqualOperator,
                    LessThanOperator,
                    LessThanOrEqualOperator,
                    ContainsOperator,
                    NotContainsOperator,
                    InOperator,
                    NotInOperator,
                    IsTrueOperator,
                    IsFalseOperator
                },
                StringComparer.OrdinalIgnoreCase);

    private string _ruleCode =
        string.Empty;

    private string _featureCode =
        string.Empty;

    private string _operatorCode =
        string.Empty;

    private string _expectedValue =
        string.Empty;

    private string _description =
        string.Empty;

    private double _weight = 1.0;
    private bool _isEnabled = true;

    /// <summary>
    /// Initializes an empty known-problem rule definition.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public KnownProblemRuleDefinition()
    {
    }

    /// <summary>
    /// Initializes a known-problem rule definition.
    /// </summary>
    /// <param name="ruleCode">
    /// Stable code identifying the classification rule.
    /// </param>
    /// <param name="featureCode">
    /// Code of the feature evaluated by the rule.
    /// </param>
    /// <param name="operatorCode">
    /// Code of the comparison operator.
    /// </param>
    /// <param name="expectedValue">
    /// Optional expected value used by the comparison.
    /// </param>
    /// <param name="description">
    /// Optional human-readable description of the rule.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="ruleCode"/>,
    /// <paramref name="featureCode"/> or
    /// <paramref name="operatorCode"/> is empty.
    /// </exception>
    public KnownProblemRuleDefinition(
        string ruleCode,
        string featureCode,
        string operatorCode,
        string expectedValue = "",
        string description = "")
    {
        if (string.IsNullOrWhiteSpace(ruleCode))
        {
            throw new ArgumentException(
                "A classification-rule code is required.",
                nameof(ruleCode));
        }

        if (string.IsNullOrWhiteSpace(featureCode))
        {
            throw new ArgumentException(
                "A feature code is required.",
                nameof(featureCode));
        }

        if (string.IsNullOrWhiteSpace(operatorCode))
        {
            throw new ArgumentException(
                "A comparison-operator code is required.",
                nameof(operatorCode));
        }

        RuleCode = ruleCode;
        FeatureCode = featureCode;
        OperatorCode = operatorCode;
        ExpectedValue = expectedValue;
        Description = description;
    }

    /// <summary>
    /// Gets or sets the stable code identifying this
    /// classification rule.
    /// </summary>
    /// <remarks>
    /// Examples include <c>LSU.SINGLE_ITEM</c>,
    /// <c>LSU.UNCAPACITATED</c> and
    /// <c>EXT.TRANSPORTATION</c>.
    /// </remarks>
    [XmlAttribute("ruleCode")]
    public string RuleCode
    {
        get => _ruleCode;
        set
        {
            if (SetProperty(
                    ref _ruleCode,
                    NormalizeRuleCode(value)))
            {
                NotifyDefinitionProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the stable code of the feature evaluated
    /// by this rule.
    /// </summary>
    /// <remarks>
    /// The feature code normally identifies a public property
    /// of <see cref="LotSizingProblemFeatures"/>.
    ///
    /// Feature lookup should be case-insensitive.
    ///
    /// Examples include <c>itemCount</c>,
    /// <c>isMultiLevel</c>, <c>isCapacitated</c> and
    /// <c>hasTransportation</c>.
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
                NotifyDefinitionProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the code of the comparison operator used
    /// by this rule.
    /// </summary>
    /// <remarks>
    /// Supported operator codes are:
    /// <list type="bullet">
    /// <item><description><c>EQUALS</c>;</description></item>
    /// <item><description><c>NOT_EQUALS</c>;</description></item>
    /// <item><description><c>GREATER_THAN</c>;</description></item>
    /// <item><description><c>GREATER_THAN_OR_EQUAL</c>;</description></item>
    /// <item><description><c>LESS_THAN</c>;</description></item>
    /// <item><description><c>LESS_THAN_OR_EQUAL</c>;</description></item>
    /// <item><description><c>CONTAINS</c>;</description></item>
    /// <item><description><c>NOT_CONTAINS</c>;</description></item>
    /// <item><description><c>IN</c>;</description></item>
    /// <item><description><c>NOT_IN</c>;</description></item>
    /// <item><description><c>IS_TRUE</c>;</description></item>
    /// <item><description><c>IS_FALSE</c>.</description></item>
    /// </list>
    /// </remarks>
    [XmlAttribute("operatorCode")]
    public string OperatorCode
    {
        get => _operatorCode;
        set
        {
            if (SetProperty(
                    ref _operatorCode,
                    NormalizeOperatorCode(value)))
            {
                NotifyDefinitionProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the value expected by the comparison.
    /// </summary>
    /// <remarks>
    /// The value is stored as invariant text so that the same
    /// class can represent Boolean, numerical, enumeration and
    /// textual comparisons.
    ///
    /// The <c>IS_TRUE</c> and <c>IS_FALSE</c> operators do not
    /// require an expected value.
    ///
    /// For <c>IN</c> and <c>NOT_IN</c>, several values may be
    /// separated by semicolons.
    /// </remarks>
    [XmlElement("expectedValue")]
    public string ExpectedValue
    {
        get => _expectedValue;
        set
        {
            if (SetProperty(
                    ref _expectedValue,
                    value?.Trim() ?? string.Empty))
            {
                NotifyDefinitionProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a human-readable description of the
    /// classification rule.
    /// </summary>
    [XmlElement("description")]
    public string Description
    {
        get => _description;
        set => SetProperty(
            ref _description,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the relative importance of this rule
    /// during weighted matching.
    /// </summary>
    /// <remarks>
    /// A weight of zero keeps the rule informative without
    /// affecting the weighted matching score.
    ///
    /// Whether the rule is required, optional, an extension
    /// or an exclusion is determined by the collection in
    /// which its code appears in
    /// <see cref="KnownProblemTypeDefinition"/>.
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
                    "The rule weight must be finite and " +
                    "non-negative.");
            }

            SetProperty(
                ref _weight,
                value);
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether this rule is
    /// enabled for automatic evaluation.
    /// </summary>
    [XmlAttribute("enabled")]
    public bool IsEnabled
    {
        get => _isEnabled;
        set
        {
            if (SetProperty(
                    ref _isEnabled,
                    value))
            {
                OnPropertyChanged(
                    nameof(CanBeEvaluated));
            }
        }
    }

    /// <summary>
    /// Gets a value indicating whether a stable rule code
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasRuleCode =>
        !string.IsNullOrWhiteSpace(
            RuleCode);

    /// <summary>
    /// Gets a value indicating whether a feature code has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasFeatureCode =>
        !string.IsNullOrWhiteSpace(
            FeatureCode);

    /// <summary>
    /// Gets a value indicating whether the operator code is
    /// supported by the standard rule evaluator.
    /// </summary>
    [XmlIgnore]
    public bool HasSupportedOperator =>
        IsSupportedOperatorCode(
            OperatorCode);

    /// <summary>
    /// Gets a value indicating whether this operator requires
    /// an explicit expected value.
    /// </summary>
    [XmlIgnore]
    public bool RequiresExpectedValue =>
        !string.Equals(
            OperatorCode,
            IsTrueOperator,
            StringComparison.OrdinalIgnoreCase) &&
        !string.Equals(
            OperatorCode,
            IsFalseOperator,
            StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a value indicating whether an expected value has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasExpectedValue =>
        !string.IsNullOrWhiteSpace(
            ExpectedValue);

    /// <summary>
    /// Gets a value indicating whether this rule contains the
    /// metadata required for evaluation.
    /// </summary>
    [XmlIgnore]
    public bool IsValidDefinition =>
        HasRuleCode &&
        HasFeatureCode &&
        HasSupportedOperator &&
        (
            !RequiresExpectedValue ||
            HasExpectedValue
        );

    /// <summary>
    /// Gets a value indicating whether this rule can
    /// currently be evaluated.
    /// </summary>
    [XmlIgnore]
    public bool CanBeEvaluated =>
        IsEnabled &&
        IsValidDefinition;

    /// <summary>
    /// Determines whether an operator code is supported by
    /// the standard rule evaluator.
    /// </summary>
    /// <param name="operatorCode">
    /// Operator code to examine.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the code identifies one of
    /// the standard comparison operators.
    /// </returns>
    public static bool IsSupportedOperatorCode(
        string operatorCode)
    {
        string normalizedOperatorCode =
            NormalizeOperatorCode(
                operatorCode);

        return SupportedOperatorCodes.Contains(
            normalizedOperatorCode);
    }

    /// <summary>
    /// Returns all standard comparison-operator codes.
    /// </summary>
    /// <returns>
    /// Ordered collection of supported operator codes.
    /// </returns>
    public static IReadOnlyList<string>
        GetSupportedOperatorCodes()
    {
        return SupportedOperatorCodes
            .OrderBy(
                operatorCode =>
                    operatorCode,
                StringComparer.Ordinal)
            .ToArray();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string expectedDescription =
            RequiresExpectedValue
                ? $" {ExpectedValue}"
                : string.Empty;

        return
            $"{RuleCode}: {FeatureCode} " +
            $"{OperatorCode}" +
            $"{expectedDescription}";
    }

    private static string NormalizeRuleCode(
        string? value)
    {
        return value?
            .Trim()
            .ToUpperInvariant() ??
            string.Empty;
    }

    private static string NormalizeOperatorCode(
        string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        string compactCode =
            new(
                value
                    .Where(
                        character =>
                            char.IsLetterOrDigit(
                                character))
                    .Select(
                        character =>
                            char.ToUpperInvariant(
                                character))
                    .ToArray());

        return compactCode switch
        {
            "EQUALS" or "EQUAL" =>
                EqualsOperator,

            "NOTEQUALS" or "NOTEQUAL" =>
                NotEqualsOperator,

            "GREATERTHAN" =>
                GreaterThanOperator,

            "GREATERTHANOREQUAL" or
            "GREATERTHANOREQUALTO" =>
                GreaterThanOrEqualOperator,

            "LESSTHAN" =>
                LessThanOperator,

            "LESSTHANOREQUAL" or
            "LESSTHANOREQUALTO" =>
                LessThanOrEqualOperator,

            "CONTAINS" or "CONTAIN" =>
                ContainsOperator,

            "NOTCONTAINS" or "NOTCONTAIN" =>
                NotContainsOperator,

            "IN" =>
                InOperator,

            "NOTIN" =>
                NotInOperator,

            "ISTRUE" or "TRUE" =>
                IsTrueOperator,

            "ISFALSE" or "FALSE" =>
                IsFalseOperator,

            _ =>
                value.Trim().ToUpperInvariant()
        };
    }

    private void NotifyDefinitionProperties()
    {
        OnPropertyChanged(
            nameof(HasRuleCode));

        OnPropertyChanged(
            nameof(HasFeatureCode));

        OnPropertyChanged(
            nameof(HasSupportedOperator));

        OnPropertyChanged(
            nameof(RequiresExpectedValue));

        OnPropertyChanged(
            nameof(HasExpectedValue));

        OnPropertyChanged(
            nameof(IsValidDefinition));

        OnPropertyChanged(
            nameof(CanBeEvaluated));
    }
}