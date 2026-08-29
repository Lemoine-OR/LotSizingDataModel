using LotSizingDataModel.Instance.ProblemClasses;

namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// Complete scientific formulation-capability assessment.
/// </summary>
public sealed class ScientificFormulationCompatibilityResult
{
    internal ScientificFormulationCompatibilityResult(
        string formulationId,
        MathematicalFormulationScientificProfile? profile,
        ScientificFormulationCompatibilityKind kind,
        CanonicalLotSizingProblemClassId? problemClass,
        IEnumerable<LotSizingProblemClassExtensionKind>
            verifiedSupportedExtensions,
        IEnumerable<LotSizingProblemClassExtensionKind>
            knownUnsupportedExtensions,
        IEnumerable<LotSizingProblemClassExtensionKind>
            undeterminedExtensions,
        IEnumerable<ScientificFormulationDiagnostic> diagnostics)
    {
        FormulationId = formulationId;
        Profile = profile;
        Kind = kind;
        ProblemClass = problemClass;

        VerifiedSupportedExtensions =
            verifiedSupportedExtensions.ToArray();

        KnownUnsupportedExtensions =
            knownUnsupportedExtensions.ToArray();

        UndeterminedExtensions =
            undeterminedExtensions.ToArray();

        Diagnostics =
            diagnostics.ToArray();
    }

    public string FormulationId { get; }
    public MathematicalFormulationScientificProfile? Profile { get; }
    public ScientificFormulationCompatibilityKind Kind { get; }

    public CanonicalLotSizingProblemClassId? ProblemClass { get; }

    public IReadOnlyList<LotSizingProblemClassExtensionKind>
        VerifiedSupportedExtensions { get; }

    public IReadOnlyList<LotSizingProblemClassExtensionKind>
        KnownUnsupportedExtensions { get; }

    public IReadOnlyList<LotSizingProblemClassExtensionKind>
        UndeterminedExtensions { get; }

    public IReadOnlyList<ScientificFormulationDiagnostic>
        Diagnostics { get; }

    public bool IsScientificallyCompatible =>
        Kind == ScientificFormulationCompatibilityKind.Compatible;
}
