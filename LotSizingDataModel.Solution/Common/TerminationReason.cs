using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Solution.Common;

/// <summary>
/// Identifies the reason why a solution-generation
/// execution was terminated.
/// </summary>
/// <remarks>
/// The termination reason is independent of the feasibility,
/// completeness and optimality of the generated solution.
/// </remarks>
[Serializable]
[XmlType(TypeName = "terminationReason")]
public enum TerminationReason
{
    /// <summary>
    /// The termination reason is unknown or was not recorded.
    /// </summary>
    [XmlEnum("unknown")]
    Unknown,

    /// <summary>
    /// The method completed its normal execution.
    /// </summary>
    [XmlEnum("completed")]
    Completed,

    /// <summary>
    /// The method terminated after proving that the retained
    /// solution is optimal.
    /// </summary>
    [XmlEnum("optimalityProven")]
    OptimalityProven,

    /// <summary>
    /// The method terminated because a predefined target
    /// objective value or solution quality was reached.
    /// </summary>
    [XmlEnum("targetReached")]
    TargetReached,

    /// <summary>
    /// The method terminated because its maximum authorized
    /// execution time was reached.
    /// </summary>
    [XmlEnum("timeLimit")]
    TimeLimit,

    /// <summary>
    /// The method terminated because its maximum number
    /// of iterations was reached.
    /// </summary>
    [XmlEnum("iterationLimit")]
    IterationLimit,

    /// <summary>
    /// The method terminated because its maximum number
    /// of solution or objective evaluations was reached.
    /// </summary>
    [XmlEnum("evaluationLimit")]
    EvaluationLimit,

    /// <summary>
    /// The method terminated because no improvement was
    /// observed during a specified number of iterations
    /// or during a specified duration.
    /// </summary>
    [XmlEnum("noImprovement")]
    NoImprovement,

    /// <summary>
    /// The method terminated because a generic computational
    /// resource limit was reached, such as a memory, node
    /// or processing limit.
    /// </summary>
    [XmlEnum("resourceLimit")]
    ResourceLimit,

    /// <summary>
    /// The execution was interrupted explicitly by a user
    /// or by the calling application.
    /// </summary>
    [XmlEnum("userInterrupted")]
    UserInterrupted,

    /// <summary>
    /// The execution terminated because of an unexpected
    /// technical or algorithmic error.
    /// </summary>
    [XmlEnum("error")]
    Error
}