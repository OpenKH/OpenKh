# SCD Format

SCD stands for *Sound Collection Data*.

This file is a container for many related sounds.

## Header
| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[8]   | File identifier, always `SEDBSSCF`. which stands for `Sound Environment DataBase ? ? ? ?`
| 0x4     | uint16  | File version `3`