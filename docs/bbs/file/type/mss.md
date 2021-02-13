# MSS Format

MSS stands for *Map Sound ?*.

It controls sound properties for the loaded map.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[4]   | File identifier, always `MSS`. Null terminated.
| 0x4     | uint16  | File version `0`
| 0x6     | uint16  | Padding
| 0x8     | uint32[2]  | Padding

## MSS Parameters

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16   | Reverberation
| 0x2     | uint16   | Sound Effect Range
| 0x4     | uint16   | Voice Range
| 0x6     | int8   | Reverberation Type
| 0x7     | uint8   | Padding
| 0x8     | uint32[2]   | Padding