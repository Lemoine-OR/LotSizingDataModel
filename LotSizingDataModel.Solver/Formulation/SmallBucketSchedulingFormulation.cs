using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Building;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Solver-independent executable MILP formulation for one canonical
/// small-bucket scheduling family.
/// </summary>
public sealed class SmallBucketSchedulingFormulation :
    MathematicalModelFormulationBase
{
    public const string DlspFormulationId =
        "small-bucket-dlsp";

    public const string CslpFormulationId =
        "small-bucket-cslp";

    public const string PlspFormulationId =
        "small-bucket-plsp";

    private readonly SmallBucketSchedulingFormulationKind _kind;
    private readonly IStandardLotSizingVariableBuilder _variableBuilder;
    private readonly IStandardLotSizingObjectiveBuilder _objectiveBuilder;
    private readonly IStandardLotSizingConstraintBuilder _constraintBuilder;
    private readonly StandardLotSizingFormulationOptions _options;
    private readonly SmallBucketSchedulingApplicabilityService
        _applicabilityService;

    public SmallBucketSchedulingFormulation(
        SmallBucketSchedulingFormulationKind kind,
        IStandardLotSizingVariableBuilder variableBuilder,
        IStandardLotSizingObjectiveBuilder objectiveBuilder,
        IStandardLotSizingConstraintBuilder constraintBuilder,
        StandardLotSizingFormulationOptions options,
        SmallBucketSchedulingApplicabilityService applicabilityService)
    {
        _kind = kind;
        _variableBuilder =
            variableBuilder ??
            throw new ArgumentNullException(nameof(variableBuilder));
        _objectiveBuilder =
            objectiveBuilder ??
            throw new ArgumentNullException(nameof(objectiveBuilder));
        _constraintBuilder =
            constraintBuilder ??
            throw new ArgumentNullException(nameof(constraintBuilder));
        _options =
            options?.Clone() ??
            throw new ArgumentNullException(nameof(options));
        _applicabilityService =
            applicabilityService ??
            throw new ArgumentNullException(nameof(applicabilityService));

        _options.EnsureValid();
    }

    public SmallBucketSchedulingFormulationKind Kind =>
        _kind;

    public override string FormulationId =>
        _kind switch
        {
            SmallBucketSchedulingFormulationKind.Dlsp =>
                DlspFormulationId,

            SmallBucketSchedulingFormulationKind.Cslp =>
                CslpFormulationId,

            SmallBucketSchedulingFormulationKind.Plsp =>
                PlspFormulationId,

            _ =>
                throw new InvalidOperationException(
                    "Unknown small-bucket formulation kind.")
        };

    public override string Name =>
        _kind switch
        {
            SmallBucketSchedulingFormulationKind.Dlsp =>
                "DLSP small-bucket MILP formulation",

            SmallBucketSchedulingFormulationKind.Cslp =>
                "CSLP small-bucket MILP formulation",

            SmallBucketSchedulingFormulationKind.Plsp =>
                "PLSP small-bucket MILP formulation",

            _ =>
                throw new InvalidOperationException(
                    "Unknown small-bucket formulation kind.")
        };

    public override string Description =>
        "Solver-independent single-resource small-bucket scheduling " +
        "formulation with persistent setup state and setup-start cost.";

    public override bool CanBuild(
        LotSizingInstance instance) =>
            _applicabilityService.CanBuild(
                instance,
                _kind);

    protected override string CreateModelName(
        LotSizingInstance instance) =>
            string.IsNullOrWhiteSpace(instance.Name)
                ? FormulationId
                : $"{instance.Name.Trim()} - {FormulationId}";

    protected override ValueTask BuildVariablesAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken) =>
            _variableBuilder.BuildAsync(
                instance,
                context,
                _options,
                cancellationToken);

    protected override ValueTask BuildObjectiveAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken) =>
            _objectiveBuilder.BuildAsync(
                instance,
                context,
                _options,
                cancellationToken);

    protected override ValueTask BuildConstraintsAsync(
        LotSizingInstance instance,
        MathematicalModelBuildContext context,
        CancellationToken cancellationToken) =>
            _constraintBuilder.BuildAsync(
                instance,
                context,
                _options,
                cancellationToken);
}
