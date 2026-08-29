# Canonical Lot-Sizing Problem Classes — alpha.15

This catalog separates **problem classes** from planning paradigms,
mathematical formulations and solution methods.

MRP and DRP are therefore not entries in this catalog.

## Executable canonical classes

LotSizingDataModel uses explicit canonical codes to avoid literature acronym
ambiguity.

### SI-ULS

Single-item, single-level, deterministic, uncapacitated production with setup
costs.

Core universal specification:

`1,SL,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:?`

### SI-CLSP

Single-item capacitated lot-sizing problem.

`1,SL,Net:UNK | Dem,Det,Prod,Cap:P,SC | Obj:?`

Single-item CLSP formulations are explicitly present in the literature.

### MI-ULS

Multi-item uncapacitated single-level lot sizing.

`m,SL,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:?`

The uncapacitated formulation is also used with a set of items in the
literature. In the absence of coupling constraints, the items decompose.

### MI-CLSP

Multi-item single-level capacitated lot sizing with shared production
capacity.

`m,SL,Net:UNK | Dem,Det,Prod,Cap:P,Cap:Shared,SC | Obj:?`

This is the common tactical CLSP interpretation in which several products
share limited machine/resource capacity.

### UMLSP

Uncapacitated multi-level lot-sizing problem:

`m,ML:?,Net:UNK | Dem,Det,Prod,Uncap:P,SC | Obj:?`

The BOM topology may be serial, assembly, arborescent or general. `MLLP` is
retained as a project/literature alias.

### MLCLSP

Multi-level capacitated lot-sizing problem:

`m,ML:?,Net:UNK | Dem,Det,Prod,Cap:P,SC | Obj:?`

MLCLSP extends capacitated lot sizing by incorporating BOM/dependent-demand
relationships.

## Core versus extension

Membership is not closed-world taxonomy.

`ExactCore` means the current descriptor matches a canonical core without any
of the explicitly tracked industrial extensions.

`CompatibleExtension` means the core problem is present plus one or more
modeled extensions such as:

- initial inventory;
- safety stock;
- backlogging or lost sales;
- setup times / start-up costs;
- lead times;
- minimum/maximum/multiple lot restrictions;
- procurement;
- transport/distribution;
- additional capacity;
- financial constraints;
- multiple objectives.

`Incomplete` means the descriptor lacks information needed to decide.

`NotApplicable` means a known characteristic contradicts the class core.

## Scientific references

The catalog is aligned with the terminology used across:

- Brahimi et al., *Single item lot sizing problems*, EJOR, 2006;
- Jans & Degraeve, *Modeling industrial lot sizing problems: a review*,
  IJPR, 2008;
- Buschkühl, Sahling, Helber & Tempelmeier,
  *Dynamic capacitated lot sizing problems: A classification and review of
  solution approaches*, OR Spectrum, 2010;
- Helber & Sahling, *A fix-and-optimize approach for the multi-level
  capacitated lot sizing problem*, IJPE, 2010.
