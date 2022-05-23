# STATUS INFO

This isn't a file, but data specification that appears instantiated in memory.

## STATUS INFO DATA

| Offset | Type  | Description
|---------|--------|------------
| 0x0     | DL_STATUS | D-Link Status Array
| 0x7E0   | [COMMAND](./Command.md#Command)[16] | Finisher Commands Array
| 0x860   | [COMMAND](./Command.md#Command)[21] | D-Link Command Array
| 0x908   | uint8[16][38] | Finisher Command Strengths?
| 0xB68   | ICOLOR | Armor Color
| 0xB6C   | uint32 | Total Experience
| 0xB70   | uint32 | Total Munny
| 0xB74   | uint32 | Medals
| 0xB78   | uint16 | Level
| 0xB7A   | uint16 | Current HP
| 0xB7C   | uint16 | Maximum HP
| 0xB7E   | uint16 | Current Focus
| 0xB80   | uint16 | Maximum Focus
| 0xB82   | uint16 | Link Points
| 0xB84   | uint16 | MP?
| 0xB86   | uint16 | DP?
| 0xB88   | uint16 | Arena Level
| 0xB8A   | uint16 | Weapon ID
| 0xB8C   | int16 | Physical Resistance
| 0xB8E   | int16 | Fire Resistance
| 0xB90   | int16 | Ice Resistance
| 0xB92   | int16 | Thunder Resistance
| 0xB94   | int16 | Darkness Resistance
| 0xB96   | uint16 | Player Weapon ID
| 0xB98   | uint16 | Selected Finisher
| 0xB9A   | uint16 | Ability Points?
| 0xB9C   | int16[14] | Padding

## STATUS INFO RELATED ENUM

| Name            |      Value      |     Description
|-----------------|----------------|------------
| START_ARMOR_COLOR  | 0xFF808080  | 
| ARMOR_COLOR_MIN  | 0x40  | 
| ARMOR_COLOR_MAX  | 0xC0  | 
| MAX_LEVEL  | 0x63 | 
| MAX_MEDAL  | 0x1869F | 
| MAX_MUNNIE  | 0xF423F | 