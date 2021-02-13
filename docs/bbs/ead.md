# EAD Format

EAD stands for *Effect At Data*.

It seems to control effects attached to objects.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `BDD`. Null terminated.
| 0x4     | int32   | Count
| 0x8     | int32   | Padding
| 0xC     | int16   | Padding
| 0xE     | int16   | Version, always `0xF`.

The following list contains an entry count specified `Count` from the Header.

## EAD Data Table

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Offset
| 0x4     | char[12]   | Name

Unknown Data list here.

## EAD Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Pointer to Effect File
| 0x4     | uint32   | Pointer to Effect Name
| 0x8     | uint32   | Pointer Bone To Use
| 0xC     | int16   | Offset X
| 0xE     | int16   | Offset Y
| 0x10    | int16   | Offset Z
| 0x12    | uint16   | Group
| 0x14    | uint32:8   | ATCInF
| 0x15    | uint32:24   | ATCol
| 0x18    | int16   | Rotation X
| 0x1A    | int16   | Rotation Y
| 0x1C    | int16   | Rotation Z
| 0x1E    | int16   | Scale X
| 0x20    | int16   | Scale Y
| 0x22    | int16   | Scale Z
| 0x24    | uint32   | Collision Count
| 0x28    | uint16   | Start Frame
| 0x2A    | uint16   | Fade Frame
| 0x2C    | uint16   | End Frame
| 0x2E    | uint8   | End Frame F
| 0x2F    | uint8   | Start Fade F
| 0x30    | uint16:4   | Level
| 0x30    | uint16:12   | Flag
| 0x32    | uint8   | ATCOutF
| 0x33    | uint8   | Play Count