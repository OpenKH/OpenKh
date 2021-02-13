# SEI Format

SEI stands for *Sound Effect Info*.

This format's purpose is unknown.

The only known instance of this file is within `arc/system/seinfo.arc` in the subfile `SeTable.sei`.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `SEI`. Null terminated.
| 0x4     | uint16  | Version, always `1`.
| 0x6     | uint8  | Model Count
| 0x7     | uint8  | Unknown
| 0x8     | char[6][ModelCount]  | List of model names

The structure for the rest of values is unknown.