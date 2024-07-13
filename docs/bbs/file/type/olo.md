# [Kingdom Hearts Birth By Sleep](index.md) - OLO format (Object Spawner)

This file controls what objects and how they're spawned in levels within [Kingdom Hearts Birth by Sleep](../../index.md).

# Header

| Offset | Type  | Description
|--------|-------|---------------
| 0x0     | char[4]  | File identifier, always `@OLO`.
| 0x4     | uint16   | File version.
| 0x6     | uint16   | Flag from [OLO Flags](###OLO-Flags)
| 0x8     | uint32     | Number of Objects to spawn.
| 0xC     | uint32     | Offset to the [Object Name](###Object-Name) section.
| 0x10    | uint32     | Number of file path addresses.
| 0x14    | uint32     | Offset to a list of [Path Name](###Path-Name) for **Files**.
| 0x18    | uint32     | Number of Script name
| 0x1C    | uint32     | Offset to a list of [Path Name](###Path-Name) for **Scripts**.
| 0x20    | uint32     | Number of Mission labels.
| 0x24    | uint32     | Offset to the [Mission Name](###Mission-Name) definitions.
| 0x28    | uint32     | Number of Triggers.
| 0x2C    | uint32     | Offset to the [Trigger Data](###Trigger-Data) definitions.
| 0x30    | uint32     | Number of Group data.
| 0x34    | uint32     | Offset to the [Group Data](###Group-Data) definitions.
| 0x38    | uint32[2]  | Padding.

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
| 0x18   | uint32  | Trigger ID. (uiID)
| 0x1C   | uint32  | [Trigger Behavior](###Trigger-Behavior)
| 0x20   | uint16  | Parameter 1 (Room to Teleport to)
| 0x22   | uint16  | Parameter 2 (Room Entrance to use)
| 0x24   | uint32  | ID of CTD file to load
| 0x28   | uint32  | Possibly a reference to [Trigger Type](###Trigger-Type) (Game Trigger)
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
| 0x10   | uint32 | ID of the Trigger associated
| 0x14   | uint32 | [Group Flag](###Group-Flag)
| 0x18   | float | Appear Parameter
| 0x1C   | uint32 | Offset to Group Data (?)
| 0x20   | float | Dead Rate
| 0x24   | uint16 | Game Trigger
| 0x26   | uint8 | Mission Parameter
| 0x27   | uint8 | Unknown Parameter
| 0x28   | uint32 | Number of Layout Object Data entities.
| 0x2C   | uint32 | Offset to the [Layout Data](###Layout-Data).

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
| 21    | 1  | Game Trigger Fire
| 22    | 1  | Mission Fire
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
| 0x0    | uint32 | Object Name
| 0x4    | Vector3 | Position
| 0x10   | Vector3 | Rotation
| 0x1C   | float | Height
| 0x20   | uint32 | [Layout Info](###Layout-Info)
| 0x24   | uint32 | Unique ID
| 0x28   | uint16 | Parameter 1 (Reward ID for [Sticker](itc.md) & [Chest](itb.md))
| 0x2A   | uint16 | Parameter 2 (Controls effect spawns)
| 0x2C   | uint16 | Parameter 3
| 0x2E   | uint16 | Trigger
| 0x30   | float | Parameter 5
| 0x34   | float | Parameter 6
| 0x38   | float | Parameter 7
| 0x3C   | float | Parameter 8
| 0x40   | int32 | Message ID
| 0x44   | uint32 | Path Name
| 0x48   | uint32 | Script Name
| 0x4C   | uint32 | Mission Label

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
