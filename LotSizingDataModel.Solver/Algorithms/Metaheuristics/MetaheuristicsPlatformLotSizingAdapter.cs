using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class MetaheuristicsPlatformLotSizingAdapter
{
    private readonly IMathematicalSolutionMappingService
        _mappingService;

    public MetaheuristicsPlatformLotSizingAdapter(
        IMathematicalSolutionMappingService mappingService)
    {
        _mappingService =
            mappingService ??
            throw new ArgumentNullException(
                nameof(mappingService));
    }

    public async ValueTask<LotSizingSolution>
        SolveAndMapAsync(
            LotSizingInstance instance,
            MathematicalModel model,
            DebConstraintGaBridgeOptions? options = null,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            model);

        var optimizer =
            new DebConstraintGaMathematicalModelOptimizer();

        DebConstraintGaBridgeResult bridgeResult =
            optimizer.Optimize(
                model,
                options,
                cancellationToken);

        if (!bridgeResult.IsFeasible)
        {
            throw new InvalidOperationException(
                "The metaheuristic run did not produce a feasible incumbent.");
        }

        var projector =
            new MetaheuristicsPlatformMathematicalResultProjector();

        var mathematicalResult =
            projector.Project(
                model,
                bridgeResult);

        MathematicalSolutionMappingResult mapping =
            await _mappingService.MapAsync(
                instance,
                model,
                mathematicalResult,
                new MathematicalSolutionMappingOptions
                {
                    IncludeZeroValues = true,
                    RequireKnownCategories = true,
                    RequireCompleteVariableValues = true
                },
                cancellationToken);

        if (!mapping.IsSuccessful ||
            mapping.Solution is null)
        {
            throw new InvalidOperationException(
                "Normalized mapping of the MetaheuristicsPlatform incumbent failed: " +
                mapping.FailureMessage);
        }

        return mapping.Solution;
    }
}
