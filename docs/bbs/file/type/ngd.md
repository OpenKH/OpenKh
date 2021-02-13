# NGD Format

NGD stands for *Navigation Grid Data*.

It seems to be used for enemy navigation around the levels.


## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `@NGD`.
| 0x4     | uint16  | File version `0`
| 0x6     | int16  | DivX
| 0x8     | int16  | DivY
| 0xA     | int16  | DivZ
| 0xC     | Vector3f  | Minimum
| 0x18     | Vector3f  | Maximum
| 0x24     | float  | Cell Size
| 0x28     | int32  | Node Count
| 0x2C     | Vector3f  | Node
| 0x38     | int32  | Bit Array X
| 0x3C     | int32  | Bit Array Y
| 0x40     | int32  | Bit Array Z
| 0x44     | int16  | Shortest Path