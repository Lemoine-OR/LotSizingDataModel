namespace LotSizingDataModel.Solver.Mapping;

public sealed class ClosedLoopDecisionSnapshot
{
    public ClosedLoopDecisionSnapshot(
        int returnStreamId,
        int period,
        double recoveryInput,
        double disposalQuantity,
        double recoveredOutput)
    {
        ReturnStreamId =
            returnStreamId;

        Period =
            period;

        RecoveryInput =
            recoveryInput;

        DisposalQuantity =
            disposalQuantity;

        RecoveredOutput =
            recoveredOutput;

        if (ReturnStreamId <= 0 ||
            Period <= 0 ||
            !double.IsFinite(RecoveryInput) ||
            !double.IsFinite(DisposalQuantity) ||
            !double.IsFinite(RecoveredOutput) ||
            RecoveryInput < 0.0 ||
            DisposalQuantity < 0.0 ||
            RecoveredOutput < 0.0)
        {
            throw new InvalidOperationException(
                "Closed-loop decision snapshot is invalid.");
        }
    }

    public int ReturnStreamId
    {
        get;
    }

    public int Period
    {
        get;
    }

    public double RecoveryInput
    {
        get;
    }

    public double DisposalQuantity
    {
        get;
    }

    public double RecoveredOutput
    {
        get;
    }
}
