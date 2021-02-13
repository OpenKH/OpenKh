# PCAM Format

PCAM stands for *Player CAMera*.

It controls various aspects of a specific player's camera.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `@PCA`.
| 0x4     | uint32  | Kind
| 0x8     | float  | View Angle
| 0xC     | float  | Size

The next data chunk is repeated twice. Normal and Extended.

## PCAM Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Type
| 0x4     | float   | Adjustment
| 0x8     | float   | Timer
| 0xC     | uint32   | Flag
| 0x10     | Vector4f   | Eye
| 0x20     | Vector4f   | Aim

