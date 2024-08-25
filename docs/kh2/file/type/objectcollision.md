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

| Id | Description
|----|-------------
| 0  | Background Collision
| 1  | Object collision
| 2  | Battle collision
| 3  | Target Collision
| 4  | Map collision
| 5  | Reaction command where the argument points to the [Command Table](../../dictionary/commands.md)
| 6  | Attack Collision
| 7  | Camera Collision
| 8  | Cast Item Collision
| 9  | Item Collision
| 10 | IK Collision
| 11 | IK Down Collision
| 12 | Neck Collision
| 13 | Guard Collision
| 14 | Ref RC Collision
| 15 | Weapon Top Collision
| 16 | Stun Collision
| 17 | Head Collision
| 18 | Blind Collision
| 19 | Talk Camera Collision
| 20 | RTN NPC Neck Collision

### Collision shape

| Id | Description
|----|-------------
| 0  | Ellipsoid
| 1  | Column
| 2  | Cube
| 3  | Sphere
