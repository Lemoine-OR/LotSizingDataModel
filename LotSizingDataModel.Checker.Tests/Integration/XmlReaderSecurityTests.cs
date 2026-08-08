using LotSizingDataModel.Checker.Campaign;

namespace LotSizingDataModel.Checker.Tests.Integration;

public sealed class XmlReaderSecurityTests
{
    [Fact]
    public void Reader_RejectsDtdDocuments()
    {
        string path =
            Path.Combine(
                Path.GetTempPath(),
                $"checker-dtd-{Guid.NewGuid():N}.xml");

        File.WriteAllText(
            path,
            "<!DOCTYPE lotSizingInstance [<!ENTITY xxe SYSTEM 'file:///etc/passwd'>]>" +
            "<lotSizingInstance>&xxe;</lotSizingInstance>");

        try
        {
            var reader =
                new LotSizingInstanceXmlFileReader();

            Assert.ThrowsAny<Exception>(
                () =>
                    reader.HasLotSizingInstanceRoot(path));
        }
        finally
        {
            File.Delete(path);
        }
    }
}
