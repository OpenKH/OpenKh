# [Kingdom Hearts Birth By Sleep](index.md) - Font archive

At the path `arc/system/`, the two files `font.arc` (Japanese font) and `fonten.arc` (European font) can be found. Those files are nothing more than an [archive](arc.md) with all the information needed to render a character or a font image.

## Font types

The following font types can be found in both font archives:

* `cmdfont`, used for the command HUD at the bottom left.
* `helpfont`, 
* `menufont`, used inside the menu.
* `mesfont`, used for game dialogs. This is the most complete font.
* `numeral`, it just contains numbers between 0 and 9.
* `fonticon`, stores console button and game icons.

## Character mapping

Internally, the game engine uses UCS as encoding, meaning that all the Shift-JIS text is ingested and converted on-the-fly to 2-bytes for each character. Then it does a look-up to both [INF for FontIcon](#inf---fonticon-variant) and [COD](#cod) to match and print that specific character by searching it using the Character ID.

Embedded in the executable, a hard-coded switch table is responsible to convert the characters from `0x21` (`'!'`) to `0x7E` (`~`) into a UCS character.

## INF

Describes the [texture](#mtx) of a font.

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | How many characters the texture contains.
| 02     | short | Texture width.
| 04     | short | Texture height.
| 06     | byte  | Character width.
| 07     | byte  | Character height.

## INF - FontIcon variant

For `fonticon`, the associated INF file is structured in a completely different way.

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Character ID
| 02     | byte  | Left
| 03     | byte  | Top
| 04     | byte  | Right
| 05     | byte  | Bottom
| 06     | short | Always 0

Left, Top, Right and Bottom are used to locate the icon in the [texture](#mtx)

## MTX

A 4-bit image (8-bit for `fonticon`) that stores all the characters in a grid. The texture is swizzled by 256 bytes, which means that the content is stored by 32x16 pixel blocks at the time (16x16 for `fonticon` since it is in 8-bit).

The texture size will be smaller than `width * height * bpp / 8` bytes long since it is swizzled. That means that the image content at the bottom-right does not contain any data since the 32x16 pixel blocks are not needed to be stored.

Since the colors of each pixel is indexed, a [color look-up table](#clu) is provided.

## CLU

A `1024 bytes` long file that stores one or more palettes for a given [MTX](#mtx).

For 4-bit MTXs, this file will contain up to 4 different palettes of 16 colors, but is most likely that only the first two are actually used.

For font files, switching between palette 1 and palette 2 will reveal two different contents from the same bitmap, a technique used in Kingdom Hearts II font files too.

For 8-bit MTXs, the file will be considered as a single palette of 256 colors.

The palette format is RGBA (bytes are stored as `0xRR 0xGG 0xBB 0xAA`).

## COD

Provides the necessary info to print a given character.

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Character ID.
| 02     | short | Position X.
| 04     | short | Position Y.
| 06     | byte  | Palette index.
| 07     | byte  | Actual character width.

The Character ID is not necessarly unique, for some weird reason. It is used to associate a specific font's texture area to a given Shift-JIS character.
