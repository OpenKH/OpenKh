# [Kingdom Hearts II](../../index.md) - DOCT

DOCT is a [occlusion culling](https://en.wikipedia.org/wiki/Hidden-surface_determination#Occlusion_culling) definition.

The occlusion culling is useful to shorten 3D rendering time. For example, buildings behinds you can be ommitted from rendering.

DOCT uses only bounding boxes to represent the areas that are holding mesh data (VIF packets) to be rendered.

## DOCT Structure

### BAR Header

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | char[4] | The identifier of the file (Should be always 0x54434F44) |
| 4 | uint32_t | Version: always `2`
| 8 | uint32_t | Unknown
| 12 | uint32_t | HeaderOffset: always `0`
| 16 | uint32_t | HeaderLength: always `0x2C`
| 20 | uint32_t | Entry1FirstOffset
| 24 | uint32_t | Entry1TotalLength
| 28 | uint32_t | Entry2FirstOffset
| 32 | uint32_t | Entry2TotalLength
| 36 | uint32_t | Entry3FirstOffset
| 40 | uint32_t | Entry3TotalLength

### Table1 (Array of entry1)

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | int16_t | Child1
| 2 | int16_t | Child2
| 4 | int16_t | Child3
| 6 | int16_t | Child4
| 8 | int16_t | Child5
| 10 | int16_t | Child6
| 12 | int16_t | Child7
| 14 | int16_t | Child8
| 16 | float32 | MinX
| 20 | float32 | MinY
| 24 | float32 | MinZ
| 28 | float32 | MaxX
| 32 | float32 | MaxY
| 36 | float32 | MaxZ
| 40 | uint16_t | Entry2Index
| 42 | uint16_t | Entry2LastIndex
| 44 | uint32_t | Unknown

_Note :_ ChildX: `-1` to mean nothing to point.

_Note :_ (X,Y,Z) are inverted (-X,-Y,-Z).


### Table2 (Array of entry2)

| Offset | Variable Type | Description |
|--------|---------------|-------------|
| 0 | uint32_t | Flags
| 4 | float32 | MinX
| 8 | float32 | MinY
| 12 | float32 | MinZ
| 16 | float32 | MaxX
| 20 | float32 | MaxY
| 24 | float32 | MaxZ

_Note :_ (X,Y,Z) are inverted (-X,-Y,-Z).
