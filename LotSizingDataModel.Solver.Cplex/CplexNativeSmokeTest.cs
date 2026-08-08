using System;
using NativeCplex = global::ILOG.CPLEX.Cplex;
using ILOG.Concert;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Runs a minimal native CPLEX optimization to validate managed
/// assemblies, native libraries, and license availability.
/// </summary>
/// <remarks>
/// The test solves:
/// <code>
/// minimize x
/// subject to x &gt;= 1
/// x &gt;= 0.
/// </code>
/// The expected solution is <c>x = 1</c> with objective value
/// <c>1</c>.
/// </remarks>
public static class CplexNativeSmokeTest
{
    /// <summary>
    /// Executes the native smoke test.
    /// </summary>
    /// <returns>
    /// Native smoke-test result.
    /// </returns>
    public static CplexNativeSmokeTestResult Run()
    {
        NativeCplex? cplex = null;

        try
        {
            cplex = new NativeCplex();

            INumVar x =
                cplex.NumVar(
                    0.0,
                    double.MaxValue,
                    "x");

            cplex.AddMinimize(
                x,
                "minimize_x");

            cplex.AddGe(
                x,
                1.0,
                "x_at_least_one");

            bool solved =
                cplex.Solve();

            if (!solved)
            {
                return new CplexNativeSmokeTestResult(
                    false,
                    cplex.Version,
                    cplex.GetStatus().ToString(),
                    null,
                    null,
                    "CPLEX loaded correctly but did not return a " +
                    "solution for the smoke-test model.");
            }

            double xValue =
                cplex.GetValue(x);

            double objectiveValue =
                cplex.ObjValue;

            bool expectedSolution =
                Math.Abs(xValue - 1.0) <= 1e-7 &&
                Math.Abs(objectiveValue - 1.0) <= 1e-7;

            return new CplexNativeSmokeTestResult(
                expectedSolution,
                cplex.Version,
                cplex.GetStatus().ToString(),
                objectiveValue,
                xValue,
                expectedSolution
                    ? "CPLEX native smoke test succeeded."
                    : "CPLEX solved the model but returned an " +
                      "unexpected result.");
        }
        catch (System.Exception exception)
        {
            return new CplexNativeSmokeTestResult(
                false,
                cplex?.Version ?? string.Empty,
                string.Empty,
                null,
                null,
                exception.ToString());
        }
        finally
        {
            cplex?.End();
        }
    }
}
