# PVD Format

PVD stands for *? Volume Data*.

It controls volumetric effects as fog or glare.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `PVD`. Null terminated.
| 0x4     | uint16  | File version `2`
| 0x6     | uint16  | Padding
| 0x8     | uint32[2]  | Padding

## PVD Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Fog Color (ABGR)
| 0x4     | float   | Fog Near
| 0x8     | float   | Fog Far
| 0xC     | float   | Disp Near
| 0x10    | float   | Disp Far
| 0x14    | float   | Glare Strength
| 0x18    | uint32   | Clear Color (ABGR)
| 0x1C    | uint32   | Flag (Unknown)
| 0x20    | float   | View Angle
| 0x24    | Vector3f   | Base Position
| 0x30    | float   | Camera Offset Y
| 0x34    | float   | Camera Rotation X
| 0x38    | float   | Camera Distance
| 0x3C    | float   | Camera Rotation