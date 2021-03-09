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
| 0x2     | uint16   | Size of Sound Entry Offset Table
| 0x4     | uint16   | Table 2 Offset Size
| 0x6     | uint16   | Unknown
| 0x8     | int32    | Offset to Table 0
| 0xC     | int32    | Sound Entry Table Offset
| 0x10    | int32    | Offset to Table 2
| 0x14    | int32    | Unknown
| 0x18    | int32    | Unknown Offset
| 0x1C    | int32    | Padding

## Stream

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