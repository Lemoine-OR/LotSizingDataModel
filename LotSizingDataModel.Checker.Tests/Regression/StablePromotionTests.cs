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

        foreach (Assembly assembly in assemblies)
        {
            Version version =
                assembly.GetName().Version ??
                throw new InvalidOperationException(
                    $"Assembly '{assembly.GetName().Name}' has no version.");

            string informationalVersion =
                assembly
                    .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                    ?.InformationalVersion ??
                string.Empty;

            string expectedStablePrefix =
                $"{version.Major}.{version.Minor}.{version.Build}.";

            Assert.StartsWith(
                expectedStablePrefix,
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

        Version[] versions =
            assemblies
                .Select(
                    assembly =>
                        assembly.GetName().Version ??
                        throw new InvalidOperationException(
                            $"Assembly '{assembly.GetName().Name}' has no version."))
                .ToArray();

        Version reference = versions[0];

        Assert.True(reference.Major > 0);

        foreach (Version version in versions)
        {
            Assert.Equal(reference.Major, version.Major);
            Assert.Equal(reference.Minor, version.Minor);
            Assert.Equal(reference.Build, version.Build);
        }
    }
}
