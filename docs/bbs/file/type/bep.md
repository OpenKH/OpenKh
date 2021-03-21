# BEP Format

BEP stands for *Base Enemy Parameter* and it controls the general status for the character amongst other things.
It is only used for one file named `EnemyCommon.bep` within `arc/system/commongame.arc`

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `@BEP`
| 0x4     | uint32 | Version, `2`
| 0x8     | uint32 | Count of [Base Parameters](###Base-Parameters)
| 0xC     | uint32 | Offset to [Base Parameters](###Base-Parameters)
| 0x10    | uint32 | Count of [Disappear Parameters](###Disappear-Parameters)
| 0x14    | uint32 | Offset to [Disappear Parameters](###Disappear-Parameters)

### Base Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Battle Level
| 0x2     | uint16   | Base Attack
| 0x4     | uint16   | Defense
| 0x6     | uint8    | Damage Ceiling
| 0x7     | uint8    | Damage Floor
| 0x8     | uint32   | Base HP
| 0xC     | uint32   | Base EXP

### Disappear Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16  | World ID
| 0x2     | uint16  | Area ID
| 0x4     | float   | Distance
