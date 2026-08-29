using LotSizingDataModel.Instance.Descriptors.Network;

namespace LotSizingDataModel.Instance.Notation;

/// <summary>
/// Renders the canonical text form of notation scheme version 1.
/// </summary>
public static class UniversalNotationRenderer
{
    public static string Render(
        UniversalLotSizingNotation notation)
    {
        ArgumentNullException.ThrowIfNull(notation);

        return
            $"{RenderAlpha(notation.Alpha)} | " +
            $"{RenderBeta(notation.Beta)} | " +
            $"{RenderGamma(notation.Gamma)}";
    }

    private static string RenderAlpha(
        UniversalNotationAlpha alpha)
    {
        string cardinality =
            alpha.ItemCardinality switch
            {
                UniversalItemCardinality.Single => "1",
                UniversalItemCardinality.Multiple => "m",
                _ => "?"
            };

        string productStructure =
            alpha.ProblemLevel switch
            {
                UniversalProblemLevel.SingleLevel => "SL",
                UniversalProblemLevel.MultiLevel =>
                    "ML:" +
                    UniversalNotationTokenCatalog
                        .GetProductStructureCode(
                            alpha.ProductStructureType),
                _ => "Level:?"
            };

        return string.Join(
            ",",
            new[]
            {
                cardinality,
                productStructure,
                RenderNetwork(alpha.Network)
            });
    }

    private static string RenderNetwork(
        UniversalNetworkNotation network)
    {
        string core =
            network.Coupling switch
            {
                NetworkCouplingType.ForwardOnly =>
                    "Net:" +
                    UniversalNotationTokenCatalog
                        .GetNetworkTopologyCode(
                            network.ForwardTopology),

                NetworkCouplingType.ReverseOnly =>
                    "Net:R:" +
                    UniversalNotationTokenCatalog
                        .GetNetworkTopologyCode(
                            network.ReverseTopology ??
                            SupplyNetworkTopologyType.Unknown),

                NetworkCouplingType.ClosedLoop =>
                    "Net:CL(F:" +
                    UniversalNotationTokenCatalog
                        .GetNetworkTopologyCode(
                            network.ForwardTopology) +
                    ";R:" +
                    UniversalNotationTokenCatalog
                        .GetNetworkTopologyCode(
                            network.ReverseTopology ??
                            SupplyNetworkTopologyType.Unknown) +
                    ")",

                _ => throw new ArgumentOutOfRangeException(
                    nameof(network),
                    network.Coupling,
                    "Unknown network coupling type.")
            };

        var modifiers = new List<string>();

        if (network.EchelonCount.HasValue)
        {
            modifiers.Add($"E{network.EchelonCount.Value}");
        }

        if (network.HasCycles)
        {
            modifiers.Add("CY");
        }

        if (network.HasMultiSourcing)
        {
            modifiers.Add("MS");
        }

        if (network.HasTransshipment)
        {
            modifiers.Add("TS");
        }

        return modifiers.Count == 0
            ? core
            : core + ":" + string.Join(":", modifiers);
    }

    private static string RenderBeta(
        UniversalNotationBeta beta)
    {
        var tokens =
            new List<string>();

        tokens.AddRange(
            beta.Features
                .OrderBy(feature => (int)feature)
                .Select(
                    UniversalNotationTokenCatalog.GetFeatureToken));

        tokens.AddRange(
            beta.SemanticConditions
                .OrderBy(condition => (int)condition)
                .Select(
                    UniversalNotationTokenCatalog
                        .GetSemanticConditionToken));

        tokens.AddRange(
            beta.TemporalQualifiers
                .OrderBy(
                    qualifier =>
                        (int)qualifier.Parameter)
                .Select(
                    UniversalNotationTokenCatalog
                        .GetTemporalQualifierToken));

        return tokens.Count == 0
            ? "None"
            : string.Join(",", tokens);
    }

    private static string RenderGamma(
        UniversalNotationGamma gamma)
    {
        return gamma.Objective switch
        {
            UniversalObjectiveKind.Economic => "Obj:Econ",
            UniversalObjectiveKind.MultipleObjectives => "Obj:Multi",
            _ => "Obj:?"
        };
    }
}
