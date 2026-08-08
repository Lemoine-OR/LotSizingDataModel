using System;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Represents the result of a minimal native CPLEX smoke test.
/// </summary>
public sealed class CplexNativeSmokeTestResult
{
    /// <summary>
    /// Initializes a native CPLEX smoke-test result.
    /// </summary>
    /// <param name="isSuccessful">
    /// Indicates whether the native test succeeded.
    /// </param>
    /// <param name="solverVersion">
    /// Native CPLEX version.
    /// </param>
    /// <param name="status">
    /// Native CPLEX status text.
    /// </param>
    /// <param name="objectiveValue">
    /// Objective value, when available.
    /// </param>
    /// <param name="variableValue">
    /// Test-variable value, when available.
    /// </param>
    /// <param name="diagnostic">
    /// Human-readable diagnostic.
    /// </param>
    public CplexNativeSmokeTestResult(
        bool isSuccessful,
        string solverVersion,
        string status,
        double? objectiveValue,
        double? variableValue,
        string diagnostic)
    {
        IsSuccessful = isSuccessful;
        SolverVersion = solverVersion ?? string.Empty;
        Status = status ?? string.Empty;
        ObjectiveValue = objectiveValue;
        VariableValue = variableValue;
        Diagnostic = diagnostic ?? string.Empty;
    }

    /// <summary>
    /// Gets a value indicating whether CPLEX loaded, solved, and
    /// returned the expected solution.
    /// </summary>
    public bool IsSuccessful { get; }

    /// <summary>
    /// Gets the native CPLEX version.
    /// </summary>
    public string SolverVersion { get; }

    /// <summary>
    /// Gets the native solve status.
    /// </summary>
    public string Status { get; }

    /// <summary>
    /// Gets the objective value, when available.
    /// </summary>
    public double? ObjectiveValue { get; }

    /// <summary>
    /// Gets the test-variable value, when available.
    /// </summary>
    public double? VariableValue { get; }

    /// <summary>
    /// Gets the diagnostic message.
    /// </summary>
    public string Diagnostic { get; }
}
