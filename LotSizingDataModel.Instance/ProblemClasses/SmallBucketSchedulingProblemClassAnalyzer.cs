using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Canonical membership analyzer for the DLSP, CSLP and PLSP small-bucket
/// scheduling families independently of their current solver support state.
/// </summary>
public sealed class SmallBucketSchedulingProblemClassAnalyzer
{
    private readonly LotSizingProblemClassExtensionAnalyzer
        _extensionAnalyzer;

    public SmallBucketSchedulingProblemClassAnalyzer()
        : this(
            new LotSizingProblemClassExtensionAnalyzer())
    {
    }

    public SmallBucketSchedulingProblemClassAnalyzer(
        LotSizingProblemClassExtensionAnalyzer extensionAnalyzer)
    {
        _extensionAnalyzer =
            extensionAnalyzer ??
            throw new ArgumentNullException(
                nameof(extensionAnalyzer));
    }

    public LotSizingProblemClassMatchResult Assess(
        LotSizingProblemDescriptor descriptor,
        LotSizingProblemClassDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(definition);

        if (
            definition.Id is not
                CanonicalLotSizingProblemClassId
                    .DiscreteLotSizingAndScheduling and not
                CanonicalLotSizingProblemClassId
                    .ContinuousSetupLotSizing and not
                CanonicalLotSizingProblemClassId
                    .ProportionalLotSizingAndScheduling)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.NotRepresentable,
                failedRequirements:
                    new[]
                    {
                        "SmallBucketSchedulingAnalyzer:UnsupportedClass"
                    });
        }

        List<string> incomplete =
            new();

        List<string> contradictions =
            new();

        SchedulingDescriptor scheduling =
            descriptor.Scheduling;

        if (!scheduling.HasIntegratedScheduling)
        {
            contradictions.Add(
                "Scheduling:Integrated");
        }

        if (!scheduling.HasSmallBucketStructure)
        {
            contradictions.Add(
                "Scheduling:SmallBucket");
        }

        if (descriptor.Structure.ItemCount == 0)
        {
            incomplete.Add(
                "Structure:ItemCount");
        }
        else if (!descriptor.Structure.IsMultiItem)
        {
            contradictions.Add(
                "Structure:MultiItem");
        }

        if (!descriptor.Structure.IsSingleLevel)
        {
            contradictions.Add(
                "Structure:SingleLevel");
        }

        if (scheduling.SchedulingResourceCount == 0)
        {
            incomplete.Add(
                "Scheduling:ResourceCount");
        }
        else if (!scheduling.HasSingleSchedulingResource)
        {
            contradictions.Add(
                "Scheduling:SingleResource");
        }

        if (!descriptor.Demand.HasDemand)
        {
            contradictions.Add(
                "Demand:Present");
        }

        if (!descriptor.Demand.IsDeterministic)
        {
            contradictions.Add(
                "Demand:Deterministic");
        }

        if (!descriptor.Production.HasProduction)
        {
            contradictions.Add(
                "Production:Present");
        }

        if (!descriptor.Capacity.HasProductionCapacity)
        {
            contradictions.Add(
                "Capacity:Production");
        }

        if (!descriptor.Capacity.HasSharedProductionCapacity)
        {
            contradictions.Add(
                "Capacity:Shared");
        }

        if (
            scheduling.SmallBucketProductionMode ==
            SmallBucketProductionMode.Unspecified)
        {
            incomplete.Add(
                "Scheduling:ProductionMode");
        }

        if (!scheduling.HasMaximumProducedItemCountConstraint)
        {
            incomplete.Add(
                "Scheduling:MaximumProducedItemCount");
        }

        if (contradictions.Count > 0)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.NotApplicable,
                failedRequirements:
                    contradictions);
        }

        if (incomplete.Count > 0)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.Incomplete,
                failedRequirements:
                    incomplete);
        }

        return definition.Id switch
        {
            CanonicalLotSizingProblemClassId
                .DiscreteLotSizingAndScheduling =>
                    AssessDlsp(
                        descriptor,
                        definition),

            CanonicalLotSizingProblemClassId
                .ContinuousSetupLotSizing =>
                    AssessCslp(
                        descriptor,
                        definition),

            CanonicalLotSizingProblemClassId
                .ProportionalLotSizingAndScheduling =>
                    AssessPlsp(
                        descriptor,
                        definition),

            _ =>
                throw new InvalidOperationException(
                    "Unexpected small-bucket scheduling problem class.")
        };
    }

    private LotSizingProblemClassMatchResult AssessDlsp(
        LotSizingProblemDescriptor descriptor,
        LotSizingProblemClassDefinition definition)
    {
        if (
            descriptor.Scheduling.SmallBucketProductionMode !=
                SmallBucketProductionMode.AllOrNothing ||
            descriptor.Scheduling.MaximumProducedItemCountPerBucket > 1)
        {
            return NotApplicable(
                definition,
                "DLSP:AllOrNothingAndAtMostOneItemPerBucket");
        }

        return Member(
            descriptor,
            definition,
            coreExtensions:
                new[]
                {
                    LotSizingProblemClassExtensionKind
                        .IntegratedScheduling,
                    LotSizingProblemClassExtensionKind
                        .SmallBucketScheduling
                });
    }

    private LotSizingProblemClassMatchResult AssessCslp(
        LotSizingProblemDescriptor descriptor,
        LotSizingProblemClassDefinition definition)
    {
        if (
            descriptor.Scheduling.SmallBucketProductionMode !=
                SmallBucketProductionMode.Continuous ||
            descriptor.Scheduling.MaximumProducedItemCountPerBucket > 1)
        {
            return NotApplicable(
                definition,
                "CSLP:ContinuousAndAtMostOneItemPerBucket");
        }

        return Member(
            descriptor,
            definition,
            coreExtensions:
                new[]
                {
                    LotSizingProblemClassExtensionKind
                        .IntegratedScheduling,
                    LotSizingProblemClassExtensionKind
                        .SmallBucketScheduling
                });
    }

    private LotSizingProblemClassMatchResult AssessPlsp(
        LotSizingProblemDescriptor descriptor,
        LotSizingProblemClassDefinition definition)
    {
        if (
            descriptor.Scheduling.SmallBucketProductionMode !=
                SmallBucketProductionMode.Continuous ||
            descriptor.Scheduling.MaximumProducedItemCountPerBucket != 2)
        {
            return NotApplicable(
                definition,
                "PLSP:ContinuousAndAtMostTwoItemsPerBucket");
        }

        if (
            !descriptor.Scheduling.HasMaximumSetupCountConstraints)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.Incomplete,
                failedRequirements:
                    new[]
                    {
                        "PLSP:MaximumSetupTransitionsPerBucket"
                    });
        }

        if (
            descriptor.Scheduling.MaximumSetupTransitionsPerBucket > 1)
        {
            return NotApplicable(
                definition,
                "PLSP:AtMostOneSetupTransitionPerBucket");
        }

        return Member(
            descriptor,
            definition,
            coreExtensions:
                new[]
                {
                    LotSizingProblemClassExtensionKind
                        .IntegratedScheduling,
                    LotSizingProblemClassExtensionKind
                        .SmallBucketScheduling,
                    LotSizingProblemClassExtensionKind
                        .MaximumSetupCount
                });
    }

    private LotSizingProblemClassMatchResult Member(
        LotSizingProblemDescriptor descriptor,
        LotSizingProblemClassDefinition definition,
        IEnumerable<LotSizingProblemClassExtensionKind>
            coreExtensions)
    {
        var core =
            new HashSet<LotSizingProblemClassExtensionKind>(
                coreExtensions);

        LotSizingProblemClassExtensionKind[] extensions =
            _extensionAnalyzer
                .Analyze(descriptor)
                .Where(
                    extension =>
                        !core.Contains(extension))
                .ToArray();

        return new LotSizingProblemClassMatchResult(
            definition,
            extensions.Length == 0
                ? LotSizingProblemClassMatchKind.ExactCore
                : LotSizingProblemClassMatchKind.CompatibleExtension,
            extensions);
    }

    private static LotSizingProblemClassMatchResult NotApplicable(
        LotSizingProblemClassDefinition definition,
        string failedRequirement) =>
            new(
                definition,
                LotSizingProblemClassMatchKind.NotApplicable,
                failedRequirements:
                    new[]
                    {
                        failedRequirement
                    });
}
