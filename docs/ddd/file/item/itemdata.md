# [Kingdom Hearts Dream Drop Distance](../index.md) - itemdata

Location: /item/jp/bin/

Contains the data for the entities' basic parameters.

## File Structure

| Amount | Description                   |
| ------ | ----------------------------- |
| 1      | Header (8 bytes)              |
| X      | Entries (4 bytes each)        |
| 1      | EOF (CD CD CD CD CD CD CD CD) |

## Header Structure

| Position | Type    | Description           |
| -------- | ------- | --------------------- |
| 0x00     | char[4] | File Identifier (ITE) |
| 0x04     | int16   | File Version?         |

## Entry Structure

| Position | Type | Description                                                                                        |
| -------- | ---- | -------------------------------------------------------------------------------------------------- |
| 0x00     | int8 | [Item Id](../dictionary/items)                                                                     |
| 0x01     | int8 | [Item Category](../dictionary/items)                                                               |
| 0x02     | int8 | Same as the ID, but starts at 1 instead of 0. Items in the category 10 use it as a boolean 0 or 1. |
| 0x03     | int8 | Enabled (Unused/Unobtainable items have a 0, others have a 1)                                      |