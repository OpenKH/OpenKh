# [Kingdom Hearts Dream Drop Distance](../index.md) - techprm

Location: /game/bin/

Contains the data for hits. techprm is generic and techprmp is for the player. (First entry in techprmp is Sora's basic downward swing)

## File Structure

| Amount | Description                   |
| ------ | ----------------------------- |
| X      | Entries (20 bytes each)       |
| 1      | EOF (CD CD CD CD CD CD CD CD) |

## Entry Structure

| Position | Type    | Description                                        |
| -------- | ------- | -------------------------------------------------- |
| 0x00     | int16   | < unknown >                                        |
| 0x02     | int16   | Power (%)                                          |
| 0x04     | int8[3] | < unknown >                                        |
| 0x07     | int8    | < unknown > (Looks like a boolean bitfield)        |
| 0x08     | int8    | Element                                            |
| 0x09     | int8[7] | < unknown >                                        |
| 0x10     | int8    | < unknown > Always 0 in the generic file (target?) |
| 0x11     | int8[3] | Padding                                            |

## Elements

| Value | Description |
| ----- | ----------- |
| 0     | Physical    |
| 1     | Fire        |
| 2     | Blizzard    |
| 3     | Thunder     |
| 4     | Water       |
| 5     | Dark        |
| 6     | Light       |
| 7     | < unknown > |
| 8     | < unknown > |