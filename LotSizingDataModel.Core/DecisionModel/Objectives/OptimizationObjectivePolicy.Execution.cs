using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LotSizingDataModel.Core.DecisionModel.Objectives;

/// <summary>
/// Adds executable multiobjective criterion data to <see cref="OptimizationObjectivePolicy"/>.
/// </summary>
public sealed partial class OptimizationObjectivePolicy
{
    [XmlArray("executionCriteria")]
    [XmlArrayItem("criterion")]
    public List<ObjectiveCriterionExecutionSpecification>
        ExecutionCriteria
    {
        get;
    } = new();

    [XmlIgnore]
    public bool HasExecutableCriterionSpecifications =>
        ExecutionCriteria.Count > 0;

    [XmlIgnore]
    public bool HasUniqueExecutableCriterionKinds =>
        ExecutionCriteria
            .Select(criterion => criterion.Kind)
            .Distinct()
            .Count() ==
        ExecutionCriteria.Count;

    [XmlIgnore]
    public bool HasUniqueLexicographicPriorities =>
        ExecutionCriteria
            .Select(criterion => criterion.Priority)
            .Distinct()
            .Count() ==
        ExecutionCriteria.Count;
}
