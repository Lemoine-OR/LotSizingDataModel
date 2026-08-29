using LotSizingDataModel.Instance.Classification;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Descriptors;
using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Metadata.Scientific;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Formulation.Scientific;
using LotSizingDataModel.Solver.Mapping.Scientific;
using LotSizingDataModel.Solver.Resolution.Scientific;

namespace LotSizingDataModel.Checker.Tests.ScientificProvenance;

public sealed class ScientificSolutionProvenanceV2Tests
{
    [Fact]
    public void V2Mapper_RecordsMethodAndBackend()
    {
        ScientificClassificationResult classification =
            new ScientificClassificationEngine()
                .Analyze(
                    LotSizingProblemDescriptor.FromLegacyFeatures(
                        new LotSizingProblemFeatures
                        {
                            ItemCount = 1,
                            PlanningHorizon = 6,
                            ProductStructureRelationshipCount = 0,
                            ProductStructureType =
                                ProductStructureType.IndependentItems,
                            HasDemand = true,
                            HasDeterministicDemand = true,
                            HasProduction = true,
                            HasSetupCosts = true
                        }));

        var registry =
            new MathematicalModelFormulationRegistry();

        registry.Register(
            StandardLotSizingFormulationFactory.CreateDefault());

        ScientificFormulationSelectionResult selection =
            new ScientificFormulationSelectionService()
                .Select(
                    classification,
                    registry);

        var solution =
            new LotSizingSolution(6);

        SolutionScientificProvenance provenance =
            ScientificSolutionProvenanceMapper.Apply(
                solution,
                selection,
                ScientificSolutionMethodCatalog.GeneralMilp,
                SolverKind.Cplex);

        Assert.Equal(
            SolutionScientificProvenance.CurrentSchemaVersion,
            provenance.SchemaVersion);

        Assert.Equal(
            "MILP-GENERAL",
            provenance.SolutionMethodId);

        Assert.Equal(
            SolverKind.Cplex.ToString(),
            provenance.SolverBackendKind);

        Assert.True(
            provenance.HasResolutionMethodEvidence);
    }
}
