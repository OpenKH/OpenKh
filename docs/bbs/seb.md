# SEB Format

SEB stands for *Sound Effect ?*.


## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `@SEB`.
| 0x4     | uint32  | File version `5`
| 0x8     | char[8]  | SEB Name
| 0x10     | uint16  | Motion Count
| 0x12     | uint16  | Max Sound Effect Per Motion
| 0x14     | uint16  | World Count
| 0x16     | uint16  | Padding

## SEB Data Table

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32  | Address
| 0x4     | uint32  | Data Count
| 0x8     | char[12]  | Motion Name

## SEB Sound Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32  | Sound Effect ID
| 0x4     | int16  | Start Frame
| 0x6     | int16  | End Fade Frame
| 0x8     | uint8  | Channel
| 0x9     | uint8  | Play Count
| 0xA     | uint8  | Volume
| 0xB     | uint8  | Kind
| 0xC     | uint8  | Random Group
| 0xD     | uint8  | Random
| 0xE     | uint8  | Padding
| 0xF     | uint8  | Padding