namespace LotSizingDataModel.Solver.External;

/// <summary>
/// Locates optional native-solver command-line executables without creating
/// a compile-time dependency on their SDKs.
/// </summary>
public static class ExternalSolverExecutableLocator
{
    /// <summary>
    /// Resolves an executable from an explicit environment variable,
    /// installation-home hints and the current <c>PATH</c>.
    /// </summary>
    /// <param name="explicitExecutableEnvironmentVariable">
    /// Environment variable that may contain a complete executable path.
    /// </param>
    /// <param name="homeEnvironmentVariables">
    /// Environment variables that may contain an installation root.
    /// </param>
    /// <param name="relativeExecutablePaths">
    /// Relative executable paths tested under every installation root.
    /// </param>
    /// <param name="pathExecutableNames">
    /// Executable names searched in the current <c>PATH</c>.
    /// </param>
    /// <returns>Resolved full path, or an empty string.</returns>
    public static string Resolve(
        string explicitExecutableEnvironmentVariable,
        IEnumerable<string> homeEnvironmentVariables,
        IEnumerable<string> relativeExecutablePaths,
        IEnumerable<string> pathExecutableNames)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            explicitExecutableEnvironmentVariable);

        ArgumentNullException.ThrowIfNull(
            homeEnvironmentVariables);
        ArgumentNullException.ThrowIfNull(
            relativeExecutablePaths);
        ArgumentNullException.ThrowIfNull(
            pathExecutableNames);

        string explicitPath =
            Environment.GetEnvironmentVariable(
                explicitExecutableEnvironmentVariable)?.Trim() ??
            string.Empty;

        if (File.Exists(explicitPath))
        {
            return Path.GetFullPath(
                explicitPath);
        }

        string[] relativePaths =
            relativeExecutablePaths
                .Where(path => !string.IsNullOrWhiteSpace(path))
                .ToArray();

        foreach (string environmentVariable in homeEnvironmentVariables)
        {
            if (string.IsNullOrWhiteSpace(
                    environmentVariable))
            {
                continue;
            }

            string home =
                Environment.GetEnvironmentVariable(
                    environmentVariable)?.Trim() ??
                string.Empty;

            if (string.IsNullOrWhiteSpace(home))
            {
                continue;
            }

            foreach (string relativePath in relativePaths)
            {
                string candidate =
                    Path.Combine(
                        home,
                        relativePath);

                if (File.Exists(candidate))
                {
                    return Path.GetFullPath(
                        candidate);
                }
            }
        }

        return FindOnPath(
            pathExecutableNames);
    }

    /// <summary>
    /// Searches the current process <c>PATH</c> for one of the specified
    /// executable names.
    /// </summary>
    /// <param name="executableNames">Executable names to test.</param>
    /// <returns>Resolved full path, or an empty string.</returns>
    public static string FindOnPath(
        IEnumerable<string> executableNames)
    {
        ArgumentNullException.ThrowIfNull(
            executableNames);

        string pathValue =
            Environment.GetEnvironmentVariable("PATH") ??
            string.Empty;

        string[] directories =
            pathValue.Split(
                Path.PathSeparator,
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries);

        foreach (string directory in directories)
        {
            foreach (string executableName in executableNames)
            {
                if (string.IsNullOrWhiteSpace(
                        executableName))
                {
                    continue;
                }

                try
                {
                    string candidate =
                        Path.Combine(
                            directory,
                            executableName.Trim());

                    if (File.Exists(candidate))
                    {
                        return Path.GetFullPath(
                            candidate);
                    }
                }
                catch
                {
                    // Ignore malformed PATH entries and keep searching.
                }
            }
        }

        return string.Empty;
    }
}
