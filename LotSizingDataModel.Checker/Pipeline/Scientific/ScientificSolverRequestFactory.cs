using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Checker.Pipeline.Scientific;

/// <summary>
/// Creates the delegated technical solver request while preserving the caller's
/// original request object.
/// </summary>
internal static class ScientificSolverRequestFactory
{
    public static SolverRequest CreateDelegated(
        SolverRequest source,
        string formulationId)
    {
        ArgumentNullException.ThrowIfNull(source);

        if (string.IsNullOrWhiteSpace(formulationId))
        {
            throw new ArgumentException(
                "A scientifically selected formulation identifier is required.",
                nameof(formulationId));
        }

        source.EnsureValid();

        var delegated =
            new SolverRequest(source.Instance!)
            {
                PreferredSolver =
                    source.PreferredSolver,

                FormulationName =
                    formulationId.Trim(),

                RunName =
                    source.RunName ?? string.Empty,

                // The scientific wrapper never mutates solver parameters.
                // Reusing the same validated parameter object avoids an
                // incomplete hand-written pseudo-clone of native settings.
                Parameters =
                    source.Parameters
            };

        foreach (
            var observer
            in source.ProgressObservers)
        {
            delegated.AddProgressObserver(observer);
        }

        delegated.EnsureValid();

        return delegated;
    }
}
