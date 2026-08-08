using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Selects a compatible formulation and builds the associated
/// solver-independent mathematical model.
/// </summary>
public sealed class MathematicalModelBuildService :
    IMathematicalModelBuildService
{
    private readonly MathematicalModelFormulationSelectionService
        _selectionService =
            new();

    /// <summary>
    /// Selects a formulation and builds the mathematical model.
    /// </summary>
    /// <param name="instance">
    /// Lot-sizing instance to formulate.
    /// </param>
    /// <param name="registry">
    /// Registry containing available formulations.
    /// </param>
    /// <param name="options">
    /// Mathematical-model build options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel formulation selection and model
    /// construction.
    /// </param>
    /// <returns>
    /// Complete mathematical-model build result.
    /// </returns>
    public async ValueTask<MathematicalModelBuildResult> BuildAsync(
        LotSizingInstance instance,
        MathematicalModelFormulationRegistry registry,
        MathematicalModelBuildOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(registry);
        ArgumentNullException.ThrowIfNull(options);

        MathematicalModelBuildOptions normalizedOptions =
            options.Clone();

        normalizedOptions.EnsureValid();

        var stopwatch =
            Stopwatch.StartNew();

        MathematicalModelFormulationSelectionResult? selection =
            null;

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            selection =
                _selectionService.Select(
                    instance,
                    registry,
                    normalizedOptions.RequestedFormulationId,
                    normalizedOptions.AllowFallback);

            if (!selection.IsSuccessful ||
                selection.Formulation is null)
            {
                stopwatch.Stop();

                return MathematicalModelBuildResult.Failure(
                    selection,
                    "No compatible mathematical formulation " +
                    "could be selected.",
                    stopwatch.Elapsed);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var model =
                await selection.Formulation.BuildAsync(
                    instance,
                    cancellationToken);

            cancellationToken.ThrowIfCancellationRequested();

            ArgumentNullException.ThrowIfNull(model);

            if (normalizedOptions.ValidateGeneratedModel)
            {
                model.EnsureValid();
            }

            if (normalizedOptions.CloneGeneratedModel)
            {
                model =
                    model.Clone();
            }

            stopwatch.Stop();

            MathematicalModelBuildResult result =
                MathematicalModelBuildResult.Success(
                    selection,
                    model,
                    stopwatch.Elapsed);

            result.AddDiagnostic(
                $"Mathematical model '{model.Name}' was built " +
                $"with {model.VariableCount} variables and " +
                $"{model.EnabledConstraintCount} enabled " +
                "constraints.");

            return result;
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            MathematicalModelBuildResult result =
                MathematicalModelBuildResult.Failure(
                    selection,
                    "Mathematical model construction was " +
                    "cancelled.",
                    stopwatch.Elapsed);

            result.AddDiagnostic(
                "The cancellation token was signalled during " +
                "formulation selection or model construction.");

            return result;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            MathematicalModelBuildResult result =
                MathematicalModelBuildResult.Failure(
                    selection,
                    exception.Message,
                    stopwatch.Elapsed);

            result.AddDiagnostic(
                exception.ToString());

            return result;
        }
    }
}
