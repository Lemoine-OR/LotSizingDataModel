using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Classification;

namespace LotSizingDataModel.Instance.Recommendation;

/// <summary>
/// Represents an extensible catalog of solution methods that
/// may be evaluated for lot-sizing problem instances.
/// </summary>
/// <remarks>
/// A method catalog contains solver-independent descriptions
/// of algorithms, formulations, heuristics and decomposition
/// procedures.
///
/// The catalog does not execute the methods. It provides the
/// capabilities and limitations used by the solution-method
/// advisor to determine technical compatibility.
/// </remarks>
[Serializable]
[XmlRoot("solutionMethodCatalog")]
[XmlType(TypeName = "solutionMethodCatalog")]
public sealed class SolutionMethodCatalog : ModelObject
{
    private string _catalogName =
        string.Empty;

    private string _catalogVersion =
        string.Empty;

    private string _description =
        string.Empty;

    private bool _allowUnknownFeatureCodes;

    /// <summary>
    /// Initializes an empty solution-method catalog.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public SolutionMethodCatalog()
    {
    }

    /// <summary>
    /// Initializes a solution-method catalog.
    /// </summary>
    /// <param name="catalogName">
    /// Human-readable catalog name.
    /// </param>
    /// <param name="catalogVersion">
    /// Version of the catalog contents and semantics.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="catalogName"/> or
    /// <paramref name="catalogVersion"/> is empty.
    /// </exception>
    public SolutionMethodCatalog(
        string catalogName,
        string catalogVersion)
    {
        if (string.IsNullOrWhiteSpace(catalogName))
        {
            throw new ArgumentException(
                "A solution-method catalog name is required.",
                nameof(catalogName));
        }

        if (string.IsNullOrWhiteSpace(catalogVersion))
        {
            throw new ArgumentException(
                "A solution-method catalog version is required.",
                nameof(catalogVersion));
        }

        CatalogName =
            catalogName;

        CatalogVersion =
            catalogVersion;
    }

    /// <summary>
    /// Gets or sets the human-readable name of the catalog.
    /// </summary>
    [XmlAttribute("catalogName")]
    public string CatalogName
    {
        get => _catalogName;
        set
        {
            if (SetProperty(
                    ref _catalogName,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasCatalogName));

                NotifyCatalogValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the catalog contents and
    /// compatibility semantics.
    /// </summary>
    [XmlAttribute("catalogVersion")]
    public string CatalogVersion
    {
        get => _catalogVersion;
        set
        {
            if (SetProperty(
                    ref _catalogVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasCatalogVersion));

                NotifyCatalogValidityProperties();
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
        set
        {
            if (SetProperty(
                    ref _description,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasDescription));
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether method
    /// definitions may reference feature codes that are not
    /// standard Boolean properties of
    /// <see cref="LotSizingProblemFeatures"/>.
    /// </summary>
    /// <remarks>
    /// The default value is <see langword="false"/>.
    ///
    /// Set this property to <see langword="true"/> when the
    /// catalog is intended to be used with a customized
    /// feature extractor or an extended advisor.
    /// </remarks>
    [XmlAttribute("allowUnknownFeatureCodes")]
    public bool AllowUnknownFeatureCodes
    {
        get => _allowUnknownFeatureCodes;
        set
        {
            if (SetProperty(
                    ref _allowUnknownFeatureCodes,
                    value))
            {
                NotifyCatalogValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets the solution-method definitions contained in the
    /// catalog.
    /// </summary>
    [XmlArray("methods")]
    [XmlArrayItem("method")]
    public List<SolutionMethodDefinition> Methods { get; } =
        new();

    /// <summary>
    /// Gets a value indicating whether the catalog has a
    /// human-readable name.
    /// </summary>
    [XmlIgnore]
    public bool HasCatalogName =>
        !string.IsNullOrWhiteSpace(
            CatalogName);

    /// <summary>
    /// Gets a value indicating whether the catalog has a
    /// version.
    /// </summary>
    [XmlIgnore]
    public bool HasCatalogVersion =>
        !string.IsNullOrWhiteSpace(
            CatalogVersion);

    /// <summary>
    /// Gets a value indicating whether the catalog has a
    /// description.
    /// </summary>
    [XmlIgnore]
    public bool HasDescription =>
        !string.IsNullOrWhiteSpace(
            Description);

    /// <summary>
    /// Gets a value indicating whether the catalog contains
    /// at least one method definition.
    /// </summary>
    [XmlIgnore]
    public bool HasMethods =>
        Methods.Count > 0;

    /// <summary>
    /// Gets the number of method definitions contained in the
    /// catalog.
    /// </summary>
    [XmlIgnore]
    public int MethodCount =>
        Methods.Count;

    /// <summary>
    /// Gets the number of enabled method definitions.
    /// </summary>
    [XmlIgnore]
    public int EnabledMethodCount =>
        Methods.Count(
            method =>
                method is not null &&
                method.IsEnabled);

    /// <summary>
    /// Gets the number of structurally valid method
    /// definitions.
    /// </summary>
    [XmlIgnore]
    public int ValidMethodCount =>
        Methods.Count(
            method =>
                method is not null &&
                method.IsValidDefinition);

    /// <summary>
    /// Gets the number of methods that may currently be
    /// evaluated by the method advisor.
    /// </summary>
    [XmlIgnore]
    public int EvaluableMethodCount =>
        Methods.Count(
            method =>
                method is not null &&
                method.CanBeEvaluated);

    /// <summary>
    /// Gets a value indicating whether the catalog is
    /// structurally valid.
    /// </summary>
    [XmlIgnore]
    public bool IsValidCatalog =>
        Validate().Count == 0;

    /// <summary>
    /// Gets a value indicating whether the catalog can be
    /// used by the solution-method advisor.
    /// </summary>
    [XmlIgnore]
    public bool CanRecommend =>
        IsValidCatalog &&
        EvaluableMethodCount > 0;

    /// <summary>
    /// Adds a solution-method definition to the catalog.
    /// </summary>
    /// <param name="method">
    /// Method definition to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="method"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the method has no stable code or when the
    /// code is already used by another method.
    /// </exception>
    public void AddMethod(
        SolutionMethodDefinition method)
    {
        ArgumentNullException.ThrowIfNull(
            method);

        if (!method.HasMethodCode)
        {
            throw new ArgumentException(
                "The solution method must have a stable code.",
                nameof(method));
        }

        if (ContainsMethodCode(
                method.MethodCode))
        {
            throw new ArgumentException(
                $"Solution-method code " +
                $"'{method.MethodCode}' is already used.",
                nameof(method));
        }

        Methods.Add(
            method);

        NotifyMethodCollectionProperties();
    }

    /// <summary>
    /// Replaces all solution-method definitions in the
    /// catalog.
    /// </summary>
    /// <param name="methods">
    /// New collection of method definitions.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="methods"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null definition,
    /// a method without a stable code or duplicate method
    /// codes.
    /// </exception>
    public void ReplaceMethods(
        IEnumerable<SolutionMethodDefinition> methods)
    {
        ArgumentNullException.ThrowIfNull(
            methods);

        SolutionMethodDefinition[] normalizedMethods =
            methods.ToArray();

        if (normalizedMethods.Any(
                method =>
                    method is null))
        {
            throw new ArgumentException(
                "The method collection cannot contain a null " +
                "definition.",
                nameof(methods));
        }

        if (normalizedMethods.Any(
                method =>
                    !method.HasMethodCode))
        {
            throw new ArgumentException(
                "Every solution method must have a stable " +
                "code.",
                nameof(methods));
        }

        string[] duplicateCodes =
            normalizedMethods
                .GroupBy(
                    method =>
                        method.MethodCode,
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

        if (duplicateCodes.Length > 0)
        {
            throw new ArgumentException(
                "Duplicate solution-method codes: " +
                string.Join(
                    ", ",
                    duplicateCodes) +
                ".",
                nameof(methods));
        }

        Methods.Clear();

        Methods.AddRange(
            normalizedMethods);

        NotifyMethodCollectionProperties();
    }

    /// <summary>
    /// Finds a solution method from its stable code.
    /// </summary>
    /// <param name="methodCode">
    /// Method code to search for.
    /// </param>
    /// <returns>
    /// Matching method definition, or
    /// <see langword="null"/> when no method uses the supplied
    /// code.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when several methods use the supplied code.
    /// </exception>
    public SolutionMethodDefinition? FindMethod(
        string methodCode)
    {
        if (string.IsNullOrWhiteSpace(methodCode))
        {
            return null;
        }

        string normalizedCode =
            methodCode.Trim();

        SolutionMethodDefinition[] matches =
            Methods
                .Where(
                    method =>
                        method is not null &&
                        string.Equals(
                            method.MethodCode,
                            normalizedCode,
                            StringComparison.OrdinalIgnoreCase))
                .Take(2)
                .ToArray();

        return matches.Length switch
        {
            0 =>
                null,

            1 =>
                matches[0],

            _ =>
                throw new InvalidOperationException(
                    $"Solution-method code '{methodCode}' is " +
                    "ambiguous in the current catalog.")
        };
    }

    /// <summary>
    /// Determines whether the catalog contains a method with
    /// the supplied code.
    /// </summary>
    /// <param name="methodCode">
    /// Method code to search for.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the code exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ContainsMethodCode(
        string methodCode)
    {
        return FindMethod(
            methodCode) is not null;
    }

    /// <summary>
    /// Removes a solution method from the catalog.
    /// </summary>
    /// <param name="methodCode">
    /// Code of the method to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a method was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveMethod(
        string methodCode)
    {
        SolutionMethodDefinition? method =
            FindMethod(
                methodCode);

        if (method is null)
        {
            return false;
        }

        bool removed =
            Methods.Remove(
                method);

        if (removed)
        {
            NotifyMethodCollectionProperties();
        }

        return removed;
    }

    /// <summary>
    /// Removes all method definitions from the catalog.
    /// </summary>
    public void ClearMethods()
    {
        if (Methods.Count == 0)
        {
            return;
        }

        Methods.Clear();

        NotifyMethodCollectionProperties();
    }

    /// <summary>
    /// Gets the enabled method definitions in deterministic
    /// priority order.
    /// </summary>
    /// <returns>
    /// Enabled method definitions ordered by decreasing
    /// priority, then by name and method code.
    /// </returns>
    /// <remarks>
    /// This method may return enabled definitions that are
    /// structurally invalid. Use
    /// <see cref="GetMethodsForEvaluation"/> to retrieve only
    /// definitions that can be evaluated.
    /// </remarks>
    public IReadOnlyList<SolutionMethodDefinition>
        GetEnabledMethods()
    {
        return Methods
            .Where(
                method =>
                    method is not null &&
                    method.IsEnabled)
            .OrderByDescending(
                method =>
                    method.Priority)
            .ThenBy(
                method =>
                    method.Name,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(
                method =>
                    method.MethodCode,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Gets the method definitions that can be evaluated by
    /// the solution-method advisor.
    /// </summary>
    /// <returns>
    /// Enabled and structurally valid method definitions in
    /// deterministic priority order.
    /// </returns>
    public IReadOnlyList<SolutionMethodDefinition>
        GetMethodsForEvaluation()
    {
        return Methods
            .Where(
                method =>
                    method is not null &&
                    method.CanBeEvaluated)
            .OrderByDescending(
                method =>
                    method.Priority)
            .ThenBy(
                method =>
                    method.Name,
                StringComparer.OrdinalIgnoreCase)
            .ThenBy(
                method =>
                    method.MethodCode,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Gets all standard Boolean feature codes that may be
    /// referenced by a solution-method definition.
    /// </summary>
    /// <returns>
    /// Ordered collection of public Boolean property names
    /// defined by <see cref="LotSizingProblemFeatures"/>.
    /// </returns>
    public static IReadOnlyList<string>
        GetKnownBooleanFeatureCodes()
    {
        return typeof(LotSizingProblemFeatures)
            .GetProperties(
                BindingFlags.Instance |
                BindingFlags.Public)
            .Where(
                property =>
                    property.CanRead &&
                    property.GetIndexParameters().Length == 0 &&
                    property.PropertyType ==
                        typeof(bool))
            .Select(
                property =>
                    property.Name)
            .OrderBy(
                featureCode =>
                    featureCode,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Validates the structural consistency of the catalog and
    /// its method definitions.
    /// </summary>
    /// <returns>
    /// Ordered validation-error collection. An empty
    /// collection indicates that the catalog is valid.
    /// </returns>
    public IReadOnlyList<string> Validate()
    {
        var errors =
            new List<string>();

        if (!HasCatalogName)
        {
            errors.Add(
                "The solution-method catalog name is missing.");
        }

        if (!HasCatalogVersion)
        {
            errors.Add(
                "The solution-method catalog version is " +
                "missing.");
        }

        if (!HasMethods)
        {
            errors.Add(
                "The solution-method catalog does not contain " +
                "any method definition.");
        }

        ValidateMethodDefinitions(
            errors);

        ValidateMethodCodeUniqueness(
            errors);

        if (!AllowUnknownFeatureCodes)
        {
            ValidateReferencedFeatureCodes(
                errors);
        }

        return errors
            .Where(
                error =>
                    !string.IsNullOrWhiteSpace(error))
            .Select(
                error =>
                    error.Trim())
            .Distinct(
                StringComparer.Ordinal)
            .OrderBy(
                error =>
                    error,
                StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Validates the catalog and throws an exception when at
    /// least one error is found.
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
            "The solution-method catalog is invalid:" +
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
            $"{MethodCount} method(s); " +
            $"{EvaluableMethodCount} evaluable";
    }

    private void ValidateMethodDefinitions(
        ICollection<string> errors)
    {
        for (int index = 0;
             index < Methods.Count;
             index++)
        {
            SolutionMethodDefinition? method =
                Methods[index];

            if (method is null)
            {
                errors.Add(
                    $"Solution-method definition at index " +
                    $"{index} is null.");

                continue;
            }

            IReadOnlyList<string> methodErrors =
                method.Validate();

            foreach (string methodError in methodErrors)
            {
                errors.Add(
                    $"Method '{DisplayMethodCode(method)}': " +
                    methodError);
            }
        }
    }

    private void ValidateMethodCodeUniqueness(
        ICollection<string> errors)
    {
        string[] missingCodeIndexes =
            Methods
                .Select(
                    (method, index) =>
                        new
                        {
                            Method =
                                method,

                            Index =
                                index
                        })
                .Where(
                    entry =>
                        entry.Method is not null &&
                        !entry.Method.HasMethodCode)
                .Select(
                    entry =>
                        entry.Index.ToString(
                            System.Globalization
                                .CultureInfo.InvariantCulture))
                .ToArray();

        if (missingCodeIndexes.Length > 0)
        {
            errors.Add(
                "Method definitions without a stable code " +
                "exist at indexes: " +
                string.Join(
                    ", ",
                    missingCodeIndexes) +
                ".");
        }

        string[] duplicateMethodCodes =
            Methods
                .Where(
                    method =>
                        method is not null &&
                        method.HasMethodCode)
                .GroupBy(
                    method =>
                        method.MethodCode,
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    methodCode =>
                        methodCode,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (duplicateMethodCodes.Length > 0)
        {
            errors.Add(
                "Duplicate solution-method codes: " +
                string.Join(
                    ", ",
                    duplicateMethodCodes) +
                ".");
        }
    }

    private void ValidateReferencedFeatureCodes(
        ICollection<string> errors)
    {
        HashSet<string> knownBooleanFeatureCodes =
            GetKnownBooleanFeatureCodes()
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        foreach (SolutionMethodDefinition method
                 in Methods.Where(
                     method =>
                         method is not null))
        {
            string[] unknownFeatureCodes =
                method
                    .GetAllFeatureCodes()
                    .Where(
                        featureCode =>
                            !knownBooleanFeatureCodes.Contains(
                                featureCode))
                    .OrderBy(
                        featureCode =>
                            featureCode,
                        StringComparer.OrdinalIgnoreCase)
                    .ToArray();

            if (unknownFeatureCodes.Length == 0)
            {
                continue;
            }

            errors.Add(
                $"Method '{DisplayMethodCode(method)}' " +
                "references feature codes that are not " +
                "public Boolean properties of " +
                $"{nameof(LotSizingProblemFeatures)}: " +
                string.Join(
                    ", ",
                    unknownFeatureCodes) +
                ".");
        }
    }

    private static string DisplayMethodCode(
        SolutionMethodDefinition method)
    {
        return method.HasMethodCode
            ? method.MethodCode
            : "<missing code>";
    }

    private void NotifyMethodCollectionProperties()
    {
        OnPropertyChanged(
            nameof(Methods));

        OnPropertyChanged(
            nameof(HasMethods));

        OnPropertyChanged(
            nameof(MethodCount));

        OnPropertyChanged(
            nameof(EnabledMethodCount));

        OnPropertyChanged(
            nameof(ValidMethodCount));

        OnPropertyChanged(
            nameof(EvaluableMethodCount));

        NotifyCatalogValidityProperties();
    }

    private void NotifyCatalogValidityProperties()
    {
        OnPropertyChanged(
            nameof(IsValidCatalog));

        OnPropertyChanged(
            nameof(CanRecommend));
    }
}