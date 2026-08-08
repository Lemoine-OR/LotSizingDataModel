using System.Globalization;
using System.Text;
using LotSizingDataModel.Solver.Modeling;

namespace LotSizingDataModel.Solver.ConsoleApp;

/// <summary>
/// Writes a complete human-readable representation of a
/// solver-independent mathematical model.
/// </summary>
public static class MathematicalModelTextExporter
{
    /// <summary>
    /// Writes the supplied mathematical model to a text file.
    /// </summary>
    /// <param name="model">Mathematical model to export.</param>
    /// <param name="filePath">Destination text-file path.</param>
    public static void Write(
        MathematicalModel model,
        string filePath)
    {
        ArgumentNullException.ThrowIfNull(model);
        ArgumentException.ThrowIfNullOrWhiteSpace(filePath);

        model.EnsureValid();

        string? directory =
            Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        using var writer =
            new StreamWriter(
                filePath,
                append: false,
                new UTF8Encoding(
                    encoderShouldEmitUTF8Identifier: true));

        WriteHeader(
            writer,
            model);

        WriteVariables(
            writer,
            model);

        WriteObjective(
            writer,
            model);

        WriteObjectiveByCategory(
            writer,
            model);

        WriteConstraints(
            writer,
            model);
    }

    private static void WriteHeader(
        TextWriter writer,
        MathematicalModel model)
    {
        writer.WriteLine(
            "LOT SIZING MATHEMATICAL MODEL DUMP");
        writer.WriteLine(
            "==================================");
        writer.WriteLine();

        writer.WriteLine(
            $"Model                 : {model.Name}");
        writer.WriteLine(
            $"Variables             : {model.VariableCount}");
        writer.WriteLine(
            $"  continuous          : {model.ContinuousVariableCount}");
        writer.WriteLine(
            $"  integer             : {model.IntegerVariableCount}");
        writer.WriteLine(
            $"  binary              : {model.BinaryVariableCount}");
        writer.WriteLine(
            $"Enabled constraints   : {model.EnabledConstraintCount}");
        writer.WriteLine(
            $"Objective sense       : {model.Objective.Sense}");
        writer.WriteLine(
            $"Objective terms       : {model.Objective.Expression.TermCount}");
        writer.WriteLine(
            $"Objective constant    : {Format(model.Objective.Expression.Constant)}");
        writer.WriteLine();
    }

    private static void WriteVariables(
        TextWriter writer,
        MathematicalModel model)
    {
        writer.WriteLine(
            "VARIABLES");
        writer.WriteLine(
            "=========");
        writer.WriteLine();

        foreach (
            MathematicalVariable variable
            in model.Variables.OrderBy(
                variable =>
                    variable.Id))
        {
            writer.WriteLine(
                $"[{variable.Id}] {variable.Name}");

            writer.WriteLine(
                $"    Type      : {variable.VariableType}");

            writer.WriteLine(
                $"    Bounds    : [{FormatBound(variable.LowerBound)}, " +
                $"{FormatBound(variable.UpperBound)}]");

            writer.WriteLine(
                $"    DomainKey : {variable.DomainKey}");

            double objectiveCoefficient =
                GetObjectiveCoefficient(
                    model,
                    variable.Id);

            writer.WriteLine(
                $"    ObjCoeff  : {Format(objectiveCoefficient)}");

            writer.WriteLine();
        }
    }

    private static void WriteObjective(
        TextWriter writer,
        MathematicalModel model)
    {
        writer.WriteLine(
            "OBJECTIVE");
        writer.WriteLine(
            "=========");
        writer.WriteLine();

        writer.WriteLine(
            $"{model.Objective.Sense} {model.Objective.Name}");

        writer.WriteLine();

        foreach (
            LinearTerm term
            in model.Objective.Expression.Terms)
        {
            MathematicalVariable? variable =
                model.FindVariableById(
                    term.VariableId);

            writer.WriteLine(
                $"  {FormatSigned(term.Coefficient)} " +
                $"{FormatVariable(variable, term.VariableId)}");
        }

        if (model.Objective.Expression.Constant != 0.0)
        {
            writer.WriteLine(
                $"  {FormatSigned(model.Objective.Expression.Constant)}");
        }

        writer.WriteLine();
    }

    private static void WriteObjectiveByCategory(
        TextWriter writer,
        MathematicalModel model)
    {
        writer.WriteLine(
            "OBJECTIVE COEFFICIENTS BY DOMAIN CATEGORY");
        writer.WriteLine(
            "=========================================");
        writer.WriteLine();

        var rows =
            model.Objective.Expression.Terms
                .Select(
                    term =>
                    {
                        MathematicalVariable? variable =
                            model.FindVariableById(
                                term.VariableId);

                        string category =
                            GetCategory(
                                variable?.DomainKey);

                        return new
                        {
                            Category = category,
                            Term = term,
                            Variable = variable
                        };
                    })
                .GroupBy(
                    row =>
                        row.Category,
                    StringComparer.OrdinalIgnoreCase)
                .OrderBy(
                    group =>
                        group.Key,
                    StringComparer.OrdinalIgnoreCase);

        foreach (var group in rows)
        {
            writer.WriteLine(
                $"[{group.Key}]");

            writer.WriteLine(
                $"  term count          : {group.Count()}");

            writer.WriteLine(
                $"  coefficient sum     : " +
                $"{Format(group.Sum(row => row.Term.Coefficient))}");

            foreach (var row in group)
            {
                writer.WriteLine(
                    $"    {FormatSigned(row.Term.Coefficient)} " +
                    $"{FormatVariable(row.Variable, row.Term.VariableId)}");
            }

            writer.WriteLine();
        }
    }

    private static void WriteConstraints(
        TextWriter writer,
        MathematicalModel model)
    {
        writer.WriteLine(
            "CONSTRAINTS");
        writer.WriteLine(
            "===========");
        writer.WriteLine();

        foreach (
            LinearConstraint constraint
            in model.Constraints
                .Where(
                    constraint =>
                        constraint.IsEnabled)
                .OrderBy(
                    constraint =>
                        constraint.Id))
        {
            writer.WriteLine(
                $"[{constraint.Id}] {constraint.Name}");

            writer.WriteLine(
                $"    DomainKey : {constraint.DomainKey}");

            writer.Write(
                "    Equation  : ");

            bool first =
                true;

            foreach (
                LinearTerm term
                in constraint.LeftHandSide.Terms)
            {
                MathematicalVariable? variable =
                    model.FindVariableById(
                        term.VariableId);

                if (!first)
                {
                    writer.Write(
                        term.Coefficient >= 0.0
                            ? " + "
                            : " - ");
                }
                else if (term.Coefficient < 0.0)
                {
                    writer.Write("-");
                }

                writer.Write(
                    $"{Format(Math.Abs(term.Coefficient))}*" +
                    $"{FormatVariable(variable, term.VariableId)}");

                first =
                    false;
            }

            if (constraint.LeftHandSide.Constant != 0.0 ||
                first)
            {
                double constant =
                    constraint.LeftHandSide.Constant;

                if (!first)
                {
                    writer.Write(
                        constant >= 0.0
                            ? " + "
                            : " - ");
                }
                else if (constant < 0.0)
                {
                    writer.Write("-");
                }

                writer.Write(
                    Format(
                        Math.Abs(constant)));
            }

            writer.Write(
                $" {FormatSense(constraint.Sense)} " +
                $"{Format(constraint.RightHandSide)}");

            writer.WriteLine();
            writer.WriteLine();
        }
    }

    private static double GetObjectiveCoefficient(
        MathematicalModel model,
        int variableId)
    {
        LinearTerm? term =
            model.Objective.Expression.Terms.FirstOrDefault(
                term =>
                    term.VariableId == variableId);

        return term?.Coefficient ?? 0.0;
    }

    private static string GetCategory(
        string? domainKey)
    {
        if (string.IsNullOrWhiteSpace(domainKey))
        {
            return "(none)";
        }

        int separatorIndex =
            domainKey.IndexOf('|');

        if (separatorIndex < 0)
        {
            return domainKey;
        }

        return domainKey[..separatorIndex];
    }

    private static string FormatVariable(
        MathematicalVariable? variable,
        int variableId)
    {
        if (variable is null)
        {
            return $"<unknown:{variableId}>";
        }

        return
            $"{variable.Name} [id={variable.Id}; key={variable.DomainKey}]";
    }

    private static string FormatSense(
        MathematicalConstraintSense sense)
    {
        return sense switch
        {
            MathematicalConstraintSense.LessThanOrEqual =>
                "<=",

            MathematicalConstraintSense.Equal =>
                "=",

            MathematicalConstraintSense.GreaterThanOrEqual =>
                ">=",

            _ =>
                sense.ToString()
        };
    }

    private static string Format(
        double value)
    {
        return value.ToString(
            "G17",
            CultureInfo.InvariantCulture);
    }

    private static string FormatBound(
        double value)
    {
        if (double.IsPositiveInfinity(value))
        {
            return "+inf";
        }

        if (double.IsNegativeInfinity(value))
        {
            return "-inf";
        }

        return Format(value);
    }

    private static string FormatSigned(
        double value)
    {
        string sign =
            value >= 0.0
                ? "+"
                : "-";

        return
            $"{sign} {Format(Math.Abs(value))}";
    }
}
