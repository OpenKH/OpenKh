# [Kingdom Hearts Birth By Sleep](index.md) - EXA format (Exusia)

This file is used to display cutscenes or anything that requires camerawork. [Kingdom Hearts Birth by Sleep](../../index).

# Header

| Offset | Type  | Field Name | Description
|--------|-------|------------|------------
| 0x0     | char[4]   | name | File identifier, always `exa`
| 0x4     | float   | version | It doesn't seem to affect the cutscene's playback.


# Exusia System Info

Right after the header.

| Offset | Type  | Field Name | Description
|--------|-------|------------|------------
| 0x0     | int32 | Event Skip Jump Frame
| 0x4     | int16 | Info Flag
| 0x6     | int16 | Movie Number

# Effects

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | List<char[0x40]>[InfoFlag]   | List to the full path of the effects

The secondary header starts right after all effects.

# EXUSIA RESOURCE INFO

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | readWait
| 0x4     | uint8    | Number of groups
| 0x5     | uint8    | Number of WallPapers.
| 0x6     | uint8    | Number of PMO resources loaded.
| 0x7     | uint8    | Number of PAM resources loaded.
| 0x8     | uint8    | Number of Effect resources loaded.
| 0x9     | uint8    | Number of CTD resources loaded.
| 0xA     | uint8    | Number of sound effect resources loaded.
| 0xB     | uint8    | Number of voice clip resources loaded.
| 0xC     | uint8    | Number of BGM resources loaded.
| 0xD     | uint8    | dummy
| 0xE     | uint8    | dummy
| 0xF     | uint8    | dummy

| 0x10    | uint32   | Unknown14
| 0x14    | uint8    | Unknown18
| 0x15    | uint8    | Unknown19
| 0x16    | uint16   | Unknown1A
| 0x18    | char[4]  | Always 'MAP\0'
| 0x1C    | char[16] | Name of the map it the event takes place in.

# Resource definition

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint   | Unknown00
| 0x4     | uint   | Unknown04
| 0x8     | uint   | Padding08
| 0xC     | char[16]   | Name of the resource. (without extension)
| 0x1C    | char[32]   | Path of the resource.
