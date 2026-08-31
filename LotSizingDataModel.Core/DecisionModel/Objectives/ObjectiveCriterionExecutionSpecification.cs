using System;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Core.DecisionModel.Objectives;

/// <summary>
/// Specifies one executable criterion in a multiobjective policy.
/// </summary>
/// <remarks>
/// The criterion kind is generic. Weight and priority are explicit source data
/// and are never inferred from list position or naming conventions.
/// </remarks>
[Serializable]
[XmlType(TypeName = "objectiveCriterionExecutionSpecification")]
public sealed class ObjectiveCriterionExecutionSpecification :
    ModelObject
{
    private OptimizationObjectiveKind _kind =
        OptimizationObjectiveKind.Economic;

    private double _weight = 1.0;
    private int _priority;
    private double _absoluteTolerance;

    [XmlAttribute("kind")]
    public OptimizationObjectiveKind Kind
    {
        get => _kind;
        set => SetProperty(ref _kind, value);
    }

    [XmlAttribute("weight")]
    public double Weight
    {
        get => _weight;
        set
        {
            if (!double.IsFinite(value) || value <= 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "An executable objective weight must be finite and strictly positive.");
            }

            SetProperty(ref _weight, value);
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
                    "Objective priority cannot be negative.");
            }

            SetProperty(ref _priority, value);
        }
    }

    [XmlAttribute("absoluteTolerance")]
    public double AbsoluteTolerance
    {
        get => _absoluteTolerance;
        set
        {
            if (!double.IsFinite(value) || value < 0.0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(value),
                    value,
                    "Lexicographic tolerance must be finite and non-negative.");
            }

            SetProperty(ref _absoluteTolerance, value);
        }
    }
}
