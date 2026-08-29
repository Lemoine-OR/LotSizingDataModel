using LotSizingDataModel.Instance.Scientific;
using LotSizingDataModel.Solver.Formulation.Scientific;

namespace LotSizingDataModel.Solver.Resolution.Scientific;

/// <summary>
/// Assesses scientific method-family relevance independently from solver
/// backend availability.
/// </summary>
public sealed class ScientificSolutionMethodCompatibilityService
{
    public ScientificSolutionMethodCandidate Assess(
        ScientificClassificationResult classification,
        ScientificFormulationSelectionResult formulationSelection,
        ScientificSolutionMethodDefinition method)
    {
        ArgumentNullException.ThrowIfNull(classification);
        ArgumentNullException.ThrowIfNull(formulationSelection);
        ArgumentNullException.ThrowIfNull(method);

        if (classification.IsBlocked)
        {
            return new ScientificSolutionMethodCandidate(
                method,
                ScientificSolutionMethodCompatibilityKind.Blocked,
                "Scientific classification is blocked.");
        }

        if (classification.PrimaryProblemClass is null)
        {
            return new ScientificSolutionMethodCandidate(
                method,
                ScientificSolutionMethodCompatibilityKind.Undetermined,
                "No unique canonical problem class is available.");
        }

        var problemClass =
            classification.PrimaryProblemClass.Definition.Id;

        if (!method.IsApplicableTo(problemClass))
        {
            return new ScientificSolutionMethodCandidate(
                method,
                ScientificSolutionMethodCompatibilityKind.Incompatible,
                $"Method family '{method.MethodId}' is not catalogued for " +
                $"canonical problem class '{problemClass}'.");
        }

        if (
            method.SupportLevel ==
            ScientificSolutionMethodSupportLevel.CatalogOnly)
        {
            return new ScientificSolutionMethodCandidate(
                method,
                ScientificSolutionMethodCompatibilityKind.CatalogOnlyRelevant,
                "Scientifically relevant family, but no executable adapter " +
                "is connected to LotSizingDataModel yet.");
        }

        if (
            method.RequiresMathematicalFormulation &&
            !formulationSelection.IsSuccessful)
        {
            return new ScientificSolutionMethodCandidate(
                method,
                ScientificSolutionMethodCompatibilityKind.Undetermined,
                "The executable method requires a scientifically selected " +
                "mathematical formulation.");
        }

        return new ScientificSolutionMethodCandidate(
            method,
            ScientificSolutionMethodCompatibilityKind.ExecutableCompatible,
            "Executable method has compatible canonical problem-class and " +
            "formulation support.");
    }
}
