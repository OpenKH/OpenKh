# [Kingdom Hearts II](../../index.md) - 00place.bin and place.bin

There are two versions of this file: `00place.bin` stored in the root director, and `place.bin` stored in `msg/{language}/`.

They share the same structure, although only `place.bin` is used in the final game. `00place.bin` seems to contain references of early stages of development and it might have been used for in-game debug functionalities, on top of embedded names of most maps.

Both files references to `zz` maps, which are not found in the final game. More information regarding worlds and maps can be found [here](../../worlds.md).

## File structure

A place file is a [BAR](bar.md) which contains binary files for each world. The structure described here is a single place entry.

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint16 | [Message ID](msg.md)
| 4      | uint16 | Map name relative offset

There is an array of places, where the length of it is not defined anywhere. But the map name offset of the first place entry always points to the end of the array.

The map name is stored only in `00place.bin` and not in `place.bin` and it contains the name of the map in Japanese, encoded using Shift-JIS. The offset is relative, meaning that it starts from the place entry and not from the beginning of the file.
