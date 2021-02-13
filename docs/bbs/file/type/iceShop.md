# ICESHOP Format

iceShop stands for *Ice-cream Shop*.

This file includes the whole list of ice-creams for sale on Disney Town's ice-cream Shop.

There is a file for each playable character with the following names:
| ARC File | Internal File  | Description
|--------|-------|------------
| SHOPICETE.ARC | iceTeShop.bin| Terra's ice-cream Shop
| SHOPICEVE.ARC | iceVeShop.bin| Ventus's ice-cream Shop
| SHOPICEAQ.ARC | iceAqShop.bin| Aqua's ice-cream Shop

Not all commands will appear in the shop even if you add them to the list.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `SHP`. Null terminated.
| 0x4     | uint16  | Version, always `1`.
| 0x6     | uint16  | Type (Unknown use)
| 0x8     | uint32  | Number of Items

This data chunk repeats as many times as the field `Number of Items` specifies.

## Ice-cream Shop Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Command Kind
| 0x0     | uint8   | Create Count
| 0x0     | int16   | Item ID 1
| 0x0     | int16   | Item ID 2
| 0x0     | int16   | Item ID 3
| 0x0     | int16   | Item ID 4
| 0x0     | uint8   | Item ID 1 Count
| 0x0     | uint8   | Item ID 2 Count
| 0x0     | uint8   | Item ID 3 Count
| 0x0     | uint8   | Item ID 4 Count