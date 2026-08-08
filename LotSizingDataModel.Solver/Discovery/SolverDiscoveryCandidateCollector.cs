using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Collects candidate native solver locations from explicit
/// paths, environment variables, the operating-system PATH,
/// and conventional installation directories.
/// </summary>
public sealed class SolverDiscoveryCandidateCollector
{
    /// <summary>
    /// Initializes a new discovery-candidate collector.
    /// </summary>
    public SolverDiscoveryCandidateCollector()
    {
    }

    /// <summary>
    /// Collects native solver location candidates.
    /// </summary>
    /// <param name="options">
    /// Solver-discovery options.
    /// </param>
    /// <returns>
    /// Distinct candidates ordered by priority and path.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="options"/> is
    /// <see langword="null"/>.
    /// </exception>
    public IReadOnlyList<SolverDiscoveryCandidate> Collect(
        SolverDiscoveryOptions options)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        options.EnsureValid();

        var candidates =
            new List<SolverDiscoveryCandidate>();

        AddExplicitDirectories(
            candidates,
            options);

        if (options.SearchEnvironmentVariables)
        {
            AddEnvironmentVariableCandidates(
                candidates);
        }

        if (options.SearchSystemPath)
        {
            AddSystemPathCandidates(
                candidates);
        }

        if (options.SearchCommonInstallationDirectories)
        {
            AddCommonInstallationCandidates(
                candidates);
        }

        return candidates
            .GroupBy(
                candidate =>
                    BuildCandidateKey(
                        candidate),
                StringComparer.OrdinalIgnoreCase)
            .Select(
                group =>
                    group
                        .OrderBy(
                            candidate =>
                                candidate.Priority)
                        .First())
            .OrderBy(
                candidate =>
                    candidate.Priority)
            .ThenBy(
                candidate =>
                    candidate.SolverKind)
            .ThenBy(
                candidate =>
                    candidate.Path,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static void AddExplicitDirectories(
        ICollection<SolverDiscoveryCandidate> candidates,
        SolverDiscoveryOptions options)
    {
        foreach (
            string configuredDirectory
            in options.SolverSearchDirectories)
        {
            string expandedPath =
                ExpandPath(
                    configuredDirectory);

            foreach (
                SolverKind solverKind
                in GetConcreteSolverKinds())
            {
                candidates.Add(
                    CreateCandidate(
                        solverKind,
                        SolverDiscoverySource
                            .ExplicitConfiguration,
                        expandedPath,
                        "Explicitly configured solver search " +
                        "directory.",
                        priority:
                            10));
            }
        }
    }

    private static void AddEnvironmentVariableCandidates(
        ICollection<SolverDiscoveryCandidate> candidates)
    {
        foreach (
            SolverEnvironmentVariableDefinition definition
            in SolverEnvironmentVariableCatalog.All)
        {
            if (definition.IsLicenseVariable)
            {
                continue;
            }

            string? value =
                Environment.GetEnvironmentVariable(
                    definition.VariableName);

            if (string.IsNullOrWhiteSpace(
                    value))
            {
                continue;
            }

            candidates.Add(
                CreateCandidate(
                    definition.SolverKind,
                    SolverDiscoverySource
                        .EnvironmentVariable,
                    ExpandPath(
                        value),
                    $"Environment variable " +
                    $"{definition.VariableName}.",
                    priority:
                        20 +
                        definition.Priority));
        }
    }

    private static void AddSystemPathCandidates(
        ICollection<SolverDiscoveryCandidate> candidates)
    {
        string? pathValue =
            Environment.GetEnvironmentVariable(
                "PATH");

        if (string.IsNullOrWhiteSpace(
                pathValue))
        {
            return;
        }

        string[] directories =
            pathValue.Split(
                Path.PathSeparator,
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        foreach (
            string directory
            in directories)
        {
            string expandedDirectory =
                ExpandPath(
                    directory);

            foreach (
                SolverKind solverKind
                in GetConcreteSolverKinds())
            {
                if (!ContainsSolverExecutable(
                        expandedDirectory,
                        solverKind))
                {
                    continue;
                }

                candidates.Add(
                    CreateCandidate(
                        solverKind,
                        SolverDiscoverySource.SystemPath,
                        expandedDirectory,
                        "Solver executable found through the " +
                        "operating-system PATH.",
                        priority:
                            200));
            }
        }
    }

    private static void AddCommonInstallationCandidates(
        ICollection<SolverDiscoveryCandidate> candidates)
    {
        foreach (
            SolverInstallationPathDefinition definition
            in SolverInstallationPathCatalog.All)
        {
            string expandedPath =
                ExpandPath(
                    definition.PathPattern);

            candidates.Add(
                CreateCandidate(
                    definition.SolverKind,
                    SolverDiscoverySource
                        .CommonInstallationDirectory,
                    expandedPath,
                    definition.Description,
                    priority:
                        300 +
                        definition.Priority));
        }
    }

    private static SolverDiscoveryCandidate CreateCandidate(
        SolverKind solverKind,
        SolverDiscoverySource source,
        string path,
        string description,
        int priority)
    {
        return new SolverDiscoveryCandidate
        {
            SolverKind =
                solverKind,

            Source =
                source,

            Path =
                path,

            Description =
                description,

            Priority =
                priority,

            Exists =
                Directory.Exists(
                    path) ||
                File.Exists(
                    path)
        };
    }

    private static bool ContainsSolverExecutable(
        string directory,
        SolverKind solverKind)
    {
        if (!Directory.Exists(
                directory))
        {
            return false;
        }

        foreach (
            string executableName
            in GetExecutableNames(
                solverKind))
        {
            if (File.Exists(
                    Path.Combine(
                        directory,
                        executableName)))
            {
                return true;
            }
        }

        return false;
    }

    private static IReadOnlyList<string> GetExecutableNames(
        SolverKind solverKind)
    {
        bool isWindows =
            OperatingSystem.IsWindows();

        return solverKind switch
        {
            SolverKind.Cplex =>
                isWindows
                    ? new[]
                    {
                        "cplex.exe"
                    }
                    : new[]
                    {
                        "cplex"
                    },

            SolverKind.Gurobi =>
                isWindows
                    ? new[]
                    {
                        "gurobi_cl.exe"
                    }
                    : new[]
                    {
                        "gurobi_cl"
                    },

            SolverKind.Xpress =>
                isWindows
                    ? new[]
                    {
                        "optimizer.exe",
                        "xprs.exe"
                    }
                    : new[]
                    {
                        "optimizer",
                        "xprs"
                    },

            SolverKind.CoinOrCbc =>
                isWindows
                    ? new[]
                    {
                        "cbc.exe"
                    }
                    : new[]
                    {
                        "cbc"
                    },

            _ =>
                Array.Empty<string>()
        };
    }

    private static IReadOnlyList<SolverKind>
        GetConcreteSolverKinds()
    {
        return new[]
        {
            SolverKind.Cplex,
            SolverKind.Gurobi,
            SolverKind.Xpress,
            SolverKind.CoinOrCbc
        };
    }

    private static string ExpandPath(
        string path)
    {
        string expanded =
            Environment.ExpandEnvironmentVariables(
                path.Trim());

        return Path.GetFullPath(
            expanded);
    }

    private static string BuildCandidateKey(
        SolverDiscoveryCandidate candidate)
    {
        return
            $"{candidate.SolverKind}|" +
            $"{Path.GetFullPath(candidate.Path)}";
    }
}
