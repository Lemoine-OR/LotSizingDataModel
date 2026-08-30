using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

public sealed class GlspSchedulingFormulation : MathematicalModelFormulationBase
{
    public const string FormulationIdValue = "macro-micro-glsp";

    private readonly IStandardLotSizingVariableBuilder _variableBuilder;
    private readonly IStandardLotSizingObjectiveBuilder _objectiveBuilder;
    private readonly IStandardLotSizingConstraintBuilder _constraintBuilder;
    private readonly StandardLotSizingFormulationOptions _options;
    private readonly GlspSchedulingApplicabilityService _applicability;

    public GlspSchedulingFormulation(
        IStandardLotSizingVariableBuilder variableBuilder,
        IStandardLotSizingObjectiveBuilder objectiveBuilder,
        IStandardLotSizingConstraintBuilder constraintBuilder,
        StandardLotSizingFormulationOptions options,
        GlspSchedulingApplicabilityService applicability)
    {
        _variableBuilder = variableBuilder ?? throw new ArgumentNullException(nameof(variableBuilder));
        _objectiveBuilder = objectiveBuilder ?? throw new ArgumentNullException(nameof(objectiveBuilder));
        _constraintBuilder = constraintBuilder ?? throw new ArgumentNullException(nameof(constraintBuilder));
        _options = options?.Clone() ?? throw new ArgumentNullException(nameof(options));
        _applicability = applicability ?? throw new ArgumentNullException(nameof(applicability));
        _options.EnsureValid();
    }

    public override string FormulationId => FormulationIdValue;
    public override string Name => "GLSP macro/micro MILP formulation";
    public override string Description =>
        "Solver-independent single-resource GLSP with variable-length micro-periods, exact setup states and sequence-dependent changeovers.";
    public override bool CanBuild(LotSizingInstance instance) => _applicability.CanBuild(instance);

    protected override string CreateModelName(LotSizingInstance instance) =>
        string.IsNullOrWhiteSpace(instance.Name) ? FormulationId : $"{instance.Name.Trim()} - {FormulationId}";

    protected override ValueTask BuildVariablesAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken) =>
            _variableBuilder.BuildAsync(instance, context, _options, cancellationToken);

    protected override ValueTask BuildObjectiveAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken) =>
            _objectiveBuilder.BuildAsync(instance, context, _options, cancellationToken);

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken) =>
            _constraintBuilder.BuildAsync(instance, context, _options, cancellationToken);
}
