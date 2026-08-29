# Typed Problem Descriptors

`v1.2.0-alpha.4` introduces a typed structured layer over the historical
`LotSizingProblemFeatures` vector without changing XML serialization.

Initial domains:
- Structure
- Time
- Demand
- Production / lot-size
- Capacity
- Setup
- Inventory / service
- Procurement
- Transportation / distribution
- Objective / finance

The migration is deliberately lossless:

`legacy Features -> typed Descriptor -> legacy Features`

The target direction remains:

`Core source data -> typed Descriptor -> legacy Features / classification / notation`

Scheduling, workforce, maintenance, uncertainty, sustainability and product
lifecycle are not added as empty placeholders. They will appear only when
their real scientific semantics exist in the data model.
