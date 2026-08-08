using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Defines one known lot-sizing problem family in the
/// problem-type catalog.
/// </summary>
/// <remarks>
/// A definition describes a problem family independently
/// from any particular supply-chain instance.
///
/// Classification rules are referenced by stable rule codes.
/// Their executable evaluation logic is stored separately so
/// that catalog metadata remains serializable and extensible.
/// </remarks>
[Serializable]
[XmlType(TypeName = "knownProblemTypeDefinition")]
public sealed class KnownProblemTypeDefinition : ModelObject
{
    private string _code =
        string.Empty;

    private string _name =
        string.Empty;

    private string _definitionVersion =
        string.Empty;

    private string _description =
        string.Empty;

    private string _parentProblemTypeCode =
        string.Empty;

    private ProblemClassificationScope _defaultScope =
        ProblemClassificationScope.CompleteProblem;

    private bool _isEnabled = true;
    private bool _isAbstract;
    private int _priority;
    private double _closestMatchThreshold = 0.5;

    /// <summary>
    /// Initializes an empty known-problem-type definition.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public KnownProblemTypeDefinition()
    {
    }

    /// <summary>
    /// Initializes a known-problem-type definition.
    /// </summary>
    /// <param name="code">
    /// Stable code identifying the problem family.
    /// </param>
    /// <param name="name">
    /// Human-readable name of the problem family.
    /// </param>
    /// <param name="definitionVersion">
    /// Version of this catalog definition.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="code"/>,
    /// <paramref name="name"/> or
    /// <paramref name="definitionVersion"/> is empty.
    /// </exception>
    public KnownProblemTypeDefinition(
        string code,
        string name,
        string definitionVersion)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "A problem-type code is required.",
                nameof(code));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A problem-type name is required.",
                nameof(name));
        }

        if (string.IsNullOrWhiteSpace(
                definitionVersion))
        {
            throw new ArgumentException(
                "A problem-type definition version is required.",
                nameof(definitionVersion));
        }

        Code = code;
        Name = name;
        DefinitionVersion = definitionVersion;
    }

    /// <summary>
    /// Gets or sets the stable code identifying the known
    /// lot-sizing problem family.
    /// </summary>
    /// <remarks>
    /// Examples include <c>LS-U</c>, <c>LS-C</c>,
    /// <c>CLSP</c> and <c>MLCLSP</c>.
    /// </remarks>
    [XmlAttribute("code")]
    public string Code
    {
        get => _code;
        set
        {
            if (SetProperty(
                    ref _code,
                    NormalizeCode(value)))
            {
                NotifyIdentityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable name of the known
    /// problem family.
    /// </summary>
    [XmlAttribute("name")]
    public string Name
    {
        get => _name;
        set
        {
            if (SetProperty(
                    ref _name,
                    value?.Trim() ?? string.Empty))
            {
                NotifyIdentityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of this catalog definition.
    /// </summary>
    /// <remarks>
    /// This version identifies the classification semantics,
    /// not the software assembly version.
    /// </remarks>
    [XmlAttribute("definitionVersion")]
    public string DefinitionVersion
    {
        get => _definitionVersion;
        set
        {
            if (SetProperty(
                    ref _definitionVersion,
                    value?.Trim() ?? string.Empty))
            {
                NotifyIdentityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a human-readable description of the
    /// known problem family.
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
    /// Gets or sets the code of a broader parent problem
    /// family.
    /// </summary>
    /// <remarks>
    /// This property may be used to represent a catalog
    /// hierarchy.
    ///
    /// For example, a specialized family may identify a
    /// more general lot-sizing family as its parent.
    ///
    /// An empty value means that no parent family has been
    /// recorded.
    /// </remarks>
    [XmlAttribute("parentProblemTypeCode")]
    public string ParentProblemTypeCode
    {
        get => _parentProblemTypeCode;
        set
        {
            if (SetProperty(
                    ref _parentProblemTypeCode,
                    NormalizeCode(value)))
            {
                OnPropertyChanged(
                    nameof(HasParentProblemType));
            }
        }
    }

    /// <summary>
    /// Gets or sets the default scope used when this family
    /// directly describes a problem.
    /// </summary>
    /// <remarks>
    /// The classifier may override this value when the family
    /// is recognized only for a relaxation, an item or another
    /// subsystem.
    /// </remarks>
    [XmlAttribute("defaultScope")]
    public ProblemClassificationScope DefaultScope
    {
        get => _defaultScope;
        set => SetProperty(
            ref _defaultScope,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether this
    /// definition is enabled for automatic classification.
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
    /// Gets or sets a value indicating whether this
    /// definition represents an abstract catalog category.
    /// </summary>
    /// <remarks>
    /// An abstract family may organize more specialized
    /// definitions without being selected as the primary
    /// classification of an instance.
    /// </remarks>
    [XmlAttribute("abstract")]
    public bool IsAbstract
    {
        get => _isAbstract;
        set
        {
            if (SetProperty(
                    ref _isAbstract,
                    value))
            {
                OnPropertyChanged(
                    nameof(CanBePrimaryMatch));
            }
        }
    }

    /// <summary>
    /// Gets or sets the priority used to resolve matches
    /// having equivalent quality.
    /// </summary>
    /// <remarks>
    /// A greater value gives the definition a greater
    /// selection priority.
    ///
    /// Match quality and required-rule satisfaction must be
    /// considered before this priority.
    /// </remarks>
    [XmlAttribute("priority")]
    public int Priority
    {
        get => _priority;
        set => SetProperty(
            ref _priority,
            value);
    }

    /// <summary>
    /// Gets or sets the minimum weighted score required for
    /// this family to be reported as the closest known family.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the value is not finite or is outside the
    /// interval from zero to one.
    /// </exception>
    [XmlAttribute("closestMatchThreshold")]
    public double ClosestMatchThreshold
    {
        get => _closestMatchThreshold;
        set
        {
            if (!double.IsFinite(value) ||
                value < 0.0 ||
                value > 1.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The closest-match threshold must be " +
                    "finite and between zero and one.");
            }

            SetProperty(
                ref _closestMatchThreshold,
                value);
        }
    }

    /// <summary>
    /// Gets the alternative codes and abbreviations used for
    /// this problem family.
    /// </summary>
    /// <remarks>
    /// The primary stable identifier remains <see cref="Code"/>.
    /// </remarks>
    [XmlArray("alternativeCodes")]
    [XmlArrayItem("code")]
    public List<string> AlternativeCodes { get; } =
        new();

    /// <summary>
    /// Gets the codes of rules that must all be satisfied for
    /// a direct match with this problem family.
    /// </summary>
    /// <remarks>
    /// Failure of one required rule prevents an exact match
    /// and normally prevents a known-extension match.
    /// </remarks>
    [XmlArray("requiredRuleCodes")]
    [XmlArrayItem("ruleCode")]
    public List<string> RequiredRuleCodes { get; } =
        new();

    /// <summary>
    /// Gets the codes of non-mandatory rules used to improve
    /// scoring and distinguish closely related families.
    /// </summary>
    [XmlArray("optionalRuleCodes")]
    [XmlArrayItem("ruleCode")]
    public List<string> OptionalRuleCodes { get; } =
        new();

    /// <summary>
    /// Gets the codes of rules identifying accepted
    /// extensions of the classical family.
    /// </summary>
    /// <remarks>
    /// When one of these rules is satisfied, the classifier
    /// may return <see cref="ProblemMatchKind.KnownExtension"/>
    /// instead of <see cref="ProblemMatchKind.Exact"/>.
    /// </remarks>
    [XmlArray("extensionRuleCodes")]
    [XmlArrayItem("ruleCode")]
    public List<string> ExtensionRuleCodes { get; } =
        new();

    /// <summary>
    /// Gets the codes of rules whose satisfaction excludes a
    /// direct correspondence with this family.
    /// </summary>
    /// <remarks>
    /// Exclusion rules describe features that fundamentally
    /// contradict the definition rather than merely extending
    /// it.
    /// </remarks>
    [XmlArray("exclusionRuleCodes")]
    [XmlArrayItem("ruleCode")]
    public List<string> ExclusionRuleCodes { get; } =
        new();

    /// <summary>
    /// Gets the bibliographic or documentary references
    /// supporting this problem-family definition.
    /// </summary>
    /// <remarks>
    /// Each entry may contain a DOI, citation, report
    /// identifier or another stable source reference.
    /// </remarks>
    [XmlArray("references")]
    [XmlArrayItem("reference")]
    public List<string> References { get; } =
        new();

    /// <summary>
    /// Gets a value indicating whether the definition has a
    /// stable problem-family code.
    /// </summary>
    [XmlIgnore]
    public bool HasCode =>
        !string.IsNullOrWhiteSpace(Code);

    /// <summary>
    /// Gets a value indicating whether the definition has a
    /// human-readable name.
    /// </summary>
    [XmlIgnore]
    public bool HasName =>
        !string.IsNullOrWhiteSpace(Name);

    /// <summary>
    /// Gets a value indicating whether a definition version
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasDefinitionVersion =>
        !string.IsNullOrWhiteSpace(
            DefinitionVersion);

    /// <summary>
    /// Gets a value indicating whether a parent problem
    /// family has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasParentProblemType =>
        !string.IsNullOrWhiteSpace(
            ParentProblemTypeCode);

    /// <summary>
    /// Gets a value indicating whether at least one required
    /// classification rule has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasRequiredRules =>
        RequiredRuleCodes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one optional
    /// classification rule has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasOptionalRules =>
        OptionalRuleCodes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one accepted
    /// extension rule has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasExtensionRules =>
        ExtensionRuleCodes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether at least one exclusion
    /// rule has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasExclusionRules =>
        ExclusionRuleCodes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the definition
    /// contains the minimum metadata required for evaluation.
    /// </summary>
    [XmlIgnore]
    public bool IsValidDefinition =>
        HasCode &&
        HasName &&
        HasDefinitionVersion &&
        HasRequiredRules;

    /// <summary>
    /// Gets a value indicating whether the definition can
    /// currently be evaluated by the classifier.
    /// </summary>
    [XmlIgnore]
    public bool CanBeEvaluated =>
        IsEnabled &&
        IsValidDefinition;

    /// <summary>
    /// Gets a value indicating whether this definition may be
    /// selected as the primary classification.
    /// </summary>
    [XmlIgnore]
    public bool CanBePrimaryMatch =>
        CanBeEvaluated &&
        !IsAbstract;

    /// <summary>
    /// Replaces the alternative codes of this problem family.
    /// </summary>
    /// <param name="codes">
    /// Alternative codes and abbreviations.
    /// </param>
    public void ReplaceAlternativeCodes(
        IEnumerable<string> codes)
    {
        ReplaceNormalizedValues(
            AlternativeCodes,
            codes,
            nameof(codes),
            normalizeAsCode: true);

        NotifyCollections();
    }

    /// <summary>
    /// Replaces the required classification-rule codes.
    /// </summary>
    /// <param name="ruleCodes">
    /// Codes of mandatory rules.
    /// </param>
    public void ReplaceRequiredRuleCodes(
        IEnumerable<string> ruleCodes)
    {
        ReplaceNormalizedValues(
            RequiredRuleCodes,
            ruleCodes,
            nameof(ruleCodes),
            normalizeAsCode: false);

        NotifyCollections();
    }

    /// <summary>
    /// Replaces the optional classification-rule codes.
    /// </summary>
    /// <param name="ruleCodes">
    /// Codes of optional rules.
    /// </param>
    public void ReplaceOptionalRuleCodes(
        IEnumerable<string> ruleCodes)
    {
        ReplaceNormalizedValues(
            OptionalRuleCodes,
            ruleCodes,
            nameof(ruleCodes),
            normalizeAsCode: false);

        NotifyCollections();
    }

    /// <summary>
    /// Replaces the accepted-extension rule codes.
    /// </summary>
    /// <param name="ruleCodes">
    /// Codes of extension rules.
    /// </param>
    public void ReplaceExtensionRuleCodes(
        IEnumerable<string> ruleCodes)
    {
        ReplaceNormalizedValues(
            ExtensionRuleCodes,
            ruleCodes,
            nameof(ruleCodes),
            normalizeAsCode: false);

        NotifyCollections();
    }

    /// <summary>
    /// Replaces the exclusion-rule codes.
    /// </summary>
    /// <param name="ruleCodes">
    /// Codes of exclusion rules.
    /// </param>
    public void ReplaceExclusionRuleCodes(
        IEnumerable<string> ruleCodes)
    {
        ReplaceNormalizedValues(
            ExclusionRuleCodes,
            ruleCodes,
            nameof(ruleCodes),
            normalizeAsCode: false);

        NotifyCollections();
    }

    /// <summary>
    /// Replaces the documentary references supporting this
    /// definition.
    /// </summary>
    /// <param name="references">
    /// Bibliographic or documentary references.
    /// </param>
    public void ReplaceReferences(
        IEnumerable<string> references)
    {
        ReplaceNormalizedValues(
            References,
            references,
            nameof(references),
            normalizeAsCode: false);

        OnPropertyChanged(nameof(References));
    }

    /// <summary>
    /// Determines whether the supplied code identifies this
    /// problem family.
    /// </summary>
    /// <param name="code">
    /// Primary or alternative code to search for.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the supplied code matches
    /// the primary code or one of the alternative codes.
    /// </returns>
    public bool MatchesCode(string code)
    {
        string normalizedCode =
            NormalizeCode(code);

        if (string.IsNullOrEmpty(normalizedCode))
        {
            return false;
        }

        if (string.Equals(
                Code,
                normalizedCode,
                StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return AlternativeCodes.Any(
            alternativeCode =>
                string.Equals(
                    alternativeCode,
                    normalizedCode,
                    StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Returns all distinct rule codes referenced by this
    /// definition.
    /// </summary>
    /// <returns>
    /// Ordered collection of referenced rule codes.
    /// </returns>
    public IReadOnlyList<string> GetAllRuleCodes()
    {
        return RequiredRuleCodes
            .Concat(OptionalRuleCodes)
            .Concat(ExtensionRuleCodes)
            .Concat(ExclusionRuleCodes)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                code => code,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return
            $"{Code} — {Name}; version " +
            $"{DefinitionVersion}; " +
            $"{RequiredRuleCodes.Count} required rule(s)";
    }

    private static string NormalizeCode(
        string? value)
    {
        return value?
            .Trim()
            .ToUpperInvariant() ??
            string.Empty;
    }

    private static string NormalizeRuleCode(
        string? value)
    {
        return value?
            .Trim() ??
            string.Empty;
    }

    private static void ReplaceNormalizedValues(
        ICollection<string> destination,
        IEnumerable<string> source,
        string parameterName,
        bool normalizeAsCode)
    {
        ArgumentNullException.ThrowIfNull(source);

        string[] normalizedValues =
            source
                .Select(
                    value =>
                        normalizeAsCode
                            ? NormalizeCode(value)
                            : NormalizeRuleCode(value))
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(value))
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    value => value,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        destination.Clear();

        foreach (string normalizedValue
                 in normalizedValues)
        {
            destination.Add(normalizedValue);
        }
    }

    private void NotifyIdentityProperties()
    {
        OnPropertyChanged(nameof(HasCode));
        OnPropertyChanged(nameof(HasName));
        OnPropertyChanged(nameof(HasDefinitionVersion));
        OnPropertyChanged(nameof(IsValidDefinition));
        OnPropertyChanged(nameof(CanBeEvaluated));
        OnPropertyChanged(nameof(CanBePrimaryMatch));
    }

    private void NotifyCollections()
    {
        OnPropertyChanged(nameof(AlternativeCodes));
        OnPropertyChanged(nameof(RequiredRuleCodes));
        OnPropertyChanged(nameof(OptionalRuleCodes));
        OnPropertyChanged(nameof(ExtensionRuleCodes));
        OnPropertyChanged(nameof(ExclusionRuleCodes));

        OnPropertyChanged(nameof(HasRequiredRules));
        OnPropertyChanged(nameof(HasOptionalRules));
        OnPropertyChanged(nameof(HasExtensionRules));
        OnPropertyChanged(nameof(HasExclusionRules));

        OnPropertyChanged(nameof(IsValidDefinition));
        OnPropertyChanged(nameof(CanBeEvaluated));
        OnPropertyChanged(nameof(CanBePrimaryMatch));
    }
}