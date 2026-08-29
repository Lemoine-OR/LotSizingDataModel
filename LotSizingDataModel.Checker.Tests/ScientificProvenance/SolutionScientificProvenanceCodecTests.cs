using LotSizingDataModel.Solution.Metadata;
using LotSizingDataModel.Solution.Metadata.Scientific;

namespace LotSizingDataModel.Checker.Tests.ScientificProvenance;

public sealed class SolutionScientificProvenanceCodecTests
{
    [Fact]
    public void Codec_RoundTripsThroughExistingGenerationParameters()
    {
        var metadata =
            new SolutionGenerationMetadata();

        var provenance =
            new SolutionScientificProvenance
            {
                DetectedNotation =
                    "1,SL,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:Econ",
                CanonicalProblemClassCode =
                    "SI-ULS",
                ProblemClassMatchKind =
                    "ExactCore",
                FormulationId =
                    "standard",
                FormulationFamily =
                    "Standard solver-independent MILP",
                FormulationScientificCompatibility =
                    "Compatible",
                CapturedAtUtc =
                    new DateTime(
                        2026,
                        8,
                        29,
                        17,
                        0,
                        0,
                        DateTimeKind.Utc)
            };

        SolutionScientificProvenanceCodec.Write(
            metadata,
            provenance);

        SolutionScientificProvenanceReadResult read =
            SolutionScientificProvenanceCodec.Read(
                metadata);

        Assert.Equal(
            SolutionScientificProvenanceReadKind.Valid,
            read.Kind);

        Assert.Equal(
            provenance.DetectedNotation,
            read.Provenance!.DetectedNotation);

        Assert.Equal(
            "SI-ULS",
            read.Provenance.CanonicalProblemClassCode);

        Assert.True(
            metadata.HasParameters);
    }

    [Fact]
    public void MissingReservedParameters_ReturnsMissing()
    {
        Assert.Equal(
            SolutionScientificProvenanceReadKind.Missing,
            SolutionScientificProvenanceCodec
                .Read(new SolutionGenerationMetadata())
                .Kind);
    }

    [Fact]
    public void UnsupportedSchema_ReturnsInvalid()
    {
        var metadata =
            new SolutionGenerationMetadata();

        metadata.SetParameter(
            LotSizingDataModel.Solution.Common.AlgorithmParameter
                .FromString(
                    SolutionScientificProvenanceCodec
                        .SchemaVersionParameter,
                    "999"));

        Assert.Equal(
            SolutionScientificProvenanceReadKind.Invalid,
            SolutionScientificProvenanceCodec
                .Read(metadata)
                .Kind);
    }

    [Fact]
    public void Write_IsIdempotentForReservedNames()
    {
        var metadata =
            new SolutionGenerationMetadata();

        var first =
            new SolutionScientificProvenance
            {
                DetectedNotation =
                    "1,SL,Net:UNK | Dem,Prod,Uncap:P,SC | Obj:Econ",
                CanonicalProblemClassCode = "SI-ULS",
                ProblemClassMatchKind = "ExactCore",
                FormulationId = "standard",
                FormulationScientificCompatibility = "Compatible"
            };

        var second =
            new SolutionScientificProvenance
            {
                DetectedNotation =
                    "1,SL,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:Econ",
                CanonicalProblemClassCode = "SI-ULS",
                ProblemClassMatchKind = "ExactCore",
                FormulationId = "standard",
                FormulationScientificCompatibility = "Compatible"
            };

        SolutionScientificProvenanceCodec.Write(metadata, first);
        SolutionScientificProvenanceCodec.Write(metadata, second);

        Assert.Equal(
            SolutionScientificProvenanceCodec.ReservedParameterNames.Count,
            metadata.Parameters.Count(
                parameter =>
                    parameter.Name.StartsWith(
                        SolutionScientificProvenanceCodec.ParameterPrefix,
                        StringComparison.OrdinalIgnoreCase)));

        Assert.Equal(
            second.DetectedNotation,
            SolutionScientificProvenanceCodec
                .Read(metadata)
                .Provenance!
                .DetectedNotation);
    }
}
