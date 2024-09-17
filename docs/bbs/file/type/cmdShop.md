# CMDSHOP Format

cmdShop stands for *Command Shop*.

This file includes the whole list of commands for sale on the Moogle's Command Shop.

There is a file for each playable character with the following names:
| ARC File | Internal File  | Description
|--------|-------|------------
| SHOPCMDTE.ARC | cmdTeShop.bin| Terra's Command Shop
| SHOPCMDVE.ARC | cmdVeShop.bin| Ventus's Command Shop
| SHOPCMDAQ.ARC | cmdAqShop.bin| Aqua's Command Shop

Not all commands will appear in the shop even if you add them to the list.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `SHP`. Null terminated.
| 0x4     | uint16  | Version, always `1`.
| 0x6     | uint16  | Type (Unknown use)
| 0x8     | uint32  | Number of Items

This data chunk repeats as many times as the field `Number of Items` specifies.

## Command Shop Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Command Index
| 0x2     | uint16   | Padding