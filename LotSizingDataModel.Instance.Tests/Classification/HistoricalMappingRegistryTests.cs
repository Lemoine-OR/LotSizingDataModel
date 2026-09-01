using LotSizingDataModel.Instance.Historical;
using Xunit;

namespace LotSizingDataModel.Instance.Tests.Classification;

public sealed class HistoricalMappingRegistryTests
{
    [Fact]
    public void BaselineRegistry_ContainsValidatedWolseyRules()
    {
        HistoricalClassificationMappingRegistry registry =
            HistoricalClassificationMappingRegistryFactory
                .CreateValidatedBaseline();

        HistoricalMappingRule capU =
            Assert.IsType<HistoricalMappingRule>(
                registry.FindForward(
                    HistoricalClassificationFamily.Wolsey,
                    "CAP=U"));

        Assert.Equal(
            HistoricalMappingConfidence.Exact,
            capU.Confidence);

        Assert.True(
            capU.AllowsInverse);

        string token =
            Assert.Single(
                capU.UniversalTokens);

        Assert.Equal(
            "Uncap:P",
            token);
    }

    [Fact]
    public void ExactInverseDetection_RequiresExactTokenSet()
    {
        HistoricalClassificationMappingRegistry registry =
            HistoricalClassificationMappingRegistryFactory
                .CreateValidatedBaseline();

        HistoricalInverseResolution resolution =
            registry.ResolveInverse(
                HistoricalClassificationFamily.Wolsey,
                new[]
                {
                    "TP:CapP=C",
                    "Cap:P"
                });

        Assert.Equal(
            HistoricalInverseResolutionStatus.Unique,
            resolution.Status);

        Assert.NotNull(
            resolution.UniqueRule);

        Assert.Equal(
            "CAP=CC",
            resolution.UniqueRule.HistoricalToken);
    }

    [Fact]
    public void ConservativeRules_DoNotParticipateInInverseDetection()
    {
        HistoricalClassificationMappingRegistry registry =
            HistoricalClassificationMappingRegistryFactory
                .CreateValidatedBaseline();

        HistoricalInverseResolution resolution =
            registry.ResolveInverse(
                HistoricalClassificationFamily.Wolsey,
                new[]
                {
                    "Scheduling:SB1"
                });

        Assert.Equal(
            HistoricalInverseResolutionStatus.NotFound,
            resolution.Status);

        Assert.Null(
            resolution.UniqueRule);
    }

    [Fact]
    public void SourceOnlyMachineLabels_HaveNoInventedProjection()
    {
        HistoricalClassificationMappingRegistry registry =
            HistoricalClassificationMappingRegistryFactory
                .CreateValidatedBaseline();

        HistoricalMappingRule im =
            Assert.IsType<HistoricalMappingRule>(
                registry.FindForward(
                    HistoricalClassificationFamily.Wolsey,
                    "IM"));

        Assert.Equal(
            HistoricalMappingConfidence.SourceOnly,
            im.Confidence);

        Assert.Empty(
            im.UniversalTokens);

        Assert.False(
            im.AllowsInverse);
    }

    [Fact]
    public void DeclaredAndDetectedLabels_RemainIndependent()
    {
        var service =
            new HistoricalMappingAuditService();

        HistoricalMappingAuditResult result =
            service.Audit(
                HistoricalClassificationFamily.Wolsey,
                declaredTokens:
                    new[]
                    {
                        "CAP=U",
                        "IM"
                    },
                detectedTokens:
                    new[]
                    {
                        "CAP=U",
                        "VAR=B"
                    });

        Assert.False(
            result.IsExactMatch);

        string declaredOnly =
            Assert.Single(
                result.DeclaredButNotDetected);

        Assert.Equal(
            "IM",
            declaredOnly);

        string detectedOnly =
            Assert.Single(
                result.DetectedButNotDeclared);

        Assert.Equal(
            "VAR=B",
            detectedOnly);
    }

    [Fact]
    public void BitranYanasseFamily_IsSupportedWithoutInventedDefaultRules()
    {
        HistoricalClassificationMappingRegistry registry =
            HistoricalClassificationMappingRegistryFactory
                .CreateValidatedBaseline();

        Assert.Empty(
            registry.GetRules(
                HistoricalClassificationFamily.BitranYanasse));

        Assert.Null(
            registry.FindForward(
                HistoricalClassificationFamily.BitranYanasse,
                "unproven-token"));
    }

    [Fact]
    public void NonExactRule_CannotEnableInverseDetection()
    {
        Assert.Throws<InvalidOperationException>(
            () =>
                new HistoricalMappingRule(
                    "test",
                    HistoricalClassificationFamily.Other,
                    "X",
                    new[]
                    {
                        "U:X"
                    },
                    HistoricalMappingConfidence.Conservative,
                    allowsInverse: true,
                    sourceReference: "test source"));
    }
}
