# [Kingdom Hearts Birth By Sleep](index.md) - OLO format (Object Spawner)

This file controls what objects and how they're spawned in levels within [Kingdom Hearts Birth by Sleep](../../index).

# Header
| Offset | Type  | Description
|--------|-------|------------
| 00     | char[4]   | File identifier, always `@OLO`
| 04     | short   | Number of Header Extra Info in header
| 06     | short | Number of Spawner Detail Entries in header
| 08     | int | Number of unique objects to spawn
| 0c     | int | Header length
| 10     | HeaderExtraInfo[NumHeaderExtra]   | Extra info in header
| 3C     | int   | padding

# Spawner List
| Offset | Type  | Description
|--------|-------|------------
| Header.HeaderLength     | string[NumObjectsSpawn]   | List of objects to spawn in the level

# Spawner Info
| Offset | Type  | Description
|--------|-------|------------
| HeaderExtraInfo.Offset     | SpawnerDetailEntry[NumSpawnerDetailEntry]   | Includes extra info on each spawner


## Header Extra Info
| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Unknown
| 04     | int   | Offset to Detail Entry

## Spawner Detail Entry
| Offset | Type  | Description
|--------|-------|------------
| 00     | int   | Unknown
| 04     | int   | Unknown
