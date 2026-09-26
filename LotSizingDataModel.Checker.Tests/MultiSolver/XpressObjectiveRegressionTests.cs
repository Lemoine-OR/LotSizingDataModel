using System.Reflection;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Modeling;
using LotSizingDataModel.Solver.Xpress;

namespace LotSizingDataModel.Checker.Tests.MultiSolver;

/// <summary>Reproduces objective attributes exposed by native Xpress without requiring a license in CI.</summary>
public sealed class XpressObjectiveRegressionTests
{
    /// <summary>LP objectives must not use the unset MIP incumbent sentinel.</summary>
    [Theory]
    [InlineData(false, 8)]
    [InlineData(true, 6)]
    public void ObjectiveUsesTheSolvedModelType(bool integer, double expected)
    {
        var result = new MathematicalModelSolveResult {HasFeasibleSolution=true, IsOptimal=true};
        Populate(result, integer);
        Assert.Equal(expected, result.ObjectiveValue);
        Assert.Equal(expected, result.BestBound);
        Assert.Equal(0, result.AbsoluteGap);
        Assert.Equal(0, result.RelativeGap);
    }

    /// <summary>No incumbent objective or gap can be published without a feasible solution.</summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void NoSolutionHasNoObjective(bool integer)
    {
        var result = new MathematicalModelSolveResult();
        Populate(result, integer);
        Assert.Null(result.ObjectiveValue);
        Assert.Null(result.AbsoluteGap);
        Assert.Null(result.RelativeGap);
    }

    private static void Populate(MathematicalModelSolveResult result, bool integer)
    {
        var model = new MathematicalModel();
        model.Variables.Add(new MathematicalVariable(1,"x",integer ? MathematicalVariableType.Integer : MathematicalVariableType.Continuous));
        var apiType = typeof(XpressSolverAdapter).Assembly.GetType("LotSizingDataModel.Solver.Xpress.XpressReflectionApi", true)!;
        var api = Activator.CreateInstance(apiType, BindingFlags.Instance | BindingFlags.NonPublic, null, new object[]{typeof(object),typeof(Attributes)}, null)!;
        var method = typeof(XpressSolverAdapter).GetMethod("PopulateObjectiveAndGap", BindingFlags.Static | BindingFlags.NonPublic)!;
        method.Invoke(null, new object[]{result, model, new Dictionary<int,double>(), new Attributes(), api});
    }

    /// <summary>Native attributes: LP has a valid solution while the MIP best incumbent is unset.</summary>
    public sealed class Attributes
    {
        /// <summary>Current LP objective.</summary>
        public double LPObjVal => 8;
        /// <summary>Current integer solution objective.</summary>
        public double MIPObjVal => 6;
        /// <summary>Unset best incumbent sentinel observed with Xpress 47.01.01.</summary>
        public double MIPBestObjVal => 1e40;
        /// <summary>MIP bound, unrelated to the pure LP objective.</summary>
        public double BestBound => 6;
    }
}
