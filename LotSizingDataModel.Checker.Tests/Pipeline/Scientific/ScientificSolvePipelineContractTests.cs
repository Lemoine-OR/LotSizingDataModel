using LotSizingDataModel.Checker.Pipeline.Scientific;

namespace LotSizingDataModel.Checker.Tests.Pipeline.Scientific;

public sealed class ScientificSolvePipelineContractTests
{
    [Fact]
    public void PipelineStatus_DoesNotConflateVerificationWithSolveCompletion()
    {
        Assert.NotEqual(
            ScientificSolvePipelineStatus.Completed,
            ScientificSolvePipelineStatus.PreflightRejected);

        Assert.NotEqual(
            ScientificSolvePipelineStatus.Completed,
            ScientificSolvePipelineStatus.FormulationDrift);

        Assert.NotEqual(
            ScientificSolvePipelineStatus.Completed,
            ScientificSolvePipelineStatus.ProvenanceCaptureFailed);
    }

    [Fact]
    public void DiagnosticNamespace_IsReservedForPipeline()
    {
        var diagnostic =
            new ScientificSolvePipelineDiagnostic(
                "LSDM-PIPE-999",
                ScientificSolvePipelineDiagnosticSeverity.Information,
                "test",
                "contract");

        Assert.StartsWith(
            "LSDM-PIPE-",
            diagnostic.Code,
            StringComparison.Ordinal);
    }
}
