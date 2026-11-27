# [Kingdom Hearts Dream Drop Distance](../index.md) - spcom

Location: /game/de/bin/

Contains the data for the spirit recipes.

## File Structure

| Amount | Description                     |
| ------ | ------------------------------- |
| 1      | Header (32 bytes)               |
| 56     | Table A counts (4 bytes each)   |
| X      | Table A entries (16 bytes each) |
| X      | Table B entries (16 bytes each) |

## Header Structure

| Position | Type    | Description           |
| -------- | ------- | --------------------- |
| 0x00     | char[8] | File version          |
| 0x08     | int32   | Spirit Count          |
| 0x0C     | int32   | Table A entry count   |
| 0x10     | int32   | Table A counts offset |
| 0x14     | int32   | Table A offset        |
| 0x18     | int32   | Table B offset        |
| 0x1C     | int32   | Table B entry count   |

## Table A Counts

Each of this entries refer to a specific spirit from Table A. It seems there's as many Counts as "Spirit Count" + 1 for either padding or end of table.

| Position | Type  | Description  |
| -------- | ----- | ------------ |
| 0x00     | int16 | Entry offset |
| 0x02     | int16 | Entry count  |

Eg: Meow Wow is in offset 1 and has 6 entries. The next spirit, Tama Sheep, has offset 7.

## Table A entries

Combinations for every spirit.

| Position | Type  | Description                            |
| -------- | ----- | -------------------------------------- |
| 0x00     | int16 | [Spirit 1](../dictionary/spirits)      |
| 0x02     | int16 | [Spirit 2](../dictionary/spirits)      |
| 0x04     | int8  | [Item 1 Id](../dictionary/items)       |
| 0x05     | int8  | [Item 1 Category](../dictionary/items) |
| 0x06     | int8  | [Item 2 Id](../dictionary/items)       |
| 0x07     | int8  | [Item 2 Category](../dictionary/items) |
| 0x08     | int8  | Chance of getting spirit 2             |
| 0x09     | int8  | Item 1 Amount                          |
| 0x0A     | int8  | Item 2 Amount                          |
| 0x0B     | int8  | < Unknown >                            |
| 0x0C     | int8  | Rank                                   |
| 0x0D     | int8  | < Unknown >                            |
| 0x0E     | int8  | < Unknown >                            |
| 0x0F     | int8  | Padding                                |

## Table B entries

Bonus parameters given by using a command on Spirit creation.

| Position | Type  | Description                            |
| -------- | ----- | -------------------------------------- |
| 0x00     | int8  | [Item 1 Id](../dictionary/items)       |
| 0x01     | int8  | [Item 1 Category](../dictionary/items) |
| 0x02     | int16 | HP                                     |
| 0x04     | int8  | Attack                                 |
| 0x05     | int8  | Defense                                |
| 0x06     | int8  | Magic                                  |
| 0x07     | int8  | Resistance fire (%)                    |
| 0x08     | int8  | Resistance blizzard (%)                |
| 0x09     | int8  | Resistance thunder (%)                 |
| 0x0A     | int8  | Resistance water (%)                   |
| 0x0B     | int8  | Resistance dark (%)                    |
| 0x0C     | int8  | Resistance light (%)                   |
| 0x0D     | int8  | Affinity                               |
| 0x0E     | int16 | Padding                                |