# ITE Format

ITE simply stands for *ITEM* and it contains the list of items in the game.

ITB and ITC formats use the item IDs from this list.

This file can be found in the `item` folder or inside `arc/system/commongame.arc`.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `ITE`. Null terminated.
| 0x4     | uint16   | Version, `1`
| 0x6     | uint16   | Padding
| 0x8     | uint16   | Weapon Data Count
| 0xA     | uint16   | Flavor Data Count
| 0xC     | uint16   | Key Item Data Count
| 0xE     | uint16   | Key Hide Data Count
| 0x10     | uint16   | Synthesis Data Count
| 0x12     | uint16   | Padding

## Weapon Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Item ID
| 0x2     | uint8   | Padding
| 0x3     | uint8   | Padding

## Flavor Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Item ID
| 0x2     | uint8   | Padding
| 0x3     | uint8   | Padding

## Key Item Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Item ID
| 0x2     | uint8   | Padding
| 0x3     | uint8   | Padding

## Key Item Hide Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Item ID
| 0x2     | uint8   | Padding
| 0x3     | uint8   | Padding

## Synthesis Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Item ID
| 0x2     | uint8   | Padding
| 0x3     | uint8   | Padding