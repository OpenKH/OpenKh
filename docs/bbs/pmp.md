# [Kingdom Hearts: Birth By Sleep](./index.md) - PMP

PMP files contain several [PMOs](./pmo.md) and their associated [textures](../common/tm2.md), which are used to render the static geometry of a map.

## PMP Header 

| Offset | Type | Description |
|--------|------|-------------|
| 0x0 | uint32 | Magic value. Always "PMP\0" (0x00504D50) |
| 0x4 | uint32[3] | Unknown. |
| 0x10 | uint16 | Object Count |
| 0x12 | uint16 | Unknown |
| 0x14 | uint32 | Unknown |
| 0x18 | uint16 | Unknown |
| 0x1C | uint16 | Texture Count |
| 0x1E | uint32 | Texture List Offset |

## Object List

The object list immediatly follows the header.

### Object List Entry

| Offset | Type | Description |
|--------|------|-------------|
| 0x0 | float[3] | Position |
| 0xC | float[3] | Rotation |
| 0x18 | float[3] | Scale |
| 0x24 | uint32 | PMO Offset |
| 0x28 | uint32 | Unknown |
| 0x2C | uint16 | Object Flags |
| 0x2E | uint16 | Unknown, possibly some kind of object ID |

Note that PMO Offset can be NULL.

## Texture List

The texture list is at Texture List Offset in the header. Note that it's uneccessary as each object's PMO also contains the offset to it's needed TM2s.

### Texture List Entry

The texture list entry is the same as the texture info structure in a PMO.

| Offset | Type | Description |
|--------|------|-------------|
| 0x0    | uint32 | TM2 Offset |
| 0x4    | char[0xC] | Texture Name |
| 0x10   | int32[4] | unknown |