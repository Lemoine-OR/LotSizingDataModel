using System;
using System.Runtime.Serialization;

namespace LotSizingDataModel.Import.DellaertJeunet;

/// <summary>
/// Represents an error encountered while reading or
/// deserializing a Dellaert–Jeunet XML document.
/// </summary>
public sealed class DellaertJeunetXmlReadException :
    Exception
{
    /// <summary>
    /// Initializes a new exception.
    /// </summary>
    public DellaertJeunetXmlReadException()
    {
    }

    /// <summary>
    /// Initializes a new exception.
    /// </summary>
    /// <param name="message">
    /// Error message.
    /// </param>
    public DellaertJeunetXmlReadException(
        string message)
        : base(message)
    {
    }

    /// <summary>
    /// Initializes a new exception.
    /// </summary>
    /// <param name="message">
    /// Error message.
    /// </param>
    /// <param name="innerException">
    /// Exception causing this error.
    /// </param>
    public DellaertJeunetXmlReadException(
        string message,
        Exception innerException)
        : base(
            message,
            innerException)
    {
    }

    /// <summary>
    /// Initializes a new exception containing an XML source
    /// location.
    /// </summary>
    /// <param name="message">
    /// Error message.
    /// </param>
    /// <param name="lineNumber">
    /// One-based XML line number.
    /// </param>
    /// <param name="linePosition">
    /// One-based XML column position.
    /// </param>
    /// <param name="innerException">
    /// Exception causing this error.
    /// </param>
    public DellaertJeunetXmlReadException(
        string message,
        int? lineNumber,
        int? linePosition,
        Exception innerException)
        : base(
            message,
            innerException)
    {
        LineNumber =
            lineNumber;

        LinePosition =
            linePosition;
    }

    //private DellaertJeunetXmlReadException(
    //    SerializationInfo info,
    //    StreamingContext context)
    //    : base(
    //        info,
    //        context)
    //{
    //    LineNumber =
    //        GetNullableInt32(
    //            info,
    //            nameof(LineNumber));

    //    LinePosition =
    //        GetNullableInt32(
    //            info,
    //            nameof(LinePosition));
    //}

    /// <summary>
    /// Gets the optional one-based XML line number.
    /// </summary>
    public int? LineNumber { get; }

    /// <summary>
    /// Gets the optional one-based XML column position.
    /// </summary>
    public int? LinePosition { get; }

    /// <inheritdoc/>
    //public override void GetObjectData(
    //    SerializationInfo info,
    //    StreamingContext context)
    //{
    //    ArgumentNullException.ThrowIfNull(
    //        info);

    //    base.GetObjectData(
    //        info,
    //        context);

    //    info.AddValue(
    //        nameof(LineNumber),
    //        LineNumber,
    //        typeof(int?));

    //    info.AddValue(
    //        nameof(LinePosition),
    //        LinePosition,
    //        typeof(int?));
    //}

    private static int? GetNullableInt32(
        SerializationInfo info,
        string name)
    {
        object? value =
            info.GetValue(
                name,
                typeof(int?));

        return value as int?;
    }
}