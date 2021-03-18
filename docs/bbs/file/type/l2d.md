# L2D Format

L2D stands for *Layout 2 Dimensional*.

This file type contains all types of menus or interactible 2D widgets.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `L2D@`.
| 0x4     | char[4]   | Version
| 0x8     | char[8]   | Date
| 0x10    | char[4]   | Name
| 0x14    | uint8[4]  | Reserved
| 0x18    | uint8[8]  | Reserved
| 0x20    | int32     | SQ2P Count
| 0x24    | int32     | SQ2P Offset
| 0x28    | int32     | LY2 Offset
| 0x2C    | int32     | File Size
| 0x30    | uint8[16] | Reserved

## SQ2P Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `SQ2P`.
| 0x4     | char[4]   | Version
| 0x8     | uint8[4]  | Reserved
| 0x10    | uint32    | SP2 Offset
| 0x14    | uint32    | SQ2 Offset
| 0x18    | uint32    | TM2 Offset
| 0x1C    | uint8[36] | Reserved

### SP2 Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `SP2@`.
| 0x4     | char[4]   | Version
| 0x8     | uint8[4]  | Reserved
| 0x10    | int32     | Parts Count
| 0x14    | int32     | Parts Offset
| 0x18    | int32     | Group Count
| 0x1C    | int32     | Group Offset
| 0x20    | int32     | Sprite Count
| 0x24    | int32     | Sprite Offset
| 0x28    | int8[24]  | Reserved

### SQ2 Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | Signature, always `SQ2@`.
| 0x4     | char[4]   | Version
| 0x8     | uint8[4]  | Reserved
| 0x10    | int32     | Sequence Count
| 0x14    | int32     | Sequence Offset
| 0x18    | int32     | Control Count
| 0x1C    | int32     | Control Offset
| 0x20    | int32     | Animation Count
| 0x24    | int32     | Animation Offset
| 0x28    | int32     | Key Count
| 0x2C    | int32     | Key Offset
| 0x30    | int32     | Sequence Name Offset
| 0x34    | int32     | Sequence ID Offset
| 0x38    | int8[8]   | Reserved