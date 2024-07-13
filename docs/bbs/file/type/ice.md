# ICE Format

ICE stands for *ICE-CREAM* and it controls how an ice cream minigame plays out.

# Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Version, always `0x41264129`
| 0x4     | int32    | File Size
| 0x8     | int32    | Item Count
| 0xC     | uint32   | Dummy

# Measure

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | int32      | Kind
| 0x4     | uint32[3]  | Marks
| 0x10    | uint32     | Sound Color
| 0x14    | uint32     | Combo