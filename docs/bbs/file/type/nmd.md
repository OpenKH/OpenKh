# NMD Format

NMD stands for *? Map ?*.

Controls something related to a map's image?

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `NMD`. Null terminated.
| 0x4     | uint16  | File version `0`
| 0x6     | uint16  | Padding
| 0x8     | uint32[2]  | Padding

## NMD Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Image W
| 0x2     | uint16   | Image H
| 0x4     | int16   | Image Offset X
| 0x6     | int16   | Image Offset Y
| 0x8     | int8   | Image Rotation Y
| 0x9     | int8   | Padding
| 0xA     | uint16   | Padding
| 0xC     | float   | Image Rate