# [Kingdom Hearts II](../../index.md) - 00objentry.bin

Contains a definition of every object and it's parameters.

## Structure

| Offset | Type | Description |
|--------|------|-------------|
| 00     | uint | ID
| 04     | byte | [Object Type](#object-types)
| 05     | byte | Subtype
| 06     | byte | Draw priority
| 07     | byte | Weapon Joint - Points to [Sklt](03system.md#sklt)
| 08     | char[32] | Model Name
| 28     | char[32] | Animation Name
| 48     | ushort | [Flags](#flags)
| 4A     | byte | [Target Type](#target-type)
| 4B     | byte | Padding
| 4C     | ushort | Neo Status
| 4E     | ushort | Neo Moveset
| 50     | float | Weight*
| 54     | byte | Spawn Limiter
| 55     | byte | Page (unknown)
| 56     | byte | [Shadow Size](#shadow-size)
| 57     | byte | [Form](#form-fm)
| 58     | ushort | Spawn additional object 1 - [OBJ LIST](../../dictionary/obj.md)
| 5A     | ushort | Spawn additional object 2 - [OBJ LIST](../../dictionary/obj.md)
| 5C     | ushort | Spawn additional object 3 - [OBJ LIST](../../dictionary/obj.md)
| 5E     | ushort | Spawn additional object 4 - [OBJ LIST](../../dictionary/obj.md)

*The first bit is somewhat related to the enemy state. Eg: Undead Pirates lose their immunity if they are changed from 2 to 0

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

### Flags

| Id   | Description |
|------|-------------|
| 0x01 | No APDX
| 0x02 | Before
| 0x04 | Fix Color
| 0x08 | Fly
| 0x10 | Scissoring
| 0x20 | Pirate
| 0x40 | OCC Wall
| 0x80 | Hift

### Target Type

Ally damage cap (01 for normal damage, 02 for chip damage...)

| Id   | Description |
|------|-------------|
| 0x0 | Target Type M
| 0x1 | Target Type S
| 0x2 | Target Type L

### Shadow Size

| Id  | Description |
|-----|-------------|
| 0x0 | No shadow
| 0x1 | Small shadow
| 0x2 | Middle shadow
| 0x3 | Large shadow
| 0x4 | Small moving shadow
| 0x5 | Middle moving shadow
| 0x6 | Large moving shadow

### Form (FM)

Controls which actions are possible in the command menu.

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
| 08 | Atlantica Sora (Magic, Drive, Party and Limit commands are greyed out)
| 09 | Sora on Carpet (Drive, Party and Limit commands are greyed out)
| 0A | Roxas Dual-Wield
| 0B | Default (used on Enemies)
| 0C | Sora in Cube / Card Form