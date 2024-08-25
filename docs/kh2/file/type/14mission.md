# [Kingdom Hearts II](../../index.md) - 14mission.bin

This is the file that is used by [MSN](../../../../msn.md) files known as "Pause Menu Controller." This file ties specific ID's to predetermined functions and text ID's.
ID 1 for example is used by the game almost anytime Sora game-overs from a battle due.

## Header

| Offset | Type   | Description
|--------|--------|------------
| 0 	 | uint32 | File type (2)
| 4 	 | uint32 | Entry Count

## Data
Entries are 64 bytes long, though there seem to be parts that are unused or unknown.
| Offset | Type   | Description
|--------|--------|------------
| 0x00 	 | uint16 | ID 
| 0x02 	 | uint16 | Number of Options
| 0x04 	 | uint16 | Option 1 Function
| 0x06 	 | uint16 | Option 1 TextId
| 0x08 	 | uint16 | Option 2 Function
| 0x0A 	 | uint16 | Option 2 TextId
| 0x0C 	 | uint16 | Option 3 Function
| 0x0E 	 | uint16 | Option 3 TextId
| 0x10 	 | uint16 | Option 4 Function
| 0x12 	 | uint16 | Option 4 TextId
| 0x14 	 | uint16 | Pause Effect
| 0x16 	 | uint16 | Pause Effect (2)
| 0x18 	 | uint16 | Unknown
| 0x1A 	 | uint16 | Unknown
| 0x1C 	 | uint16 | Unknown
| 0x1E 	 | uint16 | Unknown
| 0x20 	 | uint16 | Unknown
| 0x22 	 | uint16 | Unknown
| 0x24   | uint8  | Bitflag 1
| 0x25   | uint8  | Bitflag 2
| 0x26   | uint8  | Bitflag 3
| 0x27   | uint8  | Padding (0x19)

##Functions
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

##Pause Effects
| Value  | Description
|--------|------------
| 0x00 	 | Slight fade on-pause
| 0x01 	 | Completely blackens the screen on-pause.
| 0x02 	 | Completely whitens the screen on-pause.
| 0x03 	 | Fades from white to black on-pause.

##Pause Effects (2)
| Value  | Description
|--------|------------
| 0x0F 	 | Show Pause Animation
| 0x0D 	 | Positions Pause & Options Downwards
| 0xFF 	 | No Pause Animation

##Bitflag 1
| Value  | Description
|--------|------------
| 0x00 	 | Do not show additional text in-battle
| 0x01 	 | Show additional text/options in-battle
| 0x02 	 | ???
| 0x04 	 | ???

##Bitflag 2
| Value  | Description
|--------|------------
| 0x00 	 | ???
| 0x01 	 | Do not allow the player to unpause with Start.
| 0x02 	 | ???
| 0x04 	 | ???

##Bitflag 3
| Value  | Description
|--------|------------
| 0x00 	 | ???
| 0x01 	 | ???
| 0x02 	 | ???
| 0x04 	 | ???

