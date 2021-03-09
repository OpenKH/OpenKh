# SCD Format

SCD stands for *Sound Collection Data*.

This file is a container for many related sounds.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[8]   | File identifier, always `SEDBSSCF`. which stands for `Sound Environment DataBase ? ? ? ?`
| 0x8     | uint32  | File version, always `3`.
| 0xC     | uint8   | Endianness (0 = LE, 1 = BE)
| 0xD     | uint8   | SSCF version
| 0xE     | uint16  | Tables Offset

## Stream Header

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