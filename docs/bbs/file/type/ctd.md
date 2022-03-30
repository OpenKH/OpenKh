# [Kingdom Hearts Birth By Sleep](index.md) - CTD format

CTD is used to display text messages in game. It does not only contain the text to write on screen, but also the layout, font type, position and more.

In order to make a specific text appear in-game, the engine selects a [message](#message) thorugh its unique identifier. Then, every message will refer to a [layout](#layout) which contains the display logic.

## Text encoding

The encoding used is [Shift-JIS](https://en.wikipedia.org/wiki/Shift_JIS). The European/American text uses a slighly different variant of Shift-JIS. To learn more on how the game engine renders those characters, check how [character mapping](font#character-mapping) is performed.

## File format

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int32   | File identifier, `0x44544340`
| 04     | int32   | Version, always `1`
| 08     | int16 | RESERVED
| 0a     | int16 | Unknonw
| 0c     | int16 | [Layout](#layout) count
| 0e     | int16 | [Message](#message) count
| 10     | int32   | [Message](#message) offset
| 14     | int32   | [Layout](#layout) offset
| 18     | int32   | Text start offset
| 1c     | int32   | RESERVED

### Message

Describe a message with a text and links it to a layout

| Offset | Type  | Description
|--------|-------|------------
| 00     | int32 | Unique identifier
| 04     | int32   | Text offset
| 08     | int32   | [Layout](#layout) index
| 0C     | int32   | Wait Frame Count

### Layout

Describe how a message should be presented on screen

| Offset | Type  | Description
|--------|-------|------------
| 00     | int16 | Dialog X position
| 02     | int16 | Dialog Y position
| 04     | int16 | Balloon window width (borders excluded)
| 06     | int16 | Balloon window height (borders excluded)
| 08     | uint8  | [Dialog box alignment](#dialog-box-alignment)
| 09     | uint8  | [Dialog box border type](#dialog-box-borders)
| 0a     | int16  | [Text alignment](#text-alignment)
| 0c     | int16 | Font size. 16=100%, 8=50%
| 0e     | int16 | Horizontal space between letters
| 10     | int16 | Vertical space between letters
| 12     | int16 | Text X offset
| 14     | int16 | Text Y offset
| 16     | int16 | [Dialog Hook type](#dialog-hook)
| 18     | int16 | Dialog hook horizontal position
| 1a     | int16 | Text Color IDX
| 1c     | int16 | Padding
| 1e     | int16 | Padding

### Dialog box alignment

| Value | Description
|-------|-------------
| 0x0    | Use position
| 0x1    | Align to the left
| 0x2    | Align to the centre
| 0x3    | Align to the right

### Dialog box borders

| Value | Description
|-------|-------------
| 0x0    | Rounded borders
| 0x1    | Diamond borders
| 0x2    | Spike borders
| 0x3    | Black information dialog
| 0x4    | Diamond borders
| 0x5    | Invisible box

### Text alignment

| Value | Vertical | Horizontal | Text alignment
|-------|----------|------------|----------------
| 0x0    | Top      | Left       | Left
| 0x1    | Top      | Right      | Left
| 0x2    | Top      | Center     | Left
| 0x3    | Top      | Left       | Left
| 0x4    | Top      | Left       | Left
| 0x5    | Top      | Right      | Right
| 0x6    | Top      | Center     | Center
| 0x7    | Top      | Left       | Left
| 0x8    | Center   | Left       | Left
| 0x9    | Center   | Right      | Left
| 0xa    | Center   | Center     | Left
| 0xb    | Bottom   | Center     | Left
| 0xc    | Top      | Left       | Left
| 0xd    | Center   | Right      | Right
| 0xe    | Center   | Center     | Center
| 0xf    | Bottom   | Center     | Center

### Dialog hook

| Value | Shape  | Location | Origin
|-------|--------|----------|--------
| 0x0    | Hook   | Bottom   | Left
| 0x1    | Hook   | Bottom   | Right
| 0x2    | Hook   | Top      | Left
| 0x3    | Hook   | Top      | Right
| 0x4    | Bubble | Bottom   | Left
| 0x5    | Bubble | Bottom   | Right
| 0x6    | Bubble | Top      | Left
| 0x7    | Bubble | Top      | Right
| 0x8    | Spike  | Bottom   | Left
| 0x9    | Spike  | Bottom   | Right
| 0xa    | Spike  | Top      | Left
| 0xb    | Spike  | Top      | Right
| 0xc    | None   |          |
| 0xd    | None   |          |
| 0xe    | None   |          |
| 0xf    | None   |          |
