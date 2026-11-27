# [Kingdom Hearts Dream Drop Distance](../index.md) - tboxdt

Location: /item/jp/bin/

Contains the data for the chests. tboxdtso are Sora's and tboxdtri are Riku's.

## File Structure

| Amount | Description            |
| ------ | ---------------------- |
| 1      | Header (24 bytes)      |
| X      | Entries (8 bytes each) |

## Header Structure

| Position | Type     | Description           |
| -------- | -------- | --------------------- |
| 0x00     | char[4]  | File Identifier (ITB) |
| 0x04     | int16    | File Version?         |
| 0x08     | int16    | Entry count total     |
| 0x0C     | int8[12] | Entry count per world |

The world order is the same as listed [here](../dictionary/worlds), but starts at 1 (Destiny Islands) and ends at 14 (Traverse Town (Revisited))

## Entry Structure

| Position | Type  | Description                          |
| -------- | ----- | ------------------------------------ |
| 0x00     | int8  | < unknown >                          |
| 0x01     | int8  | < unknown >                          |
| 0x02     | int8  | [Item Id](../dictionary/items)       |
| 0x03     | int8  | [Item Category](../dictionary/items) |
| 0x04     | int8  | [World Id](../dictionary/worlds)     |
| 0x05     | int8  | Chest Id                             |
| 0x06     | int16 | Padding                              |