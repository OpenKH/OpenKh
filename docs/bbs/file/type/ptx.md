# PTX Format

PTX stands for *Pattern X* because it contains several code patterns.

This file controls things such as events triggered by OLO files or the music to play.

# PTN Type

| Value | Name  
|--------|------
| 0x0    | TYPE_MAP
| 0x1    | TYPE_BATTLE
| 0x2    | TYPE_EVENT
| 0x3    | _TYPE_MAX

## Parse Ptn Arg

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16 | Parse Flag
| 0x2    | uint16 | Section Size (from Code onwards)
| 0x4    | int16  | [Code](#CODE)
| 0x6    | int16  | Value
| 0x8    | uint32 | [Group](#Group)

## CODE

| Value | Name  
|--------|------
| 0x1   | PTNCODE_SETFILE
| 0x4   | PTNCODE_RESET_MENUFLAG
| 0x5   | PTNCODE_SET_MENUFLAG
| 0x6   | PTNCODE_P_CHARA
| 0x7   | PTNCODE_BGM
| 0x8   | PTNCODE_SET_PARAGRAPH
| 0x9   | PTNCODE_MISSION
| 0xA   | PTNCODE_TRG_ACTION
| 0xB   | PTNCODE_ENEMY_CHANGE
| 0x0000FFFF   | PTNCODE_TERMINATOR

## Group

| Value | Name  
|--------|------
| 0x0    | GRP_DEFAULT
| 0x4D   | _GARBAGE_STEP
| 0x3E8  | GRP_RES_SYSTEM
| 0x3E9  | GRP_EX_SET_PATTERN
| 0x44B  | GRP_EVENT_CATALOG
| 0x44C  | GRP_RES_GAME
| 0x4AF  | GRP_RES_PLAYER_CLEAR
| 0x4B0  | GRP_RES_PLAYER
| 0x577  | GRP_RES_WORLD_CLEAR
| 0x578  | GRP_RES_WORLD
| 0x579  | GRP_RES_WORLD_SE
| 0x5DC  | GRP_SCENE_PRESET
| 0x63F  | GRP_SCENE_CLEAR
| 0x640  | GRP_MISSION
| 0x6A4  | GRP_BG
| 0x6D6  | GRP_GIMMICK
| 0x76C  | GRP_MISSION_COMPOSE
| 0x7D0  | GRP_EVENT_CLEAR
| 0x834  | GRP_BDCOMMON
| 0x83E  | GRP_BDMAP
| 0x848  | GRP_BDPLAYER
| 0x852  | GRP_BDNPC
| 0x85C  | GRP_BDTITLE
| 0x866  | GRP_BDSPEVENT
| 0x870  | GRP_BDRESULT
| 0x8FB  | _GRP_DEFAULT_0_TAIL
| 0x8FC  | GRP_PLAYER

## Parse Pattern Argument

Use unknown.

| Value | Name  
|--------|------
| 0x2   | PARSE_SETFILE
| 0x10  | PARSE_RESET_MENUFLAG
| 0x20  | PARSE_SET_MENUFLAG
| 0x40  | PARSE_P_CHARA
| 0x80  | PARSE BGM
| 0x100 | PARSE_PARAGRAPH
| 0x200 | PARSE_MISSION
| 0x400 | PARSE_TRG_ACTION
| 0x800 | PARSE_ENEMY_CHANGE
| 0xFF2 | PARSE_ALL

---
# TYPE_EVENT

## PTNCODE_SETFILE

Controls which spawns appear in the level and what flag needs to be risen for them to be triggered.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16 | Code `Always 0x1`
| 0x2     | uint16 | String Count
| 0x4     | string[String Count] | Name of OLO files to spawn. Given an olo name `{world}{area}-{ID}.olo`, only the `ID` section is written.

## PTNCODE_BGM

This section can be added to change the music applied to an event, usually inside BTL.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16 | Code `Always 0x7`
| 0x2     | uint16 | Value
| 0x4     | uint16 | Song index to play 
| 0x6     | uint16 | unk6 // Seems to be 0xFFFF most of the time

## PTNCODE_MISSION

This section can be added to change the music applied to an event, usually inside BTL.

| Offset | Type  | Description
|--------|-------|------------
| 0x0     | uint16 | Code `Always 0x9`
| 0x2     | uint16 | Value `Always 0x4`
| 0x4     | char[16] | Mission to Trigger

## PTNCODE_TRG_ACTION

This one occupies 4 bytes per instance.

### Room Teleport

| Offset | Type  | Description
|--------|-------|------------
| 0x0    | uint16 | World
| 0x2    | uint16 | Room
| 0x4    | uint16 | Entrance
| 0x6    | uint16 | Unknown

# Jump Type

| Value | Name  
|--------|------
| 0x0    | TYPE_WIPE
| 0x1    | TYPE_BLACK
| 0x2    | TYPE_WHITE
| 0xFFFFFFFD    | TYPE_NONE
| 0xFFFFFFFE    | TYPE_DEFAULT_EXCEPT_WIPE
| 0xFFFFFFFF    | TYPE_DEFAULT

---

# P_CHARA

Unused enumeraion.

| Value | Name  
|--------|------
| 0x1    | P_CHARA_VE
| 0x2    | P_CHARA_AQ
| 0x3    | P_CHARA_TE
| 0x10   | P_CHARA_NORMAL
| 0x20   | P_CHARA_ARMOR
| 0x30   | P_CHARA_RIDE
| 0x40   | P_CHARA_RIDE_ARMOR
| 0x50   | P_CHARA_HALF_ARMOR