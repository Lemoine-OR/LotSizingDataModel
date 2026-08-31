using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Objectives;

/// <summary>
/// Declares the business objective policy independently from any particular
/// mathematical scalarization implementation.
/// </summary>
[Serializable]
[XmlType(TypeName = "optimizationObjectivePolicy")]
public sealed partial class OptimizationObjectivePolicy :
    ModelObject
{
    private ObjectiveAggregationMode _aggregationMode =
        ObjectiveAggregationMode.Single;

    [XmlAttribute("aggregationMode")]
    public ObjectiveAggregationMode AggregationMode
    {
        get => _aggregationMode;
        set => SetProperty(
            ref _aggregationMode,
            value);
    }

    [XmlArray("criteria")]
    [XmlArrayItem("criterion")]
    public List<OptimizationObjectiveCriterion> Criteria
    {
        get;
    } = new();

    [XmlIgnore]
    public int EnabledCriterionCount =>
        Criteria.Count(
            criterion =>
                criterion is not null &&
                criterion.IsEnabled);

    [XmlIgnore]
    public bool HasMultipleEnabledCriteria =>
        EnabledCriterionCount > 1;

    [XmlIgnore]
    public OptimizationObjectiveKind PrimaryObjectiveKind
    {
        get
        {
            OptimizationObjectiveCriterion? criterion =
                Criteria
                    .Where(
                        candidate =>
                            candidate is not null &&
                            candidate.IsEnabled)
                    .OrderBy(
                        candidate =>
                            candidate.Priority)
                    .ThenBy(
                        candidate =>
                            Criteria.IndexOf(candidate))
                    .FirstOrDefault();

            return criterion?.Kind ??
                OptimizationObjectiveKind.Unknown;
        }
    }

    public void EnsureValid()
    {
        OptimizationObjectiveCriterion[] enabled =
            Criteria
                .Where(
                    criterion =>
                        criterion is not null &&
                        criterion.IsEnabled)
                .ToArray();

        if (enabled.Length == 0)
        {
            throw new InvalidOperationException(
                "An objective policy must contain at least one enabled " +
                "criterion.");
        }

        if (
            enabled.Any(
                criterion =>
                    criterion.Kind ==
                    OptimizationObjectiveKind.Unknown))
        {
            throw new InvalidOperationException(
                "Enabled objective criteria require a known criterion kind.");
        }

        if (
            enabled
                .GroupBy(
                    criterion =>
                        criterion.Kind)
                .Any(
                    group =>
                        group.Count() > 1))
        {
            throw new InvalidOperationException(
                "Enabled objective criterion kinds must be unique.");
        }

        if (
            AggregationMode ==
                ObjectiveAggregationMode.Single &&
            enabled.Length != 1)
        {
            throw new InvalidOperationException(
                "Single objective aggregation requires exactly one enabled " +
                "criterion.");
        }

        if (
            AggregationMode ==
                ObjectiveAggregationMode.WeightedSum &&
            (
                enabled.Length < 2 ||
                enabled.All(
                    criterion =>
                        criterion.Weight == 0.0)
            ))
        {
            throw new InvalidOperationException(
                "Weighted-sum aggregation requires at least two enabled " +
                "criteria and at least one positive weight.");
        }

        if (
            AggregationMode ==
                ObjectiveAggregationMode.Lexicographic &&
            (
                enabled.Length < 2 ||
                enabled
                    .GroupBy(
                        criterion =>
                            criterion.Priority)
                    .Any(
                        group =>
                            group.Count() > 1)
            ))
        {
            throw new InvalidOperationException(
                "Lexicographic aggregation requires at least two enabled " +
                "criteria with distinct priorities.");
        }
    }
}
