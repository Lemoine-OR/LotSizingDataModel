using LotSizingDataModel.Core;
using LotSizingDataModel.Core.LogicalModel;
using LotSizingDataModel.Instance.Validation;

namespace LotSizingDataModel.Instance.Tests.Validation;

public sealed class InstanceModelCheckerTests
{
    [Fact]
    public void Check_MapsStructuralErrorsAndBlocksDownstreamOperations()
    {
        var instance =
            new LotSizingInstance(
                new SupplyChain(planningHorizon: 0));

        InstanceModelCheckResult result =
            new InstanceModelChecker().Check(instance);

        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == "SC001");

        Assert.Contains(
            result.Diagnostics,
            diagnostic => diagnostic.Code == "SC002");

        Assert.True(result.HasBlockingIssues);
        Assert.False(result.IsValid);
        Assert.True(result.Capabilities.CanSaveDraft);
        Assert.True(result.Capabilities.CanValidate);
        Assert.False(result.Capabilities.CanClassify);
        Assert.False(result.Capabilities.CanGenerateNotation);
        Assert.False(result.Capabilities.CanSolve);
        Assert.False(
            result.Capabilities.CanExportAsValidatedInstance);
    }

    [Fact]
    public void Check_NoDemandProducesNonBlockingSemanticWarning()
    {
        LotSizingInstance instance =
            CreateMinimalStructurallyValidInstance();

        InstanceModelCheckResult result =
            new InstanceModelChecker().Check(instance);

        InstanceDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics,
                item => item.Code == "LSDM-SEM-010");

        Assert.Equal(
            InstanceDiagnosticSeverity.Warning,
            diagnostic.Severity);

        Assert.False(result.HasBlockingIssues);
        Assert.True(result.IsValid);
        Assert.True(result.Capabilities.CanClassify);
        Assert.True(result.Capabilities.CanSolve);
    }

    [Fact]
    public void Check_MissingInstanceIdProducesWarning()
    {
        LotSizingInstance instance =
            CreateMinimalStructurallyValidInstance();

        instance.InstanceId = string.Empty;

        InstanceModelCheckResult result =
            new InstanceModelChecker().Check(instance);

        Assert.Contains(
            result.Diagnostics,
            diagnostic =>
                diagnostic.Code == "LSDM-SEM-001" &&
                diagnostic.Severity ==
                InstanceDiagnosticSeverity.Warning);
    }

    [Fact]
    public void Check_DanglingBestKnownResultIdProducesWarningWithContext()
    {
        LotSizingInstance instance =
            CreateMinimalStructurallyValidInstance();

        instance.BestKnownResultId = "missing-result";

        InstanceModelCheckResult result =
            new InstanceModelChecker().Check(instance);

        InstanceDiagnostic diagnostic =
            Assert.Single(
                result.Diagnostics,
                item => item.Code == "LSDM-SEM-020");

        Assert.Equal(
            "missing-result",
            diagnostic.Values["bestKnownResultId"]);
    }

    [Fact]
    public void Diagnostic_ValuesAreReadOnlyFromCallerPerspective()
    {
        var sourceValues =
            new Dictionary<string, string>
            {
                ["key"] = "original"
            };

        var diagnostic =
            new InstanceDiagnostic(
                code: "LSDM-SEM-TEST",
                severity:
                    InstanceDiagnosticSeverity.Information,
                path: "instance",
                message: "test",
                values: sourceValues);

        sourceValues["key"] = "mutated";

        Assert.Equal(
            "original",
            diagnostic.Values["key"]);
    }

    [Fact]
    public void Check_NullInstanceIsRejected()
    {
        var checker = new InstanceModelChecker();

        Assert.Throws<ArgumentNullException>(
            () => checker.Check(null!));
    }

    private static LotSizingInstance
        CreateMinimalStructurallyValidInstance()
    {
        var supplyChain =
            new SupplyChain(planningHorizon: 3);

        supplyChain.Items.Add(
            new Item(
                id: 1,
                name: "Item 1",
                billOfMaterialsLevel: 0));

        return new LotSizingInstance(
            supplyChain,
            name: "Minimal instance");
    }
}
