# [Kingdom Hearts II](../../index) - 00objentry.bin

This is an essential file for [Kingdom Hearts II](../../index) and it contains objects that can be spawn.

## Structure

| Offset | Type | Description |
|--------|------|-------------|
| 00     | ushort | ID
| 02     | ushort | Unknown
| 04     | byte | Object Type. Refer to [types](#types).
| 05     | byte | Unknown
| 06     | byte | Unknown
| 07     | byte | Weapon Joint
| 08     | char[32] | Model Name
| 28     | char[32] | Animation Name
| 48     | uint | Unknown
| 4C     | ushort | Neo Status
| 4E     | ushort | Neo Moveset
| 50     | uint | Unknown
| 54     | byte | Spawn Limiter
| 55     | byte | Unknown
| 56     | byte | Unknown
| 57     | byte | Unknown
| 58     | ushort | Spawn additional object 1
| 5A     | ushort | Spawn additional object 2
| 5C     | ushort | Spawn additional object 3
| 5E     | ushort | Unknown

### Object Types

| Id | Description |
|----|-------------|
| 00 | Player
| 01 | Party Member
| 02 | Dummy
| 03 | Boss (makes a finsher required to kill)
| 04 | Normal Enemy
| 05 | Keyblade
| 06 | Placeholders for ARD files
| 07 | World/Save Point
| 08 | Neutral (can be damaged by both allies and enemies)
| 09 | Partner (out of party allies)
| 0A | Chest
| 0B | Moogle
| 0C | Giant Boss
| 0D | Unknown
| 0E | Pause Menu Dummy (walking models in Pause Menu)
| 0F | NPC
| 10 | Unknown
| 11 | Objects on the World Map
| 12 | Drop Prize Container (???)
| 13 | Summon Partner
| 14 | Shop Point
| 15 | Normal Enemy
| 16 | Crowd Spawner
| 17 | Unknown