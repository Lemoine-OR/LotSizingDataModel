using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Executes one explicitly selected ULSAlgorithms heuristic on
/// the strict canonical SI-ULS shape introduced by alpha.34.
/// </summary>
public sealed class UlsAlgorithmsHeuristicLotSizingAdapter
{
    private readonly IMathematicalSolutionMappingService
        _mappingService;

    private readonly UlsAlgorithmsExactModelContractExtractor
        _extractor =
            new();

    private readonly UlsAlgorithmsHeuristicBridge
        _bridge =
            new();

    private readonly UlsAlgorithmsHeuristicMathematicalResultProjector
        _projector =
            new();

    public UlsAlgorithmsHeuristicLotSizingAdapter(
        IMathematicalSolutionMappingService mappingService)
    {
        _mappingService =
            mappingService ??
            throw new ArgumentNullException(
                nameof(mappingService));
    }

    public async ValueTask<UlsAlgorithmsHeuristicAdapterResult>
        SolveAndMapAsync(
            LotSizingInstance instance,
            MathematicalModel canonicalModel,
            string heuristicSolverId,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            canonicalModel);

        UlsAlgorithmsExactProblemData problem =
            _extractor.Extract(
                instance,
                canonicalModel);

        UlsAlgorithmsHeuristicBridgeResult externalResult =
            _bridge.Solve(
                problem,
                heuristicSolverId,
                cancellationToken);

        var mathematicalResult =
            _projector.Project(
                canonicalModel,
                problem,
                externalResult);

        MathematicalSolutionMappingResult mapping =
            await _mappingService.MapAsync(
                instance,
                canonicalModel,
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
                "The normalized LotSizingDataModel mapping of the heuristic ULS result failed: " +
                mapping.FailureMessage);
        }

        return new UlsAlgorithmsHeuristicAdapterResult(
            externalResult,
            mathematicalResult,
            mapping.Solution);
    }
}
