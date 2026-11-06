# [Kingdom Hearts Dream Drop Distance](../index.md) - lbt_list

Location: /game/de/bin/

Contains the data for the Spirits' boards' rewards.

## File Structure

| Amount | Description             |
| ------ | ----------------------- |
| 1      | Header (16 bytes)       |
| X      | Boards (192 bytes each) |
| 1      | Padding? (48 bytes)     |

Each board has 16 Reward entries ( bytes each)

## Header Structure

| Position | Type  | Description                    |
| -------- | ----- | ------------------------------ |
| 0x00     | int32 | File Identifier? (44 33 22 11) |
| 0x08     | int32 | Board Count                    |


## Board Structure

| Amount | Description             |
| ------ | ----------------------- |
| 16     | Reward Entry (12 bytes) |

## Reward Entry Structure

For some reason the reward is repeated 4 times

| Position | Type    | Description                            |
| -------- | ------- | -------------------------------------- |
| 0x00     | int8[2] | < unknown >                            |
| 0x02     | int8[2] | Padding?                               |
| 0x04     | int8    | [Item 1 Id](../dictionary/items)       |
| 0x05     | int8    | [Item 1 Category](../dictionary/items) |
| 0x06     | int8    | Item 2 Id                              |
| 0x07     | int8    | Item 2 Category                        |
| 0x08     | int8    | Item 3 Id                              |
| 0x09     | int8    | Item 3 Category                        |
| 0x0A     | int8    | Item 4 Id                              |
| 0x0B     | int8    | Item 4 Category                        |