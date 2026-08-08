using System;
using LotSizingDataModel.Solver.Cplex;

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