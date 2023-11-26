# [Kingdom Hearts II](../../index.md)
# SCD Format

SCD stands for *Sound Container Data*.

This file is a container for many related sounds.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[8]   | File identifier, always `SEDBSSCF`. which stands for `Sound Environment DataBase ? ? ? ?`
| 0x8     | uint32  | File version, always `3`.
| 0xC     | uint8   | Endianness (0 = LE, 1 = BE)
| 0xD     | uint8   | SSCF version. Always `0x400`.
| 0xE     | uint16  | Header Size
| 0x10    | uint32  | Total File Size
| 0x14    | uint32[7]  | Padding

## Table Offset Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Table 0 Offset Size
| 0x2     | uint16   | Size of Sound Entry Offset Table; this corresponds to the number of sounds within the SCD.
| 0x4     | uint16   | Table 2 Offset Size
| 0x6     | uint16   | Unknown
| 0x8     | int32    | Offset to Table 0
| 0xC     | int32    | Sound Entry Table Offset
| 0x10    | int32    | Offset to Table 2
| 0x14    | int32    | Unknown
| 0x18    | int32    | Unknown Offset; must be the same offset as the first offset found in Table 2.
| 0x1C    | int32    | Padding

Immediately after Table Offset Header is another table of offsets, not pointed to in the header. </br>
This table of offsets points to offsets that map sounds in the SCD to ID numbers used by the game.</br>
The size of this table seems to be determined by Table 0 Offset Size.</br>
This is referred to here as a Sound Mapping Table. </br>

## Sound Mapping Table
| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint8    | Unknown; seems to be set to 1 if a sound effect is mapped, and 0 if not?
| 0x1     | uint8    | Unknown; seems to be set to 2 if a sound effect is mapped, and 1 if not?
| 0x2     | uint8    | Unknown
| 0x3     | uint8    | Unknown
| 0x4     | uint16   | Unknown
| 0x6     | uint16   | Unknown
| 0x8     | float    | Volume to play at
| 0xC     | int32    | Sound Effect: Index in Game
| 0x10    | int16    | Sound Effect: Index in SCD?
| 0x12    | int16    | Sound Effect: Index in SCD

This table has entries of two different sizes, 0x10 and 0x14. </br>
An example of this can be seen with how se999 loads its sound effects. While se999 only has 96 sound effects found inside</br>
on both PC and PS2, it uses indexes up to 154, with some indexes like 60 through 70 being left blank. </br>
Dummy entries that are only 0x10 long are used to pad sound effect indexes between numbers that are used and have sounds in the SCD for them.</br>
Otherwise, entries that are 0x14 long are used where the last 4 bytes are used to map sounds in the SCD to the specified index.


## Table 0
First entry seems to be 0x58 long. </br>
All entries after seem to be 0x62 long. </br>

## Sound Entry Table
| Offset | Type  | Description
|--------|-------|------------
| 0x0     | Unknown[0x4F]   | Unknown
| 0x50    | uint32   | Audio Length (in ms)
| 0x54    | uint32   | Unknown

Most data seems to be identical here, except for 0x50. </br>
That int determines how long to play the sound effect for in-game, before cutting the audio off. </br>

## Stream (pointed to by SoundEntry Table)
| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Stream Size
| 0x4     | uint32   | Channel Count
| 0x8     | uint32   | Sample Rate
| 0xC     | uint32   | Codec
| 0x10    | uint32   | Loop Start
| 0x14    | uint32   | Loop End
| 0x18    | uint32   | Extra Data Size
| 0x1C    | uint32   | Auxiliary Chunk Count
| 0x20    | uint8[StreamSize]   | Data

## Table 2
Entries are 0x80 long.
