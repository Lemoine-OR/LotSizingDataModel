using System;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Mapping;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.Evaluation;

/// <summary>
/// Recomputes the objective from the exact normalized numerical
/// representation used by the business solution mapping layer.
/// </summary>
public static class MathematicalObjectiveRecalculator
{
    private const double DefaultAbsoluteObjectiveTolerance = 1.0e-8;
    private const double DefaultRelativeObjectiveTolerance = 1.0e-9;

    /// <summary>
    /// Recomputes and verifies the objective value.
    /// </summary>
    public static MathematicalObjectiveRecalculationResult Recalculate(
        MathematicalModel model,
        MathematicalModelSolveResult solveResult,
        MathematicalSolutionMappingOptions mappingOptions)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(solveResult);
        ArgumentNullException.ThrowIfNull(mappingOptions);

        model.EnsureValid();
        solveResult.EnsureValid();
        mappingOptions.EnsureValid();

        var result =
            new MathematicalObjectiveRecalculationResult
            {
                SolverObjectiveValue =
                    solveResult.ObjectiveValue,
                Status =
                    ObjectiveVerificationStatus.NotChecked
            };

        var normalizer =
            new MathematicalVariableValueNormalizer(
                mappingOptions.ZeroTolerance,
                MathematicalVariableValueNormalizer
                    .DefaultIntegralityTolerance,
                MathematicalVariableValueNormalizer
                    .DefaultNearIntegerTolerance);

        try
        {
            double objective =
                model.Objective.Expression.Constant;

            foreach (
                LinearTerm term
                in model.Objective.Expression.Terms)
            {
                MathematicalVariable? variable =
                    model.FindVariableById(
                        term.VariableId);

                if (variable is null)
                {
                    throw new InvalidOperationException(
                        $"Objective term references unknown variable " +
                        $"identifier '{term.VariableId}'.");
                }

                MathematicalVariableValue? rawValue =
                    solveResult.FindVariableValue(
                        term.VariableId);

                if (rawValue is null)
                {
                    throw new InvalidOperationException(
                        $"No solver value is available for objective " +
                        $"variable '{variable.Name}' " +
                        $"(identifier {variable.Id}).");
                }

                double normalizedValue =
                    normalizer.Normalize(
                        variable,
                        rawValue.Value);

                objective +=
                    term.Coefficient *
                    normalizedValue;
            }

            if (!double.IsFinite(objective))
            {
                throw new InvalidOperationException(
                    "The recomputed objective value is not finite.");
            }

            result.RecomputedObjectiveValue =
                objective;

            if (!solveResult.ObjectiveValue.HasValue)
            {
                result.Status =
                    ObjectiveVerificationStatus.Consistent;

                result.Diagnostics.Add(
                    "The objective was recomputed successfully, " +
                    "but no solver-reported objective value was " +
                    "available for comparison.");

                return result;
            }

            double reported =
                solveResult.ObjectiveValue.Value;

            double difference =
                Math.Abs(
                    reported - objective);

            double scale =
                Math.Max(
                    1.0,
                    Math.Max(
                        Math.Abs(reported),
                        Math.Abs(objective)));

            double tolerance =
                Math.Max(
                    DefaultAbsoluteObjectiveTolerance,
                    DefaultRelativeObjectiveTolerance * scale);

            result.AbsoluteDifference =
                difference;

            result.ComparisonTolerance =
                tolerance;

            result.Status =
                difference <= tolerance
                    ? ObjectiveVerificationStatus.Consistent
                    : ObjectiveVerificationStatus.Inconsistent;

            result.Diagnostics.Add(
                result.Status ==
                    ObjectiveVerificationStatus.Consistent
                    ? $"Solver objective and recomputed objective " +
                      $"are consistent. Absolute difference=" +
                      $"{difference:G17}; tolerance={tolerance:G17}."
                    : $"Solver objective and recomputed objective " +
                      $"are inconsistent. Absolute difference=" +
                      $"{difference:G17}; tolerance={tolerance:G17}.");

            return result;
        }
        catch (Exception exception)
        {
            result.Status =
                ObjectiveVerificationStatus.Failed;

            result.Diagnostics.Add(
                "Objective-value post-processing failed: " +
                exception.Message);

            return result;
        }
    }
}
