# ABIPATTERN Format

AbiPattern stands for *Ability Pattern*.

This file stores a list of abilities. It's use is unknown, but it seems to be related to command melding.

This file is contained within `Menu/Camp.arc`


## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[8]   | File identifier, always `@BINABIP`.
| 0x8     | uint16  | Version, always `256`.
| 0xA     | uint16  | Size
| 0xC     | uint16  | Data Count
| 0xE     | uint16  | Padding
| 0x10    | uint8[16]  | Ability Chance Array

## AbiPattern Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16[8]   | Ability Kind Array