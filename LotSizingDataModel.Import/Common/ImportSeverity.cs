using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.Common;

/// <summary>
/// Identifies the severity level of a diagnostic produced
/// during an instance import operation.
/// </summary>
/// <remarks>
/// Import diagnostics are used to report:
/// <list type="bullet">
/// <item>
/// <description>
/// general information about the imported source;
/// </description>
/// </item>
/// <item>
/// <description>
/// non-blocking inconsistencies or assumptions;
/// </description>
/// </item>
/// <item>
/// <description>
/// recoverable or blocking import errors.
/// </description>
/// </item>
/// </list>
/// </remarks>
[Serializable]
[XmlType(TypeName = "importSeverity")]
public enum ImportSeverity
{
    /// <summary>
    /// Indicates that no severity has been assigned.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Represents general informational feedback.
    /// </summary>
    Information = 1,

    /// <summary>
    /// Represents a non-blocking issue that should be
    /// reviewed by the user.
    /// </summary>
    Warning = 2,

    /// <summary>
    /// Represents an import error that may prevent the
    /// creation of a valid instance.
    /// </summary>
    Error = 3,

    /// <summary>
    /// Represents a fatal error that prevents the import
    /// process from continuing.
    /// </summary>
    Fatal = 4
}