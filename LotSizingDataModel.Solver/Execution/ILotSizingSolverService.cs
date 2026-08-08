using System.Threading;
using System.Threading.Tasks;

namespace LotSizingDataModel.Solver.Execution;

/// <summary>
/// Defines the high-level service used to solve a complete
/// lot-sizing instance from a normalized solver request.
/// </summary>
/// <remarks>
/// Implementations are responsible for orchestrating the complete
/// workflow:
/// <list type="number">
/// <item>
/// <description>
/// selecting a mathematical formulation and building the
/// solver-independent model;
/// </description>
/// </item>
/// <item>
/// <description>
/// selecting and executing an available solver adapter;
/// </description>
/// </item>
/// <item>
/// <description>
/// mapping the mathematical solution back to a normalized
/// lot-sizing solution;
/// </description>
/// </item>
/// <item>
/// <description>
/// returning a single <see cref="SolverRunResult"/> containing
/// the solution, solver statistics, and diagnostics.
/// </description>
/// </item>
/// </list>
/// </remarks>
public interface ILotSizingSolverService
{
    /// <summary>
    /// Solves a lot-sizing instance described by the supplied
    /// request.
    /// </summary>
    /// <param name="request">
    /// Complete normalized solver request.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel model construction, solver
    /// execution, or solution mapping.
    /// </param>
    /// <returns>
    /// Task returning the normalized solver-run result.
    /// </returns>
    /// <exception cref="System.ArgumentNullException">
    /// Thrown when <paramref name="request"/> is
    /// <see langword="null"/>.
    /// </exception>
    /// <exception cref="System.InvalidOperationException">
    /// Thrown when the request is invalid or when the configured
    /// solver infrastructure cannot execute the request.
    /// </exception>
    ValueTask<SolverRunResult> SolveAsync(
        SolverRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests interruption of the currently running solve
    /// operation, when supported by the selected solver adapter.
    /// </summary>
    /// <remarks>
    /// Calling this method when no solve is running has no
    /// effect.
    /// </remarks>
    void RequestStop();
}
