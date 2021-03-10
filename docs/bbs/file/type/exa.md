# [Kingdom Hearts Birth By Sleep](index.md) - EXA format (Exusia)

This file is used to display cutscenes or anything that requires camerawork. [Kingdom Hearts Birth by Sleep](../../index).

# Header

| Offset | Type  | Field Name | Description
|--------|-------|------------|------------
| 0x0     | char[4]   | name | File identifier, always `exa`
| 0x4     | float   | version | It doesn't seem to affect the cutscene's playback.


# EXUSIA SYSTEM INFO

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


## EXUSIA MAP AREA INFO

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int16 | Key Num
| 0x2    | int16 | Padding

## EXUSIA MAP UNKNOWN

| Offset | Type  | Description
|--------|-------|------------
| 0x18    | char[4]  | Always 'MAP\0'

## EXUSIA MAP AREA

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | char[16] | Name of the map the event takes place in.

## EXUSIA RESOURCE GROUP

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Read Frame
| 0x0    | int32 | Destination Frame
| 0x0    | int32 | Category ID
| 0x0    | char[16] | Label

## EXUSIA RESOURCE WALLPAPER

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Read Frame
| 0x4    | int32 | Destination Frame
| 0x8    | char[16] | Texture Name

## EXUSIA RESOURCE MODEL

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Read Frame
| 0x0    | int32 | Destination Frame
| 0x0    | int32 | Model Type
| 0x0    | char[16] | Name
| 0x0    | char[32] | Path

## EXUSIA RESOURCE MOTION

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Read Frame
| 0x0    | int32 | Destination Frame
| 0x0    | int32 | Load Flag
| 0x0    | int32 | Model Type
| 0x0    | char[16] | Name
| 0x0    | char[32] | Path
| 0x0    | char[16] | Pack

## EXUSIA RESOURCE EFFECT

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Read Frame
| 0x4    | int32 | Destination Frame
| 0x8    | char[32] | FEP Name
| 0x28   | char[32] | Path

## EXUSIA RESOURCE CTD

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | char[48] | CTD File Name

## EXUSIA RESOURCE SE

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Read Frame
| 0x4    | int32 | Destination Frame
| 0x8    | char[16] | Name
| 0x18   | char[32] | Path

## EXUSIA RESOURCE VOICE

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Read Frame
| 0x4    | int32 | Destination Frame
| 0x8    | char[16] | Name
| 0x18   | char[32] | Path

## EXUSIA RESOURCE BGM

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32 | Read Frame
| 0x4    | int32 | Destination Frame
| 0x8    | int32 | BGM Type
| 0xC    | char[16] | Name
| 0x1C   | char[32] | Path

## EXUSIA MESSAGE

This one seems to be used in the game more often.

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32  | Start
| 0x4    | int32  | Trigger
| 0x8    | int32  | End
| 0xC    | uint32 | ID (For the text shown)
| 0x10   | uint8  | Select
| 0x11   | uint8  | Select Max
| 0x12   | uint8  | Start Selection
| 0x13   | uint8  | Padding
| 0x14   | uint16 | Jump Frame 0
| 0x16   | uint16 | Jump Frame 1
| 0x18   | uint16 | Jump Frame 2
| 0x1A   | uint16 | Jump Frame 3

## EXUSIA MESSAGE VERSION 1

No usage of this type has been spotted in the games.

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32  | Start
| 0x4    | int32  | Trigger
| 0x8    | int32  | End
| 0xC    | uint32 | ID (For the text shown)
| 0x10   | uint8  | Select
| 0x11   | uint8  | Select Max
| 0x12   | uint8  | Start Selection
| 0x13   | uint8  | Padding
| 0x14   | uint16 | Jump Frame 0
| 0x16   | uint16 | Jump Frame 1
| 0x18   | uint16 | Jump Frame 2
| 0x1A   | uint16 | Jump Frame 3
| 0x1C   | uint16 | Jump Frame 4
| 0x1E   | uint16 | Jump Frame 5
| 0x20   | uint16 | Jump Frame 6
| 0x22   | uint16 | Jump Frame 7
| 0x24   | int16  | Jump Cancel
| 0x26   | int16  | Padding

### EXUSIA MESSAGE INFO

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int16[4] | Layer Key Count
| 0x8    | int16    | Key Layer Top
| 0xA    | int16    | Key Layer 2
| 0xC    | int16    | Key Layer 1
| 0xE    | int16    | Key Layer 0

---
# Resource definition

It is unknown where this is used.
| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint   | Unknown00
| 0x4     | uint   | Unknown04
| 0x8     | uint   | Padding08
| 0xC     | char[16]   | Name of the resource. (without extension)
| 0x1C    | char[32]   | Path of the resource.
