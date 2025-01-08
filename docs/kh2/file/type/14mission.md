# [Kingdom Hearts II](../../index.md) - 14mission.bin

This is the file that is used by [MSN](../../../../msn.md) files known as "Pause Menu Controller." This file ties specific ID's to predetermined functions and text ID's.
ID 1 for example is used by the game almost anytime Sora game-overs from a battle. ID 0x08 and 0x20 are used in Atlantica, and feature bitflags that prevent unpausing with the Start button.

## Header

| Offset | Type   | Description
|--------|--------|------------
| 00 	 | uint32 | File type (2)
| 04 	 | uint32 | Count
| 08     | Entry[Count]  | Entries

## Data
Entries are 64 bytes long, though there seem to be parts that are unused or unknown.
| Offset | Type   | Description
|--------|--------|------------
| 0x00 	 | uint16 | ID 
| 0x02 	 | uchar | Choice Num
| 0x03 	 | uchar | Choice Default
| 0x04 	 | uint16 | Option 1 Id
| 0x06 	 | uint16 | Option 1 TextId
| 0x08 	 | uint16 | Option 2 Id
| 0x0A 	 | uint16 | Option 2 TextId
| 0x0C 	 | uint16 | Option 3 Id
| 0x0E 	 | uint16 | Option 3 TextId
| 0x10 	 | uint16 | Option 4 Id
| 0x12 	 | uint16 | Option 4 TextId
| 0x14 	 | int16 | Base Sequence
| 0x16 	 | int16 | Title Sequence
| 0x18 	 | int32 | Information 
| 0x1C 	 | uint32 | EntryId
| 0x20 	 | int32 | Task
| 0x24   | uint8  | Pause Mode
| 0x25   | uint8  | Flag
| 0x26   | uint8  | Sound Pause
| 0x27   | uint8  | Padding (0x19)

## Option IDs
Functions are pre-defined.
Attempting to add Retry or Quit to forced fights will cause the game to crash.
| Value  | Description
|--------|------------
| 0x01 	 | Load Game Menu
| 0x02 	 | Continue
| 0x03 	 | Retry
| 0x04 	 | Quit
| 0x05 	 | Skip Scene
| 0x06 	 | Continue
| 0x08 	 | It's all over (Mickey)
| 0x09 	 | I won't give up! (Mickey)
| 0x0A 	 | Jiminy's Journal
| 0x0C 	 | Help
| 0x11 	 | Save
| 0x13 	 | Return to Menu

## Base Sequence
| Value  | Description
|--------|------------
| 0x00 	 | Slight fade on-pause
| 0x01 	 | Completely blackens the screen on-pause.
| 0x02 	 | Completely whitens the screen on-pause.
| 0x03 	 | Fades from white to black on-pause.

## Title Sequence
| Value  | Description
|--------|------------
| 0x0F 	 | Show Pause Animation
| 0x0D 	 | Positions Pause & Options Downwards
| 0xFF 	 | No Pause Animation

## Pause Mode
| Value  | Description
|--------|------------
| 0x00 	 | Do not show additional text in-battle
| 0x01 	 | Show additional text/options in-battle
| 0x02 	 | ???
| 0x04 	 | ???

## Flag
| Value  | Description
|--------|------------
| 0x00 	 | ???
| 0x01 	 | Do not allow the player to unpause with Start.
| 0x02 	 | ???
| 0x04 	 | ???

## Sound Pause
| Value  | Description
|--------|------------
| 0x00 	 | ???
| 0x01 	 | ???
| 0x02 	 | ???
| 0x04 	 | ???

