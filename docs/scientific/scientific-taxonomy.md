# Scientific Taxonomy

`v1.2.0-alpha.9` makes the scientific classification dimensions explicit.

The project must not collapse the following categories:

1. **Structural property**
   - BOM topology;
   - physical network topology;
   - echelon count;
   - multisourcing;
   - cyclic/acyclic structure.

2. **Planning paradigm**
   - MRP: Material Requirements Planning;
   - DRP: Distribution Requirements Planning.

3. **Lot-sizing problem class**
   - problem families such as uncapacitated/capacitated, single/multi-item,
     single/multi-level lot-sizing classes belong here;
   - the exact canonical catalog is introduced in a later mapping increment.

4. **Mathematical formulation**
   - MILP/network-flow/extended/other formulations belong here.

5. **Solution method**
   - exact algorithms, heuristics, metaheuristics and solver strategies belong
     here.

6. **Historical classification**
   - e.g. the Bitran-Yanasse temporal classification.

## DRP correction

DRP is **Distribution Requirements Planning**: a planning method/process for
calculating time-phased replenishment requirements in a distribution network.
It is not a lot-sizing mathematical model.

Therefore alpha.9 removes the property:

`IsClassicalDrpTopologyCandidate`

and replaces it with neutral observable facts:

- `HasDistributionNetwork`;
- `HasExternalDemandAtDistributionCenters`;
- `HasMultiEchelonStructure`;
- `IsAcyclicSingleSourcingDistributionNetwork`.

A later mapping layer may state that a particular planning paradigm is
compatible with a set of facts, but that mapping must never rename the
underlying network topology or lot-sizing problem class as "DRP".

## Notation consequence

The universal `alpha | beta | gamma` notation describes factual structure,
constraints/features and objective. For example:

`m,SL,Net:DIV:E3 | Dem,Det,Tr,Dist | Obj:Econ`

It should not insert `DRP` merely because the network is divergent,
multi-echelon or contains distribution centers.

## References used for the distinction

- Bitran, G.R., Yanasse, H.H. (1982), *Computational Complexity of the
  Capacitated Lot Size Problem*, Management Science 28(10), 1174-1186.
- Karimi, B., Fatemi Ghomi, S.M.T., Wilson, J.M. (2003), *The capacitated lot
  sizing problem: a review of models and algorithms*, Omega 31(5), 365-378.
- Standard terminology for Distribution Requirements Planning defines DRP as
  a method/process for determining time-phased replenishment requirements in
  a distribution network.
