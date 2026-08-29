# Generic Temporal Qualifiers

`v1.2.0-alpha.11` extends universal notation scheme v1 with generic temporal
qualifiers in the beta field.

Syntax:

`TP:<parameter>=<pattern>`

Initial parameter codes:

- `Dem`: demand;
- `SC`: setup cost;
- `HC`: holding cost;
- `PC`: production cost;
- `CapP`: production capacity.

Pattern codes reuse the canonical temporal vocabulary:

- `Z`: zero;
- `C`: constant;
- `NI`: non-increasing;
- `ND`: non-decreasing;
- `G`: general.

Example:

`1,SL,Net:UNK | Dem,Det,Prod,Cap:P,TP:SC=NI,TP:HC=G,TP:PC=NI,TP:CapP=ND | Obj:Econ`

These tokens are universal semantic qualifiers. They do not contain the name
or positional vocabulary of any historical classification.

## Matching

When a specification requires a temporal qualifier:

- no actual analysis supplied -> `Incomplete`;
- known different pattern -> `Contradiction`;
- known equal pattern -> requirement satisfied.

The generator has an overload accepting explicit generic temporal qualifiers.
This avoids pretending that an arbitrary multi-item/multi-site descriptor has
one unique aggregate temporal pattern when no scientifically valid projection
has been selected.

## Historical mappings

Bitran-Yanasse maps exactly as:

- historical alpha -> `TP:SC`;
- historical beta -> `TP:HC`;
- historical gamma -> `TP:PC`;
- historical delta -> `TP:CapP`.

The historical slash notation remains preserved separately.
