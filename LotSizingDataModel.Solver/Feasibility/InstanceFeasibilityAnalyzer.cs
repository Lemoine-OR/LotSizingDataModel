using LotSizingDataModel.Instance;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Feasibility;

/// <summary>
/// Pre-solve instance feasibility facade. It never reports
/// Feasible: only proven Infeasible or Unknown.
/// </summary>
public sealed class InstanceFeasibilityAnalyzer
{
    private readonly MathematicalModelIntrinsicFeasibilityAnalyzer
        _modelAnalyzer = new();

    public IntrinsicFeasibilityAnalysisResult AnalyzeModel(
        MathematicalModel model,
        double tolerance = 1.0e-9)
    {
        return _modelAnalyzer.Analyze(
            model,
            tolerance);
    }

    public async ValueTask<IntrinsicFeasibilityAnalysisResult>
        AnalyzeAsync(
            LotSizingInstance instance,
            IMathematicalModelFormulation formulation,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(formulation);

        MathematicalModel model =
            await formulation.BuildAsync(
                instance,
                cancellationToken);

        if (instance.HasClosedLoopReturnStreams)
        {
            instance.EnsureClosedLoopValid();

            model =
                new ClosedLoopSupplyNetworkModelDecorator()
                    .Apply(
                        instance,
                        model);
        }

        return _modelAnalyzer.Analyze(
            model);
    }

    public ValueTask<IntrinsicFeasibilityAnalysisResult>
        AnalyzeStandardAsync(
            LotSizingInstance instance,
            CancellationToken cancellationToken = default)
    {
        return AnalyzeAsync(
            instance,
            StandardLotSizingFormulationFactory.CreateDefault(),
            cancellationToken);
    }
}
