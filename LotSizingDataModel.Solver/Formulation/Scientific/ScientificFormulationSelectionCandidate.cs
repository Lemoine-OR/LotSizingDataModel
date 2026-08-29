namespace LotSizingDataModel.Solver.Formulation.Scientific;

/// <summary>
/// One registered formulation and its scientific compatibility.
/// </summary>
public sealed class ScientificFormulationSelectionCandidate
{
    internal ScientificFormulationSelectionCandidate(
        IMathematicalModelFormulation formulation,
        ScientificFormulationCompatibilityResult compatibility,
        bool? technicalCanBuild)
    {
        Formulation =
            formulation ??
            throw new ArgumentNullException(nameof(formulation));

        Compatibility =
            compatibility ??
            throw new ArgumentNullException(nameof(compatibility));

        TechnicalCanBuild = technicalCanBuild;
    }

    public IMathematicalModelFormulation Formulation { get; }
    public ScientificFormulationCompatibilityResult Compatibility { get; }

    /// <summary>
    /// Null when selection was performed from a precomputed scientific result
    /// without a concrete LotSizingInstance.
    /// </summary>
    public bool? TechnicalCanBuild { get; }

    public bool IsSelectable =>
        Compatibility.Kind ==
            ScientificFormulationCompatibilityKind.Compatible &&
        TechnicalCanBuild != false;
}
