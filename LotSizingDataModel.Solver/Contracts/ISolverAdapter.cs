using System.Collections.Generic;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Contracts;

/// <summary>
/// Defines the metadata and capabilities exposed by a solver
/// adapter plugin.
/// </summary>
/// <remarks>
/// A solver adapter is responsible for connecting the generic
/// solver layer to one native optimization engine such as
/// CPLEX, Gurobi, FICO Xpress, or COIN-OR CBC.
/// </remarks>
public interface ISolverAdapter :
    ILotSizingSolver
{
    /// <summary>
    /// Gets the unique adapter identifier.
    /// </summary>
    string AdapterId
    {
        get;
    }

    /// <summary>
    /// Gets the adapter display name.
    /// </summary>
    string AdapterName
    {
        get;
    }

    /// <summary>
    /// Gets the adapter implementation version.
    /// </summary>
    string AdapterVersion
    {
        get;
    }

    /// <summary>
    /// Gets the minimum supported solver version, when one is
    /// defined.
    /// </summary>
    string MinimumSupportedSolverVersion
    {
        get;
    }

    /// <summary>
    /// Gets the solver capabilities implemented by this
    /// adapter.
    /// </summary>
    IReadOnlyCollection<SolverCapability> Capabilities
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether this adapter supports
    /// the specified capability.
    /// </summary>
    /// <param name="capability">
    /// Capability to test.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the capability is supported;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    bool SupportsCapability(
        SolverCapability capability);
}
