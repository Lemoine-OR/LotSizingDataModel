# Start-Up Semantics — alpha.21

## Scientific definition

The start-up concept is a transition concept, not an ordinary setup concept.

Following Wolsey (2002):

- start-up cost: if a sequence of setups starts in period `t`, a cost `g_t`
  is incurred;
- start-up time: if a sequence of setups starts in period `t`, period
  capacity is reduced by `ST_t`.

This makes both concepts dependent on the beginning of a setup sequence.

## Core

New period-dependent non-negative parameters:

- `StartUpCost`;
- `StartUpTime`.

They are attached to `ProductionCharacteristic` and participate in the same
planning-horizon lifecycle as the other production decision-model parameters.

They do not reuse `FixedSetupCost` or `SetupTime`.

## Instance

New factual feature:

`HasStartUpTimes`.

`HasStartUpCosts`, which previously existed as a compatibility feature but was
always extracted as false because Core had no dedicated type, is now detected
from actual Core data.

Typed setup descriptor:

- `HasSetupCosts`;
- `HasSetupTimes`;
- `HasStartUpCosts`;
- `HasStartUpTimes`.

## Universal notation

- `SU`: start-up cost;
- `SUT`: start-up time;
- `TP:SUT=<pattern>`: temporal profile of start-up time.

This deliberately avoids the Wolsey collision:

Wolsey `ST` = start-up time.

Universal `ST` = ordinary setup time.

## Mathematical formulation status

Partial by design.

The standard formulation has no transition variable equivalent to:

`z_t = 1` when a sequence of setups starts in period `t`.

Therefore it cannot yet impose start-up cost or start-up capacity consumption.
Its scientific profile marks both extensions `KnownUnsupported`.

This is safer than accepting the instance and ignoring the parameters.
