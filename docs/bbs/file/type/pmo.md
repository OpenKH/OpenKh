# [Kingdom Hearts: Birth By Sleep](./index.md) - Models

Found inside PMP files or as raw PMO files, BBS stores it's models in PMO blobs.

The format supports varying vertex types and has support for animation bones and two pass rendering.

## PMO Header

| Offset | Type | Description |
|--------|------|-------------|
| 0x0    | uint32 | Magic Value. Always "PMO\0" (0x004F4D50) |
| 0x4    | uint8  | Number |
| 0x5    | uint8  | Group |
| 0x6    | uint8  | Version |
| 0x7    | uint8  | Padding |
| 0x8    | uint8  | Texture Count |
| 0x9    | uint8  | Padding |
| 0xA    | uint16 | Flag |
| 0xC    | uint32 | Skeleton Offset |
| 0x10   | uint32 | Mesh Offset 0 |
| 0x14   | uint16 | Triangle Count |
| 0x16   | uint16 | Vertex Count |
| 0x18   | float  | Model Scale |
| 0x1C   | uint32 | Mesh Offset 1 |
| 0x20   | float[4][8] | Bounding Box |

## Texture Info

The Header is followed by `Texture Count` Texture Info records

| Offset | Type | Description |
|--------|------|-------------|
| 0x0    | uint32    | Offset of TM2 block if present in current file. |
| 0x4    | char[12] | Texture Name |
| 0x10   | float     | Tiling Speed X |
| 0x14   | float     | Tiling Speed Y |
| 0x18   | int32[2]  | Padding |

See also: [TIM2](../common/tm2.md)

## Mesh Sections

`Mesh Offset 0` and `Mesh Offset 1` both point to the start of a list of `Mesh Section Header`s. You keep reading headers + their verts until you encounter a 0 `Vertex Count`. This structure must be aligned on a 4 byte boundary.

| Offset | Type | Description |
|--------|------|-------------|
| 0x0    | uint16   | Vertex Count |
| 0x2    | int8     | Texture ID, an index into the list of `Texture Info` records 
| 0x3    | uint8    | Vertex Size, in bytes
| 0x4    | uint32   | [Vertex Flags](#vertex-flags), see below 
| 0x8    | uint8    | Group 
| 0x9    | uint8    | Triangle Strip count 
| 0xA    | uint16   | [Vertex Attribute](#Vertex-Attribute) 
| 0xC    | uint8[8] | List of bone indices that affect this section. Only present if the PMO blob contains a skeleton (ie: `header.skeletonOffset != 0`) 
| varies | uint32 | Diffuse Color. Only present if `vertexFlags.DiffuseColor & 1` 

The section header is followed by `vertexCount * vertexSize` bytes of vertex data. The next Section header follows on the next 4-byte alignment after that.

### Vertex flags

The 24 least significant bits of this structure correspond to the PSP's [GE VTYPE command](http://hitmen.c02.at/files/yapspd/psp_doc/chap11.html#sec11.5.15).
The Primitive type bits correspond to the primitive type bits of the PSP's [GE PRIM command](http://hitmen.c02.at/files/yapspd/psp_doc/chap11.html#sec11.5.3).

| Bit | Count | Description |
|-----|-------|-------------|
|  0 | 2 | [Texture Coordinate Format](#coordinate-format) |
|  2 | 3 | [Color Format](#color-format) |
|  5 | 2 | Normal Format (Not used by BBS) |
|  7 | 2 | [Position Format](#coordinate-format) |
|  9 | 2 | [Weight Format](#coordinate-format) |
| 11 | 2 | Indices Format (Not used by BBS) |
| 13 | 1 | Unused |
| 14 | 3 | Skinning Weights Count |
| 17 | 1 | Unused |
| 18 | 3 | Number of morphing weights (Not used by BBS) |
| 21 | 2 | Unused |
| 23 | 1 | Skip Transform Pipline (Not used by BBS) |
| 24 | 1 | Uniform Diffuse Flag |
| 25 | 3 | Unknown, possibly unused |
| 28 | 4 | Primitive Type |             

### Coordinate Format
Texture Format, Normal Format, Position Format and Weight Format are as follows:

| Value | Meaning |
|-------|---------|
| 0x0 | Not Present In Vertex |
| 0x1 | 8-bit normalized |
| 0x2 | 16-bit normalized |
| 0x3 | 32-bit float |

Weight format has only been observed to use 8-bit fixed in actual PMO blobs.

### Color Format

Color Format is as follows:

| Value | Meaning |
|-------|---------|
| 0x0 | Not Present In Vertex |
| 0x4 | 16-bit BGR-5650 |
| 0x5 | 16-bit ABGR-5551 |
| 0x6 | 16-bit ABGR-4444 |
| 0x7 | 32-bit ABGR-8888 |

Only 32-bit ABGR-8888 has been observed in use by actual PMO blobs. If the Uniform Diffuse Flag is set, this value should be 0.

The number of skinning/morphing weights to be read is actually the value in the bit field + 1. If the weight format field is 0, these fields should be ignored.

Index Format has not been seen in actual PMO blobs, but if it is used the expected format is as follows:

| Value | Meaning |
|-------|---------|
| 0x0 | Not using indices |
| 0x1 | 8-bit |
| 0x2 | 16-bit |

Uniform Diffuse Flag indicates that all vertices in the section should use the same vertex color, which follows the header before the vertex data. See header structure above.

Primitive Type is as follows:

| Value | Meaning |
|-------|---------|
| 0x0 | Points |
| 0x1 | Lines |
| 0x2 | Line Strips |
| 0x3 | Triangles |
| 0x4 | Triangle Strips |
| 0x5 | Triangle Fans |
| 0x6 | Sprites (Quads) |

Only Triangles and Triangle Strips have been observed in actual PMO blobs.

### Decoding Verts

Once the header has been read, along with any optional fields present, the vertex data immediatly follows.
Each vertex follows the format described by the vertexFlags field.
The vertex properties are written in the following order. Note that only position is mandatory.

| Name |
|------|
| Joint Weights |
| Texture Coords |
| Color |
| Position |

### Reading 'Normalized' values

Vertex properties which are in the format `8-bit normalized` or `16-bit normalized` need to be converted to floating point values before rendering. Note that position values should read as **signed** values, the others as **unsigned** values. The conversion is as follows:

```c++
	// uint8
	float value = (float)data / 127.0f;
	// uint16
	float value = (float)data / 32767.0f;
```

### Vertex Attribute

This field defines what kind of primitive is being rendered.

| Value    |       Name           |      Meaning     |
|----------|----------------------|------------------|
| 0        | ATTR_BLEND_NONE      |                  
| 1        | ATTR_NOMATERIAL      |                  
| 2        | ATTR_GLARE           |                  
| 4        | ATTR_BACK            |                  
| 8        | ATTR_DIVIDE          |                  
| 16       | ATTR_TEXALPHA        | Specifies that the texture used contains alpha values for transparency 
| 24       | FLAG_SHIFT           |                  
| 28       | PRIM_SHIFT           |                  
| 32       | ATTR_BLEND_SEMITRANS | Specifies that the material must be blended as semitransparent     
| 64       | ATTR_BLEND_ADD       |                  
| 96       | ATTR_BLEND_SUB       |                  
| 224      | ATTR_BLEND_MASK      |                  
| 256      | ATTR_8               |                  
| 512      | ATTR_9               |                  
| 1024     | ATTR_DROPSHADOW      |                  
| 2048     | ATTR_ENVMAP          |                  
| 4096     | ATTR_12              |                  
| 8192     | ATTR_13              |                  
| 16384    | ATTR_14              |                  
| 32768    | ATTR_15              |                  
| 16777216 | FLAG_COLOR           |                  
| 33554432 | FLAG_NOWEIGHT        |     

## Skeleton Header

| Offset | Type | Description |
|--------|------|-------------|
| 0x0    | uint32 | Magic Value. Always "BON\0" |
| 0x4    | uint32 | padding |
| 0x8    | uint16 | Bone Count |
| 0xA    | uint16 | padding |
| 0xC    | uint16 | Skinned Bones |
| 0xE    | uint16 | Skinned Bones Initial Index |

### Joint Definition

| Offset | Type | Description |
|--------|------|-------------|
| 0x0 | uint16 | Bone Index |
| 0x2 | uint16 | Padding |
| 0x4 | uint16 | Parent Bone Index - 0xFFFF indicates no parent|
| 0x6 | uint16 | Padding |
| 0x8 | uint16 | Skinning Index |
| 0xA | uint16 | Padding |
| 0xC | uint32 | Padding |
| 0x10 | char[16] | Bone Name |
| 0x20 | Matrix4x4 | Transform |
| 0x60 | Matrix4x4 | Inverse Transform |
