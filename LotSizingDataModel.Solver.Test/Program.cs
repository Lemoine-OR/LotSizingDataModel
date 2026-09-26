using System;
using LotSizingDataModel.Solver.Cplex;

if (args.Contains("--xpress-smoke", StringComparer.Ordinal))
{
    var available = await new LotSizingDataModel.Solver.Xpress.XpressSolverAdapter().CheckAvailabilityAsync();
    Console.WriteLine($"Xpress {available.SolverVersion}: usable={available.IsUsable}");
    if (!available.IsUsable) throw new InvalidOperationException(string.Join("; ", available.Diagnostics));
    await TransportAssignmentSmokeTests.RunAsync(useXpress: true);
    await XpressNativeSmokeTests.RunAsync();
    return;
}

if (args.Contains("--transport-smoke", StringComparer.Ordinal))
{
    await TransportAssignmentSmokeTests.RunAsync();
    return;
}

CplexNativeSmokeTestResult result =
    CplexNativeSmokeTest.Run();

Console.WriteLine("CPLEX smoke test");
Console.WriteLine("================");
Console.WriteLine();

Console.WriteLine(
    $"Success : {result.IsSuccessful}");

Console.WriteLine(
    $"Version : {result.SolverVersion}");

Console.WriteLine(
    $"Status  : {result.Status}");

Console.WriteLine(
    $"Objective : {result.ObjectiveValue}");

Console.WriteLine(
    $"x         : {result.VariableValue}");

Console.WriteLine();
Console.WriteLine("Diagnostic:");
Console.WriteLine(
    result.Diagnostic);

Console.WriteLine();
Console.WriteLine(
    "Press Enter to exit.");

Console.ReadLine();
