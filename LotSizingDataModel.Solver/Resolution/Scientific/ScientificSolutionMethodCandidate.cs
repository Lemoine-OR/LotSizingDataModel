namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// One scientific solution-method candidate for a classified instance.
/// </summary>
public sealed class ScientificSolutionMethodCandidate
{
    internal ScientificSolutionMethodCandidate(
        ScientificSolutionMethodDefinition method,
        ScientificSolutionMethodCompatibilityKind compatibility,
        string rationale)
    {
        Method =
            method ??
            throw new ArgumentNullException(nameof(method));

        Compatibility = compatibility;
        Rationale = rationale ?? string.Empty;
    }

    public ScientificSolutionMethodDefinition Method { get; }

    public ScientificSolutionMethodCompatibilityKind Compatibility { get; }

    public string Rationale { get; }

    public bool IsExecutableCandidate =>
        Compatibility ==
        ScientificSolutionMethodCompatibilityKind.ExecutableCompatible;
}
