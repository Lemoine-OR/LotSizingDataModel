using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.External;

/// <summary>
/// Shared helpers used by optional native-solver adapters.
/// </summary>
public static class ExternalSolverResultUtilities
{
    /// <summary>
    /// Populates generic mathematical variable values from a dictionary keyed by
    /// mathematical variable identifier.
    /// </summary>
    /// <param name="result">Result to populate.</param>
    /// <param name="model">Source mathematical model.</param>
    /// <param name="values">Solver values keyed by mathematical variable ID.</param>
    public static void PopulateVariableValues(
        MathematicalModelSolveResult result,
        MathematicalModel model,
        IReadOnlyDictionary<int, double> values)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(values);

        foreach (MathematicalVariable variable in model.Variables)
        {
            if (!values.TryGetValue(variable.Id, out double value))
            {
                continue;
            }

            result.AddVariableValue(
                new MathematicalVariableValue(
                    variable.Id,
                    value,
                    variable.Name,
                    variable.DomainKey));
        }
    }

    /// <summary>
    /// Evaluates the mathematical objective from a complete or partial value map.
    /// </summary>
    /// <param name="model">Mathematical model.</param>
    /// <param name="values">Values keyed by mathematical variable ID.</param>
    /// <param name="objectiveValue">Evaluated objective when all referenced values exist.</param>
    /// <returns>
    /// <see langword="true"/> when every objective variable has a supplied value.
    /// </returns>
    public static bool TryEvaluateObjective(
        MathematicalModel model,
        IReadOnlyDictionary<int, double> values,
        out double objectiveValue)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentNullException.ThrowIfNull(values);

        double value =
            model.Objective.Expression.Constant;

        foreach (LinearTerm term in model.Objective.Expression.Terms)
        {
            if (!values.TryGetValue(term.VariableId, out double variableValue))
            {
                objectiveValue = default;
                return false;
            }

            value +=
                term.Coefficient * variableValue;
        }

        objectiveValue = value;
        return double.IsFinite(value);
    }

    /// <summary>
    /// Computes absolute and relative objective gaps when an incumbent and bound
    /// are available.
    /// </summary>
    /// <param name="result">Result to update.</param>
    public static void PopulateGapStatistics(
        MathematicalModelSolveResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (!result.ObjectiveValue.HasValue ||
            !result.BestBound.HasValue)
        {
            return;
        }

        result.AbsoluteGap =
            Math.Abs(
                result.ObjectiveValue.Value -
                result.BestBound.Value);

        result.RelativeGap =
            result.AbsoluteGap.Value /
            (1.0e-10 +
             Math.Abs(result.ObjectiveValue.Value));
    }

    /// <summary>
    /// Creates an isolated temporary directory for one solver invocation.
    /// </summary>
    /// <param name="solverToken">Short solver-specific directory token.</param>
    /// <returns>Created temporary directory.</returns>
    public static string CreateTemporaryDirectory(
        string solverToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(solverToken);

        string directory =
            Path.Combine(
                Path.GetTempPath(),
                "LotSizingDataModel",
                solverToken,
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(directory);
        return directory;
    }

    /// <summary>
    /// Removes a temporary directory without masking a solver result or error.
    /// </summary>
    /// <param name="directory">Directory to remove.</param>
    public static void TryDeleteDirectory(
        string directory)
    {
        if (string.IsNullOrWhiteSpace(directory))
        {
            return;
        }

        try
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(
                    directory,
                    recursive: true);
            }
        }
        catch
        {
            // Temporary cleanup is deliberately best effort.
        }
    }

    /// <summary>
    /// Copies the generated portable LP model to the user-requested export path.
    /// </summary>
    /// <param name="generatedLpPath">Generated portable LP path.</param>
    /// <param name="configuredExportPath">Configured export path.</param>
    public static void ExportPortableModel(
        string generatedLpPath,
        string configuredExportPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(generatedLpPath);

        string path =
            string.IsNullOrWhiteSpace(configuredExportPath)
                ? Path.GetFullPath("LotSizingModel.lp")
                : Path.GetFullPath(configuredExportPath);

        string? directory =
            Path.GetDirectoryName(path);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(
            generatedLpPath,
            path,
            overwrite: true);
    }
}
