# STICKERED Format

StickerEd stands for *Sticker ?*.

This file stores a list of stickers in the game and data related to them.

This file is contained within `Menu/stickerdata.arc`

## Header

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | char[8]   | File identifier, always `@BINSTED`.
| 0x8     | uint16  | Version, always `256`.
| 0xA     | uint16  | Data Offset
| 0xC     | uint16  | Data Count
| 0xE     | uint16  | Padding

## StickerEd Data

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint8   | Item ID
| 0x1     | uint8   | Padding
| 0x2     | uint16  | Rectangle SX
| 0x4     | uint16  | Rectangle SY
| 0x6     | uint16  | Rectangle EX
| 0x8     | uint16  | Rectangle EY