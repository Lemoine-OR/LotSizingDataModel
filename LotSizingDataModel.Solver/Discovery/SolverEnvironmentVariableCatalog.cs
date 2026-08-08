using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Provides the default catalog of solver-specific environment
/// variables inspected during solver discovery.
/// </summary>
/// <remarks>
/// The catalog contains generic, non-version-specific variable
/// names. Applications may supplement it with additional
/// version-specific or organization-specific definitions.
/// </remarks>
public static class SolverEnvironmentVariableCatalog
{
    private static readonly IReadOnlyList<
        SolverEnvironmentVariableDefinition> Definitions =
            CreateDefinitions();

    /// <summary>
    /// Gets all default environment-variable definitions.
    /// </summary>
    public static IReadOnlyList<
        SolverEnvironmentVariableDefinition> All =>
            Definitions;

    /// <summary>
    /// Gets the default environment-variable definitions
    /// associated with a solver.
    /// </summary>
    /// <param name="solverKind">
    /// Concrete solver kind.
    /// </param>
    /// <returns>
    /// Definitions associated with the selected solver.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="solverKind"/> is
    /// <see cref="SolverKind.Unknown"/> or
    /// <see cref="SolverKind.Automatic"/>.
    /// </exception>
    public static IReadOnlyList<
        SolverEnvironmentVariableDefinition> ForSolver(
            SolverKind solverKind)
    {
        if (solverKind is
            SolverKind.Unknown or
            SolverKind.Automatic)
        {
            throw new ArgumentOutOfRangeException(
                nameof(solverKind),
                solverKind,
                "A concrete solver kind is required.");
        }

        return Definitions
            .Where(
                definition =>
                    definition.SolverKind ==
                    solverKind)
            .OrderBy(
                definition =>
                    definition.Priority)
            .ThenBy(
                definition =>
                    definition.VariableName,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Gets a definition by environment-variable name.
    /// </summary>
    /// <param name="variableName">
    /// Environment-variable name.
    /// </param>
    /// <returns>
    /// Matching definition, or <see langword="null"/> when the
    /// name is not included in the default catalog.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="variableName"/> is empty.
    /// </exception>
    public static SolverEnvironmentVariableDefinition? Find(
        string variableName)
    {
        if (string.IsNullOrWhiteSpace(
                variableName))
        {
            throw new ArgumentException(
                "An environment-variable name is required.",
                nameof(variableName));
        }

        return Definitions.FirstOrDefault(
            definition =>
                string.Equals(
                    definition.VariableName,
                    variableName.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }

    private static IReadOnlyList<
        SolverEnvironmentVariableDefinition>
        CreateDefinitions()
    {
        return new[]
        {
            new SolverEnvironmentVariableDefinition(
                SolverKind.Cplex,
                "CPLEX_STUDIO_DIR",
                "IBM ILOG CPLEX Optimization Studio " +
                "installation directory.")
            {
                Priority =
                    10
            },

            new SolverEnvironmentVariableDefinition(
                SolverKind.Gurobi,
                "GUROBI_HOME",
                "Gurobi Optimizer installation directory.")
            {
                Priority =
                    10
            },

            new SolverEnvironmentVariableDefinition(
                SolverKind.Gurobi,
                "GRB_LICENSE_FILE",
                "Gurobi license file or license-server " +
                "configuration.")
            {
                IsLicenseVariable =
                    true,

                Priority =
                    20
            },

            new SolverEnvironmentVariableDefinition(
                SolverKind.Xpress,
                "XPRESSDIR",
                "FICO Xpress installation directory.")
            {
                Priority =
                    10
            },

            new SolverEnvironmentVariableDefinition(
                SolverKind.Xpress,
                "XPAUTH_PATH",
                "FICO Xpress authorization or license path.")
            {
                IsLicenseVariable =
                    true,

                Priority =
                    20
            }
        };
    }
}
