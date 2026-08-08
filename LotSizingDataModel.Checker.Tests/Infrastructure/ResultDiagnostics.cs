using System.Text;
using LotSizingDataModel.Checker.Results;

namespace LotSizingDataModel.Checker.Tests.Infrastructure;

internal static class ResultDiagnostics
{
    public static string Format(
        SolutionCheckResult result)
    {
        var builder = new StringBuilder();

        builder.Append("IsValid=")
            .Append(result.IsValid)
            .Append(", Structural=")
            .Append(result.IsStructurallyValid)
            .Append(", Domains=")
            .Append(result.AreVariableDomainsValid)
            .Append(", Feasible=")
            .Append(result.IsFeasible)
            .Append(", Objective=")
            .Append(result.IsObjectiveConsistent);

        foreach (SolutionCheckIssue issue in result.Issues)
        {
            builder.AppendLine();
            builder.Append(issue.Severity)
                .Append(" / ")
                .Append(issue.Kind)
                .Append(": ")
                .Append(issue.Message);
        }

        return builder.ToString();
    }
}
