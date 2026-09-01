namespace LotSizingDataModel.Instance.Historical;

public sealed class HistoricalMappingAuditService
{
    public HistoricalMappingAuditResult Audit(
        HistoricalClassificationFamily family,
        IEnumerable<string> declaredTokens,
        IEnumerable<string> detectedTokens)
    {
        return new HistoricalMappingAuditResult(
            family,
            declaredTokens,
            detectedTokens);
    }
}
