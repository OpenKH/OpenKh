# [Kingdom Hearts: Birth By Sleep](./index.md) - BCD

Found inside map `.arc`s the details of much of the file format remain unkown, however it does contain the collision data for the map.

## Header

| Offset | Type | Description |
|--------|------|-------------|
| 0x0 | uint32 | Magic value. Always `"@BCD"`
| 0x4 | uint32 | Version number.
| 0x8 | uint32 | Collision Data Count
| 0xC | uint32 | Pointer to Collision Data
| 0x10 | uint32[9] | A whole bunch of unknowns
| 0x34 | uint16 | Vertex count
| 0x36 | uint16 | Face count
| 0x38 | uint32 | Vertex list offset
| 0x3C | uint32 | Face list offset
| 0x40 | uint32 | offset to unknown data
| 0x44 | uint32 | offset to unknown data
| 0x48 | uint32 | offset to unknown data
| 0x4C | uint32 | offset to unknown data

## Vertex List

`fvec4 vertices[header.vertex_count]` found at `vertex list offset`.

## Face List

List of `header.face_count` of the following structure, found at `face list offset`.

| Offset | Type | Description |
|--------|------|-------------|
| 0x0 | uint32 | Unknown.
| 0x4 | uint32 | Unknown.
| 0x8 | uint32 | Unknown.
| 0xC | uint32 | Unknown.
| 0x10 | int16 | Vertex index 1
| 0x12 | int16 | Vertex index 2
| 0x14 | int16 | Vertex index 3
| 0x16 | int16 | Vertex index 4 *
| 0x18 | int16 | Unknown
| 0x1A | int16 | Unknown
| 0x1C | int16 | Unknown
| 0x1E | int16 | Unknown
| 0x20 | uint32 | Unknown
| 0x20 | uint32 | Unknown, possibly flags
| 0x20 | uint32 | Unknown
| 0x20 | uint32 | Unknown

* Faces are assumed to be quads unless `Vertex Index 4` is `0xFF`/`-1`, in which case it's a triangle.

## Unknown data

The last 4 values in the header point to regions in the file after the Face List. Nothing is yet known about this data, except that it's size seems to be proportional to the complexity of the associated map. The current working theory is that it is some kind of tree structure for optimizing collision queries.

Somewhere in the unknown data in the face structure should be flags that indicate if a face is a wall or floor, or if it's a trigger, as the level exit triggers seem to be present in the collision data. How the game decides what to do when you walk into them is currently unknown.