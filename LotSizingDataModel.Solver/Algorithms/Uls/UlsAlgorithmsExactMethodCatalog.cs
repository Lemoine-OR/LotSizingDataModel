using ULSAlgorithms.Abstractions;
using ULSAlgorithms.Catalog;

namespace LotSizingDataModel.Solver.Algorithms.Uls;

/// <summary>
/// Pins the external exact-method identities used by alpha.34.
/// </summary>
public static class UlsAlgorithmsExactMethodCatalog
{
    public const string PackageVersion = "1.1.0";

    public const string SourceCommit =
        "3e5595996d35373dd93f90a9245c6bc9e65f9e0d";

    public const string PackageSha256 =
        "4eff21da87a9ff7649ee2e0f1c6f835b0af3cfa6bc08dd9ba91306fdd5ff2a3c";

    public const string WagnerWhitinClassicalId =
        "wagner-whitin-classical";

    public const string ZangwillNetworkId =
        "zangwill-network";

    public static string GetSolverId(
        UlsAlgorithmsExactMethod method)
    {
        return method switch
        {
            UlsAlgorithmsExactMethod.WagnerWhitinClassical =>
                WagnerWhitinClassicalId,

            UlsAlgorithmsExactMethod.ZangwillNetwork =>
                ZangwillNetworkId,

            _ =>
                throw new ArgumentOutOfRangeException(
                    nameof(method),
                    method,
                    "The requested ULSAlgorithms exact method is not exposed by alpha.34.")
        };
    }

    public static UlsSolverDescriptor GetExactDescriptor(
        UlsAlgorithmsExactMethod method)
    {
        string solverId =
            GetSolverId(
                method);

        UlsSolverDescriptor descriptor =
            UlsSolverCatalog.Get(
                solverId);

        if (descriptor.Kind !=
            UlsSolverKind.Exact)
        {
            throw new InvalidOperationException(
                $"ULSAlgorithms solver '{solverId}' is not classified as an exact solver.");
        }

        return descriptor;
    }
}
