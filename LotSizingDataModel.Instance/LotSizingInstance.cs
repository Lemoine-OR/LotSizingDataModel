using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Metadata;
using LotSizingDataModel.Instance.Results;

namespace LotSizingDataModel.Instance;

/// <summary>
/// Represents a complete lot-sizing problem instance,
/// including its supply-chain data, metadata, structural
/// analysis, automatic classification and known results.
/// </summary>
/// <remarks>
/// The supply chain contains the source problem data.
///
/// Product-structure information, problem classification and
/// solution-method recommendations are derived information.
/// They may therefore become outdated when the supply chain is
/// replaced or modified.
/// </remarks>
[Serializable]
[XmlRoot("lotSizingInstance")]
[XmlType(TypeName = "lotSizingInstance")]
public sealed partial class LotSizingInstance : ModelObject
{
    private string _instanceId =
        Guid.NewGuid().ToString("D");

    private string _name =
        string.Empty;

    private string _description =
        string.Empty;

    private string _sourceInformation =
        string.Empty;

    private string _createdBy =
        string.Empty;

    private string _formatVersion =
        "1.0";

    private DateTime? _createdAtUtc =
        DateTime.UtcNow;

    private DateTime? _modifiedAtUtc =
        DateTime.UtcNow;

    private SupplyChain _supplyChain =
        new();

    private ProductStructureDescriptor
        _productStructure =
            new();

    private LotSizingProblemClassification
        _problemClassification =
            new();

    private string _bestKnownResultId =
        string.Empty;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty lot-sizing problem instance.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public LotSizingInstance()
    {
    }

    /// <summary>
    /// Initializes a lot-sizing problem instance from a
    /// supply-chain model.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain model represented by the instance.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplyChain"/> is
    /// <see langword="null"/>.
    /// </exception>
    public LotSizingInstance(
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(
            supplyChain);

        _supplyChain =
            supplyChain;
    }

    /// <summary>
    /// Initializes a named lot-sizing problem instance.
    /// </summary>
    /// <param name="supplyChain">
    /// Supply-chain model represented by the instance.
    /// </param>
    /// <param name="name">
    /// Human-readable name of the instance.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplyChain"/> is
    /// <see langword="null"/>.
    /// </exception>
    public LotSizingInstance(
        SupplyChain supplyChain,
        string name)
        : this(
            Guid.NewGuid().ToString("D"),
            supplyChain,
            name)
    {
    }

    /// <summary>
    /// Initializes an identified lot-sizing problem instance.
    /// </summary>
    /// <param name="instanceId">
    /// Stable identifier of the instance.
    /// </param>
    /// <param name="supplyChain">
    /// Supply-chain model represented by the instance.
    /// </param>
    /// <param name="name">
    /// Optional human-readable name of the instance.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="instanceId"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplyChain"/> is
    /// <see langword="null"/>.
    /// </exception>
    public LotSizingInstance(
        string instanceId,
        SupplyChain supplyChain,
        string name = "")
    {
        if (string.IsNullOrWhiteSpace(instanceId))
        {
            throw new ArgumentException(
                "A lot-sizing instance identifier is required.",
                nameof(instanceId));
        }

        ArgumentNullException.ThrowIfNull(
            supplyChain);

        _instanceId =
            instanceId.Trim();

        _supplyChain =
            supplyChain;

        _name =
            name?.Trim() ?? string.Empty;

        DateTime currentDateUtc =
            DateTime.UtcNow;

        _createdAtUtc =
            currentDateUtc;

        _modifiedAtUtc =
            currentDateUtc;
    }

    /// <summary>
    /// Gets or sets the stable identifier of the instance.
    /// </summary>
    /// <remarks>
    /// The identifier distinguishes the instance from other
    /// instances independently from its human-readable name.
    /// </remarks>
    [XmlAttribute("instanceId")]
    public string InstanceId
    {
        get => _instanceId;

        set
        {
            if (SetProperty(
                    ref _instanceId,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasInstanceId));
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable name of the instance.
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
                OnPropertyChanged(
                    nameof(HasName));
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable description of the
    /// problem instance.
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
    /// Gets or sets information identifying the source of the
    /// instance.
    /// </summary>
    /// <remarks>
    /// Examples include a publication, benchmark collection,
    /// industrial data set or instance generator.
    /// </remarks>
    [XmlElement("sourceInformation")]
    public string SourceInformation
    {
        get => _sourceInformation;

        set
        {
            if (SetProperty(
                    ref _sourceInformation,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasSourceInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the person, organization or software
    /// component that created the instance.
    /// </summary>
    [XmlAttribute("createdBy")]
    public string CreatedBy
    {
        get => _createdBy;

        set
        {
            if (SetProperty(
                    ref _createdBy,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasCreatedBy));
            }
        }
    }

    /// <summary>
    /// Gets or sets the format version of the serialized
    /// instance.
    /// </summary>
    [XmlAttribute("formatVersion")]
    public string FormatVersion
    {
        get => _formatVersion;

        set
        {
            if (SetProperty(
                    ref _formatVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasFormatVersion));
            }
        }
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the
    /// instance was created.
    /// </summary>
    [XmlElement("createdAtUtc", IsNullable = true)]
    public DateTime? CreatedAtUtc
    {
        get => _createdAtUtc;

        set
        {
            if (SetProperty(
                    ref _createdAtUtc,
                    ConvertToUtc(value)))
            {
                OnPropertyChanged(
                    nameof(HasCreationDate));
            }
        }
    }

    /// <summary>
    /// Gets or sets the UTC date and time at which the
    /// instance was last modified.
    /// </summary>
    [XmlElement("modifiedAtUtc", IsNullable = true)]
    public DateTime? ModifiedAtUtc
    {
        get => _modifiedAtUtc;

        set
        {
            if (SetProperty(
                    ref _modifiedAtUtc,
                    ConvertToUtc(value)))
            {
                OnPropertyChanged(
                    nameof(HasModificationDate));

                OnPropertyChanged(
                    nameof(LastModifiedAtUtc));
            }
        }
    }

    /// <summary>
    /// Gets the last-modification date of the instance.
    /// </summary>
    [XmlIgnore]
    public DateTime? LastModifiedAtUtc =>
        ModifiedAtUtc;

    /// <summary>
    /// Gets or sets the supply-chain model represented by the
    /// instance.
    /// </summary>
    /// <remarks>
    /// The property setter is primarily required for XML
    /// serialization.
    ///
    /// Application code should normally use
    /// <see cref="ReplaceSupplyChain"/> so that derived
    /// analyses and recommendations are invalidated.
    /// </remarks>
    [XmlElement("supplyChain")]
    public SupplyChain SupplyChain
    {
        get => _supplyChain;

        set
        {
            SupplyChain normalizedValue =
                value ??
                new SupplyChain();

            if (SetProperty(
                    ref _supplyChain,
                    normalizedValue))
            {
                OnPropertyChanged(
                    nameof(PlanningHorizon));
            }
        }
    }

    /// <summary>
    /// Gets or sets the product-structure descriptor
    /// associated with the supply-chain model.
    /// </summary>
    [XmlElement("productStructure")]
    public ProductStructureDescriptor ProductStructure
    {
        get => _productStructure;

        set
        {
            ProductStructureDescriptor normalizedValue =
                value ??
                new ProductStructureDescriptor();

            SetProperty(
                ref _productStructure,
                normalizedValue);
        }
    }

    /// <summary>
    /// Gets or sets the automatic lot-sizing problem
    /// classification.
    /// </summary>
    [XmlElement("problemClassification")]
    public LotSizingProblemClassification
        ProblemClassification
    {
        get =>
            _problemClassification;

        set
        {
            LotSizingProblemClassification normalizedValue =
                value ??
                new LotSizingProblemClassification();

            SetProperty(
                ref _problemClassification,
                normalizedValue);
        }
    }

    /// <summary>
    /// Gets the known results associated with the instance.
    /// </summary>
    /// <remarks>
    /// A known result may contain an objective value, a
    /// detailed solution, or both.
    /// </remarks>
    [XmlArray("knownResults")]
    [XmlArrayItem("knownResult")]
    public List<KnownResult> KnownResults { get; } =
        new();

    /// <summary>
    /// Gets or sets the identifier of the result currently
    /// considered to be the best known result.
    /// </summary>
    /// <remarks>
    /// An empty value means that no best known result has been
    /// selected.
    /// </remarks>
    [XmlAttribute("bestKnownResultId")]
    public string BestKnownResultId
    {
        get => _bestKnownResultId;

        set
        {
            if (SetProperty(
                    ref _bestKnownResultId,
                    value?.Trim() ?? string.Empty))
            {
                NotifyBestKnownResultProperties();
            }
        }
    }

    /// <summary>
    /// Gets the descriptive tags associated with the
    /// instance.
    /// </summary>
    [XmlArray("tags")]
    [XmlArrayItem("tag")]
    public List<string> Tags { get; } =
        new();

    /// <summary>
    /// Gets or sets an optional explanatory comment.
    /// </summary>
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
    /// Gets a value indicating whether the instance has a
    /// stable identifier.
    /// </summary>
    [XmlIgnore]
    public bool HasInstanceId =>
        !string.IsNullOrWhiteSpace(
            InstanceId);

    /// <summary>
    /// Gets a value indicating whether the instance has a
    /// human-readable name.
    /// </summary>
    [XmlIgnore]
    public bool HasName =>
        !string.IsNullOrWhiteSpace(
            Name);

    /// <summary>
    /// Gets a value indicating whether a description has been
    /// recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasDescription =>
        !string.IsNullOrWhiteSpace(
            Description);

    /// <summary>
    /// Gets a value indicating whether source information has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasSourceInformation =>
        !string.IsNullOrWhiteSpace(
            SourceInformation);

    /// <summary>
    /// Gets a value indicating whether creator information has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasCreatedBy =>
        !string.IsNullOrWhiteSpace(
            CreatedBy);

    /// <summary>
    /// Gets a value indicating whether a format version has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasFormatVersion =>
        !string.IsNullOrWhiteSpace(
            FormatVersion);

    /// <summary>
    /// Gets a value indicating whether a creation date has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasCreationDate =>
        CreatedAtUtc.HasValue;

    /// <summary>
    /// Gets a value indicating whether a modification date has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasModificationDate =>
        ModifiedAtUtc.HasValue;

    /// <summary>
    /// Gets the planning horizon of the supply-chain model.
    /// </summary>
    [XmlIgnore]
    public int PlanningHorizon =>
        SupplyChain.PlanningHorizon;

    /// <summary>
    /// Gets a value indicating whether at least one known
    /// result has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasKnownResults =>
        KnownResults.Count > 0;

    /// <summary>
    /// Gets the number of known results associated with the
    /// instance.
    /// </summary>
    [XmlIgnore]
    public int KnownResultCount =>
        KnownResults.Count;

    /// <summary>
    /// Gets a value indicating whether a best known result
    /// identifier has been selected.
    /// </summary>
    [XmlIgnore]
    public bool HasBestKnownResultId =>
        !string.IsNullOrWhiteSpace(
            BestKnownResultId);

    /// <summary>
    /// Gets the result selected as the best known result.
    /// </summary>
    /// <remarks>
    /// The property returns <see langword="null"/> when no
    /// result has been selected or when the referenced result
    /// cannot be found.
    /// </remarks>
    [XmlIgnore]
    public KnownResult? BestKnownResult =>
        FindKnownResult(
            BestKnownResultId);

    /// <summary>
    /// Gets a value indicating whether the selected best known
    /// result exists in <see cref="KnownResults"/>.
    /// </summary>
    [XmlIgnore]
    public bool HasBestKnownResult =>
        BestKnownResult is not null;

    /// <summary>
    /// Gets a value indicating whether the selected best-known
    /// result is eligible for that role.
    /// </summary>
    [XmlIgnore]
    public bool HasEligibleBestKnownResult =>
        BestKnownResult?.CanBeSelectedAsBestKnownResult ==
        true;

    /// <summary>
    /// Gets a value indicating whether at least one tag has
    /// been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasTags =>
        Tags.Count > 0;

    /// <summary>
    /// Gets a value indicating whether an explanatory comment
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    /// <summary>
    /// Adds a known result to the instance.
    /// </summary>
    /// <param name="knownResult">
    /// Known result to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="knownResult"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the result has no identifier or another
    /// result already uses the same identifier.
    /// </exception>
    public void AddKnownResult(
        KnownResult knownResult)
    {
        ArgumentNullException.ThrowIfNull(
            knownResult);

        if (string.IsNullOrWhiteSpace(
                knownResult.ResultId))
        {
            throw new ArgumentException(
                "The known result must have a result " +
                "identifier.",
                nameof(knownResult));
        }

        if (ContainsKnownResultId(
                knownResult.ResultId))
        {
            throw new ArgumentException(
                $"Known-result identifier " +
                $"'{knownResult.ResultId}' is already used.",
                nameof(knownResult));
        }

        KnownResults.Add(
            knownResult);

        NotifyKnownResultProperties();

        Touch();
    }

    /// <summary>
    /// Replaces all known results associated with the
    /// instance.
    /// </summary>
    /// <param name="knownResults">
    /// New known-result collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="knownResults"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when the collection contains a null result, a
    /// missing identifier or duplicate identifiers.
    /// </exception>
    public void ReplaceKnownResults(
        IEnumerable<KnownResult> knownResults)
    {
        ArgumentNullException.ThrowIfNull(
            knownResults);

        KnownResult[] materializedResults =
            knownResults.ToArray();

        if (materializedResults.Any(
                result =>
                    result is null))
        {
            throw new ArgumentException(
                "The known-result collection cannot contain " +
                "a null element.",
                nameof(knownResults));
        }

        KnownResult? resultWithoutIdentifier =
            materializedResults.FirstOrDefault(
                result =>
                    string.IsNullOrWhiteSpace(
                        result.ResultId));

        if (resultWithoutIdentifier is not null)
        {
            throw new ArgumentException(
                "Every known result must have a result " +
                "identifier.",
                nameof(knownResults));
        }

        string[] duplicateIdentifiers =
            materializedResults
                .GroupBy(
                    result =>
                        result.ResultId,
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    identifier =>
                        identifier,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (duplicateIdentifiers.Length > 0)
        {
            throw new ArgumentException(
                "Duplicate known-result identifiers: " +
                string.Join(
                    ", ",
                    duplicateIdentifiers) +
                ".",
                nameof(knownResults));
        }

        KnownResults.Clear();

        KnownResults.AddRange(
            materializedResults);

        if (HasBestKnownResultId &&
            !ContainsKnownResultId(
                BestKnownResultId))
        {
            BestKnownResultId =
                string.Empty;
        }

        NotifyKnownResultProperties();

        Touch();
    }

    /// <summary>
    /// Finds a known result from its identifier.
    /// </summary>
    /// <param name="resultId">
    /// Result identifier to search for.
    /// </param>
    /// <returns>
    /// Matching known result, or <see langword="null"/> when
    /// no result uses the supplied identifier.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when several results use the same identifier.
    /// </exception>
    public KnownResult? FindKnownResult(
        string resultId)
    {
        if (string.IsNullOrWhiteSpace(
                resultId))
        {
            return null;
        }

        string normalizedResultId =
            resultId.Trim();

        KnownResult[] matches =
            KnownResults
                .Where(
                    result =>
                        result is not null &&
                        string.Equals(
                            result.ResultId,
                            normalizedResultId,
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
                    $"Known-result identifier '{resultId}' " +
                    "is ambiguous in the current instance.")
        };
    }

    /// <summary>
    /// Determines whether a known result uses the supplied
    /// identifier.
    /// </summary>
    /// <param name="resultId">
    /// Result identifier to search for.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the identifier exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ContainsKnownResultId(
        string resultId)
    {
        return FindKnownResult(
            resultId) is not null;
    }

    /// <summary>
    /// Removes a known result from the instance.
    /// </summary>
    /// <param name="resultId">
    /// Identifier of the result to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a result was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveKnownResult(
        string resultId)
    {
        KnownResult? knownResult =
            FindKnownResult(
                resultId);

        if (knownResult is null)
        {
            return false;
        }

        bool removed =
            KnownResults.Remove(
                knownResult);

        if (!removed)
        {
            return false;
        }

        if (string.Equals(
                BestKnownResultId,
                knownResult.ResultId,
                StringComparison.OrdinalIgnoreCase))
        {
            BestKnownResultId =
                string.Empty;
        }

        NotifyKnownResultProperties();

        Touch();

        return true;
    }

    /// <summary>
    /// Removes every known result from the instance.
    /// </summary>
    public void ClearKnownResults()
    {
        if (KnownResults.Count == 0 &&
            !HasBestKnownResultId)
        {
            return;
        }

        KnownResults.Clear();

        BestKnownResultId =
            string.Empty;

        NotifyKnownResultProperties();

        Touch();
    }

    /// <summary>
    /// Selects the best known result from its identifier.
    /// </summary>
    /// <param name="resultId">
    /// Identifier of the result to select.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when the identifier is empty.
    /// </exception>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when no known result uses the supplied
    /// identifier.
    /// </exception>
    public void SetBestKnownResult(
        string resultId)
    {
        if (string.IsNullOrWhiteSpace(
                resultId))
        {
            throw new ArgumentException(
                "A best-known-result identifier is required.",
                nameof(resultId));
        }

        KnownResult? knownResult =
            FindKnownResult(
                resultId);

        if (knownResult is null)
        {
            throw new KeyNotFoundException(
                $"Known result '{resultId}' does not exist.");
        }

        BestKnownResultId =
            knownResult.ResultId;

        Touch();
    }

    /// <summary>
    /// Selects a known result as the best known result.
    /// </summary>
    /// <param name="knownResult">
    /// Known result to select.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="knownResult"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void SetBestKnownResult(
        KnownResult knownResult)
    {
        ArgumentNullException.ThrowIfNull(
            knownResult);

        SetBestKnownResult(
            knownResult.ResultId);
    }

    /// <summary>
    /// Clears the best-known-result selection.
    /// </summary>
    public void ClearBestKnownResult()
    {
        if (!HasBestKnownResultId)
        {
            return;
        }

        BestKnownResultId =
            string.Empty;

        Touch();
    }

    /// <summary>
    /// Replaces the supply-chain model and invalidates all
    /// derived analysis and recommendation information.
    /// </summary>
    /// <param name="supplyChain">
    /// New supply-chain model.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="supplyChain"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void ReplaceSupplyChain(
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(
            supplyChain);

        SupplyChain =
            supplyChain;

        ProductStructure
            .MarkAsOutdated();

        ProblemClassification
            .MarkAsOutdated();

        ClearSolutionMethodRecommendationReport();

        Touch();
    }

    /// <summary>
    /// Clears derived product-structure, classification and
    /// solution-method recommendation information.
    /// </summary>
    /// <remarks>
    /// The supply-chain data and known results are preserved.
    /// </remarks>
    public void ClearDerivedAnalysis()
    {
        ProductStructure
            .ClearDetectedAnalysis();

        ProblemClassification
            .ClearClassificationResult();

        ClearSolutionMethodRecommendationReport();

        Touch();
    }

    /// <summary>
    /// Clears derived product-structure, classification and
    /// recommendation information.
    /// </summary>
    /// <remarks>
    /// This method is an alias of
    /// <see cref="ClearDerivedAnalysis"/>.
    /// </remarks>
    public void ClearAnalysis()
    {
        ClearDerivedAnalysis();
    }

    /// <summary>
    /// Adds a descriptive tag to the instance.
    /// </summary>
    /// <param name="tag">
    /// Tag to add.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the tag was added;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="tag"/> is empty.
    /// </exception>
    public bool AddTag(
        string tag)
    {
        if (string.IsNullOrWhiteSpace(
                tag))
        {
            throw new ArgumentException(
                "A non-empty tag is required.",
                nameof(tag));
        }

        string normalizedTag =
            tag.Trim();

        if (Tags.Contains(
                normalizedTag,
                StringComparer.OrdinalIgnoreCase))
        {
            return false;
        }

        Tags.Add(
            normalizedTag);

        SortTags();

        NotifyTagProperties();

        Touch();

        return true;
    }

    /// <summary>
    /// Removes a descriptive tag from the instance.
    /// </summary>
    /// <param name="tag">
    /// Tag to remove.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the tag was removed;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool RemoveTag(
        string tag)
    {
        if (string.IsNullOrWhiteSpace(
                tag))
        {
            return false;
        }

        string? existingTag =
            Tags.FirstOrDefault(
                candidate =>
                    string.Equals(
                        candidate,
                        tag.Trim(),
                        StringComparison.OrdinalIgnoreCase));

        if (existingTag is null)
        {
            return false;
        }

        bool removed =
            Tags.Remove(
                existingTag);

        if (removed)
        {
            NotifyTagProperties();

            Touch();
        }

        return removed;
    }

    /// <summary>
    /// Replaces every descriptive tag associated with the
    /// instance.
    /// </summary>
    /// <param name="tags">
    /// New tag collection.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="tags"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void ReplaceTags(
        IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(
            tags);

        string[] normalizedTags =
            tags
                .Where(
                    tag =>
                        !string.IsNullOrWhiteSpace(
                            tag))
                .Select(
                    tag =>
                        tag.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    tag =>
                        tag,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        Tags.Clear();

        Tags.AddRange(
            normalizedTags);

        NotifyTagProperties();

        Touch();
    }

    /// <summary>
    /// Removes every descriptive tag from the instance.
    /// </summary>
    public void ClearTags()
    {
        if (Tags.Count == 0)
        {
            return;
        }

        Tags.Clear();

        NotifyTagProperties();

        Touch();
    }

    /// <summary>
    /// Updates the last-modification date of the instance.
    /// </summary>
    public void Touch()
    {
        ModifiedAtUtc =
            DateTime.UtcNow;
    }

    /// <summary>
    /// Determines whether the best-known-result identifier
    /// must be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an identifier is present;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeBestKnownResultId()
    {
        return HasBestKnownResultId;
    }

    /// <summary>
    /// Determines whether the known-result collection must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the collection is not
    /// empty; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeKnownResults()
    {
        return HasKnownResults;
    }

    /// <summary>
    /// Determines whether the tag collection must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when the collection is not
    /// empty; otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeTags()
    {
        return HasTags;
    }

    /// <summary>
    /// Determines whether the creation date must be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a creation date exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeCreatedAtUtc()
    {
        return HasCreationDate;
    }

    /// <summary>
    /// Determines whether the modification date must be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a modification date exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeModifiedAtUtc()
    {
        return HasModificationDate;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        string instanceDescription =
            HasName
                ? Name
                : HasInstanceId
                    ? InstanceId
                    : "Unnamed lot-sizing instance";

        return
            $"{instanceDescription}; " +
            $"horizon {PlanningHorizon.ToString(
                CultureInfo.InvariantCulture)}; " +
            $"{KnownResultCount.ToString(
                CultureInfo.InvariantCulture)} known result(s)";
    }

    private static DateTime? ConvertToUtc(
        DateTime? value)
    {
        if (!value.HasValue)
        {
            return null;
        }

        DateTime dateTimeValue =
            value.Value;

        return dateTimeValue.Kind switch
        {
            DateTimeKind.Utc =>
                dateTimeValue,

            DateTimeKind.Local =>
                dateTimeValue.ToUniversalTime(),

            _ =>
                DateTime.SpecifyKind(
                    dateTimeValue,
                    DateTimeKind.Utc)
        };
    }

    private void SortTags()
    {
        string[] orderedTags =
            Tags
                .OrderBy(
                    tag =>
                        tag,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        Tags.Clear();

        Tags.AddRange(
            orderedTags);
    }

    private void NotifyKnownResultProperties()
    {
        OnPropertyChanged(
            nameof(KnownResults));

        OnPropertyChanged(
            nameof(HasKnownResults));

        OnPropertyChanged(
            nameof(KnownResultCount));

        NotifyBestKnownResultProperties();
    }

    private void NotifyBestKnownResultProperties()
    {
        OnPropertyChanged(
            nameof(BestKnownResultId));

        OnPropertyChanged(
            nameof(HasBestKnownResultId));

        OnPropertyChanged(
            nameof(BestKnownResult));

        OnPropertyChanged(
            nameof(HasBestKnownResult));

        OnPropertyChanged(
            nameof(HasEligibleBestKnownResult));
    }

    private void NotifyTagProperties()
    {
        OnPropertyChanged(
            nameof(Tags));

        OnPropertyChanged(
            nameof(HasTags));
    }
}