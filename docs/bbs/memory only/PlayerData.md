# PLAYER DATA

This isn't a file, but data specification that appears instantiated in memory.

## Stats

| Offset | Type  | Description
|---------|--------|------------
| 0x0     | uint32 | Experience
| 0x4     | uint32 | Munny
| 0x8     | uint32 | Medals
| 0xC     | uint32 | Style Flag
| 0x10    | uint32 | Enchantment Flag
| 0x14    | uint32 | Arena Level Flag
| 0x18    | int32 | Next Level Experience
| 0x1C    | uint16 | Maximum HP
| 0x1E    | uint16 | Current HP
| 0x20    | uint16 | Maximum Focus
| 0x22    | uint16 | Current Focus
| 0x24    | uint16 | LP
| 0x26    | uint16 | Pre EXP
| 0x28    | int8[8] | Deck Reload Time
| 0x30    | int8 | Currently Selected Deck
| 0x31    | uint8 | Current Level
| 0x32    | uint8 | Ability Points
| 0x33    | uint8 | MP?
| 0x34    | uint8 | D-Link Points
| 0x35    | uint8 | Weapon ID
| 0x36    | uint8 | Max Deck Slots
| 0x37    | uint8 | Arena Level
| 0x38    | int16 | Physical Resistance
| 0x3A    | int16 | Fire Resistance
| 0x3C    | int16 | Ice Resistance
| 0x3E    | int16 | Thunder Resistance
| 0x40    | int16 | Darkness Resistance
| 0x42    | int16 | HP Ability
| 0x44    | uint16 | Old Finisher Type
| 0x46    | int16 | Padding
| 0x48    | [COMMAND](./Command.md#Command)[8] | Command Deck
| 0x88    | [COMMAND](./Command.md#Command) | Finisher Command
| 0x90    | [COMMAND](./Command.md#Command) | Shotlock Command
| 0x98    | [COMMAND](./Command.md#Command) | Jump Command
| 0xA0    | [COMMAND](./Command.md#Command) | Glide Command
| 0xA8    | [COMMAND](./Command.md#Command) | Air Dash Command
| 0xB0    | [COMMAND](./Command.md#Command) | Ground Dash Command
| 0xB8    | [COMMAND](./Command.md#Command) | Dash Ability
| 0xC0    | [COMMAND](./Command.md#Command) | Avoid Slide Command
| 0xC8    | [COMMAND](./Command.md#Command) | Combo Slide Command
| 0xD0    | [COMMAND](./Command.md#Command) | Turn Ability Command
| 0xD8    | [COMMAND](./Command.md#Command) | Guard Command
| 0xE0    | [COMMAND](./Command.md#Command) | Guard Ability Command
| 0xE8    | [COMMAND](./Command.md#Command) | Blow Ability Command
| 0xF0    | [COMMAND](./Command.md#Command)[21] | Link Command
| 0x198   | [COMMAND](./Command.md#Command)[3] | Next Finisher Command
| 0x1B0   | char[38] | Finisher Command Name
| 0x1D6   | uint8 | Padding
| 0x1D7   | uint8 | Padding

