# GLSP Solver / Solution / Checker Bridge — alpha.29

Mathematical categories: `microPeriodProduction`, `microPeriodSetupState`, `auxiliaryMicroPeriodChangeover`. Domain keys carry plant, work center, macro period and micro-period index. Production/setup keys also carry routing/item; changeovers carry from-item/to-item.

`WorkCenterSchedulingDecision` remains the normalized persisted schedule. Changeovers are not persisted because they are exactly derivable from adjacent setup states. Aggregate macro production is retained and linked exactly to micro production, avoiding duplication of inventory, demand, lead-time and network equations.
