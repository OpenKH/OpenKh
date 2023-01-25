# [Kingdom Hearts II](../../index.md) - 00objentry.bin

Contains a definition of every object and it's parameters.

## Structure

| Offset | Type | Description |
|--------|------|-------------|
| 0x00   | uint32 | ID
| 0x04   | uint8 | [Object Type](#object-types)
| 0x05   | uint8 | Subtype
| 0x06   | uint8 | Draw priority
| 0x07   | uint8 | Weapon Joint - Points to [Sklt](03system.md#sklt)
| 0x08   | char[32] | Model Name
| 0x28   | char[32] | Animation Name
| 0x48   | uint16 | [Flags](#flags)
| 0x4A   | uint8 | [Target Type](#target-type)
| 0x4B   | uint8 | Padding
| 0x4C   | uint16 | Neo Status
| 0x4E   | uint16 | Neo Moveset
| 0x50   | float | Weight
| 0x54   | uint8 | Spawn Limiter
| 0x55   | uint8 | Page (unknown)
| 0x56   | uint8 | [Shadow Size](#shadow-size)
| 0x57   | uint8 | [Form](#form-fm)
| 0x58   | uint16 | Spawn additional object 1 - [OBJ LIST](../../dictionary/obj.md)
| 0x5A   | uint16 | Spawn additional object 2 - [OBJ LIST](../../dictionary/obj.md)
| 0x5C   | uint16 | Spawn additional object 3 - [OBJ LIST](../../dictionary/obj.md)
| 0x5E   | uint16 | Spawn additional object 4 - [OBJ LIST](../../dictionary/obj.md)

### Object Types

| Id   | Name (ELF) | Description |
|------|------------|-------------|
| 0x00 | PLAYER   | Player
| 0x01 | FRIEND   | Party Member
| 0x02 | NPC      | NPC
| 0x03 | BOSS     | Boss (makes a finsher required to kill)
| 0x04 | ZAKO     | Normal Enemy
| 0x05 | WEAPON   | Weapon
| 0x06 | E_WEAPON | Placeholders for ARD files
| 0x07 | SP       | World/Save Point
| 0x08 | F_OBJ    | Neutral (can be damaged by both allies and enemies)
| 0x09 | BTLNPC   | Partner (out of party allies)
| 0x0A | TREASURE | Chest
| 0x0B | SUBMENU  | Moogle
| 0x0C | L_BOSS   | Large Boss
| 0x0D | G_OBJ    | Unknown
| 0x0E | MEMO     | Pause Menu Dummy (walking models in Pause Menu)
| 0x0F | RTN      | Unknown
| 0x10 | MINIGAME | Unknown
| 0x11 | WORLDMAP | Objects on the World Map
| 0x12 | PRIZEBOX | Drop Item Container
| 0x13 | SUMMON   | Summon Partner
| 0x14 | SHOP     | Shop Point
| 0x15 | L_ZAKO   | Normal Enemy
| 0x16 | MASSEFFECT | Crowd Spawner
| 0x17 | E_OBJ    | Unknown
| 0x18 | JIGSAW   | Puzzle Piece

### Flags

| Position | Size | Description |
|----------|------|-------------|
| 0 | 1 | No APDX
| 1 | 1 | Before
| 2 | 1 | Fix Color
| 3 | 1 | Fly
| 4 | 1 | Scissoring
| 5 | 1 | Pirate
| 6 | 1 | OCC Wall
| 7 | 1 | Hift

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

| Id   | Description |
|------|-------------|
| 0x00 | Sora / Roxas
| 0x01 | Valor Form
| 0x02 | Wisdom Form
| 0x03 | Limit Form
| 0x04 | Master Form
| 0x05 | Final Form
| 0x06 | Anti Form
| 0x07 | Lion King Sora
| 0x08 | Atlantica Sora (Magic, Drive, Party and Limit commands are greyed out)
| 0x09 | Sora on Carpet (Drive, Party and Limit commands are greyed out)
| 0x0A | Roxas Dual-Wield
| 0x0B | Default (used on Enemies)
| 0x0C | Sora in Cube / Card Form
