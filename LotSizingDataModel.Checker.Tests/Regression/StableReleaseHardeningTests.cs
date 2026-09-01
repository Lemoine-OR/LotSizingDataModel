using System.Reflection;
using System.Text;
using LotSizingDataModel.Core;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Serialization;
using LotSizingDataModel.Solution;
using LotSizingDataModel.Solution.Serialization;
using Xunit;

namespace LotSizingDataModel.Checker.Tests.Regression;

public sealed class StableReleaseHardeningTests
{
    [Fact]
    public void InstanceXml_RoundTripsWithProtectedRoot()
    {
        var instance =
            new LotSizingInstance(
                new SupplyChain(
                    planningHorizon:
                        2));

        string xml =
            LotSizingInstanceXmlSerializer
                .SerializeToString(
                    instance,
                    validateBeforeSerialization:
                        false,
                    validateCurrentFingerprint:
                        false,
                    indent:
                        true);

        Assert.Contains(
            "<lotSizingInstance",
            xml,
            StringComparison.Ordinal);

        LotSizingInstance clone =
            LotSizingInstanceXmlSerializer
                .DeserializeFromString(
                    xml,
                    validateAfterDeserialization:
                        false,
                    validateCurrentFingerprint:
                        false);

        Assert.Equal(
            2,
            clone.PlanningHorizon);
    }

    [Fact]
    public void SolutionXml_RoundTripsWithProtectedRoot()
    {
        var solution =
            new LotSizingSolution(
                planningHorizon:
                    2);

        var serializer =
            new LotSizingSolutionXmlSerializer();

        string xml =
            serializer.SerializeToString(
                solution,
                validateBeforeSerialization:
                    false);

        Assert.Contains(
            "<lotSizingSolution",
            xml,
            StringComparison.Ordinal);

        LotSizingSolution clone =
            serializer.DeserializeFromString(
                xml,
                validateAfterDeserialization:
                    false);

        Assert.Equal(
            2,
            clone.PlanningHorizon);
    }

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

            string numericPrefix =
                informationalVersion.Split(
                    '+',
                    2,
                    StringSplitOptions.None)[0];

            string[] numericParts =
                numericPrefix.Split(
                    '.',
                    StringSplitOptions.None);

            Assert.Equal(
                4,
                numericParts.Length);

            Assert.Equal(
                "1",
                numericParts[0]);

            Assert.Equal(
                "2",
                numericParts[1]);

            Assert.Equal(
                "0",
                numericParts[2]);

            Assert.True(
                int.TryParse(
                    numericParts[3],
                    out int buildHeight));

            Assert.True(
                buildHeight >= 0);
        }
    }

    [Fact]
    public void XmlSerializers_UseUtf8WithoutBomForStringRoundTrip()
    {
        Assert.False(
            LotSizingInstanceXmlSerializer
                .XmlEncoding
                .GetPreamble()
                .Any());

        var solution =
            new LotSizingSolution(
                planningHorizon:
                    1);

        string xml =
            new LotSizingSolutionXmlSerializer()
                .SerializeToString(
                    solution,
                    validateBeforeSerialization:
                        false);

        byte[] bytes =
            Encoding.UTF8.GetBytes(
                xml);

        byte[] bom =
            Encoding.UTF8.GetPreamble();

        Assert.False(
            bytes.Take(
                    bom.Length)
                .SequenceEqual(
                    bom));
    }
}
