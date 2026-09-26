# Central transport lanes and assignments

## Audit of the previous model

The starting point was commit `524da10` (1.3.0). The following are observations
from the implementation, not inferred business requirements.

| Area | Previous implementation | Change |
|---|---|---|
| Core/PhysicalModel/TransportLane.cs | Origin, destination and delay, no ID | Stable lane ID and endpoints only |
| Core/PhysicalModel/TransportResource.cs | Own mutable list of lanes; uniqueness per resource | Independent resource; capacities/costs unchanged |
| Core/Relationships/TransportCharacteristic*.cs | Item/resource compatibility, item-specific capacities, setup and usage parameters | Same scope and parameters retained |
| Core/Validation/SupplyChainValidator.cs | Required at least one lane per resource | Central uniqueness/reference validation; separate readiness diagnostics |
| Core/Building/SupplyChainModelBuilder.cs | Added a lane inside a resource | Adds central lanes and explicit assignments |
| Core/Indexing/SupplyChainIndex.cs | Resource index only | Lane and assignment indexes, rebuilt explicitly |
| Core/Querying/*.cs | Direct options and Dijkstra fastest path over nested lanes | Join central lanes with assignment delays; retain resource and assignment identity |
| Core/Serialization/SupplyChainXmlSerializer.cs and Instance/Serialization/LotSizingInstanceXmlSerializer.cs | Direct XML object loading | In-memory migration before object loading |
| Instance/Classification and Instance/Descriptors/Network | Network inferred from each resource's lanes | Network inferred from authorized assignments |
| Solution/Creation and Solution/Validation | Resource plus typed endpoints as transport decision key | Same natural key, resolved against central assignments |
| Solver/Formulation/Transport* and InventoryBalanceConstraintFamilyBuilder | Departures, delayed receipts; global and item capacity already summed across lanes | Assignment domain/delay; shared extra-capacity budget; activation corrections |

The generic executable MILP and independent checker exist in this repository.
The checker evaluates the generated model and uses the Solution mapping; it
does not have a separate handwritten transport-balance implementation. No
transport-specific initial-transit entity was found. `Inventory.ScheduledReceipt`
exists for already scheduled receipts; its documented production/purchase scope
is retained. Import's Dellaert–Jeunet input is a separate format and defines no
transport lanes; no transport importer was invented.

The existing `Core/Analysis/CapacityAnalyzer`, `ResourceLoadAnalyzer` and
`CostAnalyzer` work on caller-supplied item/resource activity totals, not on
lane objects; their resource-level APIs require no new lane representation.
They are diagnostic helpers, not the executable MILP/checker. Their historical
absent-consumption default is zero, whereas the executable transport formulation
uses one; `ResourceLoadAnalyzer` also accepts a single aggregate setup boolean
per item/resource. Those existing conventions are retained rather than silently
changed by this structural migration. For cross-checks, supply explicit unit
consumption coefficients and use the generated-model checker for multiple
per-lane setups. Aligning these older aggregate helper semantics is a separate
pre-existing limitation, not a claim of this transport formulation's tests.

## Domain and cardinalities

`SupplyChain.TransportLanes` contains `TransportLane(Id, Origin, Destination)`.
`SupplyChain.TransportResources` contains independent `TransportResource(Id, Name)`.
`SupplyChain.TransportAssignments` contains
`TransportAssignment(LaneId, TransportResourceId, LeadTime)`.

A lane has exactly one origin and one different destination. A warehouse reference
is the pair `(Kind, ReferenceId)`; a plant warehouse and standalone warehouse
with the same numeric ID are distinct. The directed endpoint pair and lane ID
are each unique. Reverse movement requires a separate lane and assignment.
Each assignment references exactly one existing lane and one existing resource;
its pair is unique and its delay is an integer greater than or equal to zero.
A lane and a resource each have zero or more assignments: the relationship is
many-to-many. Empty assignment sets are valid during construction.

`AssignedTransportLane` is an unpersisted join holding references to a central
lane and an assignment. Its endpoint/delay getters delegate to those objects;
it is not an alternative stored representation. `GetTransportLanes(resourceId)`
returns this join. `FindTransportLane` and `FindTransportAssignment` resolve
unique keys. `SupplyChainIndex` resolves lane IDs and assignment pairs and,
like the pre-existing index, must be rebuilt after direct collection edits.

The ordinary validator reports structural errors. Call
`ValidateTransportReadiness` separately for warnings about unassigned lanes or
resources; these warnings do not make a construction-stage model invalid.
These diagnostics do not claim that the complete optimization problem is feasible.

Removal is explicit: remove assignments before a referenced lane; remove both
assignments and item/resource characteristics before a resource. The methods
throw without mutation if references remain. No cascade occurs. Existing public
collections remain mutable for XML/construction, so directly removing a warehouse,
lane or resource may temporarily leave invalid references, which validation
reports. IDs and endpoint references must not be changed while external solutions
or indexes refer to them; validate/rebuild or regenerate dependent artifacts after
an intentional edit. The model does not own external Solution objects and cannot
cascade into them; Solution validation rejects decisions on removed assignments.

## Formulation and implementation mapping

Periods are the existing one-based set `1,...,H`. Let `L` be directed lanes,
`R` resources, `A ⊆ L × R` authorized assignments, and `K ⊆ I × R` the
item/resource pairs defined by `TransportCharacteristic`. For `(l,r) ∈ A`,
`tau_(l,r)` is `TransportAssignment.LeadTime`. Variables are created only when
both `(l,r) ∈ A` and `(i,r) ∈ K`.

| Symbol | Meaning / source | Generator or consumer |
|---|---|---|
| `x_(i,l,r,t)` | Quantity departing at period t | TransportVariableFamilyBuilder; TransportDecision.TransportedQuantities |
| `z_(i,l,r,t)` | Existing per-lane item setup | TransportSetupVariableFamilyBuilder; TransportDecision setup series |
| `y_(r,t)` | One activation per resource/period | TransportResourceActivationVariableFamilyBuilder |
| `u_(r,t)` | Shared resource additional capacity | TransportResourceAdditionalCapacityVariableFamilyBuilder |
| `v_(i,l,r,t)` | Allocation of item/resource extra capacity to a lane | TransportAdditionalCapacityVariableFamilyBuilder |
| `a_(i,r,t)` | UnitCapacityConsumption, default 1 | TransportCharacteristic |
| `s_(i,r,t)` | SetupTime, default 0 | TransportCharacteristic |
| `C_(r,t), U_(r,t), F_(r,t)` | CapacityConstraint, AdditionalCapacity, FixedUsageCost | TransportResource |
| `C_(i,r,t), V_(i,r,t), c_(i,r,t), f_(i,r,t)` | Item capacity, additional capacity, UnitUsageCost, FixedSetupCost | TransportCharacteristic |

Transport domain keys and Solution keys continue to identify a lane by its unique
**typed directed endpoint pair**, together with the resource, item and period.
Thus both lane and resource are unambiguously identified without storing another
lane reference in solutions. Variable display names now use the stable lane ID,
avoiding collisions between warehouse kinds. Existing mapping classes retain
their endpoint-key contract, including `TransportDecisionMapper`,
`TransportSetupDecisionMapper`, and the capacity/activation mappers.

For the transport part of an inventory balance at warehouse `w`,

```text
I_(i,w,t) - I_(i,w,t-1)
  = other existing inflows/outflows
    + sum[x_(i,l,r,t-tau_(l,r)) : destination(l)=w, t-tau_(l,r)>=1]
    - sum[x_(i,l,r,t) : origin(l)=w].
```

`InventoryBalanceConstraintFamilyBuilder` reads the delay from the joined
assignment. Delay zero produces a departure and arrival in the same period.
There are no variables at periods zero or below and no inferred initial transit.
Existing initial inventory and scheduled receipts remain unchanged. As before,
departures whose arrivals are after `H` remain permitted: they consume source
stock and departure-period capacity/cost, with no receipt or terminal transit
valuation within the modeled horizon. This is an explicit preservation of the
existing open-horizon convention, not a new terminal-stock rule.

For each resource and departure period, the global constraint is

```text
sum_(i,l compatible with r) [a_(i,r,t) x_(i,l,r,t) + s_(i,r,t) z_(i,l,r,t)]
    <= C_(r,t) + u_(r,t),        0 <= u_(r,t) <= U_(r,t).
```

This is one constraint and one additional-capacity variable per resource/period,
not one capacity allowance per assignment. The analogous item/resource constraint
is summed over its assigned lanes and uses `sum_l v_(i,l,r,t)` as additional
capacity. A new allocation budget imposes

```text
sum_l v_(i,l,r,t) <= V_(i,r,t).
```

Previously each lane could independently allocate the full item/resource extra
capacity, multiplying the actual allowance. The new budget corrects that defect
without changing the existing per-lane Solution allocation series or its unit cost.

The transport portion of the objective retains

```text
sum_(i,l,r,t) [c_(i,r,t) x_(i,l,r,t) + f_(i,r,t) z_(i,l,r,t)
              + additionalItemCapacityCost_(i,r,t) v_(i,l,r,t)]
 + sum_(r,t) [F_(r,t) y_(r,t) + additionalResourceCapacityCost_(r,t) u_(r,t)].
```

The resource fixed cost occurs once, regardless of the number of lanes used.
Item setup costs/time retain the existing per-lane convention; they are not
silently reinterpreted as a single shared item setup. Positive departures now
require the applicable resource activation and item setup through
`x <= M y` and `x <= M z`. `M` is a valid quantity upper bound derived from
global/item capacity including enabled additional capacity divided by positive
unit consumption. Use the tighter bound when both exist. If no finite bound can
be derived, callers must provide `StandardLotSizingFormulationOptions.TransportActivationBigM`;
otherwise formulation throws an explanatory error. No arbitrary bound is selected.
This fixes the former missing setup link and the resource activation link that
ignored additional capacity (and was absent for uncapacitated resources).

For example, departures of six units on each of two truck lanes consume twelve
units of the same capacity. With regular capacity ten and additional capacity
two, both are possible only with two additional units. A fixed truck cost of
seven is incurred once, not twice. For Nantes–Lyon, departure in period one
arrives in period three by a truck with delay two, or period four by a train
with delay three.

## XML migration and API break

The transport subformat is now `transportFormatVersion="2"` on `supplyChain`.
This versions transport independently of unrelated instance provenance formats.
Transport-free documents omit the new empty collections and attribute, preserving
their canonical XML/fingerprints. No release tag or package version is published
by this change; the API break must be included in the next release policy decision.

Use `SupplyChainXmlSerializer` or `LotSizingInstanceXmlSerializer` for loading old
files. They call `TransportXmlMigration` through their existing secure XML readers.
Migration is in memory and never writes the input file. Save to a **different
output path** to retain the original; the pre-existing Save APIs intentionally
overwrite their requested destination.

The migrator groups old `transportResource/lanes/transportLane` elements by typed
directed endpoint pair, sorts the numeric `(origin kind, origin ID, destination
kind, destination ID)` tuples, and assigns IDs `1,...,N`. Consequently identical
legacy networks give identical lane IDs irrespective of resource/lane ordering
or culture. It emits assignments sorted by resource ID, transferring each delay.
IDs, names, parameters, and other resource and characteristic XML remain in place.
Identical duplicate entries are consolidated; contradictory delays for the same
pair throw. Malformed integers, negative delays, unknown legacy lane fields and
mixed central/nested representations are rejected instead of choosing a value or
discarding information. A version-2 document is not migrated again. Unknown future
versions are rejected. Raw `XmlSerializer` loading of nested lanes throws with
instructions to use a migrating serializer rather than silently dropping lanes.

Migrated networks have a different canonical XML fingerprint. Recorded instance
provenance/fingerprints are not silently rewritten. If a historic instance carries
an old fingerprint, use the existing `validateCurrentFingerprint: false` load
option for the explicit migration workflow, inspect and rebaseline provenance
using the existing instance APIs before normal validated use. Old files and their
recorded provenance remain available for audit.

Removed APIs: `TransportResource.Lanes`, `AddLane`, `RemoveLane`, `FindLane`,
`TransportLane.LeadTime`, and the old lane constructor and nested builder overload.
Replace them with central lane creation, `AddTransportAssignment`, explicit
assignment removal, and the central query APIs. Path/direct-option results expose
the assigned-lane join; its `Lane` and `Assignment` retain the exact source objects.
The fastest-path criterion remains the existing Dijkstra minimum total delay;
it is never applied to migration or ordinary direct-option enumeration.

## Construction example

```csharp
var chain = new SupplyChain(4);
chain.AddStandaloneWarehouse(new StandaloneWarehouse(1, "Nantes"));
chain.AddStandaloneWarehouse(new StandaloneWarehouse(2, "Lyon"));
chain.AddStandaloneWarehouse(new StandaloneWarehouse(3, "Bordeaux"));
chain.AddTransportResource(new TransportResource(10, "Truck") {
    CapacityConstraint = new CapacityConstraint(4, 10),
    FixedUsageCost = new FixedUsageCost(4, 7)
});
chain.AddTransportResource(new TransportResource(20, "Train"));
chain.AddTransportLane(new TransportLane(100,
    WarehouseReference.ForStandaloneWarehouse(1),
    WarehouseReference.ForStandaloneWarehouse(2)));
chain.AddTransportLane(new TransportLane(101,
    WarehouseReference.ForStandaloneWarehouse(1),
    WarehouseReference.ForStandaloneWarehouse(3)));
chain.AddTransportAssignment(new TransportAssignment(100, 10, 2));
chain.AddTransportAssignment(new TransportAssignment(100, 20, 3));
chain.AddTransportAssignment(new TransportAssignment(101, 10, 1));
// Add items, inventories and TransportCharacteristic(itemId, resourceId)
// as usual before building the optimization instance.
```

## Verification sources

`Core.Tests/TransportAssignmentTests.cs` covers cardinalities, typed endpoint
identity, reverse lanes, unassigned construction/readiness, duplicate/reference
errors, explicit removals, central indexes, direct options/fastest paths,
serialization, migration ordering/idempotence/conflicts and invalid delays.
`Checker.Tests/Formulation/TransportAssignmentFormulationTests.cs` executes the
MILP generators and verifies numerical coefficients, aggregated resource/item
capacity, fixed costs, activation/setup links, assignment/item domain restrictions,
zero/different/extreme delays, horizon edges, scheduled receipts, Solution
reference validation, instance XML, and equivalence of a migrated single-lane
model. These are executable model-generation and numerical constraint tests;
they do not require or claim a commercial optimizer solve.

An additional native CPLEX regression is available when CPLEX is installed:
`dotnet run --project LotSizingDataModel.Solver.Test -c Release -- --transport-smoke`.
It solves two zero-delay lanes with one shared truck: three units per lane cost
19 (12 variable + 7 fixed); six per lane are infeasible at capacity ten; with two
extra units costing three each, the optimum is 37 (24 + 7 + 6).
