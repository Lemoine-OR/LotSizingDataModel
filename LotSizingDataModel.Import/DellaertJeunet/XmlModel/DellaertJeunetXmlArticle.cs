using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace LotSizingDataModel.Import.DellaertJeunet.XmlModel;

/// <summary>
/// Represents the bibliographic article metadata embedded in
/// a Dellaert–Jeunet XML benchmark instance.
/// </summary>
/// <remarks>
/// This class is a source-format data-transfer object.
///
/// It reproduces the structure of the optional
/// <c>Article</c> element found near the beginning of a
/// Dellaert–Jeunet XML instance.
///
/// A typical source fragment is:
/// <code>
/// &lt;Article&gt;
///   &lt;Name&gt;
///     Solving large unconstrained multilevel lot-sizing
///     problems using a hybrid genetic algorithm
///   &lt;/Name&gt;
///   &lt;Year&gt;2000&lt;/Year&gt;
///   &lt;Journal&gt;
///     International Journal of Production Research,
///     vol. 38, n°5
///   &lt;/Journal&gt;
///   &lt;Pages&gt;1083-1099&lt;/Pages&gt;
///   &lt;DOI&gt;
///     https://doi.org/10.1080/002075400189031
///   &lt;/DOI&gt;
///   &lt;Authors&gt;
///     &lt;Author&gt;...&lt;/Author&gt;
///   &lt;/Authors&gt;
/// &lt;/Article&gt;
/// </code>
///
/// Bibliographic validation and conversion into domain
/// metadata are performed by higher-level services.
/// </remarks>
[Serializable]
[XmlType(
    TypeName = "Article",
    AnonymousType = true)]
public sealed class DellaertJeunetXmlArticle
{
    private string _name =
        string.Empty;

    private int _year;

    private string _journal =
        string.Empty;

    private string _pages =
        string.Empty;

    private string _doi =
        string.Empty;

    private List<DellaertJeunetXmlAuthor> _authors =
        new();

    /// <summary>
    /// Initializes an empty bibliographic article.
    /// </summary>
    /// <remarks>
    /// This public parameterless constructor is required by
    /// <see cref="XmlSerializer"/>.
    /// </remarks>
    public DellaertJeunetXmlArticle()
    {
    }

    /// <summary>
    /// Initializes bibliographic article metadata.
    /// </summary>
    /// <param name="name">
    /// Article title.
    /// </param>
    /// <param name="year">
    /// Publication year.
    /// </param>
    /// <param name="journal">
    /// Journal description.
    /// </param>
    /// <param name="pages">
    /// Page range.
    /// </param>
    /// <param name="doi">
    /// DOI value or DOI URL.
    /// </param>
    /// <param name="authors">
    /// Article authors.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="authors"/> is
    /// <see langword="null"/>.
    /// </exception>
    public DellaertJeunetXmlArticle(
        string name,
        int year,
        string journal,
        string pages,
        string doi,
        IEnumerable<DellaertJeunetXmlAuthor> authors)
    {
        ArgumentNullException.ThrowIfNull(
            authors);

        Name =
            name;

        Year =
            year;

        Journal =
            journal;

        Pages =
            pages;

        Doi =
            doi;

        Authors =
            authors.ToList();
    }

    /// <summary>
    /// Gets or sets the article title.
    /// </summary>
    [XmlElement(
        "Name",
        Order = 0)]
    public string Name
    {
        get =>
            _name;

        set =>
            _name =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the publication year.
    /// </summary>
    /// <remarks>
    /// A value equal to zero indicates that no year was
    /// supplied by the source document.
    ///
    /// Detailed validation of plausible publication years is
    /// performed by the source validator.
    /// </remarks>
    [XmlElement(
        "Year",
        Order = 1)]
    public int Year
    {
        get =>
            _year;

        set =>
            _year =
                value;
    }

    /// <summary>
    /// Gets or sets the journal description.
    /// </summary>
    /// <remarks>
    /// The source value may include the journal title, volume
    /// and issue information in a single text field.
    /// </remarks>
    [XmlElement(
        "Journal",
        Order = 2)]
    public string Journal
    {
        get =>
            _journal;

        set =>
            _journal =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the page range.
    /// </summary>
    [XmlElement(
        "Pages",
        Order = 3)]
    public string Pages
    {
        get =>
            _pages;

        set =>
            _pages =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the article DOI.
    /// </summary>
    /// <remarks>
    /// The source format may contain either a bare DOI such as
    /// <c>10.1080/002075400189031</c> or a complete DOI URL.
    ///
    /// The original text is preserved by this DTO.
    /// Normalization is performed later by the importer.
    /// </remarks>
    [XmlElement(
        "DOI",
        Order = 4)]
    public string Doi
    {
        get =>
            _doi;

        set =>
            _doi =
                value?.Trim() ??
                string.Empty;
    }

    /// <summary>
    /// Gets or sets the article authors.
    /// </summary>
    /// <remarks>
    /// The outer XML element is named <c>Authors</c> and each
    /// contained element is named <c>Author</c>.
    ///
    /// The setter replaces a null collection with an empty
    /// collection to simplify validation and conversion.
    /// </remarks>
    [XmlArray(
        "Authors",
        Order = 5)]
    [XmlArrayItem(
        "Author",
        typeof(DellaertJeunetXmlAuthor),
        IsNullable = false)]
    public List<DellaertJeunetXmlAuthor> Authors
    {
        get =>
            _authors;

        set =>
            _authors =
                value ??
                new List<DellaertJeunetXmlAuthor>();
    }

    /// <summary>
    /// Gets a value indicating whether the article title is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasName =>
        !string.IsNullOrWhiteSpace(
            Name);

    /// <summary>
    /// Gets a value indicating whether a publication year is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasYear =>
        Year > 0;

    /// <summary>
    /// Gets a value indicating whether journal information is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasJournal =>
        !string.IsNullOrWhiteSpace(
            Journal);

    /// <summary>
    /// Gets a value indicating whether page information is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasPages =>
        !string.IsNullOrWhiteSpace(
            Pages);

    /// <summary>
    /// Gets a value indicating whether a DOI value is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasDoi =>
        !string.IsNullOrWhiteSpace(
            Doi);

    /// <summary>
    /// Gets a value indicating whether at least one author is
    /// available.
    /// </summary>
    [XmlIgnore]
    public bool HasAuthors =>
        Authors.Count > 0;

    /// <summary>
    /// Gets the number of authors.
    /// </summary>
    [XmlIgnore]
    public int AuthorCount =>
        Authors.Count;

    /// <summary>
    /// Gets the authors that contain sufficient identity
    /// information.
    /// </summary>
    [XmlIgnore]
    public IReadOnlyList<DellaertJeunetXmlAuthor>
        ValidAuthors =>
            Authors
                .Where(
                    author =>
                        author is not null &&
                        author.IsValid)
                .ToArray();

    /// <summary>
    /// Gets a compact author-list representation.
    /// </summary>
    /// <remarks>
    /// Valid display names are separated by semicolons.
    /// </remarks>
    [XmlIgnore]
    public string AuthorsDisplayText =>
        string.Join(
            "; ",
            ValidAuthors.Select(
                author =>
                    author.DisplayName));

    /// <summary>
    /// Gets a normalized DOI value without the common DOI URL
    /// prefix.
    /// </summary>
    /// <remarks>
    /// This property does not modify the original
    /// <see cref="Doi"/> value.
    /// </remarks>
    [XmlIgnore]
    public string NormalizedDoi
    {
        get
        {
            if (!HasDoi)
            {
                return string.Empty;
            }

            const string securePrefix =
                "https://doi.org/";

            const string insecurePrefix =
                "http://doi.org/";

            const string legacyPrefix =
                "http://dx.doi.org/";

            if (Doi.StartsWith(
                    securePrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    Doi[securePrefix.Length..]
                        .Trim();
            }

            if (Doi.StartsWith(
                    insecurePrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    Doi[insecurePrefix.Length..]
                        .Trim();
            }

            if (Doi.StartsWith(
                    legacyPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                return
                    Doi[legacyPrefix.Length..]
                        .Trim();
            }

            return Doi.Trim();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the article contains at
    /// least one bibliographic value.
    /// </summary>
    [XmlIgnore]
    public bool HasBibliographicData =>
        HasName ||
        HasYear ||
        HasJournal ||
        HasPages ||
        HasDoi ||
        HasAuthors;

    /// <summary>
    /// Gets a value indicating whether the bibliographic
    /// metadata is structurally valid.
    /// </summary>
    /// <remarks>
    /// An article is considered structurally valid when:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// the title is non-empty;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// the publication year is strictly positive;
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// every author entry is non-null and structurally valid.
    /// </description>
    /// </item>
    /// </list>
    ///
    /// Journal, pages and DOI are optional at DTO level.
    /// </remarks>
    [XmlIgnore]
    public bool IsValid =>
        HasName &&
        HasYear &&
        Authors.All(
            author =>
                author is not null &&
                author.IsValid);

    /// <summary>
    /// Adds an author to the article.
    /// </summary>
    /// <param name="author">
    /// Author to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="author"/> is
    /// <see langword="null"/>.
    /// </exception>
    public void AddAuthor(
        DellaertJeunetXmlAuthor author)
    {
        ArgumentNullException.ThrowIfNull(
            author);

        Authors.Add(
            author);
    }

    /// <summary>
    /// Removes all authors from the article.
    /// </summary>
    public void ClearAuthors()
    {
        Authors.Clear();
    }

    /// <summary>
    /// Creates a compact bibliographic citation.
    /// </summary>
    /// <returns>
    /// Human-readable citation assembled from the available
    /// metadata.
    /// </returns>
    public string BuildCitation()
    {
        var parts =
            new List<string>();

        if (HasAuthors)
        {
            string authorText =
                AuthorsDisplayText;

            if (!string.IsNullOrWhiteSpace(
                    authorText))
            {
                parts.Add(
                    authorText);
            }
        }

        if (HasYear)
        {
            parts.Add(
                $"({Year})");
        }

        if (HasName)
        {
            parts.Add(
                Name);
        }

        if (HasJournal)
        {
            parts.Add(
                Journal);
        }

        if (HasPages)
        {
            parts.Add(
                $"pp. {Pages}");
        }

        if (HasDoi)
        {
            parts.Add(
                $"DOI: {NormalizedDoi}");
        }

        return string.Join(
            ". ",
            parts);
    }

    /// <summary>
    /// Determines whether the article title should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a title exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeName()
    {
        return HasName;
    }

    /// <summary>
    /// Determines whether the publication year should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a year exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeYear()
    {
        return HasYear;
    }

    /// <summary>
    /// Determines whether journal information should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when journal information exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeJournal()
    {
        return HasJournal;
    }

    /// <summary>
    /// Determines whether page information should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when page information exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializePages()
    {
        return HasPages;
    }

    /// <summary>
    /// Determines whether a DOI should be serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when a DOI exists; otherwise,
    /// <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeDoi()
    {
        return HasDoi;
    }

    /// <summary>
    /// Determines whether the author collection should be
    /// serialized.
    /// </summary>
    /// <returns>
    /// <see langword="true"/> when at least one author exists;
    /// otherwise, <see langword="false"/>.
    /// </returns>
    public bool ShouldSerializeAuthors()
    {
        return HasAuthors;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return BuildCitation();
    }
}