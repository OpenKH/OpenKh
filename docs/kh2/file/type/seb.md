# [Kingdom Hearts II](../../index.md) - SEB (Sound effect)

Sound effect; on PC, these files are pointers to an SCD in the objects remastered folder	


## SEB Structure: PC

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | string | Seemingly always "ORIGIN". If removed, sound effects from se000 will play instead.
| 8      | uint16 | Sound ID Number; not used by the game?
| 10     | uint16 | Unknown; not used by the game?
| 12     | uint16 | Sound ID Number
| 16     | string | Filepath to SCD in objects remastered folder.

Offset 12 must be a unique ID. If it is not a unique ID, the SCD will not load and will instead load the last SCD with the same ID.
