# Command Param Format

It controls parameters related to command stats.

This data is located with the game's executable.

## Command Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int16  | Buy Price
| 0x2     | uint8  | Sell Price
| 0x3     | uint8  | Reload Time (seconds)
| 0x4     | uint8  | Gauge Fill (out of 100)
| 0x5     | uint8  | Maximum Level
| 0x6     | int16  | Level Base (Unknown)
| 0x8     | int16  | Level EXP increase
| 0xA     | int16[10]  | Command Power (Per level)