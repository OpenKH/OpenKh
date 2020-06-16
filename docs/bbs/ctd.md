# [Kingdom Hearts Birth By Sleep](index.md) - CTD format

CTD is used to display text messages in game. It does not only contain the text to write on screen, but also the layout, font type, position and more.

In order to make a specific text appear in-game, the engine selects a [message](#message) thorugh its unique identifier. Then, every message will refer to a [layout](#layout) which contains the display logic.

## Text encoding

The encoding used is [Shift-JIS](https://en.wikipedia.org/wiki/Shift_JIS). The European/American text uses a slighly different variant of Shift-JIS. To learn more on how the game engine renders those characters, check how [character mapping](font#character-mapping) is performed.

## File format

### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File identifier, `0x44544340`
| 04     | int   | Version, always `1`
| 08     | short | RESERVED
| 0a     | short | Unknonw
| 0c     | short | [Layout](#layout) count
| 0e     | short | [Message](#message) count
| 10     | int   | [Message](#message) offset
| 14     | int   | [Layout](#layout) offset
| 18     | int   | Text start offset
| 1c     | int   | RESERVED

### Message

Describe a message with a text and links it to a layout

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Unique identifier
| 02     | short | Unknown
| 04     | int   | Text offset
| 08     | int   | [Layout](#layout) index

### Layout

Describe how a message should be presented on screen

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Dialog X position
| 02     | short | Dialog Y position
| 04     | short | Balloon window width (borders excluded)
| 06     | short | Balloon window height (borders excluded)
| 08     | byte  | [Dialog box alignment](#dialog-box-alignment)
| 09     | byte  | [Dialog box border type](#dialog-box-borders)
| 0a     | byte  | [Text alignment](#text-alignment)
| 0b     | byte  | Unknown
| 0c     | short | Font size. 16=100%, 8=50%
| 0e     | short | Horizontal space between letters
| 10     | short | Vertical space between letters
| 12     | short | Text X offset
| 14     | short | Text Y offset
| 16     | short | [Dialog Hook type](#dialog-hook)
| 18     | short | Dialog hook horizontal position
| 1a     | short | Unknown
| 1c     | short | Unknown
| 1e     | short | Unknown

### Dialog box alignment

| Value | Description
|-------|-------------
| 00    | Use position
| 01    | Align to the left
| 02    | Align to the centre
| 03    | Align to the right

### Dialog box borders

| Value | Description
|-------|-------------
| 00    | Rounded borders
| 01    | Diamond borders
| 02    | Spike borders
| 03    | Black information dialog
| 04    | Diamond borders
| 05    | Invisible box

### Text alignment

| Value | Vertical | Horizontal | Text alignment
|-------|----------|------------|----------------
| 00    | Top      | Left       | Left
| 01    | Top      | Right      | Left
| 02    | Top      | Center     | Left
| 03    | Top      | Left       | Left
| 04    | Top      | Left       | Left
| 05    | Top      | Right      | Right
| 06    | Top      | Center     | Center
| 07    | Top      | Left       | Left
| 08    | Center   | Left       | Left
| 09    | Center   | Right      | Left
| 0a    | Center   | Center     | Left
| 0b    | Bottom   | Center     | Left
| 0c    | Top      | Left       | Left
| 0d    | Center   | Right      | Right
| 0e    | Center   | Center     | Center
| 0f    | Bottom   | Center     | Center

### Dialog hook

| Value | Shape  | Location | Origin
|-------|--------|----------|--------
| 00    | Hook   | Bottom   | Left
| 01    | Hook   | Bottom   | Right
| 02    | Hook   | Top      | Left
| 03    | Hook   | Top      | Right
| 04    | Bubble | Bottom   | Left
| 05    | Bubble | Bottom   | Right
| 06    | Bubble | Top      | Left
| 07    | Bubble | Top      | Right
| 08    | Spike  | Bottom   | Left
| 09    | Spike  | Bottom   | Right
| 0a    | Spike  | Top      | Left
| 0b    | Spike  | Top      | Right
| 0c    | None   |          |
| 0d    | None   |          |
| 0e    | None   |          |
| 0f    | None   |          |
