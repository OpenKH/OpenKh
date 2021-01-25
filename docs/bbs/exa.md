# [Kingdom Hearts Birth By Sleep](index.md) - EXA format (Exusia)

This file is used to display cutscenes or anything that requires camerawork. [Kingdom Hearts Birth by Sleep](../../index).

# Header
| Offset | Type  | Field Name | Description
|--------|-------|------------|------------
| 0x0     | char[4]   | name | File identifier, always `exa\0`
| 0x4     | float   | version | 
| 0x8     | uint8 | 
| 0x9     | uint8 | 
| 0xA     | uint8 | 
| 0xB     | uint8 | 
| 0xC     | uint8 | Number of particle effects resources
| 0xD     | uint8 | 
| 0xE     | uint8 | 
| 0xF     | uint8 | 

# Effects
| Offset | Type  | Description
|--------|-------|------------
| 0x10     | List<char[0x40]>   | List to the full path of the effects

The secondary header starts right after all effects.

# EXUSIA RESOURCE INFO
| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Unknown00
| 0x4     | uint32   | readWait
| 0x8     | uint8   | Number of groups
| 0x9     | uint8   | Number of WallPapers.
| 0xA     | uint8   | Number of PMO resources loaded.
| 0xB     | uint8   | Number of PAM resources loaded.
| 0xC     | uint8   | Number of Effect resources loaded.
| 0xD     | uint8   | Number of CTD resources loaded.
| 0xE     | uint8   | Number of sound effect resources loaded.
| 0xF     | uint8   | Number of voice clip resources loaded.
| 0x10     | uint8   | Number of BGM resources loaded.
| 0x11     | uint8   | dummy
| 0x12     | uint8   | dummy
| 0x13     | uint8   | dummy
| 0x14     | uint32   | Unknown14
| 0x18     | uint8   | Unknown18
| 0x19     | uint8   | Unknown19
| 0x1A     | uint16   | Unknown1A
| 0x1C     | char[4]   | Always 'MAP\0'
| 0x20     | char[16]   | Name of the map it the event takes place in.

# Resource definition
| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint   | Unknown00
| 0x4     | uint   | Unknown04
| 0x8     | uint   | Padding08
| 0xC     | char[16]   | Name of the resource. (without extension)
| 0x1C     | char[32]   | Path of the resource.
