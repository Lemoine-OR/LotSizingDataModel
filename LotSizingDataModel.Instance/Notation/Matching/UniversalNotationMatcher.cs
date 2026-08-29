using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Descriptors.Network;

namespace LotSizingDataModel.Instance.Notation.Matching;

/// <summary>
/// Matches a typed lot-sizing descriptor against a universal-notation
/// problem specification.
/// </summary>
/// <remarks>
/// Explicit specification tokens are requirements. Omitted beta tokens and
/// omitted optional network modifiers are unconstrained, not false.
///
/// Precedence is:
/// Contradiction > Incomplete > Exact > Compatible.
/// Exact is established by equality with the descriptor's generated canonical
/// notation after all explicit requirements have been satisfied.
/// </remarks>
public sealed class UniversalNotationMatcher
{
    private readonly UniversalNotationGenerator _generator;

    public UniversalNotationMatcher()
        : this(new UniversalNotationGenerator())
    {
    }

    public UniversalNotationMatcher(
        UniversalNotationGenerator generator)
    {
        _generator =
            generator ??
            throw new ArgumentNullException(nameof(generator));
    }

    public UniversalNotationMatchResult Match(
        LotSizingProblemDescriptor descriptor,
        string specificationText,
        string? schemeVersion = null)
    {
        ArgumentNullException.ThrowIfNull(descriptor);

        UniversalProblemSpecification specification =
            UniversalProblemSpecification.Parse(
                specificationText,
                schemeVersion);

        return Match(
            descriptor,
            specification);
    }

    public UniversalNotationMatchResult Match(
        LotSizingProblemDescriptor descriptor,
        UniversalProblemSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(descriptor);
        ArgumentNullException.ThrowIfNull(specification);

        UniversalLotSizingNotation generated =
            _generator.Generate(descriptor);

        string generatedText =
            generated.Render();

        var issues =
            new List<UniversalNotationMatchIssue>();

        MatchAlpha(
            generated.Alpha,
            specification.Notation.Alpha,
            issues);

        MatchBeta(
            generated.Beta,
            specification.Notation.Beta,
            issues);

        MatchGamma(
            generated.Gamma,
            specification.Notation.Gamma,
            issues);

        UniversalNotationMatchKind kind;

        if (issues.Any(issue => issue.IsContradiction))
        {
            kind =
                UniversalNotationMatchKind.Contradiction;
        }
        else if (issues.Count > 0)
        {
            kind =
                UniversalNotationMatchKind.Incomplete;
        }
        else if (
            string.Equals(
                generatedText,
                specification.CanonicalText,
                StringComparison.Ordinal))
        {
            kind =
                UniversalNotationMatchKind.Exact;
        }
        else
        {
            kind =
                UniversalNotationMatchKind.Compatible;
        }

        return new UniversalNotationMatchResult(
            kind,
            specification,
            generatedText,
            issues);
    }

    private static void MatchAlpha(
        UniversalNotationAlpha actual,
        UniversalNotationAlpha expected,
        ICollection<UniversalNotationMatchIssue> issues)
    {
        MatchEnumRequirement(
            actual.ItemCardinality,
            expected.ItemCardinality,
            UniversalItemCardinality.Unknown,
            "LSDM-MATCH-001",
            "LSDM-MATCH-002",
            "alpha.itemCardinality",
            issues);

        MatchEnumRequirement(
            actual.ProblemLevel,
            expected.ProblemLevel,
            UniversalProblemLevel.Unknown,
            "LSDM-MATCH-010",
            "LSDM-MATCH-011",
            "alpha.problemLevel",
            issues);

        if (
            expected.ProblemLevel ==
                UniversalProblemLevel.MultiLevel &&
            expected.ProductStructureType !=
                ProductStructureType.Unknown)
        {
            if (
                actual.ProductStructureType ==
                ProductStructureType.Unknown)
            {
                AddIncomplete(
                    issues,
                    "LSDM-MATCH-013",
                    "alpha.productStructure",
                    expected.ProductStructureType.ToString(),
                    "Unknown",
                    "The descriptor does not determine the BOM topology.");
            }
            else if (
                actual.ProductStructureType !=
                expected.ProductStructureType)
            {
                AddContradiction(
                    issues,
                    "LSDM-MATCH-012",
                    "alpha.productStructure",
                    expected.ProductStructureType.ToString(),
                    actual.ProductStructureType.ToString(),
                    "The known BOM topology contradicts the specification.");
            }
        }

        MatchNetwork(
            actual.Network,
            expected.Network,
            issues);
    }

    private static void MatchNetwork(
        UniversalNetworkNotation actual,
        UniversalNetworkNotation expected,
        ICollection<UniversalNotationMatchIssue> issues)
    {
        if (actual.Coupling != expected.Coupling)
        {
            AddContradiction(
                issues,
                "LSDM-MATCH-020",
                "alpha.network.coupling",
                expected.Coupling.ToString(),
                actual.Coupling.ToString(),
                "The physical-network coupling contradicts the specification.");

            return;
        }

        if (
            expected.ForwardTopology !=
                SupplyNetworkTopologyType.Unknown)
        {
            if (
                actual.ForwardTopology ==
                SupplyNetworkTopologyType.Unknown)
            {
                AddIncomplete(
                    issues,
                    "LSDM-MATCH-022",
                    "alpha.network.forwardTopology",
                    expected.ForwardTopology.ToString(),
                    "Unknown",
                    "The forward topology cannot currently be determined.");
            }
            else if (
                actual.ForwardTopology !=
                expected.ForwardTopology)
            {
                AddContradiction(
                    issues,
                    "LSDM-MATCH-021",
                    "alpha.network.forwardTopology",
                    expected.ForwardTopology.ToString(),
                    actual.ForwardTopology.ToString(),
                    "The known forward topology contradicts the specification.");
            }
        }

        if (
            expected.Coupling is
                NetworkCouplingType.ReverseOnly or
                NetworkCouplingType.ClosedLoop &&
            expected.ReverseTopology.HasValue &&
            expected.ReverseTopology.Value !=
                SupplyNetworkTopologyType.Unknown)
        {
            if (
                !actual.ReverseTopology.HasValue ||
                actual.ReverseTopology.Value ==
                    SupplyNetworkTopologyType.Unknown)
            {
                AddIncomplete(
                    issues,
                    "LSDM-MATCH-028",
                    "alpha.network.reverseTopology",
                    expected.ReverseTopology.Value.ToString(),
                    "Unknown",
                    "The reverse topology cannot currently be determined.");
            }
            else if (
                actual.ReverseTopology.Value !=
                expected.ReverseTopology.Value)
            {
                AddContradiction(
                    issues,
                    "LSDM-MATCH-029",
                    "alpha.network.reverseTopology",
                    expected.ReverseTopology.Value.ToString(),
                    actual.ReverseTopology.Value.ToString(),
                    "The known reverse topology contradicts the specification.");
            }
        }

        if (expected.EchelonCount.HasValue)
        {
            if (!actual.EchelonCount.HasValue)
            {
                if (actual.HasCycles)
                {
                    AddContradiction(
                        issues,
                        "LSDM-MATCH-023",
                        "alpha.network.echelonCount",
                        expected.EchelonCount.Value.ToString(),
                        "undefined(cyclic)",
                        "A cyclic physical network cannot satisfy the requested acyclic echelon count.");
                }
                else
                {
                    AddIncomplete(
                        issues,
                        "LSDM-MATCH-024",
                        "alpha.network.echelonCount",
                        expected.EchelonCount.Value.ToString(),
                        "Unknown",
                        "The descriptor does not determine an echelon count.");
                }
            }
            else if (
                actual.EchelonCount.Value !=
                expected.EchelonCount.Value)
            {
                AddContradiction(
                    issues,
                    "LSDM-MATCH-023",
                    "alpha.network.echelonCount",
                    expected.EchelonCount.Value.ToString(),
                    actual.EchelonCount.Value.ToString(),
                    "The known echelon count contradicts the specification.");
            }
        }

        RequirePositiveFlag(
            expected.HasCycles,
            actual.HasCycles,
            "LSDM-MATCH-025",
            "alpha.network.cycles",
            "CY",
            issues);

        RequirePositiveFlag(
            expected.HasMultiSourcing,
            actual.HasMultiSourcing,
            "LSDM-MATCH-026",
            "alpha.network.multiSourcing",
            "MS",
            issues);

        RequirePositiveFlag(
            expected.HasTransshipment,
            actual.HasTransshipment,
            "LSDM-MATCH-027",
            "alpha.network.transshipment",
            "TS",
            issues);
    }

    private static void MatchBeta(
        UniversalNotationBeta actual,
        UniversalNotationBeta expected,
        ICollection<UniversalNotationMatchIssue> issues)
    {
        foreach (
            UniversalNotationFeature requiredFeature
            in expected.Features)
        {
            if (!actual.Contains(requiredFeature))
            {
                AddContradiction(
                    issues,
                    "LSDM-MATCH-030",
                    "beta.features",
                    requiredFeature.ToString(),
                    "Absent",
                    "A required beta feature is not present in the descriptor.");
            }
        }
    }

    private static void MatchGamma(
        UniversalNotationGamma actual,
        UniversalNotationGamma expected,
        ICollection<UniversalNotationMatchIssue> issues)
    {
        if (
            expected.Objective ==
            UniversalObjectiveKind.Unknown)
        {
            return;
        }

        if (
            actual.Objective ==
            UniversalObjectiveKind.Unknown)
        {
            AddIncomplete(
                issues,
                "LSDM-MATCH-041",
                "gamma.objective",
                expected.Objective.ToString(),
                "Unknown",
                "The descriptor does not determine the requested objective family.");

            return;
        }

        if (
            actual.Objective !=
            expected.Objective)
        {
            AddContradiction(
                issues,
                "LSDM-MATCH-040",
                "gamma.objective",
                expected.Objective.ToString(),
                actual.Objective.ToString(),
                "The known objective family contradicts the specification.");
        }
    }

    private static void MatchEnumRequirement<T>(
        T actual,
        T expected,
        T unknown,
        string contradictionCode,
        string incompleteCode,
        string path,
        ICollection<UniversalNotationMatchIssue> issues)
        where T : struct, Enum
    {
        if (EqualityComparer<T>.Default.Equals(expected, unknown))
        {
            return;
        }

        if (EqualityComparer<T>.Default.Equals(actual, unknown))
        {
            AddIncomplete(
                issues,
                incompleteCode,
                path,
                expected.ToString(),
                unknown.ToString(),
                "The descriptor does not determine this requested characteristic.");

            return;
        }

        if (!EqualityComparer<T>.Default.Equals(actual, expected))
        {
            AddContradiction(
                issues,
                contradictionCode,
                path,
                expected.ToString(),
                actual.ToString(),
                "The known descriptor characteristic contradicts the specification.");
        }
    }

    private static void RequirePositiveFlag(
        bool expected,
        bool actual,
        string code,
        string path,
        string token,
        ICollection<UniversalNotationMatchIssue> issues)
    {
        if (expected && !actual)
        {
            AddContradiction(
                issues,
                code,
                path,
                token,
                "Absent",
                $"The specification explicitly requires network modifier {token}.");
        }
    }

    private static void AddContradiction(
        ICollection<UniversalNotationMatchIssue> issues,
        string code,
        string path,
        string expected,
        string actual,
        string message)
    {
        issues.Add(
            new UniversalNotationMatchIssue(
                code,
                path,
                expected,
                actual,
                message,
                isContradiction: true));
    }

    private static void AddIncomplete(
        ICollection<UniversalNotationMatchIssue> issues,
        string code,
        string path,
        string expected,
        string actual,
        string message)
    {
        issues.Add(
            new UniversalNotationMatchIssue(
                code,
                path,
                expected,
                actual,
                message,
                isContradiction: false));
    }
}
