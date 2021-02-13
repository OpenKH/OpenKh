# BGM Format

BGM stands for *BackGround Music*.

This file contains the list of all songs used in the game and their identifiers.

This file is within `arc/system/common.arc` in the subfile `bgm.bin`.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Version, always `1`.
| 0x4     | uint32   | Music Count

## BGM Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Index
| 0x2     | uint16   | Size
| 0x4     | char[12]   | Name