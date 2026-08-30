using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// Scientific capability contract for one solver-independent formulation.
/// </summary>
/// <remarks>
/// Supported and known-unsupported extensions are explicit. Any extension
/// absent from both sets is intentionally Undetermined.
/// </remarks>
public sealed class MathematicalFormulationScientificProfile
{
    private readonly IReadOnlyCollection<CanonicalLotSizingProblemClassId>
        _supportedProblemClasses;

    private readonly IReadOnlyCollection<LotSizingProblemClassExtensionKind>
        _supportedExtensions;

    private readonly IReadOnlyCollection<LotSizingProblemClassExtensionKind>
        _knownUnsupportedExtensions;

    private readonly IReadOnlyCollection<OptimizationObjectiveKind>
        _supportedObjectiveKinds;

    public MathematicalFormulationScientificProfile(
        string formulationId,
        string formulationFamily,
        IEnumerable<CanonicalLotSizingProblemClassId>
            supportedProblemClasses,
        IEnumerable<LotSizingProblemClassExtensionKind>?
            supportedExtensions = null,
        IEnumerable<LotSizingProblemClassExtensionKind>?
            knownUnsupportedExtensions = null,
        IEnumerable<OptimizationObjectiveKind>?
            supportedObjectiveKinds = null,
        IEnumerable<string>? evidence = null)
    {
        if (string.IsNullOrWhiteSpace(formulationId))
        {
            throw new ArgumentException(
                "A formulation identifier is required.",
                nameof(formulationId));
        }

        if (string.IsNullOrWhiteSpace(formulationFamily))
        {
            throw new ArgumentException(
                "A formulation family is required.",
                nameof(formulationFamily));
        }

        ArgumentNullException.ThrowIfNull(supportedProblemClasses);

        FormulationId = formulationId.Trim();
        FormulationFamily = formulationFamily.Trim();

        _supportedProblemClasses =
            supportedProblemClasses
                .Distinct()
                .OrderBy(value => (int)value)
                .ToArray();

        _supportedExtensions =
            (supportedExtensions ??
             Array.Empty<LotSizingProblemClassExtensionKind>())
                .Distinct()
                .OrderBy(value => (int)value)
                .ToArray();

        _knownUnsupportedExtensions =
            (knownUnsupportedExtensions ??
             Array.Empty<LotSizingProblemClassExtensionKind>())
                .Distinct()
                .OrderBy(value => (int)value)
                .ToArray();

        _supportedObjectiveKinds =
            (supportedObjectiveKinds ??
             new[]
             {
                 OptimizationObjectiveKind.Economic
             })
                .Where(
                    value =>
                        value != OptimizationObjectiveKind.Unknown)
                .Distinct()
                .OrderBy(value => (int)value)
                .ToArray();

        LotSizingProblemClassExtensionKind[] overlap =
            _supportedExtensions
                .Intersect(_knownUnsupportedExtensions)
                .ToArray();

        if (overlap.Length > 0)
        {
            throw new ArgumentException(
                "A formulation extension cannot be both supported and " +
                "known unsupported: " +
                string.Join(",", overlap));
        }

        Evidence =
            (evidence ?? Array.Empty<string>())
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .ToArray();
    }

    public string FormulationId { get; }
    public string FormulationFamily { get; }

    public IReadOnlyCollection<CanonicalLotSizingProblemClassId>
        SupportedProblemClasses =>
            _supportedProblemClasses;

    public IReadOnlyCollection<LotSizingProblemClassExtensionKind>
        SupportedExtensions =>
            _supportedExtensions;

    public IReadOnlyCollection<LotSizingProblemClassExtensionKind>
        KnownUnsupportedExtensions =>
            _knownUnsupportedExtensions;

    public IReadOnlyCollection<OptimizationObjectiveKind>
        SupportedObjectiveKinds =>
            _supportedObjectiveKinds;

    public IReadOnlyList<string> Evidence { get; }

    public bool SupportsProblemClass(
        CanonicalLotSizingProblemClassId problemClass) =>
            _supportedProblemClasses.Contains(problemClass);

    public bool IsExtensionVerifiedSupported(
        LotSizingProblemClassExtensionKind extension) =>
            _supportedExtensions.Contains(extension);

    public bool IsExtensionKnownUnsupported(
        LotSizingProblemClassExtensionKind extension) =>
            _knownUnsupportedExtensions.Contains(extension);

    public bool SupportsObjectiveKind(
        OptimizationObjectiveKind objectiveKind) =>
            _supportedObjectiveKinds.Contains(objectiveKind);
}
