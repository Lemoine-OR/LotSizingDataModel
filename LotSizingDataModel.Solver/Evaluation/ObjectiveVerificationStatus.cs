using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solver.Evaluation;

/// <summary>
/// Indicates whether a solver-reported objective value is
/// consistent with an objective value independently recomputed
/// from normalized decision-variable values.
/// </summary>
[Serializable]
[XmlType(TypeName = "objectiveVerificationStatus")]
public enum ObjectiveVerificationStatus
{
    /// <summary>
    /// No objective-value verification was performed.
    /// </summary>
    NotChecked = 0,

    /// <summary>
    /// The solver-reported and recomputed objective values agree
    /// within the configured tolerance.
    /// </summary>
    Consistent = 1,

    /// <summary>
    /// The solver-reported and recomputed objective values differ
    /// by more than the configured tolerance.
    /// </summary>
    Inconsistent = 2,

    /// <summary>
    /// Objective-value verification could not be completed.
    /// </summary>
    Failed = 3
}
