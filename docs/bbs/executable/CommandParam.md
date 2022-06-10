# Command Param Format

It controls parameters related to command stats.

This data is located with the game's executable.

## Command Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int16  | Buy Price
| 0x2     | uint8  | Sell Price
| 0x3     | uint8  | Reload Time (seconds) / Amount uses for Items / Maximum level for Abilities
| 0x4     | uint8  | Gauge Fill (out of 100)
| 0x5     | uint8  | Maximum Level
| 0x6     | int16  | CP for Base Level
| 0x8     | int16  | Level CP increase
| 0xA     | int16[10]  | Command Power (Per level) **

** This list can be used for special purposes, mainly for non-equippable commands. A list of some of those uses can be read [here](../../dictionary/commands_special.md).