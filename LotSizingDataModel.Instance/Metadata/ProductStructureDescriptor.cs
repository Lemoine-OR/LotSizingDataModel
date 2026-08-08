using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Common;

namespace LotSizingDataModel.Instance.Metadata;

/// <summary>
/// Describes the declared and automatically detected structure
/// of the product bill-of-materials graph of an instance.
/// </summary>
/// <remarks>
/// The declared type represents information supplied by an
/// author, publication or data provider.
///
/// The detected type represents the result of an automatic
/// analysis of the supply-chain component relationships.
///
/// Both values are retained when they differ.
/// </remarks>
[Serializable]
[XmlType(TypeName = "productStructureDescriptor")]
public sealed class ProductStructureDescriptor :
    ModelObject
{
    private ProductStructureType _declaredType =
        ProductStructureType.Unknown;

    private ProductStructureType _detectedType =
        ProductStructureType.Unknown;

    private ProductStructureCheckStatus _checkStatus =
        ProductStructureCheckStatus.NotAnalyzed;

    private bool _hasCycle;
    private int _maximumDepth;

    private DateTime? _analyzedAtUtc;

    private string _analyzerVersion =
        string.Empty;

    private string _supplyChainFingerprint =
        string.Empty;

    private string _analysisComment =
        string.Empty;

    /// <summary>
    /// Initializes an empty product-structure descriptor.
    /// </summary>
    public ProductStructureDescriptor()
    {
    }

    /// <summary>
    /// Initializes a descriptor with a declared
    /// product-structure type.
    /// </summary>
    /// <param name="declaredType">
    /// Product-structure type declared by an author,
    /// publication or data provider.
    /// </param>
    public ProductStructureDescriptor(
        ProductStructureType declaredType)
    {
        DeclaredType = declaredType;

        CheckStatus =
            declaredType ==
                ProductStructureType.Unknown
                ? ProductStructureCheckStatus.NotAnalyzed
                : ProductStructureCheckStatus.DeclaredOnly;
    }

    /// <summary>
    /// Gets or sets the product-structure type declared
    /// by an author, publication or data provider.
    /// </summary>
    /// <remarks>
    /// This value must not be silently replaced when an
    /// automatic analysis produces a different result.
    /// </remarks>
    [XmlAttribute("declaredType")]
    public ProductStructureType DeclaredType
    {
        get => _declaredType;
        set
        {
            if (SetProperty(
                    ref _declaredType,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the product-structure type detected
    /// automatically from the supply-chain data.
    /// </summary>
    [XmlAttribute("detectedType")]
    public ProductStructureType DetectedType
    {
        get => _detectedType;
        set
        {
            if (SetProperty(
                    ref _detectedType,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the current status of the product-structure
    /// declaration and automatic verification.
    /// </summary>
    [XmlAttribute("checkStatus")]
    public ProductStructureCheckStatus CheckStatus
    {
        get => _checkStatus;
        set
        {
            if (SetProperty(
                    ref _checkStatus,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the analyzed
    /// bill-of-materials graph contains at least one cycle.
    /// </summary>
    /// <remarks>
    /// A cyclic product structure is considered invalid.
    /// </remarks>
    [XmlAttribute("hasCycle")]
    public bool HasCycle
    {
        get => _hasCycle;
        set
        {
            if (SetProperty(
                    ref _hasCycle,
                    value))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the maximum number of bill-of-materials
    /// relationships on a path from a leaf item to a root item.
    /// </summary>
    /// <remarks>
    /// A value of zero represents independent items or an
    /// analysis for which no product-structure arc exists.
    /// </remarks>
    [XmlAttribute("maximumDepth")]
    public int MaximumDepth
    {
        get => _maximumDepth;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "The maximum depth cannot be negative.");
            }

            SetProperty(
                ref _maximumDepth,
                value);
        }
    }

    /// <summary>
    /// Gets the identifiers of root items in the
    /// bill-of-materials graph.
    /// </summary>
    /// <remarks>
    /// A root item is not consumed as a component by another
    /// item in the analyzed product structure.
    /// </remarks>
    [XmlArray("rootItemIds")]
    [XmlArrayItem("itemId")]
    public List<int> RootItemIds { get; } =
        new();

    /// <summary>
    /// Gets the identifiers of leaf items in the
    /// bill-of-materials graph.
    /// </summary>
    /// <remarks>
    /// A leaf item does not consume another component item.
    /// </remarks>
    [XmlArray("leafItemIds")]
    [XmlArrayItem("itemId")]
    public List<int> LeafItemIds { get; } =
        new();

    /// <summary>
    /// Gets the identifiers of components used by more than
    /// one immediate parent item.
    /// </summary>
    [XmlArray("sharedComponentItemIds")]
    [XmlArrayItem("itemId")]
    public List<int> SharedComponentItemIds { get; } =
        new();

    /// <summary>
    /// Gets or sets the UTC date and time at which the
    /// automatic analysis was performed.
    /// </summary>
    /// <remarks>
    /// A null value means that no automatic analysis date
    /// has been recorded.
    /// </remarks>
    [XmlElement("analyzedAtUtc", IsNullable = true)]
    public DateTime? AnalyzedAtUtc
    {
        get => _analyzedAtUtc;
        set
        {
            DateTime? utcValue =
                value.HasValue
                    ? ConvertToUtc(value.Value)
                    : null;

            if (SetProperty(
                    ref _analyzedAtUtc,
                    utcValue))
            {
                NotifyDerivedProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the component that
    /// performed the product-structure analysis.
    /// </summary>
    [XmlAttribute("analyzerVersion")]
    public string AnalyzerVersion
    {
        get => _analyzerVersion;
        set => SetProperty(
            ref _analyzerVersion,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets the fingerprint of the supply-chain data
    /// used during the product-structure analysis.
    /// </summary>
    /// <remarks>
    /// This value allows a validator to detect that the
    /// supply chain has changed since the analysis.
    /// </remarks>
    [XmlAttribute("supplyChainFingerprint")]
    public string SupplyChainFingerprint
    {
        get => _supplyChainFingerprint;
        set => SetProperty(
            ref _supplyChainFingerprint,
            value?.Trim() ?? string.Empty);
    }

    /// <summary>
    /// Gets or sets an optional human-readable comment
    /// about the product-structure analysis.
    /// </summary>
    [XmlElement("analysisComment")]
    public string AnalysisComment
    {
        get => _analysisComment;
        set => SetProperty(
            ref _analysisComment,
            value ?? string.Empty);
    }

    /// <summary>
    /// Gets a value indicating whether an author or source
    /// declared a product-structure type.
    /// </summary>
    [XmlIgnore]
    public bool HasDeclaredType =>
        DeclaredType !=
        ProductStructureType.Unknown;

    /// <summary>
    /// Gets a value indicating whether an automatic analysis
    /// detected a product-structure type.
    /// </summary>
    [XmlIgnore]
    public bool HasDetectedType =>
        DetectedType !=
        ProductStructureType.Unknown;

    /// <summary>
    /// Gets a value indicating whether an automatic analysis
    /// date has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasBeenAnalyzed =>
        AnalyzedAtUtc.HasValue;

    /// <summary>
    /// Gets a value indicating whether at least one component
    /// is shared by several immediate parent items.
    /// </summary>
    [XmlIgnore]
    public bool HasSharedComponents =>
        SharedComponentItemIds.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the declared and
    /// detected structure types contradict each other.
    /// </summary>
    [XmlIgnore]
    public bool HasDeclarationConflict =>
        CheckStatus ==
        ProductStructureCheckStatus
            .DeclaredAndContradicted;

    /// <summary>
    /// Gets the product-structure type that should normally
    /// be used by higher-level classification services.
    /// </summary>
    /// <remarks>
    /// A valid automatically detected type takes priority
    /// over a declaration.
    ///
    /// An invalid or outdated analysis produces
    /// <see cref="ProductStructureType.Unknown"/>.
    /// </remarks>
    [XmlIgnore]
    public ProductStructureType EffectiveType =>
        CheckStatus switch
        {
            ProductStructureCheckStatus.DetectedOnly
                => DetectedType,

            ProductStructureCheckStatus.DeclaredAndConfirmed
                => DetectedType,

            ProductStructureCheckStatus
                .DeclaredAndContradicted
                => DetectedType,

            ProductStructureCheckStatus.DeclaredOnly
                => DeclaredType,

            _ =>
                ProductStructureType.Unknown
        };

    /// <summary>
    /// Gets a value indicating whether the recorded structure
    /// type may currently be used for problem classification.
    /// </summary>
    [XmlIgnore]
    public bool CanBeUsedForClassification =>
        EffectiveType !=
            ProductStructureType.Unknown &&
        CheckStatus !=
            ProductStructureCheckStatus.Invalid &&
        CheckStatus !=
            ProductStructureCheckStatus.Outdated;

    /// <summary>
    /// Marks the recorded automatic analysis as outdated.
    /// </summary>
    /// <remarks>
    /// The previously detected information is retained for
    /// traceability, but it must not be used until the product
    /// structure is analyzed again.
    /// </remarks>
    public void MarkAsOutdated()
    {
        if (!HasBeenAnalyzed &&
            !HasDetectedType)
        {
            return;
        }

        CheckStatus =
            ProductStructureCheckStatus.Outdated;
    }

    /// <summary>
    /// Removes all automatically detected product-structure
    /// information while preserving the declared type.
    /// </summary>
    public void ClearDetectedAnalysis()
    {
        DetectedType =
            ProductStructureType.Unknown;

        HasCycle = false;
        MaximumDepth = 0;

        RootItemIds.Clear();
        LeafItemIds.Clear();
        SharedComponentItemIds.Clear();

        AnalyzedAtUtc = null;
        AnalyzerVersion = string.Empty;
        SupplyChainFingerprint = string.Empty;
        AnalysisComment = string.Empty;

        CheckStatus =
            HasDeclaredType
                ? ProductStructureCheckStatus.DeclaredOnly
                : ProductStructureCheckStatus.NotAnalyzed;

        NotifyDerivedProperties();
    }

    /// <summary>
    /// Replaces the item sets generated by the automatic
    /// product-structure analysis.
    /// </summary>
    /// <param name="rootItemIds">
    /// Identifiers of root items.
    /// </param>
    /// <param name="leafItemIds">
    /// Identifiers of leaf items.
    /// </param>
    /// <param name="sharedComponentItemIds">
    /// Identifiers of shared component items.
    /// </param>
    public void ReplaceAnalyzedItemSets(
        IEnumerable<int> rootItemIds,
        IEnumerable<int> leafItemIds,
        IEnumerable<int> sharedComponentItemIds)
    {
        ArgumentNullException.ThrowIfNull(rootItemIds);
        ArgumentNullException.ThrowIfNull(leafItemIds);
        ArgumentNullException.ThrowIfNull(
            sharedComponentItemIds);

        ReplaceItemIds(
            RootItemIds,
            rootItemIds,
            nameof(rootItemIds));

        ReplaceItemIds(
            LeafItemIds,
            leafItemIds,
            nameof(leafItemIds));

        ReplaceItemIds(
            SharedComponentItemIds,
            sharedComponentItemIds,
            nameof(sharedComponentItemIds));

        OnPropertyChanged(
            nameof(RootItemIds));

        OnPropertyChanged(
            nameof(LeafItemIds));

        OnPropertyChanged(
            nameof(SharedComponentItemIds));

        NotifyDerivedProperties();
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return
            $"Declared: {DeclaredType}; " +
            $"detected: {DetectedType}; " +
            $"status: {CheckStatus}; " +
            $"depth: {MaximumDepth}";
    }

    private static void ReplaceItemIds(
        ICollection<int> destination,
        IEnumerable<int> source,
        string parameterName)
    {
        int[] itemIds =
            source
                .Distinct()
                .OrderBy(
                    itemId =>
                        itemId)
                .ToArray();

        if (itemIds.Any(
                itemId =>
                    itemId <= 0))
        {
            throw new ArgumentException(
                "Every item identifier must be " +
                "strictly positive.",
                parameterName);
        }

        destination.Clear();

        foreach (int itemId in itemIds)
        {
            destination.Add(itemId);
        }
    }

    private static DateTime ConvertToUtc(
        DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc =>
                value,

            DateTimeKind.Local =>
                value.ToUniversalTime(),

            _ =>
                DateTime.SpecifyKind(
                    value,
                    DateTimeKind.Utc)
        };
    }

    private void NotifyDerivedProperties()
    {
        OnPropertyChanged(
            nameof(HasDeclaredType));

        OnPropertyChanged(
            nameof(HasDetectedType));

        OnPropertyChanged(
            nameof(HasBeenAnalyzed));

        OnPropertyChanged(
            nameof(HasSharedComponents));

        OnPropertyChanged(
            nameof(HasDeclarationConflict));

        OnPropertyChanged(
            nameof(EffectiveType));

        OnPropertyChanged(
            nameof(CanBeUsedForClassification));
    }
}