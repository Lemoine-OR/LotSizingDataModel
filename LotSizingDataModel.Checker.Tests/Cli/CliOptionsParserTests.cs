using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Cli;

namespace LotSizingDataModel.Checker.Tests.Cli;

public sealed class CliOptionsParserTests
{
    [Fact]
    public void Parse_ValidArguments_UsesInvariantNumerics()
    {
        CliParseResult result =
            CliOptionsParser.Parse(
            [
                @"C:\Benchmarks",
                "--level=feasibility",
                "--parallelism",
                "3",
                "--objective-abs-tol=1.25e-7",
                "--feasibility-tol",
                "2.5e-8",
                "--no-progress"
            ]);

        Assert.True(result.Success, result.ErrorMessage);
        Assert.NotNull(result.Options);

        CliOptions options =
            result.Options!;

        Assert.Equal(@"C:\Benchmarks", options.InputDirectory);
        Assert.Equal(3, options.MaxDegreeOfParallelism);
        Assert.Equal(
            SolutionCheckLevel.Feasibility,
            options.CheckOptions.Level);
        Assert.Equal(
            1.25e-7,
            options.CheckOptions.ObjectiveAbsoluteTolerance);
        Assert.Equal(
            2.5e-8,
            options.CheckOptions.FeasibilityTolerance);
        Assert.False(options.ShowProgress);
    }

    [Fact]
    public void Parse_NegativeTolerance_IsRejected()
    {
        CliParseResult result =
            CliOptionsParser.Parse(
            [
                @"C:\Benchmarks",
                "--feasibility-tol=-1e-6"
            ]);

        Assert.False(result.Success);
        Assert.True(
            (result.ErrorMessage ?? string.Empty).Contains(
                "non-negative",
                StringComparison.OrdinalIgnoreCase),
            result.ErrorMessage);
    }
}
