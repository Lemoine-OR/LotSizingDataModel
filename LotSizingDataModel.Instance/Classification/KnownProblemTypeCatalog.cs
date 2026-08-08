using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification;

/// <summary>
/// Stores known lot-sizing problem-family definitions and
/// the reusable rules used to recognize them.
/// </summary>
/// <remarks>
/// A catalog is versioned independently from the classifier
/// implementation.
///
/// The catalog may be serialized to XML and extended without
/// modifying the supply-chain data model.
///
/// Methods such as <see cref="AddDefinition"/> and
/// <see cref="AddRule"/> enforce code uniqueness. However,
/// XML deserialization or direct collection modifications may
/// bypass these methods. Therefore, <see cref="Validate"/>
/// should be called before the catalog is used.
/// </remarks>
[Serializable]
[XmlRoot("knownProblemTypeCatalog")]
[XmlType(TypeName = "knownProblemTypeCatalog")]
public sealed class KnownProblemTypeCatalog : ModelObject
{
    private string _catalogName =
        string.Empty;

    private string _catalogVersion =
        string.Empty;

    private string _description =
        string.Empty;

    /// <summary>
    /// Initializes an empty known-problem-type catalog.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public KnownProblemTypeCatalog()
    {
    }

    /// <summary>
    /// Initializes a known-problem-type catalog.
    /// </summary>
    /// <param name="catalogName">
    /// Human-readable name of the catalog.
    /// </param>
    /// <param name="catalogVersion">
    /// Version identifying the catalog contents and semantics.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="catalogName"/> or
    /// <paramref name="catalogVersion"/> is empty.
    /// </exception>
    public KnownProblemTypeCatalog(
        string catalogName,
        string catalogVersion)
    {
        if (string.IsNullOrWhiteSpace(catalogName))
        {
            throw new ArgumentException(
                "A catalog name is required.",
                nameof(catalogName));
        }

        if (string.IsNullOrWhiteSpace(catalogVersion))
        {
            throw new ArgumentException(
                "A catalog version is required.",
                nameof(catalogVersion));
        }

        CatalogName = catalogName;
        CatalogVersion = catalogVersion;
    }

    /// <summary>
    /// Gets or sets the human-readable name of the catalog.
    /// </summary>
    [XmlAttribute("name")]
    public string CatalogName
    {
        get => _catalogName;
        set
        {
            if (SetProperty(
                    ref _catalogName,
                    value?.Trim() ?? string.Empty))
            {
                NotifyValidationProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the version identifying the catalog
    /// contents and classification semantics.
    /// </summary>
    /// <remarks>
    /// This version is independent from the software assembly
    /// version and from individual problem-family definition
    /// versions.
    /// </remarks>
    [XmlAttribute("version")]
    public string CatalogVersion
    {
        get => _catalogVersion;
        set
        {
            if (SetProperty(
                    ref _catalogVersion,
                    value?.Trim() ?? string.Empty))
            {
                NotifyValidationProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a human-readable description of the
    /// catalog.
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
    /// Gets the known lot-sizing problem-family definitions.
    /// </summary>
    [XmlArray("problemTypeDefinitions")]
    [XmlArrayItem("problemTypeDefinition")]
    public List<KnownProblemTypeDefinition> Definitions
    {
        get;
    } = new();

    /// <summary>
    /// Gets the reusable classification-rule definitions.
    /// </summary>
    [XmlArray("ruleDefinitions")]
    [XmlArrayItem("ruleDefinition")]
    public List<KnownProblemRuleDefinition> Rules
    {
        get;
    } = new();

    /// <summary>
    /// Gets a value indicating whether the catalog has a
    /// human-readable name.
    /// </summary>
    [XmlIgnore]
    public bool HasCatalogName =>
        !string.IsNullOrWhiteSpace(CatalogName);

    /// <summary>
    /// Gets a value indicating whether the catalog has a
    /// version.
    /// </summary>
    [XmlIgnore]
    public bool HasCatalogVersion =>
        !string.IsNullOrWhiteSpace(CatalogVersion);

    /// <summary>
    /// Gets the number of problem-family definitions.
    /// </summary>
    [XmlIgnore]
    public int DefinitionCount =>
        Definitions.Count;

    /// <summary>
    /// Gets the number of rule definitions.
    /// </summary>
    [XmlIgnore]
    public int RuleCount =>
        Rules.Count;

    /// <summary>
    /// Gets the number of enabled problem-family definitions.
    /// </summary>
    [XmlIgnore]
    public int EnabledDefinitionCount =>
        Definitions.Count(
            definition =>
                definition is not null &&
                definition.IsEnabled);

    /// <summary>
    /// Gets the number of enabled rule definitions.
    /// </summary>
    [XmlIgnore]
    public int EnabledRuleCount =>
        Rules.Count(
            rule =>
                rule is not null &&
                rule.IsEnabled);

    /// <summary>
    /// Gets a value indicating whether the catalog contains
    /// no problem-family definition and no rule.
    /// </summary>
    [XmlIgnore]
    public bool IsEmpty =>
        Definitions.Count == 0 &&
        Rules.Count == 0;

    /// <summary>
    /// Gets a value indicating whether the catalog currently
    /// passes all structural validation checks.
    /// </summary>
    /// <remarks>
    /// This property performs a complete validation each time
    /// it is accessed.
    /// </remarks>
    [XmlIgnore]
    public bool IsValidCatalog =>
        Validate().Count == 0;

    /// <summary>
    /// Gets a value indicating whether the catalog can be
    /// used for automatic classification.
    /// </summary>
    [XmlIgnore]
    public bool CanClassify =>
        IsValidCatalog &&
        EnabledDefinitionCount > 0 &&
        EnabledRuleCount > 0;

    /// <summary>
    /// Adds a known problem-family definition.
    /// </summary>
    /// <param name="definition">
    /// Definition to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="definition"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the definition has no primary code or when
    /// one of its codes conflicts with an existing definition.
    /// </exception>
    public void AddDefinition(
        KnownProblemTypeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        if (string.IsNullOrWhiteSpace(definition.Code))
        {
            throw new ArgumentException(
                "The problem-family definition must have " +
                "a primary code.",
                nameof(definition));
        }

        var candidateDefinitions =
            Definitions
                .Concat(new[] { definition })
                .ToArray();

        IReadOnlyList<string> identityErrors =
            ValidateDefinitionCodeUniqueness(
                candidateDefinitions);

        if (identityErrors.Count > 0)
        {
            throw new ArgumentException(
                string.Join(
                    Environment.NewLine,
                    identityErrors),
                nameof(definition));
        }

        Definitions.Add(definition);

        NotifyCatalogCollections();
    }

    /// <summary>
    /// Adds a reusable classification-rule definition.
    /// </summary>
    /// <param name="rule">
    /// Rule definition to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rule"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the rule has no code or when its code is
    /// already used by another rule.
    /// </exception>
    public void AddRule(
        KnownProblemRuleDefinition rule)
    {
        ArgumentNullException.ThrowIfNull(rule);

        if (string.IsNullOrWhiteSpace(rule.RuleCode))
        {
            throw new ArgumentException(
                "The rule definition must have a rule code.",
                nameof(rule));
        }

        bool codeAlreadyUsed =
            Rules.Any(
                existingRule =>
                    existingRule is not null &&
                    string.Equals(
                        existingRule.RuleCode,
                        rule.RuleCode,
                        StringComparison.OrdinalIgnoreCase));

        if (codeAlreadyUsed)
        {
            throw new ArgumentException(
                $"Rule code '{rule.RuleCode}' is already " +
                "used by the catalog.",
                nameof(rule));
        }

        Rules.Add(rule);

        NotifyCatalogCollections();
    }

    /// <summary>
    /// Replaces all problem-family definitions.
    /// </summary>
    /// <param name="definitions">
    /// New problem-family definitions.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="definitions"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null element or
    /// conflicting primary or alternative codes.
    /// </exception>
    public void ReplaceDefinitions(
        IEnumerable<KnownProblemTypeDefinition> definitions)
    {
        ArgumentNullException.ThrowIfNull(definitions);

        KnownProblemTypeDefinition[] normalizedDefinitions =
            definitions.ToArray();

        if (normalizedDefinitions.Any(
                definition =>
                    definition is null))
        {
            throw new ArgumentException(
                "The definition collection cannot contain " +
                "a null element.",
                nameof(definitions));
        }

        IReadOnlyList<string> identityErrors =
            ValidateDefinitionCodeUniqueness(
                normalizedDefinitions);

        if (identityErrors.Count > 0)
        {
            throw new ArgumentException(
                string.Join(
                    Environment.NewLine,
                    identityErrors),
                nameof(definitions));
        }

        Definitions.Clear();
        Definitions.AddRange(normalizedDefinitions);

        NotifyCatalogCollections();
    }

    /// <summary>
    /// Replaces all reusable rule definitions.
    /// </summary>
    /// <param name="rules">
    /// New rule definitions.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="rules"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null element,
    /// an empty rule code or duplicate rule codes.
    /// </exception>
    public void ReplaceRules(
        IEnumerable<KnownProblemRuleDefinition> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        KnownProblemRuleDefinition[] normalizedRules =
            rules.ToArray();

        if (normalizedRules.Any(
                rule =>
                    rule is null))
        {
            throw new ArgumentException(
                "The rule collection cannot contain a null " +
                "element.",
                nameof(rules));
        }

        string[] emptyRuleCodes =
            normalizedRules
                .Where(
                    rule =>
                        string.IsNullOrWhiteSpace(
                            rule.RuleCode))
                .Select(
                    (_, index) =>
                        index.ToString())
                .ToArray();

        if (emptyRuleCodes.Length > 0)
        {
            throw new ArgumentException(
                "Every rule definition must have a rule code.",
                nameof(rules));
        }

        string[] duplicateRuleCodes =
            normalizedRules
                .GroupBy(
                    rule =>
                        rule.RuleCode,
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    code =>
                        code,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (duplicateRuleCodes.Length > 0)
        {
            throw new ArgumentException(
                "Duplicate rule codes: " +
                string.Join(", ", duplicateRuleCodes) +
                ".",
                nameof(rules));
        }

        Rules.Clear();
        Rules.AddRange(normalizedRules);

        NotifyCatalogCollections();
    }

    /// <summary>
    /// Finds a problem-family definition from its primary
    /// code or one of its alternative codes.
    /// </summary>
    /// <param name="code">
    /// Primary or alternative code to search for.
    /// </param>
    /// <returns>
    /// Matching definition, or <see langword="null"/> when no
    /// definition matches the supplied code.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the catalog contains several definitions
    /// matching the same code.
    /// </exception>
    public KnownProblemTypeDefinition? FindDefinition(
        string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        KnownProblemTypeDefinition[] matches =
            Definitions
                .Where(
                    definition =>
                        definition is not null &&
                        definition.MatchesCode(code))
                .Take(2)
                .ToArray();

        if (matches.Length > 1)
        {
            throw new InvalidOperationException(
                $"Problem-family code '{code}' is ambiguous " +
                "in the current catalog.");
        }

        return matches.Length == 0
            ? null
            : matches[0];
    }

    /// <summary>
    /// Finds a reusable classification rule from its code.
    /// </summary>
    /// <param name="ruleCode">
    /// Rule code to search for.
    /// </param>
    /// <returns>
    /// Matching rule definition, or <see langword="null"/>
    /// when no rule matches the supplied code.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the catalog contains several rules with
    /// the same code.
    /// </exception>
    public KnownProblemRuleDefinition? FindRule(
        string ruleCode)
    {
        if (string.IsNullOrWhiteSpace(ruleCode))
        {
            return null;
        }

        KnownProblemRuleDefinition[] matches =
            Rules
                .Where(
                    rule =>
                        rule is not null &&
                        string.Equals(
                            rule.RuleCode,
                            ruleCode.Trim(),
                            StringComparison.OrdinalIgnoreCase))
                .Take(2)
                .ToArray();

        if (matches.Length > 1)
        {
            throw new InvalidOperationException(
                $"Rule code '{ruleCode}' is ambiguous in " +
                "the current catalog.");
        }

        return matches.Length == 0
            ? null
            : matches[0];
    }

    /// <summary>
    /// Determines whether a problem-family code or alias is
    /// present in the catalog.
    /// </summary>
    /// <param name="code">
    /// Primary or alternative code to search for.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the code identifies one
    /// catalog definition.
    /// </returns>
    public bool ContainsDefinition(string code)
    {
        return FindDefinition(code) is not null;
    }

    /// <summary>
    /// Determines whether a reusable rule code is present in
    /// the catalog.
    /// </summary>
    /// <param name="ruleCode">
    /// Rule code to search for.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the rule exists.
    /// </returns>
    public bool ContainsRule(string ruleCode)
    {
        return FindRule(ruleCode) is not null;
    }

    /// <summary>
    /// Returns the enabled and valid problem-family
    /// definitions in classification order.
    /// </summary>
    /// <returns>
    /// Definitions ordered by decreasing priority and then by
    /// problem-family code.
    /// </returns>
    public IReadOnlyList<KnownProblemTypeDefinition>
        GetDefinitionsForClassification()
    {
        return Definitions
            .Where(
                definition =>
                    definition is not null &&
                    definition.CanBeEvaluated)
            .OrderByDescending(
                definition =>
                    definition.Priority)
            .ThenBy(
                definition =>
                    definition.Code,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Resolves all rule definitions referenced by one
    /// problem-family definition.
    /// </summary>
    /// <param name="definition">
    /// Problem-family definition whose rules must be resolved.
    /// </param>
    /// <returns>
    /// Referenced rules ordered by their rule codes.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="definition"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when a referenced rule is missing.
    /// </exception>
    public IReadOnlyList<KnownProblemRuleDefinition>
        GetReferencedRules(
            KnownProblemTypeDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(definition);

        var resolvedRules =
            new List<KnownProblemRuleDefinition>();

        foreach (string ruleCode
                 in definition.GetAllRuleCodes())
        {
            KnownProblemRuleDefinition? rule =
                FindRule(ruleCode);

            if (rule is null)
            {
                throw new KeyNotFoundException(
                    $"Problem-family definition " +
                    $"'{definition.Code}' references missing " +
                    $"rule '{ruleCode}'.");
            }

            resolvedRules.Add(rule);
        }

        return resolvedRules;
    }

    /// <summary>
    /// Validates the complete catalog structure.
    /// </summary>
    /// <returns>
    /// Ordered collection of validation-error messages.
    /// An empty collection indicates that the catalog is
    /// structurally valid.
    /// </returns>
    public IReadOnlyList<string> Validate()
    {
        var errors =
            new List<string>();

        if (!HasCatalogName)
        {
            errors.Add(
                "The catalog name is missing.");
        }

        if (!HasCatalogVersion)
        {
            errors.Add(
                "The catalog version is missing.");
        }

        if (Definitions.Count == 0)
        {
            errors.Add(
                "The catalog does not contain any " +
                "problem-family definition.");
        }

        if (Rules.Count == 0)
        {
            errors.Add(
                "The catalog does not contain any rule " +
                "definition.");
        }

        if (Definitions.Any(
                definition =>
                    definition is null))
        {
            errors.Add(
                "The problem-family definition collection " +
                "contains a null element.");
        }

        if (Rules.Any(
                rule =>
                    rule is null))
        {
            errors.Add(
                "The rule-definition collection contains a " +
                "null element.");
        }

        KnownProblemTypeDefinition[] definitions =
            Definitions
                .Where(
                    definition =>
                        definition is not null)
                .ToArray();

        KnownProblemRuleDefinition[] rules =
            Rules
                .Where(
                    rule =>
                        rule is not null)
                .ToArray();

        foreach (KnownProblemTypeDefinition definition
                 in definitions)
        {
            if (!definition.IsValidDefinition)
            {
                errors.Add(
                    $"Problem-family definition " +
                    $"'{DisplayDefinitionCode(definition)}' " +
                    "is incomplete or invalid.");
            }
        }

        foreach (KnownProblemRuleDefinition rule
                 in rules)
        {
            if (!rule.IsValidDefinition)
            {
                errors.Add(
                    $"Rule definition " +
                    $"'{DisplayRuleCode(rule)}' is incomplete " +
                    "or invalid.");
            }
        }

        errors.AddRange(
            ValidateDefinitionCodeUniqueness(
                definitions));

        Dictionary<string, KnownProblemRuleDefinition>
            ruleIndex =
                BuildRuleIndex(
                    rules,
                    errors);

        Dictionary<string, KnownProblemTypeDefinition>
            definitionIndex =
                BuildDefinitionIndex(
                    definitions,
                    errors);

        ValidateReferencedRules(
            definitions,
            ruleIndex,
            errors);

        ValidateParentDefinitions(
            definitions,
            definitionIndex,
            errors);

        return errors
            .Distinct(StringComparer.Ordinal)
            .OrderBy(
                error =>
                    error,
                StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Validates the catalog and throws an exception when at
    /// least one structural error is detected.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the catalog is invalid.
    /// </exception>
    public void EnsureValid()
    {
        IReadOnlyList<string> errors =
            Validate();

        if (errors.Count == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "The known-problem-type catalog is invalid:" +
            Environment.NewLine +
            string.Join(
                Environment.NewLine,
                errors.Select(
                    error =>
                        "- " + error)));
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return
            $"{CatalogName}; version {CatalogVersion}; " +
            $"{DefinitionCount} definition(s); " +
            $"{RuleCount} rule(s)";
    }

    private static IReadOnlyList<string>
        ValidateDefinitionCodeUniqueness(
            IEnumerable<KnownProblemTypeDefinition>
                definitions)
    {
        var errors =
            new List<string>();

        var codeOwners =
            new Dictionary<
                string,
                KnownProblemTypeDefinition>(
                    StringComparer.OrdinalIgnoreCase);

        foreach (KnownProblemTypeDefinition definition
                 in definitions)
        {
            RegisterDefinitionCode(
                definition.Code,
                definition,
                "primary code",
                codeOwners,
                errors);

            foreach (string alternativeCode
                     in definition.AlternativeCodes)
            {
                RegisterDefinitionCode(
                    alternativeCode,
                    definition,
                    "alternative code",
                    codeOwners,
                    errors);
            }
        }

        return errors;
    }

    private static void RegisterDefinitionCode(
        string code,
        KnownProblemTypeDefinition definition,
        string codeKind,
        IDictionary<string, KnownProblemTypeDefinition>
            codeOwners,
        ICollection<string> errors)
    {
        string normalizedCode =
            code?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizedCode))
        {
            errors.Add(
                $"Problem-family definition " +
                $"'{DisplayDefinitionCode(definition)}' " +
                $"contains an empty {codeKind}.");

            return;
        }

        if (!codeOwners.TryGetValue(
                normalizedCode,
                out KnownProblemTypeDefinition?
                    existingDefinition))
        {
            codeOwners.Add(
                normalizedCode,
                definition);

            return;
        }

        if (ReferenceEquals(
                existingDefinition,
                definition))
        {
            errors.Add(
                $"Code '{normalizedCode}' is registered " +
                $"more than once by problem-family " +
                $"definition " +
                $"'{DisplayDefinitionCode(definition)}'.");

            return;
        }

        errors.Add(
            $"Code '{normalizedCode}' is shared by " +
            $"problem-family definitions " +
            $"'{DisplayDefinitionCode(existingDefinition)}' " +
            $"and '{DisplayDefinitionCode(definition)}'.");
    }

    private static Dictionary<
        string,
        KnownProblemRuleDefinition>
        BuildRuleIndex(
            IEnumerable<KnownProblemRuleDefinition> rules,
            ICollection<string> errors)
    {
        var ruleIndex =
            new Dictionary<
                string,
                KnownProblemRuleDefinition>(
                    StringComparer.OrdinalIgnoreCase);

        foreach (KnownProblemRuleDefinition rule
                 in rules)
        {
            string ruleCode =
                rule.RuleCode?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(ruleCode))
            {
                continue;
            }

            if (!ruleIndex.TryAdd(
                    ruleCode,
                    rule))
            {
                errors.Add(
                    $"Rule code '{ruleCode}' is declared " +
                    "more than once.");
            }
        }

        return ruleIndex;
    }

    private static Dictionary<
        string,
        KnownProblemTypeDefinition>
        BuildDefinitionIndex(
            IEnumerable<KnownProblemTypeDefinition>
                definitions,
            ICollection<string> errors)
    {
        var definitionIndex =
            new Dictionary<
                string,
                KnownProblemTypeDefinition>(
                    StringComparer.OrdinalIgnoreCase);

        foreach (KnownProblemTypeDefinition definition
                 in definitions)
        {
            RegisterDefinitionIndexCode(
                definition.Code,
                definition,
                definitionIndex);

            foreach (string alternativeCode
                     in definition.AlternativeCodes)
            {
                RegisterDefinitionIndexCode(
                    alternativeCode,
                    definition,
                    definitionIndex);
            }
        }

        return definitionIndex;
    }

    private static void RegisterDefinitionIndexCode(
        string code,
        KnownProblemTypeDefinition definition,
        IDictionary<string, KnownProblemTypeDefinition>
            definitionIndex)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return;
        }

        string normalizedCode =
            code.Trim();

        if (!definitionIndex.ContainsKey(normalizedCode))
        {
            definitionIndex.Add(
                normalizedCode,
                definition);
        }
    }

    private static void ValidateReferencedRules(
        IEnumerable<KnownProblemTypeDefinition>
            definitions,
        IReadOnlyDictionary<
            string,
            KnownProblemRuleDefinition> ruleIndex,
        ICollection<string> errors)
    {
        foreach (KnownProblemTypeDefinition definition
                 in definitions)
        {
            var categoriesByRuleCode =
                new Dictionary<string, string>(
                    StringComparer.OrdinalIgnoreCase);

            ValidateRuleCategory(
                definition,
                "required",
                definition.RequiredRuleCodes,
                ruleIndex,
                categoriesByRuleCode,
                errors);

            ValidateRuleCategory(
                definition,
                "optional",
                definition.OptionalRuleCodes,
                ruleIndex,
                categoriesByRuleCode,
                errors);

            ValidateRuleCategory(
                definition,
                "extension",
                definition.ExtensionRuleCodes,
                ruleIndex,
                categoriesByRuleCode,
                errors);

            ValidateRuleCategory(
                definition,
                "exclusion",
                definition.ExclusionRuleCodes,
                ruleIndex,
                categoriesByRuleCode,
                errors);
        }
    }

    private static void ValidateRuleCategory(
        KnownProblemTypeDefinition definition,
        string categoryName,
        IEnumerable<string> ruleCodes,
        IReadOnlyDictionary<
            string,
            KnownProblemRuleDefinition> ruleIndex,
        IDictionary<string, string> categoriesByRuleCode,
        ICollection<string> errors)
    {
        var codesInCurrentCategory =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (string rawRuleCode
                 in ruleCodes)
        {
            string ruleCode =
                rawRuleCode?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(ruleCode))
            {
                errors.Add(
                    $"Problem-family definition " +
                    $"'{DisplayDefinitionCode(definition)}' " +
                    $"contains an empty {categoryName} " +
                    "rule code.");

                continue;
            }

            if (!codesInCurrentCategory.Add(ruleCode))
            {
                errors.Add(
                    $"Problem-family definition " +
                    $"'{DisplayDefinitionCode(definition)}' " +
                    $"contains rule '{ruleCode}' more than " +
                    $"once in its {categoryName} rules.");
            }

            if (!ruleIndex.TryGetValue(
                    ruleCode,
                    out KnownProblemRuleDefinition? rule))
            {
                errors.Add(
                    $"Problem-family definition " +
                    $"'{DisplayDefinitionCode(definition)}' " +
                    $"references missing {categoryName} rule " +
                    $"'{ruleCode}'.");

                continue;
            }

            if (!rule.IsEnabled)
            {
                errors.Add(
                    $"Problem-family definition " +
                    $"'{DisplayDefinitionCode(definition)}' " +
                    $"references disabled {categoryName} " +
                    $"rule '{ruleCode}'.");
            }

            if (categoriesByRuleCode.TryGetValue(
                    ruleCode,
                    out string? existingCategory))
            {
                if (!string.Equals(
                        existingCategory,
                        categoryName,
                        StringComparison.Ordinal))
                {
                    errors.Add(
                        $"Problem-family definition " +
                        $"'{DisplayDefinitionCode(definition)}' " +
                        $"uses rule '{ruleCode}' in both the " +
                        $"'{existingCategory}' and " +
                        $"'{categoryName}' categories.");
                }
            }
            else
            {
                categoriesByRuleCode.Add(
                    ruleCode,
                    categoryName);
            }
        }
    }

    private static void ValidateParentDefinitions(
        IEnumerable<KnownProblemTypeDefinition>
            definitions,
        IReadOnlyDictionary<
            string,
            KnownProblemTypeDefinition> definitionIndex,
        ICollection<string> errors)
    {
        KnownProblemTypeDefinition[] normalizedDefinitions =
            definitions.ToArray();

        foreach (KnownProblemTypeDefinition definition
                 in normalizedDefinitions)
        {
            if (!definition.HasParentProblemType)
            {
                continue;
            }

            if (!definitionIndex.TryGetValue(
                    definition.ParentProblemTypeCode,
                    out KnownProblemTypeDefinition?
                        parentDefinition))
            {
                errors.Add(
                    $"Problem-family definition " +
                    $"'{DisplayDefinitionCode(definition)}' " +
                    $"references missing parent family " +
                    $"'{definition.ParentProblemTypeCode}'.");

                continue;
            }

            if (ReferenceEquals(
                    definition,
                    parentDefinition))
            {
                errors.Add(
                    $"Problem-family definition " +
                    $"'{DisplayDefinitionCode(definition)}' " +
                    "cannot be its own parent.");
            }
        }

        var reportedCycles =
            new HashSet<string>(
                StringComparer.OrdinalIgnoreCase);

        foreach (KnownProblemTypeDefinition startDefinition
                 in normalizedDefinitions)
        {
            var positions =
                new Dictionary<
                    KnownProblemTypeDefinition,
                    int>();

            var path =
                new List<KnownProblemTypeDefinition>();

            KnownProblemTypeDefinition? currentDefinition =
                startDefinition;

            while (currentDefinition is not null)
            {
                if (positions.TryGetValue(
                        currentDefinition,
                        out int cycleStartIndex))
                {
                    string[] cycleCodes =
                        path
                            .Skip(cycleStartIndex)
                            .Select(
                                definition =>
                                    DisplayDefinitionCode(
                                        definition))
                            .Concat(
                                new[]
                                {
                                    DisplayDefinitionCode(
                                        currentDefinition)
                                })
                            .ToArray();

                    string cycleDescription =
                        string.Join(
                            " -> ",
                            cycleCodes);

                    if (reportedCycles.Add(cycleDescription))
                    {
                        errors.Add(
                            "The problem-family hierarchy " +
                            $"contains a cycle: " +
                            $"{cycleDescription}.");
                    }

                    break;
                }

                positions.Add(
                    currentDefinition,
                    path.Count);

                path.Add(currentDefinition);

                if (!currentDefinition
                        .HasParentProblemType)
                {
                    break;
                }

                if (!definitionIndex.TryGetValue(
                        currentDefinition
                            .ParentProblemTypeCode,
                        out KnownProblemTypeDefinition?
                            parentDefinition))
                {
                    break;
                }

                currentDefinition =
                    parentDefinition;
            }
        }
    }

    private static string DisplayDefinitionCode(
        KnownProblemTypeDefinition definition)
    {
        return string.IsNullOrWhiteSpace(definition.Code)
            ? "<missing code>"
            : definition.Code;
    }

    private static string DisplayRuleCode(
        KnownProblemRuleDefinition rule)
    {
        return string.IsNullOrWhiteSpace(rule.RuleCode)
            ? "<missing code>"
            : rule.RuleCode;
    }

    private void NotifyCatalogCollections()
    {
        OnPropertyChanged(nameof(Definitions));
        OnPropertyChanged(nameof(Rules));
        OnPropertyChanged(nameof(DefinitionCount));
        OnPropertyChanged(nameof(RuleCount));
        OnPropertyChanged(nameof(EnabledDefinitionCount));
        OnPropertyChanged(nameof(EnabledRuleCount));
        OnPropertyChanged(nameof(IsEmpty));

        NotifyValidationProperties();
    }

    private void NotifyValidationProperties()
    {
        OnPropertyChanged(nameof(HasCatalogName));
        OnPropertyChanged(nameof(HasCatalogVersion));
        OnPropertyChanged(nameof(IsValidCatalog));
        OnPropertyChanged(nameof(CanClassify));
    }
}