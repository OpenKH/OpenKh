# TM2 - PlayStation 2 texture format

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
| 20  | 6     | PSM  | [Pixel storage format](#psm)
| 26  | 4     | TW   | log2(texture width)
| 30  | 4     | TH   | log2(texture height)
| 34  | 1     | TCC  | 1 if the texture or the clut contains an alpha channel.
| 35  | 2     | TFX  | [Texture function](#texture-function)
| 37  | 14    | CBP  | Clut buffer location. Multiply it by 0x100 to get the raw VRAM pointer.
| 51  | 4     | CPSM | [Clut storage format](#cpsm)
| 55  | 1     | CSM  | Storage mode. Purpose unknown.
| 56  | 5     | CSA  | Offset. Purpose unknown.
| 61  | 3     | CLD  | Load control. Purpose unknown.

### GsReg

Undocumented.

### GsClut

Undocumented.

## Types

### PSM

| Value | Description
|-------|------------
| 0     | PSMCT32
| 1     | PSMCT24
| 2     | PSMCT16
| 10    | PSMCT16S
| 19    | PSMT8
| 20    | PSMT4
| 27    | PSMT8H
| 26    | PSMT4HL
| 44    | PSMT4HH
| 48    | PSMZ32
| 49    | PSMZ24
| 50    | PSMZ16
| 58    | PSMZ16S

### CPSM

| Value | Description
|-------|------------
| 0     | PSMCT32, 32-bit color palette
| 1     | PSMCT24, 24-bit color palette
| 2     | PSMCT16, 16-bit color palette
| 10    | PSMCT16S, 16-bit color palette

### Color type

| Value | Description
|-------|------------
| 0     | Undefined
| 1     | 16-bit RGBA (A1B5G5R5)
| 2     | 32-bit RGB (X8B8G8R8)
| 3     | 32-bit RGBA (A8B8G8R8)
| 4     | 4-bit indexed
| 5     | 8-bit indexed

### Texture function

| Value | Description
|-------|------------
| 0     | Modulate
| 1     | Decal
| 2     | Hilight
| 3     | Hilight 2
