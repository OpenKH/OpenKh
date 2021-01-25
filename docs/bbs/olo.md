# [Kingdom Hearts Birth By Sleep](index.md) - OLO format (Object Spawner)

<<<<<<< HEAD
This file controls what objects and how they're spawned in levels within [Kingdom Hearts Birth by Sleep](../../index.md).

# Header
| Offset | Type  | Description
|--------|-------|---------------
| 0x0     | char[4]  | File identifier, always `@OLO`.
| 0x4     | ushort   | File version.
| 0x6     | ushort   | Flag from [OLO Flags](###OLO-Flags)
| 0x8     | uint     | Number of Objects to spawn.
| 0xC     | uint     | Offset to the [Object Name](###Object-Name) section.
| 0x10    | uint     | Number of file path addresses.
| 0x14    | uint     | Offset to a list of [Path Name](###Path-Name) for **files**.
| 0x18    | uint     | Number of Script name
| 0x1C    | uint     | Offset to a list of [Path Name](###Path-Name) for **Scripts**.
| 0x20    | uint     | Number of Mission labels.
| 0x24    | uint     | Offset to the [Mission Name](###Mission-Name) definitions.
| 0x28    | uint     | Number of Triggers.
| 0x2C    | uint     | Offset to the [Trigger Data](###Trigger-Data) definitions.
| 0x30    | uint     | Number of Group data.
| 0x34    | uint     | Offset to the [Group Data](###Group-Data) definitions.
| 0x38    | uint[2]  | Padding.

### **OLO Flags**
| Value |       Name            | Description
|-------|-----------------------|--------------
| 0     | FLAG_NONE             | Unused
| 1     | FLAG_ENEMY            | Used for regular enemies
| 2     | FLAG_GIMMICK          | Used for gimmick objects
| 4     | FLAG_NPC              | Used for Non Playable Characters
| 8     | FLAG_PLAYER           | Used for Player Characters
| 16    | FLAG_EVENT_TRIGGER    | Used for triggers

OLO Flags is a bitfield.

# Object Name
| Type  | Description
|-------|------------
| char[16]   | File to load.

`Object Name` just consists of a list of Objects where its count is decided by `uiObjNameNum`

# Path Name Section
| Type  | Field Name
|-------|------------ 
|  char[32]  | szName

# Mission Name
| Type  
|-------
|  string 

# Trigger Data
| Offset | Type  | Description
|--------|-------|------------
| 0x0    | Vector3  | Trigger location.
| 0xC    | Vector3  | Trigger scale.
| 0x18   | uint  | Trigger ID. (uiID)
| 0x1C   | uint  | [Trigger Behavior](###Trigger-Behavior)
| 0x20   | ushort[2]  | Unknown Parameters
| 0x24   | uint  | ID of CTD file to load
| 0x28   | uint  | Possibly a reference to [Trigger Type](###Trigger-Type) (Game Trigger)
| 0x2C   | float  | Yaw rotation.

### Trigger Behavior
| Bit   | Count  | Behavior
|-------|-------|--------------
| 0     | 4  | [Type](###Trigger-Type)
| 4     | 4  | [Shape](###Trigger-Shape)
| 8     | 1  | Fire
| 9     | 1  | Stop
| 10    | 22 | Padding

### Trigger Type
| Value | Name  | Description
|-------|-------|--------------
| 0     | Scene Jump
| 1     | Appear Enemy
| 2     | Begin Gimmick
| 3     | Begin Event
| 4     | Destination
| 5     | Message
| 6     | Mission

### Trigger Shape
| Value |Description
|-------|--------------
| 0     | Box
| 1     | Sphere
| 2     | Cylinder

# Group Data
| Offset | Type  | Description
|--------|-------|------------
| 0x0    | Vector3 | Object center location.
| 0xC    | float | Radius of object.
| 0x10   | uint | ID of the Trigger associated
| 0x14   | uint | [Group Flag](###Group-Flag)
| 0x18   | float | Appear Parameter
| 0x1C   | uint | Offset to Group Data (?)
| 0x20   | float | Dead Rate
| 0x24   | ushort | Game Trigger
| 0x26   | uint8 | Mission Parameter
| 0x27   | uint8 | Unknown Parameter
| 0x28   | uint | Number of Layout Object Data entities.
| 0x2A   | uint | Offset to the [Layout Data](###Layout-Data).

### Group Flag
| Bit   | Count  | Behavior
|-------|-------|--------------
| 0     | 4  | Appear Type
| 4     | 1  | Link
| 5     | 1  | Appear OK
| 6     | 1  | Link Invoke
| 7     | 4  | Step
| 11    | 1  | Fire
| 12    | 8  | ID
| 20    | 1  | Specified
| 21    | 1  | Game Trigger to Fire
| 22    | 1  | Mission to Fire
| 23    | 1  | All Dead No Appear
| 24    | 5  | Group ID
| 29    | 3  | Padding

### Appear Type
| Value | Name  | Description
|--------|-------|--------------
| 0    | APPEAR_TYPE_NONE |
| 1    | APPEAR_TYPE_PLAYER_DISTANCE |
| 2    | APPEAR_TYPE_SPECIFIED |
| 3    | APPEAR_TYPE_NPC_DISTANCE |

# Layout Data
| Offset | Type  | Description
|--------|-------|------------
| 0x0    | char[16] | Object Name
| 0x10   | Vector3 | Position
| 0x1C   | Vector3 | Rotation
| 0x20   | float | Height
| 0x24   | uint | [Layout Info](###Layout-Info)
| 0x28   | uint | Unique ID
| 0x2C   | ushort | Parameter 5
| 0x2E   | ushort | Parameter 6
| 0x30   | ushort | Parameter 7
| 0x32   | ushort | Parameter 8
| 0x34   | uint | Message ID
| 0x38   | char[32] | Path Name
| 0x58   | char[32] | Script Name
| 0x78   | char[16] | Mission Label

 ### Layout Info
 | Bit   | Count  | Behavior
|-------|-------|--------------
| 0     | 1  | Appear
| 1     | 1  | Load Only
| 2     | 1  | Dead
| 3     | 8  | ID
| 11    | 1  | Model Display Off
| 12    | 8  | Group ID
| 20    | 1  | No Load
| 21    | 8  | Network ID
| 29    | 3  | Padding
=======
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
>>>>>>> e5f64d008e0a83ff49f89813fc49c3c79fdaab1a
