using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.Common;

/// <summary>
/// Identifies the external data format used to describe a
/// lot-sizing problem instance.
/// </summary>
/// <remarks>
/// This enumeration is intentionally independent from the
/// file extension.
///
/// Several instance formats may use the same extension. For
/// example, different benchmark families may all be stored in
/// XML files while using incompatible document structures.
///
/// Importers and format detectors should therefore rely on
/// document content rather than file extensions alone.
/// </remarks>
[Serializable]
[XmlType(TypeName = "instanceFormat")]
public enum InstanceFormat
{
    /// <summary>
    /// Indicates that the instance format has not been
    /// identified.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// Represents the XML format used for Dellaert and Jeunet
    /// multilevel lot-sizing benchmark instances.
    /// </summary>
    DellaertJeunetXml = 1,

    /// <summary>
    /// Represents a generic XML format defined by
    /// LotSizingDataModel.
    /// </summary>
    LotSizingDataModelXml = 2,

    /// <summary>
    /// Represents a generic comma-separated-value format.
    /// </summary>
    Csv = 3,

    /// <summary>
    /// Represents a generic JSON format.
    /// </summary>
    Json = 4,

    /// <summary>
    /// Represents a user-defined or application-specific
    /// instance format.
    /// </summary>
    Custom = 1000
}