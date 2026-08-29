namespace LotSizingDataModel.Instance.Scientific;

/// <summary>
/// Declares exactly which scientific axes this engine infers.
/// </summary>
public sealed class ScientificClassificationCoverage
{
    public static ScientificClassificationCoverage Current { get; } =
        new();

    private ScientificClassificationCoverage()
    {
    }

    public ScientificClassificationAxisStatus GetStatus(
        ScientificClassificationAxis axis)
    {
        return axis switch
        {
            ScientificClassificationAxis.StructuralProperties =>
                ScientificClassificationAxisStatus.Analyzed,

            ScientificClassificationAxis.ProblemClasses =>
                ScientificClassificationAxisStatus.Analyzed,

            ScientificClassificationAxis.HistoricalClassifications =>
                ScientificClassificationAxisStatus.CapabilityOnly,

            ScientificClassificationAxis.PlanningParadigms =>
                ScientificClassificationAxisStatus.NotInferred,

            ScientificClassificationAxis.MathematicalFormulations =>
                ScientificClassificationAxisStatus.NotInferred,

            ScientificClassificationAxis.SolutionMethods =>
                ScientificClassificationAxisStatus.NotInferred,

            _ => throw new ArgumentOutOfRangeException(
                nameof(axis),
                axis,
                "Unknown scientific classification axis.")
        };
    }

    public bool InfersPlanningParadigms =>
        false;

    public bool InfersMathematicalFormulations =>
        false;

    public bool InfersSolutionMethods =>
        false;
}
