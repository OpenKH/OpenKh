# [Kingdom Hearts II](../../index) - 15jigsaw.bin

This file is exclusive to the Final Mix releases and it defines which puzzle piece you get when you pick up a crown.

## Header

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | int    | Id
| 02     | int    | Count
| 04     | Entry[Count]  | Entries

## Entry

| Offset | Type   | Description |
|--------|--------|-------------|
| 00     | byte   | [Picture](jiminy.md#puzz)
| 01     | byte   | Piece
| 02     | ushort | Text which gets displayed when picking up
| 04     | byte   | [World](../../worlds.md)
| 05     | byte   | Room
| 06     | byte   | World Piece Id 
| 07     | byte   | 
| 08     | ushort | 