# LSI/1.0 compact reference

Canonical form:

`LSI/1.0: pi{...} | alpha{...} | beta{...} | gamma{...} @ sigma{...}`

## pi

- H: horizon regime
- TM: time model
- BK: bucket/scheduling regime
- INF: information regime
- DEM: demand temporal regime
- DEM.SRC: demand source

## alpha

- I: item cardinality
- L: single/multi-level
- PS: product structure
- NET: supply-network topology
- SITE: single/multi-site
- WC: work-center count

## beta

Selected mappings include:

- CAP.P, CAP.P.SH, CAP.S, CAP.T, CAP.W
- SET.C, SET.T, SET.SU.C, SET.SU.T
- SET.INIT, SET.CO, SET.SD.T, SET.SD.C
- SET.FAM, SET.FAM.T
- LOT.MIN, LOT.MAX, LOT.MUL, LOT.GRP
- LT.P, LT.S, LT.T
- INV.I0, INV.SS
- SHORT.BO, SHORT.LS
- SRC.BUY, TRANS, DIST
- FIN
- SCH, SCH.BB, SCH.SB, SCH.MM, SCH.MAXSET

Temporal qualifiers are rendered as `rho(code)=pattern`, with patterns
Z, C, NI, ND, G or ?.

## gamma

- S=MIN
- AGG=SINGLE or MULTI
- OBJ=ECON, MULTI, FIN, SUST, SERVICE or ?

## sigma

- T, I, P, WC, WH, SUP, DC, TR, BOM, DEPTH
