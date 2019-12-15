# [Kingdom Hearts II](../../index) - Raw Texture

This is used by 3D models of Kingdom Hearts II to give them a texture.

The format holds low-level data to instruct the Playstation 2 where to store the texture in the VRAM and how the GPU should use it for rendering.

When GS is mentioned, it refers to the Playstation 2 GPU.

## Header

| Offset | Type   | Description
|--------|--------|-------------
| 0      | uint32 | Magic code. Always 0.
| 4      | int32  | Color count. How many individual colors are stored in the CLUT.
| 8      | int32  | [Texture info](#texture-info) count. 16 bytes aligned.
| 12     | int32  | [GS Info](#gs-info) count.
| 16     | int32  | [Offset data](#offset-data) count.
| 20     | int32  | [Texture info](#texture-info) offset.
| 24     | int32  | [GS Info](#gs-info) offset.
| 28     | int32  | Picture offset, where all the pixels are located. 128 bytes aligned.
| 32     | int32  | Palette offset, where the whole palette is located.

## Offset data

The offset data tells what Texture Info is associated to a specific GS Info.

The algorithm is `textureInfo = textureInfoTable[OffsetData[gsInfoIndex]]`.

## Texture info

This table is mostly unknown, but most of the data does not change anyway between textures. It is supposed to be used to interpret picture information on the process to upload them to the GS VRAM.

All the fields in the table are for unknown purpose unless specified.

| Offset | Type   | Descriptor
|--------|--------|------------
| 0      | int32  | Always 0x10000006
| 4      | int32  | Always 0
| 8      | int32  | Always 0x13000000
| 12     | int32  | Always 0x50000006
| 16     | int32  | Always 4
| 20     | int32  | Always 0x10000000
| 24     | int32  | Always 14
| 28     | int32  | Always 0
| 32     | int32  | Always 0
| 36     | int32  | 
| 40     | int32  | 
| 44     | int32  | Always 0
| 48     | int32  | Always 0
| 52     | int32  | Always 0
| 56     | int32  | Always 0x51
| 60     | int32  | Always 0
| 64     | int32  | 
| 68     | int32  | 
| 72     | int32  | Always 0x52
| 76     | int32  | Always 0
| 80     | int32  | Always 0
| 84     | int32  | Always 0
| 88     | int32  | Always 0x53
| 92     | int32  | Always 0
| 96     | int32  | 
| 100    | int32  | Always 0
| 104    | int32  | Always 0
| 108    | int32  | Always 0
| 112    | int32  | 
| 116    | int32  | Picture offset, where the texture is stored
| 120    | int32  | Always 0
| 124    | int32  | 
| 128    | int32  | 
| 132    | int32  | Always 0
| 136    | int32  | Always 0x13000000
| 140    | int32  | Always 0

## GS info

This table is also unknown, but the few information found points to describe how the pictures stored into GS VRAM are supposed to be used by GS.

| Offset | Type   | Descriptor
|--------|--------|------------
| 0      | int64  | Always 0x0000000010000008
| 8      | int64  | Always 0x5000000813000000
| 16     | int64  | Always 0x1000000000008007
| 24     | int64  | Always 14
| 32     | int64  | Always 0
| 40     | int64  | Always 0x3f
| 48     | int64  | 
| 56     | int64  | Always 0x34
| 64     | int64  | 
| 72     | int64  | Always 0x36
| 80     | int64  | 
| 88     | int64  | Always 0x16
| 96     | int64  | 
| 104    | int64  | Always 0x14
| 112    | int64  | [GS TEX0 register](../../common/tm2#gstex)
| 120    | int64  | 
| 128    | int64  | 
| 136    | int64  | Always 8
| 144    | int64  | Always 0x0000000060000000
| 152    | int64  | Always 0x0000000013000000
