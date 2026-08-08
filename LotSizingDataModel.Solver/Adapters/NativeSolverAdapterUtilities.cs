using System;
using System.IO;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Adapters;

/// <summary>
/// Provides small reusable helpers for native solver plugins.
/// </summary>
public static class NativeSolverAdapterUtilities
{
    /// <summary>
    /// Creates an unavailable solver description with one
    /// diagnostic message.
    /// </summary>
    /// <param name="solverKind">
    /// Solver kind.
    /// </param>
    /// <param name="solverName">
    /// Solver display name.
    /// </param>
    /// <param name="status">
    /// Availability status.
    /// </param>
    /// <param name="diagnostic">
    /// Diagnostic message.
    /// </param>
    /// <returns>
    /// Availability information.
    /// </returns>
    public static SolverAvailabilityInfo CreateAvailability(
        SolverKind solverKind,
        string solverName,
        SolverAvailabilityStatus status,
        string diagnostic)
    {
        if (string.IsNullOrWhiteSpace(
                solverName))
        {
            throw new ArgumentException(
                "A solver name is required.",
                nameof(solverName));
        }

        if (string.IsNullOrWhiteSpace(
                diagnostic))
        {
            throw new ArgumentException(
                "A diagnostic message is required.",
                nameof(diagnostic));
        }

        var result =
            new SolverAvailabilityInfo(
                solverKind,
                status)
            {
                SolverName =
                    solverName
            };

        result.AddDiagnostic(
            diagnostic);

        return result;
    }

    /// <summary>
    /// Returns the first existing full path among candidate
    /// paths.
    /// </summary>
    /// <param name="candidatePaths">
    /// Candidate file or directory paths.
    /// </param>
    /// <returns>
    /// First existing path, or an empty string when none exists.
    /// </returns>
    public static string FindFirstExistingPath(
        params string?[] candidatePaths)
    {
        ArgumentNullException.ThrowIfNull(
            candidatePaths);

        foreach (
            string? candidate
            in candidatePaths)
        {
            if (string.IsNullOrWhiteSpace(
                    candidate))
            {
                continue;
            }

            string expanded =
                Environment.ExpandEnvironmentVariables(
                    candidate.Trim());

            string fullPath;

            try
            {
                fullPath =
                    Path.GetFullPath(
                        expanded);
            }
            catch (
                Exception exception)
                when (
                    exception is
                        ArgumentException or
                        NotSupportedException or
                        PathTooLongException)
            {
                continue;
            }

            if (File.Exists(
                    fullPath) ||
                Directory.Exists(
                    fullPath))
            {
                return fullPath;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// Reads and trims an environment variable.
    /// </summary>
    /// <param name="name">
    /// Environment-variable name.
    /// </param>
    /// <returns>
    /// Trimmed value, or an empty string when undefined.
    /// </returns>
    public static string ReadEnvironmentVariable(
        string name)
    {
        if (string.IsNullOrWhiteSpace(
                name))
        {
            throw new ArgumentException(
                "An environment-variable name is required.",
                nameof(name));
        }

        return
            Environment.GetEnvironmentVariable(
                name.Trim())?.Trim() ??
            string.Empty;
    }
}
