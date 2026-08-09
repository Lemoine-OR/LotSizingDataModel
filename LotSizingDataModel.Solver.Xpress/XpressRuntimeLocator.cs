using System.Reflection;
using System.Runtime.Loader;

namespace LotSizingDataModel.Solver.Xpress;

/// <summary>
/// Locates and loads the optional FICO Xpress Optimizer managed assembly.
/// </summary>
internal static class XpressRuntimeLocator
{
    internal const string ExplicitAssemblyEnvironmentVariable =
        "LOTSIZING_XPRESS_OPTIMIZER_ASSEMBLY";

    /// <summary>
    /// Loads the already available Optimizer assembly or resolves it from common
    /// Xpress installation locations.
    /// </summary>
    /// <param name="resolvedPath">Resolved assembly path when loaded from disk.</param>
    /// <returns>Loaded assembly, or <see langword="null"/>.</returns>
    internal static Assembly? TryLoad(
        out string resolvedPath)
    {
        resolvedPath = string.Empty;

        Assembly? loaded =
            AppDomain.CurrentDomain
                .GetAssemblies()
                .FirstOrDefault(
                    assembly =>
                        string.Equals(
                            assembly.GetName().Name,
                            "Optimizer",
                            StringComparison.OrdinalIgnoreCase));

        if (loaded is not null)
        {
            resolvedPath =
                string.IsNullOrWhiteSpace(loaded.Location)
                    ? "already loaded"
                    : loaded.Location;
            return loaded;
        }

        string explicitPath =
            Environment.GetEnvironmentVariable(
                ExplicitAssemblyEnvironmentVariable)?.Trim() ??
            string.Empty;

        if (File.Exists(explicitPath))
        {
            resolvedPath = Path.GetFullPath(explicitPath);
            return AssemblyLoadContext.Default.LoadFromAssemblyPath(
                resolvedPath);
        }

        string xpressDirectory =
            Environment.GetEnvironmentVariable("XPRESSDIR")?.Trim() ??
            string.Empty;

        if (!string.IsNullOrWhiteSpace(xpressDirectory))
        {
            string[] candidates =
            [
                Path.Combine(xpressDirectory, "bin", "Optimizer.dll"),
                Path.Combine(xpressDirectory, "lib", "Optimizer.dll"),
                Path.Combine(xpressDirectory, "bin", "dotnet", "Optimizer.dll"),
                Path.Combine(xpressDirectory, "lib", "dotnet", "Optimizer.dll"),
                Path.Combine(xpressDirectory, "Optimizer.dll")
            ];

            foreach (string candidate in candidates)
            {
                if (!File.Exists(candidate))
                {
                    continue;
                }

                resolvedPath = Path.GetFullPath(candidate);
                return AssemblyLoadContext.Default.LoadFromAssemblyPath(
                    resolvedPath);
            }
        }

        try
        {
            Assembly assembly =
                Assembly.Load("Optimizer");

            resolvedPath =
                string.IsNullOrWhiteSpace(assembly.Location)
                    ? "assembly probing"
                    : assembly.Location;

            return assembly;
        }
        catch
        {
            return null;
        }
    }
}
