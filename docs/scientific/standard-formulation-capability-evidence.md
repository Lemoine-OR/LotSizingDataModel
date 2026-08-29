# Standard Formulation Capability Evidence

The alpha.17 profile is deliberately tied to actual source components rather
than broad claims.

Examples in the current formulation factory:

- `ProductionVariableFamilyBuilder`;
- `SetupVariableFamilyBuilder`;
- `InventoryVariableFamilyBuilder`;
- `BacklogVariableFamilyBuilder`;
- `ShortageVariableFamilyBuilder`;
- `ProcurementVariableFamilyBuilder`;
- `TransportVariableFamilyBuilder`;
- additional-capacity variable families;
- `LotSizeMultipleVariableFamilyBuilder`.

Constraint families include:

- production/setup links;
- minimum lot size;
- lot-size multiple;
- grouping constraints;
- safety stock;
- inventory balance;
- demand satisfaction;
- inventory/work-center/warehouse/transport capacities;
- resource-activation links.

`InventoryBalanceConstraintFamilyBuilder` explicitly shifts production,
supplier and transport flows by their respective lead times and includes
initial inventory in period 1.

`WorkCenterCapacityConstraintFamilyBuilder` explicitly adds setup-time
consumption when setup variables exist.

The capability profile should be updated whenever the standard formulation
factory gains or loses a builder family. Scientific support must never drift
away from implementation support.
