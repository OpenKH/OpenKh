# ITC Format

ITC stands for *ITem Collection* and it seems to contain the list of items obtained through collectibles. In this case, just the crowns for stickers.

Located in the `ITEM` folder.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `ITC`. Null terminated.
| 0x4     | uint16   | Version, `1`
| 0x6     | uint16   | Padding
| 0x8     | uint16   | Count of total items
| 0xA     | uint16   | Padding
| 0xC     | uint8   | Item Count in `DP`
| 0xD     | uint8   | Item Count in `SW`
| 0xE     | uint8   | Item Count in `CD`
| 0xF     | uint8   | Item Count in `SB`
| 0x10    | uint8   | Item Count in `YT`
| 0x11    | uint8   | Item Count in `RG`
| 0x12    | uint8   | Item Count in `JB`
| 0x13    | uint8   | Item Count in `HE`
| 0x14    | uint8   | Item Count in `LS`
| 0x15    | uint8   | Item Count in `DI`
| 0x16    | uint8   | Item Count in `PP`
| 0x17    | uint8   | Item Count in `DC`
| 0x18    | uint8   | Item Count in `KG`
| 0x19    | uint8   | Item Count in `VS`
| 0x1A    | uint8   | Item Count in `BD`
| 0x1B    | uint8   | Item Count in `WM`

## ITC Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Collection ID
| 0x2     | uint16   | Item ID
| 0x4     | uint8   | World ID
| 0x5     | uint8   | Padding
| 0x6     | uint8   | Padding
| 0x7     | uint8   | Padding