# [Kingdom Hearts: Birth By Sleep](./index.md) - PMP

PMP files contain several [PMOs](./pmo.md) and their associated [textures](../common/tm2.md), which are used to render the static geometry of a map.

## PMP Header 

| Offset | Type | Description |
|--------|------|-------------|
| 0x0 | uint32 | Magic value. Always "PMP\0" (0x00504D50) |
| 0x4 | uint16 | Version |
| 0x6 | uint16 | Padding |
| 0x8 | uint32 | Padding |
| 0xC | uint8[3] | Padding |
| 0xF | uint8 | Flag |
| 0x10 | uint16 | Object Instance Count |
| 0x12 | uint16 | Model Instance Count |
| 0x14 | uint32 | Padding |
| 0x18 | uint16 | Padding |
| 0x1A | uint16 | Texture Count |
| 0x1C | uint32 | Texture List Offset |

### Map Flags

The usage of these flags is still unknown.

| Value | Name  
|--------|------
| 0 | NO_FLAG
| 1 | MAPFLAG_DISPOFF
| 2 | MAPFLAG_PRESETOFF
| 4 | MAPFLAG_SYSPRESETOFF

## Object Instance List

The object instance list immediatly follows the header.

### Object Instance Entry

| Offset | Type | Description |
|--------|------|-------------|
| 0x0 | float[3] | Position |
| 0xC | float[3] | Rotation |
| 0x18 | float[3] | Scale |
| 0x24 | uint32 | PMO Offset |
| 0x28 | uint32 | Unknown pointer |
| 0x2C | uint16 | Object Flag |
| 0x2E | uint16 | Object ID |

Note that PMO Offset can be NULL.

## Texture List

The texture list is at Texture List Offset in the header. Note that it's uneccessary as each object's PMO also contains the offset to it's needed TM2s.

### Texture List Entry

The texture list entry is the same as the texture info structure in a PMO.

| Offset | Type | Description |
|--------|------|-------------|
| 0x0    | uint32 | TM2 Offset |
| 0x4    | char[0xC] | Texture Name |
| 0x10   | float | Animates texture in X axis |
| 0x14   | float | Animates texture in Y axis |
| 0x18   | int32[2] | Padding |
