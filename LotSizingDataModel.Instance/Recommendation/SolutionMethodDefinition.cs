using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Solution.Common;

namespace LotSizingDataModel.Instance.Recommendation;

/// <summary>
/// Describes the applicability, capabilities and limitations
/// of a solution method for lot-sizing problem instances.
/// </summary>
/// <remarks>
/// A method definition is independent from a particular
/// instance. It specifies:
/// <list type="bullet">
/// <item>
/// <description>
/// the problem families supported by the method;
/// </description>
/// </item>
/// <item>
/// <description>
/// the product structures accepted by the method;
/// </description>
/// </item>
/// <item>
/// <description>
/// required, supported, partially supported and unsupported
/// problem features;
/// </description>
/// </item>
/// <item>
/// <description>
/// whether the method applies to complete problems,
/// relaxations or subproblems;
/// </description>
/// </item>
/// <item>
/// <description>
/// hard and recommended instance-size limits;
/// </description>
/// </item>
/// <item>
/// <description>
/// the type of result or bound that the method can provide.
/// </description>
/// </item>
/// </list>
///
/// Feature codes normally refer to properties of
/// <c>LotSizingProblemFeatures</c>.
/// </remarks>
[Serializable]
[XmlType(TypeName = "solutionMethodDefinition")]
public sealed class SolutionMethodDefinition : ModelObject
{
    private string _methodCode =
        string.Empty;

    private string _name =
        string.Empty;

    private string _methodVersion =
        string.Empty;

    private SolutionMethodKind _methodKind =
        default;

    private string _description =
        string.Empty;

    private bool _isEnabled =
        true;

    private int _priority;

    private bool _supportsAnyProblemFamily;

    private bool _supportsAnyProductStructure;

    private bool _supportsUnclassifiedProblems;

    private bool _supportsAmbiguousClassifications;

    private bool _supportsCompleteProblems =
        true;

    private bool _supportsRelaxations;

    private bool _supportsSubproblems;

    private bool _canProduceFeasibleSolution =
        true;

    private bool _canProveOptimality;

    private bool _canProvideLowerBound;

    private bool _canProvideUpperBound =
        true;

    private int? _maximumItemCount;

    private int? _maximumPlanningHorizon;

    private int? _maximumPlantCount;

    private int? _maximumWorkCenterCount;

    private int? _maximumWarehouseCount;

    private int? _maximumSupplierCount;

    private int? _maximumTransportResourceCount;

    private int? _maximumBillOfMaterialsRelationshipCount;

    private int? _maximumProductStructureDepth;

    private int? _recommendedMaximumItemCount;

    private int? _recommendedMaximumPlanningHorizon;

    private int?
        _recommendedMaximumBillOfMaterialsRelationshipCount;

    private int? _recommendedMaximumProductStructureDepth;

    private string _implementationName =
        string.Empty;

    private string _implementationVersion =
        string.Empty;

    private string _comment =
        string.Empty;

    /// <summary>
    /// Initializes an empty solution-method definition.
    /// </summary>
    /// <remarks>
    /// This constructor is required for XML serialization.
    /// </remarks>
    public SolutionMethodDefinition()
    {
    }

    /// <summary>
    /// Initializes a solution-method definition.
    /// </summary>
    /// <param name="methodCode">
    /// Stable code identifying the solution method.
    /// </param>
    /// <param name="name">
    /// Human-readable method name.
    /// </param>
    /// <param name="methodKind">
    /// General category of the solution method.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="methodCode"/> or
    /// <paramref name="name"/> is empty.
    /// </exception>
    public SolutionMethodDefinition(
        string methodCode,
        string name,
        SolutionMethodKind methodKind)
    {
        if (string.IsNullOrWhiteSpace(methodCode))
        {
            throw new ArgumentException(
                "A solution-method code is required.",
                nameof(methodCode));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A solution-method name is required.",
                nameof(name));
        }

        MethodCode =
            methodCode;

        Name =
            name;

        MethodKind =
            methodKind;
    }

    /// <summary>
    /// Gets or sets the stable code identifying the solution
    /// method.
    /// </summary>
    /// <remarks>
    /// Examples include <c>WW-DP</c>,
    /// <c>MILP-GENERIC</c>, <c>MLLP-DP</c> and
    /// <c>FIX-AND-OPTIMIZE</c>.
    /// </remarks>
    [XmlAttribute("methodCode")]
    public string MethodCode
    {
        get => _methodCode;
        set
        {
            if (SetProperty(
                    ref _methodCode,
                    NormalizeCode(value)))
            {
                OnPropertyChanged(
                    nameof(HasMethodCode));

                OnPropertyChanged(
                    nameof(IsValidDefinition));

                OnPropertyChanged(
                    nameof(CanBeEvaluated));
            }
        }
    }

    /// <summary>
    /// Gets or sets the human-readable name of the method.
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

                OnPropertyChanged(
                    nameof(IsValidDefinition));

                OnPropertyChanged(
                    nameof(CanBeEvaluated));
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the method definition.
    /// </summary>
    /// <remarks>
    /// This version describes the method capabilities stored
    /// in the catalog. It does not necessarily identify a
    /// software implementation version.
    /// </remarks>
    [XmlAttribute("methodVersion")]
    public string MethodVersion
    {
        get => _methodVersion;
        set
        {
            if (SetProperty(
                    ref _methodVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasMethodVersion));
            }
        }
    }

    /// <summary>
    /// Gets or sets the general category of the solution
    /// method.
    /// </summary>
    [XmlAttribute("methodKind")]
    public SolutionMethodKind MethodKind
    {
        get => _methodKind;
        set => SetProperty(
            ref _methodKind,
            value);
    }

    /// <summary>
    /// Gets or sets a human-readable description of the
    /// method and its principal assumptions.
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
    /// Gets or sets a value indicating whether this method
    /// definition is enabled in the catalog.
    /// </summary>
    [XmlAttribute("isEnabled")]
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
    /// Gets or sets the catalog priority of the method.
    /// </summary>
    /// <remarks>
    /// A greater value gives the method preference when two
    /// recommendations otherwise have equivalent scores.
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
    /// Gets or sets a value indicating whether the method is
    /// independent from a specific classified problem family.
    /// </summary>
    /// <remarks>
    /// This property is appropriate for generic mathematical
    /// programming methods whose applicability is primarily
    /// determined by features and model size.
    /// </remarks>
    [XmlAttribute("supportsAnyProblemFamily")]
    public bool SupportsAnyProblemFamily
    {
        get => _supportsAnyProblemFamily;
        set
        {
            if (SetProperty(
                    ref _supportsAnyProblemFamily,
                    value))
            {
                NotifyDefinitionValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method
    /// supports any valid product structure.
    /// </summary>
    /// <remarks>
    /// When this property is <see langword="false"/>, the
    /// accepted structures must be listed in
    /// <see cref="SupportedProductStructureTypes"/>.
    /// </remarks>
    [XmlAttribute("supportsAnyProductStructure")]
    public bool SupportsAnyProductStructure
    {
        get => _supportsAnyProductStructure;
        set
        {
            if (SetProperty(
                    ref _supportsAnyProductStructure,
                    value))
            {
                NotifyDefinitionValidityProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method may
    /// be evaluated for a problem that has not been assigned
    /// to a known family.
    /// </summary>
    [XmlAttribute("supportsUnclassifiedProblems")]
    public bool SupportsUnclassifiedProblems
    {
        get => _supportsUnclassifiedProblems;
        set => SetProperty(
            ref _supportsUnclassifiedProblems,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method may
    /// be evaluated when the problem-family classification is
    /// ambiguous.
    /// </summary>
    [XmlAttribute("supportsAmbiguousClassifications")]
    public bool SupportsAmbiguousClassifications
    {
        get => _supportsAmbiguousClassifications;
        set => SetProperty(
            ref _supportsAmbiguousClassifications,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method can
    /// solve a complete problem instance.
    /// </summary>
    [XmlAttribute("supportsCompleteProblems")]
    public bool SupportsCompleteProblems
    {
        get => _supportsCompleteProblems;
        set
        {
            if (SetProperty(
                    ref _supportsCompleteProblems,
                    value))
            {
                NotifyScopeProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method can
    /// solve a relaxation of a complete problem.
    /// </summary>
    [XmlAttribute("supportsRelaxations")]
    public bool SupportsRelaxations
    {
        get => _supportsRelaxations;
        set
        {
            if (SetProperty(
                    ref _supportsRelaxations,
                    value))
            {
                NotifyScopeProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method can
    /// solve subproblems extracted from a complete instance.
    /// </summary>
    [XmlAttribute("supportsSubproblems")]
    public bool SupportsSubproblems
    {
        get => _supportsSubproblems;
        set
        {
            if (SetProperty(
                    ref _supportsSubproblems,
                    value))
            {
                NotifyScopeProperties();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method can
    /// produce a feasible solution to the problem scope being
    /// solved.
    /// </summary>
    [XmlAttribute("canProduceFeasibleSolution")]
    public bool CanProduceFeasibleSolution
    {
        get => _canProduceFeasibleSolution;
        set => SetProperty(
            ref _canProduceFeasibleSolution,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method can
    /// prove optimality under its stated assumptions.
    /// </summary>
    [XmlAttribute("canProveOptimality")]
    public bool CanProveOptimality
    {
        get => _canProveOptimality;
        set => SetProperty(
            ref _canProveOptimality,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method can
    /// produce a valid lower bound for a minimization problem.
    /// </summary>
    [XmlAttribute("canProvideLowerBound")]
    public bool CanProvideLowerBound
    {
        get => _canProvideLowerBound;
        set => SetProperty(
            ref _canProvideLowerBound,
            value);
    }

    /// <summary>
    /// Gets or sets a value indicating whether the method can
    /// produce a valid upper bound for a minimization problem.
    /// </summary>
    /// <remarks>
    /// A feasible solution normally provides an upper bound
    /// for a minimization problem.
    /// </remarks>
    [XmlAttribute("canProvideUpperBound")]
    public bool CanProvideUpperBound
    {
        get => _canProvideUpperBound;
        set => SetProperty(
            ref _canProvideUpperBound,
            value);
    }

    /// <summary>
    /// Gets the problem-family codes directly supported by
    /// the method.
    /// </summary>
    /// <remarks>
    /// Codes are compared case-insensitively.
    ///
    /// Examples include <c>LS-U</c>, <c>LS-C</c>,
    /// <c>CLSP</c>, <c>MLLP</c> and <c>MLCLSP</c>.
    /// </remarks>
    [XmlArray("supportedProblemTypeCodes")]
    [XmlArrayItem("problemTypeCode")]
    public List<string> SupportedProblemTypeCodes { get; } =
        new();

    /// <summary>
    /// Gets the problem-family codes for which the method is
    /// considered particularly appropriate.
    /// </summary>
    [XmlArray("preferredProblemTypeCodes")]
    [XmlArrayItem("problemTypeCode")]
    public List<string> PreferredProblemTypeCodes { get; } =
        new();

    /// <summary>
    /// Gets the product structures supported by the method.
    /// </summary>
    /// <remarks>
    /// This collection is used when
    /// <see cref="SupportsAnyProductStructure"/> is
    /// <see langword="false"/>.
    /// </remarks>
    [XmlArray("supportedProductStructureTypes")]
    [XmlArrayItem("productStructureType")]
    public List<ProductStructureType>
        SupportedProductStructureTypes
    { get; } =
            new();

    /// <summary>
    /// Gets the feature codes that must be active for the
    /// method to be applicable.
    /// </summary>
    /// <remarks>
    /// A required feature is expected to represent a Boolean
    /// property whose value must be <see langword="true"/>.
    /// </remarks>
    [XmlArray("requiredFeatureCodes")]
    [XmlArrayItem("featureCode")]
    public List<string> RequiredFeatureCodes { get; } =
        new();

    /// <summary>
    /// Gets the active feature codes explicitly supported by
    /// the method.
    /// </summary>
    /// <remarks>
    /// This collection is primarily intended for optional
    /// extensions such as backlogging, transportation or
    /// additional capacity.
    /// </remarks>
    [XmlArray("supportedFeatureCodes")]
    [XmlArrayItem("featureCode")]
    public List<string> SupportedFeatureCodes { get; } =
        new();

    /// <summary>
    /// Gets the feature codes supported only through an
    /// adaptation, approximation, decomposition or
    /// implementation-specific extension.
    /// </summary>
    [XmlArray("partiallySupportedFeatureCodes")]
    [XmlArrayItem("featureCode")]
    public List<string> PartiallySupportedFeatureCodes
    {
        get;
    } = new();

    /// <summary>
    /// Gets the active feature codes that make the method
    /// incompatible with the complete problem.
    /// </summary>
    [XmlArray("unsupportedFeatureCodes")]
    [XmlArrayItem("featureCode")]
    public List<string> UnsupportedFeatureCodes { get; } =
        new();

    /// <summary>
    /// Gets the feature codes that make the method
    /// particularly attractive when they are active.
    /// </summary>
    [XmlArray("preferredFeatureCodes")]
    [XmlArrayItem("featureCode")]
    public List<string> PreferredFeatureCodes { get; } =
        new();

    /// <summary>
    /// Gets or sets the hard maximum number of items accepted
    /// by the method or its current implementation.
    /// </summary>
    [XmlElement("maximumItemCount", IsNullable = true)]
    public int? MaximumItemCount
    {
        get => _maximumItemCount;
        set => SetHardLimit(
            ref _maximumItemCount,
            value,
            nameof(MaximumItemCount));
    }

    /// <summary>
    /// Gets or sets the hard maximum number of planning
    /// periods accepted by the method.
    /// </summary>
    [XmlElement("maximumPlanningHorizon", IsNullable = true)]
    public int? MaximumPlanningHorizon
    {
        get => _maximumPlanningHorizon;
        set => SetHardLimit(
            ref _maximumPlanningHorizon,
            value,
            nameof(MaximumPlanningHorizon));
    }

    /// <summary>
    /// Gets or sets the hard maximum number of plants
    /// accepted by the method.
    /// </summary>
    [XmlElement("maximumPlantCount", IsNullable = true)]
    public int? MaximumPlantCount
    {
        get => _maximumPlantCount;
        set => SetHardLimit(
            ref _maximumPlantCount,
            value,
            nameof(MaximumPlantCount));
    }

    /// <summary>
    /// Gets or sets the hard maximum number of work centers
    /// accepted by the method.
    /// </summary>
    [XmlElement("maximumWorkCenterCount", IsNullable = true)]
    public int? MaximumWorkCenterCount
    {
        get => _maximumWorkCenterCount;
        set => SetHardLimit(
            ref _maximumWorkCenterCount,
            value,
            nameof(MaximumWorkCenterCount));
    }

    /// <summary>
    /// Gets or sets the hard maximum number of warehouses
    /// accepted by the method.
    /// </summary>
    [XmlElement("maximumWarehouseCount", IsNullable = true)]
    public int? MaximumWarehouseCount
    {
        get => _maximumWarehouseCount;
        set => SetHardLimit(
            ref _maximumWarehouseCount,
            value,
            nameof(MaximumWarehouseCount));
    }

    /// <summary>
    /// Gets or sets the hard maximum number of suppliers
    /// accepted by the method.
    /// </summary>
    [XmlElement("maximumSupplierCount", IsNullable = true)]
    public int? MaximumSupplierCount
    {
        get => _maximumSupplierCount;
        set => SetHardLimit(
            ref _maximumSupplierCount,
            value,
            nameof(MaximumSupplierCount));
    }

    /// <summary>
    /// Gets or sets the hard maximum number of transport
    /// resources accepted by the method.
    /// </summary>
    [XmlElement(
        "maximumTransportResourceCount",
        IsNullable = true)]
    public int? MaximumTransportResourceCount
    {
        get => _maximumTransportResourceCount;
        set => SetHardLimit(
            ref _maximumTransportResourceCount,
            value,
            nameof(MaximumTransportResourceCount));
    }

    /// <summary>
    /// Gets or sets the hard maximum number of
    /// bill-of-materials relationships accepted by the
    /// method.
    /// </summary>
    [XmlElement(
        "maximumBillOfMaterialsRelationshipCount",
        IsNullable = true)]
    public int? MaximumBillOfMaterialsRelationshipCount
    {
        get => _maximumBillOfMaterialsRelationshipCount;
        set => SetHardLimit(
            ref _maximumBillOfMaterialsRelationshipCount,
            value,
            nameof(
                MaximumBillOfMaterialsRelationshipCount));
    }

    /// <summary>
    /// Gets or sets the hard maximum product-structure depth
    /// accepted by the method.
    /// </summary>
    [XmlElement(
        "maximumProductStructureDepth",
        IsNullable = true)]
    public int? MaximumProductStructureDepth
    {
        get => _maximumProductStructureDepth;
        set => SetHardLimit(
            ref _maximumProductStructureDepth,
            value,
            nameof(MaximumProductStructureDepth));
    }

    /// <summary>
    /// Gets or sets the recommended maximum number of items
    /// for effective use of the method.
    /// </summary>
    /// <remarks>
    /// Exceeding this value does not make the method
    /// incompatible, but may reduce its recommendation score.
    /// </remarks>
    [XmlElement(
        "recommendedMaximumItemCount",
        IsNullable = true)]
    public int? RecommendedMaximumItemCount
    {
        get => _recommendedMaximumItemCount;
        set => SetRecommendedLimit(
            ref _recommendedMaximumItemCount,
            value,
            nameof(RecommendedMaximumItemCount));
    }

    /// <summary>
    /// Gets or sets the recommended maximum planning horizon
    /// for effective use of the method.
    /// </summary>
    [XmlElement(
        "recommendedMaximumPlanningHorizon",
        IsNullable = true)]
    public int? RecommendedMaximumPlanningHorizon
    {
        get => _recommendedMaximumPlanningHorizon;
        set => SetRecommendedLimit(
            ref _recommendedMaximumPlanningHorizon,
            value,
            nameof(RecommendedMaximumPlanningHorizon));
    }

    /// <summary>
    /// Gets or sets the recommended maximum number of
    /// bill-of-materials relationships.
    /// </summary>
    [XmlElement(
        "recommendedMaximumBillOfMaterialsRelationshipCount",
        IsNullable = true)]
    public int?
        RecommendedMaximumBillOfMaterialsRelationshipCount
    {
        get =>
            _recommendedMaximumBillOfMaterialsRelationshipCount;

        set => SetRecommendedLimit(
            ref
                _recommendedMaximumBillOfMaterialsRelationshipCount,
            value,
            nameof(
                RecommendedMaximumBillOfMaterialsRelationshipCount));
    }

    /// <summary>
    /// Gets or sets the recommended maximum product-structure
    /// depth.
    /// </summary>
    [XmlElement(
        "recommendedMaximumProductStructureDepth",
        IsNullable = true)]
    public int? RecommendedMaximumProductStructureDepth
    {
        get => _recommendedMaximumProductStructureDepth;
        set => SetRecommendedLimit(
            ref _recommendedMaximumProductStructureDepth,
            value,
            nameof(
                RecommendedMaximumProductStructureDepth));
    }

    /// <summary>
    /// Gets or sets the name of an implementation associated
    /// with this method definition.
    /// </summary>
    /// <remarks>
    /// This property is optional because a method definition
    /// may describe an algorithm independently from any
    /// software implementation.
    /// </remarks>
    [XmlElement("implementationName")]
    public string ImplementationName
    {
        get => _implementationName;
        set
        {
            if (SetProperty(
                    ref _implementationName,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasImplementationInformation));
            }
        }
    }

    /// <summary>
    /// Gets or sets the version of the associated method
    /// implementation.
    /// </summary>
    [XmlAttribute("implementationVersion")]
    public string ImplementationVersion
    {
        get => _implementationVersion;
        set
        {
            if (SetProperty(
                    ref _implementationVersion,
                    value?.Trim() ?? string.Empty))
            {
                OnPropertyChanged(
                    nameof(HasImplementationInformation));
            }
        }
    }

    /// <summary>
    /// Gets the documentary or bibliographic references
    /// associated with the method.
    /// </summary>
    [XmlArray("references")]
    [XmlArrayItem("reference")]
    public List<string> References { get; } =
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
    /// Gets a value indicating whether a stable method code
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasMethodCode =>
        !string.IsNullOrWhiteSpace(
            MethodCode);

    /// <summary>
    /// Gets a value indicating whether a human-readable name
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasName =>
        !string.IsNullOrWhiteSpace(
            Name);

    /// <summary>
    /// Gets a value indicating whether a method-definition
    /// version has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasMethodVersion =>
        !string.IsNullOrWhiteSpace(
            MethodVersion);

    /// <summary>
    /// Gets a value indicating whether a description has been
    /// recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasDescription =>
        !string.IsNullOrWhiteSpace(
            Description);

    /// <summary>
    /// Gets a value indicating whether the method explicitly
    /// supports at least one problem family.
    /// </summary>
    [XmlIgnore]
    public bool HasSupportedProblemTypes =>
        SupportedProblemTypeCodes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the method identifies
    /// at least one preferred problem family.
    /// </summary>
    [XmlIgnore]
    public bool HasPreferredProblemTypes =>
        PreferredProblemTypeCodes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the method explicitly
    /// supports at least one product-structure type.
    /// </summary>
    [XmlIgnore]
    public bool HasSupportedProductStructureTypes =>
        SupportedProductStructureTypes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the method has at least
    /// one explicit feature capability or restriction.
    /// </summary>
    [XmlIgnore]
    public bool HasFeatureRules =>
        RequiredFeatureCodes.Count > 0 ||
        SupportedFeatureCodes.Count > 0 ||
        PartiallySupportedFeatureCodes.Count > 0 ||
        UnsupportedFeatureCodes.Count > 0 ||
        PreferredFeatureCodes.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the method supports at
    /// least one problem scope.
    /// </summary>
    [XmlIgnore]
    public bool HasSupportedScope =>
        SupportsCompleteProblems ||
        SupportsRelaxations ||
        SupportsSubproblems;

    /// <summary>
    /// Gets a value indicating whether at least one hard size
    /// limit has been specified.
    /// </summary>
    [XmlIgnore]
    public bool HasHardSizeLimits =>
        MaximumItemCount.HasValue ||
        MaximumPlanningHorizon.HasValue ||
        MaximumPlantCount.HasValue ||
        MaximumWorkCenterCount.HasValue ||
        MaximumWarehouseCount.HasValue ||
        MaximumSupplierCount.HasValue ||
        MaximumTransportResourceCount.HasValue ||
        MaximumBillOfMaterialsRelationshipCount.HasValue ||
        MaximumProductStructureDepth.HasValue;

    /// <summary>
    /// Gets a value indicating whether at least one
    /// recommended size limit has been specified.
    /// </summary>
    [XmlIgnore]
    public bool HasRecommendedSizeLimits =>
        RecommendedMaximumItemCount.HasValue ||
        RecommendedMaximumPlanningHorizon.HasValue ||
        RecommendedMaximumBillOfMaterialsRelationshipCount
            .HasValue ||
        RecommendedMaximumProductStructureDepth.HasValue;

    /// <summary>
    /// Gets a value indicating whether implementation
    /// information has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasImplementationInformation =>
        !string.IsNullOrWhiteSpace(
            ImplementationName) ||
        !string.IsNullOrWhiteSpace(
            ImplementationVersion);

    /// <summary>
    /// Gets a value indicating whether at least one reference
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasReferences =>
        References.Count > 0;

    /// <summary>
    /// Gets a value indicating whether an explanatory comment
    /// has been recorded.
    /// </summary>
    [XmlIgnore]
    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    /// <summary>
    /// Gets a value indicating whether the method definition
    /// contains the minimum information required for
    /// evaluation.
    /// </summary>
    [XmlIgnore]
    public bool IsValidDefinition =>
        Validate().Count == 0;

    /// <summary>
    /// Gets a value indicating whether this method can be
    /// considered by the solution-method advisor.
    /// </summary>
    [XmlIgnore]
    public bool CanBeEvaluated =>
        IsEnabled &&
        IsValidDefinition;

    /// <summary>
    /// Determines whether this method supports a problem
    /// family code.
    /// </summary>
    /// <param name="problemTypeCode">
    /// Problem-family code to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when every family is supported
    /// or when the supplied family appears in the supported
    /// family collection; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool SupportsProblemTypeCode(
        string problemTypeCode)
    {
        if (SupportsAnyProblemFamily)
        {
            return true;
        }

        return ContainsCode(
            SupportedProblemTypeCodes,
            problemTypeCode);
    }

    /// <summary>
    /// Determines whether this method identifies a problem
    /// family as particularly appropriate.
    /// </summary>
    /// <param name="problemTypeCode">
    /// Problem-family code to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the code appears in
    /// <see cref="PreferredProblemTypeCodes"/>.
    /// </returns>
    public bool PrefersProblemTypeCode(
        string problemTypeCode)
    {
        return ContainsCode(
            PreferredProblemTypeCodes,
            problemTypeCode);
    }

    /// <summary>
    /// Determines whether this method supports a given
    /// product-structure type.
    /// </summary>
    /// <param name="productStructureType">
    /// Product-structure type to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when every product structure is
    /// supported or when the supplied structure appears in
    /// the supported structure collection.
    /// </returns>
    public bool SupportsProductStructureType(
        ProductStructureType productStructureType)
    {
        return
            SupportsAnyProductStructure ||
            SupportedProductStructureTypes.Contains(
                productStructureType);
    }

    /// <summary>
    /// Determines whether an active feature is explicitly
    /// supported by the method.
    /// </summary>
    /// <param name="featureCode">
    /// Feature code to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the feature is required or
    /// explicitly supported.
    /// </returns>
    public bool SupportsFeatureCode(
        string featureCode)
    {
        return
            ContainsCode(
                RequiredFeatureCodes,
                featureCode) ||
            ContainsCode(
                SupportedFeatureCodes,
                featureCode);
    }

    /// <summary>
    /// Determines whether a feature is only partially
    /// supported.
    /// </summary>
    /// <param name="featureCode">
    /// Feature code to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the feature appears in the
    /// partially supported feature collection.
    /// </returns>
    public bool PartiallySupportsFeatureCode(
        string featureCode)
    {
        return ContainsCode(
            PartiallySupportedFeatureCodes,
            featureCode);
    }

    /// <summary>
    /// Determines whether a feature is explicitly unsupported.
    /// </summary>
    /// <param name="featureCode">
    /// Feature code to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the feature appears in the
    /// unsupported feature collection.
    /// </returns>
    public bool ExcludesFeatureCode(
        string featureCode)
    {
        return ContainsCode(
            UnsupportedFeatureCodes,
            featureCode);
    }

    /// <summary>
    /// Replaces the supported problem-family codes.
    /// </summary>
    /// <param name="problemTypeCodes">
    /// New supported problem-family codes.
    /// </param>
    public void ReplaceSupportedProblemTypeCodes(
        IEnumerable<string> problemTypeCodes)
    {
        ReplaceCodeCollection(
            SupportedProblemTypeCodes,
            problemTypeCodes);

        NotifyProblemTypeProperties();
    }

    /// <summary>
    /// Replaces the preferred problem-family codes.
    /// </summary>
    /// <param name="problemTypeCodes">
    /// New preferred problem-family codes.
    /// </param>
    public void ReplacePreferredProblemTypeCodes(
        IEnumerable<string> problemTypeCodes)
    {
        ReplaceCodeCollection(
            PreferredProblemTypeCodes,
            problemTypeCodes);

        NotifyProblemTypeProperties();
    }

    /// <summary>
    /// Replaces the product structures supported by the
    /// method.
    /// </summary>
    /// <param name="productStructureTypes">
    /// New supported product structures.
    /// </param>
    public void ReplaceSupportedProductStructureTypes(
        IEnumerable<ProductStructureType>
            productStructureTypes)
    {
        ArgumentNullException.ThrowIfNull(
            productStructureTypes);

        ProductStructureType[] normalizedValues =
            productStructureTypes
                .Distinct()
                .OrderBy(
                    value =>
                        value)
                .ToArray();

        SupportedProductStructureTypes.Clear();

        SupportedProductStructureTypes.AddRange(
            normalizedValues);

        OnPropertyChanged(
            nameof(SupportedProductStructureTypes));

        OnPropertyChanged(
            nameof(
                HasSupportedProductStructureTypes));

        NotifyDefinitionValidityProperties();
    }

    /// <summary>
    /// Replaces the required feature codes.
    /// </summary>
    /// <param name="featureCodes">
    /// New required feature codes.
    /// </param>
    public void ReplaceRequiredFeatureCodes(
        IEnumerable<string> featureCodes)
    {
        ReplaceFeatureCodeCollection(
            RequiredFeatureCodes,
            featureCodes);

        NotifyFeatureProperties();
    }

    /// <summary>
    /// Replaces the explicitly supported feature codes.
    /// </summary>
    /// <param name="featureCodes">
    /// New supported feature codes.
    /// </param>
    public void ReplaceSupportedFeatureCodes(
        IEnumerable<string> featureCodes)
    {
        ReplaceFeatureCodeCollection(
            SupportedFeatureCodes,
            featureCodes);

        NotifyFeatureProperties();
    }

    /// <summary>
    /// Replaces the partially supported feature codes.
    /// </summary>
    /// <param name="featureCodes">
    /// New partially supported feature codes.
    /// </param>
    public void ReplacePartiallySupportedFeatureCodes(
        IEnumerable<string> featureCodes)
    {
        ReplaceFeatureCodeCollection(
            PartiallySupportedFeatureCodes,
            featureCodes);

        NotifyFeatureProperties();
    }

    /// <summary>
    /// Replaces the unsupported feature codes.
    /// </summary>
    /// <param name="featureCodes">
    /// New unsupported feature codes.
    /// </param>
    public void ReplaceUnsupportedFeatureCodes(
        IEnumerable<string> featureCodes)
    {
        ReplaceFeatureCodeCollection(
            UnsupportedFeatureCodes,
            featureCodes);

        NotifyFeatureProperties();
    }

    /// <summary>
    /// Replaces the preferred feature codes.
    /// </summary>
    /// <param name="featureCodes">
    /// New preferred feature codes.
    /// </param>
    public void ReplacePreferredFeatureCodes(
        IEnumerable<string> featureCodes)
    {
        ReplaceFeatureCodeCollection(
            PreferredFeatureCodes,
            featureCodes);

        NotifyFeatureProperties();
    }

    /// <summary>
    /// Replaces the documentary or bibliographic references
    /// associated with the method.
    /// </summary>
    /// <param name="references">
    /// New references.
    /// </param>
    public void ReplaceReferences(
        IEnumerable<string> references)
    {
        ArgumentNullException.ThrowIfNull(
            references);

        string[] normalizedReferences =
            references
                .Where(
                    reference =>
                        !string.IsNullOrWhiteSpace(
                            reference))
                .Select(
                    reference =>
                        reference.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    reference =>
                        reference,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        References.Clear();

        References.AddRange(
            normalizedReferences);

        OnPropertyChanged(
            nameof(References));

        OnPropertyChanged(
            nameof(HasReferences));
    }

    /// <summary>
    /// Returns the distinct feature codes referenced by this
    /// method definition.
    /// </summary>
    /// <returns>
    /// Ordered feature-code collection.
    /// </returns>
    public IReadOnlyList<string> GetAllFeatureCodes()
    {
        return RequiredFeatureCodes
            .Concat(
                SupportedFeatureCodes)
            .Concat(
                PartiallySupportedFeatureCodes)
            .Concat(
                UnsupportedFeatureCodes)
            .Concat(
                PreferredFeatureCodes)
            .Where(
                featureCode =>
                    !string.IsNullOrWhiteSpace(
                        featureCode))
            .Select(
                featureCode =>
                    featureCode.Trim())
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .OrderBy(
                featureCode =>
                    featureCode,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Validates the structural consistency of this method
    /// definition.
    /// </summary>
    /// <returns>
    /// Ordered validation-error collection. An empty
    /// collection indicates that the definition is valid.
    /// </returns>
    public IReadOnlyList<string> Validate()
    {
        var errors =
            new List<string>();

        if (!HasMethodCode)
        {
            errors.Add(
                "The solution-method code is missing.");
        }

        if (!HasName)
        {
            errors.Add(
                "The solution-method name is missing.");
        }

        if (!HasSupportedScope)
        {
            errors.Add(
                "The solution method does not support a " +
                "complete problem, relaxation or subproblem.");
        }

        if (!SupportsAnyProblemFamily &&
            !HasSupportedProblemTypes &&
            !SupportsUnclassifiedProblems)
        {
            errors.Add(
                "The method does not declare any supported " +
                "problem family and is not marked as generic.");
        }

        if (!SupportsAnyProductStructure &&
            !HasSupportedProductStructureTypes)
        {
            errors.Add(
                "The method does not declare any supported " +
                "product structure.");
        }

        ValidateStringCollection(
            SupportedProblemTypeCodes,
            "supported problem-family code",
            errors);

        ValidateStringCollection(
            PreferredProblemTypeCodes,
            "preferred problem-family code",
            errors);

        ValidateStringCollection(
            RequiredFeatureCodes,
            "required feature code",
            errors);

        ValidateStringCollection(
            SupportedFeatureCodes,
            "supported feature code",
            errors);

        ValidateStringCollection(
            PartiallySupportedFeatureCodes,
            "partially supported feature code",
            errors);

        ValidateStringCollection(
            UnsupportedFeatureCodes,
            "unsupported feature code",
            errors);

        ValidateStringCollection(
            PreferredFeatureCodes,
            "preferred feature code",
            errors);

        ValidateProductStructureCollection(
            errors);

        ValidateProblemTypeRelationships(
            errors);

        ValidateFeatureRelationships(
            errors);

        ValidateRecommendedLimits(
            errors);

        return errors
            .Distinct(
                StringComparer.Ordinal)
            .OrderBy(
                error =>
                    error,
                StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Validates the method definition and throws an exception
    /// when at least one error is found.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the method definition is invalid.
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
            "The solution-method definition is invalid:" +
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
        string familyDescription =
            SupportsAnyProblemFamily
                ? "any problem family"
                : $"{SupportedProblemTypeCodes.Count} " +
                  "supported family code(s)";

        return
            $"{MethodCode} — {Name}; " +
            $"{MethodKind}; " +
            $"{familyDescription}";
    }

    private static string NormalizeCode(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? string.Empty
            : value.Trim().ToUpperInvariant();
    }

    private static bool ContainsCode(
        IEnumerable<string> codes,
        string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return false;
        }

        string normalizedCode =
            code.Trim();

        return codes.Any(
            candidate =>
                string.Equals(
                    candidate,
                    normalizedCode,
                    StringComparison.OrdinalIgnoreCase));
    }

    private static void ReplaceCodeCollection(
        ICollection<string> destination,
        IEnumerable<string> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        string[] normalizedCodes =
            source
                .Where(
                    code =>
                        !string.IsNullOrWhiteSpace(code))
                .Select(
                    NormalizeCode)
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    code =>
                        code,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        destination.Clear();

        foreach (string code in normalizedCodes)
        {
            destination.Add(code);
        }
    }

    private static void ReplaceFeatureCodeCollection(
        ICollection<string> destination,
        IEnumerable<string> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        string[] normalizedCodes =
            source
                .Where(
                    code =>
                        !string.IsNullOrWhiteSpace(code))
                .Select(
                    code =>
                        code.Trim())
                .Distinct(
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    code =>
                        code,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        destination.Clear();

        foreach (string code in normalizedCodes)
        {
            destination.Add(code);
        }
    }

    private static void ValidateStringCollection(
        IEnumerable<string> values,
        string valueDescription,
        ICollection<string> errors)
    {
        string[] materializedValues =
            values.ToArray();

        if (materializedValues.Any(
                value =>
                    string.IsNullOrWhiteSpace(value)))
        {
            errors.Add(
                $"Every {valueDescription} must be " +
                "non-empty.");
        }

        string[] duplicateValues =
            materializedValues
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(value))
                .GroupBy(
                    value =>
                        value.Trim(),
                    StringComparer.OrdinalIgnoreCase)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    value =>
                        value,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (duplicateValues.Length > 0)
        {
            errors.Add(
                $"Duplicate {valueDescription} values: " +
                string.Join(
                    ", ",
                    duplicateValues) +
                ".");
        }
    }

    private void ValidateProductStructureCollection(
        ICollection<string> errors)
    {
        ProductStructureType[] duplicateTypes =
            SupportedProductStructureTypes
                .GroupBy(
                    productStructureType =>
                        productStructureType)
                .Where(
                    group =>
                        group.Count() > 1)
                .Select(
                    group =>
                        group.Key)
                .OrderBy(
                    productStructureType =>
                        productStructureType)
                .ToArray();

        if (duplicateTypes.Length > 0)
        {
            errors.Add(
                "Duplicate supported product-structure " +
                "types: " +
                string.Join(
                    ", ",
                    duplicateTypes) +
                ".");
        }

        if (SupportedProductStructureTypes.Contains(
                ProductStructureType.Unknown))
        {
            errors.Add(
                "Unknown cannot be declared as a supported " +
                "product-structure type.");
        }
    }

    private void ValidateProblemTypeRelationships(
        ICollection<string> errors)
    {
        if (SupportsAnyProblemFamily)
        {
            return;
        }

        string[] unsupportedPreferredCodes =
            PreferredProblemTypeCodes
                .Where(
                    preferredCode =>
                        !ContainsCode(
                            SupportedProblemTypeCodes,
                            preferredCode))
                .OrderBy(
                    code =>
                        code,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (unsupportedPreferredCodes.Length > 0)
        {
            errors.Add(
                "Preferred problem-family codes must also be " +
                "supported: " +
                string.Join(
                    ", ",
                    unsupportedPreferredCodes) +
                ".");
        }
    }

    private void ValidateFeatureRelationships(
        ICollection<string> errors)
    {
        AddOverlapError(
            RequiredFeatureCodes,
            UnsupportedFeatureCodes,
            "required",
            "unsupported",
            errors);

        AddOverlapError(
            RequiredFeatureCodes,
            PartiallySupportedFeatureCodes,
            "required",
            "partially supported",
            errors);

        AddOverlapError(
            SupportedFeatureCodes,
            UnsupportedFeatureCodes,
            "supported",
            "unsupported",
            errors);

        AddOverlapError(
            SupportedFeatureCodes,
            PartiallySupportedFeatureCodes,
            "supported",
            "partially supported",
            errors);

        AddOverlapError(
            PreferredFeatureCodes,
            UnsupportedFeatureCodes,
            "preferred",
            "unsupported",
            errors);

        AddOverlapError(
            PreferredFeatureCodes,
            PartiallySupportedFeatureCodes,
            "preferred",
            "partially supported",
            errors);
    }

    private void ValidateRecommendedLimits(
        ICollection<string> errors)
    {
        ValidateRecommendedLimit(
            RecommendedMaximumItemCount,
            MaximumItemCount,
            "item count",
            errors);

        ValidateRecommendedLimit(
            RecommendedMaximumPlanningHorizon,
            MaximumPlanningHorizon,
            "planning horizon",
            errors);

        ValidateRecommendedLimit(
            RecommendedMaximumBillOfMaterialsRelationshipCount,
            MaximumBillOfMaterialsRelationshipCount,
            "bill-of-materials relationship count",
            errors);

        ValidateRecommendedLimit(
            RecommendedMaximumProductStructureDepth,
            MaximumProductStructureDepth,
            "product-structure depth",
            errors);
    }

    private static void ValidateRecommendedLimit(
        int? recommendedLimit,
        int? hardLimit,
        string limitDescription,
        ICollection<string> errors)
    {
        if (recommendedLimit.HasValue &&
            hardLimit.HasValue &&
            recommendedLimit.Value >
            hardLimit.Value)
        {
            errors.Add(
                $"The recommended maximum " +
                $"{limitDescription} cannot exceed its hard " +
                "maximum.");
        }
    }

    private static void AddOverlapError(
        IEnumerable<string> firstCollection,
        IEnumerable<string> secondCollection,
        string firstDescription,
        string secondDescription,
        ICollection<string> errors)
    {
        string[] overlappingCodes =
            firstCollection
                .Intersect(
                    secondCollection,
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    code =>
                        code,
                    StringComparer.OrdinalIgnoreCase)
                .ToArray();

        if (overlappingCodes.Length == 0)
        {
            return;
        }

        errors.Add(
            $"Feature codes cannot be both " +
            $"{firstDescription} and {secondDescription}: " +
            string.Join(
                ", ",
                overlappingCodes) +
            ".");
    }

    private void SetHardLimit(
        ref int? storage,
        int? value,
        string propertyName)
    {
        ValidatePositiveNullableLimit(
            value,
            propertyName);

        if (SetProperty(
                ref storage,
                value,
                propertyName))
        {
            OnPropertyChanged(
                nameof(HasHardSizeLimits));

            NotifyDefinitionValidityProperties();
        }
    }

    private void SetRecommendedLimit(
        ref int? storage,
        int? value,
        string propertyName)
    {
        ValidatePositiveNullableLimit(
            value,
            propertyName);

        if (SetProperty(
                ref storage,
                value,
                propertyName))
        {
            OnPropertyChanged(
                nameof(HasRecommendedSizeLimits));

            NotifyDefinitionValidityProperties();
        }
    }

    private static void ValidatePositiveNullableLimit(
        int? value,
        string parameterName)
    {
        if (value.HasValue &&
            value.Value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "A method size limit must be strictly " +
                "positive when specified.");
        }
    }

    private void NotifyProblemTypeProperties()
    {
        OnPropertyChanged(
            nameof(SupportedProblemTypeCodes));

        OnPropertyChanged(
            nameof(PreferredProblemTypeCodes));

        OnPropertyChanged(
            nameof(HasSupportedProblemTypes));

        OnPropertyChanged(
            nameof(HasPreferredProblemTypes));

        NotifyDefinitionValidityProperties();
    }

    private void NotifyFeatureProperties()
    {
        OnPropertyChanged(
            nameof(RequiredFeatureCodes));

        OnPropertyChanged(
            nameof(SupportedFeatureCodes));

        OnPropertyChanged(
            nameof(PartiallySupportedFeatureCodes));

        OnPropertyChanged(
            nameof(UnsupportedFeatureCodes));

        OnPropertyChanged(
            nameof(PreferredFeatureCodes));

        OnPropertyChanged(
            nameof(HasFeatureRules));

        NotifyDefinitionValidityProperties();
    }

    private void NotifyScopeProperties()
    {
        OnPropertyChanged(
            nameof(HasSupportedScope));

        NotifyDefinitionValidityProperties();
    }

    private void NotifyDefinitionValidityProperties()
    {
        OnPropertyChanged(
            nameof(IsValidDefinition));

        OnPropertyChanged(
            nameof(CanBeEvaluated));
    }
}