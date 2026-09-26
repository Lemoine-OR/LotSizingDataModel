using LotSizingDataModel.Solver.Xpress;
using LotSizingDataModel.Solver.Modeling;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Common;

/// <summary>Optional licensed Xpress regressions, run with --xpress-smoke.</summary>
public static class XpressNativeSmokeTests
{
    public static void Verify(MathematicalModel model, MathematicalModelSolveResult result)
    {
        if (!result.HasFeasibleSolution) throw new Exception("Expected a feasible solution");
        var values = result.VariableValues.ToDictionary(v => v.VariableId, v => v.Value);
        foreach (var v in model.Variables)
        {
            if (!values.TryGetValue(v.Id, out var x) || !double.IsFinite(x)) throw new Exception($"Missing/nonfinite {v.Name}");
            if (x < v.LowerBound - 1e-6 || x > v.UpperBound + 1e-6) throw new Exception($"Bound violation {v.Name}");
            if (v.VariableType != MathematicalVariableType.Continuous && Math.Abs(x-Math.Round(x))>1e-6) throw new Exception($"Integrality violation {v.Name}");
        }
        double Eval(LinearExpression e) => e.Constant + e.Terms.Sum(t => t.Coefficient * values[t.VariableId]);
        foreach (var c in model.Constraints)
        {
            var delta=Eval(c.LeftHandSide)-c.RightHandSide;
            bool bad=c.Sense switch { MathematicalConstraintSense.Equal => Math.Abs(delta)>1e-6, MathematicalConstraintSense.LessThanOrEqual => delta>1e-6, _ => delta < -1e-6 };
            if (bad) throw new Exception($"Constraint violation {c.Name}: {delta}");
        }
        if (result.ObjectiveValue is not double obj || Math.Abs(Eval(model.Objective.Expression)-obj)>1e-6) throw new Exception("Objective recomputation mismatch");
    }
    public static async Task RunAsync()
    {
        foreach (bool maximize in new[]{false,true})
        {
            var m=new MathematicalModel {Name="column_mapping"};
            m.Variables.Add(new MathematicalVariable(20,"z_first",MathematicalVariableType.Continuous,0,10));
            m.Variables.Add(new MathematicalVariable(3,"a_second",MathematicalVariableType.Continuous,0,10));
            m.Objective.Sense=maximize?ObjectiveSense.Maximize:ObjectiveSense.Minimize;
            m.Objective.Expression.AddTerm(3,3); // Deliberately reverse declaration and objective order.
            m.Objective.Expression.AddTerm(20,2);
            var sum=new LinearExpression(); sum.AddTerm(20,1);sum.AddTerm(3,1);
            m.Constraints.Add(new LinearConstraint(1,"sum",sum,MathematicalConstraintSense.Equal,4));
            var r=await new XpressSolverAdapter().SolveAsync(new MathematicalModelSolveRequest {Model=m});
            Verify(m,r);
            var expected=maximize?12:8;
            if (!r.IsOptimal || Math.Abs(r.ObjectiveValue!.Value-expected)>1e-6) throw new Exception("LP optimum mismatch");
            Console.WriteLine($"LP {(maximize?"max":"min")}: PASS; objective={r.ObjectiveValue}; "+string.Join(",",r.VariableValues.Select(v=>$"{v.VariableName}={v.Value}")));
        }
        var mip=new MathematicalModel {Name="integer_binary"};
        mip.Variables.Add(new MathematicalVariable(1,"integer",MathematicalVariableType.Integer,0,10));
        mip.Variables.Add(new MathematicalVariable(2,"binary",MathematicalVariableType.Binary,0,1));
        mip.Objective.Sense=ObjectiveSense.Maximize;
        mip.Objective.Expression.AddTerm(1,3);mip.Objective.Expression.AddTerm(2,2);
        var budget=new LinearExpression();budget.AddTerm(1,2);budget.AddTerm(2,2);
        mip.Constraints.Add(new LinearConstraint(1,"budget",budget,MathematicalConstraintSense.LessThanOrEqual,5));
        var mr=await new XpressSolverAdapter().SolveAsync(new MathematicalModelSolveRequest {Model=mip});
        Verify(mip,mr);
        if (!mr.IsOptimal || Math.Abs(mr.ObjectiveValue!.Value-6)>1e-6) throw new Exception("MIP optimum mismatch");
        Console.WriteLine($"Integer/binary MIP: PASS; objective={mr.ObjectiveValue}");
        var unbounded=new MathematicalModel {Name="unbounded"};
        unbounded.Variables.Add(new MathematicalVariable(1,"x",MathematicalVariableType.Continuous));
        unbounded.Objective.Sense=ObjectiveSense.Maximize; unbounded.Objective.Expression.AddTerm(1,1);
        var ur=await new XpressSolverAdapter().SolveAsync(new MathematicalModelSolveRequest {Model=unbounded});
        if(ur.TerminationReason!=SolverTerminationReason.Unbounded || ur.HasFeasibleSolution || ur.ObjectiveValue is not null || ur.VariableValues.Count != 0) throw new Exception($"Unbounded status mismatch: {ur.TerminationReason}");
        Console.WriteLine("Unbounded LP: PASS");
        Console.WriteLine("Generic native regressions: 4/4 passed.");
    }
}
