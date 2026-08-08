using System;
using System.Collections.Generic;
using System.IO;

namespace LotSizingDataModel.Solver.Cplex;

/// <summary>
/// Describes and discovers supported IBM ILOG CPLEX
/// installations.
/// </summary>
public static class CplexInstallationLocator
{
    private static readonly IReadOnlyList<CplexInstallationDefinition>
        Definitions =
        [
            new(
                "22.2",
                [
                    "CPLEX_STUDIO_DIR222",
                    "CPLEX_STUDIO_DIR2220"
                ],
                [
                    "CPLEX_Studio222",
                    "CPLEX_Studio2220"
                ]),
            new(
                "22.1.2",
                ["CPLEX_STUDIO_DIR2212"],
                ["CPLEX_Studio2212"]),
            new(
                "22.1.1",
                ["CPLEX_STUDIO_DIR2211"],
                ["CPLEX_Studio2211"]),
            new(
                "22.1",
                ["CPLEX_STUDIO_DIR221"],
                ["CPLEX_Studio221"]),
            new(
                "20.1",
                ["CPLEX_STUDIO_DIR201"],
                ["CPLEX_Studio201"])
        ];

    /// <summary>
    /// Gets the supported installation definitions, ordered
    /// from newest to oldest.
    /// </summary>
    public static IReadOnlyList<CplexInstallationDefinition>
        SupportedInstallations =>
            Definitions;

    /// <summary>
    /// Finds the newest compatible CPLEX installation visible
    /// to the current process.
    /// </summary>
    /// <returns>
    /// Discovery result containing the selected installation and
    /// diagnostic messages.
    /// </returns>
    public static CplexInstallationDiscoveryResult Discover()
    {
        var diagnostics =
            new List<string>();

        foreach (CplexInstallationDefinition definition in Definitions)
        {
            foreach (string variableName in definition.EnvironmentVariables)
            {
                string? root =
                    Environment.GetEnvironmentVariable(
                        variableName);

                if (TryResolve(
                        definition.Version,
                        root,
                        variableName,
                        out CplexInstallationInfo? info))
                {
                    return new CplexInstallationDiscoveryResult(
                        info,
                        diagnostics);
                }

                if (!string.IsNullOrWhiteSpace(root))
                {
                    diagnostics.Add(
                        $"Environment variable '{variableName}' points " +
                        $"to '{root}', but the required CPLEX .NET " +
                        "assemblies were not found there.");
                }
            }

            if (!OperatingSystem.IsWindows())
            {
                continue;
            }

            string programFiles =
                Environment.GetFolderPath(
                    Environment.SpecialFolder.ProgramFiles);

            foreach (string folderName in definition.WindowsFolderNames)
            {
                string root =
                    Path.Combine(
                        programFiles,
                        "IBM",
                        "ILOG",
                        folderName);

                if (TryResolve(
                        definition.Version,
                        root,
                        "default Windows installation",
                        out CplexInstallationInfo? info))
                {
                    return new CplexInstallationDiscoveryResult(
                        info,
                        diagnostics);
                }
            }
        }

        diagnostics.Add(
            "No compatible CPLEX installation was found. " +
            "Supported automatic discovery currently covers " +
            "22.2.x, 22.1.2, 22.1.1, 22.1 and 20.1.");

        return new CplexInstallationDiscoveryResult(
            null,
            diagnostics);
    }

    private static bool TryResolve(
        string version,
        string? rootDirectory,
        string source,
        out CplexInstallationInfo? installation)
    {
        installation =
            null;

        if (string.IsNullOrWhiteSpace(rootDirectory))
        {
            return false;
        }

        string root =
            Environment.ExpandEnvironmentVariables(
                rootDirectory.Trim());

        string managedDirectory =
            Path.Combine(
                root,
                "cplex",
                "bin",
                "x64_win64");

        string concertAssembly =
            Path.Combine(
                managedDirectory,
                "ILOG.Concert.dll");

        string cplexAssembly =
            Path.Combine(
                managedDirectory,
                "ILOG.CPLEX.dll");

        if (!File.Exists(concertAssembly) ||
            !File.Exists(cplexAssembly))
        {
            return false;
        }

        installation =
            new CplexInstallationInfo(
                version,
                root,
                managedDirectory,
                source);

        return true;
    }
}
