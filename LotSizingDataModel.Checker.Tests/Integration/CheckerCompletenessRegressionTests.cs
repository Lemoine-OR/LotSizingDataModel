using System.Collections;
using System.Reflection;
using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Orchestration;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Checker.Tests.Infrastructure;

namespace LotSizingDataModel.Checker.Tests.Integration;

/// <summary>
/// Regression tests for the independent checker's completeness semantics.
/// </summary>
public sealed class CheckerCompletenessRegressionTests
{
    [Fact]
    public async Task ReferenceSolution_FullCheck_DoesNotReportLegacySol005Warning()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        var checker =
            new LotSizingSolutionChecker();

        SolutionCheckResult result =
            await checker.CheckAsync(
                data.Instance,
                data.Solution,
                new SolutionCheckOptions
                {
                    Level = SolutionCheckLevel.Full
                });

        Assert.True(
            result.IsValid,
            ResultDiagnostics.Format(result));

        Assert.DoesNotContain(
            result.Issues,
            issue =>
                issue.Message.StartsWith(
                    "SOL005:",
                    StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task MissingProductionDecision_FailsIndependentCompletenessCheck()
    {
        ReferenceFixtureData data =
            ReferenceFixture.Load();

        RemoveFirstItem(
            data.Solution,
            "ProductionDecisions");

        var checker =
            new LotSizingSolutionChecker();

        SolutionCheckResult result =
            await checker.CheckAsync(
                data.Instance,
                data.Solution,
                new SolutionCheckOptions
                {
                    Level = SolutionCheckLevel.Structural
                });

        Assert.True(result.StructuralCheckCompleted);
        Assert.False(result.IsStructurallyValid);
        Assert.False(result.IsValid);

        Assert.Contains(
            result.Issues,
            issue =>
                issue.Kind == SolutionCheckIssueKind.Structural &&
                issue.Message.StartsWith(
                    "Missing production decision",
                    StringComparison.OrdinalIgnoreCase));

        Assert.DoesNotContain(
            result.Issues,
            issue =>
                issue.Message.StartsWith(
                    "SOL005:",
                    StringComparison.OrdinalIgnoreCase));
    }

    private static void RemoveFirstItem(
        object owner,
        string propertyName)
    {
        ArgumentNullException.ThrowIfNull(owner);

        PropertyInfo property =
            owner.GetType().GetProperty(
                propertyName,
                BindingFlags.Instance | BindingFlags.Public) ??
            throw new InvalidOperationException(
                $"Property '{propertyName}' was not found on " +
                $"{owner.GetType().FullName}.");

        object collection =
            property.GetValue(owner) ??
            throw new InvalidOperationException(
                $"Collection '{propertyName}' is null.");

        if (collection is IList list)
        {
            if (list.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Collection '{propertyName}' is empty.");
            }

            list.RemoveAt(0);
            return;
        }

        if (collection is not IEnumerable enumerable)
        {
            throw new InvalidOperationException(
                $"Property '{propertyName}' is not an enumerable collection.");
        }

        object firstItem =
            enumerable.Cast<object>().FirstOrDefault() ??
            throw new InvalidOperationException(
                $"Collection '{propertyName}' is empty.");

        MethodInfo? removeMethod =
            collection.GetType()
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .FirstOrDefault(
                    method =>
                        method.Name == "Remove" &&
                        method.GetParameters().Length == 1 &&
                        method.GetParameters()[0].ParameterType
                            .IsInstanceOfType(firstItem));

        if (removeMethod is null)
        {
            throw new InvalidOperationException(
                $"Collection '{propertyName}' does not expose a usable " +
                "Remove/RemoveAt operation for this regression test.");
        }

        removeMethod.Invoke(
            collection,
            [firstItem]);
    }
}
