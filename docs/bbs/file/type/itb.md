# ITB Format

ITB stands for *Item Treasure Box* and it contains the list of items obtained in treasure boxes.

This file can be found in the `item` folder or within `arc/system/common_xx.arc` where `xx` is the first two letters of the character it belongs to, for example `Aq` for Aqua.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `ITB`. Null terminated.
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

## ITB Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Treasure Box ID
| 0x2     | uint16   | Reward ID
| 0x4     | uint8   | [Reward Kind](#Reward-Kind)
| 0x5     | uint8   | World ID
| 0x6     | uint8   | Report ID
| 0x7     | uint8   | Padding

## Reward Kind
| Value | Name  | Description
|--------|-------|------------
| 0     | COMMAND   | 
| 1     | ITEM   | 

## Item Kind

| Value | Name  | Description
|--------|-------|------------
| -1     | ITEM_UNKNOWN   | 
| 0     | ITEM_WEAPON   | 
| 1     | ITEM_FLAVOR   | 
| 2     | ITEM_HID   | 
| 3     | ITEM_KEY   | 
| 4     | ITEM_ALCHEMY   | 