using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Pinned public ULSAlgorithms v1.1.0 heuristic inventory.
/// </summary>
public static class UlsAlgorithmsHeuristicCatalog
{
    private static readonly string[] PinnedIds =
    [
        "chiu-modified-least-unit-cost",
        "chiu-ting-modified-part-period-balancing",
        "freeland-colley",
        "groff",
        "ho-chang-solis-improved-net-least-period-cost",
        "ho-chang-solis-net-least-period-cost",
        "karni-maximum-part-period-gain",
        "least-unit-cost",
        "lot-for-lot",
        "mclaren-order-moment",
        "part-period-balancing",
        "part-period-simplified",
        "patterson-laforge-incremental-part-period",
        "periodic-order-quantity",
        "segerstedt-reformulated-silver-meal",
        "silver-meal",
        "wemmerlov-modified-ppb",
        "wemmerlov-modified-ppb-lalb",
        "wemmerlov-ppb-lalb"
    ];

    public static IReadOnlyList<string> SolverIds =>
        PinnedIds;

    public static IReadOnlyList<UlsSolverDescriptor>
        GetPinnedDescriptors()
    {
        UlsSolverDescriptor[] actual =
            UlsSolverCatalog.Heuristics
                .OrderBy(
                    descriptor =>
                        descriptor.Id,
                    StringComparer.Ordinal)
                .ToArray();

        string[] actualIds =
            actual
                .Select(
                    descriptor =>
                        descriptor.Id)
                .ToArray();

        if (!actualIds.SequenceEqual(
                PinnedIds,
                StringComparer.Ordinal))
        {
            throw new InvalidOperationException(
                "The ULSAlgorithms v1.1.0 heuristic catalog does not match the alpha.35 pinned inventory.");
        }

        foreach (UlsSolverDescriptor descriptor
                 in actual)
        {
            EnsureHeuristicDescriptor(
                descriptor);
        }

        return actual;
    }

    public static UlsSolverDescriptor GetRequired(
        string solverId)
    {
        if (string.IsNullOrWhiteSpace(
                solverId))
        {
            throw new ArgumentException(
                "A ULSAlgorithms heuristic solver ID is required.",
                nameof(solverId));
        }

        string normalized =
            solverId.Trim();

        if (!PinnedIds.Contains(
                normalized,
                StringComparer.Ordinal))
        {
            throw new ArgumentOutOfRangeException(
                nameof(solverId),
                solverId,
                "The requested method is not one of the alpha.35 pinned ULSAlgorithms heuristics.");
        }

        UlsSolverDescriptor descriptor =
            UlsSolverCatalog.Get(
                normalized);

        EnsureHeuristicDescriptor(
            descriptor);

        return descriptor;
    }

    private static void EnsureHeuristicDescriptor(
        UlsSolverDescriptor descriptor)
    {
        if (descriptor.Kind !=
                UlsSolverKind.Heuristic ||
            descriptor.Category !=
                UlsSolverCategory.Heuristic)
        {
            throw new InvalidOperationException(
                $"ULSAlgorithms solver '{descriptor.Id}' is not classified as a heuristic.");
        }

        if (descriptor.RequiresExternalSolver)
        {
            throw new InvalidOperationException(
                $"Pinned heuristic '{descriptor.Id}' unexpectedly requires an external optimization solver.");
        }
    }
}
