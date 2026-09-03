# LSI 1.0 engineering capitalization

This document records permanent engineering guards learned while integrating
LSI Packs 01-10 into the local LotSizingDataModel worktree.

## Generated-source installation

1. The local worktree is the execution truth.
2. Run a read-only capability audit before cross-layer generated packs.
3. A generated C# file replacement must receive a current LastWriteTimeUtc.
4. Remove affected bin/obj or perform a reliable non-incremental rebuild.
5. Run tests with --no-build only after the fresh build completed.
6. A partial extension requires the principal declaration to be partial.
7. Cross-namespace generated symbols require explicit using directives.
8. PowerShell parser guards apply only to PowerShell source.
9. Do not use fragile silent multiline text replacement for critical predicates.
10. Do not embed if/else or concatenation expressions directly in PowerShell
    command argument lists; compute values first.

## Integration fixtures

1. A routing is not a complete production capability declaration.
2. Full formulation fixtures must materialize the ProductionCharacteristic for
   every routed item/work-center pair required by the formulation.
3. A full-formulation fixture must test CanBuild before BuildAsync.
4. A fixture should prove its own topology before testing a validator.
5. After two speculative repair attempts, stop and run an instrumented
   read-only diagnostic.

## Scientific semantics

1. Unknown never means absent in LSI.
2. Existing historical classifiers remain compatibility projections.
3. Production setup family is distinct from item setup, product family, BOM
   grouping and grouping constraints.
4. Setup carry-over and sequence-dependent changeovers are represented
   explicitly and may only be admitted by a formulation that has an executable
   scheduling profile.
5. Sequence-dependent setup state is a scheduling construct and must not be
   silently approximated by an unrelated big-bucket setup binary.

These guards are part of the LSI 1.0 finalization baseline.
