using System.Reflection;
using LotSizingDataModel.Core;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Solution;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Regression;

public sealed class StablePromotionTests
{
    [Fact]
    public void FirstPartyAssemblies_ReportStableInformationalVersion()
    {
        Assembly[] assemblies =
        [
            typeof(SupplyChain).Assembly,
            typeof(LotSizingInstance).Assembly,
            typeof(LotSizingSolution).Assembly
        ];

        foreach (Assembly assembly
                 in assemblies)
        {
            string informationalVersion =
                assembly
                    .GetCustomAttribute<
                        AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion ??
                string.Empty;

            Assert.StartsWith(
                "1.2.0.",
                informationalVersion,
                StringComparison.Ordinal);

            Assert.DoesNotContain(
                "-alpha",
                informationalVersion,
                StringComparison.OrdinalIgnoreCase);

            Assert.Contains(
                "+",
                informationalVersion,
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void StableAssemblyVersionsShareSameMajorMinor()
    {
        Assembly[] assemblies =
        [
            typeof(SupplyChain).Assembly,
            typeof(LotSizingInstance).Assembly,
            typeof(LotSizingSolution).Assembly
        ];

        foreach (Assembly assembly
                 in assemblies)
        {
            Version? version =
                assembly.GetName().Version;

            Assert.NotNull(
                version);

            Assert.Equal(
                1,
                version.Major);

            Assert.Equal(
                2,
                version.Minor);
        }
    }
}
