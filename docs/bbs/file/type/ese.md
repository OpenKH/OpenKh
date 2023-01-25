# ESE Format

ESE stands for `Effect Sound Effect`.

These files are contained within the `.arc` files of characters beginning with `e` or `b`, for example, `b01ex00.arc` has a file named `b01ex00.ese` which contains its stats.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | char[4]  | File identifier. Always `ESE`, null terminated.
| 0x4    | int32    | Padding
| 0x8    | int32    | [Flags](#Flags)
| 0xC    | uint16   | Version, always `2`.
| 0xE    | uint16   | [Data](#Data) Count
| 0x10   | uint32[Data Count]   | Pointers to [Data Entries](#Data)


### Flags

There is only one flag, with a value of `0x1`.

## Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16   | Count
| 0x2    | int16    | Effect ID
| 0x4    | char[16] | Effect Particle Name
| 0x14   | uint16   | Code - Number
| 0x16   | uint16   | Code - File
| 0x18   | uint8    | Fade Time
| 0x19   | uint8    | Padding
| 0x1A   | uint16   | Group