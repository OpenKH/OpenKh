# MDLSHOP Format

mdlShop stands for *Medal Shop*.

This file includes the whole list of rewards for sale on Mirage Arena's Medal Shop.

There is a file for each playable character with the following names:
| ARC File | Internal File  | Description
|--------|-------|------------
| SHOPMDLTE.ARC | mdlTeShop.bin| Terra's Medal Shop
| SHOPMDLVE.ARC | mdlVeShop.bin| Ventus's Medal Shop
| SHOPMDLAQ.ARC | mdlAqShop.bin| Aqua's Medal Shop

Not all commands will appear in the shop even if you add them to the list.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `SHP`. Null terminated.
| 0x4     | uint16  | Version, always `1`.
| 0x6     | uint16  | Type (Unknown use)
| 0x8     | uint32  | Number of Items

This data chunk repeats as many times as the field `Number of Items` specifies.

## Medal Shop Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Item Index
| 0x2     | uint8   | [Type](#Command-Type)
| 0x3     | uint8   | Release Arena Level
| 0x4     | int16   | Arena Level Required
| 0x6     | int16   | Medal Cost

## Command Type

| Value | Name  | Description
|--------|-------|------------
| 0     | SHOP_ID_NONE   | Regular list of items.
| 1     | SHOP_ID_ACCESSORIE   | Doesn't work
| 2     | SHOP_ID_COMMAND   | 
| 3     | SHOP_ID_COMMAND_HOLO   | Same as `SHOP_ID_COMMAND`
| 4     | SHOP_ID_ICE   | Same as `SHOP_ID_COMMAND`
| 5     | SHOP_ID_MEDAL   | Same as `SHOP_ID_COMMAND`