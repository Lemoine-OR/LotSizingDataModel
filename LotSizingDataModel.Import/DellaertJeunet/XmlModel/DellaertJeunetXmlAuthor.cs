using System;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.DellaertJeunet.XmlModel;

/// <summary>
/// Represents one author stored in the bibliographic metadata
/// of a Dellaert–Jeunet XML benchmark instance.
/// </summary>
/// <remarks>
/// This class is a source-format data-transfer object.
///
/// It reproduces the XML structure used by the benchmark files
/// and must remain independent from the domain model contained
/// in <c>LotSizingDataModel.Core</c> and
/// <c>LotSizingDataModel.Instance</c>.
///
/// A source author is represented as follows:
/// <code>
/// &lt;Author&gt;
///   &lt;Firstname&gt;Nico&lt;/Firstname&gt;
///   &lt;LastName&gt;Dellaert&lt;/LastName&gt;
///   &lt;Email&gt;n.p.dellaert@tue.nl&lt;/Email&gt;
///   &lt;Institution&gt;
///     Eindhoven University of Technology
///   &lt;/Institution&gt;
/// &lt;/Author&gt;
/// </code>
/// </remarks>
[Serializable]
[XmlType(
    TypeName = "Author",
    AnonymousType = true)]
public sealed class DellaertJeunetXmlAuthor
{
    private string _firstName =
        string.Empty;

    private string _lastName =
        string.Empty;

    private string _email =
        string.Empty;

    private string _institution =
        string.Empty;

    /// <summary>
    /// Initializes an empty source author.
    /// </summary>
    /// <remarks>
    /// This public parameterless constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public DellaertJeunetXmlAuthor()
    {
    }

    /// <summary>
    /// Initializes a source author.
    /// </summary>
    /// <param name="firstName">
    /// Author first name.
    /// </param>
    /// <param name="lastName">
    /// Author last name.
    /// </param>
    /// <param name="email">
    /// Author email address.
    /// </param>
    /// <param name="institution">
    /// Author institution.
    /// </param>
    public DellaertJeunetXmlAuthor(
        string firstName,
        string lastName,
        string email,
        string institution)
    {
        FirstName =
            firstName;

        LastName =
            lastName;

        Email =
            email;

        Institution =
            institution;
    }

    /// <summary>
    /// Gets or sets the author first name.
    /// </summary>
    /// <remarks>
    /// The XML element is named <c>Firstname</c>, with a
    /// lowercase <c>n</c>, and must therefore be mapped
    /// explicitly.
    /// </remarks>
    [XmlElement(
        "Firstname",
        Order = 0)]
    public string FirstName
    {
        get =>
            _firstName;

        set =>
            _firstName =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the author last name.
    /// </summary>
    [XmlElement(
        "LastName",
        Order = 1)]
    public string LastName
    {
        get =>
            _lastName;

        set =>
            _lastName =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the author email address.
    /// </summary>
    /// <remarks>
    /// This property preserves the source value. Detailed
    /// email-address validation belongs to the source
    /// validator rather than to this XML data-transfer object.
    /// </remarks>
    [XmlElement(
        "Email",
        Order = 2)]
    public string Email
    {
        get =>
            _email;

        set =>
            _email =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the author institution.
    /// </summary>
    [XmlElement(
        "Institution",
        Order = 3)]
    public string Institution
    {
        get =>
            _institution;

        set =>
            _institution =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets a value indicating whether the author first name
    /// is available.
    /// </summary>
    [XmlIgnore]
    public bool HasFirstName =>
        !string.IsNullOrWhiteSpace(
            FirstName);

    /// <summary>
    /// Gets a value indicating whether the author last name is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasLastName =>
        !string.IsNullOrWhiteSpace(
            LastName);

    /// <summary>
    /// Gets a value indicating whether the author email
    /// address is available.
    /// </summary>
    [XmlIgnore]
    public bool HasEmail =>
        !string.IsNullOrWhiteSpace(
            Email);

    /// <summary>
    /// Gets a value indicating whether the author institution
    /// is available.
    /// </summary>
    [XmlIgnore]
    public bool HasInstitution =>
        !string.IsNullOrWhiteSpace(
            Institution);

    /// <summary>
    /// Gets a value indicating whether at least one author
    /// identity field is available.
    /// </summary>
    [XmlIgnore]
    public bool HasIdentity =>
        HasFirstName ||
        HasLastName;

    /// <summary>
    /// Gets the author display name.
    /// </summary>
    /// <remarks>
    /// The display name combines the first name and last name
    /// when available.
    /// </remarks>
    [XmlIgnore]
    public string DisplayName
    {
        get
        {
            if (HasFirstName &&
                HasLastName)
            {
                return
                    FirstName +
                    " " +
                    LastName;
            }

            if (HasLastName)
            {
                return LastName;
            }

            return FirstName;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the source author
    /// contains sufficient identity information.
    /// </summary>
    /// <remarks>
    /// An author is considered structurally valid when at
    /// least a first name or a last name is available.
    ///
    /// Email and institution are optional because some
    /// benchmark variants may omit them.
    /// </remarks>
    [XmlIgnore]
    public bool IsValid =>
        HasIdentity;

    /// <summary>
    /// Determines whether the first-name element should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a first name exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeFirstName()
    {
        return HasFirstName;
    }

    /// <summary>
    /// Determines whether the last-name element should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a last name exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeLastName()
    {
        return HasLastName;
    }

    /// <summary>
    /// Determines whether the email element should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an email address exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeEmail()
    {
        return HasEmail;
    }

    /// <summary>
    /// Determines whether the institution element should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when an institution exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeInstitution()
    {
        return HasInstitution;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (HasInstitution &&
            HasIdentity)
        {
            return
                DisplayName +
                " — " +
                Institution;
        }

        if (HasIdentity)
        {
            return DisplayName;
        }

        if (HasInstitution)
        {
            return Institution;
        }

        return string.Empty;
    }
}