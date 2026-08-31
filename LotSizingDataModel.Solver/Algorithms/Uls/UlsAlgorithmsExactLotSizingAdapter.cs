using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Solves a canonical SI-ULS instance with one explicitly
/// requested exact ULSAlgorithms method and maps the result
/// through the existing normalized solution-mapping service.
/// </summary>
public sealed class UlsAlgorithmsExactLotSizingAdapter
{
    private readonly IMathematicalSolutionMappingService
        _mappingService;

    private readonly UlsAlgorithmsExactModelContractExtractor
        _extractor;

    private readonly UlsAlgorithmsExactBridge
        _bridge;

    private readonly UlsAlgorithmsExactMathematicalResultProjector
        _projector;

    public UlsAlgorithmsExactLotSizingAdapter(
        IMathematicalSolutionMappingService mappingService)
    {
        _mappingService =
            mappingService ??
            throw new ArgumentNullException(
                nameof(mappingService));

        _extractor =
            new UlsAlgorithmsExactModelContractExtractor();

        _bridge =
            new UlsAlgorithmsExactBridge();

        _projector =
            new UlsAlgorithmsExactMathematicalResultProjector();
    }

    public async ValueTask<UlsAlgorithmsExactAdapterResult>
        SolveAndMapAsync(
            LotSizingInstance instance,
            MathematicalModel canonicalModel,
            UlsAlgorithmsExactMethod method,
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

        UlsAlgorithmsExactBridgeResult externalResult =
            _bridge.Solve(
                problem,
                method,
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
                "The normalized LotSizingDataModel mapping of the external exact ULS result failed: " +
                mapping.FailureMessage);
        }

        return new UlsAlgorithmsExactAdapterResult(
            externalResult,
            mathematicalResult,
            mapping.Solution);
    }
}
