using System.Reflection;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Regression;

public sealed class PublicApiIntegrationSurfaceTests
{
    private static readonly (string Assembly, string Type)[]
        CriticalTypes =
        [
            (
                "LotSizingDataModel.Instance",
                "LotSizingDataModel.Instance.LotSizingInstance"
            ),
            (
                "LotSizingDataModel.Solution",
                "LotSizingDataModel.Solution.LotSizingSolution"
            ),
            (
                "LotSizingDataModel.Solver",
                "LotSizingDataModel.Solver.Modeling.MathematicalModel"
            ),
            (
                "LotSizingDataModel.Checker",
                "LotSizingDataModel.Checker.Facade.LotSizingSolutionVerificationService"
            ),
            (
                "LotSizingDataModel.Checker.Campaign",
                "LotSizingDataModel.Checker.Campaign.DirectoryVerificationCampaignService"
            ),
            (
                "LotSizingDataModel.Instance",
                "LotSizingDataModel.Instance.Historical.HistoricalClassificationMappingRegistry"
            ),
            (
                "LotSizingDataModel.Instance",
                "LotSizingDataModel.Instance.Benchmark.BenchmarkRunProvenance"
            ),
            (
                "LotSizingDataModel.Checker.Campaign",
                "LotSizingDataModel.Checker.Campaign.Benchmark.BenchmarkCampaignReportWriter"
            )
        ];

    [Fact]
    public void CriticalConsumerTypes_RemainPublicAndResolvable()
    {
        foreach ((string assemblyName, string typeName)
                 in CriticalTypes)
        {
            Assembly assembly =
                Assembly.Load(
                    assemblyName);

            Type? type =
                assembly.GetType(
                    typeName,
                    throwOnError:
                        false,
                    ignoreCase:
                        false);

            Assert.NotNull(
                type);

            Assert.True(
                type.IsPublic,
                $"Critical consumer type '{typeName}' is no longer public.");
        }
    }

    [Fact]
    public void FoundationAssemblies_DoNotReferencePresentationFrameworks()
    {
        string[] assemblies =
        [
            "LotSizingDataModel.Core",
            "LotSizingDataModel.Solution",
            "LotSizingDataModel.Instance",
            "LotSizingDataModel.Solver",
            "LotSizingDataModel.Checker",
            "LotSizingDataModel.Checker.Campaign"
        ];

        string[] forbiddenPrefixes =
        [
            "Avalonia",
            "PresentationFramework",
            "PresentationCore",
            "Microsoft.UI",
            "WinUI",
            "System.Windows.Forms"
        ];

        foreach (string assemblyName
                 in assemblies)
        {
            Assembly assembly =
                Assembly.Load(
                    assemblyName);

            string[] references =
                assembly
                    .GetReferencedAssemblies()
                    .Select(
                        reference =>
                            reference.Name ??
                            string.Empty)
                    .ToArray();

            foreach (string forbidden
                     in forbiddenPrefixes)
            {
                Assert.DoesNotContain(
                    references,
                    reference =>
                        reference.StartsWith(
                            forbidden,
                            StringComparison.OrdinalIgnoreCase));
            }
        }
    }

    [Fact]
    public void FoundationAssemblies_DoNotReferenceDownstreamAlgorithmProjects()
    {
        string[] assemblies =
        [
            "LotSizingDataModel.Core",
            "LotSizingDataModel.Solution",
            "LotSizingDataModel.Instance",
            "LotSizingDataModel.Solver",
            "LotSizingDataModel.Checker",
            "LotSizingDataModel.Checker.Campaign"
        ];

        foreach (string assemblyName
                 in assemblies)
        {
            Assembly assembly =
                Assembly.Load(
                    assemblyName);

            string[] references =
                assembly
                    .GetReferencedAssemblies()
                    .Select(
                        reference =>
                            reference.Name ??
                            string.Empty)
                    .ToArray();

            Assert.DoesNotContain(
                references,
                reference =>
                    reference.StartsWith(
                        "MLLPAlgorithm",
                        StringComparison.OrdinalIgnoreCase));
        }
    }

    [Fact]
    public void LayerDependencies_DoNotReverse()
    {
        AssertNoReferencePrefix(
            "LotSizingDataModel.Core",
            "LotSizingDataModel.Instance",
            "LotSizingDataModel.Solution",
            "LotSizingDataModel.Solver",
            "LotSizingDataModel.Checker");

        AssertNoReferencePrefix(
            "LotSizingDataModel.Solution",
            "LotSizingDataModel.Instance",
            "LotSizingDataModel.Solver",
            "LotSizingDataModel.Checker");

        AssertNoReferencePrefix(
            "LotSizingDataModel.Instance",
            "LotSizingDataModel.Solver",
            "LotSizingDataModel.Checker");

        AssertNoReferencePrefix(
            "LotSizingDataModel.Solver",
            "LotSizingDataModel.Checker");

        AssertNoReferencePrefix(
            "LotSizingDataModel.Checker",
            "LotSizingDataModel.Checker.Campaign");
    }

    private static void AssertNoReferencePrefix(
        string assemblyName,
        params string[] forbiddenPrefixes)
    {
        Assembly assembly =
            Assembly.Load(
                assemblyName);

        string[] references =
            assembly
                .GetReferencedAssemblies()
                .Select(
                    reference =>
                        reference.Name ??
                        string.Empty)
                .ToArray();

        foreach (string forbiddenPrefix
                 in forbiddenPrefixes)
        {
            Assert.DoesNotContain(
                references,
                reference =>
                    reference.StartsWith(
                        forbiddenPrefix,
                        StringComparison.Ordinal));
        }
    }
}
