# w_start Format

w_start stands for *World Start*.

It controls the initial location you appear in when you first arrive a world.

This file is located inside `parag_xx.arc` where `xx` are the initial of the character name. The internal file is named `w_start_xx.bin` where xx means the same as explained before.

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint32  | Version, always `2`.
| 0x4     | uint32  | Data Count

## w_start Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint8  | World ID
| 0x1     | uint8  | Room ID
| 0x2     | uint8  | Start Position
| 0x3     | uint8  | Ex Set Ptn (Unknown)
| 0x4     | uint16 | Paragraph (Unknown)