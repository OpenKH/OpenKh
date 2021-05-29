# [Kingdom Hearts Dream Drop Distance](../index.md) - dropprm

Location: /game/bin/

Contains the data for the entities' dropped prizes.

## File Structure

| Amount | Description                   |
| ------ | ----------------------------- |
| X      | Entries (28 bytes each)       |
| 1      | EOF (CD CD CD CD CD CD CD CD) |


## Entry Structure

Each entry contains 7 prize structures, which are limited to specific reward categories/types. Trying to make it drop a different type will either drop nothing or an error item.

| Amount | Description                               |
| ------ | ----------------------------------------- |
| 3      | Orb prize (HP, munny, drop points)        |
| 1      | Treat prize                               |
| 1      | Item prize (Item Command or Training Toy) |
| 2      | Dream Piece prize                         |

## Prize Structure

| Position | Type | Description                          |
| -------- | ---- | ------------------------------------ |
| 0x00     | int8 | [Item Id](../dictionary/items)       |
| 0x01     | int8 | [Item Category](../dictionary/items) |
| 0x02     | int8 | Padding                              |
| 0x03     | int8 | Chance (%)                           |