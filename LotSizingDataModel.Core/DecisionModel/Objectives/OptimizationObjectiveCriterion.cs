using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Objectives;

/// <summary>
/// Declares one business optimization criterion in an objective policy.
/// </summary>
[Serializable]
[XmlType(TypeName = "optimizationObjectiveCriterion")]
public sealed class OptimizationObjectiveCriterion :
    ModelObject
{
    private OptimizationObjectiveKind _kind =
        OptimizationObjectiveKind.Unknown;

    private bool _isEnabled = true;
    private double _weight = 1.0;
    private int _priority;

    [XmlAttribute("kind")]
    public OptimizationObjectiveKind Kind
    {
        get => _kind;
        set => SetProperty(ref _kind, value);
    }

    [XmlAttribute("enabled")]
    public bool IsEnabled
    {
        get => _isEnabled;
        set => SetProperty(ref _isEnabled, value);
    }

    [XmlAttribute("weight")]
    public double Weight
    {
        get => _weight;
        set
        {
            if (
                !double.IsFinite(value) ||
                value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "An objective weight must be finite and non-negative.");
            }

            SetProperty(
                ref _weight,
                value);
        }
    }

    [XmlAttribute("priority")]
    public int Priority
    {
        get => _priority;
        set
        {
            if (value < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "An objective priority cannot be negative.");
            }

            SetProperty(
                ref _priority,
                value);
        }
    }
}
