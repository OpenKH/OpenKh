# [Kingdom Hearts Dream Drop Distance](../index.md) - Spirits

Each spirit is 256 bytes long.

## Spirit Structure

| Position | Type      | Description                                                           |
| -------- | --------- | --------------------------------------------------------------------- |
| 0x00     | int8      | [Spirit Type](../dictionary/spirits.md)                               |
| 0x01     | int8      | < unknown >                                                           |
| 0x02     | int8      | Disposition and Rank                                                  |
| 0x03     | int8      | Level                                                                 |
| 0x04     | int8[2]   | < unknown >                                                           |
| 0x06     | char[22]  | Name (Each letter is 2 bytes long)                                    |
| 0x1C     | int8[2]   | Padding?                                                              |
| 0x1E     | int8      | Affinity Level and < unknown >                                        |
| 0x1F     | int8      | < unknown >                                                           |
| 0x20     | int8[4]   | Colour (RGB + "FF")                                                   |
| 0x24     | int32     | Experience                                                            |
| 0x28     | int32     | Affinity points                                                       |
| 0x2C     | int16     | LP                                                                    |
| 0x2E     | int16     | < unknown >                                                           |
| 0x30     | int16     | Max HP                                                                |
| 0x32     | int8[11]  | < unknown >                                                           |
| 0x3D     | int8      | Attack                                                                |
| 0x3E     | int8      | Magic                                                                 |
| 0x3F     | int8      | Defense                                                               |
| 0x40     | int16     | Weakness physical (%)                                                 |
| 0x42     | int16     | Weakness fire (%)                                                     |
| 0x44     | int16     | Weakness blizzard (%)                                                 |
| 0x46     | int16     | Weakness thunder (%)                                                  |
| 0x48     | int16     | Weakness water (%)                                                    |
| 0x4A     | int16     | Weakness dark (%)                                                     |
| 0x4C     | int16     | Weakness light (%)                                                    |
| 0x4E     | int8[3]   | < unknown >                                                           |
| 0x51     | int8      | Times linked with the spirit                                          |
| 0x52     | int8[3]   | < unknown >                                                           |
| 0x55     | int32     | Used in battle at runtime                                             |
| 0x59     | int8[10]  | < unknown >                                                           |
| 0x63     | int8      | Shine (Obtained when all nodes are unlocked; 01 = normal, 02 = shiny) |
| 0x64     | int16     | Petting value (Changes when petting)                                  |
| 0x66     | int8[154] | < unknown > (Some of these have the state of the spirit board)        |