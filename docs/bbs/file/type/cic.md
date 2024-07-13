# CIC Format

CIC stands for *Common Ice ?* and it controls several aspects of the ice cream minigame.

# Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | File identifier, always `0x41261002`
| 0x4     | uint32   | Size
| 0x8     | int32    | Base Points
| 0xC     | float    | Stage Level 0
| 0x10    | float    | Stage Level 1
| 0x14    | float    | Stage Level 2
| 0x18    | int32    | Stage Combo 0
| 0x1C    | int32    | Stage Combo 1
| 0x20    | int32    | Stage Combo 2
| 0x24    | float    | Bad Timing
| 0x28    | float    | Good Timing
| 0x2C    | float    | Excellent Timing
| 0x30    | int32    | Art 1
| 0x34    | int32    | Art 2
| 0x38    | int32    | Art 3
| 0x3C    | int32    | Art 4
| 0x40    | int32    | Ice Bonus
| 0x44    | int32    | Ice Failure
| 0x48    | int32    | Combo Points