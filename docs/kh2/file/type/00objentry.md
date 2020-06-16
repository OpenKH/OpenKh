# [Kingdom Hearts II](../../index.md) - 00objentry.bin

Contains a definition of every object and it's parameters.

## Structure

| Offset | Type | Description |
|--------|------|-------------|
| 00     | ushort | ID
| 02     | ushort | Unknown
| 04     | byte | [Object Type](#object-types)
| 05     | byte | Unknown
| 06     | byte | Unknown
| 07     | byte | Weapon Joint
| 08     | char[32] | Model Name
| 28     | char[32] | Animation Name
| 48     | ushort | Unknown
| 4A     | ushort | Unknown
| 4C     | ushort | Neo Status
| 4E     | ushort | Neo Moveset
| 50     | ushort | Unknown
| 52     | short | Weight
| 54     | byte | Spawn Limiter
| 55     | byte | Unknown
| 56     | byte | Unknown
| 57     | byte | [Command Menu Options](#command-menu-options-fm)
| 58     | ushort | Spawn additional object 1
| 5A     | ushort | Spawn additional object 2
| 5C     | ushort | Spawn additional object 3
| 5E     | ushort | Unknown

### Object Types

| Id | Name (ELF) | Description |
|----|------------|-------------|
| 00 | PLAYER   | Player
| 01 | FRIEND   | Party Member
| 02 | NPC      | NPC
| 03 | BOSS     | Boss (makes a finsher required to kill)
| 04 | ZAKO     | Normal Enemy
| 05 | WEAPON   | Weapon
| 06 | E_WEAPON | Placeholders for ARD files
| 07 | SP       | World/Save Point
| 08 | F_OBJ    | Neutral (can be damaged by both allies and enemies)
| 09 | BTLNPC   | Partner (out of party allies)
| 0A | TREASURE | Chest
| 0B | SUBMENU  | Moogle
| 0C | L_BOSS   | Large Boss
| 0D | G_OBJ    | Unknown
| 0E | MEMO     | Pause Menu Dummy (walking models in Pause Menu)
| 0F | RTN      | Unknown
| 10 | MINIGAME | Unknown
| 11 | WORLDMAP | Objects on the World Map
| 12 | PRIZEBOX | Drop Item Container
| 13 | SUMMON   | Summon Partner
| 14 | SHOP     | Shop Point
| 15 | L_ZAKO   | Normal Enemy
| 16 | MASSEFFECT | Crowd Spawner
| 17 | E_OBJ    | Unknown
| 18 | JIGSAW   | Puzzle Piece

### Command Menu Options (FM)

| Id | Description |
|----|-------------|
| 00 | Sora / Roxas
| 01 | Valor Form
| 02 | Wisdom Form
| 03 | Limit Form
| 04 | Master Form
| 05 | Final Form
| 06 | Anti Form
| 07 | Lion King Sora
| 08 | Magic, Drive, Party and Limit commands are greyed out
| 09 | Drive, Party and Limit commands are greyed out (not used ingame)
| 0A | Same as 08, only used in P_EX110_BTLF (Roxas Dual-Wield)
| 0B | Only Attack and Summon commands are available, default
| 0C | Sora in Cube / Card Form (Luxord battle, not used ingame)