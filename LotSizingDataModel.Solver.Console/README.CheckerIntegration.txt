LotSizingDataModel.Solver.Console - integrated solution checking
===============================================================

Purpose
-------
This Program.cs extends the existing CPLEX batch console so that every
candidate solution is independently checked immediately after solving and
before it is persisted as a KnownResult.

Processing pipeline per XML instance
------------------------------------
1. Read the LotSizingInstance.
2. Build the standard MathematicalModel and export the diagnostic dump.
3. Reuse the already initialized CPLEX infrastructure.
4. Solve the instance.
5. Build an in-memory candidate KnownResult. Its ReportedObjectiveValue is
   the raw objective returned by CPLEX.
6. Run LotSizingSolutionChecker at Full level using the MathematicalModel
   already built in step 2. No additional model build is performed solely
   for the checker.
7. Only when the checker returns VALID:
   - add the KnownResult to the instance;
   - update the best-known result when appropriate;
   - serialize the resolved instance to Petit\résolu.

Independent objective check
---------------------------
The solver service already performs its own objective post-processing. That
value is kept as useful solver-side diagnostics, but it is NOT used as the
independent external reference by VerifyKnownResultAsync.

When KnownResult.ReportedObjectiveValue is available, the checker compares:

    raw objective reported by CPLEX
        versus
    objective independently recomputed from the candidate decision values
    and the MathematicalModel objective expression.

This avoids the circular comparison "recomputed objective versus the same
recomputed objective".

Checker rejection
-----------------
A mathematically invalid candidate is different from a technical failure.
When the checker rejects a solution:
- the batch continues;
- the candidate KnownResult is NOT inserted into the instance;
- the candidate is NOT made the best-known result;
- the resolved XML is NOT written;
- the following diagnostics remain available:

    Petit\résolu\<instance>.mathematical-model.txt
    Petit\résolu\<instance>.solution-check.txt

A successful candidate additionally produces:

    Petit\résolu\<instance>.xml

Old output files for the current instance are deleted before a new attempt,
so a rejected or failed rerun cannot leave a stale resolved XML that looks
current.

KnownResult / SolutionEvaluation updates
----------------------------------------
For a Full valid check, the checker:
- records independently established feasibility;
- records maximum/total constraint violation and violated count;
- stores the checker-recomputed objective in Solution.Evaluation;
- sets evaluator metadata to LotSizingSolutionChecker / 1.1;
- promotes the candidate KnownResult to AutomaticallyVerified.

Optimality status, best bound and optimality gaps remain solver information
and are not invented or overwritten by the checker.

Project reference
-----------------
The existing LotSizingDataModel.Solver.Console.csproj must reference the
checker project. Add:

    <ProjectReference Include="..\LotSizingDataModel.Checker\LotSizingDataModel.Checker.csproj" />

Do not replace the whole Solver.Console project file if it already contains
CPLEX-specific configuration or other project references.

Installation
------------
1. Update the checker projects with the Package 14 files.
2. Replace Program.cs in LotSizingDataModel.Solver.Console.
3. Add the ProjectReference shown above to the existing Solver.Console csproj.
4. Clean Solution.
5. Rebuild Solution.
6. Run all checker tests. Package 14 contains 17 tests.
7. Run LotSizingDataModel.Solver.Console (Ctrl+F5).

Expected console behavior
-------------------------
Each solved instance now shows:
- solver raw objective;
- solver-side recomputed objective;
- checker stage results (structure, domains, feasibility, objective);
- violated-constraint count / maximum violation;
- raw CPLEX objective used by the checker;
- independently recomputed checker objective;
- objective difference and comparison tolerance;
- path of the detailed solution-check report.

Final batch counters distinguish:
- solved and independently verified instances;
- candidates rejected by the checker;
- technical failures.
