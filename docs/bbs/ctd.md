# [Kingdom Hearts Birth By Sleep](index) - CTD format

CTD is used to display text messages in game. It does not only contain the text to write on screen, but also the layout, font type, position and more.

In order to make a specific text appear in-game, the engine selects a [message](#message) thorugh its unique identifier. Then, every message will refer to a [layout](#layout) which contains the display logic.

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
| 00     | short | Position X
| 02     | short | Position Y
| 04     | short | *winW
| 06     | short | *winH
| 08     | byte  | *formatType1
| 09     | byte  | *dialogType
| 0a     | byte  | *formatType2
| 0b     | byte  | *unk1
| 0c     | short | Font size
| 0e     | short | *unk2
| 10     | short | *fontSeparation
| 12     | short | *unk3
| 14     | short | *unk4
| 16     | short | *unk5
| 18     | short | *unk6
| 1a     | short | *color
| 1c     | short | *unk7
| 1e     | short | *unk8