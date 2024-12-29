# BIC Format

BIC stands for *BGM Ice Collection* and it contains the list of songs used in the Ice Cream minigame in Disney Town.

This file is a `.bic`, but contained in a `.bin`

# Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32   | Version, always `0x41264126`
| 0x4     | int32    | File Size
| 0x8     | int32    | Item Count
| 0xC     | uint32   | Dummy

## BGM Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | int32  | BGM Resource Number
| 0x4    | int32  | Tempo
| 0x8    | int32  | Loop Start
| 0xC    | int32  | Loop End