using LotSizingDataModel.Instance.Notation.Matching;

namespace LotSizingDataModel.Instance.ProblemClasses;

/// <summary>
/// Canonical lot-sizing problem-class catalog.
/// </summary>
/// <remarks>
/// Codes are intentionally more explicit than historically ambiguous
/// acronyms such as CLSP or ULSP.
/// </remarks>
public static class LotSizingProblemClassCatalog
{
    public static LotSizingProblemClassDefinition
        SingleItemUncapacitated { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .SingleItemUncapacitatedLotSizing,
                "SI-ULS",
                "Single-item uncapacitated lot-sizing problem",
                "1,SL,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:?",
                aliases:
                    new[]
                    {
                        "SI-ULSP",
                        "ULS",
                        "ULSP"
                    },
                note:
                    "Canonical LSDM code disambiguates the item cardinality.");

    public static LotSizingProblemClassDefinition
        SingleItemCapacitated { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .SingleItemCapacitatedLotSizing,
                "SI-CLSP",
                "Single-item capacitated lot-sizing problem",
                "1,SL,Net:UNK | Dem,Det,Prod,Cap:P,SC | Obj:?",
                aliases:
                    new[]
                    {
                        "CSILSP",
                        "CLSP-SI",
                        "CLSP"
                    });

    public static LotSizingProblemClassDefinition
        MultiItemUncapacitated { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .MultiItemUncapacitatedLotSizing,
                "MI-ULS",
                "Multi-item uncapacitated single-level lot-sizing problem",
                "m,SL,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:?",
                aliases:
                    new[]
                    {
                        "MI-ULSP",
                        "ULS",
                        "ULSP"
                    },
                note:
                    "Without coupling constraints this class decomposes by item.");

    public static LotSizingProblemClassDefinition
        MultiItemCapacitated { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .MultiItemCapacitatedLotSizing,
                "MI-CLSP",
                "Multi-item capacitated single-level lot-sizing problem",
                "m,SL,Net:UNK | Dem,Det,Prod,Cap:P,Cap:Shared,SC | Obj:?",
                aliases:
                    new[]
                    {
                        "MCLSP",
                        "CLSP-MI",
                        "CLSP"
                    },
                note:
                    "The canonical multi-item class requires shared production capacity.");

    public static LotSizingProblemClassDefinition
        UncapacitatedMultiLevel { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .UncapacitatedMultiLevelLotSizing,
                "UMLSP",
                "Uncapacitated multi-level lot-sizing problem",
                "m,ML:?,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:?",
                aliases:
                    new[]
                    {
                        "MLLP",
                        "MLLS",
                        "UMLSP"
                    },
                note:
                    "MLLP is retained as an alias used in the project/literature corpus.");

    public static LotSizingProblemClassDefinition
        MultiLevelCapacitated { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .MultiLevelCapacitatedLotSizing,
                "MLCLSP",
                "Multi-level capacitated lot-sizing problem",
                "m,ML:?,Net:UNK | Dem,Det,Prod,Cap:P,SC | Obj:?",
                aliases:
                    new[]
                    {
                        "CMLLS",
                        "MLLS",
                        "MLCLSP"
                    });

    public static LotSizingProblemClassDefinition
        Dlsp { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .DiscreteLotSizingAndScheduling,
                "DLSP",
                "Discrete lot-sizing and scheduling problem",
                "m,SL,Net:UNK | Dem,Det,Prod,Cap:P,Cap:Shared,Sched," +
                "Bucket:SB,SchedRes:1,SBProd:0F,BucketItems:1 | Obj:?",
                note:
                    "Single-resource small-bucket class with at most one " +
                    "produced item per bucket and all-or-nothing production.");

    public static LotSizingProblemClassDefinition
        Cslp { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .ContinuousSetupLotSizing,
                "CSLP",
                "Continuous setup lot-sizing problem",
                "m,SL,Net:UNK | Dem,Det,Prod,Cap:P,Cap:Shared,Sched," +
                "Bucket:SB,SchedRes:1,SBProd:Cont,BucketItems:1 | Obj:?",
                note:
                    "Single-resource small-bucket class with at most one " +
                    "produced item per bucket and continuous lot quantity.");

    public static LotSizingProblemClassDefinition
        Plsp { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .ProportionalLotSizingAndScheduling,
                "PLSP",
                "Proportional lot-sizing and scheduling problem",
                "m,SL,Net:UNK | Dem,Det,Prod,Cap:P,Cap:Shared,Sched," +
                "Bucket:SB,MaxSetup,SchedRes:1,SBProd:Cont," +
                "BucketItems:2,SetupTrans:1 | Obj:?",
                note:
                    "Single-resource PLSP with one setup-state carry-over, " +
                    "continuous production of at most two items, and at most " +
                    "one setup transition per bucket.");

    public static LotSizingProblemClassDefinition
        Glsp { get; } =
            Executable(
                CanonicalLotSizingProblemClassId
                    .GeneralLotSizingAndScheduling,
                "GLSP",
                "General lot-sizing and scheduling problem",
                "m,SL,Net:UNK | Dem,Det,Prod,Cap:P,Cap:Shared,Sched," +
                "Bucket:MM,SchedRes:1,MicroGrid,MicroLen:Var," +
                "MicroItem:1 | Obj:?",
                note:
                    "Single-resource two-level macro/micro scheduling class " +
                    "with a predefined micro-period grid, variable micro-period " +
                    "lengths and one item/setup state per micro-period.");

    public static IReadOnlyList<LotSizingProblemClassDefinition>
        All { get; } =
            new[]
            {
                SingleItemUncapacitated,
                SingleItemCapacitated,
                MultiItemUncapacitated,
                MultiItemCapacitated,
                UncapacitatedMultiLevel,
                MultiLevelCapacitated,
                Dlsp,
                Cslp,
                Plsp,
                Glsp
            };

    public static IReadOnlyList<LotSizingProblemClassDefinition>
        ExecutableClasses =>
            All
                .Where(
                    definition =>
                        definition.SupportLevel ==
                        LotSizingProblemClassSupportLevel.Executable)
                .ToArray();

    public static IReadOnlyList<LotSizingProblemClassDefinition>
        ClassifiableClasses =>
            All
                .Where(
                    definition =>
                        definition.SupportLevel ==
                        LotSizingProblemClassSupportLevel.Classifiable)
                .ToArray();

    public static IReadOnlyList<LotSizingProblemClassDefinition>
        DetectableClasses =>
            All
                .Where(
                    definition =>
                        definition.SupportLevel !=
                        LotSizingProblemClassSupportLevel.CatalogOnly)
                .ToArray();

    public static IReadOnlyList<LotSizingProblemClassDefinition>
        SchedulingDetectableClasses =>
            new[]
            {
                Dlsp,
                Cslp,
                Plsp,
                Glsp
            };

    public static LotSizingProblemClassDefinition? FindById(
        CanonicalLotSizingProblemClassId id) =>
            All.FirstOrDefault(
                definition =>
                    definition.Id == id);

    public static LotSizingProblemClassDefinition? FindByCode(
        string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            return null;
        }

        return All.FirstOrDefault(
            definition =>
                definition.Code.Equals(
                    code.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }

    private static LotSizingProblemClassDefinition Executable(
        CanonicalLotSizingProblemClassId id,
        string code,
        string name,
        string specification,
        IEnumerable<string>? aliases = null,
        string? note = null) =>
            new(
                id,
                code,
                name,
                LotSizingProblemClassSupportLevel.Executable,
                aliases,
                UniversalProblemSpecification.Parse(specification),
                scientificNote: note);

    private static LotSizingProblemClassDefinition Classifiable(
        CanonicalLotSizingProblemClassId id,
        string code,
        string name,
        string? note = null) =>
            new(
                id,
                code,
                name,
                LotSizingProblemClassSupportLevel.Classifiable,
                aliases:
                    new[]
                    {
                        code
                    },
                scientificNote:
                    note);

    private static LotSizingProblemClassDefinition CatalogOnly(
        CanonicalLotSizingProblemClassId id,
        string code,
        string name,
        IEnumerable<string> gaps,
        string? note = null) =>
            new(
                id,
                code,
                name,
                LotSizingProblemClassSupportLevel.CatalogOnly,
                aliases: new[] { code },
                capabilityGaps: gaps,
                scientificNote: note);
}
