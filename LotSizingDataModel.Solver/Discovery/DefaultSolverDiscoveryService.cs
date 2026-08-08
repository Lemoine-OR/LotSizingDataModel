using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LotSizingDataModel.Solver.Common;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Provides the default solver discovery implementation used
/// by the generic solver infrastructure.
/// </summary>
/// <remarks>
/// This service does not reference any vendor SDK. It discovers
/// native solver installation candidates and managed adapter
/// plugin assemblies using file-system conventions only.
/// Vendor-specific validation remains the responsibility of
/// the corresponding adapter plugin.
/// </remarks>
public sealed class DefaultSolverDiscoveryService :
    ISolverDiscoveryService
{
    private readonly SolverDiscoveryCandidateCollector
        _candidateCollector;

    /// <summary>
    /// Initializes the default solver discovery service.
    /// </summary>
    public DefaultSolverDiscoveryService()
        : this(
            new SolverDiscoveryCandidateCollector())
    {
    }

    /// <summary>
    /// Initializes the default solver discovery service with an
    /// explicitly supplied candidate collector.
    /// </summary>
    /// <param name="candidateCollector">
    /// Collector used to locate native solver installation
    /// candidates.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="candidateCollector"/> is
    /// <see langword="null"/>.
    /// </exception>
    public DefaultSolverDiscoveryService(
        SolverDiscoveryCandidateCollector candidateCollector)
    {
        ArgumentNullException.ThrowIfNull(
            candidateCollector);

        _candidateCollector =
            candidateCollector;
    }

    /// <summary>
    /// Discovers native solver candidates and managed adapter
    /// plugin assemblies.
    /// </summary>
    /// <param name="options">
    /// Solver-discovery options.
    /// </param>
    /// <param name="cancellationToken">
    /// Token used to cancel discovery.
    /// </param>
    /// <returns>
    /// Complete normalized solver-discovery result.
    /// </returns>
    public ValueTask<SolverDiscoveryResult> DiscoverAsync(
        SolverDiscoveryOptions options,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            options);

        options.EnsureValid();

        var stopwatch =
            Stopwatch.StartNew();

        var result =
            new SolverDiscoveryResult();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            IReadOnlyList<SolverDiscoveryCandidate> candidates =
                _candidateCollector.Collect(
                    options);

            foreach (
                SolverDiscoveryCandidate candidate
                in candidates)
            {
                cancellationToken.ThrowIfCancellationRequested();

                result.Candidates.Add(
                    candidate);
            }

            DiscoverAdapterDescriptors(
                options,
                result,
                cancellationToken);

            BuildAvailabilityInformation(
                candidates,
                options,
                result);

            stopwatch.Stop();

            result.CompletedAtUtc =
                DateTime.UtcNow;

            result.ElapsedSeconds =
                stopwatch.Elapsed.TotalSeconds;

            result.AddDiagnostic(
                $"Solver discovery completed with " +
                $"{result.Candidates.Count} installation " +
                $"candidates, " +
                $"{result.AdapterDescriptors.Count} adapter " +
                $"descriptors, and " +
                $"{result.AvailabilityInformation.Count} " +
                "solver availability records.");

            return ValueTask.FromResult(
                result);
        }
        catch (OperationCanceledException)
        {
            stopwatch.Stop();

            result.CompletedAtUtc =
                DateTime.UtcNow;

            result.ElapsedSeconds =
                stopwatch.Elapsed.TotalSeconds;

            result.AddDiagnostic(
                "Solver discovery was cancelled.");

            return ValueTask.FromResult(
                result);
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            result.CompletedAtUtc =
                DateTime.UtcNow;

            result.ElapsedSeconds =
                stopwatch.Elapsed.TotalSeconds;

            result.AddDiagnostic(
                $"Solver discovery failed: {exception.Message}");

            result.AddDiagnostic(
                exception.ToString());

            return ValueTask.FromResult(
                result);
        }
    }

    private static void DiscoverAdapterDescriptors(
        SolverDiscoveryOptions options,
        SolverDiscoveryResult result,
        CancellationToken cancellationToken)
    {
        foreach (
            string directory
            in GetAdapterSearchDirectories(
                options))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(
                    directory))
            {
                continue;
            }

            SearchOption searchOption =
                options.RecursiveAdapterSearch
                    ? SearchOption.AllDirectories
                    : SearchOption.TopDirectoryOnly;

            IEnumerable<string> assemblyPaths;

            try
            {
                assemblyPaths =
                    Directory.EnumerateFiles(
                        directory,
                        "LotSizingDataModel.Solver.*.dll",
                        searchOption);
            }
            catch (
                Exception exception)
                when (
                    exception is
                        UnauthorizedAccessException or
                        IOException)
            {
                result.AddDiagnostic(
                    $"Adapter directory '{directory}' could not " +
                    $"be scanned: {exception.Message}");

                continue;
            }

            foreach (
                string assemblyPath
                in assemblyPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (!TryInferSolverKind(
                        assemblyPath,
                        out SolverKind solverKind))
                {
                    continue;
                }

                string fullPath =
                    Path.GetFullPath(
                        assemblyPath);

                if (result.AdapterDescriptors.Any(
                        descriptor =>
                            string.Equals(
                                descriptor.AssemblyPath,
                                fullPath,
                                StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                string shortName =
                    solverKind switch
                    {
                        SolverKind.Cplex =>
                            "Cplex",

                        SolverKind.Gurobi =>
                            "Gurobi",

                        SolverKind.Xpress =>
                            "Xpress",

                        SolverKind.CoinOrCbc =>
                            "CoinOr",

                        _ =>
                            solverKind.ToString()
                    };

                var descriptor =
                    new SolverAdapterDescriptor
                    {
                        AdapterId =
                            $"LotSizingDataModel.Solver." +
                            $"{shortName}",

                        AdapterName =
                            $"{shortName} solver adapter",

                        AdapterVersion =
                            GetAssemblyVersionHint(
                                fullPath),

                        SolverKind =
                            solverKind,

                        AssemblyPath =
                            fullPath,

                        TypeName =
                            string.Empty
                    };

                result.AdapterDescriptors.Add(
                    descriptor);
            }
        }
    }

    private static void BuildAvailabilityInformation(
        IReadOnlyList<SolverDiscoveryCandidate> candidates,
        SolverDiscoveryOptions options,
        SolverDiscoveryResult result)
    {
        foreach (
            SolverKind solverKind
            in GetConcreteSolverKinds())
        {
            SolverDiscoveryCandidate? bestCandidate =
                candidates
                    .Where(
                        candidate =>
                            candidate.SolverKind ==
                                solverKind &&
                            candidate.Exists)
                    .OrderBy(
                        candidate =>
                            candidate.Priority)
                    .FirstOrDefault();

            var availability =
                new SolverAvailabilityInfo(
                    solverKind,
                    bestCandidate is null
                        ? SolverAvailabilityStatus.NotInstalled
                        : options.ValidateLicenses
                            ? SolverAvailabilityStatus
                                .AvailableWithLimitations
                            : SolverAvailabilityStatus.Available)
                {
                    SolverName =
                        GetSolverName(
                            solverKind),

                    InstallationPath =
                        bestCandidate?.Path ??
                        string.Empty,

                    SolverVersion =
                        bestCandidate?.VersionHint ??
                        string.Empty
                };

            if (bestCandidate is null)
            {
                availability.AddDiagnostic(
                    "No existing installation candidate was " +
                    "found.");

                result.AvailabilityInformation.Add(
                    availability);

                continue;
            }

            availability.AddDiagnostic(
                $"Installation candidate detected from " +
                $"{bestCandidate.Source}: " +
                $"'{bestCandidate.Path}'.");

            if (options.ValidateLicenses)
            {
                availability.AddLimitation(
                    "Native solver and license validation is " +
                    "deferred to the vendor-specific adapter.");

                availability.AddDiagnostic(
                    "The generic solver project intentionally " +
                    "does not load vendor SDKs during discovery.");
            }

            result.AvailabilityInformation.Add(
                availability);
        }
    }

    private static IReadOnlyList<string>
        GetAdapterSearchDirectories(
            SolverDiscoveryOptions options)
    {
        var directories =
            new List<string>();

        string applicationDirectory =
            AppContext.BaseDirectory;

        if (options.SearchApplicationDirectory)
        {
            directories.Add(
                applicationDirectory);
        }

        if (options.SearchPluginSubdirectory)
        {
            directories.Add(
                Path.Combine(
                    applicationDirectory,
                    options.PluginSubdirectoryName));
        }

        directories.AddRange(
            options.AdapterSearchDirectories);

        return directories
            .Where(
                directory =>
                    !string.IsNullOrWhiteSpace(
                        directory))
            .Select(
                directory =>
                    Path.GetFullPath(
                        Environment.ExpandEnvironmentVariables(
                            directory.Trim())))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool TryInferSolverKind(
        string assemblyPath,
        out SolverKind solverKind)
    {
        string fileName =
            Path.GetFileNameWithoutExtension(
                assemblyPath);

        if (fileName.EndsWith(
                ".Cplex",
                StringComparison.OrdinalIgnoreCase))
        {
            solverKind =
                SolverKind.Cplex;

            return true;
        }

        if (fileName.EndsWith(
                ".Gurobi",
                StringComparison.OrdinalIgnoreCase))
        {
            solverKind =
                SolverKind.Gurobi;

            return true;
        }

        if (fileName.EndsWith(
                ".Xpress",
                StringComparison.OrdinalIgnoreCase))
        {
            solverKind =
                SolverKind.Xpress;

            return true;
        }

        if (fileName.EndsWith(
                ".CoinOr",
                StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(
                ".CoinOrCbc",
                StringComparison.OrdinalIgnoreCase) ||
            fileName.EndsWith(
                ".Cbc",
                StringComparison.OrdinalIgnoreCase))
        {
            solverKind =
                SolverKind.CoinOrCbc;

            return true;
        }

        solverKind =
            SolverKind.Unknown;

        return false;
    }

    private static string GetAssemblyVersionHint(
        string assemblyPath)
    {
        try
        {
            FileVersionInfo versionInfo =
                FileVersionInfo.GetVersionInfo(
                    assemblyPath);

            if (!string.IsNullOrWhiteSpace(
                    versionInfo.FileVersion))
            {
                return versionInfo.FileVersion;
            }
        }
        catch (
            Exception exception)
            when (
                exception is
                    IOException or
                    UnauthorizedAccessException or
                    ArgumentException)
        {
            _ =
                exception;
        }

        return "1.0.0";
    }

    private static string GetSolverName(
        SolverKind solverKind)
    {
        return solverKind switch
        {
            SolverKind.Cplex =>
                "IBM ILOG CPLEX",

            SolverKind.Gurobi =>
                "Gurobi Optimizer",

            SolverKind.Xpress =>
                "FICO Xpress Optimizer",

            SolverKind.CoinOrCbc =>
                "COIN-OR CBC",

            _ =>
                solverKind.ToString()
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
}
