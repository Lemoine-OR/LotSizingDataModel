using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Selects a compatible mathematical formulation for a
/// lot-sizing instance.
/// </summary>
public sealed class MathematicalModelFormulationSelectionService
{
    /// <summary>
    /// Selects a mathematical formulation.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to formulate.
    /// </param>
    /// <param name="registry">
    /// Formulation registry.
    /// </param>
    /// <param name="requestedFormulationId">
    /// Optional requested formulation identifier. An empty value
    /// enables automatic selection.
    /// </param>
    /// <param name="allowFallback">
    /// Indicates whether another compatible formulation may be
    /// selected when the requested formulation is unavailable or
    /// incompatible.
    /// </param>
    /// <returns>
    /// Formulation-selection result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="instance"/> or
    /// <paramref name="registry"/> is <see langword="null"/>.
    /// </exception>
    public MathematicalModelFormulationSelectionResult Select(
        LotSizingInstance instance,
        MathematicalModelFormulationRegistry registry,
        string requestedFormulationId = "",
        bool allowFallback = true)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        ArgumentNullException.ThrowIfNull(
            registry);

        string normalizedRequestedFormulationId =
            requestedFormulationId?.Trim() ??
            string.Empty;

        var diagnostics =
            new List<string>();

        if (!string.IsNullOrWhiteSpace(
                normalizedRequestedFormulationId))
        {
            if (registry.TryGet(
                    normalizedRequestedFormulationId,
                    out IMathematicalModelFormulation?
                        requestedFormulation))
            {
                if (requestedFormulation!.CanBuild(
                        instance))
                {
                    MathematicalModelFormulationSelectionResult
                        success =
                            MathematicalModelFormulationSelectionResult
                                .Success(
                                    normalizedRequestedFormulationId,
                                    requestedFormulation,
                                    usedFallback:
                                        false);

                    success.AddDiagnostic(
                        $"Requested formulation " +
                        $"'{requestedFormulation.FormulationId}' " +
                        "was selected.");

                    return success;
                }

                diagnostics.Add(
                    $"Requested formulation " +
                    $"'{requestedFormulation.FormulationId}' does " +
                    "not support the supplied lot-sizing " +
                    "instance.");
            }
            else
            {
                diagnostics.Add(
                    $"Requested formulation " +
                    $"'{normalizedRequestedFormulationId}' is not " +
                    "registered.");
            }

            if (!allowFallback)
            {
                MathematicalModelFormulationSelectionResult
                    failure =
                        MathematicalModelFormulationSelectionResult
                            .Failure(
                                normalizedRequestedFormulationId,
                                "No compatible requested " +
                                "formulation could be selected.");

                AddDiagnostics(
                    failure,
                    diagnostics);

                return failure;
            }
        }

        IMathematicalModelFormulation? fallbackFormulation =
            registry.GetAll()
                .FirstOrDefault(
                    formulation =>
                        formulation.CanBuild(
                            instance));

        if (fallbackFormulation is null)
        {
            MathematicalModelFormulationSelectionResult failure =
                MathematicalModelFormulationSelectionResult
                    .Failure(
                        normalizedRequestedFormulationId,
                        "No registered mathematical formulation " +
                        "supports the supplied lot-sizing " +
                        "instance.");

            AddDiagnostics(
                failure,
                diagnostics);

            return failure;
        }

        bool usedFallback =
            !string.IsNullOrWhiteSpace(
                normalizedRequestedFormulationId);

        MathematicalModelFormulationSelectionResult result =
            MathematicalModelFormulationSelectionResult.Success(
                normalizedRequestedFormulationId,
                fallbackFormulation,
                usedFallback);

        AddDiagnostics(
            result,
            diagnostics);

        result.AddDiagnostic(
            usedFallback
                ? $"Fallback formulation " +
                  $"'{fallbackFormulation.FormulationId}' was " +
                  "selected."
                : $"Formulation " +
                  $"'{fallbackFormulation.FormulationId}' was " +
                  "selected automatically.");

        return result;
    }

    private static void AddDiagnostics(
        MathematicalModelFormulationSelectionResult result,
        IEnumerable<string> diagnostics)
    {
        foreach (
            string diagnostic
            in diagnostics)
        {
            result.AddDiagnostic(
                diagnostic);
        }
    }
}
