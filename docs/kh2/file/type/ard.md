# [Kingdom Hearts II](../../index.md) - ARD

This describes how the game should behave once inside a map. This is responsible to spawn the player, NPC, enemies, run cutscenes, change music, invoke [MSN](msn.md) files and more.

Internally it is a [BAR](bar.md) file, composed by micro files of different purpose:

- [Spawnpoint](#spawn-point) to spawn objects.
- [Spawnscript](#spawn-script) to execute microcode
- Animload responsible to run a cutscene
- [AI](ai.md) to execute exceptional microcode.

## Spawn Point

This is the easiest micro-format of an ARD file. This is mainly responsible to spawn objects inside a map, to describe how NPCs should roaming around, to describe how to interact with a specific NPC or even triggering an event like a cutscene or just by changing a map.

### File structure

#### Header

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | File type; always 2.
| 04     | int   | [Spawn point descriptor count](#spawn-point-descriptor)

#### Spawn point descriptor

When referring to _place's door_ it means that is the index where to spawn the character to when loading the next map (simplified: which part of the map you're coming from). When the door is 99, the playable character is spawn where the save point is located.

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Unknown
| 02     | short | Unknown
| 04     | short | [Entity count](#entity)
| 06     | short | [Event activator count](#event-activator)
| 08     | short | [Walk path count](#walk-path). Usually 1.
| 0a     | short | [Unknown tableA count](#unknown-table-a)
| 0c     | int   | [Unknown tableC count](#unknown-table-c)
| 10     | int   | Always 0 [*¹](#notes)
| 14     | int   | Always 0 [*¹](#notes)
| 18     | int   | Always 0 [*¹](#notes)
| 1c     | byte  | [World's place index](../../worlds.md)
| 1d     | byte  | Place's door.
| 1e     | byte  | [World ID](../../worlds.md)
| 1f     | byte  | Unknown
| 20     | int   | Unknown
| 24     | int   | Unknown
| 28     | int   | Always 0 [*¹](#notes)

#### Entity

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | [Model ID](./00objentry.md)
| 04     | float | Position X  
| 08     | float | Position Y
| 0c     | float | Position Z
| 10     | float | Rotation X
| 14     | float | Rotation Y
| 18     | float | Rotation Z
| 1c     | short | Unknown
| 1e     | short | Unknown
| 20     | int   | Unknown
| 24     | int   | AI parameter
| 28     | int   | Talk message
| 2c     | int   | Reaction command
| 30     | int   | Unknown
| 34     | int   | Always 0 [*¹](#notes)
| 38     | int   | Always 0 [*¹](#notes)
| 3c     | int   | Always 0 [*¹](#notes)

#### Event activator

This is an invisible wall that is responsibe to activate an event. Which event is it, it is described in the [spawn point descriptor](#spawn-point-descriptor). One common usage is changing the map when the player touch the map's "border".

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Unknown
| 04     | float | Position X  
| 08     | float | Position Y
| 0c     | float | Position Z
| 10     | float | Scale X
| 14     | float | Scale Y
| 18     | float | Scale Z
| 1c     | float | Rotation X
| 20     | float | Rotation Y
| 24     | float | Rotation Z
| 28     | int   | Unknown
| 2c     | int   | Unknown
| 30     | int   | Always 0 [*¹](#notes)
| 34     | int   | Always 0 [*¹](#notes)
| 38     | int   | Always 0 [*¹](#notes)
| 3c     | int   | Always 0 [*¹](#notes)

#### Walk path

Instructs all the [entities](#entity) to follow a walk path, defined by multiple points in the map.

From some early tests, the entities would pick a random point in the walk path and walk there from they were started from. There is no known way to make it follow in order. Also when the map is loaded, the entity can be randomly teleported in any of those walk path points.

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | Unknown
| 02     | short | [Walk point count](#walk-point)
| 04     | short | Unknown
| 06     | short | Unknown

#### Walk point

Just a 12-byte structure, read as Vector3f.

### Unknown table A

| Offset | Type  | Description
|--------|-------|------------
| 00     | byte  | Unknown
| 01     | byte  | Unknown
| 02     | byte  | Unknown
| 03     | byte  | Unknown
| 04     | byte  | Unknown
| 05     | byte  | Unknown
| 06     | short | Unknown
| 08     | int   | Unknown
| 0c     | int   | Unknown

### Unknown table C

| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Unknown
| 04     | int   | Unknown

## Spawn Script

This is a micro-code that is responsible to change the behaviour of a specific loaded map.

Every map have three spawn script files: `map`, `btl` and `evt`, loaded in this very speicfic order. Each file have a list of programs, where every program is represented by an unique ID. When a map loads, the game instructs the map loader which program to load ([sub_181cc0](#notes)). A map can load only a single program per spawn script file, but a script can load multiple programs.

### Structure

The file begins with a list of programs, defined as followed:

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | [Program](#program) ID
| 02     | short | [Program](#program) length in bytes
| 04     | byte[] | Program data

### Program

Each program is a list of functions, defined as followed:

| Offset | Type  | Description
|--------|-------|------------
| 00     | short | [Operation code](#operation-code)
| 02     | short | Parameter count for the operation
| 04     | int[] | Parameters

### Operation code

There is a total of 30 operation codes for the spawn script. The parser can be found at [sub_181d30](#notes).

- 00: [Spawn](#spawn)
- 01: [MapOcclusion](#mapocclusion)
- 02: [MultipleSpawn](#multiplespawn)
- 03: [unknown](#unknown03)
- 04: [unknown](#unknown04)
- 05: [unknown](#unknown05)
- 06: [unknown](#unknown06)
- 07: [unknown](#unknown07)
- 09: [unknown](#unknown09)
- 0a: [unknown](#unknown0a)
- 0b: [unknown](#unknown0b)
- 0c: [Scene](#scene)
- 0e: [unknown](#unknown07e)
- 0f: [Bgm](#bgm)
- 10: [Party](#party)
- 11: [unknown](#unknown11)
- 12: [unknown](#unknown12)
- 13: [unknown](#unknown13)
- 14: [unknown](#unknown14)
- 15: [Mission](#mission)
- 16: [Bar](#bar)
- 17: [unknown](#unknown17)
- 18: [unknown](#unknown18)
- 19: [unknown](#unknown19)
- 1a: [unknown](#unknown1a)
- 1b: [unknown](#unknown1b)
- 1c: [unknown](#unknown1c)
- 1d: [unknown](#unknown1d)
- 1e: [BattleLevel](#battlelevel)
- 1f: [unknown](#unknown1f)

#### Spawn

Loads a [Spawn Point](#spawn-point) file. The parameter is a 4-byte string with the name of the spawn script of the ARD's BAR file. This is the most used opcode, with a usage count of 11949.

#### MapOcclusion

Specify a 64-bit mask that is responsible to enable or disable specific map meshes and collisions. This is very common and used 2186 times.

#### MultipleSpawn

This is a bit unknown, but it seems to be used to only load `b_xx` type of spawnpoint. It is probably used to chain multiple battles (eg. spawn more enemies when the current enemeies are all defeated). There are multiple parameter specified based on the function's parameter count, where each one of them looks like [Spawn](#spawn) parameter.

Not common (25 times), used in maps like `bb00`, `bb06`, `ca14`, etc. .

#### Unknown03

This apparently loads only `b_xx` type of spawnpoints. The first parameter is an unknown integer, while the second one is a 4-byte string.
Very uncommon, used 17 times and only in the worlds BB, CA, LK and MU.

#### Unknown04

Set the memory area `01c6053c` with the 4-byte parameter. The purpose of that memory area is unknown. It's found 465 times and only in `btl`.

#### Unknown05

Set the memory area `0034ecd0` with the 4-byte parameter. The purpose of that memory area is unknown. Found 34 times almost every time in `btl`.

#### Unknown06

Set the memory area `0034ecd8` with the 4-byte parameter. The purpose of that memory area is unknown. Found 64 times and only in `btl`.

#### Unknown07

Set the memory area `0034ecdc` with the 4-byte parameter. The purpose of that memory area is unknown. Very uncommon as it's ony found 7 times in the maps `ca12` and `nm02` in `btl`.

#### Unknown09

Looks like similar to [Spawn](#spawn), but it's way less used. Found 210 times, mostly in `map` and just once in `wi00` as `evt`.

#### Unknown0a

Found 73 times. Purpose unknown.

#### Unknown0b

Set the memory area `0034ecc8` with the first 4-byte parameter. But it always seems to have 2 parameters. The purpose of that memory area is unknown. Found 203 times and only in `map`.

#### Scene

This is the most compelx op-code, since internally it process something we would call [scene script](#scene-script). There is currently no way to entirely parse this opcode. This is commonly used, as the usage count tops 2408 times. It is only found in `evt` scripts.

#### Unknown0e

Set the memory area `0034ece0` with the first 4-byte parameter. The purpose of that memory area is unknown. Used 323 times and only in `map`.

#### Party

Set how the party member needs to be structured. According to `dbg/member.bin`, those are the only allowed values as a parameter:

| Value | Name          | Description
|-------|---------------|-------------
| 0     | NO_FRIEND     | Allow only Sora
| 1     | DEFAULT       | Allow Donald and Goofy
| 2     | W_FRIEND      | Allow Donald, Goofy and world guest.
| 3     | W_FRIEND_IN   | Same as `W_FRIEND` but forces the world guest to be in at the beginning.
| 4     | W_FRIEND_FIX  | Same as `W_FRIEND_IN`, but you can not swap the world guest.
| 5     | W_FRIEND_ONLY | Allow only with Sora and the world guest.
| 6     | DONALD_ONLY   | Only allow Sora and Donald.

#### Bgm

Set the map's background musics. The single 4-byte parameter can be read as two 2-byte integers, which represents the field and battle music ID that can be found in `bgm/music_xxx`. This overrides the default field and battle music used in the current map.

#### Unknown11

Set the memory area `0034ece4` with the first 4-byte parameter. Found 80 times and only in `map`.

#### Unknown12

Set the memory area `0034ecd4` with the first 4-byte parameter. This opcode works but it is not used by any program.

#### Unknown13

Set the memory area `0034ece8` with the first 4-byte parameter. Found 39 times and only in `map` for `hb17`, `lk13`, `mu07` and `po00`, with a parameter of `1` or `2`.

#### Unknown14

Set the 3rd bit of the memory area `0034f240`. Used 89 times.

#### Mission

Loads a [mission](msn.md) file by specifying its MSN file name without path and extension.

#### Bar

Loads a [BAR](bar.md) file by specifying the entire path. The `%s` symbol can be specified in the path to not hard-code the script to work just for a single language. The BAR file can be a [layout](2ld.md) or a `minigame` file.

#### Unknown17

Set the 5th bit of the memory area `0034f240`. This is only used twice in `mu07` and might be related to the mechanic of Sora entering in the tornado.

#### Unknown18

Set the memory area `0034ecec` and `0034ecf0` with the first two parameters. It is only used by `hb33`, `hb34`, `hb38` and `he15` in `btl`.

#### Unknown19

Unknown.

#### Unknown1a

It seems to do something with `01c60548`. It is only used 3 times in `hb13` by `evt`.

#### Unknown1b

It seems to do something with `01c60550`. It is only used 3 times in `hb13` by `evt` and in the same programs used by [Unknown1a](#unknown1a).

#### Unknown1c

Seems to recursively call the spawn script parser, but it is only used in `he09` for 130 times. Purpose unknown.

#### Unknown1d

Purpose unknown. Used 196 times.

#### BattleLevel

Override the battle level of the playing map. Usually used for special boss battle only.

#### Unknown1f

Purpose unknown. Very similar to [Unknown03](#unknown03)

### Scene script

The scene script contains functions with a variable amount of parameters.

| Opcode | Name     | Description
|--------|----------|-------------
| 00     | Cutscene | Play a cutscene. param1 cutscene file, param2 unknown
| 01     |          |
| 02     |          |
| 03     |          |
| 04     |          |
| 05     |          |
| 06     | GetItem  | Obtain an item. It is possible to obtain up to 7 items in a row.
| 07     |          |
| 08     |          |

## Notes

*¹ When referred as _always 0_ means that there are no game files that set it as a value different than that. Still it is unknown if the field is actually used in-game, which would potentially lead to unused functionalities.

*² When `sub_`_`xxxxxx`_ or a memory address is read, they refer to the values found in the retail version of Kingdom Hearts II: Final Mix, executable `SLPM_66675.elf`.
