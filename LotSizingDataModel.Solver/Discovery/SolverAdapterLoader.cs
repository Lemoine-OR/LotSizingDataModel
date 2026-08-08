using System;
using System.IO;
using System.Reflection;
using System.Runtime.Loader;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Contracts;

namespace LotSizingDataModel.Solver.Discovery;

/// <summary>
/// Loads solver adapter plugins from managed assemblies.
/// </summary>
/// <remarks>
/// Adapter-loading failures are converted into
/// <see cref="SolverAdapterLoadResult"/> instances so that a
/// missing or incompatible solver plugin does not prevent other
/// adapters from being discovered and used.
/// </remarks>
public sealed class SolverAdapterLoader
{
    /// <summary>
    /// Initializes a new solver-adapter loader.
    /// </summary>
    public SolverAdapterLoader()
    {
    }

    /// <summary>
    /// Loads a solver adapter described by the supplied
    /// descriptor.
    /// </summary>
    /// <param name="descriptor">
    /// Solver-adapter descriptor.
    /// </param>
    /// <returns>
    /// Adapter-loading result.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="descriptor"/> is
    /// <see langword="null"/>.
    /// </exception>
    public SolverAdapterLoadResult Load(
        SolverAdapterDescriptor descriptor)
    {
        ArgumentNullException.ThrowIfNull(
            descriptor);

        var result =
            new SolverAdapterLoadResult
            {
                Descriptor =
                    descriptor,

                AssemblyPath =
                    descriptor.AssemblyPath,

                TypeName =
                    descriptor.TypeName,

                Status =
                    SolverAdapterLoadStatus.Discovered
            };

        try
        {
            descriptor.EnsureValid();

            if (string.IsNullOrWhiteSpace(
                    descriptor.AssemblyPath))
            {
                result.Status =
                    SolverAdapterLoadStatus.AssemblyNotFound;

                result.AddDiagnostic(
                    "The adapter assembly path is empty.");

                return result;
            }

            string assemblyPath =
                Path.GetFullPath(
                    descriptor.AssemblyPath);

            result.AssemblyPath =
                assemblyPath;

            if (!File.Exists(
                    assemblyPath))
            {
                result.Status =
                    SolverAdapterLoadStatus.AssemblyNotFound;

                result.AddDiagnostic(
                    $"The adapter assembly was not found at " +
                    $"'{assemblyPath}'.");

                return result;
            }

            Assembly assembly;

            try
            {
                assembly =
                    AssemblyLoadContext.Default
                        .LoadFromAssemblyPath(
                            assemblyPath);
            }
            catch (Exception exception)
                when (
                    exception is
                        FileLoadException or
                        FileNotFoundException or
                        BadImageFormatException)
            {
                result.Status =
                    SolverAdapterLoadStatus.AssemblyLoadFailure;

                result.SetException(
                    exception);

                result.AddDiagnostic(
                    "The adapter assembly could not be loaded.");

                return result;
            }

            Type? adapterType =
                ResolveAdapterType(
                    assembly,
                    descriptor.TypeName);

            if (adapterType is null)
            {
                result.Status =
                    SolverAdapterLoadStatus.AdapterTypeNotFound;

                result.AddDiagnostic(
                    string.IsNullOrWhiteSpace(
                        descriptor.TypeName)
                        ? "No public concrete solver-adapter " +
                          "type was found in the assembly."
                        : $"Adapter type " +
                          $"'{descriptor.TypeName}' was not " +
                          "found in the assembly.");

                return result;
            }

            result.TypeName =
                adapterType.AssemblyQualifiedName ??
                adapterType.FullName ??
                adapterType.Name;

            if (!typeof(ISolverAdapter).IsAssignableFrom(
                    adapterType))
            {
                result.Status =
                    SolverAdapterLoadStatus.InvalidAdapterType;

                result.AddDiagnostic(
                    $"Type '{adapterType.FullName}' does not " +
                    $"implement {nameof(ISolverAdapter)}.");

                return result;
            }

            if (adapterType.IsAbstract ||
                adapterType.IsInterface)
            {
                result.Status =
                    SolverAdapterLoadStatus.InvalidAdapterType;

                result.AddDiagnostic(
                    $"Type '{adapterType.FullName}' is not a " +
                    "concrete adapter type.");

                return result;
            }

            if (adapterType.GetConstructor(
                    Type.EmptyTypes) is null)
            {
                result.Status =
                    SolverAdapterLoadStatus.InvalidAdapterType;

                result.AddDiagnostic(
                    $"Type '{adapterType.FullName}' does not " +
                    "provide a public parameterless " +
                    "constructor.");

                return result;
            }

            try
            {
                result.Adapter =
                    (ISolverAdapter?)Activator.CreateInstance(
                        adapterType);
            }
            catch (Exception exception)
                when (
                    exception is
                        TargetInvocationException or
                        MemberAccessException or
                        MissingMethodException or
                        TypeInitializationException)
            {
                result.Status =
                    IsNativeDependencyFailure(
                        exception)
                        ? SolverAdapterLoadStatus
                            .NativeDependencyFailure
                        : SolverAdapterLoadStatus
                            .InstantiationFailure;

                result.SetException(
                    GetInnermostException(
                        exception));

                result.AddDiagnostic(
                    "The solver adapter could not be " +
                    "instantiated.");

                return result;
            }

            if (result.Adapter is null)
            {
                result.Status =
                    SolverAdapterLoadStatus
                        .InstantiationFailure;

                result.AddDiagnostic(
                    "The adapter constructor returned no " +
                    "instance.");

                return result;
            }

            if (result.Adapter.SolverKind !=
                descriptor.SolverKind)
            {
                result.Status =
                    SolverAdapterLoadStatus.InvalidAdapterType;

                result.AddDiagnostic(
                    $"The adapter reports solver kind " +
                    $"'{result.Adapter.SolverKind}', whereas " +
                    $"the descriptor declares " +
                    $"'{descriptor.SolverKind}'.");

                result.Adapter =
                    null;

                return result;
            }

            result.Status =
                SolverAdapterLoadStatus.Loaded;

            result.AddDiagnostic(
                $"Adapter '{result.Adapter.AdapterName}' was " +
                "loaded successfully.");

            return result;
        }
        catch (Exception exception)
        {
            result.Status =
                IsNativeDependencyFailure(
                    exception)
                    ? SolverAdapterLoadStatus
                        .NativeDependencyFailure
                    : SolverAdapterLoadStatus
                        .AssemblyLoadFailure;

            result.SetException(
                GetInnermostException(
                    exception));

            result.AddDiagnostic(
                "An unexpected error occurred while loading " +
                "the solver adapter.");

            return result;
        }
    }

    private static Type? ResolveAdapterType(
        Assembly assembly,
        string typeName)
    {
        if (!string.IsNullOrWhiteSpace(
                typeName))
        {
            return assembly.GetType(
                typeName.Trim(),
                throwOnError:
                    false,
                ignoreCase:
                    false);
        }

        try
        {
            return Array.Find(
                assembly.GetExportedTypes(),
                type =>
                    typeof(ISolverAdapter)
                        .IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface);
        }
        catch (ReflectionTypeLoadException exception)
        {
            return Array.Find(
                exception.Types,
                type =>
                    type is not null &&
                    typeof(ISolverAdapter)
                        .IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface);
        }
    }

    private static bool IsNativeDependencyFailure(
        Exception exception)
    {
        Exception innermostException =
            GetInnermostException(
                exception);

        return innermostException is
            DllNotFoundException or
            BadImageFormatException or
            FileNotFoundException;
    }

    private static Exception GetInnermostException(
        Exception exception)
    {
        Exception currentException =
            exception;

        while (currentException.InnerException is not null)
        {
            currentException =
                currentException.InnerException;
        }

        return currentException;
    }
}
