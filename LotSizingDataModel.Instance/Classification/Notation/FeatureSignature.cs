using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using LotSizingDataModel.Core.Common;

namespace LotSizingDataModel.Instance.Classification.Notation;

/// <summary>
/// LSI feature and constraint block (beta).
/// The v1 structure deliberately stores stable feature codes
/// rather than one property per literature extension.
/// </summary>
[Serializable]
[XmlType(TypeName = "lsiFeatureSignature")]
public sealed class FeatureSignature : ModelObject
{
    [XmlArray("features")]
    [XmlArrayItem("feature")]
    public List<FeatureEntry> Features { get; } = new();

    public void Set(
        string code,
        FeatureState state,
        TemporalProfile? temporalProfile = null)
    {
        string normalizedCode = NormalizeCode(code);

        FeatureEntry? existing =
            Features.FirstOrDefault(entry =>
                string.Equals(
                    entry.Code,
                    normalizedCode,
                    StringComparison.Ordinal));

        if (existing is null)
        {
            existing = new FeatureEntry
            {
                Code = normalizedCode
            };
            Features.Add(existing);
        }

        existing.State = state;
        existing.TemporalProfile = temporalProfile;

        Features.Sort((left, right) =>
            StringComparer.Ordinal.Compare(
                left.Code,
                right.Code));
    }

    public FeatureEntry? Find(string code)
    {
        string normalizedCode = NormalizeCode(code);

        return Features.FirstOrDefault(entry =>
            string.Equals(
                entry.Code,
                normalizedCode,
                StringComparison.Ordinal));
    }

    private static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException(
                "An LSI feature code is required.",
                nameof(code));
        }

        return code.Trim().ToUpperInvariant();
    }
}

[Serializable]
[XmlType(TypeName = "lsiFeatureEntry")]
public sealed class FeatureEntry : ModelObject
{
    private string _code = string.Empty;
    private FeatureState _state = FeatureState.Unknown;
    private TemporalProfile? _temporalProfile;

    [XmlAttribute("code")]
    public string Code
    {
        get => _code;
        set => SetProperty(
            ref _code,
            value?.Trim().ToUpperInvariant()
                ?? string.Empty);
    }

    [XmlAttribute("state")]
    public FeatureState State
    {
        get => _state;
        set => SetProperty(ref _state, value);
    }

    [XmlElement("temporalProfile")]
    public TemporalProfile? TemporalProfile
    {
        get => _temporalProfile;
        set => SetProperty(ref _temporalProfile, value);
    }
}
