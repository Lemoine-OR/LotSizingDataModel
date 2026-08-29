using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Checker.Tests.Pipeline.Scientific;

internal sealed class ScientificSolvePipelineFakeSolverService :
    ILotSizingSolverService
{
    private readonly Func<SolverRequest, SolverRunResult> _resultFactory;

    public ScientificSolvePipelineFakeSolverService(
        Func<SolverRequest, SolverRunResult> resultFactory)
    {
        _resultFactory =
            resultFactory ??
            throw new ArgumentNullException(nameof(resultFactory));
    }

    public int SolveCallCount { get; private set; }

    public SolverRequest? LastRequest { get; private set; }

    public bool StopRequested { get; private set; }

    public ValueTask<SolverRunResult> SolveAsync(
        SolverRequest request,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        SolveCallCount++;
        LastRequest = request;

        return ValueTask.FromResult(
            _resultFactory(request));
    }

    public void RequestStop()
    {
        StopRequested = true;
    }

    public static SolverRunResult Success(
        SolverRequest request,
        LotSizingSolution solution,
        string? formulationName = null) =>
            new()
            {
                RunName =
                    request.RunName,
                SolverKind =
                    SolverKind.Cplex,
                SolverName =
                    "Fake scientific solver",
                SolverVersion =
                    "1.0",
                FormulationName =
                    formulationName ??
                    request.FormulationName,
                StartedAtUtc =
                    DateTime.UtcNow,
                CompletedAtUtc =
                    DateTime.UtcNow,
                ElapsedSeconds =
                    0.001,
                TerminationReason =
                    SolverTerminationReason.Optimal,
                SolutionCount =
                    1,
                ObjectiveValue =
                    solution.Evaluation.ObjectiveValue,
                Solution =
                    solution
            };

    public static SolverRunResult NoSolution(
        SolverRequest request) =>
            new()
            {
                RunName =
                    request.RunName,
                SolverKind =
                    SolverKind.Cplex,
                SolverName =
                    "Fake scientific solver",
                SolverVersion =
                    "1.0",
                FormulationName =
                    request.FormulationName,
                StartedAtUtc =
                    DateTime.UtcNow,
                CompletedAtUtc =
                    DateTime.UtcNow,
                ElapsedSeconds =
                    0.001,
                TerminationReason =
                    SolverTerminationReason.Infeasible,
                SolutionCount =
                    0,
                Solution =
                    null
            };
}
