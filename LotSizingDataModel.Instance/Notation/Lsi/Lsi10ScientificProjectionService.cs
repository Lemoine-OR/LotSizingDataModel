using LotSizingDataModel.Core;
using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Descriptors;

namespace LotSizingDataModel.Instance.Notation.Lsi;

/// <summary>
/// End-to-end LSI/1.0 projection service using the stable 1.2.x semantic path.
/// </summary>
public sealed class Lsi10ScientificProjectionService
{
    public Lsi10Projection Project(
        SupplyChain supplyChain)
    {
        ArgumentNullException.ThrowIfNull(supplyChain);

        LotSizingProblemFeatures features =
            LotSizingProblemFeatureExtractor.Extract(
                supplyChain);

        LotSizingProblemDescriptor descriptor =
            LotSizingProblemDescriptor.FromLegacyFeatures(
                features,
                supplyChain);

        UniversalLotSizingNotation universal =
            new UniversalNotationGenerator()
                .Generate(descriptor);

        return new Lsi10ScientificProjector()
            .Project(
                descriptor,
                universal);
    }
}
