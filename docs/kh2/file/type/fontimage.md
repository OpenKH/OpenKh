# [Kingdom Hearts II](../../index.md) - Font and icon sprite images

fontimage.bar:

Tag   | Type      | Purpose
------|-----------|------------------
`sys` | RawBitmap | System text font
`evt` | RawBitmap | Event text font
`icon`| RawBitmap | Icon sprite

## `sys` / `evt` bitmap image format

4 bpp bitmap with fixed *width* of 512 pixel.

*Height* is calculated by the formula: `(FileSize / 256)`.

Two 2bpp bitmaps are combined into single 4bpp bitmap.

First bitmap. Pixel order is: `LL HH`

|Bit| 7  | 6  | 5  | 4  | 3  | 2  | 1  | 0  |
|---|:-: |:-: |:-: |:-: |:-: |:-: |:-: |:-: |
|   | -- | -- | HH | HH | -- | -- | LL | LL |

Second bitmap. Pixel order is: `LL HH`

|Bit| 7  | 6  | 5  | 4  | 3  | 2  | 1  | 0  |
|---|:-: |:-: |:-: |:-: |:-: |:-: |:-: |:-: |
|   | HH | HH | -- | -- | LL | LL | -- | -- |

Color palette is 4 colors gray palette like this:

 R | G | B
--:|--:|--:
  0|  0|  0
 85| 85| 85
170|170|170
255|255|255

## `icon`

Sprite image of: 8 bpp bitmap (256 x 160) + 256 colors palette.

File layout:

```
0000h: 8bpp bitmap (256 x 160)
A000h: 256 colors palette (RR GG BB Ps2Alpha)
A400h: EOF

```
