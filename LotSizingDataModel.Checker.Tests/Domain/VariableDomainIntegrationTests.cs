using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Domain;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Checker.Tests.Infrastructure;

namespace LotSizingDataModel.Checker.Tests.Domain;

public sealed class VariableDomainIntegrationTests
{
    [Fact]
    public void BinarySetupValueEqualToTwo_IsRejected()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        object productionDecision =
            data.Solution.ProductionDecisions
                .Cast<object>()
                .First();

        ReflectionMutation.SetFirstNumericSeriesValue(
            productionDecision,
            "Setups",
            2.0);

        var checker =
            new SolutionVariableDomainChecker();

        SolutionCheckResult result =
            checker.Check(
                data.Solution,
                new SolutionCheckOptions());

        Assert.False(result.AreVariableDomainsValid);
        Assert.Contains(
            result.Issues,
            issue =>
                issue.Kind == SolutionCheckIssueKind.VariableDomain);
    }
}
