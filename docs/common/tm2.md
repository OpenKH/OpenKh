# TM2 (PlayStation 2 texture format) - Back to [Index](../index.md)

As the title suggests, this is the texture format used by PlayStation 2 games, not only Kingdom Hearts.

It contains information like GS hardware registers and where the texture will be allocated in the VRAM.

TM2 is a container of pictures, where every picture can optionally have a Color Look-Up Table or multiple levels of mipmaps. The way how the mipmaps works are not yet fully documented here.

## File structure

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | MagicCode, always 'TIM2'
| 04     | byte  | File format revision. Usually 4.
| 05     | byte  | Format. Usually 0.
| 06     | short | Picture count
| 08     | int   | RESERVED
| 0c     | int   | RESERVED
| 10     | [Picture*](#picture) | List of pictures

When a game engine wants to pick a specific picture, it should move through the memory from the first picture header, using then `TotalSize` to move to the next one.

eg.
```
Picture* GetPicture(Picture* firstPicture, int index) {
    off_t offset = (off_t)firstPicture;
    while (index-- > 0)
        offset += ((Picture*)offset)->TotalSize;

    return (Picture*)offset;
}
```

### Picture

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Total size in bytes used by the picture
| 04     | int   | Clut size in bytes used by the palette
| 08     | int   | Image size in bytes used by the bitmap
| 0c     | short | Header size
| 0e     | short | Number of colors used by the clut
| 10     | byte  | Picture format
| 11     | byte  | Mipmap count
| 12     | byte  | [Clut color type](#color-type)
| 13     | byte  | [Image color type](#color-type)
| 14     | short | Image width in pixels
| 16     | short | Image height in pixels
| 18     | long  | [GsTex register](#gstex)
| 20     | long  | [GsTex register](#gstex)
| 28     | int   | [Gs flags register](#gsreg)
| 2c     | int   | [Gs clut register](#gsclut)

Right after the picture header structure, if Mipmap count is greater than 1 then a [Mipmap structure](#mipmap) is found.

After the optional Mimap, there is a bitmap with the size specified by the picture header. Then finally an optional palette, also with the size in byte specified by the picture header.

### Mipmap

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Miptbp register
| 04     | int   | Miptbp register
| 08     | int   | Miptbp register
| 0c     | int   | Miptbp register
| 10     | int[8] | Array of sizes

The purpose of those fields is currently unknown.

## Registers

### GsTex

| Bit | Count | Name | Description
|-----|-------|------|------------
| 0   | 14    | TBP0 | Texture buffer location. Multiply it by 0x100 to get the raw VRAM pointer.
| 14  | 6     | TBW  | Texture buffer width
| 20  | 6     | PSM  | [Pixel storage format](#psm-register-pixel-storage-mode)
| 26  | 4     | TW   | log2(texture width)
| 30  | 4     | TH   | log2(texture height)
| 34  | 1     | TCC  | 1 if the texture or the clut contains an alpha channel.
| 35  | 2     | TFX  | [Texture function](#tfx-register-texture-function)
| 37  | 14    | CBP  | Clut buffer location. Multiply it by 0x100 to get the raw VRAM pointer.
| 51  | 4     | CPSM | [Clut storage format](#cpsm-register-color-look-up-pixel-storage-mode)
| 55  | 1     | CSM  | [Clut Storage mode](#csm-register-color-storage-mode)
| 56  | 5     | CSA  | Clut Entry Offset. Mostly used by 4-bit images.
| 61  | 3     | CLD  | Load control. Purpose unknown.

### GsReg

Undocumented.

### GsClut

Undocumented.

## Types

### PSM register (Pixel Storage Mode)

Defines how pixel are arranged in each 32-bit word of local memory.

| Value | Name     | Description
|-------|----------|-------------
| 0     | PSMCT32  | RGBA32, uses 32-bit per pixel.
| 1     | PSMCT24  | RGB24, uses 24-bit per pixel with the upper 8 bit unused.
| 2     | PSMCT16  | RGBA16 unsigned, pack two pixels in 32-bit in little endian order.
| 10    | PSMCT16S | RGBA16 signed, pack two pixels in 32-bit in little endian order.
| 19    | PSMT8    | 8-bit indexed, packing 4 pixels per 32-bit.
| 20    | PSMT4    | 4-bit indexed, packing 8 pixels per 32-bit.
| 27    | PSMT8H   | 8-bit indexed, but the upper 24-bit are unused.
| 26    | PSMT4HL  | 4-bit indexed, but the upper 24-bit are unused.
| 44    | PSMT4HH  | 4-bit indexed, where the bits 4-7 are evaluated and the rest discarded.
| 48    | PSMZ32   | 32-bit Z buffer
| 49    | PSMZ24   | 24-bit Z buffer with the upper 8-bit unused
| 50    | PSMZ16   | 16-bit unsigned Z buffer, pack two pixels in 32-bit in little endian order.
| 58    | PSMZ16S  | 16-bit signed Z buffer, pack two pixels in 32-bit in little endian order.

### CPSM register (Color look-up Pixel Storage Mode)

| Value | Name     | Description
|-------|----------|-------------
| 0     | PSMCT32  | 32-bit color palette
| 1     | PSMCT24  | 24-bit color palette
| 2     | PSMCT16  | 16-bit color palette
| 10    | PSMCT16S | 16-bit color palette

### CSM register (Color Storage Mode)

There are two possible storage modes:

* `CSM1`: The pixels are stored and swizzled every 0x20 bytes. This option is faster for PS2 rendering.
* `CSM2`: The pixels are stored sequencially. But the PS2 GPU uses the CLUT slower.

### Color type

| Value | Description
|-------|------------
| 0     | Undefined
| 1     | 16-bit RGBA (A1B5G5R5)
| 2     | 32-bit RGB (X8B8G8R8)
| 3     | 32-bit RGBA (A8B8G8R8)
| 4     | 4-bit indexed
| 5     | 8-bit indexed

### TFX register (Texture function)

| Value | Description
|-------|------------
| 0     | Modulate
| 1     | Decal
| 2     | Hilight
| 3     | Hilight 2
