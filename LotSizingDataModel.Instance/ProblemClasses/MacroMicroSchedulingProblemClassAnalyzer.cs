using LotSizingDataModel.Core.DecisionModel.Scheduling;
using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.ProblemClasses;

public sealed class MacroMicroSchedulingProblemClassAnalyzer
{
    private readonly LotSizingProblemClassExtensionAnalyzer _extensionAnalyzer;

    public MacroMicroSchedulingProblemClassAnalyzer()
        : this(new LotSizingProblemClassExtensionAnalyzer())
    {
    }

    public MacroMicroSchedulingProblemClassAnalyzer(
        LotSizingProblemClassExtensionAnalyzer extensionAnalyzer)
    {
        _extensionAnalyzer =
            extensionAnalyzer ??
            throw new ArgumentNullException(nameof(extensionAnalyzer));
    }

    public LotSizingProblemClassMatchResult Assess(
        LotSizingProblemDescriptor descriptor,
        LotSizingProblemClassDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(definition);

        if (definition.Id !=
            CanonicalLotSizingProblemClassId.GeneralLotSizingAndScheduling)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.NotRepresentable,
                failedRequirements:
                    new[] { "MacroMicroSchedulingAnalyzer:UnsupportedClass" });
        }

        var incomplete = new List<string>();
        var contradictions = new List<string>();
        SchedulingDescriptor scheduling = descriptor.Scheduling;

        if (!scheduling.HasIntegratedScheduling)
            contradictions.Add("Scheduling:Integrated");

        if (!scheduling.HasMicroPeriodStructure)
            contradictions.Add("Scheduling:MacroMicro");

        if (!scheduling.HasExplicitMicroPeriodGrid)
            incomplete.Add("Scheduling:MicroPeriodGrid");

        if (scheduling.MicroPeriodLengthMode == MicroPeriodLengthMode.Unspecified)
            incomplete.Add("Scheduling:MicroPeriodLengthMode");
        else if (!scheduling.HasVariableLengthMicroPeriods)
            contradictions.Add("Scheduling:VariableMicroPeriodLength");

        if (scheduling.MicroPeriodAssignmentMode == MicroPeriodAssignmentMode.Unspecified)
            incomplete.Add("Scheduling:MicroPeriodAssignmentMode");
        else if (!scheduling.HasSingleItemPerMicroPeriod)
            contradictions.Add("Scheduling:SingleItemPerMicroPeriod");

        if (descriptor.Structure.ItemCount == 0)
            incomplete.Add("Structure:ItemCount");
        else if (!descriptor.Structure.IsMultiItem)
            contradictions.Add("Structure:MultiItem");

        if (!descriptor.Structure.IsSingleLevel)
            contradictions.Add("Structure:SingleLevel");

        if (scheduling.SchedulingResourceCount == 0)
            incomplete.Add("Scheduling:ResourceCount");
        else if (!scheduling.HasSingleSchedulingResource)
            contradictions.Add("Scheduling:SingleResource");

        if (!descriptor.Demand.HasDemand)
            contradictions.Add("Demand:Present");

        if (!descriptor.Demand.IsDeterministic)
            contradictions.Add("Demand:Deterministic");

        if (!descriptor.Production.HasProduction)
            contradictions.Add("Production:Present");

        if (!descriptor.Capacity.HasProductionCapacity)
            contradictions.Add("Capacity:Production");

        if (!descriptor.Capacity.HasSharedProductionCapacity)
            contradictions.Add("Capacity:Shared");

        if (contradictions.Count > 0)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.NotApplicable,
                failedRequirements: contradictions);
        }

        if (incomplete.Count > 0)
        {
            return new LotSizingProblemClassMatchResult(
                definition,
                LotSizingProblemClassMatchKind.Incomplete,
                failedRequirements: incomplete);
        }

        var coreExtensions =
            new HashSet<LotSizingProblemClassExtensionKind>
            {
                LotSizingProblemClassExtensionKind.IntegratedScheduling,
                LotSizingProblemClassExtensionKind.MacroMicroScheduling
            };

        LotSizingProblemClassExtensionKind[] extensions =
            _extensionAnalyzer
                .Analyze(descriptor)
                .Where(extension => !coreExtensions.Contains(extension))
                .ToArray();

        return new LotSizingProblemClassMatchResult(
            definition,
            extensions.Length == 0
                ? LotSizingProblemClassMatchKind.ExactCore
                : LotSizingProblemClassMatchKind.CompatibleExtension,
            extensions);
    }
}
