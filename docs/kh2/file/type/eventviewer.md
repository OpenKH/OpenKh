# [Kingdom Hearts II](../../index.md) - eventviewer.bar

This is the file that organizes the events used in Theater Mode. It outlines which events to play and in what order they should be played in.

## Header
| Offset | Type   | Description
|--------|--------|------------
| 0x00 	 | uint32 | File type (1)
| 0x04 	 | uint32 | Entry Count

## Data
| Offset | Type   | Description
|--------|--------|------------
| 0x00 	 | uint16 | Type
| 0x02 	 | uint16 | TextId
| 0x04 	 | uint16 | World
| 0x06 	 | uint16 | ProgramID
| 0x08 	 | uint16 | ProgramID
| 0x0A 	 | uint16 | Progress Flag to Unlock

"Type" is what determines a new "Chapter," a category to hold a series of events. 
When this value equals 0, this indicates the start of a new "Chapter" as referred to by the game.
Every entry afterwards with a Type value of 1 will be found inside this Chapter.
Additionally, the ProgramID for Type 0 determines which cutscene will play first when the "Play all" option is selected.

TextId's are found inside msg/xx/title.bar.

It is unknown why some Program ID's are repeated twice. The game always seems to play the first Program ID, and not the second.

"Progress Flag to Unlock" refers to whether or not a specific Progress Flag must be triggered to unlock that Chapter.
This is used not only for the Secret Movies, but optional world visits like Pride Lands, so that the player cannot rewatch cutscenes they had never seen.
