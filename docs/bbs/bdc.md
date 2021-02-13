# BDC Format

BDC stands for *Board Dice Common*.

It controls various generic aspects related to the Command Board.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `BDC`. Null terminated.
| 0x4     | uint32  | Version
| 0x8     | uint32  | Padding
| 0xC     | uint32  | Padding

## BCD Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | float   | Chain
| 0x4     | float   | Monopoly
| 0x8     | float[5]   | Value
| 0x1C    | float[5]   | Cost
| 0x30    | float   | Change
| 0x34    | float   | Buy
| 0x38    | float   | Sell
| 0x3C    | float   | Panel Bonus
| 0x40    | float   | Round Bonus
| 0x44    | float   | Card Bonus
| 0x48    | float   | Joker
| 0x4C    | float   | Inflation
| 0x50    | float   | Experience Base
| 0x54    | float[3]   | Experience
| 0x60    | float   | MedalRate
| 0x64    | int16  | Check Play Count
| 0x66    | int16  | Padding
| 0x68    | int32  | Padding
| 0x6C    | int32  | Padding
| 0x70    | int16  | BP Magnet Base
| 0x72    | int16  | Special CD Base
| 0x74    | int16  | Special DC Base
| 0x76    | int16  | Special LS Base
| 0x78    | int16  | Special WP Player Base
| 0x7A    | int16  | Special WP Mi Base
| 0x7C    | int16  | Special Ex0 Minimum Base
| 0x7E    | int16  | Special Ex0 Maximum Base
| 0x80    | int16  | Special Ex1 Minimum Base
| 0x82    | int16  | Special Ex1 Maximum Base
| 0x84    | float  | Special Ex0 Minimum Buy
| 0x88    | float  | Special Ex0 Maximum Buy
| 0x8C    | float  | Special Ex1 Minimum Buy
| 0x90    | float  | Special Ex1 Maximum Buy
| 0x94    | int32[4][8]  | Clear BP