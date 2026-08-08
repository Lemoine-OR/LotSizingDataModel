using System;
using System.Collections.Generic;
using System.Linq;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Provides default conventional installation paths for the
/// supported mathematical optimization solvers.
/// </summary>
/// <remarks>
/// These paths are only discovery hints. A solver may still be
/// detected through explicit configuration, environment
/// variables, the operating-system PATH, or a custom search
/// directory.
/// </remarks>
public static class SolverInstallationPathCatalog
{
    private static readonly IReadOnlyList<
        SolverInstallationPathDefinition> Definitions =
            CreateDefinitions();

    /// <summary>
    /// Gets all default installation-path definitions.
    /// </summary>
    public static IReadOnlyList<
        SolverInstallationPathDefinition> All =>
            Definitions;

    /// <summary>
    /// Gets the default installation-path definitions
    /// associated with a concrete solver.
    /// </summary>
    /// <param name="solverKind">
    /// Solver kind.
    /// </param>
    /// <returns>
    /// Installation-path definitions ordered by priority.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="solverKind"/> is
    /// <see cref="SolverKind.Unknown"/> or
    /// <see cref="SolverKind.Automatic"/>.
    /// </exception>
    public static IReadOnlyList<
        SolverInstallationPathDefinition> ForSolver(
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
                    definition.PathPattern,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static IReadOnlyList<
        SolverInstallationPathDefinition>
        CreateDefinitions()
    {
        return new[]
        {
            new SolverInstallationPathDefinition(
                SolverKind.Cplex,
                @"%ProgramFiles%\IBM\ILOG",
                "Common IBM ILOG installation directory.")
            {
                Priority =
                    100,

                RecursiveSearch =
                    true
            },

            new SolverInstallationPathDefinition(
                SolverKind.Cplex,
                @"%ProgramFiles(x86)%\IBM\ILOG",
                "Common 32-bit IBM ILOG installation " +
                "directory.")
            {
                Priority =
                    110,

                RecursiveSearch =
                    true
            },

            new SolverInstallationPathDefinition(
                SolverKind.Gurobi,
                @"%ProgramFiles%\Gurobi",
                "Common Gurobi installation directory.")
            {
                Priority =
                    100,

                RecursiveSearch =
                    true
            },

            new SolverInstallationPathDefinition(
                SolverKind.Gurobi,
                @"C:\gurobi",
                "Alternative Gurobi installation root.")
            {
                Priority =
                    110,

                RecursiveSearch =
                    true
            },

            new SolverInstallationPathDefinition(
                SolverKind.Xpress,
                @"%ProgramFiles%\FICO",
                "Common FICO installation directory.")
            {
                Priority =
                    100,

                RecursiveSearch =
                    true
            },

            new SolverInstallationPathDefinition(
                SolverKind.Xpress,
                @"%ProgramFiles%\FICO Xpress",
                "Alternative FICO Xpress installation " +
                "directory.")
            {
                Priority =
                    110,

                RecursiveSearch =
                    true
            },

            new SolverInstallationPathDefinition(
                SolverKind.CoinOrCbc,
                @"%ProgramFiles%\COIN-OR",
                "Common COIN-OR installation directory.")
            {
                Priority =
                    100,

                RecursiveSearch =
                    true,

                ExpectedFileName =
                    "cbc.exe"
            },

            new SolverInstallationPathDefinition(
                SolverKind.CoinOrCbc,
                @"C:\coin-or",
                "Alternative COIN-OR installation root.")
            {
                Priority =
                    110,

                RecursiveSearch =
                    true,

                ExpectedFileName =
                    "cbc.exe"
            }
        };
    }
}
