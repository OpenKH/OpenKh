# [Kingdom Hearts Dream Drop Distance](../index.md) - btlparam

Location: /game/bin/

Contains the data for the entities' basic parameters.

## File Structure

| Amount | Description                   |
| ------ | ----------------------------- |
| X      | Entries (72 bytes each)       |
| 1      | EOF (CD CD CD CD CD CD CD CD) |


## Entry Structure

| Position | Type     | Description                      |
| -------- | -------- | -------------------------------- |
| 0x00        | char[8]  | [Entity](../dictionary/entities) |
| 0x08        | int16    | < unknown > (%)                  |
| 0x0A       | int16    | Exp (%)                          |
| 0x0C       | int8[4]  | < unknown >                      |
| 0x10       | int16    | Weakness physical (%)            |
| 0x12       | int16    | Weakness fire (%)                |
| 0x14       | int16    | Weakness blizzard (%)            |
| 0x16       | int16    | Weakness thunder (%)             |
| 0x18       | int16    | Weakness water (%)               |
| 0x1A       | int16    | Weakness dark (%)                |
| 0x1C       | int16    | Weakness light (%)               |
| 0x1E       | int8[14] | < unknown >                      |
| 0x2C       | int16    | HP (%)                           |
| 0x2E       | int16    | Attack (%)                       |
| 0x30       | int16    | Magic (%)                        |
| 0x32       | int16    | Defense (%)                      |
| 0x34       | int8[4]  | < unknown >                      |
| 0x38       | int8[2]  | [Status Flags](#Status-Flags)    |
| 0x3A       | int8     | < unknown >                      |
| 0x40       | int8[13] | Reserved for runtime             |

---

## Status Flags

0 = immune

| Position | Size | Description |
| -------- | ---- | ----------- |
| 0        | 1    | Freeze      |
| 1        | 1    | Stop        |
| 2        | 1    | Gravity     |
| 3        | 1    | Magnet      |
| 4        | 1    | Stun        |
| 5        | 1    | Sleep       |
| 6        | 1    | Bind        |
| 7        | 1    | Slow        |
| 8        | 1    | Poison      |
| 9        | 1    | Ignite      |
| 10       | 1    | Confusion   |
| 11       | 1    | Blindness   |
| 12       | 1    | Mini        |
| 13       | 1    | Time Bomb   |
| 14       | 1    | < unknown > |
| 15       | 1    | Zantetsuken |