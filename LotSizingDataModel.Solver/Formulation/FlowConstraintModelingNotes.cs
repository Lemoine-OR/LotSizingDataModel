namespace LotSizingDataModel.Solver.Formulation;

/// <summary>
/// Documents the period conventions used by the standard flow
/// formulation.
/// </summary>
/// <remarks>
/// <para>
/// Production variables are interpreted as production starts.
/// A routing with lead time <c>L</c> started in period <c>t</c>
/// reaches its plant warehouse in period <c>t + L</c>.
/// </para>
/// <para>
/// Procurement variables are interpreted as supplier orders.
/// A supplier delivery with lead time <c>L</c> ordered in period
/// <c>t</c> reaches its destination warehouse in period
/// <c>t + L</c>.
/// </para>
/// <para>
/// Transport variables are interpreted as departures. A
/// transport quantity leaves the origin in its decision period
/// and reaches the destination after the lane lead time.
/// </para>
/// <para>
/// Bill-of-material component consumption occurs in the parent
/// production-start period. This convention is deliberately
/// independent from the parent routing completion lead time.
/// </para>
/// </remarks>
internal static class FlowConstraintModelingNotes
{
}
