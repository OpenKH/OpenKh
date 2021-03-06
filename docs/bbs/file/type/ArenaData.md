# ARENADATA Format

This file stores a list missions on Mirage Arena and parameters related to them.

This file is contained within `arc/system/common_vs.arc` named `ArenaData.bin`.

## Arena Data 1

This section is read until a completely NULL section is found. 

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | CTD ID
| 0x4     | int8   | Area
| 0x5     | int8   | Rank
| 0x6     | int8   | Level
| 0x7     | int8   | Extra
| 0x8     | uint16  | Medal Reward
| 0xA     | uint16  | Data (unknown)
| 0xC     | int8   | Rounds Count
| 0xD     | int8   | Text ID
| 0xE     | int8   | Area Data
| 0xF     | int8   | Area Count

The second data structure is unknown at the moment.