namespace LotSizingDataModel.Instance.Historical;

public static class HistoricalClassificationMappingRegistryFactory
{
    public const string BaselineRegistryVersion =
        "1.0-alpha.41";

    public static HistoricalClassificationMappingRegistry
        CreateValidatedBaseline()
    {
        var registry =
            new HistoricalClassificationMappingRegistry(
                BaselineRegistryVersion);

        AddWolseyMappings(
            registry);

        registry.EnsureValid();

        return registry;
    }

    private static void AddWolseyMappings(
        HistoricalClassificationMappingRegistry registry)
    {
        const string source =
            "Wolsey 2002 mapping consolidated by validated LotSizingDataModel historical milestones through alpha.32.";

        AddExact(registry, "WOLSEY-CAP-U", "CAP=U",
            new[] { "Uncap:P" }, source);

        AddExact(registry, "WOLSEY-CAP-CC", "CAP=CC",
            new[] { "Cap:P", "TP:CapP=C" }, source);

        AddExact(registry, "WOLSEY-CAP-C", "CAP=C",
            new[] { "Cap:P", "Cap:Var" }, source);

        AddExact(registry, "WOLSEY-VAR-B", "VAR=B",
            new[] { "BL" }, source);

        AddExact(registry, "WOLSEY-VAR-SC", "VAR=SC",
            new[] { "SU" }, source);

        AddExact(registry, "WOLSEY-VAR-ST", "VAR=ST",
            new[] { "SUT" }, source);

        AddExact(registry, "WOLSEY-VAR-ST-C", "VAR=ST(C)",
            new[] { "SUT", "TP:SUT=C" }, source);

        AddExact(registry, "WOLSEY-VAR-LB", "VAR=LB",
            new[] { "MinLot" }, source);

        AddExact(registry, "WOLSEY-VAR-SS", "VAR=SS",
            new[] { "SS" }, source);

        AddExact(registry, "WOLSEY-PROB-WW", "PROB=WW",
            new[] { "Cost:NS" }, source);

        AddExact(registry, "WOLSEY-VAR-SL", "VAR=SL",
            new[] { "SalesOption" }, source);

        AddExact(registry, "WOLSEY-SQT", "SQT",
            new[] { "SDCT" }, source);

        AddExact(registry, "WOLSEY-SQC", "SQC",
            new[] { "SDCC" }, source);

        AddExact(registry, "WOLSEY-SET", "SET",
            new[] { "SetupTime" }, source);

        AddConservative(registry, "WOLSEY-SB1", "SB1",
            new[] { "Scheduling:SB1" }, source);

        AddConservative(registry, "WOLSEY-SB2", "SB2",
            new[] { "Scheduling:SB2" }, source);

        AddConservative(registry, "WOLSEY-BB", "BB",
            new[] { "Scheduling:BB" }, source);

        AddSourceOnly(
            registry,
            "WOLSEY-IM",
            "IM",
            source,
            "Machine label retained from the source only; no automatic generic semantics are inferred.");

        AddSourceOnly(
            registry,
            "WOLSEY-VM",
            "VM",
            source,
            "Machine label retained from the source only; no automatic generic semantics are inferred.");
    }

    private static void AddExact(
        HistoricalClassificationMappingRegistry registry,
        string ruleId,
        string historicalToken,
        IEnumerable<string> universalTokens,
        string source)
    {
        registry.AddRule(
            new HistoricalMappingRule(
                ruleId,
                HistoricalClassificationFamily.Wolsey,
                historicalToken,
                universalTokens,
                HistoricalMappingConfidence.Exact,
                allowsInverse: true,
                sourceReference: source));
    }

    private static void AddConservative(
        HistoricalClassificationMappingRegistry registry,
        string ruleId,
        string historicalToken,
        IEnumerable<string> universalTokens,
        string source)
    {
        registry.AddRule(
            new HistoricalMappingRule(
                ruleId,
                HistoricalClassificationFamily.Wolsey,
                historicalToken,
                universalTokens,
                HistoricalMappingConfidence.Conservative,
                allowsInverse: false,
                sourceReference: source,
                notes:
                    "Conservative projection; inverse detection is intentionally disabled."));
    }

    private static void AddSourceOnly(
        HistoricalClassificationMappingRegistry registry,
        string ruleId,
        string historicalToken,
        string source,
        string notes)
    {
        registry.AddRule(
            new HistoricalMappingRule(
                ruleId,
                HistoricalClassificationFamily.Wolsey,
                historicalToken,
                Array.Empty<string>(),
                HistoricalMappingConfidence.SourceOnly,
                allowsInverse: false,
                sourceReference: source,
                notes: notes));
    }
}
