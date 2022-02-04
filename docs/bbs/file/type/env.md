# ENV Format

ENV stands for *Environment*.

This file type is exclusive to the HD version of Birth by Sleep.

It's used for adding additional sound effects for the environment.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `env$`.
| 0x4     | uint32    | Version
| 0x8     | uint32    | [Setting Data](#Setting-Data) Count
| 0xC     | uint32    | Flag Type

### Setting Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[16]   | Entry Name
| 0x10    | char[16]   | SCD File Name
| 0x20    | uint32     | World Prefix / World Resident Use Flag
| 0x24    | int32      | Sound ID
| 0x28    | uint32     | Flag Type
| 0x2C    | int32      | Control Handle
| 0x30    | Vector4f   | Sound Position
| 0x40    | float      | Sound Volume
| 0x44    | uint32     | Frame
| 0x48    | float      | 3D Range Max
| 0x4C    | float      | 3D Range Min