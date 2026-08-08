using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using LotSizingDataModel.Import.Common;
using LotSizingDataModel.Import.DellaertJeunet;
using LotSizingDataModel.Import.DellaertJeunet.XmlModel;
using LotSizingDataModel.Instance;

namespace LotSizingDataModel.BatchConverter;

/// <summary>
/// Converts a directory of Dellaert–Jeunet XML instances into
/// the LotSizingDataModel XML format.
/// </summary>
internal static class Program
{
    private const string SourceDirectory =
        @"C:\Users\david\Documents\test";

    private const string OutputDirectoryName =
        "NouveauFormat";

    private const string SmallDirectoryName =
        "Petit";

    private const string MediumDirectoryName =
        "Moyen";

    private const string LargeDirectoryName =
        "Grand";

    private static readonly DellaertJeunetXmlReader
        SourceReader =
            new();

    private static readonly DellaertJeunetInstanceImporter
        Importer =
            new();

    /// <summary>
    /// Runs the batch conversion.
    /// </summary>
    /// <returns>
    /// Zero when all files are converted successfully;
    /// otherwise, one.
    /// </returns>
    public static int Main()
    {
        Console.OutputEncoding =
            Encoding.UTF8;

        Console.WriteLine(
            "Conversion des instances Dellaert–Jeunet");

        Console.WriteLine(
            new string(
                '=',
                60));

        if (!Directory.Exists(
                SourceDirectory))
        {
            Console.Error.WriteLine(
                $"Le répertoire source n'existe pas : " +
                $"{SourceDirectory}");

            return 1;
        }

        string outputRootDirectory =
            Path.Combine(
                SourceDirectory,
                OutputDirectoryName);

        CreateOutputDirectories(
            outputRootDirectory);

        string[] sourceFiles =
            FindSourceFiles(
                SourceDirectory,
                outputRootDirectory);

        if (sourceFiles.Length == 0)
        {
            Console.WriteLine(
                "Aucun fichier XML n'a été trouvé.");

            return 0;
        }

        Console.WriteLine(
            $"{sourceFiles.Length} fichier(s) XML trouvé(s).");

        Console.WriteLine();

        int successCount =
            0;

        int failureCount =
            0;

        int skippedCount =
            0;

        foreach (string sourceFile in sourceFiles)
        {
            ConversionStatus status =
                ConvertFile(
                    sourceFile,
                    outputRootDirectory);

            switch (status)
            {
                case ConversionStatus.Success:
                    successCount++;
                    break;

                case ConversionStatus.Skipped:
                    skippedCount++;
                    break;

                default:
                    failureCount++;
                    break;
            }
        }

        Console.WriteLine();

        Console.WriteLine(
            new string(
                '=',
                60));

        Console.WriteLine(
            $"Conversions réussies : {successCount}");

        Console.WriteLine(
            $"Fichiers ignorés     : {skippedCount}");

        Console.WriteLine(
            $"Échecs               : {failureCount}");

        Console.WriteLine(
            $"Répertoire de sortie : {outputRootDirectory}");

        return
            failureCount == 0
                ? 0
                : 1;
    }

    private static ConversionStatus ConvertFile(
        string sourceFile,
        string outputRootDirectory)
    {
        Console.WriteLine(
            $"Traitement : {Path.GetFileName(sourceFile)}");

        try
        {
            if (!Importer.CanImport(
                    sourceFile))
            {
                Console.WriteLine(
                    "  → Ignoré : format Dellaert–Jeunet " +
                    "non reconnu.");

                return ConversionStatus.Skipped;
            }

            DellaertJeunetXmlInstance sourceMetadata =
                SourceReader.Read(
                    sourceFile);

            string sizeDirectoryName =
                DetermineSizeDirectory(
                    sourceMetadata);

            string destinationDirectory =
                Path.Combine(
                    outputRootDirectory,
                    sizeDirectoryName);

            var options =
                new DellaertJeunetImportOptions
                {
                    ValidateSourceData =
                        true,

                    ValidateImportedInstance =
                        true,

                    AnalyzeProductStructure =
                        true,

                    ClassifyProblem =
                        true,

                    GenerateMethodRecommendations =
                        false,

                    ConvertEmptyDemandToZeroSeries =
                        true,

                    PreserveBibliographicMetadata =
                        true,

                    PreserveSourceIdentifiers =
                        true,

                    IncludeInformationDiagnostics =
                        false,

                    IncludeTechnicalDetails =
                        true,

                    ThrowOnError =
                        false,

                    SourceName =
                        Path.GetFileName(
                            sourceFile)
                };

            InstanceImportResult result =
                Importer.Import(
                    sourceFile,
                    options);

            DisplayDiagnostics(
                result);

            if (result.Instance is null)
            {
                Console.Error.WriteLine(
                    "  → Échec : aucune instance n'a été " +
                    "créée.");

                return ConversionStatus.Failure;
            }

            if (result.HasBlockingDiagnostics)
            {
                Console.Error.WriteLine(
                    "  → Échec : l'import contient des " +
                    "erreurs bloquantes.");

                return ConversionStatus.Failure;
            }

            string outputFileName =
                BuildOutputFileName(
                    sourceMetadata,
                    sizeDirectoryName);

            string outputFilePath =
                BuildUniqueFilePath(
                    destinationDirectory,
                    outputFileName);

            SerializeInstance(
                result.Instance,
                outputFilePath);

            Console.WriteLine(
                $"  → Taille : {sizeDirectoryName}");

            Console.WriteLine(
                $"  → Articles : {sourceMetadata.ItemCount}");

            Console.WriteLine(
                $"  → Périodes : " +
                $"{sourceMetadata.NumberOfPeriods}");

            Console.WriteLine(
                $"  → Sauvegardé : {outputFilePath}");

            Console.WriteLine();

            return ConversionStatus.Success;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(
                "  → Erreur inattendue pendant la conversion.");

            DisplayException(
                exception);

            Console.Error.WriteLine();

            return ConversionStatus.Failure;
        }
    }

    private static string[] FindSourceFiles(
        string sourceDirectory,
        string outputRootDirectory)
    {
        string normalizedOutputDirectory =
            Path.GetFullPath(
                outputRootDirectory)
            .TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

        return Directory
            .EnumerateFiles(
                sourceDirectory,
                "*.xml",
                SearchOption.AllDirectories)
            .Where(
                filePath =>
                    !IsLocatedInsideDirectory(
                        filePath,
                        normalizedOutputDirectory))
            .OrderBy(
                filePath =>
                    filePath,
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsLocatedInsideDirectory(
        string filePath,
        string directoryPath)
    {
        string normalizedFilePath =
            Path.GetFullPath(
                filePath);

        string directoryPrefix =
            directoryPath +
            Path.DirectorySeparatorChar;

        return normalizedFilePath.StartsWith(
            directoryPrefix,
            StringComparison.OrdinalIgnoreCase);
    }

    private static void CreateOutputDirectories(
        string outputRootDirectory)
    {
        Directory.CreateDirectory(
            outputRootDirectory);

        Directory.CreateDirectory(
            Path.Combine(
                outputRootDirectory,
                SmallDirectoryName));

        Directory.CreateDirectory(
            Path.Combine(
                outputRootDirectory,
                MediumDirectoryName));

        Directory.CreateDirectory(
            Path.Combine(
                outputRootDirectory,
                LargeDirectoryName));
    }

    private static string DetermineSizeDirectory(
        DellaertJeunetXmlInstance source)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        if (string.IsNullOrWhiteSpace(
                source.Name))
        {
            throw new InvalidOperationException(
                "The source instance name is required to " +
                "determine its phase and size category.");
        }

        string instanceName =
            Path.GetFileNameWithoutExtension(
                source.Name.Trim());

        Match phaseMatch =
            Regex.Match(
                instanceName,
                @"^ph(?<phase>[123])",
                RegexOptions.IgnoreCase |
                RegexOptions.CultureInvariant);

        if (!phaseMatch.Success)
        {
            throw new InvalidOperationException(
                $"The phase could not be determined from " +
                $"instance name '{source.Name}'. The name " +
                "must start with ph1, ph2, or ph3.");
        }

        int phase =
            int.Parse(
                phaseMatch.Groups["phase"].Value,
                NumberStyles.None,
                CultureInfo.InvariantCulture);

        return phase switch
        {
            1 =>
                SmallDirectoryName,

            2 =>
                MediumDirectoryName,

            3 =>
                LargeDirectoryName,

            _ =>
                throw new InvalidOperationException(
                    $"Unsupported phase '{phase}' in instance " +
                    $"name '{source.Name}'.")
        };
    }

    private static string BuildOutputFileName(
        DellaertJeunetXmlInstance source,
        string sizeDirectoryName)
    {
        string safeSize =
            SanitizeFileNamePart(
                sizeDirectoryName);

        string safeBomType =
            SanitizeFileNamePart(
                string.IsNullOrWhiteSpace(
                    source.BomType)
                    ? "BOM-Inconnu"
                    : source.BomType);

        string safeOriginalName =
            SanitizeFileNamePart(
                string.IsNullOrWhiteSpace(
                    source.Name)
                    ? "SansNom"
                    : source.Name);

        string identifier =
            source.Id.ToString(
                CultureInfo.InvariantCulture);

        string itemCount =
            source.ItemCount.ToString(
                CultureInfo.InvariantCulture);

        string periodCount =
            source.NumberOfPeriods.ToString(
                CultureInfo.InvariantCulture);

        return
            $"DJ_{safeSize}_" +
            $"{itemCount}items_" +
            $"{periodCount}periodes_" +
            $"{safeBomType}_" +
            $"ID{identifier}_" +
            $"{safeOriginalName}.xml";
    }

    private static string SanitizeFileNamePart(
        string value)
    {
        if (string.IsNullOrWhiteSpace(
                value))
        {
            return "Inconnu";
        }

        char[] invalidCharacters =
            Path.GetInvalidFileNameChars();

        var builder =
            new StringBuilder();

        bool previousWasSeparator =
            false;

        foreach (char character in value.Trim())
        {
            bool mustReplace =
                invalidCharacters.Contains(
                    character) ||
                char.IsWhiteSpace(
                    character) ||
                character is
                    '–' or
                    '—' or
                    '/' or
                    '\\';

            if (mustReplace)
            {
                if (!previousWasSeparator)
                {
                    builder.Append(
                        '-');

                    previousWasSeparator =
                        true;
                }

                continue;
            }

            builder.Append(
                character);

            previousWasSeparator =
                false;
        }

        string result =
            builder
                .ToString()
                .Trim(
                    '-',
                    '_',
                    '.');

        return
            string.IsNullOrWhiteSpace(
                result)
                ? "Inconnu"
                : result;
    }

    private static string BuildUniqueFilePath(
        string destinationDirectory,
        string fileName)
    {
        string initialPath =
            Path.Combine(
                destinationDirectory,
                fileName);

        if (!File.Exists(
                initialPath))
        {
            return initialPath;
        }

        string nameWithoutExtension =
            Path.GetFileNameWithoutExtension(
                fileName);

        string extension =
            Path.GetExtension(
                fileName);

        int copyNumber =
            2;

        while (true)
        {
            string candidateName =
                $"{nameWithoutExtension}_v{copyNumber}" +
                extension;

            string candidatePath =
                Path.Combine(
                    destinationDirectory,
                    candidateName);

            if (!File.Exists(
                    candidatePath))
            {
                return candidatePath;
            }

            copyNumber++;
        }
    }

    private static void SerializeInstance(
        LotSizingInstance instance,
        string outputFilePath)
    {
        ArgumentNullException.ThrowIfNull(
            instance);

        string? parentDirectory =
            Path.GetDirectoryName(
                outputFilePath);

        if (!string.IsNullOrWhiteSpace(
                parentDirectory))
        {
            Directory.CreateDirectory(
                parentDirectory);
        }

        var serializer =
            new XmlSerializer(
                typeof(LotSizingInstance));

        var settings =
            new XmlWriterSettings
            {
                Indent =
                    true,

                IndentChars =
                    "    ",

                Encoding =
                    new UTF8Encoding(
                        encoderShouldEmitUTF8Identifier:
                            false),

                NewLineChars =
                    Environment.NewLine,

                NewLineHandling =
                    NewLineHandling.Replace,

                OmitXmlDeclaration =
                    false
            };

        using XmlWriter writer =
            XmlWriter.Create(
                outputFilePath,
                settings);

        serializer.Serialize(
            writer,
            instance);
    }

    private static void DisplayDiagnostics(
        InstanceImportResult result)
    {
        foreach (ImportDiagnostic diagnostic
                 in result.Diagnostics)
        {
            string prefix =
                diagnostic.Severity switch
                {
                    ImportSeverity.Warning =>
                        "Avertissement",

                    ImportSeverity.Error =>
                        "Erreur",

                    ImportSeverity.Fatal =>
                        "Erreur fatale",

                    _ =>
                        "Information"
                };

            Console.WriteLine(
                $"  [{prefix} {diagnostic.Code}] " +
                $"{diagnostic.Message}");

            if (!string.IsNullOrWhiteSpace(
                    diagnostic.EntityKey))
            {
                Console.WriteLine(
                    $"      Entité : {diagnostic.EntityKey}");
            }

            if (!string.IsNullOrWhiteSpace(
                    diagnostic.SourcePath))
            {
                Console.WriteLine(
                    $"      Source : {diagnostic.SourcePath}");
            }

            if (!string.IsNullOrWhiteSpace(
                    diagnostic.ExceptionType))
            {
                Console.WriteLine(
                    $"      Exception : " +
                    $"{diagnostic.ExceptionType}");
            }

            if (!string.IsNullOrWhiteSpace(
                    diagnostic.TechnicalDetails))
            {
                Console.WriteLine(
                    $"      Détail : " +
                    $"{diagnostic.TechnicalDetails}");
            }
        }
    }

    private enum ConversionStatus
    {
        Success = 0,

        Skipped = 1,

        Failure = 2
    }

    private static void DisplayException(
    Exception exception,
    string indentation = "      ")
    {
        ArgumentNullException.ThrowIfNull(
            exception);

        Exception? currentException =
            exception;

        int level =
            0;

        while (currentException is not null)
        {
            string prefix =
                level == 0
                    ? "Exception"
                    : $"Inner exception {level}";

            Console.Error.WriteLine(
                $"{indentation}{prefix} : " +
                $"{currentException.GetType().FullName}");

            Console.Error.WriteLine(
                $"{indentation}Message : " +
                $"{currentException.Message}");

            currentException =
                currentException.InnerException;

            level++;
        }
    }

}
