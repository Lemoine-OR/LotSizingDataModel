using System.Xml.Serialization;
using LotSizingDataModel.Core.DecisionModel.Objectives;
using LotSizingDataModel.Core.DecisionModel.Planning;

namespace LotSizingDataModel.Core;

/// <summary>
/// Adds explicit LSI-relevant planning and objective semantics
/// without changing historical source-data structures.
/// </summary>
public sealed partial class SupplyChain
{
    private LotSizingPlanningContext? _planningContext;
    private OptimizationObjectivePolicy? _objectivePolicy;

    /// <summary>
    /// Gets or sets explicit planning semantics such as the
    /// big-bucket/small-bucket interpretation.
    /// Null means that the semantics have not been declared.
    /// </summary>
    [XmlElement("planningContext")]
    public LotSizingPlanningContext? PlanningContext
    {
        get => _planningContext;
        set
        {
            if (ReferenceEquals(_planningContext, value))
            {
                return;
            }

            _planningContext = value;
            OnPropertyChanged(nameof(PlanningContext));
        }
    }

    /// <summary>
    /// Gets or sets the explicit business objective policy.
    /// Null means that no objective semantics have been declared.
    /// </summary>
    [XmlElement("objectivePolicy")]
    public OptimizationObjectivePolicy? ObjectivePolicy
    {
        get => _objectivePolicy;
        set
        {
            if (ReferenceEquals(_objectivePolicy, value))
            {
                return;
            }

            value?.EnsureValid();

            _objectivePolicy = value;
            OnPropertyChanged(nameof(ObjectivePolicy));
        }
    }
}
