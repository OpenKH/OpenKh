# [Kingdom Hearts II](../index.md) - Raw Texture

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
| 112    | int64  | [GS TEX0 register](../../common/tm2.md#gstex)
| 120    | int64  | [Texture Wrap Mode](#texture-wrap-mode)
| 128    | int64  | 
| 136    | int64  | Always 8
| 144    | int64  | Always 0x0000000060000000
| 152    | int64  | Always 0x0000000013000000

## Texture Wrap Mode

In PlayStation 2 graphics, texture wrapping does not only occur when UV are `<0.0` or `>1.0`, but it is possible to define a custom wrap box. This is done by the Texture Wrap register.

| Bit | Count | Name | Description
|-----|-------|------|------------
| 0   | 2     | WMS  | Defines the [wrap mode](#wrap-mode) for the U coordinate
| 2   | 2     | WMT  | Defines the [wrap mode](#wrap-mode) for the V coordinate
| 4   | 10    | MINU | Lower clamp value for U coordinate
| 14  | 10    | MAXU | Upper clamp value for U coordinate
| 24  | 10    | MINU | Lower clamp value for V coordinate
| 34  | 10    | MAXU | Upper clamp value for V coordinate
| 44  | 20    |      | not used

MINx and MAXx are only applied if the [wrap mode](#wrap-mode) is of type `REGION`.

## Wrap Mode (also called WMS and WMT)

| Value | Description
|-------|------------
| 0     | REPEAT
| 1     | CLAMP
| 2     | REGION_REPEAT
| 3     | REGION_CLAMP

## Texture Animation Metadata

Includes parameters for animating textures. This section appears at the end of the file.

The main header does not directly provide an offset to this metadata. Instead, this offset is calculated as the sum of the CLUT data offset and the total byte size of CLUT data.

Each block begins with a 4-byte tag and an optional byte size field, followed by the data itself.

| Offset | Type    | Descriptor
|--------|---------|------------
| 0      | char[4] | Tag name. See below sub-sections for possible tags.
| 4      | uint32  | Total byte size of data. Present for all tags except _KN5.
| 8      |         | Data. Varies by tag.

### UVSC (UV Scroll)

Provides a single pair of U and V speed factors for texture scrolling. Entries in the VIF table for the model may apply scrolling by specifying an index corresponding with a UVSC entry.

| Offset | Type    | Descriptor
|--------|---------|------------
| 0      | char[4] | "UVSC"
| 4      | uint32  | Total byte size of data. Always `0xC`.
| 8      | uint32  | Index of entry.
| 12     | float   | U scroll speed.
| 16     | float   | V scroll speed.

### TEXA (Texture Animation)

Provides parameters and image data for rendering sprite animations for a single texture. Consists of one or more frame tables for animations as well as raw image data for individual sprites.

Most of the GS register fields used to upload the base image texture, including the base pointer, buffer width and pixel storage format, are used to upload sprite image data for the active animation frame to GS memory (see [texture info](#texture-info)). The pixel offset and size of the transmission area (dsax, dsay, rrw, rrh) are set such that the upload overwrites the base image at the given region with image data for the sprite. All sprites under a single TEXA tag are the same size and pixel format, which means that these parameters are constant across all animations.

Image data for sprites is stored contiguously. Starting offsets for each image are calculated using a sprite index as well as the sprite dimensions and bits per pixel specified in the tag header.

| Offset | Type    | Descriptor
|--------|---------|------------
| 0      | char[4] | "TEXA"
| 4      | uint32  | Total byte size of data.
| 8      | uint16  | 
| 10     | uint16  | Texture index to apply the animation.
| 12     | uint16  | Frame stride in halfwords for entries in the [frame table](#frame-table-entry).
| 14     | uint16  | Bits per pixel of sprite image data.
| 16     | uint16  | Base slot index. Applies to entries in the [slot table](#animation-slot-table).
| 18     | uint16  | Maximum slot index. Applies to entries in the [slot table](#animation-slot-table).
| 20     | uint16  | Number of animations.
| 22     | uint16  | Number of sprites in sprite image data.
| 24     | uint16  | U offset in base image in pixels (dsax).
| 26     | uint16  | V offset in base image in pixels (dsay).
| 28     | uint16  | Sprite width in pixels (rrw).
| 30     | uint16  | Sprite height in pixels (rrh).
| 32     | uint32  | Offset of [slot table](#animation-slot-table).
| 36     | uint32  | Offset of [animation table](#animation-table-entry).
| 40     | uint32  | Offset of sprite image data.
| 44     | uint32  | Default animation index (idle).

#### Animation Slot Table

This table contains animation slots where each slot may optionally be assigned an animation index. An external source can enable one of these slots to trigger a certain animation for the texture. The exact interaction with these external files is currently unknown.

| Offset | Type   | Descriptor
|--------|--------|------------
| 0      | uint32 | Animation index assigned to the slot (actual index is `value - 1`). If `0`, the slot is empty.

#### Animation Table Entry

| Offset | Type   | Descriptor
|--------|--------|------------
| 0      | uint32 | Offset of first frame in the [frame table](#frame-table-entry).

#### Frame Table Entry

| Offset | Type         | Descriptor
|--------|--------------|------------
| 0      | uint16 : 0-3 | Frame control. <br> `0`: Enable sprite <br> `1`: Disable sprite (use the base image) <br> `2`: Jump to given frame offset (loop) <br> `3`: Stop the animation
| 0      | int16 : 4-15 | Loop offset in number of frames. Usually < 0 if present.
| 2      | uint16       | Minimum length of frame.
| 4      | uint16       | Maximum length of frame.
| 6      | uint16       | Sprite image index.

If minimum length < maximum length, a random number between [minimum, maximum] is selected as the length the frame. Random intervals are used in cases such as blinking animations for characters.

### _DMY

Dummy tag used for padding.

| Offset | Type    | Descriptor
|--------|---------|------------
| 0      | char[4] | "_DMY"
| 4      | uint32  | Total byte size of data. Always `0`.

### _KN5

Indicates end of metadata.

| Offset | Type    | Descriptor
|--------|---------|------------
| 0      | char[4] | "_KN5"
