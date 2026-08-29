using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// One technical optimization backend. A backend is not a scientific solution
/// method.
/// </summary>
public sealed class ScientificSolverBackendDefinition
{
    public ScientificSolverBackendDefinition(
        SolverKind solverKind,
        string name,
        IEnumerable<ScientificSolutionMethodCategory>
            supportedMethodCategories)
    {
        if (
            solverKind is
                SolverKind.Unknown or
                SolverKind.Automatic)
        {
            throw new ArgumentOutOfRangeException(
                nameof(solverKind),
                solverKind,
                "A backend definition requires a concrete solver kind.");
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "A backend name is required.",
                nameof(name));
        }

        ArgumentNullException.ThrowIfNull(supportedMethodCategories);

        SolverKind = solverKind;
        Name = name.Trim();

        SupportedMethodCategories =
            supportedMethodCategories
                .Distinct()
                .OrderBy(value => (int)value)
                .ToArray();
    }

    public SolverKind SolverKind { get; }
    public string Name { get; }

    public IReadOnlyList<ScientificSolutionMethodCategory>
        SupportedMethodCategories { get; }

    public bool Supports(
        ScientificSolutionMethodDefinition method) =>
            SupportedMethodCategories.Contains(
                method.Category);
}
