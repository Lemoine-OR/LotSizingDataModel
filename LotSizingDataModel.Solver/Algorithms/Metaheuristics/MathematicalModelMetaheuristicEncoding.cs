using LotSizingDataModel.Solver.Modeling;
using MetaheuristicsPlatform.SearchSpaces.Continuous;

namespace LotSizingDataModel.Solver.Algorithms.Metaheuristics;

public sealed class MathematicalModelMetaheuristicEncoding
{
    private readonly MathematicalVariable[] _variables;
    private readonly int[] _searchModelIndices;
    private readonly double[] _effectiveLowerBounds;
    private readonly double[] _effectiveUpperBounds;
    private readonly double[] _fixedValues;
    private readonly Dictionary<int, int> _modelIndexByVariableId;

    public MathematicalModelMetaheuristicEncoding(
        MathematicalModel model,
        MathematicalModelMetaheuristicEncodingOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(
            model);

        model.EnsureValid();

        options ??=
            new MathematicalModelMetaheuristicEncodingOptions();

        ArgumentNullException.ThrowIfNull(
            options.BoundOverrides);

        _variables =
            model.Variables
                .OrderBy(
                    variable =>
                        variable.Id)
                .ToArray();

        if (_variables.Length == 0)
        {
            throw new InvalidOperationException(
                "A metaheuristic encoding requires at least one mathematical variable.");
        }

        _modelIndexByVariableId =
            _variables
                .Select(
                    (variable, index) =>
                        new
                        {
                            variable.Id,
                            Index = index
                        })
                .ToDictionary(
                    entry =>
                        entry.Id,
                    entry =>
                        entry.Index);

        foreach (int variableId
                 in options.BoundOverrides.Keys)
        {
            if (!_modelIndexByVariableId.ContainsKey(
                    variableId))
            {
                throw new InvalidOperationException(
                    $"Bound override references unknown variable identifier '{variableId}'.");
            }
        }

        _fixedValues =
            Enumerable.Repeat(
                    double.NaN,
                    _variables.Length)
                .ToArray();

        var searchIndices =
            new List<int>();

        var searchLower =
            new List<double>();

        var searchUpper =
            new List<double>();

        var effectiveLower =
            new List<double>();

        var effectiveUpper =
            new List<double>();

        for (int modelIndex = 0;
             modelIndex < _variables.Length;
             modelIndex++)
        {
            MathematicalVariable variable =
                _variables[modelIndex];

            EnsureSupportedVariableType(
                variable);

            double lower =
                variable.LowerBound;

            double upper =
                variable.UpperBound;

            if (options.BoundOverrides.TryGetValue(
                    variable.Id,
                    out MetaheuristicVariableBounds overrideBounds))
            {
                ValidateOverride(
                    variable,
                    overrideBounds);

                lower =
                    overrideBounds.Lower;

                upper =
                    overrideBounds.Upper;
            }

            if (!double.IsFinite(lower) ||
                !double.IsFinite(upper))
            {
                throw new NotSupportedException(
                    $"Variable '{variable.Name}' requires explicit finite metaheuristic bounds.");
            }

            if (lower >
                upper)
            {
                throw new InvalidOperationException(
                    $"Effective bounds for variable '{variable.Name}' are inconsistent.");
            }

            if (lower ==
                upper)
            {
                _fixedValues[modelIndex] =
                    NormalizeValue(
                        variable,
                        lower,
                        lower,
                        upper);

                continue;
            }

            ValidateFreeBounds(
                variable,
                lower,
                upper);

            searchIndices.Add(
                modelIndex);

            searchLower.Add(
                lower);

            searchUpper.Add(
                upper);

            effectiveLower.Add(
                lower);

            effectiveUpper.Add(
                upper);
        }

        if (searchIndices.Count == 0)
        {
            throw new NotSupportedException(
                "Metaheuristic search requires at least one non-fixed variable.");
        }

        _searchModelIndices =
            searchIndices.ToArray();

        _effectiveLowerBounds =
            effectiveLower.ToArray();

        _effectiveUpperBounds =
            effectiveUpper.ToArray();

        SearchSpace =
            new BoundedContinuousSearchSpace(
                searchLower,
                searchUpper);
    }

    public IBoundedContinuousSearchSpace SearchSpace
    {
        get;
    }

    public IReadOnlyList<MathematicalVariable> ModelVariables =>
        _variables;

    public int ModelDimension =>
        _variables.Length;

    public int SearchDimension =>
        _searchModelIndices.Length;

    public double[] Decode(
        ReadOnlySpan<double> searchPoint)
    {
        if (searchPoint.Length !=
            SearchDimension)
        {
            throw new ArgumentException(
                $"Expected search dimension {SearchDimension}, received {searchPoint.Length}.",
                nameof(searchPoint));
        }

        var values =
            (double[])_fixedValues.Clone();

        for (int searchIndex = 0;
             searchIndex < _searchModelIndices.Length;
             searchIndex++)
        {
            double raw =
                searchPoint[searchIndex];

            if (!double.IsFinite(
                    raw))
            {
                throw new InvalidOperationException(
                    $"Search coordinate {searchIndex} is not finite.");
            }

            int modelIndex =
                _searchModelIndices[searchIndex];

            MathematicalVariable variable =
                _variables[modelIndex];

            values[modelIndex] =
                NormalizeValue(
                    variable,
                    raw,
                    _effectiveLowerBounds[searchIndex],
                    _effectiveUpperBounds[searchIndex]);
        }

        return values;
    }

    public int GetModelIndex(
        int variableId)
    {
        if (!_modelIndexByVariableId.TryGetValue(
                variableId,
                out int index))
        {
            throw new KeyNotFoundException(
                $"Unknown mathematical variable identifier '{variableId}'.");
        }

        return index;
    }

    private static void EnsureSupportedVariableType(
        MathematicalVariable variable)
    {
        switch (variable.VariableType)
        {
            case MathematicalVariableType.Continuous:
            case MathematicalVariableType.Integer:
            case MathematicalVariableType.Binary:
                return;

            case MathematicalVariableType.SemiContinuous:
            case MathematicalVariableType.SemiInteger:
                throw new NotSupportedException(
                    $"Variable '{variable.Name}' uses a semi-domain not enabled in alpha.37.");

            default:
                throw new NotSupportedException(
                    $"Variable '{variable.Name}' uses unsupported type '{variable.VariableType}'.");
        }
    }

    private static void ValidateOverride(
        MathematicalVariable variable,
        MetaheuristicVariableBounds bounds)
    {
        if (!double.IsFinite(bounds.Lower) ||
            !double.IsFinite(bounds.Upper) ||
            bounds.Lower >=
                bounds.Upper)
        {
            throw new InvalidOperationException(
                $"Finite strictly ordered override bounds are required for variable '{variable.Name}'.");
        }

        if (variable.HasFiniteLowerBound &&
            bounds.Lower <
                variable.LowerBound)
        {
            throw new InvalidOperationException(
                $"Override lower bound for variable '{variable.Name}' violates the mathematical lower bound.");
        }

        if (variable.HasFiniteUpperBound &&
            bounds.Upper >
                variable.UpperBound)
        {
            throw new InvalidOperationException(
                $"Override upper bound for variable '{variable.Name}' violates the mathematical upper bound.");
        }
    }

    private static void ValidateFreeBounds(
        MathematicalVariable variable,
        double lower,
        double upper)
    {
        if (variable.VariableType ==
            MathematicalVariableType.Binary)
        {
            if (lower != 0.0 ||
                upper != 1.0)
            {
                throw new NotSupportedException(
                    $"A free binary variable '{variable.Name}' must expose the complete [0,1] latent interval.");
            }

            return;
        }

        if (variable.VariableType ==
            MathematicalVariableType.Integer &&
            (lower !=
                 Math.Truncate(lower) ||
             upper !=
                 Math.Truncate(upper)))
        {
            throw new InvalidOperationException(
                $"Integer variable '{variable.Name}' requires integral effective bounds.");
        }
    }

    private static double NormalizeValue(
        MathematicalVariable variable,
        double raw,
        double lower,
        double upper)
    {
        double clamped =
            Math.Clamp(
                raw,
                lower,
                upper);

        return variable.VariableType switch
        {
            MathematicalVariableType.Continuous =>
                clamped,

            MathematicalVariableType.Integer =>
                Math.Clamp(
                    Math.Round(
                        clamped,
                        MidpointRounding.AwayFromZero),
                    lower,
                    upper),

            MathematicalVariableType.Binary =>
                clamped >= 0.5
                    ? 1.0
                    : 0.0,

            _ =>
                throw new NotSupportedException(
                    $"Variable type '{variable.VariableType}' cannot be normalized by alpha.37.")
        };
    }
}
