namespace LotSizingDataModel.Solver.Feasibility;

public readonly record struct LinearExpressionBoundInterval(
    double Minimum,
    double Maximum)
{
    public bool IsInformative =>
        !double.IsNaN(Minimum) &&
        !double.IsNaN(Maximum) &&
        Minimum <= Maximum;
}
