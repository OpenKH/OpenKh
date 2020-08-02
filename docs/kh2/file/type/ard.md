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

## Notes

*¹ When referred as _always 0_ means that there are no game files that set it as a value different than that. Still it is unknown if the field is actually used in-game, which would potentially lead to unused functionalities.
