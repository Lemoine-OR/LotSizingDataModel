using System.Text;
using LotSizingDataModel.Checker.Common;
using LotSizingDataModel.Checker.Configuration;
using LotSizingDataModel.Checker.Facade;
using LotSizingDataModel.Checker.Reporting;
using LotSizingDataModel.Checker.Results;
using LotSizingDataModel.Instance;
using LotSizingDataModel.Instance.Common;
using LotSizingDataModel.Instance.Creation;
using LotSizingDataModel.Instance.Results;
using LotSizingDataModel.Instance.Serialization;
using LotSizingDataModel.Solution.Common;
using LotSizingDataModel.Solver.Common;
using LotSizingDataModel.Solver.Configuration;
using LotSizingDataModel.Solver.ConsoleApp;
using LotSizingDataModel.Solver.Discovery;
using LotSizingDataModel.Solver.Execution;
using LotSizingDataModel.Solver.Formulation;
using LotSizingDataModel.Solver.Modeling;

Console.OutputEncoding =
    Encoding.UTF8;

const string DefaultInputDirectory =
    @"C:\Users\david\Documents\test\NouveauFormat\Petit";

SolverKind preferredSolver =
    ParsePreferredSolver(args);

string solverDisplayName =
    GetSolverDisplayName(preferredSolver);

string inputDirectory =
    GetOptionValue(args, "--input") ??
    DefaultInputDirectory;

string resolvedDirectoryName =
    preferredSolver == SolverKind.Cplex
        ? "résolu"
        : $"résolu-{GetSolverSlug(preferredSolver)}";

string resolvedDirectory =
    Path.Combine(
        inputDirectory,
        resolvedDirectoryName);

Console.WriteLine(
    "LotSizingDataModel - multi-solver batch + independent checker");
Console.WriteLine(
    "============================================================");
Console.WriteLine();
Console.WriteLine(
    $"Solveur demandé      : {solverDisplayName}");
Console.WriteLine(
    $"Répertoire d'entrée : {inputDirectory}");
Console.WriteLine(
    $"Répertoire résolu    : {resolvedDirectory}");
Console.WriteLine();

if (!Directory.Exists(
        inputDirectory))
{
    Console.Error.WriteLine(
        "ERREUR : le répertoire d'entrée n'existe pas.");

    Environment.ExitCode =
        1;

    return;
}

Directory.CreateDirectory(
    resolvedDirectory);

/*
 * Only XML files located directly in Petit are selected.
 * Files located in Petit\\résolu are therefore never processed.
 */
string[] inputFiles =
    Directory
        .EnumerateFiles(
            inputDirectory,
            "*.xml",
            SearchOption.TopDirectoryOnly)
        .OrderBy(
            path =>
                Path.GetFileName(path),
            StringComparer.OrdinalIgnoreCase)
        .ToArray();

if (inputFiles.Length == 0)
{
    Console.WriteLine(
        "Aucune instance XML à traiter.");

    return;
}

Console.WriteLine(
    $"{inputFiles.Length} instance(s) à traiter.");
Console.WriteLine();

var formulationOptions =
    new StandardLotSizingFormulationOptions();

var discoveryOptions =
    new SolverDiscoveryOptions();

var verificationService =
    new LotSizingSolutionVerificationService();

var reportFormatter =
    new SolutionCheckTextReportFormatter();

var verificationOptions =
    new SolutionVerificationOptions
    {
        CheckOptions =
            new SolutionCheckOptions
            {
                Level =
                    SolutionCheckLevel.Full
            },
        ApplyToSolutionEvaluation =
            true,
        UpdateKnownResultFeasibility =
            true,
        PromoteFullyVerifiedKnownResult =
            true,
        EvaluatorName =
            "LotSizingSolutionChecker",
        EvaluatorVersion =
            "1.1"
    };

var reportOptions =
    new SolutionCheckReportOptions
    {
        IncludeStageDetails =
            true,
        IncludeFeasibilityMetrics =
            true,
        IncludeObjectiveMetrics =
            true,
        IncludeInformationIssues =
            true,
        IncludeWarningIssues =
            true,
        IncludeErrorIssues =
            true,
        MaximumDetailedIssues =
            100,
        SortIssuesBySeverity =
            true
    };

Console.WriteLine(
    $"Initialisation du solveur {solverDisplayName}...");

StandardLotSizingSolverBootstrapResult bootstrap;

try
{
    bootstrap =
        await StandardLotSizingSolverBootstrapper.InitializeAsync(
            formulationOptions,
            discoveryOptions);
}
catch (System.Exception exception)
{
    Console.Error.WriteLine(
        "ERREUR : échec de l'initialisation du solveur.");

    Console.Error.WriteLine(
        exception);

    Environment.ExitCode =
        2;

    return;
}

if (!bootstrap.IsSuccessful ||
    bootstrap.SolverService is null)
{
    Console.Error.WriteLine(
        "ERREUR : l'infrastructure de résolution " +
        "n'a pas pu être initialisée.");

    Environment.ExitCode =
        3;

    return;
}

if (!bootstrap.CanSolve)
{
    Console.Error.WriteLine(
        "ERREUR : aucun solveur utilisable n'a été détecté.");

    Environment.ExitCode =
        4;

    return;
}

Console.WriteLine(
    $"Infrastructure prête pour {solverDisplayName}.");
Console.WriteLine(
    "Checker indépendant prêt.");
Console.WriteLine();

int succeeded =
    0;

int checkerRejected =
    0;

int failed =
    0;

var rejectedFiles =
    new List<string>();

var failedFiles =
    new List<string>();

for (int fileIndex = 0;
     fileIndex < inputFiles.Length;
     fileIndex++)
{
    string inputPath =
        inputFiles[fileIndex];

    string inputFileName =
        Path.GetFileName(
            inputPath);

    string instanceStem =
        Path.GetFileNameWithoutExtension(
            inputFileName);

    string outputPath =
        Path.Combine(
            resolvedDirectory,
            inputFileName);

    string mathematicalModelPath =
        Path.Combine(
            resolvedDirectory,
            $"{instanceStem}.mathematical-model.txt");

    string solutionCheckReportPath =
        Path.Combine(
            resolvedDirectory,
            $"{instanceStem}.solution-check.txt");

    Console.WriteLine(
        new string(
            '=',
            78));

    Console.WriteLine(
        $"INSTANCE {fileIndex + 1}/{inputFiles.Length}");

    Console.WriteLine(
        inputFileName);

    Console.WriteLine(
        new string(
            '-',
            78));

    try
    {
        DeleteIfExists(outputPath);
        DeleteIfExists(solutionCheckReportPath);
        DeleteIfExists(mathematicalModelPath);

        Console.WriteLine(
            "1/7 - Lecture de l'instance...");

        LotSizingInstance instance =
            LotSizingInstanceXmlSerializer.DeserializeFromFile(
                inputPath);

        Console.WriteLine(
            $"      OK - {instance.Name} " +
            $"({instance.PlanningHorizon} périodes)");

        Console.WriteLine(
            "2/7 - Construction du modèle mathématique de diagnostic...");

        MathematicalModelFormulationRegistry formulationRegistry =
            StandardLotSizingFormulationRegistryFactory.Create(
                formulationOptions);

        var modelBuildOptions =
            new MathematicalModelBuildOptions
            {
                RequestedFormulationId =
                    StandardLotSizingFormulation.StandardFormulationId,

                AllowFallback =
                    false,

                ValidateGeneratedModel =
                    true,

                CloneGeneratedModel =
                    false
            };

        var modelBuildService =
            new MathematicalModelBuildService();

        MathematicalModelBuildResult modelBuildResult =
            await modelBuildService.BuildAsync(
                instance,
                formulationRegistry,
                modelBuildOptions);

        if (!modelBuildResult.IsSuccessful ||
            modelBuildResult.Model is null)
        {
            throw new InvalidOperationException(
                "Impossible de construire le modèle mathématique. " +
                string.Join(
                    " | ",
                    modelBuildResult.Diagnostics));
        }

        MathematicalModel mathematicalModel =
            modelBuildResult.Model;

        MathematicalModelTextExporter.Write(
            mathematicalModel,
            mathematicalModelPath);

        Console.WriteLine(
            $"      OK - {mathematicalModel.VariableCount} variables, " +
            $"{mathematicalModel.EnabledConstraintCount} contraintes.");

        Console.WriteLine(
            $"      Dump : {mathematicalModelPath}");

        Console.WriteLine(
            $"3/7 - Solveur demandé : {solverDisplayName}.");

        Console.WriteLine(
            "4/7 - Résolution...");

        var request =
            new SolverRequest(
                instance)
            {
                PreferredSolver =
                    preferredSolver,

                FormulationName =
                    StandardLotSizingFormulation.StandardFormulationId,

                RunName =
                    $"{solverDisplayName} - {instanceStem}",

                Parameters =
                    new SolverParameters()
            };

        SolverRunResult runResult =
            await bootstrap.SolverService.SolveAsync(
                request);

        Console.WriteLine(
            $"      Solveur  : " +
            $"{runResult.SolverName} {runResult.SolverVersion}");

        Console.WriteLine(
            $"      Statut   : {runResult.TerminationReason}");

        Console.WriteLine(
            $"      Durée    : {runResult.ElapsedSeconds:F3} s");

        if (runResult.ObjectiveValue.HasValue)
        {
            Console.WriteLine(
                $"      Objectif solveur   : " +
                $"{runResult.ObjectiveValue.Value:G17}");
        }

        if (runResult.RecomputedObjectiveValue.HasValue)
        {
            Console.WriteLine(
                $"      Objectif recalculé par Solver : " +
                $"{runResult.RecomputedObjectiveValue.Value:G17}");

            Console.WriteLine(
                $"      Vérification interne Solver   : " +
                $"{runResult.ObjectiveVerificationStatus}");

            if (runResult.ObjectiveDifference.HasValue)
            {
                Console.WriteLine(
                    $"      Écart interne Solver          : " +
                    $"{runResult.ObjectiveDifference.Value:G17}");
            }
        }

        if (!runResult.HasSolution ||
            runResult.Solution is null)
        {
            string diagnostics =
                runResult.Diagnostics.Count == 0
                    ? "Aucun diagnostic supplémentaire."
                    : string.Join(
                        " | ",
                        runResult.Diagnostics);

            throw new InvalidOperationException(
                "Aucune solution exploitable n'a été produite. " +
                diagnostics);
        }

        Console.WriteLine(
            "5/7 - Préparation du résultat candidat...");

        string resultId =
            $"solver-{runResult.RunId}";

        string fingerprint =
            LotSizingInstanceFactory.ComputeSupplyChainFingerprint(
                instance.SupplyChain);

        var knownResult =
            new KnownResult(
                resultId)
            {
                Name =
                    $"{runResult.SolverName} solution",

                /*
                 * Keep the raw objective reported by the native solver.
                 * VerifyKnownResultAsync uses this value as the external
                 * reference and recomputes the objective independently from
                 * the mapped detailed solution.
                 */
                ReportedObjectiveValue =
                    runResult.ObjectiveValue,

                ObjectiveName =
                    "TotalCost",

                OptimalityStatus =
                    runResult.TerminationReason ==
                        SolverTerminationReason.Optimal
                        ? OptimalityStatus.ProvenOptimal
                        : OptimalityStatus.NoProof,

                DetailedSolution =
                    runResult.Solution,

                MethodName =
                    string.IsNullOrWhiteSpace(
                        runResult.SolverVersion)
                        ? runResult.SolverName
                        : $"{runResult.SolverName} " +
                          $"{runResult.SolverVersion}",

                ObtainedAtUtc =
                    runResult.CompletedAtUtc ??
                    DateTime.UtcNow,

                RecordedAtUtc =
                    DateTime.UtcNow,

                SupplyChainFingerprint =
                    fingerprint,

                Comment =
                    $"Termination reason: " +
                    $"{runResult.TerminationReason}. " +
                    $"Elapsed time: " +
                    $"{runResult.ElapsedSeconds:F6} s."
            };

        Console.WriteLine(
            $"      OK - candidat {knownResult.ResultId} préparé.");

        Console.WriteLine(
            "6/7 - Vérification indépendante de la solution...");

        LotSizingSolutionVerificationResult verification =
            await verificationService.VerifyKnownResultAsync(
                instance,
                knownResult,
                mathematicalModel,
                verificationOptions);

        SolutionCheckResult checkResult =
            verification.CheckResult;

        string checkReport =
            reportFormatter.Format(
                checkResult,
                candidateName:
                    $"{inputFileName} / {knownResult.ResultId}",
                options:
                    reportOptions);

        await File.WriteAllTextAsync(
            solutionCheckReportPath,
            checkReport,
            new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: false));

        DisplayCheckerSummary(
            checkResult,
            solutionCheckReportPath);

        if (!verification.IsValid)
        {
            checkerRejected++;

            rejectedFiles.Add(
                inputFileName);

            Console.Error.WriteLine(
                "      REJET - la solution ne satisfait pas le checker indépendant.");

            Console.Error.WriteLine(
                "      L'instance n'est pas écrite dans le répertoire résolu " +
                "et ce résultat n'est pas enregistré comme KnownResult.");

            Console.WriteLine();

            continue;
        }

        instance.AddKnownResult(
            knownResult);

        if (runResult.TerminationReason ==
                SolverTerminationReason.Optimal ||
            !instance.HasBestKnownResultId)
        {
            instance.SetBestKnownResult(
                knownResult);
        }

        Console.WriteLine(
            "      ACCEPTÉ - résultat vérifié et ajouté à l'instance.");

        Console.WriteLine(
            "7/7 - Écriture de l'instance résolue...");

        LotSizingInstanceXmlSerializer.SerializeToFile(
            instance,
            outputPath,
            validateBeforeSerialization: true,
            validateCurrentFingerprint: true,
            indent: true);

        Console.WriteLine(
            $"      OK - {outputPath}");

        succeeded++;
    }
    catch (System.Exception exception)
    {
        failed++;

        failedFiles.Add(
            inputFileName);

        Console.Error.WriteLine();
        Console.Error.WriteLine(
            $"ÉCHEC TECHNIQUE pour {inputFileName}");

        Console.Error.WriteLine(
            exception.Message);

        /*
         * The batch deliberately continues with the next instance.
         * One invalid or unsupported instance must not stop all
         * remaining resolutions.
         */
    }

    Console.WriteLine();
}

Console.WriteLine(
    new string(
        '=',
        78));

Console.WriteLine(
    "BILAN DU TRAITEMENT");

Console.WriteLine(
    new string(
        '=',
        78));

Console.WriteLine(
    $"Instances détectées       : {inputFiles.Length}");

Console.WriteLine(
    $"Résolues et vérifiées     : {succeeded}");

Console.WriteLine(
    $"Rejetées par le checker   : {checkerRejected}");

Console.WriteLine(
    $"Échecs techniques         : {failed}");

Console.WriteLine(
    $"Résultats / rapports      : {resolvedDirectory}");

if (rejectedFiles.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine(
        "Solutions rejetées par le checker :");

    foreach (
        string rejectedFile
        in rejectedFiles)
    {
        Console.WriteLine(
            $"  - {rejectedFile}");
    }
}

if (failedFiles.Count > 0)
{
    Console.WriteLine();
    Console.WriteLine(
        "Instances en échec technique :");

    foreach (
        string failedFile
        in failedFiles)
    {
        Console.WriteLine(
            $"  - {failedFile}");
    }
}

Environment.ExitCode =
    checkerRejected == 0 &&
    failed == 0
        ? 0
        : 10;

static SolverKind ParsePreferredSolver(
    string[] arguments)
{
    string? value =
        GetOptionValue(
            arguments,
            "--solver");

    if (string.IsNullOrWhiteSpace(value))
    {
        return SolverKind.Cplex;
    }

    return value.Trim().ToLowerInvariant() switch
    {
        "cplex" => SolverKind.Cplex,
        "gurobi" => SolverKind.Gurobi,
        "xpress" => SolverKind.Xpress,
        "xpressmp" => SolverKind.Xpress,
        "cbc" => SolverKind.CoinOrCbc,
        "coinorcbc" => SolverKind.CoinOrCbc,
        "coin-or-cbc" => SolverKind.CoinOrCbc,
        _ => throw new ArgumentException(
            $"Solveur inconnu '{value}'. Valeurs admises : " +
            "cplex, gurobi, xpress, cbc.")
    };
}

static string? GetOptionValue(
    string[] arguments,
    string optionName)
{
    for (int index = 0;
         index < arguments.Length;
         index++)
    {
        string argument =
            arguments[index];

        if (string.Equals(
                argument,
                optionName,
                StringComparison.OrdinalIgnoreCase))
        {
            if (index + 1 >= arguments.Length)
            {
                throw new ArgumentException(
                    $"L'option '{optionName}' attend une valeur.");
            }

            return arguments[index + 1];
        }

        string prefix =
            optionName + "=";

        if (argument.StartsWith(
                prefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return argument[prefix.Length..];
        }
    }

    return null;
}

static string GetSolverDisplayName(
    SolverKind solverKind)
{
    return solverKind switch
    {
        SolverKind.Cplex => "IBM ILOG CPLEX",
        SolverKind.Gurobi => "Gurobi Optimizer",
        SolverKind.Xpress => "FICO Xpress MP",
        SolverKind.CoinOrCbc => "COIN-OR CBC",
        _ => solverKind.ToString()
    };
}

static string GetSolverSlug(
    SolverKind solverKind)
{
    return solverKind switch
    {
        SolverKind.Cplex => "cplex",
        SolverKind.Gurobi => "gurobi",
        SolverKind.Xpress => "xpress",
        SolverKind.CoinOrCbc => "cbc",
        _ => solverKind.ToString().ToLowerInvariant()
    };
}

static void DisplayCheckerSummary(
    SolutionCheckResult result,
    string reportPath)
{
    Console.WriteLine(
        $"      Checker   : {(result.IsValid ? "VALID" : "INVALID")}");

    Console.WriteLine(
        $"      Structure : {FormatStage(result.StructuralCheckCompleted, result.IsStructurallyValid)}");

    Console.WriteLine(
        $"      Domaines  : {FormatStage(result.VariableDomainCheckCompleted, result.AreVariableDomainsValid)}");

    Console.WriteLine(
        $"      Faisable  : {FormatStage(result.FeasibilityCheckCompleted, result.IsFeasible)}");

    Console.WriteLine(
        $"      Objectif  : {FormatStage(result.ObjectiveCheckCompleted, result.IsObjectiveConsistent)}");

    if (result.FeasibilityCheckCompleted)
    {
        Console.WriteLine(
            $"      Contraintes violées : {result.ViolatedConstraintCount}");

        Console.WriteLine(
            $"      Violation maximale  : {result.MaximumConstraintViolation:G17}");
    }

    if (result.ReportedObjectiveValue.HasValue)
    {
        Console.WriteLine(
            $"      Objectif reporté solveur : {result.ReportedObjectiveValue.Value:G17}");
    }

    if (result.RecomputedObjectiveValue.HasValue)
    {
        Console.WriteLine(
            $"      Objectif checker        : {result.RecomputedObjectiveValue.Value:G17}");
    }

    if (result.ObjectiveDifference.HasValue)
    {
        Console.WriteLine(
            $"      Écart objectif          : {result.ObjectiveDifference.Value:G17}");
    }

    if (result.ObjectiveComparisonTolerance.HasValue)
    {
        Console.WriteLine(
            $"      Tolérance comparaison   : {result.ObjectiveComparisonTolerance.Value:G17}");
    }

    int errorCount =
        result.Issues.Count(
            issue =>
                issue.Severity ==
                    SolutionCheckSeverity.Error);

    int warningCount =
        result.Issues.Count(
            issue =>
                issue.Severity ==
                    SolutionCheckSeverity.Warning);

    Console.WriteLine(
        $"      Diagnostics : {errorCount} erreur(s), {warningCount} avertissement(s)");

    Console.WriteLine(
        $"      Rapport     : {reportPath}");
}

static string FormatStage(
    bool completed,
    bool passed)
{
    if (!completed)
    {
        return "NON EXÉCUTÉ";
    }

    return passed
        ? "OK"
        : "ÉCHEC";
}


static void DeleteIfExists(
    string path)
{
    if (File.Exists(path))
    {
        File.Delete(path);
    }
}
