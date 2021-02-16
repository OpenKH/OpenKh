# [Kingdom Hearts II](../../index.md) - Object Collision

Define collisions for objects. They can be found in [MDLX](mdlx.md) for every object that can interact with the map or other objects.

## Format

### Header

| Offset | Type   | Description
|--------|--------|------------
| 0      | uint32 | Collision entry count
| 4      | uint32 | Enable collisions; always 1
| 8      | byte[56] | padding
| 64     | CollisionEntry[0..n] | [Collision entries](#collision-entry)

### Collision entry

| Offset | Type   | Descriptio
|--------|--------|------------
| 0      | byte   | Group
| 1      | byte   | Parts
| 2      | short  | Argument
| 4      | byte   | [Type](#collision-type)
| 5      | byte   | [Shape](#collision-shape)
| 6      | short  | Bone index of the model; 16384 means none
| 8      | short  | Position X relative to the bone
| 10     | short  | Position Y relative to the bone
| 12     | short  | Position Z relative to the bone
| 14     | short  | Height relative to the bone
| 16     | short  | Radius of collision shape
| 18     | short  | Height of collision shape with

### Collision type

Unfortunately many of them are still unknown.

| Id | Description
|----|-------------
| 0  |
| 1  | Object collision
| 2  | Battle collision
| 3  |
| 4  | Map collision
| 5  | Reaction command where the argument points to the [Command Table](../../dictionary/commands.md)
| 6  |
| 7  |
| 8  |
| 9  |
| 10 |
| 11 |
| 12 |
| 13 |
| 14 |
| 15 |
| 16 |
| 17 |

### Collision shape

| Id | Description
|----|-------------
| 0  | Ellipsoid
| 1  | Column
| 2  | unused
| 3  | Ellipsoid / Sphere?
