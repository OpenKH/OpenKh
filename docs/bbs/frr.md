# FRR Format

FRR stands for *? ? Resident*.

This format's purpose is unknown.

The only known instance of this file is within `arc/system/common.arc` in the subfile `resident.frr`.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `FRR`. Null terminated.
| 0x4     | uint32  | Index
| 0x8     | uint32  | Major Version
| 0xC     | uint32  | Minor Version
| 0x10    | uint32  | File Size
| 0x14    | uint32  | Texture Count
| 0x18    | uint32  | Animation Count
| 0x1C    | uint32  | Model Count
| 0x20    | uint32  | VList Count
| 0x24    | uint32  | Texture Handle Offset
| 0x28    | uint32  | Animation Handle Offset
| 0x2C    | uint32  | Model Handle Offset
| 0x30    | uint32  | VList Handle Offset
| 0x34    | uint32  | Texture Data Offset
| 0x38    | uint32  | Animation Data Offset
| 0x3C    | uint32  | Model Data Offset
| 0x40    | uint32  | VList Data Offset

How the data for this file is handled is unknown.