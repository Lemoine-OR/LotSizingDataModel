using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Execution;

namespace LotSizingDataModel.Checker.Pipeline.Scientific;

/// <summary>
/// Request for the end-to-end scientific solve pipeline.
/// </summary>
public sealed class ScientificSolvePipelineRequest
{
    public ScientificSolvePipelineRequest(
        SolverRequest solverRequest,
        ScientificClassificationRequest? classificationRequest = null,
        bool verifyNumerically = true,
        bool verifyProvenance = true)
    {
        SolverRequest =
            solverRequest ??
            throw new ArgumentNullException(nameof(solverRequest));

        ClassificationRequest =
            classificationRequest ??
            new ScientificClassificationRequest();

        VerifyNumerically = verifyNumerically;
        VerifyProvenance = verifyProvenance;
    }

    public SolverRequest SolverRequest { get; }

    public ScientificClassificationRequest ClassificationRequest { get; }

    public bool VerifyNumerically { get; }

    public bool VerifyProvenance { get; }
}
