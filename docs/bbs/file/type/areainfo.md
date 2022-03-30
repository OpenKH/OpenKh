# AREAINFO Format

This file stores information related to areas shown in the worldmap.

The subfile is contained within `preset/areainfo.arc`

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Version, always `2`.
| 0x4     | uint32   | Data Count

## Area Info Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint8    | World ID
| 0x1     | uint8    | Room ID
| 0x2     | uint8    | Flag
| 0x3     | uint8    | External SCD
| 0x4     | uint16   | Heap
| 0x6     | uint16   | Padding