# WORLDPOINT Format

This file stores information related to areas visitable in each world through the worldmap.

The subfile is contained within `preset/areainfo.arc`

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Version, always `1`.
| 0x4     | uint32   | Data Count

## Worldpoint Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Index
| 0x2     | uint8   | World ID
| 0x3     | uint8   | Room ID