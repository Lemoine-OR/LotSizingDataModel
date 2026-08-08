using System;
using System.Collections.Generic;
using LotSizingDataModel.Instance.Recommendation;

namespace LotSizingDataModel.Instance.Creation;

/// <summary>
/// Creates, applies and refreshes solution-method
/// recommendation reports for lot-sizing instances.
/// </summary>
/// <remarks>
/// This factory coordinates the supply-chain fingerprint,
/// the solution-method catalog and
/// <see cref="SolutionMethodAdvisor"/>.
/// </remarks>
public static class LotSizingInstanceRecommendationFactory
{
    /// <summary>
    /// Creates a recommendation report using the standard
    /// solution-method catalog.
    /// </summary>
    /// <param name="instance">
    /// Instance to evaluate.
    /// </param>
    /// <param name="recommendedScoreThreshold">
    /// Minimum score required for a directly compatible
    /// method to be marked as recommended.
    /// </param>
    /// <param name="includeIncompatibleMethods">
    /// Value indicating whether incompatible methods must be
    /// retained in the report.
    /// </param>
    /// <returns>
    /// Newly generated recommendation report.
    /// </returns>
    public static SolutionMethodRecommendationReport
        CreateStandardReport(
            LotSizingInstance instance,
            double recommendedScoreThreshold =
                SolutionMethodAdvisor
                    .DefaultRecommendedScoreThreshold,
            bool includeIncompatibleMethods = true)
    {
        ArgumentNullException.ThrowIfNull(instance);

        SolutionMethodCatalog catalog =
            SolutionMethodCatalogFactory
                .CreateStandardCatalog();

        return CreateReport(
            instance,
            catalog,
            recommendedScoreThreshold,
            includeIncompatibleMethods);
    }

    /// <summary>
    /// Creates a recommendation report using a supplied
    /// solution-method catalog.
    /// </summary>
    /// <param name="instance">
    /// Instance to evaluate.
    /// </param>
    /// <param name="catalog">
    /// Catalog containing the methods to evaluate.
    /// </param>
    /// <param name="recommendedScoreThreshold">
    /// Minimum score required for a directly compatible
    /// method to be marked as recommended.
    /// </param>
    /// <param name="includeIncompatibleMethods">
    /// Value indicating whether incompatible methods must be
    /// retained in the report.
    /// </param>
    /// <returns>
    /// Newly generated recommendation report.
    /// </returns>
    public static SolutionMethodRecommendationReport
        CreateReport(
            LotSizingInstance instance,
            SolutionMethodCatalog catalog,
            double recommendedScoreThreshold =
                SolutionMethodAdvisor
                    .DefaultRecommendedScoreThreshold,
            bool includeIncompatibleMethods = true)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(catalog);

        ValidateRecommendedScoreThreshold(
            recommendedScoreThreshold);

        catalog.EnsureValid();

        string supplyChainFingerprint =
            LotSizingInstanceFactory
                .ComputeSupplyChainFingerprint(
                    instance.SupplyChain);

        IReadOnlyList<SolutionMethodRecommendation>
            recommendations =
                SolutionMethodAdvisor.Recommend(
                    instance,
                    catalog,
                    recommendedScoreThreshold,
                    includeIncompatibleMethods);

        var report =
            new SolutionMethodRecommendationReport(
                advisorVersion:
                    SolutionMethodAdvisor.CurrentVersion,
                catalogName:
                    catalog.CatalogName,
                catalogVersion:
                    catalog.CatalogVersion,
                supplyChainFingerprint:
                    supplyChainFingerprint,
                recommendedScoreThreshold:
                    recommendedScoreThreshold);

        report.ReplaceRecommendations(
            recommendations);

        report.EnsureValid();

        return report;
    }

    /// <summary>
    /// Creates a report from the standard catalog and stores
    /// it in the instance.
    /// </summary>
    /// <param name="instance">
    /// Instance to update.
    /// </param>
    /// <param name="recommendedScoreThreshold">
    /// Minimum score required for a directly compatible
    /// method to be marked as recommended.
    /// </param>
    /// <param name="includeIncompatibleMethods">
    /// Value indicating whether incompatible methods must be
    /// retained in the report.
    /// </param>
    /// <returns>
    /// Report stored in the instance.
    /// </returns>
    public static SolutionMethodRecommendationReport
        CreateAndApplyStandardReport(
            LotSizingInstance instance,
            double recommendedScoreThreshold =
                SolutionMethodAdvisor
                    .DefaultRecommendedScoreThreshold,
            bool includeIncompatibleMethods = true)
    {
        ArgumentNullException.ThrowIfNull(instance);

        SolutionMethodRecommendationReport report =
            CreateStandardReport(
                instance,
                recommendedScoreThreshold,
                includeIncompatibleMethods);

        ApplyReport(
            instance,
            report);

        return report;
    }

    /// <summary>
    /// Creates a report from a supplied catalog and stores it
    /// in the instance.
    /// </summary>
    /// <param name="instance">
    /// Instance to update.
    /// </param>
    /// <param name="catalog">
    /// Catalog containing the methods to evaluate.
    /// </param>
    /// <param name="recommendedScoreThreshold">
    /// Minimum score required for a directly compatible
    /// method to be marked as recommended.
    /// </param>
    /// <param name="includeIncompatibleMethods">
    /// Value indicating whether incompatible methods must be
    /// retained in the report.
    /// </param>
    /// <returns>
    /// Report stored in the instance.
    /// </returns>
    public static SolutionMethodRecommendationReport
        CreateAndApplyReport(
            LotSizingInstance instance,
            SolutionMethodCatalog catalog,
            double recommendedScoreThreshold =
                SolutionMethodAdvisor
                    .DefaultRecommendedScoreThreshold,
            bool includeIncompatibleMethods = true)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(catalog);

        SolutionMethodRecommendationReport report =
            CreateReport(
                instance,
                catalog,
                recommendedScoreThreshold,
                includeIncompatibleMethods);

        ApplyReport(
            instance,
            report);

        return report;
    }

    /// <summary>
    /// Stores an existing recommendation report in an
    /// instance after checking its fingerprint.
    /// </summary>
    /// <param name="instance">
    /// Instance to update.
    /// </param>
    /// <param name="report">
    /// Report to apply.
    /// </param>
    public static void ApplyReport(
        LotSizingInstance instance,
        SolutionMethodRecommendationReport report)
    {
        ArgumentNullException.ThrowIfNull(instance);
        ArgumentNullException.ThrowIfNull(report);

        report.EnsureValid();

        string currentFingerprint =
            LotSizingInstanceFactory
                .ComputeSupplyChainFingerprint(
                    instance.SupplyChain);

        if (!report.MatchesSupplyChainFingerprint(
                currentFingerprint))
        {
            throw new InvalidOperationException(
                "The solution-method recommendation report " +
                "does not match the current supply-chain " +
                "data.");
        }

        instance.ReplaceSolutionMethodRecommendationReport(
            report);

        instance.Touch();
    }

    /// <summary>
    /// Determines whether the report stored in an instance is
    /// current for its supply-chain data.
    /// </summary>
    /// <param name="instance">
    /// Instance whose report must be checked.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when a generated report exists
    /// and its fingerprint matches the current supply chain;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool HasCurrentReport(
        LotSizingInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (!instance.HasSolutionMethodRecommendationReport)
        {
            return false;
        }

        string currentFingerprint =
            LotSizingInstanceFactory
                .ComputeSupplyChainFingerprint(
                    instance.SupplyChain);

        return instance
            .SolutionMethodRecommendationsMatchFingerprint(
                currentFingerprint);
    }

    /// <summary>
    /// Regenerates the standard report when it is absent,
    /// outdated or based on another advisor or catalog
    /// version.
    /// </summary>
    /// <param name="instance">
    /// Instance to update.
    /// </param>
    /// <param name="recommendedScoreThreshold">
    /// Minimum score required for a directly compatible
    /// method to be marked as recommended.
    /// </param>
    /// <param name="includeIncompatibleMethods">
    /// Value indicating whether incompatible methods must be
    /// retained in the report.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when the report was regenerated;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public static bool RefreshStandardReportIfRequired(
        LotSizingInstance instance,
        double recommendedScoreThreshold =
            SolutionMethodAdvisor
                .DefaultRecommendedScoreThreshold,
        bool includeIncompatibleMethods = true)
    {
        ArgumentNullException.ThrowIfNull(instance);

        ValidateRecommendedScoreThreshold(
            recommendedScoreThreshold);

        SolutionMethodRecommendationReport currentReport =
            instance.SolutionMethodRecommendationReport;

        bool reportIsCurrent =
            HasCurrentReport(instance) &&
            NearlyEqual(
                currentReport.RecommendedScoreThreshold,
                recommendedScoreThreshold) &&
            string.Equals(
                currentReport.AdvisorVersion,
                SolutionMethodAdvisor.CurrentVersion,
                StringComparison.Ordinal) &&
            string.Equals(
                currentReport.CatalogName,
                SolutionMethodCatalogFactory
                    .StandardCatalogName,
                StringComparison.Ordinal) &&
            string.Equals(
                currentReport.CatalogVersion,
                SolutionMethodCatalogFactory
                    .StandardCatalogVersion,
                StringComparison.Ordinal);

        if (reportIsCurrent)
        {
            return false;
        }

        CreateAndApplyStandardReport(
            instance,
            recommendedScoreThreshold,
            includeIncompatibleMethods);

        return true;
    }

    /// <summary>
    /// Clears a stored report when it does not match the
    /// current supply-chain fingerprint.
    /// </summary>
    /// <param name="instance">
    /// Instance whose report must be checked.
    /// </param>
    /// <returns>
    /// <see langword="true"/> when an outdated report was
    /// cleared; otherwise, <see langword="false"/>.
    /// </returns>
    public static bool ClearOutdatedReport(
        LotSizingInstance instance)
    {
        ArgumentNullException.ThrowIfNull(instance);

        if (!instance.HasSolutionMethodRecommendationReport ||
            HasCurrentReport(instance))
        {
            return false;
        }

        instance.ClearSolutionMethodRecommendationReport();
        instance.Touch();

        return true;
    }

    private static bool NearlyEqual(
        double left,
        double right)
    {
        return Math.Abs(left - right) <= 1e-12;
    }

    private static void ValidateRecommendedScoreThreshold(
        double recommendedScoreThreshold)
    {
        if (!double.IsFinite(recommendedScoreThreshold) ||
            recommendedScoreThreshold < 0.0 ||
            recommendedScoreThreshold > 1.0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(recommendedScoreThreshold),
                recommendedScoreThreshold,
                "The recommended-score threshold must be " +
                "finite and lie between zero and one.");
        }
    }
}
