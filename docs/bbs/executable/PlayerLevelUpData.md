# PLAYER LEVEL UP DATA

This isn't a file, but data specification that appears instantiated in memory.

## Stats

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32[99] | Level Up Experience
| 0x18C   | uint32[3][99] | [Level Up](#Level-Up-Values) Rewards
| 0x630   | uint32[24] | unknown

Level up Rewards consist of a triplet of uint32, one for each character in this following order: `Ventus-Aqua-Terra` repeating for each level starting with Level 1 stats which start from `0, 0, 0`.

### Level Up Values

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint8 | Strength Increase
| 0x1    | uint8 | Magic Increase
| 0x2    | uint8 | Defense Increase
| 0x3    | uint8 | Padding
