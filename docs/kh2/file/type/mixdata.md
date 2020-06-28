# [Kingdom Hearts II](../../index) - mixdata.bar

This file contains informations about the moogle shop. Internally it is a [bar](bar.md) file.

* [Sub-file Header](#header)
* Entries
    * [RECI](#reci)
    * [COND](#cond)
    * [LEVE](#leve)
    * [EXP](#exp)

## Header

Each sub-file starts with a header. The structure for all is the same, aside from the Magic Code.

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | int    | Magic Code
| 04     | int    | Unknown. Seems to be related to the game version
| 08     | int    | Entries count
| 0C     | int    | Padding

## Entries

## Reci

Contains the moogle recipes.

Magic Code `MIRE`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | [Item ID of Recipe](./03system.md#item)
| 02     | byte   | 
| 03     | byte   |
| 04     | ushort | Obtained Item
| 06     | ushort | Upgraded Item (e.g Plus Accessory)
| 08     | ushort | Ingredient 1
| 0A     | ushort | Needed amount of Ingredient 1
| 0C     | ushort | Ingredient 2
| 0E     | ushort | Needed amount of Ingredient 2
| 10     | ushort | Ingredient 3
| 12     | ushort | Needed amount of Ingredient 3
| 14     | ushort | Ingredient 4
| 16     | ushort | Needed amount of Ingredient 4
| 18     | ushort | Ingredient 5
| 1A     | ushort | Needed amount of Ingredient 5
| 1C     | ushort | Ingredient 6
| 1E     | ushort | Needed amount of Ingredient 6

## Cond

Contains the table from Synthesize -> Lists (Get n types of materials etc.)

Magic Code `MICO`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | Id
| 02     | short  | Reward (either an Item or a shop upgrade)
| 04     | byte   |
| 05     | byte   |
| 06     | byte   |
| 07     | byte   |
| 08     | short  | Count of needed Materials
| 0A     | short  |


## Leve

Contains moogle level up informations.

Magic Code `MILV`

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | ushort | Id
| 02     | ushort |
| 04     | ushort | 
| 06     | ushort |
| 08     | int    | EXP needed


## Exp