using LotSizingDataModel.Checker.Campaign;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Results;
using LotSizingDataModel.Solution;

namespace LotSizingDataModel.Checker.Tests.Infrastructure;

internal sealed class ReferenceFixtureData
{
    public required LotSizingInstance Instance { get; init; }
    public required KnownResult KnownResult { get; init; }
    public required LotSizingSolution Solution { get; init; }
}

internal static class ReferenceFixture
{
    private const string FixtureFileName =
        "DJ_Petit_5items_12periodes_Serial_ID45_ph1in45st1de6mh1ms0.xml";

    public static string GetPath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "Fixtures",
            FixtureFileName);
    }

    public static ReferenceFixtureData Load()
    {
        string path =
            GetPath();

        var reader =
            new LotSizingInstanceXmlFileReader();

        LotSizingInstance instance =
            reader.Read(path);

        KnownResult knownResult =
            instance.KnownResults
                .Single(result => result.DetailedSolution is not null);

        LotSizingSolution solution =
            knownResult.DetailedSolution ??
            throw new InvalidOperationException(
                "The reference fixture contains no detailed solution.");

        return new ReferenceFixtureData
        {
            Instance = instance,
            KnownResult = knownResult,
            Solution = solution
        };
    }
}
